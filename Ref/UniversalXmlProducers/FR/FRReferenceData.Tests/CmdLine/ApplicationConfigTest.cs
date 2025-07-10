using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.CmdLine
{
	[TestFixture]
	public class ApplicationConfigTest
	{
		[Test]
		public void TestApplicationSettings()
		{
			ApplicationConfig.ReLoad();
			Assert.That("https://www.douane.gouv.fr/sites/default/files/drop/record.zip", Is.EqualTo(ApplicationConfig.Instance.DeltaGBaseUrl));
			Assert.That("https://moa.douane.gouv.fr/sites/default/files/drop/record.zip", Is.EqualTo(ApplicationConfig.Instance.UCC6BaseUrl));
			Assert.That(@"..\..\UxmlFiles\Downloads", Is.EqualTo(ApplicationConfig.Instance.DownloadDirectory));
			Assert.That(@"..\..\UxmlFiles\Drop", Is.EqualTo(ApplicationConfig.Instance.DropDirectory));
			Assert.That("drop_Fr.zip", Is.EqualTo(ApplicationConfig.Instance.DownloadFileName));

			Assert.That("AEROPORT.XML", Is.EqualTo(ApplicationConfig.Instance.AirportFileName));
			Assert.That("CANA_CACO_RESTIT.XML", Is.EqualTo(ApplicationConfig.Instance.AdditionalCodesFileName));
			Assert.That("PAYS.XML", Is.EqualTo(ApplicationConfig.Instance.CustomsDestinationCodesFileName));
			Assert.That("RECORD_NIS_CONDITIONS_ECO_RP.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEInwardProcessingConditionsListForAdhocAuthorisationsCodesFileName));
			Assert.That("RECORD_NIS_IDENTIFICATION_MARCHANDISES_RP.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIELegalConditionsListForAdhocAuthorisationsCodesFileName));
			Assert.That("RECORD_NIS_MOTIFS_INVALIDATION.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEInvalidationMotivationReferenceCodesFileName));
			Assert.That("RECORD_NES_MOTIFS_RECTIFICATION.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEMotivationForRectificationRequestCodesFileName));
			Assert.That("CSRD2_CCI_CL239.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEImportAdditionalInformationCodesFileName));
			Assert.That("CSRD2_CUST_FR_CCI_CLFR239.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEImportAdditionalInformationCodesFRSpecifiedFileName));
			Assert.That("CSRD2_CCI_CL380.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEImportAdditionalReferenceCodesFileName));
			Assert.That("CSRD2_CUST_FR_CCI_CLFR380.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEImportAdditionalReferenceCodesFRSpecifiedFileName));
			Assert.That("CSRD2_CCI_CL716.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEImportControlResultsTypeCodesFileName));
			Assert.That("CSRD2_CCI_CL754.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEImportTransportDocumentTypeCodesFileName));
			Assert.That("CSRD2_CCI_CL790.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEImportControlTypeTypeCodesFileName));
			Assert.That("CSRD2_CCI_CL047.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEImportTypeOfDiscrepanciesTypeCodesFileName));
			Assert.That("CSRD2_CCI_CL740.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEImportRiskAreaCodeTypeCodesFileName));
			Assert.That("CSRD2_CUST_FR_CCI_CLFR214.xml", Is.EqualTo(ApplicationConfig.Instance.DeltaIEImportPreviousDocumentTypeCodesFileName));
			Assert.That("CSRD2_CCI_CL042.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEImportEntrySubStyleCodesFileName));
			Assert.That("CSRD2_CCI_CL213.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEImportSupportingDocumentTypeCodesFileName));
			Assert.That("RITA_CCI_CLFR213.XML", Is.EqualTo(ApplicationConfig.Instance.DeltaIEImportSupportingDocumentTypeCodesFRSpecifiedFileName));
			Assert.That("NATURE_TRANSACT_TOTAL.xml", Is.EqualTo(ApplicationConfig.Instance.DeltaIETransactionNatureCodesFileName));
			Assert.That("CODE_TAXE_NOUV.XML", Is.EqualTo(ApplicationConfig.Instance.RateTypeFileName));
			Assert.That("COURS_DEVISES.XML", Is.EqualTo(ApplicationConfig.Instance.CurrencyPricesFileName));
			Assert.That("DEVISES.XML", Is.EqualTo(ApplicationConfig.Instance.CurrenciesFileName));
			Assert.That("MENTION_SPECIALE.XML", Is.EqualTo(ApplicationConfig.Instance.SpecialMentionFileName));
			Assert.That("NATURE_DOC_PEC.XML", Is.EqualTo(ApplicationConfig.Instance.DocumentNatureFileName));
			Assert.That("REGIME_CODE_COMM.XML", Is.EqualTo(ApplicationConfig.Instance.ConcessionFileName));
			Assert.That("REGIME_DOUANIER.XML", Is.EqualTo(ApplicationConfig.Instance.CustomsProcedureFileName));
			Assert.That("REGIME_PRECEDENT.XML", Is.EqualTo(ApplicationConfig.Instance.PreviousProcedureFileName));
			Assert.That("REGIME_SOLLICITE.XML", Is.EqualTo(ApplicationConfig.Instance.ProcedureFileName));
			Assert.That("TYPE_DOCUMENT_JOINT.XML", Is.EqualTo(ApplicationConfig.Instance.DocumentTypeFileName));
			Assert.That("UNITE_MESURAGE.XML", Is.EqualTo(ApplicationConfig.Instance.UnitOfMeasureFileName));
			Assert.That("CANAAI2.XML", Is.EqualTo(ApplicationConfig.Instance.VATAdditionalCodesFileName));
			Assert.That("ZONE.XML", Is.EqualTo(ApplicationConfig.Instance.ZoneFileName));
			Assert.That("PNTS_CodeLists.xlsx", Is.EqualTo(ApplicationConfig.Instance.PntsCodeListsFileName));

			Assert.That(@"FRAdditionalCodes.xml", Is.EqualTo(ApplicationConfig.Instance.FRAdditionalCodesOutputFile));
			Assert.That(@"FRAirports.xml", Is.EqualTo(ApplicationConfig.Instance.FRAirportOutputFile));
			Assert.That(@"FRConditionTypes.xml", Is.EqualTo(ApplicationConfig.Instance.FRConditionTypesOutputFile));
			Assert.That(@"FRDocumentNatures.xml", Is.EqualTo(ApplicationConfig.Instance.FRDocumentNatureOutputFile));
			Assert.That(@"FRDocumentTypes.xml", Is.EqualTo(ApplicationConfig.Instance.FRDocumentTypeOutputFile));
			Assert.That(@"FRExchangeRates.xml", Is.EqualTo(ApplicationConfig.Instance.FRExchangeOutputFile));
			Assert.That(@"FRNationalRateCodeUsage.xml", Is.EqualTo(ApplicationConfig.Instance.FRNationalRateCodeUsageOutputFile));
			Assert.That(@"FRRateTypes.xml", Is.EqualTo(ApplicationConfig.Instance.FRRateTypeOutputFile));
			Assert.That(@"FRSpecialMentions.xml", Is.EqualTo(ApplicationConfig.Instance.FRSpecialMentionOutputFile));
			Assert.That(@"FRUnitsOfMeasure.xml", Is.EqualTo(ApplicationConfig.Instance.FRUnitOfMeasureOutputFile));
			Assert.That(@"FRUnitsOfMeasureTranslation.xml", Is.EqualTo(ApplicationConfig.Instance.FRUnitOfMeasureTranslationOutputFile));
			Assert.That(@"FRCustomsProcedures.xml", Is.EqualTo(ApplicationConfig.Instance.FRCustomsProcedureOutputFile));
			Assert.That(@"FRDeltaIECustomsProcedures.xml", Is.EqualTo(ApplicationConfig.Instance.FRDeltaIECustomsProcedureOutputFile));
			Assert.That(@"FRTradeGroup.xml", Is.EqualTo(ApplicationConfig.Instance.FRTradeGroupOutputFile));
			Assert.That(@"FRVATAdditionalCodes.xml", Is.EqualTo(ApplicationConfig.Instance.FRVATAdditionalCodesOutputFile));

			Assert.That("https://www.douane.gouv.fr/rita-s/ServiceMoteurTarifaireTiers", Is.EqualTo(ApplicationConfig.Instance.RITAWebServiceURL));
			Assert.That(30, Is.EqualTo(ApplicationConfig.Instance.DownloadTimeoutInSeconds));
			Assert.That(@"..\..\UxmlFiles", Is.EqualTo(ApplicationConfig.Instance.OutputDirectory));
			Assert.That(10000, Is.EqualTo(ApplicationConfig.Instance.MinDelayBetweenRequestsToRITA));
			Assert.That(10, Is.EqualTo(ApplicationConfig.Instance.MaxAttemptsCountInCaseOfTechnicalError));
			Assert.That(900000, Is.EqualTo(ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfTechnicalError));
			Assert.That(60000, Is.EqualTo(ApplicationConfig.Instance.MinDelayBetweenAttemptsInCaseOfDownloadError));
			Assert.That(10, Is.EqualTo(ApplicationConfig.Instance.MaxAttemptsCountInCaseOfDownloadError));

			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			Assert.That(Path.Combine(binPath, @"Templates\TempMeasuresRequest.XML"), Is.EqualTo(ApplicationConfig.Instance.WebServiceMeasureRequestTemplate));
			Assert.That(Path.Combine(binPath, @"Templates\TempConditionsRequest.XML"), Is.EqualTo(ApplicationConfig.Instance.WebServiceConditionRequestTemplate));
			Assert.That(Path.Combine(binPath, @"Templates\TempNomenclatureRequest.XML"), Is.EqualTo(ApplicationConfig.Instance.WebServiceNomenclatureRequestTemplate));
			Assert.That(20m, Is.EqualTo(ApplicationConfig.Instance.StandardVatRate));
			Assert.That(13m, Is.EqualTo(ApplicationConfig.Instance.PetroleumVatRate));
			Assert.That(10m, Is.EqualTo(ApplicationConfig.Instance.HalfVatRate));
			Assert.That(8.5m, Is.EqualTo(ApplicationConfig.Instance.DOMStandardVatRate));
			Assert.That(5.5m, Is.EqualTo(ApplicationConfig.Instance.ReducedVatRate));
			Assert.That(2.1m, Is.EqualTo(ApplicationConfig.Instance.SuperReducedVatRate));
			Assert.That(1.75m, Is.EqualTo(ApplicationConfig.Instance.DOMLiveStockVatRate));
			Assert.That(1.05m, Is.EqualTo(ApplicationConfig.Instance.DOMPressVatRate));
			Assert.That(0.9m, Is.EqualTo(ApplicationConfig.Instance.CorsicaSuperReducedVatRate));
			Assert.That(0m, Is.EqualTo(ApplicationConfig.Instance.FreeVatRate));

			Assert.AreEqual(false, ApplicationConfig.Instance.IsRefDbServiceSecure);
			Assert.AreEqual("https://refdbrepoupdate.wisecloud.zone/Update/odata/", ApplicationConfig.Instance.RefDbServiceURI);

			Assert.AreEqual("bernard.navarro@wisetechglobal.com,sophie.joannan@wisetechglobal.com,jesse.zhang@wisetechglobal.com", ApplicationConfig.Instance.EmailRecipients);
			Assert.AreEqual("donotreply_refservice@wisetechglobal.com", ApplicationConfig.Instance.EmailSender);
			Assert.AreEqual("mail.test.wisecloud.zone", ApplicationConfig.Instance.EmailSmtpServer);
			Assert.AreEqual("donotreply_refservice", ApplicationConfig.Instance.EmailUsername);
			Assert.AreEqual(string.Empty, ApplicationConfig.Instance.EmailCredentialsPassword);
			Assert.AreEqual(25, ApplicationConfig.Instance.EmailSmtpPort);

			Assert.AreEqual("2044,2045,2091,9092,2413,2423,2700,2704,C401,C638,C639,C678", ApplicationConfig.Instance.PermitDocumentTypes);
			Assert.AreEqual("L100,E013", ApplicationConfig.Instance.ODSDocumentTypes);
			Assert.AreEqual("U167,U395,U397,U437", ApplicationConfig.Instance.NationalRateCodesRequiringUsageTracking);
		}

		[Test]
		public void TestReload()
		{
			ApplicationConfig.Instance.DeltaGBaseUrl = "test";
			Assert.That("test", Is.EqualTo(ApplicationConfig.Instance.DeltaGBaseUrl));

			ApplicationConfig.ReLoad();
			Assert.That("https://www.douane.gouv.fr/sites/default/files/drop/record.zip", Is.EqualTo(ApplicationConfig.Instance.DeltaGBaseUrl));
		}
	}
}
