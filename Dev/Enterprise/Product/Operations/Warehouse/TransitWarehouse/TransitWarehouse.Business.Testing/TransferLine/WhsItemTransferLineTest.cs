using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class WhsItemTransferLineTest : TestCaseWithFactory
	{
		#region TestPackageState

		public void TestPackageState()
		{
			var transferLine = Factory.New<WhsItemTransferLine>();

			var packageState1 = Factory.New<WhsItemPackageState>();
			var packageState1PK = packageState1.PK;
			transferLine.WTF_WPS_PackageState = packageState1PK;
			AssertEquals(packageState1PK, transferLine.WTF_WPS_PackageState);
			AssertEquals(packageState1, transferLine.PackageState);

			var packageState2 = Factory.New<WhsItemPackageState>();
			var packageState2PK = packageState2.PK;
			transferLine.WTF_WPS_PackageState = packageState2PK;
			AssertEquals(packageState2PK, transferLine.WTF_WPS_PackageState);
			AssertEquals(packageState2, transferLine.PackageState);
		}

		#endregion

		#region TestFromLocation

		public void TestFromLocation()
		{
			var transferLine = Factory.New<WhsItemTransferLine>();

			var location1 = Factory.New<WhsLocation>();
			var location1PK = location1.PK;
			transferLine.WTF_WL_From = location1PK;
			AssertEquals(location1PK, transferLine.WTF_WL_From);
			AssertEquals(location1, transferLine.From);

			var location2 = Factory.New<WhsLocation>();
			var location2PK = location2.PK;
			transferLine.WTF_WL_From = location2PK;
			AssertEquals(location2PK, transferLine.WTF_WL_From);
			AssertEquals(location2, transferLine.From);
		}

		#endregion

		#region TestToLocation

		public void TestToLocation()
		{
			var transferLine = Factory.New<WhsItemTransferLine>();

			var location1 = Factory.New<WhsLocation>();
			var location1PK = location1.PK;
			transferLine.WTF_WL_To = location1PK;
			AssertEquals(location1PK, transferLine.WTF_WL_To);
			AssertEquals(location1, transferLine.To);

			var location2 = Factory.New<WhsLocation>();
			var location2PK = location2.PK;
			transferLine.WTF_WL_To = location2PK;
			AssertEquals(location2PK, transferLine.WTF_WL_To);
			AssertEquals(location2, transferLine.To);
		}

		#endregion

		#region TestTransferHeader

		public void TestTransferHeader()
		{
			var transferLine = Factory.New<WhsItemTransferLine>();

			var transferHeader1 = Factory.New<WhsItemTransferHeader>();
			var transferHeader1PK = transferHeader1.PK;
			transferLine.WTF_WTH_TransitTransferHeader = transferHeader1PK;
			AssertEquals(transferHeader1PK, transferLine.WTF_WTH_TransitTransferHeader);
			AssertEquals(transferHeader1, transferLine.TransferHeader);

			var transferHeader2 = Factory.New<WhsItemTransferHeader>();
			var transferHeader2PK = transferHeader2.PK;
			transferLine.WTF_WTH_TransitTransferHeader = transferHeader2PK;
			AssertEquals(transferHeader2PK, transferLine.WTF_WTH_TransitTransferHeader);
			AssertEquals(transferHeader2, transferLine.TransferHeader);
		}

		#endregion
	}
}
