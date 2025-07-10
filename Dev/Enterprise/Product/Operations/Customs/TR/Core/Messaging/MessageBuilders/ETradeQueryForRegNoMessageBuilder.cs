using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Customs.TR.MessageDefinitions.Soapenv;
using CargoWise.Customs.TR.MessageDefinitions.TemETrade;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Messaging
{
	public class ETradeQueryForRegNoMessageBuilder
	{
		public ETradeQueryForRegNoMessageBuilder()
		{
		}

		public ZString GetMessageText(IETradeQueryForRegNo provider)
		{
			var settings = new XmlWriterSettings
			{
				Indent = true,
				Encoding = Encoding.UTF8,
				OmitXmlDeclaration = true
			};

			var envelope = new Envelope()
			{
				Header = new EnvelopeHeader(),
				Body = new EnvelopeBody()
				{
					GeciciTescildenTescilNoSorgula = new GeciciTescildenTescilNoSorgula()
					{
						GeciciTescilNo = provider.TemporaryRegistrationNo,
						KullaniciAdi = provider.UserName,
						KullaniciSifre = provider.UserPassword
					}
				}
			};

			var namespaces = new XmlSerializerNamespaces();
			namespaces.Add((NoResString)"soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
			namespaces.Add((NoResString)"tem", "http://tempuri.org/");
			return TRMessageHelper.SerializeSOAPMessage(envelope, settings, namespaces);
		}
	}
}
