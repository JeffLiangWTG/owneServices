using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ImportEntryStatusCalculator))]
	[CargoWise.Data.Testing.UseSnapshotProtection]
	sealed class EntryStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		[TestDateIncremental(0, 0, 0, 2)]
		public void TestDeriveStatus()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			MQEDIMessage entrySummaryMessage = (MQEDIMessage)entry.Messages.AddNew(typeof(MQEDIMessage));
			entrySummaryMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entrySummaryMessage.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			entrySummaryMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			entrySummaryMessage.EM_MessageText = "B018888XJ5ER                                               6000790              E08888XJ5 10000657B00001267ACCEPTED - RECORDS REQUIRED             01808        E08888XJ5 10000657B00001267CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002";

			Factory.Save();

			MQEDIMessage cargoReleaseProcessingResult = (MQEDIMessage)entry.Messages.AddNew(typeof(MQEDIMessage));
			cargoReleaseProcessingResult.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cargoReleaseProcessingResult.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			cargoReleaseProcessingResult.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			cargoReleaseProcessingResult.EM_MessageText = "B018888XJ5RR                                                                    R18888XJ5 100006570191-013199000B00001267APLUAPL EMERALD         365  080807    R4            6985445                             00000150PK   APLU             R5100807012306ENTRY DOCUMENTS REQUIRED                                          Y018888XJ5RR00003";

			Factory.Save();

			AssertEquals("Entry Status calculated", ImportEntryStatusList.Codes._06, entry.CH_EntryStatus);

			MQEDIMessage queryEntryStatusResult = (MQEDIMessage)entry.Messages.AddNew(typeof(MQEDIMessage));
			queryEntryStatusResult.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			queryEntryStatusResult.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			queryEntryStatusResult.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatusResponse;
			queryEntryStatusResult.EM_MessageText = "B018888XJ5IS                                               6000795              R18888XJ5 100006570191-013199000B00001267APLUAPL EMERALD         365  080807Q999R4            6985445                             00000150PK   APLU             R5100807012307Override to Intensive                                             Y  8888XJ5IS00003";

			Factory.Save();

			AssertEquals("Entry Status calculated", ImportEntryStatusList.Codes._07, entry.CH_EntryStatus);

			MQEDIMessage entrySummaryResponse = (MQEDIMessage)entry.Messages.AddNew(typeof(MQEDIMessage));
			entrySummaryResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entrySummaryResponse.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.USCustomsImport;
			entrySummaryResponse.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			entrySummaryResponse.EM_MessageText = "B018888XJ5ER                                               6000491              E08888XJ5 10000483         PAPERLESS - FILER RETAIN RECORDS        11   57A     Y  8888XJ5ER00001";

			Factory.Save();

			AssertEquals("Entry Status calculated", ImportEntryStatusList.Codes._05, entry.CH_EntryStatus);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			return new ImportEntryStatusCalculator(entry);
		}

		protected override void SetUp()
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			base.SetUp();
		}
	}
}
