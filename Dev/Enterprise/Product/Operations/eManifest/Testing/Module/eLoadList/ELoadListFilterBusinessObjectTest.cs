namespace Enterprise.eManifest.Module.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Types;
	using Enterprise.eManifest.Business;
	using Enterprise.eManifest.Module;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Modules.Testing;
	using NUnit.Framework;
	using ZArchitecture.Schema;

	[TestedType(typeof(ELoadListFilterBusinessObject))]
	internal class ELoadListFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region MasterBill Filter

		public void TestMasterBillFilter_MasterBillIsNotSpecified_FindAllLists()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_MasterBillNumber = "McLaren";
			list2.DO_MasterBillNumber = "Perez";
			list3.DO_MasterBillNumber = "Button";

			Factory.Save();

			var filterToTest = (ModuleNumberFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.MasterBill];
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZString[] { "McLaren", "Perez", "Button" };
			var actual = filteredObjects.Cast<ELoadList>().Select(l => l.DO_MasterBillNumber).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestMasterBillFilter_MasterBillIsSpecified_FindListsWithSpecifiedMasterBill()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_MasterBillNumber = "McLaren";
			list2.DO_MasterBillNumber = "Perez";
			list3.DO_MasterBillNumber = "Button";

			Factory.Save();

			var filterToTest = (ModuleNumberFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.MasterBill];
			filterToTest.Property = "Per";
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZString[] { "Perez" };
			var actual = filteredObjects.Cast<ELoadList>().Select(l => l.DO_MasterBillNumber).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		#endregion

		#region UniqueReference Filter

		public void TestUniqueReferenceFilter_UniqueReferenceIsNotSpecified_FindAllLists()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_UniqueReference = "McLaren";
			list2.DO_UniqueReference = "Perez";
			list3.DO_UniqueReference = "Button";

			Factory.Save();

			var filterToTest = (ModuleNumberFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.UniqueReference];
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZString[] { "McLaren", "Perez", "Button" };
			var actual = filteredObjects.Cast<ELoadList>().Select(l => l.DO_UniqueReference).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestUniqueReferenceFilter_UniqueReferenceIsSpecified_FindListsWithSpecifiedReference()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_UniqueReference = "McLaren";
			list2.DO_UniqueReference = "Perez";
			list3.DO_UniqueReference = "Button";

			Factory.Save();

			var filterToTest = (ModuleNumberFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.UniqueReference];
			filterToTest.Property = "Per";
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZString[] { "Perez" };
			var actual = filteredObjects.Cast<ELoadList>().Select(l => l.DO_UniqueReference).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		#endregion

		#region Vessel + Voyage/Flight

		public void TestUniqueReferenceFilter_FlightAndVoyageAreNotSpecified_FindAllLists()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_RV_NKVessel = "McLaren";
			list1.DO_VoyageFlight = "111";

			list2.DO_RV_NKVessel = "McLaren";
			list2.DO_VoyageFlight = "222";

			list3.DO_RV_NKVessel = "ManUtd";
			list3.DO_VoyageFlight = "111";

			Factory.Save();

			var filterToTest = (ModuleTextAndNkFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.FlightVoyageNumAndVessel];
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list2.PK, list3.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestUniqueReferenceFilter_FlightVoyageIsSpecified_FindListsWithSpecifiedFlight()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_RV_NKVessel = "McLaren";
			list1.DO_VoyageFlight = "111";

			list2.DO_RV_NKVessel = "McLaren";
			list2.DO_VoyageFlight = "222";

			list3.DO_RV_NKVessel = "ManUtd";
			list3.DO_VoyageFlight = "111";

			Factory.Save();

			var filterToTest = (ModuleTextAndNkFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.FlightVoyageNumAndVessel];
			filterToTest.IsActive = true;
			filterToTest.Property = "11";

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list3.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);

			filterToTest.Property = "2";
			filterToTest.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			expected = new ZGuid[] { list2.PK };
			actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);

			filterToTest.Property = "";
			filterToTest.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			expected = Array.Empty<ZGuid>();
			actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);

			filterToTest.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			expected = new ZGuid[] { list1.PK, list2.PK, list3.PK };
			actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestUniqueReferenceFilter_VesselIsSpecified_FindListsWithSpecifiedVoyage()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_RV_NKVessel = "McLaren";
			list1.DO_VoyageFlight = "111";

			list2.DO_RV_NKVessel = "McLaren";
			list2.DO_VoyageFlight = "222";

			list3.DO_RV_NKVessel = "ManUtd";
			list3.DO_VoyageFlight = "111";

			Factory.Save();

			var filterToTest = (ModuleTextAndNkFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.FlightVoyageNumAndVessel];
			filterToTest.IsActive = true;
			filterToTest.NkProperty = "McLaren";

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list2.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);

			filterToTest.NkProperty = "Man";
			filterToTest.IsActive = true;
			filterToTest.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			expected = new ZGuid[] { list3.PK };
			actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);

			filterToTest.NkProperty = "";
			filterToTest.IsActive = true;
			filterToTest.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			expected = Array.Empty<ZGuid>();
			actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);

			filterToTest.NkProperty = "";
			filterToTest.IsActive = true;
			filterToTest.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			expected = new ZGuid[] { list1.PK, list2.PK, list3.PK };
			actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestUniqueReferenceFilter_VoyageAndFlightAreSpecified_FindListsWithSpecifiedVoyageAndFlight()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_RV_NKVessel = "McLaren";
			list1.DO_VoyageFlight = "111";

			list2.DO_RV_NKVessel = "McLaren";
			list2.DO_VoyageFlight = "222";

			list3.DO_RV_NKVessel = "ManUtd";
			list3.DO_VoyageFlight = "111";

			Factory.Save();

			var filterToTest = (ModuleTextAndNkFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.FlightVoyageNumAndVessel];
			filterToTest.IsActive = true;
			filterToTest.Property = "11";
			filterToTest.NkProperty = "McLaren";

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		#endregion

		#region ETA Filter

		public void TestETAFilter_ETAIsNotSpecified_FindAllLists()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_E_ARV = ZDateTime.Now.AddDays(-1);
			list2.DO_E_ARV = ZDateTime.Now.AddDays(-2);
			list3.DO_E_ARV = ZDateTime.Now.AddDays(-3);

			Factory.Save();

			var filterToTest = (ModuleDateFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.ETA];
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list2.PK, list3.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestETAFilter_ETAIsSpecified_FindListsWithSpecifiedETA()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();
			var list4 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_E_ARV = new DateTime(2013, 6, 14).ToUniversalTime();
			list2.DO_E_ARV = new DateTime(2013, 6, 16).ToUniversalTime();
			list3.DO_E_ARV = new DateTime(2013, 6, 18).ToUniversalTime();
			list4.DO_E_ARV = new DateTime(2013, 6, 20).ToUniversalTime();

			Factory.Save();

			var filteredObjects = new ELoadListCollection(Factory);

			var filterToTest = (ModuleDateFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.ETA];
			filterToTest.IsActive = true;
			filterToTest.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;

			filterToTest.Property1 = new ZDateTime(2013, 6, 13);
			filterToTest.Property2 = new ZDateTime(2013, 6, 17);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list2.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestETADateRangeAndTimeRangeFilterChange()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_E_ARV = new DateTime(2013, 2, 21).ToUniversalTime();
			list2.DO_E_ARV = new DateTime(2013, 2, 23).ToUniversalTime();
			list3.DO_E_ARV = new DateTime(2013, 2, 25).ToUniversalTime();

			Factory.Save();

			var filteredObjects = new ELoadListCollection(Factory);
			var testFilter = (ModuleDateFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.ETA];

			testFilter.IsActive = true;
			testFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			testFilter.Property1 = new ZDateTime(2013, 2, 20);
			testFilter.Property2 = new ZDateTime(2013, 2, 24);

			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list2.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);

			testFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filteredObjects.Load(FilterStripBisnessObject.Filter);
			var expected2 = new ZGuid[] { list1.PK, list2.PK, list3.PK };
			var actual2 = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected2, actual2);
		}

		#endregion

		#region ETD Filter

		public void TestETDFilter_ETDIsNotSpecified_FindAllLists()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_E_DEP = ZDateTime.Now.AddDays(-1);
			list2.DO_E_DEP = ZDateTime.Now.AddDays(-2);
			list3.DO_E_DEP = ZDateTime.Now.AddDays(-3);

			Factory.Save();

			var filterToTest = (ModuleDateFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.ETD];
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list2.PK, list3.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestETDFilter_ETDIsSpecified_FindListsWithSpecifiedETD()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();
			var list4 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_E_DEP = new DateTime(2013, 6, 14).ToUniversalTime();
			list2.DO_E_DEP = new DateTime(2013, 6, 16).ToUniversalTime();
			list3.DO_E_DEP = new DateTime(2013, 6, 18).ToUniversalTime();
			list4.DO_E_DEP = new DateTime(2013, 6, 20).ToUniversalTime();

			Factory.Save();

			var filteredObjects = new ELoadListCollection(Factory);

			var filterToTest = (ModuleDateFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.ETD];
			filterToTest.IsActive = true;
			filterToTest.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;

			filterToTest.Property1 = new ZDateTime(2013, 6, 13);
			filterToTest.Property2 = new ZDateTime(2013, 6, 17);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list2.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestETDDateRangeAndTimeRangeFilterChange()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_E_DEP = new DateTime(2013, 2, 21).ToUniversalTime();
			list2.DO_E_DEP = new DateTime(2013, 2, 23).ToUniversalTime();
			list3.DO_E_DEP = new DateTime(2013, 2, 25).ToUniversalTime();

			Factory.Save();

			var filteredObjects = new ELoadListCollection(Factory);
			var testFilter = (ModuleDateFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.ETD];

			testFilter.IsActive = true;
			testFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			testFilter.Property1 = new ZDateTime(2013, 2, 20);
			testFilter.Property2 = new ZDateTime(2013, 2, 24);

			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list2.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);

			testFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filteredObjects.Load(FilterStripBisnessObject.Filter);
			var expected2 = new ZGuid[] { list1.PK, list2.PK, list3.PK };
			var actual2 = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected2, actual2);
		}

		#endregion

		#region ContainerNumber Filter

		public void TestContainerNumberFilter_ContainerNumberIsNotSpecified_FindAllLists()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_ContainerNumber = "McLaren";
			list2.DO_ContainerNumber = "Perez";
			list3.DO_ContainerNumber = "Button";

			Factory.Save();

			var filterToTest = (ModuleNumberFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.ContainerNum];
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZString[] { "McLaren", "Perez", "Button" };
			var actual = filteredObjects.Cast<ELoadList>().Select(l => l.DO_ContainerNumber).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestContainerNumberFilter_ContainerNumberIsSpecified_FindListsWithSpecifiedContainerNumber()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_ContainerNumber = "McLaren";
			list2.DO_ContainerNumber = "Perez";
			list3.DO_ContainerNumber = "Button";

			Factory.Save();

			var filterToTest = (ModuleNumberFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.ContainerNum];
			filterToTest.Property = "Per";
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZString[] { "Perez" };
			var actual = filteredObjects.Cast<ELoadList>().Select(l => l.DO_ContainerNumber).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		#endregion

		#region Status Filter

		public void TestStatusFilter_StatusIsNotSpecified_FindAllLists()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_Status = "OPN";
			list2.DO_Status = "CLS";
			list3.DO_Status = "OPN";

			Factory.Save();

			var filterToTest = (ModuleTextFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.Status];
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list2.PK, list3.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestStatusFilter_StatusIsSpecified_FindListsWithSpecifiedStatus()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			list1.DO_Status = "OPN";
			list2.DO_Status = "CLS";
			list3.DO_Status = "OPN";

			Factory.Save();

			var filterToTest = (ModuleTextFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.Status];
			filterToTest.Property = "OPN";
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list3.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		#endregion

		#region OriginDepot Filter

		public void TestOriginDepotFilter_OriginDepotIsNotSpecified_FindAllLists()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			var organisation3 = Factory.NewWithValidTestData<OrgHeader>();

			list1.DO_OA_OriginDepot = organisation1.MainAddress.PK;
			list2.DO_OA_OriginDepot = organisation2.MainAddress.PK;
			list3.DO_OA_OriginDepot = organisation3.MainAddress.PK;

			Factory.Save();

			var filterToTest = (ModuleGuidFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.OriginDepot];
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list2.PK, list3.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestOriginDepotFilter_OriginDepotSpecified_FindListsWithSpecifiedOriginDepot()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();

			list1.DO_OA_OriginDepot = organisation1.MainAddress.PK;
			list2.DO_OA_OriginDepot = organisation2.MainAddress.PK;
			list3.DO_OA_OriginDepot = organisation1.MainAddress.PK;

			Factory.Save();

			var filterToTest = (ModuleGuidFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.OriginDepot];
			filterToTest.Property = organisation1.PK;
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list3.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		#endregion

		#region DestinationDepot Filter

		public void TestDestinationDepotFilter_DestinationDepotIsNotSpecified_FindAllLists()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			var organisation3 = Factory.NewWithValidTestData<OrgHeader>();

			list1.DO_OA_DestinationDepot = organisation1.MainAddress.PK;
			list2.DO_OA_DestinationDepot = organisation2.MainAddress.PK;
			list3.DO_OA_DestinationDepot = organisation3.MainAddress.PK;

			Factory.Save();

			var filterToTest = (ModuleGuidFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.DestinationDepot];
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list2.PK, list3.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		public void TestDestinationDepotFilter_DestinationDepotSpecified_FindListsWithSpecifiedDestinationDepot()
		{
			var list1 = Factory.NewWithValidTestData<ELoadList>();
			var list2 = Factory.NewWithValidTestData<ELoadList>();
			var list3 = Factory.NewWithValidTestData<ELoadList>();

			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();

			list1.DO_OA_DestinationDepot = organisation1.MainAddress.PK;
			list2.DO_OA_DestinationDepot = organisation2.MainAddress.PK;
			list3.DO_OA_DestinationDepot = organisation1.MainAddress.PK;

			Factory.Save();

			var filterToTest = (ModuleGuidFilter)FilterStripBisnessObject[ELoadListFilterBusinessObject.Descriptions.DestinationDepot];
			filterToTest.Property = organisation1.PK;
			filterToTest.IsActive = true;

			var filteredObjects = new ELoadListCollection(Factory);
			filteredObjects.Load(FilterStripBisnessObject.Filter);

			var expected = new ZGuid[] { list1.PK, list3.PK };
			var actual = filteredObjects.Select(l => l.PK).ToList();
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		#endregion

		#region FilterStripBusinessObjectTestCase

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ELoadListFilterBusinessObject();
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();

			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, ELoadListFilterBusinessObject.Descriptions.OriginDepot));
			result.Add(TableFilter(OrgAddressSchema.Constants.TableName, ELoadListFilterBusinessObject.Descriptions.DestinationDepot));

			return result;
		}

		#endregion

		#region Implementation

		protected FilterStripBusinessObject FilterStripBisnessObject
		{
			get { return filterStripBisnessObject ?? (filterStripBisnessObject = GetNewFilterStripBusinessObject()); }
		}

		FilterStripBusinessObject filterStripBisnessObject;

		#endregion
	}
}
