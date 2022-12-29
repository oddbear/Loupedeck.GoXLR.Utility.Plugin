using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Loupedeck.GoXLR.Utility.Plugin.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace Loupedeck.GoXLR.Utility.Plugin
{
	public class GoXlrUtilityClient : IDisposable
    {
        private readonly Thread _thread;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private WebSocket _client;

        private int _commandIndex = 0;

        /// <summary>Serial numbers of devices.</summary>
        public string[] Devices;

        /// <summary>Event handler for patches.</summary>
        public event EventHandler<Patch> PatchEvent;
        public event EventHandler<(PluginStatus status, string message)> PluginStatusEvent;

        public GoXlrUtilityClient()
        {
            _thread = new Thread(Reconnect);
		}

        private void Reconnect()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                bool Connected() => _client?.ReadyState == WebSocketState.Open;

                try
                {
                    if (Connected())
                        continue;

                    if (_client is null)
                        _client = CreateClient();

                    _client.Connect();
                    
                    if (Connected())
                    {
                        PluginStatusEvent?.Invoke(this, (PluginStatus.Normal, "Connected"));
                    }
                    else
                    {
                        PluginStatusEvent?.Invoke(this, (PluginStatus.Warning, "Could not connect to goxlr utility, is it running on this machine?"));
                    }
                }
                catch (Exception exception)
                {
                    if (exception.Message != "A series of reconnecting has failed.")
                    {
                        Trace.WriteLine($"{exception.GetType().Name}: {exception.Message}");
                        PluginStatusEvent?.Invoke(this, (PluginStatus.Error, $"Error: {exception.Message}"));
                    }

                    IDisposable oldClient = _client;
                    _client = CreateClient();
                    oldClient?.Dispose();
                }
                finally
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
        }

        private WebSocket CreateClient()
        {
            var client = new WebSocket("ws://127.0.0.1:14564/api/websocket");
            client.OnOpen += ClientOnOpen;
            client.OnClose += ClientOnClose;
            client.OnMessage += ClientOnMessage;

            return client;
        }

        public void Start()
        {
            _thread.Start();
		}

        public void SendCommand(string commandName, params object[] parameters)
        {
			if (commandName is null)
				return;

			if (parameters is null || parameters.Length < 1)
				return;

            var commandParameters = parameters.Length == 1
                ? parameters[0]
                : parameters;

			SendCommand(new Dictionary<string, object>
            {
                [commandName] = commandParameters
            });
		}
		
        private void SendCommand(object command)
        {
            var serial = Devices?.FirstOrDefault();

            if (serial is null)
				return;

			var id = _commandIndex++;
            var finalRequest = new
            {
                id,
                data = new
                {
                    Command = new[] {
                        serial,
                        command
                    }
                }
			};
			
            var json = JsonConvert.SerializeObject(finalRequest, new StringEnumConverter());
            _client.Send(json);
		}

		private void ClientOnOpen(object sender, EventArgs eventArgs)
        {
            _client.Send($"{{\"id\":{_commandIndex++},\"data\":\"GetStatus\"}}");
        }

		private void ClientOnClose(object sender, CloseEventArgs closeEventArgs)
		{
			//closeEventArgs.Dump();
		}

		private void ClientOnMessage(object sender, MessageEventArgs message)
		{
			if (message.Data is null)
				return;

			try
            {
                if (!message.IsText)
                    return;

                var jObject = JsonConvert.DeserializeObject<JObject>(message.Data);

                var data = jObject?.Object("data");

                var status = data.Object("Status");
                if (status != null)
                {
                    var mixers = status.Object("mixers");
                    if (mixers != null)
                    {
                        Devices = mixers
                            .Properties()
                            .Select(property => property.Name)
                            .ToArray();
                    }

                    TraverseObject(status);
                }

                var patches = data.Array("Patch");
                if (patches != null)
                {
                    foreach (var patch in patches.ToObject<Patch[]>())
                    {
                        PatchEvent?.Invoke(this, patch);
                    }
                }
            }
			catch (Exception ex)
			{
				//message.Dump("Error");
			}
		}

		private void TraverseObject(JObject jObject, string path = null)
		{
			foreach (var property in jObject.Properties())
			{
				var currentPath = $"{path}/{property.Name}";

				switch (property.Value)
				{
					case JObject jObjectValue:
						TraverseObject(jObjectValue, currentPath);
						break;

					//				case JArray jArray:
					//					var array = jArray.ToObject<int[]>();
					//					break;
					//
					//				case JValue jValue:
					//					Console.WriteLine($"{currentPath}: {jValue}");
					//					break;

					default:
						PatchEvent?.Invoke(this, new Patch { Op = OpPatchEnum.Replace, Path = currentPath, Value = property.Value });
						break;
				}
			}
		}

		public void Dispose()
		{
            _cancellationTokenSource?.Cancel();
            ((IDisposable)_client)?.Dispose();
		}
	}

    static class JsonNetExtensions
	{
        public static JArray Array(this JObject jObject, string propertyName)
		    => jObject?.Property(propertyName)?.Value as JArray;

		public static JObject Object(this JObject jObject, string propertyName)
            => jObject?.Property(propertyName)?.Value as JObject;
    }
	
	public class Patch
	{
		[JsonProperty("op")]
		public OpPatchEnum Op { get; set; }

		[JsonProperty("path")]
		public string Path { get; set; }

		[JsonProperty("value")]
		public JToken Value { get; set; }
	}
}
