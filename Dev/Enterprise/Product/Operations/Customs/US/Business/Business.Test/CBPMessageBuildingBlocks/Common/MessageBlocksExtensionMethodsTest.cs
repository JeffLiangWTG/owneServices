using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Output;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class MessageBlocksExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetAmount()
		{
			var list = new List<ENS89>();
			var ens89 = new ENS89();
			list.Add(ens89);

			ens89.ClassCode = Core.Constants.USCustoms.FeeCodes.Avocado;
			ens89.TotalAmount = 1m;

			ens89.ClassCode1 = Core.Constants.USCustoms.FeeCodes.Beef;
			ens89.TotalAmount1 = 2m;

			ens89.ClassCode2 = Core.Constants.USCustoms.FeeCodes.Blueberry;
			ens89.TotalAmount2 = 3m;

			ens89.ClassCode3 = Core.Constants.USCustoms.FeeCodes.Cotton;
			ens89.TotalAmount3 = 4m;

			ens89.ClassCode4 = Core.Constants.USCustoms.FeeCodes.FreshLimes;
			ens89.TotalAmount4 = 5m;

			ens89 = new ENS89();
			list.Add(ens89);

			ens89.ClassCode = Core.Constants.USCustoms.FeeCodes.HMF;
			ens89.TotalAmount = 6m;

			ens89.ClassCode1 = Core.Constants.USCustoms.FeeCodes.Honey;
			ens89.TotalAmount1 = 7m;

			ens89.ClassCode2 = Core.Constants.USCustoms.FeeCodes.Mango;
			ens89.TotalAmount2 = 8m;

			ens89.ClassCode3 = Core.Constants.USCustoms.FeeCodes.Mushroom;
			ens89.TotalAmount3 = 9m;

			ens89.ClassCode4 = Core.Constants.USCustoms.FeeCodes.Raspberry;
			ens89.TotalAmount4 = 10m;

			AssertEquals(1m, list.GetAmount(Core.Constants.USCustoms.FeeCodes.Avocado));
			AssertEquals(2m, list.GetAmount(Core.Constants.USCustoms.FeeCodes.Beef));
			AssertEquals(3m, list.GetAmount(Core.Constants.USCustoms.FeeCodes.Blueberry));
			AssertEquals(4m, list.GetAmount(Core.Constants.USCustoms.FeeCodes.Cotton));
			AssertEquals(5m, list.GetAmount(Core.Constants.USCustoms.FeeCodes.FreshLimes));
			AssertEquals(6m, list.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals(7m, list.GetAmount(Core.Constants.USCustoms.FeeCodes.Honey));
			AssertEquals(8m, list.GetAmount(Core.Constants.USCustoms.FeeCodes.Mango));
			AssertEquals(9m, list.GetAmount(Core.Constants.USCustoms.FeeCodes.Mushroom));
			AssertEquals(10m, list.GetAmount(Core.Constants.USCustoms.FeeCodes.Raspberry));
		}

		public void TestIsFailed()
		{
			var block = new ENQJ9();
			block.NarrativeMessage = ACSABIProcessor.Constants.EntrySummaryQuery.BillDataStatus;
			AssertEquals(false, block.IsFailed());

			block.NarrativeMessage = ACSABIProcessor.Constants.EntrySummaryQuery.CollectionDataStatus;
			AssertEquals(false, block.IsFailed());

			block.NarrativeMessage = "Failed";
			AssertEquals(true, block.IsFailed());
		}

		public void TestProcessBillDispositionDetails()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var bill = dec.Bills.AddNew();
			bill.CU_BillNum = "MASTER1";

			var message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message.EM_MessageText = "B001101SV9SO                                00                                  " +
					"SO101901SV9  71001299 0123-456789012AL                      2246 021413         " +
					"SO20CR B00159843                                                                " +
					"SO40R    ALP31222406                                       00000600             " +
					"SO50022713162694BILL DEPARTED                                                   " +
					"SO60022713162696DOCUMENT REQUIRED                               03              " +
					"Y  3910SV9SO00000";

			var block50 = message.MessageBlock.MessageBlocks.OfType<Messaging.Business.MessageBuildingBlocks.ACE.Output.ASESSO50>().FirstOrDefault();
			AssertNotNull(block50);
			AssertEquals("BILL DEPARTED", block50.NarrativeMessage);

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			message2.EM_MessageText = "B018888SV9C1                                                                    WO103901SV9  70000045 01390123456789APLUVESSEL 110          001T 1203142        WO40HAPLUHOUSE1                                            00000010             WO50120314153157SPLIT BILL DOES NOT QUALIFY FOR RELEASE YAPLU001T 1201142704    Y018888SV9CQ00003";

			var houseBill = bill.ChildBills.AddNew();
			houseBill.CU_BillNum = "HOUSE1";

			var blockWO50 = message2.MessageBlock.MessageBlocks.OfType<Messaging.Business.MessageBuildingBlocks.ACE.Output.ACEQWO50>().FirstOrDefault();
			AssertNotNull(blockWO50);
			AssertEquals("SPLIT BILL DOES NOT QUALIFY FOR RELEASE", blockWO50.NarrativeMessage);
		}
	}
}
