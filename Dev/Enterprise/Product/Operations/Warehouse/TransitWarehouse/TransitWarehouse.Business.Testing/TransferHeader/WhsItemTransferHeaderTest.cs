using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class WhsItemTransferHeaderTest : TestCaseWithFactory
	{
		#region TestAutoLog

		public void TestAutoLog()
		{
			var transferHeader = Factory.New<WhsItemTransferHeader>();
			AssertEquals(true, transferHeader.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestReferenceNumber

		public void TestReferenceNumber()
		{
			var transferHeader = Factory.New<WhsItemTransferHeader>();
			transferHeader.WTH_ReferenceNumber = "TH123";
			AssertEquals("TH123", transferHeader.WTH_ReferenceNumber);

			transferHeader.WTH_ReferenceNumber = "TH321";
			AssertEquals("TH321", transferHeader.WTH_ReferenceNumber);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			var warehouse1 = Factory.New<WhsWarehouse>();
			var warehouse1PK = warehouse1.PK;

			var transferHeader = Factory.New<WhsItemTransferHeader>();
			transferHeader.WTH_WW_Warehouse = warehouse1PK;
			AssertEquals(warehouse1PK, transferHeader.WTH_WW_Warehouse);
			AssertEquals(warehouse1, transferHeader.Warehouse);

			var warehouse2 = Factory.New<WhsWarehouse>();
			var warehouse2PK = warehouse2.PK;
			transferHeader.WTH_WW_Warehouse = warehouse2PK;
			AssertEquals(warehouse2PK, transferHeader.WTH_WW_Warehouse);
			AssertEquals(warehouse2, transferHeader.Warehouse);
		}

		#endregion

		#region TestIsFinalised

		public void TestIsFinalised()
		{
			var transferHeader = Factory.New<WhsItemTransferHeader>();
			AssertEquals(false, transferHeader.WTH_IsFinalised);

			transferHeader.WTH_IsFinalised = true;
			AssertEquals(true, transferHeader.WTH_IsFinalised);
		}

		#endregion

		#region TestTransferType

		public void TestTransferType()
		{
			var transferHeader = Factory.New<WhsItemTransferHeader>();
			transferHeader.WTH_TransferType = "PUT";
			AssertEquals("PUT", transferHeader.WTH_TransferType);

			transferHeader.WTH_TransferType = "TRF";
			AssertEquals("TRF", transferHeader.WTH_TransferType);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var transferHeader = Factory.New<WhsItemTransferHeader>();
			AssertEquals("Transfer", transferHeader.HumanReadableName);

			transferHeader.WTH_ReferenceNumber = "Test";
			AssertEquals("Transfer Test", transferHeader.HumanReadableName);
		}

		#endregion

		#region TestIDocumentSupportable

		public void TestIDocumentSupportable()
		{
			var transferHeader = Factory.New<WhsItemTransferHeader>();
			AssertNotNull(((IDocumentSupportable)transferHeader).DocumentSupporter);
		}

		#endregion
	}
}
