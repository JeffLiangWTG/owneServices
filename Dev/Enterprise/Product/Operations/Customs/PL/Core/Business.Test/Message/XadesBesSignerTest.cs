using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

class XadesBesSignerTest : TestCaseWithFactory
{
	const string XmlDocumentName = "DocumentToSign1.xml";
	const string SignatureTextFileName = "ExpectedSignature.txt";

	[TestDate(2022, 1, 1)]
	public void TestSign_ValidCertificate()
	{
		if (!TryToGetTestFile(XmlDocumentName, out ZString xmlString))
		{
			Fail($"The specified embedded \"{XmlDocumentName}\" is not found.");
			return;
		}

		if (!TryToGetTestFile(SignatureTextFileName, out ZString expectedSignature))
		{
			Fail($"The specified embedded \"{SignatureTextFileName}\" is not found.");
			return;
		}

		var uniqueId = "69481592241f4a75847a0475afecc3a2";
		var validCertificate = new X509Certificate2(X509Certificate2TestHelper.ValidCertificate, X509Certificate2TestHelper.ValidPassword);
		var signedXml = XadesBesSigner.Sign(xmlString, validCertificate, uniqueId);
		AssertContains(expectedSignature.Trim(), signedXml);
	}

	public void TestSign_CertificateWithoutPrivateKey()
	{
		if (!TryToGetTestFile(XmlDocumentName, out ZString xmlString))
		{
			Fail($"The specified embedded \"{XmlDocumentName}\" is not found.");
			return;
		}

		var validCertificate = new X509Certificate2(X509Certificate2TestHelper.ValidCertificate_NoPrivateKey);
		AssertExceptionThrown<ArgumentNullException>("Value cannot be null.\r\nParameter name: RSA Private Key of certificate", () => XadesBesSigner.Sign(xmlString, validCertificate));
	}

	public void TestSign_DSACertificate()
	{
		if (!TryToGetTestFile(XmlDocumentName, out ZString xmlString))
		{
			Fail($"The specified embedded \"{XmlDocumentName}\" is not found.");
			return;
		}

		var validCertificate = new X509Certificate2(X509Certificate2TestHelper.ValidDSACertificate, X509Certificate2TestHelper.ValidDSAPassword);
		AssertExceptionThrown<ArgumentNullException>("Value cannot be null.\r\nParameter name: RSA Private Key of certificate", () => XadesBesSigner.Sign(xmlString, validCertificate));
	}

	bool TryToGetTestFile(ZString documentName, out ZString xmlString)
	{
		xmlString = ZString.Empty;
		var embeddedXmlPath = $"{TestFilesLocation}.{documentName}";
		using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedXmlPath))
		{
			if (stream == null)
			{
				return false;
			}

			var reader = new StreamReader(stream);
			xmlString = new ZString(reader.ReadToEnd());
			return true;
		}
	}

	string TestFilesLocation => "Enterprise.Customs.PL.Business.Testing.Message.TestFiles";
}
