using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.US.Business.OperationalAction.Testing
{
	sealed class OperationalActionBulkCargoManifestEntryStatusQueryMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendWhenActionIsMAW()
		{
			var declaration = Factory.New<JobDeclaration>();

			var log = new DummyOperationalActionSectionLog();
			var sender = new OperationalActionBulkCargoManifestEntryStatusQueryMessageSender(
				declaration,
				CargoManifestStatusQueryActionList.Codes.MAWB,
				LimitOutputCodeList.Descriptions._0MostRecentResults,
				false,
				false);
			sender.OperationalActionSendMessage(true, log);
			AssertEquals("No message to send", 0, declaration.Messages.Count);

			var master1 = declaration.Bills.AddNew();
			master1.CU_BillNum = "081";
			master1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;

			var master2 = declaration.Bills.AddNew();
			master2.CU_BillNum = "082";
			master2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;

			sender = new OperationalActionBulkCargoManifestEntryStatusQueryMessageSender(
				declaration,
				CargoManifestStatusQueryActionList.Codes.MAWB,
				LimitOutputCodeList.Descriptions._1Last5Results,
				false,
				true);
			sender.OperationalActionSendMessage(true, log);
			AssertEquals("Message should be sent", 2, declaration.Messages.Count);

			var qwr1 = (ACEQWR1)declaration.Messages[0].MessageBlock.MessageBlocks.FirstOrDefault(x => x is ACEQWR1);
			AssertNotNull(qwr1);
			AssertEquals("1", qwr1.LimitOutputOption);
		}

		public void TestSendWhenActionIsHAW()
		{
			var declaration = Factory.New<JobDeclaration>();

			var house = declaration.Bills.AddNew();
			house.CU_BillNum = "083";
			house.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;

			var log = new DummyOperationalActionSectionLog();
			var sender = new OperationalActionBulkCargoManifestEntryStatusQueryMessageSender(
				declaration,
				CargoManifestStatusQueryActionList.Codes.HAWB,
				LimitOutputCodeList.Descriptions._0MostRecentResults,
				false,
				false);
			sender.OperationalActionSendMessage(true, log);
			AssertEquals("Message should be sent", 1, declaration.Messages.Count);

			var qwr1 = (ACEQWR1)declaration.Messages[0].MessageBlock.MessageBlocks.FirstOrDefault(x => x is ACEQWR1);
			AssertNotNull(qwr1);
			AssertEquals("", qwr1.LimitOutputOption);
		}

		public void TestSendWhenActionIsENT()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "qwe";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var log = new DummyOperationalActionSectionLog();
			var sender = new OperationalActionBulkCargoManifestEntryStatusQueryMessageSender(
				declaration,
				CargoManifestStatusQueryActionList.Codes.Entry,
				LimitOutputCodeList.Descriptions._0MostRecentResults,
				true,
				false);
			sender.OperationalActionSendMessage(true, log);

			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			AssertEquals("Message should be sent", 1, declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Count);

			var qwr1 = (CMQR1)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0].MessageBlock.MessageBlocks.FirstOrDefault(x => x is CMQR1);
			AssertNotNull(qwr1);
		}

		public void TestSendWhenActionIsINB()
		{
			var declaration = Factory.New<JobDeclaration>();
			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "M1";

			bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill1.CU_BillNum = "H1";
			bill1.ITNumber = "5678";

			bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill1.CU_BillNum = "H2";
			bill1.ITNumber = "1234";
			bill1.ITAndSplitDetails.AddNew().US_ITNumber = "V12456789";
			bill1.ITAndSplitDetails.AddNew().US_ITNumber = "987654321";

			var log = new DummyOperationalActionSectionLog();
			var sender = new OperationalActionBulkCargoManifestEntryStatusQueryMessageSender(
				declaration,
				CargoManifestStatusQueryActionList.Codes.InBond,
				LimitOutputCodeList.Descriptions._2AllAvailableResults,
				false,
				false);
			sender.OperationalActionSendMessage(true, log);
			AssertEquals("Message should be sent", 4, declaration.Messages.Count);

			var qwr1 = (ACEQWR1)declaration.Messages[0].MessageBlock.MessageBlocks.FirstOrDefault(x => x is ACEQWR1);
			AssertNotNull(qwr1);
			AssertEquals("2", qwr1.LimitOutputOption);
		}

		public void TestSendWhenActionIsORT()
		{
			var declaration = Factory.New<JobDeclaration>();

			var master1 = declaration.Bills.AddNew();
			master1.CU_BillNum = "081";
			master1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;

			var master2 = declaration.Bills.AddNew();
			master2.CU_BillNum = "082";
			master2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;

			var house = declaration.Bills.AddNew();
			house.CU_BillNum = "083";
			house.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;

			var log = new DummyOperationalActionSectionLog();
			var sender = new OperationalActionBulkCargoManifestEntryStatusQueryMessageSender(
				declaration,
				CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill,
				LimitOutputCodeList.Descriptions._2AllAvailableResults,
				false,
				true);
			sender.OperationalActionSendMessage(true, log);
			AssertEquals("Message should be sent", 3, declaration.Messages.Count);

			var qwr1 = (ACEQWR1)declaration.Messages[0].MessageBlock.MessageBlocks.FirstOrDefault(x => x is ACEQWR1);
			AssertNotNull(qwr1);
			AssertEquals("2", qwr1.LimitOutputOption);
			AssertEquals("Y", qwr1.RequestForRelatedBOLIndicator);
		}
	}
}
