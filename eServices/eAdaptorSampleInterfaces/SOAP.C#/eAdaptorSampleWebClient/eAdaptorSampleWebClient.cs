using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Xml;

namespace CargoWise.eAdaptorSampleWebClient
{
	public class eAdaptorSampleWebClient
	{
		public static bool Ping(string serviceAddress, string senderId, string password)
		{
			using (var adapter = new eHubAdapter(GetConfiguration(serviceAddress), senderId, password))
			{
				return true;
			}
		}

		public static void SendMessage(string serviceAddress, string messageFilePath, string recipientId, string senderId, string password)
		{
			string messageNamespace = GetMessageNamespace(messageFilePath);

			using (var fileStream = GetFileAsStream(messageFilePath))
			{
				using (var adapter = new eHubAdapter(GetConfiguration(serviceAddress), senderId, password))
				{
					adapter.Outbox.AddMessage(
						new eHubMessage(
							Guid.NewGuid(),
							senderId,
							recipientId,
							MessageSchemaType.Xml,
							GetApplicationCode(messageNamespace),
							GetSchemaName(messageNamespace),
							fileStream
							));

					adapter.SendMessages();
				}
			}
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
			while (reader.Read()) if (reader.NodeType == XmlNodeType.Element) break;
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

				default: return "";
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

				default:
					return messageNamespace;
			}
		}

		static Stream GetFileAsStream(string fileName)
		{
			return new FileStream(fileName, FileMode.Open);
		}

		static IServiceConfiguration GetConfiguration(string serviceAddress)
		{
			if (serviceAddress.StartsWith("https")) return new eAdaptorHttpsConfiguration(serviceAddress);
			else return new eAdaptorHttpConfiguration(serviceAddress);
		}

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
					binding.Security.Transport.Realm = "";
					binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
					binding.Security.Message.AlgorithmSuite = SecurityAlgorithmSuite.Default;

					return binding;
				}
			}
		}

		class eAdaptorHttpsConfiguration : ServiceConfiguration
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
}
