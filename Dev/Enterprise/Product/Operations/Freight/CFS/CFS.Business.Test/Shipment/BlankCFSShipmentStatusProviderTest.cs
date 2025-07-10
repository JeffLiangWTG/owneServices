using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class BlankCFSShipmentStatusProviderTest : TestCaseWithFactory
	{
		public void TestStatus()
		{
			AssertEquals(ZString.Empty, provider.Status);
		}

		public void TestShortStatus()
		{
			AssertEquals(ZString.Empty, provider.ShortStatus);
		}

		public void TestDetailsFromMessages()
		{
			AssertEquals(ZString.Empty, provider.DetailsFromMessages);
		}

		public void TestStatusClass()
		{
			AssertEquals(StatusClass.None, provider.StatusClass);
		}

		public void TestCanSaveAndPrint()
		{
			var mock = new Mock<ISaveAndPrintUI>();
			AssertEquals(true, provider.CanSaveAndPrint(mock.Object));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			shipment = Factory.New<GatePassShipment>();
			provider = new BlankCFSShipmentStatusProvider(shipment);
		}

		GatePassShipment shipment;
		BlankCFSShipmentStatusProvider provider;

		#endregion
	}
}
