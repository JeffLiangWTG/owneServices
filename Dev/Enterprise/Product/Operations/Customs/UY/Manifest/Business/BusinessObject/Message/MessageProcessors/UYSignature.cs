using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public static class UYSignature
	{
		public static ZString Sign(ZString eM_BodyText, X509Certificate2 certificate)
		{
			var keyinfo = new KeyInfo();
			keyinfo.AddClause(new KeyInfoX509Data(certificate));

			var doc = new XmlDocument();
			doc.LoadXml(eM_BodyText);

			var signedXml = new SignedXml(doc)
			{
				SigningKey = certificate.GetRSAPrivateKey()
			};

			var reference = new Reference();
			reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
			reference.Uri = "";
			signedXml.AddReference(reference);

			signedXml.KeyInfo = keyinfo;
			signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;
			signedXml.ComputeSignature();

			doc.DocumentElement.AppendChild(signedXml.GetXml());

			return doc.InnerXml;
		}
	}
}
