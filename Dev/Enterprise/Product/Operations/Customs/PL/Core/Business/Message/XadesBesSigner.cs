using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

#region SuppressResourceStringsCheckRegion

public static class XadesBesSigner
{
	const string DsNamespace = "http://www.w3.org/2000/09/xmldsig#";
	const string XadesNamespace = "http://uri.etsi.org/01903/v1.3.2#";
	const string CanonicalizationMethodAlgorithm = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments";

	public static ZString Sign(ZString bodyText, X509Certificate2 certificate, ZString? uniqueId = null)
	{
		Argument.NotNull(certificate.GetRSAPrivateKey(), $"RSA Private Key of {nameof(certificate)}");
		uniqueId = uniqueId ?? Guid.NewGuid().ToString("N");
		byte[] messageBytes = Encoding.UTF8.GetBytes(bodyText);
		return Sign(messageBytes, certificate, (ZString)uniqueId);
	}

	static ZString Sign(byte[] messageBytes, X509Certificate2 certificate, ZString uniqueId)
	{
		ZString timestamp = ZDateTime.UtcNow.ToString("O");
		ZString sIdSignature = $"id-{uniqueId}";
		ZString sIdRef1 = $"r-{sIdSignature}-1";
		ZString sIdSignatureValue = $"value-{sIdSignature}";
		ZString sidSignedProperties = $"xades-{sIdSignature}";

		ZString rootNamespaces = GetRootNamespaces(messageBytes);
		ZString sRef1 = GetFirstReference(sIdRef1, messageBytes);
		var sSignedProperties =
			GetSignedPropertiesForHashing(sidSignedProperties, timestamp, sIdRef1, rootNamespaces, certificate);
		ZString sRef2 = GetSecondReference(sidSignedProperties, sSignedProperties);
		var sSignedInfo = GetSignedInfo(rootNamespaces, sRef1, sRef2);

		var hashSignedInfo = HashSha256(DoC14NTransformationWithCommentAndHash(sSignedInfo));
		RSAPKCS1SignatureFormatter rsaPkcs1SignatureFormatter =
			new RSAPKCS1SignatureFormatter(certificate.GetRSAPrivateKey());
		rsaPkcs1SignatureFormatter.SetHashAlgorithm("SHA256");
		byte[] signature = rsaPkcs1SignatureFormatter.CreateSignature(hashSignedInfo);
		ZString sB64Sign = Convert.ToBase64String(signature, Base64FormattingOptions.None);
		ZString sB64Cert = Convert.ToBase64String(certificate.GetRawCertData(), Base64FormattingOptions.None);
		ZString sKeyInfo = GetKeyInfo(sB64Cert);

		var sXmlSignature = GetSignature(sIdSignature, sSignedInfo, sIdSignatureValue, sB64Sign, sKeyInfo, sSignedProperties);

		ZString sOutput = Encoding.UTF8.GetString(messageBytes);
		int iLastTag = sOutput.LastIndexOf("</", StringComparison.Ordinal);
		return sOutput.Substring(0, iLastTag) + sXmlSignature + sOutput.Substring(iLastTag);
	}

	static ZString GetSignature(ZString sIdSignature, ZString sSignedInfo, ZString sIdSignatureValue, ZString sB64Sign, ZString sKeyInfo, ZString sSignedProperties)
	{
		return $"<ds:Signature xmlns:ds=\"{DsNamespace}\" Id=\"{sIdSignature}\">"
				+ $"{sSignedInfo}"
				+ $"<ds:SignatureValue Id=\"{sIdSignatureValue}\">{sB64Sign}</ds:SignatureValue>"
				+ $"{sKeyInfo}"
				+ "<ds:Object>"
				+ $"<xades:QualifyingProperties xmlns:xades=\"{XadesNamespace}\" Target=\"#{sIdSignature}\">"
				+ $"{sSignedProperties}"
				+ "</xades:QualifyingProperties>"
				+ "</ds:Object>"
				+ "</ds:Signature>";
	}

	static ZString GetSignedInfo(ZString rootNamespaces, ZString sRef1, ZString sRef2)
	{
		return $"<ds:SignedInfo xmlns:ds=\"{DsNamespace}\" {rootNamespaces} >"
				+ $"<ds:CanonicalizationMethod Algorithm=\"{CanonicalizationMethodAlgorithm}\"/>"
				+ $"<ds:SignatureMethod Algorithm=\"{SignedXml.XmlDsigRSASHA256Url}\"/>"
				+ $"{sRef1}"
				+ $"{sRef2}"
				+ "</ds:SignedInfo>";
	}

	static ZString GetKeyInfo(ZString sB64Cert)
	{
		return "<ds:KeyInfo>"
				+ "<ds:X509Data>"
				+ $"<ds:X509Certificate>{sB64Cert}</ds:X509Certificate>"
				+ "</ds:X509Data>"
				+ "</ds:KeyInfo>";
	}

