using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace Loupedeck.GoXLR.Utility.Plugin
{
	public class GoXlrUtilityClient : IDisposable
	{
		/// <summary>Event handler for patches.</summary>
		public event EventHandler<Patch> PatchEvent;
        
        /// <summary>Serial numbers of devices.</summary>
        public string[] Devices;

		private readonly WebSocket _client;

		public GoXlrUtilityClient()
		{
			_client = new WebSocket("ws://127.0.0.1:14564/api/websocket");
			_client.OnOpen += ClientOnOpen;
			_client.OnClose += ClientOnClose;
			_client.OnMessage += ClientOnMessage;
		}

		private int _commandIndex = 0;

		public void Start()
		{
			_client.Connect();
			_client.Send($"{{\"id\":{_commandIndex++},\"data\":\"GetStatus\"}}");
		}

        public void SendCommand(string commandName, params object[] parameters)
        {
            var commandParameters = parameters.Length == 1
                ? parameters[0]
                : parameters;

			SendCommand(new Dictionary<string, object>
            {
                [commandName] = commandParameters
            });
		}

		//TODO: Make private, all commands is simpler to call using method above:
        public void SendCommand(object command, string serial = null)
        {
			if (serial is null)
                serial = Devices.FirstOrDefault();

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

            var json = JsonConvert.SerializeObject(finalRequest);
            _client.Send(json);
		}

		private void ClientOnOpen(object sender, EventArgs eventArgs)
		{
			//eventArgs.Dump();
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

	public enum OpPatchEnum
	{
		Add,
		Replace,
		Remove
	}
}
