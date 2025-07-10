using CargoWise.Types;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSPackLineCollectionTest : BaseFreightTest
	{
		public void TestLargestLinesPackageType()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			CFSPackLine packLine1 = shipment.OuterPackLines.AddNew();
			CFSPackLine packLine2 = shipment.OuterPackLines.AddNew();
			CFSPackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine2.JL_PackageCount = 20;
			packLine2.JL_PackageCount = 30;

			packLine1.JL_F3_NKPackType = "XXX";
			packLine2.JL_F3_NKPackType = "YYY";
			packLine3.JL_F3_NKPackType = ZString.Empty;

			AssertEquals("LargestLinesPackageType", "YYY", shipment.OuterPackLines.LargestLinesPackageType);
		}
	}
}