	static ZString GetFirstReference(ZString sIdRef1, byte[] messageBytes)
	{
		SHA256 sha256 = SHA256.Create();
		byte[] fileExcC14Nxml = ExccC14NTransform(messageBytes, false);
		byte[] hashFileXml = sha256.ComputeHash(fileExcC14Nxml);
		ZString sB64Hash = Convert.ToBase64String(hashFileXml, Base64FormattingOptions.None);
		return $"<ds:Reference Id=\"{sIdRef1}\" URI=\"\">"
				+ "<ds:Transforms>"
				+ $"<ds:Transform Algorithm=\"{SignedXml.XmlDsigXPathTransformUrl}\">"
				+ "<ds:XPath>not(ancestor-or-self::ds:Signature)</ds:XPath>"
				+ "</ds:Transform>"
				+ $"<ds:Transform Algorithm=\"{SignedXml.XmlDsigExcC14NTransformUrl}\"></ds:Transform>"
				+ "</ds:Transforms>"
				+ $"<ds:DigestMethod Algorithm=\"{SignedXml.XmlDsigSHA256Url}\"></ds:DigestMethod>"
				+ $"<ds:DigestValue>{sB64Hash}</ds:DigestValue>"
				+ "</ds:Reference>";
	}

	static ZString GetSecondReference(ZString sidSignedProperties, ZString sSignedProperties)
	{
		var hashSignedProperties =
			Convert.ToBase64String(HashSha256(DoC14NTransformationWithCommentAndHash(sSignedProperties)));
		return
			$"<ds:Reference Type=\"http://uri.etsi.org/01903#SignedProperties\" URI=\"#{sidSignedProperties}\">"
			+ "<ds:Transforms>"
			+ $"<ds:Transform Algorithm=\"{SignedXml.XmlDsigC14NWithCommentsTransformUrl}\"></ds:Transform>"
			+ "</ds:Transforms>"
			+ $"<ds:DigestMethod Algorithm=\"{SignedXml.XmlDsigSHA256Url}\"></ds:DigestMethod>"
			+ $"<ds:DigestValue>{hashSignedProperties}</ds:DigestValue>"
			+ "</ds:Reference>";
	}

	static ZString GetSignedPropertiesForHashing(ZString idSignedProperties, ZString timestampZString,
		ZString idReference1, ZString rootNamespaces, X509Certificate2 cert)
	{
		var (certDigest, certIssuer, certSerial) = GetCertificateInformation(cert);
		return
			$"<xades:SignedProperties xmlns:ds=\"{DsNamespace}\" xmlns:xades=\"{XadesNamespace}\" {rootNamespaces} Id=\"{idSignedProperties}\">"
			+ "<xades:SignedSignatureProperties>"
			+ $"<xades:SigningTime>{timestampZString}</xades:SigningTime>"
			+ "<xades:SigningCertificate>"
			+ "<xades:Cert>"
			+ "<xades:CertDigest>"
			+ $"<ds:DigestMethod Algorithm=\"{SignedXml.XmlDsigSHA256Url}\"/>"
			+ $"<ds:DigestValue>{certDigest}</ds:DigestValue>"
			+ "</xades:CertDigest>"
			+ "<xades:IssuerSerial>"
			+ $"<ds:X509IssuerName>{certIssuer}</ds:X509IssuerName>"
			+ $"<ds:X509SerialNumber>{certSerial}</ds:X509SerialNumber>"
			+ "</xades:IssuerSerial>"
			+ "</xades:Cert>"
			+ "</xades:SigningCertificate>"
			+ "</xades:SignedSignatureProperties>"
			+ "<xades:SignedDataObjectProperties>"
			+ $"<xades:DataObjectFormat ObjectReference=\"#{idReference1}\">"
			+ "<xades:MimeType>application/octet-stream</xades:MimeType>"
			+ "</xades:DataObjectFormat>"
			+ "</xades:SignedDataObjectProperties>"
			+ "</xades:SignedProperties>";
	}

	static (ZString, ZString, ZString) GetCertificateInformation(X509Certificate2 cert)
	{
		var sha256 = SHA256.Create();
		var certDigest =
			Convert.ToBase64String(sha256.ComputeHash(cert.GetRawCertData()), Base64FormattingOptions.None);
		var certIssuerName = cert.Issuer.Replace(", ", ",");
		var ba = cert.GetSerialNumber();
		var bb = new byte[ba.Length < 4 ? 4 : ba.Length];
		for (int i = 0; i < ba.Length; i++)
		{
			bb[i] = ba[i];
		}

		var certSerial = BitConverter.ToUInt32(bb, 0);
		return (certDigest, certIssuerName, certSerial.ToString());
	}

	static byte[] ExccC14NTransform(byte[] dataBytes, bool withComment)
	{
		MemoryStream stream = new MemoryStream(dataBytes);
		var xmlDsigC14NTransform = new XmlDsigExcC14NTransform(withComment);
		xmlDsigC14NTransform.LoadInput(stream);
		Type streamType = typeof(MemoryStream);
		MemoryStream outputStream = (MemoryStream)xmlDsigC14NTransform.GetOutput(streamType);
		return outputStream.ToArray();
	}

	static ZString GetRootNamespaces(byte[] dataBytes)
	{
		using (var ms = new MemoryStream(dataBytes))
		{
			var rootElement = XElement.Load(ms);
			return ZString.Join(" ",
				rootElement.Attributes().Where(x => x.IsNamespaceDeclaration).Select(x => new ZString(x.ToString()))
					.ToArray());
		}
	}

	static byte[] HashSha256(byte[] byteArray)
	{
		return SHA256.Create().ComputeHash(byteArray);
	}

	static byte[] DoC14NTransformationWithCommentAndHash(ZString xml)
	{
		var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
		var transform2 = new XmlDsigC14NWithCommentsTransform();
		transform2.LoadInput(stream);
		MemoryStream outputStream = (MemoryStream)transform2.GetOutput(typeof(MemoryStream));
		var ba = outputStream.ToArray();
		return ba;
	}
}

#endregion
