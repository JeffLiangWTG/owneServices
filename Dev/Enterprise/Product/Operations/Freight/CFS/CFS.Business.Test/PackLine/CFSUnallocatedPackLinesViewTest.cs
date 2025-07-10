using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSUnallocatedPackLinesViewTest : BaseFreightTest
	{
		public void TestUsingPivotWithLoadListAndSailing()
		{
			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			loadList.AutomaticallyUpdatePackLineContainers = false;
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.Transports[0].JW_JX = new ConstantsAndReusables(Factory).CreateNewSailing(true).PK;

			CFSContainer container = loadList.Containers.AddNew();
			CFSShipment shipment = loadList.Shipments.AddNew();
			shipment.JS_A_RCV = ZDateTime.Today;
			Factory.Save();
			CFSPackLine line1 = shipment.OuterPackLines.AddNew();
			AssertEquals("Count", 1, loadList.UnAllocatedPackLines.Count);

			line1.SetContainer(loadList, container);
			AssertNotNull("Expecting line1 to be in a container on this consol.", line1.GetContainer(loadList));

			CFSPackLine line2 = shipment.OuterPackLines.AddNew();
			line2.SetContainer(loadList, null);

			AssertEquals("Count", 1, loadList.UnAllocatedPackLines.Count);
			AssertEquals("Item 0", line2, loadList.UnAllocatedPackLines[0]);
		}
	}
}
