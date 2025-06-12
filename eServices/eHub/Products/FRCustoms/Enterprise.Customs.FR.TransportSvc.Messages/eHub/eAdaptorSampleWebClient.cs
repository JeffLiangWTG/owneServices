using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Xml;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;

namespace Enterprise.Customs.FR.TransportSvc.Messages
{
	public static class eAdaptorSampleWebClient
	{
		public static bool Ping(string serviceAddress, string senderId, string password)
		{
			using (var adapter = new eHubAdapter(GetConfiguration(serviceAddress), senderId, password))
			{
				return true;
			}
		}

		public static string ReceiveErrorMessage { get; set; }

		public static string SendErrorMessage { get; set; }

		public static List<IeHubMessage> ReceiveMessage(string serviceAddress, string senderId, string password)
		{
			ReceiveErrorMessage = string.Empty;

			var result = new List<IeHubMessage>();

			try
			{
				using (var adapter = new eHubAdapter(GetConfiguration(serviceAddress), senderId, password))
				{
					adapter.RetrieveMessages();
					result.AddRange(adapter.Inbox);
					adapter.Inbox.MarkAsRead();
				}
			}
			catch (eHubAdapterException ex)
			{
				ReceiveErrorMessage = ex.GetType().ToString() + "\r\n"
					+ "*********************************\r\n"
					+ ex.Message + "\r\n"
					+ "*********************************\r\n"
					+ ex.StackTrace;
			}
			return result;
		}

		//Below function is used to forward Universal events to ehub. Exception are managed here.
		public static void SendMessage(string serviceAddress, string messageFilePath, string recipientId, string senderId, string password)
		{
			SendErrorMessage = string.Empty;

			if (string.IsNullOrEmpty(messageFilePath))
			{
				throw new System.IO.FileNotFoundException(messageFilePath);
			}

			try
			{
				var messageNamespace = GetMessageNamespace(messageFilePath);

				using (var fileStream = GetFileAsStream(messageFilePath))
				{
					using (var adapter = new eHubAdapter(GetConfiguration(serviceAddress), senderId, password))
					{
						using (var message = new eHubMessage(
								Guid.NewGuid(),
								senderId,
								recipientId,
								MessageSchemaType.Xml,
								GetApplicationCode(messageNamespace),
								GetSchemaName(messageNamespace),
								fileStream))
								{
									adapter.Outbox.AddMessage(message);
									adapter.SendMessages();
								}
					}
				}
			}
			catch (Exception ex)
			{
				SendErrorMessage = ex.GetType().ToString() + "\r\n"
					+ "*********************************\r\n"
					+ ex.Message + "\r\n"
					+ "*********************************\r\n"
					+ ex.StackTrace;
			}
		}

		//Below function is used to forward Delta Responses to ehub. Exception is managed in ProcessResponses().
		public static string SendMessage(string serviceAddress, MemoryStream memorieStream, string recipientId, string senderId, string password, string messageNamespace)
		{
			SendErrorMessage = string.Empty;

			using (var adapter = new eHubAdapter(GetConfiguration(serviceAddress), senderId, password))
			{
				var newGuid = Guid.NewGuid();
				var message = new eHubMessage(
						newGuid,
						senderId,
						recipientId,
						MessageSchemaType.Xml,
						GetApplicationCode(messageNamespace),
						GetSchemaName(messageNamespace),
						memorieStream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();
				message.Dispose();
				return newGuid.ToString();
			}
		}

		public static bool MessageTransformAs(IeHubMessage message, string filePath)
		{
			var isSavedFile = false;

			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}

			return isSavedFile;
		}

		static string GetMessageNamespace(string messageFilePath)
		{
			using (var fileStream = GetFileAsStream(messageFilePath))
			{
				using (var reader = XmlReader.Create(fileStream))
				{
					ReadToNextElement(reader);
					return reader.NamespaceURI;
				}
			}
		}

