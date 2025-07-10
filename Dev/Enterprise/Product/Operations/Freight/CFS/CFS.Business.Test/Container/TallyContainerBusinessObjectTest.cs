using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business
{
	[TestedType(typeof(TallyContainer))]
	public class TallyContainerBusinessObjectTest : CFSBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.TallyContainer);
			base.SetUp();
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			PackUnpackLoadListConsol loadList = factory.New<PackUnpackLoadListConsol>();
			loadList.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			Transport transport = loadList.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_JX = CreateNewImportSailing(factory).PK;

			loadList.AutomaticallyUpdatePackLineContainers = true;

			loadList.Shipments.AddNew();
			loadList.Shipments[0].OuterPackLines.AddNew();
			loadList.Shipments[0].JS_OuterPacks = 10;

			TallyContainer result = loadList.Containers.AddNew();
			loadList.Shipments[0].OuterPackLines[0].SetContainer(result.PK);

			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			TallyContainer res = Factory.New<TallyContainer>();
			return res;
		}

		#endregion
	}
}
