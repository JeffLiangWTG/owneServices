using System.Linq;
using CargoWise.eHub.Portal.Models.View.Certificate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.Model
{
	[TestClass]
	public class ContainerTypeTests
	{
		[TestMethod]
		public void TestX509_Text()
		{
			var containerType = ContainerType.GetContainerType(ContainerType.X509_Text.ID);
			Assert.AreEqual("X.509 Certificate - x509-text (*.cer,*.crt,*.der)", containerType.DisplayName);
			Assert.AreEqual("application/x-x509-ca-cert", containerType.FileContentType);
			Assert.AreEqual(FileType.Text, containerType.Type);
			Assert.AreEqual(".cer", containerType.DefaultExtension);
			Assert.AreEqual("x509-text", containerType.ID);
		}

		[TestMethod]
		public void TestX509_Binary()
		{
			var containerType = ContainerType.GetContainerType(ContainerType.X509_Binary.ID);
			Assert.AreEqual("X.509 Certificate - x509-binary (*.cer,*.crt,*.der)", containerType.DisplayName);
			Assert.AreEqual("application/x-x509-ca-cert", containerType.FileContentType);
			Assert.AreEqual(FileType.Binary, containerType.Type);
			Assert.AreEqual(".cer", containerType.DefaultExtension);
			Assert.AreEqual("x509-binary", containerType.ID);
		}

		[TestMethod]
		public void TestPEM_Text()
		{
			var containerType = ContainerType.GetContainerType(ContainerType.PEM_Text.ID);
			Assert.AreEqual("S/MIME: Privacy Enhanced Mail - pem-text (*.pem)", containerType.DisplayName);
			Assert.AreEqual("application/x-pem-file", containerType.FileContentType);
			Assert.AreEqual(FileType.Text, containerType.Type);
			Assert.AreEqual(".pem", containerType.DefaultExtension);
			Assert.AreEqual("pem-text", containerType.ID);
		}

		[TestMethod]
		public void TestPKCS12_Binary()
		{
			var containerType = ContainerType.GetContainerType(ContainerType.PKCS12_Binary.ID);
			Assert.AreEqual("Personal Information Exchange - pkcs12-binary (*.pfx,*.p12)", containerType.DisplayName);
			Assert.AreEqual("application/x-pkcs12", containerType.FileContentType);
			Assert.AreEqual(FileType.Binary, containerType.Type);
			Assert.AreEqual(".pfx", containerType.DefaultExtension);
			Assert.AreEqual("pkcs12-binary", containerType.ID);
		}

		[TestMethod]
		public void TestPKCS7_Binary()
		{
			var containerType = ContainerType.GetContainerType(ContainerType.PKCS7_Binary.ID);
			Assert.AreEqual("Cryptographic Message Syntax Standard - PKCS #7 Certficates - pkcs7-binary (*.p7b,*.spc)", containerType.DisplayName);
			Assert.AreEqual("application/x-pkcs7-certificates", containerType.FileContentType);
			Assert.AreEqual(FileType.Binary, containerType.Type);
			Assert.AreEqual(".p7b", containerType.DefaultExtension);
			Assert.AreEqual("pkcs7-binary", containerType.ID);
		}

		[TestMethod]
		public void TestPKCS7_Text()
		{
			var containerType = ContainerType.GetContainerType(ContainerType.PKCS7_Text.ID);
			Assert.AreEqual("Cryptographic Message Syntax Standard - PKCS #7 Certficates - pkcs7-text (*.p7b,*.spc)", containerType.DisplayName);
			Assert.AreEqual("application/x-pkcs7-certificates", containerType.FileContentType);
			Assert.AreEqual(FileType.Text, containerType.Type);
			Assert.AreEqual(".p7b", containerType.DefaultExtension);
			Assert.AreEqual("pkcs7-text", containerType.ID);
		}

		[TestMethod]
		public void TestOPENSSH_Text()
		{
			var containerType = ContainerType.GetContainerType(ContainerType.OPENSSH_Text.ID);
			Assert.AreEqual("OpenSSH certificates - openssh-text (*.*)", containerType.DisplayName);
			Assert.AreEqual("text/text", containerType.FileContentType);
			Assert.AreEqual(FileType.Text, containerType.Type);
			Assert.AreEqual(".ssh", containerType.DefaultExtension);
			Assert.AreEqual("openssh-text", containerType.ID);
		}

		[TestMethod]
		public void TestContainerList()
		{
			var containerTypes = ContainerType.List;
			var setGetProperties = new string[] { "DisplayName", "FileContentType", "Type", "DefaultExtension", "ID" };
			Assert.AreEqual(7, containerTypes.Count());
			Assert.AreEqual(8, typeof(ContainerType).GetProperties().Where(x => !setGetProperties.Any(y => y == x.Name)).Count());
			Assert.AreEqual(1, containerTypes.Count(x => x.ID == ContainerType.X509_Text.ID));
			Assert.AreEqual(1, containerTypes.Count(x => x.ID == ContainerType.X509_Binary.ID));
			Assert.AreEqual(1, containerTypes.Count(x => x.ID == ContainerType.PEM_Text.ID));
			Assert.AreEqual(1, containerTypes.Count(x => x.ID == ContainerType.PKCS12_Binary.ID));
			Assert.AreEqual(1, containerTypes.Count(x => x.ID == ContainerType.PKCS7_Binary.ID));
			Assert.AreEqual(1, containerTypes.Count(x => x.ID == ContainerType.PKCS7_Text.ID));
			Assert.AreEqual(1, containerTypes.Count(x => x.ID == ContainerType.OPENSSH_Text.ID));
		}
	}
}
