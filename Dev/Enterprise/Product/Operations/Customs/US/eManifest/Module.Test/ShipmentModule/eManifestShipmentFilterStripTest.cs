using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Module.Testing
{
	[TestedType(typeof(eManifestShipmentFilterStrip))]
	sealed class eManifestShipmentFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestModuleFilter()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_Code = "t1";
			glbCompany.GC_RN_NKCountryCode = "US";
			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_Code = "t2";
			glbCompany2.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_Code = "Ts1";
			var branch2 = glbCompany.Branches.AddNew();
			branch2.GB_GC = glbCompany.PK;
			branch2.GB_Code = "Ts2";
			var trip1 = Factory.NewWithValidTestData<Trip>();
			trip1.BH_GB = branch.PK;
			trip1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			var trip2 = Factory.NewWithValidTestData<Trip>();
			trip2.BH_GB = branch2.PK;
			trip2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			var shipment1 = trip1.Shipments.AddNew().B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Released;
			var shipment2 = trip1.Shipments.AddNew();
			shipment2.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Error;
			var shipment3 = trip2.Shipments.AddNew();
			shipment3.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Cancelled;
			Factory.Save();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var shipments = Factory.Load<Shipment>(new eManifestShipmentFilterStrip().Filter);
				AssertEquals(2, shipments.Length);
				Assert(shipments.Any(x => x.PK == shipment2.PK));
				Assert(shipments.Any(x => x.PK == shipment3.PK));
			}
		}

		public void TestTripReference()
		{
			var filter = (ModuleGuidFilter)FilterStrip[eManifestFilterStrip.Descriptions.TripReferenceFilterId];
			AssertNotNull("Trip Reference filter", filter);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.Equal, trip1.PK, shipment1, shipment2);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.NotEqual, trip1.PK, shipment3, shipment4);
			eManifestFilterStripTest.AssertNoBlankFilter(filter);
			eManifestFilterStripTest.AssertEqualFilter(filter);
		}

		public void TestShipmentControlNumber()
		{
			var filter = (ModuleTextFilter)FilterStrip[eManifestShipmentFilterStrip.Descriptions.ShipmentControlNumberFilterId];
			AssertNotNull("Shipment Control Number filter", filter);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.Equal, "AAGC123456789012", shipment1, shipment2);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.NotEqual, "AAGC123456789012", shipment3, shipment4);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.StartsWith, "AAGC", shipment1, shipment2);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.DoesNotStartWith, "AAGC", shipment3, shipment4);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.Contains, "123", shipment1, shipment2, shipment3);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.NotContains, "123", shipment4);
			AssertProperShipmentsLoaded(filter, SpecialComparisonOperator.IsBlank, string.Empty, shipment4);
			AssertProperShipmentsLoaded(filter, SpecialComparisonOperator.IsNotBlank, string.Empty, shipment1, shipment2, shipment3);
		}

		public void TestShipmentType()
		{
			var filter = (ModuleTextFilter)FilterStrip[eManifestShipmentFilterStrip.Descriptions.ShipmentTypeFilterId];
			AssertNotNull("Shipment Type filter", filter);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.Equal, ShipmentTypes.Codes.Inbond, shipment1, shipment2);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.NotEqual, ShipmentTypes.Codes.Inbond, shipment3, shipment4);
			AssertProperShipmentsLoaded(filter, SpecialComparisonOperator.IsBlank, string.Empty, shipment4);
			AssertProperShipmentsLoaded(filter, SpecialComparisonOperator.IsNotBlank, string.Empty, shipment1, shipment2, shipment3);
			eManifestFilterStripTest.AssertEqualFilter(filter);
		}

		public void TestReleaseStatus()
		{
			var filter = (ModuleTextFilter)FilterStrip[eManifestFilterStrip.Descriptions.ReleaseStatusFilterId];
			AssertNotNull("Release Status filter", filter);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.Equal, ShipmentEntryStatusList.Codes.Accepted, shipment1, shipment2);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.NotEqual, ShipmentEntryStatusList.Codes.Accepted, shipment3, shipment4);
			AssertProperShipmentsLoaded(filter, SpecialComparisonOperator.IsBlank, string.Empty, shipment4);
			AssertProperShipmentsLoaded(filter, SpecialComparisonOperator.IsNotBlank, string.Empty, shipment1, shipment2, shipment3);
			eManifestFilterStripTest.AssertEqualFilter(filter);
		}

		public void TestPortOfLading()
		{
			var filter = (ModuleTextAndNkFilter)FilterStrip[eManifestShipmentFilterStrip.Descriptions.PortOfLadingFilterId];
			AssertNotNull("Port Of Lading filter", filter);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.Equal, "CATOR", shipment1, shipment2);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.NotEqual, "CATOR", shipment3, shipment4);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.StartsWith, "CAT", shipment1, shipment2, shipment3);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.DoesNotStartWith, "CAT", shipment4);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.Contains, "TS", shipment3);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.NotContains, "TS", shipment1, shipment2, shipment4);
			AssertProperShipmentsLoaded(filter, SpecialComparisonOperator.IsBlank, string.Empty, shipment4);
			AssertProperShipmentsLoaded(filter, SpecialComparisonOperator.IsNotBlank, string.Empty, shipment1, shipment2, shipment3);
		}

		public void TestPortOfLadingScheduleK()
		{
			var filter = (ModuleTextAndNkFilter)FilterStrip[eManifestShipmentFilterStrip.Descriptions.PortOfLadingKCodeFilterId];
			AssertNotNull("Port Of Lading (Schedule K) filter", filter);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.Equal, "01535", shipment1, shipment2);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.NotEqual, "01535", shipment3, shipment4);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.StartsWith, "015", shipment1, shipment2, shipment3);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.DoesNotStartWith, "015", shipment4);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.Contains, "53", shipment1, shipment2, shipment3);
			AssertProperShipmentsLoaded(filter, SQLComparisonOperator.NotContains, "53", shipment4);
			AssertProperShipmentsLoaded(filter, SpecialComparisonOperator.IsBlank, string.Empty, shipment4);
			AssertProperShipmentsLoaded(filter, SpecialComparisonOperator.IsNotBlank, string.Empty, shipment1, shipment2, shipment3);
		}

		public void TestShipperConsignee()
		{
			var filter = (ModuleGuidsFilter)FilterStrip[eManifestShipmentFilterStrip.Descriptions.ShipperConsigneeFilterId];
			AssertNotNull("Shipper/Consignee filter", filter);
			AssertProperShipmentsLoaded(filter, shipper.PK, consignee.PK, shipment1);
			AssertProperShipmentsLoaded(filter, shipper.PK, ZGuid.Empty, shipment1, shipment3);
			AssertProperShipmentsLoaded(filter, ZGuid.Empty, consignee.PK, shipment1, shipment2);
			AssertProperShipmentsLoaded(filter, ZGuid.Empty, ZGuid.Empty, shipment1, shipment2, shipment3, shipment4);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new eManifestShipmentFilterStrip();
		}

		protected override void SetUp()
		{
			base.SetUp();
			trip1 = Factory.New<Trip>();
			trip1.BH_JobReference = "MAN0000001";
			shipment1 = trip1.Shipments.AddNew();
			shipment1.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			shipment1.B0_MasterBillNumber = "AAGC123456789012";
			shipment1.B0_RL_NKPortOfLading = "CATOR";
			shipment1.B0_PortOfLadingKCode = "01535";
			shipment1.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "TSTCONSIGNEE";
			consignee.Addresses.AddNewMainAddress();
			shipment1.Consignee.E2_OA_Address = consignee.MainAddress.PK;
			shipper = Factory.NewWithValidTestData<OrgHeader>();
			consignee.Addresses.AddNewMainAddress();
			shipper.OH_Code = "TSTSHIPPER";
			shipment1.Shipper.E2_OA_Address = shipper.MainAddress.PK;
			shipment2 = trip1.Shipments.AddNew();
			shipment2.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			shipment2.B0_MasterBillNumber = "AAGC123456789012";
			shipment2.B0_RL_NKPortOfLading = "CATOR";
			shipment2.B0_PortOfLadingKCode = "01535";
			shipment2.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			shipment2.Consignee.E2_OA_Address = consignee.MainAddress.PK;
			trip2 = Factory.New<Trip>();
			trip2.BH_JobReference = "MAN0000002";
			shipment3 = trip2.Shipments.AddNew();
			shipment3.B0_ShipmentType = ShipmentTypes.Codes.PAPS;
			shipment3.B0_MasterBillNumber = "LOCK987654321233";
			shipment3.B0_RL_NKPortOfLading = "CATST";
			shipment3.B0_PortOfLadingKCode = "01533";
			shipment3.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Released;
			shipment3.Shipper.E2_OA_Address = shipper.MainAddress.PK;
			shipment4 = trip2.Shipments.AddNew();
			shipment4.B0_ShipmentType = ZString.Empty;
			Factory.Save();
		}
		eManifestShipmentFilterStrip filterStrip;
		Shipment shipment1;
		Shipment shipment2;
		Shipment shipment3;
		Shipment shipment4;
		Trip trip1;
		Trip trip2;
		OrgHeader consignee;
		OrgHeader shipper;

		eManifestShipmentFilterStrip FilterStrip => filterStrip ?? (filterStrip = new eManifestShipmentFilterStrip());

		void AssertProperShipmentsLoaded(ModuleTextBaseFilter filter, SQLComparisonOperator comparisonOperator, string property, params Shipment[] expectedShipments)
		{
			filter.SqlComparisonOperator = comparisonOperator;
			filter.Property = property;
			AssertProperShipmentsLoaded(filter, expectedShipments);
		}

		void AssertProperShipmentsLoaded(ModuleGuidsFilter filter, ZGuid property1, ZGuid property2, params Shipment[] expectedShipments)
		{
			filter.Property1 = property1;
			filter.Property2 = property2;
			AssertProperShipmentsLoaded(filter, expectedShipments);
		}

		void AssertProperShipmentsLoaded(ModuleGuidFilter filter, SQLComparisonOperator comparisonOperator, ZGuid property, params Shipment[] expectedShipments)
		{
			filter.SqlComparisonOperator = comparisonOperator;
			filter.Property = property;
			AssertProperShipmentsLoaded(filter, expectedShipments);
		}

		void AssertProperShipmentsLoaded(ModuleFilter filter, Shipment[] expectedShipments)
		{
			var shipments = Factory.Load<Shipment>(filter.Query);
			AssertEquals("Proper quantity of shipments loaded", expectedShipments.Length, shipments.Length);
			foreach (var shipment in expectedShipments)
			{
				Assert("Shipment should be loaded", shipments.Any(s => s.PK == shipment.PK));
			}
		}
	}

	[TestedType(typeof(eManifestShipmentFilterStrip.NoBlankModuleGuidFilter))]
	sealed class NoBlankModuleGuidFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new eManifestShipmentFilterStrip.NoBlankModuleGuidFilter(
				eManifestFilterStrip.Descriptions.TripReferenceFilterId,
				eManifestFilterStrip.Descriptions.TripReferenceMultilingualDescription,
				FilterCategories.NumbersAndReferences,
				ModuleIDs.Customs.US.eManifest,
				CusInBondBillSchema.B0_BH,
				() => eManifestModule.GetNewGridCollection(Factory));
		}
	}
}
