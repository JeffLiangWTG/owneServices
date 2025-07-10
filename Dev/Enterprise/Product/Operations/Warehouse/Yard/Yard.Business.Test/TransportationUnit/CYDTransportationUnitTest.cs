using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDTransportationUnit))]
	public class CYDTransportationUnitTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var tpu = factory.NewWithValidTestData<CYDTransportationUnit>();
			tpu.YTU_WL_WaitingBayLocation = CreateALocation(factory).PK;
			return tpu;
		}

		WhsLocation CreateALocation(BusinessObjectFactory factory)
		{
			var helper = new WhsTestHelperFunctionsEnv(factory);
			var warehouse = helper.CreateWarehouse("WHS");
			var putAwayArea = helper.CreateArea(warehouse, "PutAway AREA");
			var pickupArea = helper.CreateArea(warehouse, "Pickup AREA");
			var loc = factory.NewWithValidTestData<WhsLocation>();
			loc.WLV_WA_PutawayArea = putAwayArea.PK;
			loc.WLV_WA_PickingArea = pickupArea.PK;
			factory.Save();
			return loc;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CYDTransportationUnit>();
		}

		#region TestWaitingBayLocation

		public void TestWaitingBayLocation()
		{
			var waitingBayLocation = Factory.NewWithValidTestData<WhsLocation>();
			var transportationUnit = (CYDTransportationUnit)GetNewBusinessObject();
			transportationUnit.YTU_WL_WaitingBayLocation = waitingBayLocation.PK;
			AssertEquals(waitingBayLocation, transportationUnit.WaitingBayLocation);
		}

		#endregion

		#region TestGetTemplateSelectionCriteria

		public void TestGetTemplateSelectionCriteria()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var transportUnit1 = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var jobDocAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress1.E2_ParentID = transportUnit1.PK;
			jobDocAddress1.OrganisationPK = client.PK;
			jobDocAddress1.E2_AddressType = AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress;

			var result1 = (ColumnValueRanker)transportUnit1.GetTemplateSelectionCriteria();
			AssertContainsExactElementsInAnyOrder(new[] { client.PK, ZGuid.Empty }, result1.GetValues(ProcessTaskTemplateSchema.P0_OH_Client).Cast<ZGuid>());

			var transportUnit2 = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var result2 = (ColumnValueRanker)transportUnit2.GetTemplateSelectionCriteria();
			AssertContainsExactElementsInAnyOrder(new[] { ZGuid.Empty }, result2.GetValues(ProcessTaskTemplateSchema.P0_OH_Client).Cast<ZGuid>());
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var transportationUnit = Factory.New<CYDTransportationUnit>();
			AssertEquals(Constants.DocManagerCodes.CYDTransportationUnit, ((IDocManagerSupport)transportationUnit).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region eDocs Provider

		public void TestGetEDocsProviderSupport()
		{
			IEDocsProvider businessObj = (IEDocsProvider)GetNewBusinessObject();
			AssertEquals("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), businessObj.GetEDocsProviderSupporter().GetType());
		}

		#endregion

		public void TestDeliveryCollectionProperty()
		{
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			AssertEquals(0, transportationUnit.Deliveries.Count);

			var delivery1 = Factory.NewWithValidTestData<CYDDelivery>();
			delivery1.YDL_YTU_DeliveryTransportationUnit = transportationUnit.PK;
			var delivery2 = Factory.NewWithValidTestData<CYDDelivery>();
			delivery2.YDL_YTU_DeliveryTransportationUnit = transportationUnit.PK;
			AssertEquals(2, transportationUnit.Deliveries.Count);
			AssertEquals(delivery1, transportationUnit.Deliveries[0]);
			AssertEquals(delivery2, transportationUnit.Deliveries[1]);
		}

		public void TestPickupCollectionProperty()
		{
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			AssertEquals(0, transportationUnit.Pickups.Count);

			var pickup1 = Factory.NewWithValidTestData<CYDPickup>();
			pickup1.YPL_YTU_PickupTransportationUnit = transportationUnit.PK;
			var pickup2 = Factory.NewWithValidTestData<CYDPickup>();
			pickup2.YPL_YTU_PickupTransportationUnit = transportationUnit.PK;
			AssertEquals(2, transportationUnit.Pickups.Count);
			AssertEquals(pickup1, transportationUnit.Pickups[0]);
			AssertEquals(pickup2, transportationUnit.Pickups[1]);
		}
	}
}
