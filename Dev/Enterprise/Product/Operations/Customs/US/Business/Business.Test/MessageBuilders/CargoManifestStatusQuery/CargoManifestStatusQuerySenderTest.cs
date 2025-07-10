using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CargoManifestStatusQuerySenderTest : TestCaseWithFactory
	{
		[TestDate(2024, 06, 06, 13, 30, 00)]
		public void TestCargoManifestStatusQueryIsGenerated()
		{
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Air, declaration.JE_DateOfArrival, true, ZString.Empty);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Rail, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Truck, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty);
		}

		[TestDate(2024, 06, 06, 13, 30, 00)]
		public void TestCargoManifestStatusQueryIsGeneratedForFTZ()
		{
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Air, declaration.JE_DateOfArrival, true, ZString.Empty, true, JobMessageTypeList.Codes.FTZ);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty, true, JobMessageTypeList.Codes.FTZ);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Rail, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty, true, JobMessageTypeList.Codes.FTZ);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Truck, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty, true, JobMessageTypeList.Codes.FTZ);
		}

		[TestDate(2024, 06, 06, 13, 30, 00)]
		public void TestAutoGeneratingOnRegistryETAOptionIsTrue()
		{
			var registryItem = new AutoQueryBillOfLadingCargoManifestStatus();
			registryItem.SendBasedOnETA = false;

			USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(declaration.RegistryCompanyPK, System.Guid.Empty, System.Guid.Empty, registryItem);
			registryItem.SendBasedOnETA = true;
			USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(System.Guid.Empty, declaration.RegistryBranchPK, System.Guid.Empty, registryItem);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty);

			USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(declaration.RegistryCompanyPK, System.Guid.Empty, System.Guid.Empty, registryItem);
			registryItem.SendBasedOnETA = false;
			USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(System.Guid.Empty, declaration.RegistryBranchPK, System.Guid.Empty, registryItem);
			AssertDoesNotGenerateBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, true);

			USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(declaration.RegistryCompanyPK, System.Guid.Empty, System.Guid.Empty, registryItem);
			USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(System.Guid.Empty, declaration.RegistryBranchPK, System.Guid.Empty, registryItem);
			AssertDoesNotGenerateBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, true);
		}

		[TestDate(2024, 06, 06, 13, 30, 00)]
		public void TestGeneratedQueriesOnRegistryETAOptionAreOnlyCreatedOnce()
		{
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Air, declaration.JE_DateOfArrival, true, ZString.Empty);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Air, declaration.JE_DateOfArrival, true, ZString.Empty, clearMessages: false);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty, clearMessages: false);
		}

		[TestDate(2024, 06, 06, 13, 30, 00)]
		public void TestGeneratedQueriesOnRegistryETAOptionAreOnlyCreatedOnceForFTZ()
		{
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Air, declaration.JE_DateOfArrival, true, ZString.Empty, true, JobMessageTypeList.Codes.FTZ);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Air, declaration.JE_DateOfArrival, true, ZString.Empty, false, JobMessageTypeList.Codes.FTZ);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty, true, JobMessageTypeList.Codes.FTZ);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty, false, JobMessageTypeList.Codes.FTZ);
		}

		[TestDate(2024, 06, 06, 13, 30, 00)]
		public void TestAutoGeneratingOnRegistrySaveOptionIsTrue()
		{
			var registryItem = new AutoQueryBillOfLadingCargoManifestStatus();
			registryItem.SendOnFirstSave = false;

			USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(declaration.RegistryCompanyPK, System.Guid.Empty, System.Guid.Empty, registryItem);
			registryItem.SendOnFirstSave = true;
			registryItem.UpdateEntryWithResults = true;
			USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(System.Guid.Empty, declaration.RegistryBranchPK, System.Guid.Empty, registryItem);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, ZDate.Empty, false, "UpdateEntryWithResults");

			USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(declaration.RegistryCompanyPK, System.Guid.Empty, System.Guid.Empty, registryItem);
			registryItem.SendOnFirstSave = false;
			USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(System.Guid.Empty, declaration.RegistryBranchPK, System.Guid.Empty, registryItem);
			AssertDoesNotGenerateBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, false);

			USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(declaration.RegistryCompanyPK, System.Guid.Empty, System.Guid.Empty, registryItem);
			USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(System.Guid.Empty, declaration.RegistryBranchPK, System.Guid.Empty, registryItem);
			AssertDoesNotGenerateBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, false);
		}

		[TestDate(2024, 06, 06, 13, 30, 00)]
		public void TestHeldUntilDateIsEmptyWhenArrivalDateIsTodayOrInThePast()
		{
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(2);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Air, declaration.JE_DateOfArrival, true, ZString.Empty);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty);

			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(1);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Air, declaration.JE_DateOfArrival, true, ZString.Empty);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty);

			declaration.JE_DateOfArrival = ZDateTime.Today;
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Air, declaration.JE_DateOfArrival, true, ZString.Empty);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty);

			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(-1);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Air, declaration.JE_DateOfArrival, true, ZString.Empty);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, declaration.JE_DateOfArrival.AddDays(-1), true, ZString.Empty);

			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Air, declaration.JE_DateOfArrival, true, ZString.Empty);
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, declaration.JE_DateOfArrival, true, ZString.Empty);
		}

		[TestDate(2024, 06, 06, 13, 30, 00)]
		public void TestGeneratedQueriesHeldUntilDateUpdatesWithNewETA()
		{
			var initialArrivalDate = ZDateTime.Today.AddDays(15);
			declaration.JE_DateOfArrival = initialArrivalDate;
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Sea, initialArrivalDate.AddDays(-1), true, ZString.Empty);

			var updatedArrivalDate = ZDateTime.Today.AddDays(30);
			declaration.JE_DateOfArrival = updatedArrivalDate;
			AssertHasCargoManifestQueryMessages("Generated messages HeldUntilDate is arrival date -1", 3, EDIMessageStatusList.Codes.Queued, initialArrivalDate.AddDays(-1), ZString.Empty);

			Factory.Save();
			AssertHasCargoManifestQueryMessages("Generated messages HeldUntilDate is updated to be the new arrival date -1", 3, EDIMessageStatusList.Codes.Queued, updatedArrivalDate.AddDays(-1), ZString.Empty);
		}

		[TestDate(2024, 06, 06, 13, 30, 00)]
		public void TestGeneratedQueriesHeldUntilDateNotUpdatedAfterSent()
		{
			var initialArrivalDate = ZDateTime.Today.AddDays(1);
			declaration.JE_DateOfArrival = initialArrivalDate;
			AssertGeneratesBillOfLadingQueriesForTransportMode(TransportTypeList.Codes.Air, initialArrivalDate, true, ZString.Empty);

			// mark all messages as sent
			foreach (MQEDIMessage message in declaration.Messages)
			{
				if (message.IsCargoManifestQuery)
				{
					message.EM_Status = MQEDIMessage.Status.Sent;
				}
			}

			var updatedArrivalDate = ZDateTime.Today.AddDays(7);
			declaration.JE_DateOfArrival = updatedArrivalDate;
			Factory.Save();

			AssertHasCargoManifestQueryMessages("Generated messages HeldUntilDate is original arrival date", 4, EDIMessageStatusList.Codes.Sent, initialArrivalDate, ZString.Empty);
			AssertHasCargoManifestQueryMessages("No messages with HeldUntilDate is the new arrival date", 0, EDIMessageStatusList.Codes.Queued, updatedArrivalDate, ZString.Empty);
		}

		[TestDate(2024, 06, 06, 13, 30, 00)]
		public void TestAutoQuerySendsAirMawbIfNoHawbExists()
		{
			declaration.Messages.RemoveAndDeleteAll();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			SetUpResponseMessage();

			var nonMasterBills = declaration.Bills.Cast<Bill>().Where(b => !b.IsMasterBill).ToList();
			declaration.Bills.RemoveRange(nonMasterBills);

			CargoManifestStatusQuerySender.MarkForAutoSendingIfEligible(declaration, true);

			AssertHasCargoManifestQueryMessages("Declaration has 3 generated messages", 3, EDIMessageStatusList.Codes.Queued, declaration.JE_DateOfArrival, ZString.Empty);
			AssertEquals("3 messages used the 'MAWB' action code to be generated", 3, GetCountOfMessagesOfSubType(EM_MessageSubTypeList.Codes.CargoManifestMAWBQuery));
			AssertEquals("0 messages used the 'HAWB' action code to be generated", 0, GetCountOfMessagesOfSubType(EM_MessageSubTypeList.Codes.CargoManifestHAWBQuery));
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetUpDeclaration();
			var registryItem = new AutoQueryBillOfLadingCargoManifestStatus();
			registryItem.SendBasedOnETA = true;
			registryItem.SendOnFirstSave = true;
			registryItem.UpdateEntryWithResults = true;
			USCustomsDataRegistry.Instance.AutoQueryBillOfLadingCargoManifestStatus.SetValue(System.Guid.Empty, declaration.RegistryBranchPK, System.Guid.Empty, registryItem);
		}
		JobDeclaration declaration;

		void AssertGeneratesBillOfLadingQueriesForTransportMode(ZString transportMode, ZDateTime expectedHeldUntilDate, bool shouldHeld, ZString updateEntryWithResults, bool clearMessages = true, string shipmentType = "IMP")
		{
			if (clearMessages)
			{
				declaration.Messages.RemoveAndDeleteAll();
			}

			declaration.JE_TransportMode = transportMode;
			declaration.JE_MessageType = shipmentType;

			SetUpResponseMessage();
			CargoManifestStatusQuerySender.MarkForAutoSendingIfEligible(declaration, shouldHeld);

			if (transportMode == TransportTypeList.Codes.Air)
			{
				AssertEquals("Generated messages are a Cargo Manifest Entry Release Status Query", 4, declaration.Messages.Cast<MQEDIMessage>().Count(m => m.IsCargoManifestQuery));
				AssertHasCargoManifestQueryMessages("Declaration has 2 generated messages", 4, EDIMessageStatusList.Codes.Queued, expectedHeldUntilDate, updateEntryWithResults);
				AssertEquals("1 messages used the 'MAWB' action code to be generated", 1, GetCountOfMessagesOfSubType(EM_MessageSubTypeList.Codes.CargoManifestMAWBQuery));
				AssertEquals("3 messages used the 'HAWB' action code to be generated", 3, GetCountOfMessagesOfSubType(EM_MessageSubTypeList.Codes.CargoManifestHAWBQuery));
				var blocks = declaration.Messages.Cast<MQEDIMessage>().SelectMany(m => m.GetMessageBlocks<ACEQWR1>());
				Assert("All blocks must have RequestForRelatedBills flag set", blocks.All(mb => !string.Equals(mb.RequestForRelatedBOLIndicator, "Y")));
			}
			else
			{
				AssertEquals("Generated messages are a Cargo Manifest Entry Release Status Query", 3, declaration.Messages.Cast<MQEDIMessage>().Count(m => m.IsCargoManifestQuery));
				AssertHasCargoManifestQueryMessages("Declaration has 3 generated messages", 3, EDIMessageStatusList.Codes.Queued, expectedHeldUntilDate, updateEntryWithResults);
				AssertEquals("Generated messages used the 'ORT' action code to be generated", 3, GetCountOfMessagesOfSubType(EM_MessageSubTypeList.Codes.CargoManifestBillOfLadingQuery));
				var blocks = declaration.Messages.Cast<MQEDIMessage>().SelectMany(m => m.GetMessageBlocks<ACEQWR1>());
				Assert("All blocks must have RequestForRelatedBills flag set", blocks.All(mb => string.Equals(mb.RequestForRelatedBOLIndicator, "Y")));
			}
		}

		void AssertDoesNotGenerateBillOfLadingQueriesForTransportMode(ZString transportMode, bool shouldHeld)
		{
			declaration.Messages.RemoveAndDeleteAll();
			declaration.JE_TransportMode = transportMode;

			SetUpResponseMessage();
			CargoManifestStatusQuerySender.MarkForAutoSendingIfEligible(declaration, shouldHeld);

			AssertEquals("Generated messages are a Cargo Manifest Entry Release Status Query", 0, declaration.Messages.Cast<MQEDIMessage>().Count(m => m.IsCargoManifestQuery));
		}

		void AssertHasCargoManifestQueryMessages(ZString message, int count, ZString status, ZDateTime heldUntilDate, ZString updateEntryWithResults)
		{
			if (heldUntilDate.IsValid)
			{
				var now = ZDateTime.Now;
				var timespanNow = new TimeSpan(now.Hour, now.Minute, 0);
				heldUntilDate = heldUntilDate.Add(timespanNow);
			}

			var messages = declaration.Messages.Cast<MQEDIMessage>().ToList();
			if (messages.Count(m => m.IsCargoManifestQuery && m.EM_Status == status && m.EM_HeldUntilDate == heldUntilDate) != count)
			{
				CombineAssertions(() =>
				{
					AssertEquals(message + ". Messages are Cargo Manifest Query", count, messages.Count(m => m.IsCargoManifestQuery));
					AssertEquals(message + ". Messages EM_Status are " + status, count, messages.Count(m => m.EM_Status == status));
					AssertEquals(message + ". Messages EM_HeldUntilDate are " + heldUntilDate, count, messages.Count(m => m.EM_HeldUntilDate == heldUntilDate));
					AssertEquals(message + ". Messages EM_ApplicationReference are " + updateEntryWithResults, count, messages.Count(m => m.EM_ApplicationReference == updateEntryWithResults));
				});
			}
		}

		int GetCountOfMessagesOfSubType(string messageSubType)
		{
			return declaration.Messages.Cast<MQEDIMessage>().Count(m => m.EM_MessageSubType == messageSubType);
		}

		void SetUpDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(30);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Weight = 9000m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_InvoiceQuantity = 10000m;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsQuantity = 70m;

			var billMB1 = declaration.Bills.AddNew();
			billMB1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			billMB1.CU_BillNum = "0018210009999";

			var billMB2 = declaration.Bills.AddNew();
			billMB2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			billMB2.CU_BillNum = "0018210009988";

			var billMB3 = declaration.Bills.AddNew();
			billMB3.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			billMB3.CU_BillNum = "0018210009933";

			var billHB1 = declaration.Bills.AddNew();
			billHB1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			billHB1.CU_BillNum = "0018210009977";
			billHB1.CU_ParentBillUniqueCode = billMB1.CU_BillUniqueCode;

			var billHB2 = declaration.Bills.AddNew();
			billHB2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			billHB2.CU_BillNum = "0018210009966";
			billHB2.CU_ParentBillUniqueCode = billMB2.CU_BillUniqueCode;

			var billHB3 = declaration.Bills.AddNew();
			billHB3.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			billHB3.CU_BillNum = "0018210009934";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entryHeader.CH_Status = Common.US.ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
		}

		void SetUpResponseMessage()
		{
			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			responseMessage.EM_Status = EDIMessage.Status.Received;
			responseMessage.EM_MessageNum = "HYEDUSCMT_148191";
			responseMessage.EM_MessageText = "B011101SV9SX                                               HYEDUSCMT_164692     SE10ASV9  71012932 01EI 58-12345678911800000100001101  1101                     SE15RAPLUMST081515A                                        00000100CS   N       SE20CR B00164172                                                                SE40001HK CYCLOHEXYLDIAZENIUMDIOXY                                              SE6029021100000000010000                                                        OI        CYCLOHEXYLDIAZENIUMDIOXY                                              PG01001EPAPS1   Y                        130.027                                SE9013PH6   MISSING DIS DOCUMENTATION                                           PG02PCAS 1234                                                                   PG04YCYCLOHEXYLDIAZENIUMDIOXY                                            1000000PG07ACME                                                                        PG14 EP8241-420                                                                 PG19EPN   012345CHN002                                                          PG19CB                   U.S. DEMO COMPANY               184 TEST STREET        PG20                                     CHICAGO              IL US60056        PG21CB CRAIG SEELIG           2154441234     CRAIG.SEELIG@TEST.COM              PG55NP                                                                          PG19IM                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21IM CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG55CI                                                                          PG19OV                   MARITIME INDUSTRIAL SHIPPERS    600 PORT STREET        PG20                                     NEWARK               NJ US20034        PG21OV JAY SMITH              14101123456    JAY.SMITH@MARITIMESHIPPERS.COM     PG19DEQ                  ACE TEST SUPPLIER HK            171-172 GLOUCESTER ROADPG20                                     HONG KONG               HK             PG21DEQJOHN SMITH             5555555555     JOHN.SMITH@COMPANY.COM             PG19LG                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US194441234    PG21LG CRAIGORY SEELIG        2158885555     TEST@IMPORTER.COM                  PG22 944         CI EP3                                                         PG261000000100000DR                                                             PG262                                                                           PG29KG 000000100000                                                             SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00044                                                               ";

			declaration.Messages.Add(responseMessage);
		}
	}
}
