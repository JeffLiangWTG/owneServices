using System;
using CargoWise.eHub.Integration;
using CargoWise.eServices.USCustoms.Integration;
using CargoWise.eServices.USCustoms.MessageHandlerABIACE;
using CargoWise.eServices.USCustoms.MessageHandlerAES;
using CargoWise.eServices.USCustoms.MessageHandlerAMA;
using CargoWise.eServices.USCustoms.MessageHandlerAMS;
using CargoWise.eServices.USCustoms.MessageHandlerUEM;
using CargoWise.eServices.USCustoms.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceBroker.Common;

namespace CargoWise.eServices.USCustoms.Tests
{
	[TestClass]
	public class ConfigurationHelperTest
	{
		protected void AssertException(Action action, Type exceptionType, string exceptionMessage)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				Assert.AreEqual(exceptionType, ex.GetType());
				Assert.AreEqual(exceptionMessage, ex.Message);
				return;
			}

			throw new AssertFailedException(string.Format("Expected exception with type {0}, but no exception occured", exceptionType.ToString()));
		}

		[TestMethod]
		public void TestGetInboundMessageHandler()
		{
			var repositoryMock = new Mock<IRegistryRepository>();
			var repository = repositoryMock.Object;

			Assert.AreEqual(typeof(ABIACEInboundMessageHandler), ConfigurationHelper.GetInboundMessageHandler(Constants.MessagaType.ABI, repository).GetType());
			Assert.AreEqual(typeof(ABIACEInboundMessageHandler), ConfigurationHelper.GetInboundMessageHandler(Constants.MessagaType.ACE, repository).GetType());
			Assert.AreEqual(typeof(ISFInboundMessageHandler), ConfigurationHelper.GetInboundMessageHandler(Constants.MessagaType.ISF, repository).GetType());
			Assert.AreEqual(typeof(AESInboundMessageHandler), ConfigurationHelper.GetInboundMessageHandler(Constants.MessagaType.AES, repository).GetType());
			Assert.AreEqual(typeof(AMAInboundMessageHandler), ConfigurationHelper.GetInboundMessageHandler(Constants.MessagaType.AMA, repository).GetType());
			Assert.AreEqual(typeof(AMSInboundMessageHandler), ConfigurationHelper.GetInboundMessageHandler(Constants.MessagaType.AMS, repository).GetType());
			Assert.AreEqual(typeof(UEMInboundMessageHandler), ConfigurationHelper.GetInboundMessageHandler(Constants.MessagaType.UEM, repository).GetType());
			AssertException(() => { ConfigurationHelper.GetInboundMessageHandler("whatever", repository); }, typeof(ApplicationException), "Message Type 'whatever' for type 'IInboundConfiguration' is not supported");

		}

		[TestMethod]
		public void TestGetOutboundMessageHandler()
		{
			var repositoryMock = new Mock<IRegistryRepository>();

			//ACE
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.ADCVDCaseInformationQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.CensusWarningOverride, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.CensusWarningQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.EntrySummary, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.EntrySummaryQueryOld, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.EntrySummaryQueryResponseOld, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.ADCVDCaseInformationQueryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.CargoReleaseTransactionsResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.CensusWarningOverrideResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.CensusWarningQueryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.EntrySummaryNotification, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.EntrySummaryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.SimplifiedEntrySubmissions, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.SimplifiedEntryAcceptReject, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.SimplrfiedEntryStatus, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.QueryACECargoManifestEntryRelease, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.QueryACECargoManifestEntryReleaseResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.QuotaQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.QuotaQueryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.StatementUpdate, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.StatementUpdateResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.StandalonePriorNotice, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.StandalonePriorNoticeResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.StandalonePriorNoticeStatusNotification, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.ExtractReference, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.FoodAndDrugAdministrationAffirmationofComplianceTypesQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.FoodAndDrugAdministrationProductCodeQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.ManufacturerNameAndAddressQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.ReconciliationEntrySummary, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.DailyStatement, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.eBondStatusNotification, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.ExtractReferenceResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.FoodAndDrugAdministrationAffirmationofComplianceTypesQueryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.FoodAndDrugAdministrationProductCodeQueryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.ManufacturerNameAndAddressQueryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.ReconciliationEntrySummaryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.QueryImporterBond, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.QueryImporterBondResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.Add5106toImporterFileProcessingResults, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.StatementRequestReroute, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.StatementRequestRerouteResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.Drawback, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.HTSQueryTransaction, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.HarmonisedTariffQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.HarmonisedTariffQueryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.TemporaryImportationExtensionBondExtensionAndClosure, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.PGADataCorrection, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.ACHDebitEntrySummaryPresentation, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.GBIReferenceCreateUpdateDelete, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.GBIReferenceCreateUpdateDeleteResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.GBIReferenceStatusUpdate, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.EntrySummaryQueryRequest, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.EntrySummaryQueryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.eCertQueryRequest, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ACEOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ACEInterchangeType.eCertQueryResponse, repositoryMock.Object).GetType());

			//ABI
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AllMessages, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.FDAPriorNotice, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ElectronicInvoice, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AntidumpingCountervailingDutyQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.OtherAgencyEntryDataUpdate, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.EntryDateUpdateTransaction, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ExtractReferenceFiles, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.FishAndWildlifeService, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.QueryLaboratoryGauger, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.CargoReleaseTransactions, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.BorderCargoRelease, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.StatementDeleteTransaction, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.QueryEntryStatus, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ConsigneeNameAddressAdd, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.QueryCurrentEntryStatus, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.QueryEntrySummary, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.DrawbackSummary, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.NAFTADutyDeferral, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ConsigneeNameAddressQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.BillofLadingUpdate, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AdministrativeMessageQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ProductCodeBuilderUpdateQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.FoodandDrugAdministrationProductCode, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.FoodandDrugAdministrationAffirmationofComplianceType, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ParticipatingGovernmentAgencies, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ProtestInitialFiling, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentation, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.FoodandDrugAdministrationEstablishmentIdentifier, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ProtestAmendment, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AutomatedClearinghouse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ABIStatementACHPaymentReroute, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.InbondTransaction, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ReconciliationEntryFiling, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ProtestAddenda, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ProtestServiceRequest, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ImporterSecurityFiling, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AddCBPFormCBPF5106DatatotheImporterFile, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.QueryQuota, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.HarmonizedTariffScheduleQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.InbondUpdateTransferofLiability, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AirInbond, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AirInbondUpdateTransferOfLiabilityPriorNotice, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.TemporaryImportationBondEntrySummaries, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.QueryErrorStatistics, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ManufacturerNameandAddressQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.BrokerManifestDownload, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ElectronicInvoiceResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AntidumpingCountervailingDutyQueryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.OtherGovernmentAgencyMessage, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.EntryDateUpdateTransactionResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.OtherAgencyEntryDataCorrection, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ExtractReferenceFilesResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.FishAndWildlifeServiceResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.QueryLaboratoryGaugerResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.CargoReleaseTransactionsResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.BorderCargoReleaseResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.StatementDeleteTransactionResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ConsigneeNameAddressAddResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.QueryEntryStatusResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.QueryCurrentEntryStatusResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ExtractADDCVDCaseFileResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.DrawbackSummaryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.QueryEntrySummaryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.NAFTADutyDeferralResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ConsigneeNameAddressQueryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.BillofLadingUpdateResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ProductCodeBuilderUpdateQueryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AdministrativeMessage, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.PeriodicMonthlyStatement, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ForeignTradeZone, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.CourtesyNoticeofLiquidation, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.InbondStatusNotification, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.FoodandDrugAdministrationProductCodeResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.UserStatistics, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.FoodandDrugAdministrationAffirmationofComplianceTypes, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ProtestInitialFilingResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.BillofLadingProcessingResults, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.FoodandDrugAdministrationEstablishmentIdentifierResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ProtestAmendmentResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ABIStatementACHPaymentRerouteResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.DailyStatement, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AutomatedClearinghouseResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.InbondTransactionResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ReconciliationEntryFilingResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ProtestAddendaResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.CargoReleaseProcessingResults, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ImporterSecurityFilingStatusAdvisory, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ProtestServiceRequestResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ImporterSecurityFilingResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ProtestAutomaticNotificationandResponsetoFilerQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.TemporaryImportationBondDuetoExpire, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.QueryQuotaResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ElectronicRejectRequestNotification, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.HarmonizedSystemUpdate, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.QueryHarmonizedSystem, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.InbondUpdateTransferofLiabilityResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.LineRelease, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.TemporaryImportationBondEntrySummariesResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AirInbondResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AirInbondUpdateTransferOfLiabilityPriorNoticeResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.FTZDownloadofDatatoZoneOperator, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.QueryErrorStatisticsResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.CurrencyUpdate, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ManufacturerNameandAddressAddResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.ManufacturerNameandAddressQueryResponse, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.FDAPriorNoticeStatusMessage, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.BIRDTransaction, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.BIRDEntrySummaryQuery, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AirInbondUpdateTransferOfLiabilityPriorNotice, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ABIInterchangeType.AirInbondUpdateTransferOfLiabilityPriorNoticeResponse, repositoryMock.Object).GetType());

			//ISF
			Assert.AreEqual(typeof(ISFOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ISFInterchangeType.ImporterSecurityFiling, false, true, true, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ISFOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ISFInterchangeType.ImporterSecurityFiling, false, true, false, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ISFOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ISFInterchangeType.ImporterSecurityFilingStatusAdvisory, false, true, false, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ISFOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ISFInterchangeType.ImporterSecurityFilingResponse, false, true, false, repositoryMock.Object).GetType());

			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ISFInterchangeType.ImporterSecurityFiling, true, false, false, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ISFOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ISFInterchangeType.ImporterSecurityFiling, true, false, true, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ISFInterchangeType.ImporterSecurityFilingStatusAdvisory, true, false, false, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(ABIOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ISFInterchangeType.ImporterSecurityFilingResponse, true, false, false, repositoryMock.Object).GetType());

			//FTZ
			Assert.AreEqual(typeof(FTZOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.FTZInterchangeType.FTZAdmissionData, repositoryMock.Object).GetType());
			Assert.AreEqual(typeof(FTZOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.FTZInterchangeType.FTZEventReporting, repositoryMock.Object).GetType());

			AssertException(() => { ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.ISFInterchangeType.ImporterSecurityFiling, false, false, false, repositoryMock.Object); }, typeof(ApplicationException),
				"Interchange Type 'SF' and Application Code 'USI' for type 'IOutboundConfiguration' is not supported");

			AssertException(() => { ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, "whatever", repositoryMock.Object); }, typeof(ApplicationException),
				"Interchange Type 'whatever' and Application Code 'USI' for type 'IOutboundConfiguration' is not supported");

			//AES
			Assert.AreEqual(typeof(AESOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USExport, "whatever", repositoryMock.Object).GetType());

			//AMA
			Assert.AreEqual(typeof(AMAOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.AMA, "whatever", repositoryMock.Object).GetType());

			//AMS
			Assert.AreEqual(typeof(AMSOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.AMS, "whatever", repositoryMock.Object).GetType());

			//UEM
			Assert.AreEqual(typeof(UEMOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USExportManifest, "whatever", repositoryMock.Object).GetType());

			//MID
			Assert.AreEqual(typeof(MIDOutboundMessageHandler), ConfigurationHelper.GetOutboundMessageHandler(ApplicationCode.USImport, Constants.MIDInterchangeType.ManufacturerNameandAddressAdd, repositoryMock.Object).GetType());
		}
	}
}
