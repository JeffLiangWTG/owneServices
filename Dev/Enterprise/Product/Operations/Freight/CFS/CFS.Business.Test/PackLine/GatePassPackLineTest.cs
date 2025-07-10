using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business
{
	[TestedType(typeof(GatePassPackLine))]
	public class GatePassPackLineTest : CFSBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var packLine = factory.NewWithValidTestData<GatePassPackLine>();
			packLine.PackLocations.AddNew();

			return packLine;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = Factory.New<GatePassShipment>();
			ImportContainer = Factory.New<GatePassContainer>();
			ImportContainer.JC_Purpose = Constants.ContainerPackingMode.Import;
			TestPack = Factory.New<GatePassPackLine>();

			TestPack.JL_JS = Shipment.PK;
			TestPack.SetContainer(ImportContainer.PK);
		}

		public void TestShipment()
		{
			AssertEquals(Shipment, TestPack.Shipment);
		}

		public void TestIsFullyDelivered()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();

			GatePassPackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 7;
			packLine.JL_Outturn = 7;

			CommonPickupDeliveryConfirm leg = shipment.DestinationCFSDepartures.AddNew();
			AssertEquals("Fully Delivered", true, packLine.IsFullyDelivered);

			CommonConfirmDivot divot = leg.GetDivot(packLine);
			divot.J8_PackagesDelivered = 5;
			AssertEquals("Not Fully Delivered", false, packLine.IsFullyDelivered);
		}

		protected GatePassPackLine TestPack;
		protected GatePassShipment Shipment;
		protected GatePassContainer ImportContainer;
	}
}
