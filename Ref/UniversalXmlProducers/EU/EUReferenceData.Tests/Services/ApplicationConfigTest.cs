using CargoWise.RefDbRepo.EUReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class ApplicationConfigTest
	{
		[Test]
		public void OutputDirectory()
		{
			Assert.That(ApplicationConfig.Instance.OutputDirectory, Is.EqualTo(@"..\..\UXmlFiles"));
		}

		[Test]
		public void DownloadPageUrl()
		{
			Assert.That(ApplicationConfig.Instance.DownloadPageUrl, Is.EqualTo(@"https://ec.europa.eu/taxation_customs/dds2/col/download_data_generic.jsp?Lang=en"));
		}

		[Test]
		public void CUSNumberEntryPointUrl()
		{
			Assert.That(ApplicationConfig.Instance.CUSNumberEntryPointUrl, Is.EqualTo(@"https://ec.europa.eu/taxation_customs/dds2/ecics/chemicalsubstance_consultation.jsp?Lang=en"));
		}

		[Test]
		public void CUSNumberListUrlPattern()
		{
			Assert.That(ApplicationConfig.Instance.CUSNumberListUrlPattern, Is.EqualTo(@"https://ec.europa.eu/taxation_customs/dds2/ecics/chemicalsubstance_list.jsp?Lang=en&offset={0}&LangNm=en&sortOrder=1"));
		}

		[Test]
		public void CUSNumberSoapServiceUrl()
		{
			Assert.That(ApplicationConfig.Instance.CUSNumberSoapServiceUrl, Is.EqualTo(@"https://ec.europa.eu/taxation_customs/dds2/ecics/cs/services/chemical-substance"));
		}

		[Test]
		public void TestCUSNumberSoapServiceNumberOfItemsPerCall()
		{
			Assert.That(ApplicationConfig.Instance.CUSNumberSoapServiceNumberOfItemsPerCall, Is.EqualTo(10));
		}

		[Test]
		public void CUSNumberPagesPerBatch()
		{
			Assert.That(20, Is.EqualTo(ApplicationConfig.Instance.CUSNumberPagesPerBatch));
		}

		[Test]
		public void RDEntryFileLinkPattern()
		{
			Assert.That(ApplicationConfig.Instance.RDEntryFileLinkPattern, Is.EqualTo(@"<a.*COL-Generic-\d+\.zip.*>[\s\S]*?</a>"));
		}

		[Test]
		public void CustomsMeursingDownloadDir()
		{
			Assert.That(ApplicationConfig.Instance.CustomsMeursingDownloadDir, Is.EqualTo(@"..\..\UxmlFiles\Downloads"));
		}

		[Test]
		public void CircabcServiceUrlTemplate()
		{
			Assert.That(ApplicationConfig.Instance.CircabcServiceUrlTemplate, Is.EqualTo(@"https://circabc.europa.eu/service/circabc/spaces/{0}/children?language=de&guest=true&limit=20&page=1&order=modified_DESC&folderOnly=false&fileOnly=false"));
		}

		[Test]
		public void CirabcDutiesAndRelatedRootId()
		{
			Assert.That(ApplicationConfig.Instance.CirabcDutiesAndRelatedRootId, Is.EqualTo(@"ac2dee97-426f-4d5f-a63a-7d8760f29513"));
		}

		[Test]
		public void CircabcDownloadUrlTemplate()
		{
			Assert.That(ApplicationConfig.Instance.CircabcDownloadUrlTemplate, Is.EqualTo(@"https://circabc.europa.eu/rest/download/{0}"));
		}

		[Test]
		public void KindOfPackagesDownloadUrl()
		{
			Assert.That(ApplicationConfig.Instance.KindOfPackagesDownloadUrl, Is.EqualTo("https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_ICS2_KindOfPackages.zip"));
		}

		[Test]
		public void NctsCodeListDownloadUrl()
		{
			Assert.That(ApplicationConfig.Instance.NctsCodeListDownloadUrl, Is.EqualTo("https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_{Domain}_{CodeListType}.zip"));
		}

		[Test]
		public void AESNationalityDownloadUrl()
		{
			Assert.That(ApplicationConfig.Instance.AESNationalityUrl, Is.EqualTo("https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_AES_Nationality.zip"));
		}

		[Test]
		public void AESAdditionalInformationUrl()
		{
			Assert.That(ApplicationConfig.Instance.AESAdditionalInformationUrl, Is.EqualTo("https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_AES_AdditionalInformation.zip"));
		}

		[Test]
		public void AESAdditionalReferenceUrl()
		{
			Assert.That(ApplicationConfig.Instance.AESAdditionalReferenceUrl, Is.EqualTo("https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_AES_AdditionalReference.zip"));
		}

		[Test]
		public void AESTransportDocumentTypeUrl()
		{
			Assert.That(ApplicationConfig.Instance.AESTransportDocumentTypeUrl, Is.EqualTo("https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_AES_TransportDocumentType.zip"));
		}

		[Test]
		public void AESPreviousDocumentTypeUrl()
		{
			Assert.That(ApplicationConfig.Instance.AESPreviousDocumentTypeUrl, Is.EqualTo("https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_AES_PreviousDocumentType.zip"));
		}

		[Test]
		public void AuthorisationTypeUrl()
		{
			Assert.That(ApplicationConfig.Instance.AuthorisationTypeUrl, Is.EqualTo("https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_CCI_AuthorisationType.zip"));
		}

		[Test]
		public void CCIPreviousDocumentTypeUrl()
		{
			Assert.That(ApplicationConfig.Instance.CCIPreviousDocumentTypeUrl, Is.EqualTo("https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_CCI_PreviousDocumentType.zip"));
		}

		[Test]
		public void CCIMethodOfPaymentUrl()
		{
			Assert.That(ApplicationConfig.Instance.CCIMethodOfPaymentUrl, Is.EqualTo("https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_CCI_MethodOfPayment.zip"));
		}

		[Test]
		public void RefCusCodeListAdditionalTranslationPath()
		{
			Assert.That(ApplicationConfig.Instance.RefCusCodeListAdditionalTranslationsPath, Is.EqualTo(".\\AdditionalTranslations"));
		}
	}
}
