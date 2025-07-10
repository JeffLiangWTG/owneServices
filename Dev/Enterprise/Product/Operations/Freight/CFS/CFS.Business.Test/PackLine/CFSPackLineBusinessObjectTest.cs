using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSPackLine))]
	public class CFSPackLineBusinessObjectTest : CFSBusinessObjectTestCase
	{
		public void TestPackLocations()
		{
			CFSPackLine line = (CFSPackLine)GetNewBusinessObject();

			AssertNotNull(line.PackLocations);
			AssertEquals("CFSPackLine.PackLocations should be of type CFSPackLocationCollection.", typeof(CFSPackLocationCollection), ((PackLine)line).PackLocations.GetType());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CFSShipment shipment = factory.New<CFSShipment>();
			CFSPackLine packLine = shipment.OuterPackLines.AddNew();

			return packLine;
		}

		#endregion
	}
}
