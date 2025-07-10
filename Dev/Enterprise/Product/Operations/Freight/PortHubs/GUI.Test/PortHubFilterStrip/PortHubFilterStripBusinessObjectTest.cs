using Enterprise.Freight.PortHubs.Business;
using Enterprise.Freight.PortHubs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(PortHubFilterStripBusinessObject))]
	sealed class PortHubFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDirection()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection1.TY_Direction = PortHubSelectionDirectionList.Codes.Delivery;

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection2.TY_Direction = PortHubSelectionDirectionList.Codes.Pickup;

			Factory.Save();

			var filterBizO = new PortHubFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBizO[PortHubFilterStripBusinessObject.Descriptions.Direction];
			filter.Property = PortHubSelectionDirectionList.Codes.Pickup;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);

			filter.Property = PortHubSelectionDirectionList.Codes.Delivery;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);
		}

		public void TestServiceLevels()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection1.TY_RS_NKServiceLevel = "D2D";

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection2.TY_RS_NKServiceLevel = "STD";

			Factory.Save();

			var filterBizO = new PortHubFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBizO[PortHubFilterStripBusinessObject.Descriptions.ServiceLevel];
			filter.Property = "D2D";
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);

			filter.Property = "STD";
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);
		}

		public void TestPackTypes()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection1.TY_F3_NKPackType = Core.Constants.PkgUnit.Drum;

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection2.TY_F3_NKPackType = Core.Constants.PkgUnit.Pallet;

			Factory.Save();

			var filterBizO = new PortHubFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBizO[PortHubFilterStripBusinessObject.Descriptions.PackType];
			filter.Property = Core.Constants.PkgUnit.Pallet;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);

			filter.Property = Core.Constants.PkgUnit.Drum;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);
		}

		public void TestFreightMode()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection1.TY_RatingFreightMode = Core.Constants.TransportModes.Sea;

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection2.TY_RatingFreightMode = Core.Constants.TransportModes.Air;

			Factory.Save();

			var filterBizO = new PortHubFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBizO[PortHubFilterStripBusinessObject.Descriptions.Mode];
			filter.Property = Core.Constants.TransportModes.Sea;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);

			filter.Property = Core.Constants.TransportModes.Air;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);
		}

		public void TestUNDGClass()
		{
			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection1.TY_UndgClass = "1";

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection2.TY_UndgClass = "All";

			Factory.Save();

			var filterBizO = new PortHubFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBizO[PortHubFilterStripBusinessObject.Descriptions.UndgClass];
			filter.Property = "All";
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);

			filter.Property = "1";
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);
		}

		public void TestOriginDepot()
		{
			var originDepot1 = Factory.NewWithValidTestData<OrgHeader>();
			var originDepot2 = Factory.NewWithValidTestData<OrgHeader>();

			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection1.TY_OA_DispatchDepotAddress = originDepot1.MainAddress.PK;

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = Factory.NewWithValidTestData<OrgAddress>().PK;
			portHubSelection2.TY_OA_DispatchDepotAddress = originDepot2.MainAddress.PK;

			Factory.Save();

			var filterBizO = new PortHubFilterStripBusinessObject();
			ModuleGuidFilter filter = (ModuleGuidFilter)filterBizO[PortHubFilterStripBusinessObject.Descriptions.OriginDepot];
			filter.Property = originDepot1.PK;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);

			filter.Property = originDepot2.PK;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);
		}

		public void TestDestinationDepot()
		{
			var destinationDepot1 = Factory.NewWithValidTestData<OrgHeader>();
			var destinationDepot2 = Factory.NewWithValidTestData<OrgHeader>();

			var collection = new PortHubSelectionCollectionWrapper(Factory).Collection;

			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = destinationDepot1.MainAddress.PK;

			var portHubSelection2 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection2.TY_OA_DepotAddress = destinationDepot2.MainAddress.PK;

			Factory.Save();

			var filterBizO = new PortHubFilterStripBusinessObject();
			ModuleGuidFilter filter = (ModuleGuidFilter)filterBizO[PortHubFilterStripBusinessObject.Descriptions.DestinationDepot];
			filter.Property = destinationDepot1.PK;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection1, collection);
			AssertCollectionNotContains(portHubSelection2, collection);

			filter.Property = destinationDepot2.PK;
			filter.IsActive = true;

			collection.Load(filterBizO.Filter);
			AssertCollectionContains(portHubSelection2, collection);
			AssertCollectionNotContains(portHubSelection1, collection);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new PortHubFilterStripBusinessObject();
		}
	}
}
