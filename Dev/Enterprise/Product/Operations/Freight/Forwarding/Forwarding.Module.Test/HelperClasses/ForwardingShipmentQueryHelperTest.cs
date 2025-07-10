using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class ForwardingShipmentQueryHelperTest : TestCaseWithFactory
	{
		public void TestIsHazardous_ShipmentWithCommodityHazardous()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			var packLine1 = shipment1.OuterPackLines.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();

			var hazardousCommodity = Factory.New<RefCommodityCode>();
			hazardousCommodity.RH_Code = "DED";
			hazardousCommodity.RH_IsHazardous = true;

			var safeCommodity = Factory.New<RefCommodityCode>();
			safeCommodity.RH_Code = "SAF";
			safeCommodity.RH_IsHazardous = false;

			packLine1.JL_RH_NKCommodityCode = hazardousCommodity.RH_Code;
			packLine2.JL_RH_NKCommodityCode = safeCommodity.RH_Code;

			Factory.Save();

			var results = new ForwardingShipmentCollection(Factory);
			var shipmentFilter = (ModuleFlagsFilter)FilterStripBizO["Is Hazardous"];
			shipmentFilter.IsActive = true;

			shipmentFilter.Property0 = true;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering shipments for 'Is Hazardous' = true", () =>
			{
				AssertEquals("Result should contain Shipment1 as packLine1 has a hazardous commodity.", true, results.Contains(shipment1.PK));
				AssertEquals("Result should not contain Shipment2 as packLine2 has a safe commodity.", false, results.Contains(shipment2.PK));
			});

			shipmentFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering shipments for 'Is Hazardous' = false", () =>
			{
				AssertEquals("Result should not contain Shipment1 as packLine1 has a hazardous commodity.", false, results.Contains(shipment1.PK));
				AssertEquals("Result should contain Shipment2 as packLine2 has a safe commodity.", true, results.Contains(shipment2.PK));
			});
		}

		public void TestIsHazardous_ShipmentWithDangerousGoodsClass()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			var packLine1 = shipment1.OuterPackLines.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();

			var dangerousGood = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood.DI_IMOClass = "1";
			packLine1.UNDGs.Add(dangerousGood);
			packLine2.UNDGs.RemoveAllFromRelationship();

			Factory.Save();

			var results = new ForwardingShipmentCollection(Factory);
			var shipmentFilter = (ModuleFlagsFilter)FilterStripBizO["Is Hazardous"];
			shipmentFilter.IsActive = true;

			shipmentFilter.Property0 = true;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering shipments for 'Is Hazardous' = true", () =>
			{
				AssertEquals("Result should contain Shipment1 as packLine1 has a dangerous good with a DG class.", true, results.Contains(shipment1.PK));
				AssertEquals("Result should not contain Shipment2 as packLine2 does not have a dangerous good.", false, results.Contains(shipment2.PK));
			});

			shipmentFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering shipments for 'Is Hazardous' = false", () =>
			{
				AssertEquals("Result should not contain Shipment1 as packLine1 has a dangerous good with a DG class.", false, results.Contains(shipment1.PK));
				AssertEquals("Result should contain Shipment2 as packLine2 does not have a dangerous good.", true, results.Contains(shipment2.PK));
			});
		}

		public void TestIsHazardous_ShipmentWithDangerousGoodsWithoutDGClass()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			var packLine1 = shipment1.OuterPackLines.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();

			var dangerousGoodWithoutDGClass = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGoodWithoutDGClass.DI_IMOClass = ZString.Empty;
			packLine1.UNDGs.Add(dangerousGoodWithoutDGClass);
			packLine2.UNDGs.RemoveAllFromRelationship();

			Factory.Save();

			var results = new ForwardingShipmentCollection(Factory);
			var shipmentFilter = (ModuleFlagsFilter)FilterStripBizO["Is Hazardous"];
			shipmentFilter.IsActive = true;

			shipmentFilter.Property0 = true;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering shipments for 'Is Hazardous' = true", () =>
			{
				AssertEquals("Result should not contain Shipment1 as packLine1 has a dangerous good but without a DG class.", false, results.Contains(shipment1.PK));
				AssertEquals("Result should not contain Shipment1 as packLine1 does not have a dangerous good.", false, results.Contains(shipment2.PK));
			});

			shipmentFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering shipments for 'Is Hazardous' = false", () =>
			{
				AssertEquals("Result should contain Shipment1 as packLine1 has a dangerous good but without a DG class.", true, results.Contains(shipment1.PK));
				AssertEquals("Result should contain Shipment1 as packLine1 does not have a dangerous good.", true, results.Contains(shipment2.PK));
			});
		}

		public void TestIsHazardous_ShipmentWithDangerousAndNonDangerousPacklines()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();

			var packLine1a = shipment1.OuterPackLines.AddNew();
			var packLine1b = shipment1.OuterPackLines.AddNew();
			var packLine2a = shipment2.OuterPackLines.AddNew();
			var packLine2b = shipment2.OuterPackLines.AddNew();
			var packLine3a = shipment3.OuterPackLines.AddNew();
			var packLine3b = shipment3.OuterPackLines.AddNew();

			var dangerousGood1 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGood1.DI_IMOClass = "1";

			packLine1a.UNDGs.RemoveAllFromRelationship();
			packLine1b.UNDGs.Add(dangerousGood1);

			var dangerousGoodWithoutDGClass2 = Factory.NewWithValidTestData<UNDGDataItem>();
			dangerousGoodWithoutDGClass2.DI_IMOClass = ZString.Empty;

			packLine2a.UNDGs.Add(dangerousGoodWithoutDGClass2);
			packLine2b.UNDGs.RemoveAllFromRelationship();

			var hazardousCommodity3 = Factory.New<RefCommodityCode>();
			hazardousCommodity3.RH_Code = "DED";
			hazardousCommodity3.RH_IsHazardous = true;
			var safeCommodity3 = Factory.New<RefCommodityCode>();
			safeCommodity3.RH_Code = "SAF";
			safeCommodity3.RH_IsHazardous = false;

			packLine3a.JL_RH_NKCommodityCode = hazardousCommodity3.RH_Code;
			packLine3b.JL_RH_NKCommodityCode = safeCommodity3.RH_Code;

			Factory.Save();

			var results = new ForwardingShipmentCollection(Factory);
			var shipmentFilter = (ModuleFlagsFilter)FilterStripBizO["Is Hazardous"];
			shipmentFilter.IsActive = true;

			shipmentFilter.Property0 = true;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering shipments for 'Is Hazardous' = true", () =>
			{
				AssertEquals("Result should contain Shipment1 as packLine1a has a dangerous good with a DG class.", true, results.Contains(shipment1.PK));
				AssertEquals("Result should not contain Shipment2 as neither packline2a or packline2b are hazardous.", false, results.Contains(shipment2.PK));
				AssertEquals("Result should contain Shipment3 as packLine1a has a hazardous commodity.", true, results.Contains(shipment3.PK));
			});

			shipmentFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering shipments for 'Is Hazardous' = false", () =>
			{
				AssertEquals("Result should not contain Shipment1 as packLine1a has a dangerous good with a DG class.", false, results.Contains(shipment1.PK));
				AssertEquals("Result should contain Shipment2 as neither packline2a or packline2b are hazardous.", true, results.Contains(shipment2.PK));
				AssertEquals("Result should not contain Shipment3 as packLine1a has a hazardous commodity.", false, results.Contains(shipment3.PK));
			});
		}

		protected FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				if (fFilterStripBizO == null)
				{
					fFilterStripBizO = new JobShipmentFilterBusinessObject();
				}
				return fFilterStripBizO;
			}
		}

		FilterStripBusinessObject fFilterStripBizO;
	}
}