		static void ReadToNextElement(XmlReader reader)
		{
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					break;
				}
			}
		}

		static string GetApplicationCode(string messageNamespace)
		{
			switch (messageNamespace)
			{
				case "http://www.cargowise.com/Schemas/Universal":
				case "http://www.cargowise.com/Schemas/Universal/2011/11":
					return "UDM";

				case "http://www.cargowise.com/Schemas/Native":
					return "NDM";

				case "http://www.edi.com.au/EnterpriseService/":
					return "XMS";

				case "EAD":
					return "GMD";

				default:
					return string.Empty;
			}
		}

		static string GetSchemaName(string messageNamespace)
		{
			switch (messageNamespace)
			{
				case "http://www.cargowise.com/Schemas/Native":
				case "http://www.cargowise.com/Schemas/Universal":
				case "http://www.cargowise.com/Schemas/Universal/2011/11":
					return messageNamespace + "#UniversalInterchange";

				case "http://www.edi.com.au/EnterpriseService/":

					return messageNamespace + "#XmlInterchange";

				case "EAD":
					return "FRC";

				default:
					return messageNamespace;
			}
		}

		static Stream GetFileAsStream(string fileName)
		{
			return new FileStream(fileName, FileMode.Open);
		}

		public static IServiceConfiguration GetConfiguration(string serviceAddress)
		{
			if (!string.IsNullOrEmpty(serviceAddress) && serviceAddress.StartsWith("https", StringComparison.InvariantCulture))
			{
				return new eAdaptorHttpsConfiguration(serviceAddress);
			}
			else
			{
				return new eAdaptorHttpConfiguration(serviceAddress);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Class names should begin with uppercase")]
		class eAdaptorHttpConfiguration : IServiceConfiguration
		{
			public string ServiceAddress { get; private set; }
			public eAdaptorHttpConfiguration(string serviceAddress)
			{
				ServiceAddress = serviceAddress;
			}

			public System.ServiceModel.EndpointAddress EndpointAddress
			{
				get { return new EndpointAddress(new Uri(ServiceAddress)); }
			}

			public System.ServiceModel.Channels.Binding Binding
			{
				get
				{
					var binding = new BasicHttpBinding();
					binding.CloseTimeout = new TimeSpan(0, 1, 0);
					binding.OpenTimeout = new TimeSpan(0, 1, 0);
					binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
					binding.SendTimeout = new TimeSpan(0, 1, 0);
					binding.AllowCookies = false;
					binding.BypassProxyOnLocal = false;
					binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
					binding.MaxBufferSize = 65536;
					binding.MaxBufferPoolSize = 524288;
					binding.MaxReceivedMessageSize = 65536;
					binding.MessageEncoding = WSMessageEncoding.Text;
					binding.TextEncoding = Encoding.UTF8;
					binding.TransferMode = TransferMode.Buffered;
					binding.UseDefaultWebProxy = true;

					binding.ReaderQuotas.MaxDepth = 32;
					binding.ReaderQuotas.MaxStringContentLength = 8192;
					binding.ReaderQuotas.MaxArrayLength = 16384;
					binding.ReaderQuotas.MaxBytesPerRead = 4096;
					binding.ReaderQuotas.MaxNameTableCharCount = 16384;

					binding.Security.Mode = BasicHttpSecurityMode.None;
					binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
					binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
					binding.Security.Transport.Realm = string.Empty;
					binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
					binding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Default;

					return binding;
				}
			}
		}
	}

	class eAdaptorHttpsConfiguration : CargoWise.eHub.Common.ServiceConfiguration
	{
		public string ServiceAddress { get; private set; }
		public eAdaptorHttpsConfiguration(string serviceAddress)
		{
			ServiceAddress = serviceAddress;
		}

		public override System.ServiceModel.EndpointAddress EndpointAddress
		{
			get { return new EndpointAddress(new Uri(ServiceAddress)); }
		}
	}
}
