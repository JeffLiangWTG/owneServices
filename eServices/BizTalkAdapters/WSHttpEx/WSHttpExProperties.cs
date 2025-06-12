using System;
using System.Threading;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Common;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.BizTalkAdapters.WSHttpEx
{
	public enum InboundBodyLocations
	{
		EntireEnvelope = 0,
		BodyContents = 1,
		BodyPath = 2
	}

	public class WSHttpExProperties : LoggingProperties
	{
		string inboundBodyPathExpression;
		static readonly PropertyBase isSolicitResponseProp = new BTS.IsSolicitResponse();

		public WSHttpExProperties(IBaseMessage message, string propertyNamespace)
		{
			var config = (string)message.Context.Read("AdapterConfig", propertyNamespace);
			if (null != config)
			{
				var locationConfigDom = new XmlDocument();
				locationConfigDom.LoadXml(config);

				var portName = (string)message.Context.Read("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
				ReadLocationConfiguration(locationConfigDom, portName);
			}

			IsTwoWay = (bool)message.Context.Read(isSolicitResponseProp.Name.Name, isSolicitResponseProp.Name.Namespace);
			CertificatePassPhrase = (string)message.Context.Read("CertificatePassPhrase", propertyNamespace);
			var certificateString = (string)message.Context.Read("Certificate", propertyNamespace);
			if (!string.IsNullOrEmpty(certificateString))
			{
				Certificate = Convert.FromBase64String(certificateString);
			}
			SecurityMode = (string)message.Context.Read("SecurityMode", "http://schemas.microsoft.com/BizTalk/2006/01/Adapters/WCF-properties");
			UserName = (string)message.Context.Read("UserName", "http://schemas.microsoft.com/BizTalk/2006/01/Adapters/WCF-properties");
			Password = (string)message.Context.Read("Password", "http://schemas.microsoft.com/BizTalk/2006/01/Adapters/WCF-properties");
		}

		public string Uri
		{
			get;
			private set;
		}

		public bool IsTwoWay
		{
			get;
			private set;
		}

		public string ContentType
		{
			get;
			private set;
		}

		public string CertificatePassPhrase
		{
			get;
			private set;
		}

		public byte[] Certificate
		{
			get;
			private set;
		}

		public int Timeout
		{
			get;
			private set;
		}

		public string SoapAction
		{
			get;
			private set;
		}

		public bool IgnoreServerCertErrors
		{
			get;
			private set;
		}

		public InboundBodyLocations InboundBodyLocation
		{
			get;
			private set;
		}

		public string InboundBodyPathExpression
		{
			get
			{
				switch (InboundBodyLocation)
				{
					case InboundBodyLocations.EntireEnvelope:
						return string.Empty;

					case InboundBodyLocations.BodyPath:
						return inboundBodyPathExpression;

					case InboundBodyLocations.BodyContents:
						return "/*";

					default:
						throw new NotImplementedException(string.Format("InboundBodyLocation {0} has not been implemented", InboundBodyLocation));
				}
			}
		}

		public string ResponseMessageEncoding
		{
			get;
			private set;
		}

		public string SecurityMode
		{
			get;
			private set;
		}

		public string UserName
		{
			get;
			private set;
		}

		public string Password
		{
			get;
			private set;
		}

		public override void ReadLocationConfiguration(XmlDocument configDOM, string portName, CancellationToken cancelToken = default)
		{
			base.ReadLocationConfiguration(configDOM, portName);

			Uri = IfExistsExtract(configDOM, "Config/uri", null);
			Timeout = IfExistsExtractInt(configDOM, "/Config/timeout", 120000);
			ContentType = IfExistsExtract(configDOM, "/Config/contentType", "text/xml");
			SoapAction = IfExistsExtract(configDOM, "/Config/soapAction", null);
			IgnoreServerCertErrors = IfExistsExtractBool(configDOM, "Config/ignoreServerCertErrors", false);
			InboundBodyLocation = (InboundBodyLocations)IfExistsExtractInt(configDOM, "/Config/inboundBodyLocation", (int)InboundBodyLocations.BodyContents);
			inboundBodyPathExpression = IfExistsExtract(configDOM, "/Config/inboundBodyPathExpression", string.Empty);
			ResponseMessageEncoding = IfExistsExtract(configDOM, "Config/responseMessageEncoding", string.Empty);
		}
	}
}
