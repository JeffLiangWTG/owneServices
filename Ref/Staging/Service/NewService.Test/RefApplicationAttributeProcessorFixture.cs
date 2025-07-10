using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.NewService.Controllers;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.NewService.Test
{
	[TestFixture]
	class RefApplicationAttributeProcessorFixture
	{
		[TestCase(true)]
		[TestCase(false)]
		public void HideSecretFileContent(bool isSecret)
		{
			var attribute = isSecret ? attribute_SecretFile : attribute_File;
			var result = RefApplicationAttributeProcessor.GetWithoutSecretContent([attribute]).FirstOrDefault();
			Assert.That(result.RAA_Content, isSecret ? Is.Null : Is.Not.Null);
		}

		[Test]
		public void EncryptCredentialValue()
		{
			var mockCrypto = new Mock<IRefDbRepoCrypto>();
			mockCrypto.Setup(m => m.EncryptRSA(It.IsAny<string>())).Returns([1, 2, 3, 4, 5]);
			RefApplicationAttributeProcessor.EncryptCredentialValue(attribute_Credential, mockCrypto.Object);
			Assert.That(attribute_Credential.RAA_Content, Is.Not.Null);
			Assert.That(attribute_Credential.RAA_Value, Is.Empty);
		}

		[Test]
		public void EncryptSecretFileValue()
		{
			var mockCrypto = new Mock<IRefDbRepoCrypto>();
			var encryptedContent = new byte[] { 7, 6, 3, 3, 1 };
			mockCrypto.Setup(m => m.EncryptAES(It.IsAny<byte[]>())).Returns(encryptedContent);
			RefApplicationAttributeProcessor.EncryptCredentialValue(attribute_SecretFile, mockCrypto.Object);
			Assert.That(attribute_SecretFile.RAA_Value, Is.EqualTo("test.key"));
			Assert.That(attribute_SecretFile.RAA_Content, Is.EqualTo(encryptedContent));
		}

		RefApplicationAttribute attribute_File = new RefApplicationAttribute
		{
			RAA_PK = Guid.Parse("73E19BF9-3779-4F77-A213-1E7F72E69071"),
			RAA_ConfigFilePath = "ABC.CmdLine.exe.config",
			RAA_AttributeName = "InputFile",
			RAA_Value = "UxmlFiles.txt",
			RAA_RAT_NKType = "File",
			RAA_JobGroup = "ABC",
			RAA_Content = [1, 2, 3, 4, 5]
		};

		RefApplicationAttribute attribute_SecretFile = new RefApplicationAttribute
		{
			RAA_PK = Guid.Parse("73E19BF9-3779-4F77-A213-1E7F72E69071"),
			RAA_ConfigFilePath = "ABC.CmdLine.exe.config",
			RAA_AttributeName = "InputKeyFile",
			RAA_Value = "test.key",
			RAA_RAT_NKType = "SecretFile",
			RAA_JobGroup = "ABC",
			RAA_Content = [1, 2, 3, 4, 5]
		};

		RefApplicationAttribute attribute_Credential = new RefApplicationAttribute
		{
			RAA_PK = Guid.Parse("68756069-2FFF-4421-8756-F143B8793E6B"),
			RAA_ConfigFilePath = "CargoWise.RefDbRepo.BEReferenceData.CmdLine.config.json",
			RAA_AttributeName = "TestPassword",
			RAA_Value = "23123123322",
			RAA_RAT_NKType = "Credential",
			RAA_JobGroup = "BE Customs",
		};
	}
}
