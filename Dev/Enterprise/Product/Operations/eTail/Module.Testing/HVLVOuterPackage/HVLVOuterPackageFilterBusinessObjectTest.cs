using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Module.Testing
{
	[TestedType(typeof(HVLVOuterPackageFilterBusinessObject))]
	class HVLVOuterPackageFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Numbers and References

		#region TestJobNumberFilter

		public void TestBarcodeNumberFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(HVLVOuterPackageFilterBusinessObject.Descriptions.BarcodeNumber, FilterCategories.NumbersAndReferences, "Barcode #");
		}

		public void TestBarcodeNumberFilter_SingleMatch()
		{
			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_PackageBarcode = "AAA";
			package2.HVO_PackageBarcode = "BBB";
			package3.HVO_PackageBarcode = "AAB";

			var filterBizo = GetNewFilterStripBusinessObject();
			var barcodeFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.BarcodeNumber] as ModuleTextFilter;
			barcodeFilter.Property = "AAA";

			AssertTextFilterMatches(barcodeFilter, bizo => bizo.HVO_PackageBarcode, new[] { (package1, true), (package2, false), (package3, false) });
		}

		public void TestBarcodeNumberFilter_MultipleMatch()
		{
			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_PackageBarcode = "AAA";
			package2.HVO_PackageBarcode = "BBB";
			package3.HVO_PackageBarcode = "AAB";

			var filterBizo = GetNewFilterStripBusinessObject();
			var barcodeFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.BarcodeNumber] as ModuleTextFilter;
			barcodeFilter.Property = "AA";

			AssertTextFilterMatches(barcodeFilter, bizo => bizo.HVO_PackageBarcode, new[] { (package1, true), (package2, false), (package3, true) });
		}

		#endregion

		#region TestContainerNumberFilter

		public void TestContainerNumberFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(HVLVOuterPackageFilterBusinessObject.Descriptions.ContainerNumber, FilterCategories.NumbersAndReferences, "Container #");
		}

		public void TestContainerNumberFilter_SingleMatch()
		{
			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_ContainerNumber = "AAA";
			package2.HVO_ContainerNumber = "BBB";
			package3.HVO_ContainerNumber = "AAB";

			var filterBizo = GetNewFilterStripBusinessObject();
			var containerNumberFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.ContainerNumber] as ModuleTextFilter;
			containerNumberFilter.Property = "AAA";

			AssertTextFilterMatches(containerNumberFilter, bizo => bizo.HVO_ContainerNumber, new[] { (package1, true), (package2, false), (package3, false) });
		}

		public void TestContainerNumberFilter_MultipleMatch()
		{
			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_ContainerNumber = "AAA";
			package2.HVO_ContainerNumber = "BBB";
			package3.HVO_ContainerNumber = "AAB";

			var filterBizo = GetNewFilterStripBusinessObject();
			var containerNumberFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.ContainerNumber] as ModuleTextFilter;
			containerNumberFilter.Property = "AA";

			AssertTextFilterMatches(containerNumberFilter, bizo => bizo.HVO_ContainerNumber, new[] { (package1, true), (package2, false), (package3, true) });
		}

		#endregion

		#region TestReferenceNumberFilter

		public void TestReferenceNumberFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(HVLVOuterPackageFilterBusinessObject.Descriptions.ReferenceNumber, FilterCategories.NumbersAndReferences, "Reference #");
		}

		public void TestReferenceNumberFilter_SingleMatch()
		{
			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_PackageReference = "AAA";
			package2.HVO_PackageReference = "BBB";
			package3.HVO_PackageReference = "AAB";

			var filterBizo = GetNewFilterStripBusinessObject();
			var referenceNumberFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.ReferenceNumber] as ModuleTextFilter;
			referenceNumberFilter.Property = "AAA";

			AssertTextFilterMatches(referenceNumberFilter, bizo => bizo.HVO_PackageReference, new[] { (package1, true), (package2, false), (package3, false) });
		}

		public void TestReferenceNumberFilter_MultipleMatch()
		{
			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_PackageReference = "AAA";
			package2.HVO_PackageReference = "BBB";
			package3.HVO_PackageReference = "AAB";

			var filterBizo = GetNewFilterStripBusinessObject();
			var referenceNumberFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.ReferenceNumber] as ModuleTextFilter;
			referenceNumberFilter.Property = "AA";

			AssertTextFilterMatches(referenceNumberFilter, bizo => bizo.HVO_PackageReference, new[] { (package1, true), (package2, false), (package3, true) });
		}

		#endregion

		#region TestCommodityCodeFilter

		public void TestCommodityCodeFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(HVLVOuterPackageFilterBusinessObject.Descriptions.CommodityCode, FilterCategories.NumbersAndReferences, "Commodity Code");
		}

		public void TestCommodityCodeFilter_SingleMatch()
		{
			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_RH_NKCommodityCode = "CC1";
			package2.HVO_RH_NKCommodityCode = "CC2";
			package3.HVO_RH_NKCommodityCode = "CC1";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var commodityCodeFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.CommodityCode] as ModuleNkFilter;
			commodityCodeFilter.Property = "CC2";

			AssertTextFilterMatches(commodityCodeFilter, bizo => bizo.HVO_RH_NKCommodityCode, new[] { (package1, false), (package2, true), (package3, false) });
		}

		public void TestCommodityCodeFilter_MultipleMatch()
		{
			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_RH_NKCommodityCode = "CC1";
			package2.HVO_RH_NKCommodityCode = "CC2";
			package3.HVO_RH_NKCommodityCode = "CC1";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var commodityCodeFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.CommodityCode] as ModuleNkFilter;
			commodityCodeFilter.Property = "CC1";

			AssertTextFilterMatches(commodityCodeFilter, bizo => bizo.HVO_RH_NKCommodityCode, new[] { (package1, true), (package2, false), (package3, true) });
		}

		#endregion

		#region TestDestinationDepotFilter

		public void TestDestinationDepotFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(HVLVOuterPackageFilterBusinessObject.Descriptions.DestinationDepot, FilterCategories.Organisations, "Destination Depot");
		}

		public void TestDestinationDepotFilter_SingleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_OA_DestinationDepot = org1.MainAddress.PK;
			package2.HVO_OA_DestinationDepot = org2.MainAddress.PK;
			package3.HVO_OA_DestinationDepot = org1.MainAddress.PK;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var destinationDepotFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.DestinationDepot] as ModuleGuidFilter;
			destinationDepotFilter.Property = org2.PK;

			AssertGuidFilterMatches(destinationDepotFilter, bizo => bizo.HVO_OA_DestinationDepot, new[] { (package1, false), (package2, true), (package3, false) });
		}

		public void TestDestinationDepotFilter_MultipleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_OA_DestinationDepot = org1.MainAddress.PK;
			package2.HVO_OA_DestinationDepot = org2.MainAddress.PK;
			package3.HVO_OA_DestinationDepot = org1.MainAddress.PK;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var destinationDepotFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.DestinationDepot] as ModuleGuidFilter;
			destinationDepotFilter.Property = org1.PK;

			AssertGuidFilterMatches(destinationDepotFilter, bizo => bizo.HVO_OA_DestinationDepot, new[] { (package1, true), (package2, false), (package3, true) });
		}

		#endregion

		#endregion

		#region Organisations

		#region TestLastMileCarrierFilter

		public void TestLastMileCarrierFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(HVLVOuterPackageFilterBusinessObject.Descriptions.LastMileCarrier, FilterCategories.Organisations, "Last Mile Carrier");
		}

		public void TestLastMileCarrierFilter_SingleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_OH_LastMileCarrier = org1.PK;
			package2.HVO_OH_LastMileCarrier = org2.PK;
			package3.HVO_OH_LastMileCarrier = org1.PK;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var lastMileCarrierFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.LastMileCarrier] as ModuleGuidFilter;
			lastMileCarrierFilter.Property = org2.PK;

			AssertGuidFilterMatches(lastMileCarrierFilter, bizo => bizo.HVO_OH_LastMileCarrier, new[] { (package1, false), (package2, true), (package3, false) });
		}

		public void TestLastMileCarrierFilter_MultipleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_OH_LastMileCarrier = org1.PK;
			package2.HVO_OH_LastMileCarrier = org2.PK;
			package3.HVO_OH_LastMileCarrier = org1.PK;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var lastMileCarrierFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.LastMileCarrier] as ModuleGuidFilter;
			lastMileCarrierFilter.Property = org1.PK;

			AssertGuidFilterMatches(lastMileCarrierFilter, bizo => bizo.HVO_OH_LastMileCarrier, new[] { (package1, true), (package2, false), (package3, true) });
		}

		#endregion

		#endregion

		#region Modes and Types

		#region TestLastMileCarrierServiceLevelFilter

		public void TestLastMileCarrierServiceLevelFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(HVLVOuterPackageFilterBusinessObject.Descriptions.LastMileCarrierServiceLevel, FilterCategories.ModesAndTypes, "Last Mile Carrier Service Level");
		}

		public void TestLastMileCarrierServiceLevelFilter_SingleMatch()
		{
			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_PL_NKLastMileCarrierServiceLevel = "LV1";
			package2.HVO_PL_NKLastMileCarrierServiceLevel = "LV2";
			package3.HVO_PL_NKLastMileCarrierServiceLevel = "LV1";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var lastMileCarrierServiceLevelFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.LastMileCarrierServiceLevel] as ModuleTextFilter;
			lastMileCarrierServiceLevelFilter.Property = "LV2";

			AssertTextFilterMatches(lastMileCarrierServiceLevelFilter, bizo => bizo.HVO_PL_NKLastMileCarrierServiceLevel, new[] { (package1, false), (package2, true), (package3, false) });
		}

		public void TestLastMileCarrierServiceLevelFilter_MultipleMatch()
		{
			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_PL_NKLastMileCarrierServiceLevel = "LV1";
			package2.HVO_PL_NKLastMileCarrierServiceLevel = "LV2";
			package3.HVO_PL_NKLastMileCarrierServiceLevel = "LV1";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var lastMileCarrierServiceLevelFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.LastMileCarrierServiceLevel] as ModuleTextFilter;
			lastMileCarrierServiceLevelFilter.Property = "LV1";

			AssertTextFilterMatches(lastMileCarrierServiceLevelFilter, bizo => bizo.HVO_PL_NKLastMileCarrierServiceLevel, new[] { (package1, true), (package2, false), (package3, true) });
		}

		#endregion

		#endregion

		#region Status

		public void TestStatusFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(HVLVOuterPackageFilterBusinessObject.Descriptions.Status, FilterCategories.StatusAndFlags, "Status");
		}

		public void TestStatusFilter_SingleMatch()
		{
			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_Status = HVLVOuterPackageStatus.Codes.Open;
			package2.HVO_Status = HVLVOuterPackageStatus.Codes.Closed;
			package3.HVO_Status = HVLVOuterPackageStatus.Codes.Consolidated;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var statusFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.Status] as ModuleTextFilter;
			statusFilter.Property = HVLVOuterPackageStatus.Codes.Open;

			AssertTextFilterMatches(statusFilter, bizo => bizo.HVO_OH_LastMileCarrier, new[] { (package1, true), (package2, false), (package3, false) });
		}

		public void TestStatusFilter_MultipleMatch()
		{
			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_Status = HVLVOuterPackageStatus.Codes.Open;
			package2.HVO_Status = HVLVOuterPackageStatus.Codes.Open;
			package3.HVO_Status = HVLVOuterPackageStatus.Codes.Consolidated;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var statusFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.Status] as ModuleTextFilter;
			statusFilter.Property = HVLVOuterPackageStatus.Codes.Open;

			AssertTextFilterMatches(statusFilter, bizo => bizo.HVO_OH_LastMileCarrier, new[] { (package1, true), (package2, true), (package3, false) });
		}

		#endregion

		#region Other

		#region TestLoadedOnShipmentFilter

		public void TestLoadedOnShipmentFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(HVLVOuterPackageFilterBusinessObject.Descriptions.LoadedOnConsol, FilterCategories.Other, "Loaded-On Consol");
		}

		public void TestLoadedOnShipmentFilter_SingleMatch()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();

			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_JK_LoadedOnConsol = consol1.PK;
			package2.HVO_JK_LoadedOnConsol = consol2.PK;
			package3.HVO_JK_LoadedOnConsol = consol1.PK;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var loadedOnShipmentFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.LoadedOnConsol] as ModuleGuidFilter;
			loadedOnShipmentFilter.Property = consol2.PK;

			AssertGuidFilterMatches(loadedOnShipmentFilter, bizo => bizo.HVO_JK_LoadedOnConsol, new[] { (package1, false), (package2, true), (package3, false) });
		}

		public void TestLoadedOnShipmentFilter_MultipleMatch()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingConsol>();

			var package1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var package3 = Factory.NewWithValidTestData<HVLVOuterPackage>();

			package1.HVO_JK_LoadedOnConsol = shipment1.PK;
			package2.HVO_JK_LoadedOnConsol = shipment2.PK;
			package3.HVO_JK_LoadedOnConsol = shipment1.PK;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var loadedOnShipmentFilter = filterBizo[HVLVOuterPackageFilterBusinessObject.Descriptions.LoadedOnConsol] as ModuleGuidFilter;
			loadedOnShipmentFilter.Property = shipment1.PK;

			AssertGuidFilterMatches(loadedOnShipmentFilter, bizo => bizo.HVO_JK_LoadedOnConsol, new[] { (package1, true), (package2, false), (package3, true) });
		}

		#endregion

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new HVLVOuterPackageFilterBusinessObject();

		void AssertGuidFilterMatches(ModuleGuidFilter filter, Func<HVLVOuterPackage, object> propertyLookup, IEnumerable<(HVLVOuterPackage bizo, bool shouldMatch)> expectedMatches)
		{
			AssertBizosMatchSingleFilterProperty(filter, filter.Property, propertyLookup, expectedMatches);
		}

		void AssertTextFilterMatches(ModuleTextBaseFilter filter, Func<HVLVOuterPackage, object> propertyLookup, IEnumerable<(HVLVOuterPackage bizo, bool shouldMatch)> expectedMatches)
		{
			AssertBizosMatchSingleFilterProperty(filter, filter.Property, propertyLookup, expectedMatches);
		}

		void AssertBizosMatchSingleFilterProperty(ModuleFilter filter, object filterProperty, Func<HVLVOuterPackage, object> propertyLookup, IEnumerable<(HVLVOuterPackage bizo, bool shouldMatch)> expectedMatches)
		{
			CombineAssertions(() =>
			{
				expectedMatches.ForEach(expectedMatch =>
					AssertBizoMatchesFilter($"{filter.Description} filter - {filterProperty} matches {propertyLookup(expectedMatch.bizo)}:", filter.Query, expectedMatch.bizo, expectedMatch.shouldMatch)
				);
			});
		}

		void AssertBizoMatchesFilter(string message, ZQuery query, BusinessObject bizo, bool expectedMatch)
		{
			AssertEquals(message, expectedMatch, bizo.MatchesFilter(query));
		}

		void AssertFilterCategoryAndDescription(string filterIdentifier, FilterCategory expectedCategory, string expectedDescription)
		{
			var filterBizo = GetNewFilterStripBusinessObject();
			var moduleFilter = filterBizo[filterIdentifier];

			AssertNotNull($"{filterIdentifier} - Filter should exist:", moduleFilter);
			CombineAssertions(() =>
			{
				AssertEquals($"{filterIdentifier} - Filter Category:", expectedCategory, moduleFilter.Category);
				AssertEquals($"{filterIdentifier} - Filter Description:", expectedDescription, moduleFilter.MultilingualDescription);
			});
		}

		#endregion
	}
}
