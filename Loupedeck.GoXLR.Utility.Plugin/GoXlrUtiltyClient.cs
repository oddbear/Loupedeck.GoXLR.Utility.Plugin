using Newtonsoft.Json;
using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace Loupedeck.GoXLR.Utility.Plugin
{
	public class GoXlrUtiltyClient : IDisposable
	{
		/// <summary>Event handler for patches.</summary>
		public event EventHandler<Patch> PatchEvent;
        
        /// <summary>Serial numbers of devices.</summary>
        public string[] Devices;

		private readonly WebSocket _client;

		public GoXlrUtiltyClient()
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
				if (message.IsText)
				{
					var jObject = JsonConvert.DeserializeObject<Response>(message.Data);
					if (jObject?.Data?.Status != null)
					{
                        var mixers = jObject?.Data?.Status["mixers"];
                        if (mixers is JObject devices)
                        {
                            Devices = devices
                                .Properties()
                                .Select(property => property.Name)
                                .ToArray();
                        }

                        TraverseObject(jObject.Data.Status);
					}

					if (jObject?.Data?.Patch != null)
					{
						foreach (var patch in jObject.Data.Patch)
						{
							PatchEvent?.Invoke(this, patch);
						}
					}
				}
			}
			catch
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

	public class Response
	{
		[JsonProperty("id")]
		public uint Id { get; set; }

		[JsonProperty("data")]
		public DataPayload Data { get; set; }
	}

	public class DataPayload
	{
		[JsonProperty("Error")]
		public string Error { get; set; }

		//[JsonProperty("HttpState")]
		//public HttpSettings.HttpSettings HttpState { get; set; }

		[JsonProperty("Patch")]
		public Patch[] Patch { get; set; }

		[JsonProperty("Status")]
		public JObject Status { get; set; }
	}

	public class Patch
	{
		[JsonProperty("op")]
		//[JsonConverter(typeof(JsonStringEnumConverter))]
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
