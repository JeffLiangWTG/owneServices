using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(JobMawbFilterBusinessObject))]
	public class JobMawbFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TextFiltersTests

		public void TestServiceLevelList()
		{
			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			carrier.OH_IsAirLine = true;
			carrier.MiscServ.OM_RM_Airline = RefAirline.LoadFromAirlinePrefix(Factory, "999").PK;
			OrgCarrierServiceLevel carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrierServiceLevel.PL_Code = "ZUB";
			carrierServiceLevel.PL_CarrierServiceLevelDescription = "ZUBIN";
			Factory.Save();

			JobMawbFilterBusinessObject filter = new JobMawbFilterBusinessObject();
			AssertEquals(1, ((ModuleTextFilter)filter["Service Level"]).List.Count);
			AssertEquals(1, ((ModuleTextFilter)filter["Service Level"]).List.Count);

			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;
			((ModuleTextFilter)filter["Masterbill Number"]).Property = "999353535";

			AssertEquals(2, ((ModuleTextFilter)filter["Service Level"]).List.Count);
			AssertEquals(2, ((ModuleTextFilter)filter["Service Level"]).List.Count);

			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;
			((ModuleTextFilter)filter["Masterbill Number"]).Property = "111983535";

			AssertEquals(1, ((ModuleTextFilter)filter["Service Level"]).List.Count);
			AssertEquals(1, ((ModuleTextFilter)filter["Service Level"]).List.Count);
		}

		public void TestServiceLevelFilter()
		{
			JobMawb serviceLevel1 = Factory.NewWithValidTestData<JobMawb>();
			JobMawb serviceLevel2 = Factory.NewWithValidTestData<JobMawb>();
			serviceLevel1.JM_ServiceLevel = "Ind";
			serviceLevel2.JM_ServiceLevel = "Som";
			Factory.Save();
			JobMawbFilterBusinessObject filter = new JobMawbFilterBusinessObject();
			((ModuleTextFilter)filter["Service Level"]).Property = "Ind";
			((ModuleTextFilter)filter["Service Level"]).IsActive = true;
			JobMawbCollection mawbs = new JobMawbCollection(Factory, filter.Filter);
			mawbs.Load();
			AssertCollectionContains(serviceLevel1, mawbs);
			AssertCollectionNotContains(serviceLevel2, mawbs);
		}

		public void TestPopulatingServiceLevelList()
		{
			var filter = new JobMawbFilterBusinessObject();
			var serviceLevelFilter = (ModuleTextFilter)filter["Service Level"];

			AssertContainsExactElementsInAnyOrder(new[] { "STD" }, (serviceLevelFilter.List as CodeDescriptionPairList).GetAllCodes());

			Func<string, string, string, OrgHeader> makeTestCarrierWithValidTestData = (prefix, serviceLevelCode, serviceLevelDescrtion) =>
			{
				var airline = RefAirline.LoadFromAirlinePrefix(Factory, prefix);
				if (airline == null)
				{
					airline = Factory.NewWithValidTestData<RefAirline>();
					airline.RM_EagleAddedAirlinePrefixOrAccountingCode = prefix;
				}
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.MiscServ.OM_RM_Airline = airline.PK;
				OrgCarrierServiceLevel carrierServiceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
				carrierServiceLevel.PL_Code = serviceLevelCode;
				carrierServiceLevel.PL_CarrierServiceLevelDescription = serviceLevelDescrtion;

				Factory.Save();

				return carrier;
			};

			var carrier1 = makeTestCarrierWithValidTestData("081", "NOV", "NOVA1");

			var masterBillFilter1 = (ModuleTextFilter)filter["Masterbill Number"];
			masterBillFilter1.Property = "081";
			masterBillFilter1.IsActive = true;

			var mawbs = new JobMawbCollection(Factory);

			mawbs.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "NOV", "STD" }, (serviceLevelFilter.List as CodeDescriptionPairList).GetAllCodes());

			var carrier2 = makeTestCarrierWithValidTestData("222", "NUB", "NOOBY");

			var masterBillFilter2 = (ModuleTextFilter)filter.CreateDuplicateFor("Masterbill Number");
			masterBillFilter2.Property = "222";
			masterBillFilter2.IsActive = true;

			mawbs.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "NOV", "STD", "NUB" }, (serviceLevelFilter.List as CodeDescriptionPairList).GetAllCodes());

			var carrier3 = makeTestCarrierWithValidTestData("333", "NOV", "NOVA1");

			var masterBillFilter3 = (ModuleTextFilter)filter.CreateDuplicateFor("Masterbill Number");
			masterBillFilter3.Property = "333";
			masterBillFilter3.IsActive = true;

			mawbs.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "NOV", "STD", "NUB" }, (serviceLevelFilter.List as CodeDescriptionPairList).GetAllCodes());
		}

		public void TestMultipleMasterBillNumberModuleFilters()
		{
			var masterbill1 = Factory.NewWithValidTestData<JobMawb>();
			var masterbill2 = Factory.NewWithValidTestData<JobMawb>();

			masterbill1.JM_Airline3DigitPrefix = "081";
			masterbill2.JM_Airline3DigitPrefix = "222";
			Factory.Save();

			var filter = new JobMawbFilterBusinessObject();

			var masterBillFilter1 = (ModuleTextFilter)filter["Masterbill Number"];
			masterBillFilter1.Property = "081";
			masterBillFilter1.IsActive = true;

			var masterBillFilter2 = (ModuleTextFilter)filter.CreateDuplicateFor("Masterbill Number");
			masterBillFilter2.Property = "222";
			masterBillFilter2.IsActive = true;

			masterBillFilter1.OrCategory = FilterOrCategory.Red;
			masterBillFilter2.OrCategory = FilterOrCategory.Red;

			var mawbs = new JobMawbCollection(Factory);
			mawbs.Load(filter.Filter);

			AssertCollectionContains(masterbill1, mawbs);
			AssertCollectionContains(masterbill2, mawbs);
		}

		public void TestAirlineFilter()
		{
			JobMawb airline1 = Factory.NewWithValidTestData<JobMawb>();
			JobMawb airline2 = Factory.NewWithValidTestData<JobMawb>();
			airline1.JM_Airline3DigitPrefix = "123";
			airline2.JM_Airline3DigitPrefix = "789";

			Factory.Save();

			JobMawbFilterBusinessObject filter = new JobMawbFilterBusinessObject();
			((ModuleTextFilter)filter["Airline"]).Property = "123";
			((ModuleTextFilter)filter["Airline"]).IsActive = true;

			JobMawbCollection mawbs = new JobMawbCollection(Factory, filter.Filter);
			mawbs.Load();

			AssertCollectionContains(airline1, mawbs);
			AssertCollectionNotContains(airline2, mawbs);
		}

		public void TestBranchFilter()
		{
			var branchSyd = Factory.LoadTop1<GlbBranch>(new ZQuery().AddToFilter(GlbBranchSchema.GB_Code, "SYD"));
			var branchBne = Factory.LoadTop1<GlbBranch>(new ZQuery().AddToFilter(GlbBranchSchema.GB_Code, "BNE"));
			var airline1 = Factory.NewWithValidTestData<JobMawb>();
			airline1.JM_Airline3DigitPrefix = "123";
			airline1.JM_GB = branchSyd.PK;
			var airline2 = Factory.NewWithValidTestData<JobMawb>();
			airline2.JM_Airline3DigitPrefix = "789";
			airline2.JM_GB = branchBne.PK;

			Factory.Save();

			var filter = new JobMawbFilterBusinessObject();
			((ModuleGuidFilter)filter["Branch"]).Property = branchSyd.PK;
			((ModuleGuidFilter)filter["Branch"]).IsActive = true;
			var mawbs = new JobMawbCollection(Factory, filter.Filter);
			mawbs.Load();
			AssertCollectionContains(airline1, mawbs);
			AssertCollectionNotContains(airline2, mawbs);

			filter = new JobMawbFilterBusinessObject();
			((ModuleGuidFilter)filter["Branch"]).Property = branchBne.PK;
			((ModuleGuidFilter)filter["Branch"]).IsActive = true;
			mawbs = new JobMawbCollection(Factory, filter.Filter);
			mawbs.Load();
			AssertCollectionContains(airline2, mawbs);
			AssertCollectionNotContains(airline1, mawbs);
		}

		public void TestCompanyFilter()
		{
			var anotherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery()
				.AddToFilter(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var mawb1 = Factory.NewWithValidTestData<JobMawb>();
			mawb1.JM_Airline3DigitPrefix = "123";
			mawb1.JM_GC_Company = GlbCompany.CurrentCompany.PK;
			var mawb2 = Factory.NewWithValidTestData<JobMawb>();
			mawb2.JM_Airline3DigitPrefix = "123";
			mawb2.JM_GC_Company = anotherCompany.PK;

			Factory.Save();

			var filter = new JobMawbFilterBusinessObject();
			((ModuleGuidFilter)filter["Company"]).Property = GlbCompany.CurrentCompany.PK;
			((ModuleGuidFilter)filter["Company"]).IsActive = true;
			var mawbs = new JobMawbCollection(Factory, filter.Filter);
			mawbs.Load();
			AssertCollectionContains(mawb1, mawbs);
			AssertCollectionNotContains(mawb2, mawbs);

			filter = new JobMawbFilterBusinessObject();
			((ModuleGuidFilter)filter["Company"]).Property = anotherCompany.PK;
			((ModuleGuidFilter)filter["Company"]).IsActive = true;
			mawbs = new JobMawbCollection(Factory, filter.Filter);
			mawbs.Load();
			AssertCollectionContains(mawb2, mawbs);
			AssertCollectionNotContains(mawb1, mawbs);
		}

		public void TestSerialNumberFilter()
		{
			JobMawb serialNumber1 = Factory.NewWithValidTestData<JobMawb>();
			JobMawb serialNumber2 = Factory.NewWithValidTestData<JobMawb>();
			serialNumber1.JM_MAWB = "12345678";
			serialNumber2.JM_MAWB = "56784567";

			Factory.Save();

			JobMawbFilterBusinessObject filter = new JobMawbFilterBusinessObject();
			((ModuleTextFilter)filter["Serial No."]).Property = "12345678";
			((ModuleTextFilter)filter["Serial No."]).IsActive = true;

			JobMawbCollection mawbs = new JobMawbCollection(Factory, filter.Filter);
			mawbs.Load();

			AssertCollectionContains(serialNumber1, mawbs);
			AssertCollectionNotContains(serialNumber2, mawbs);
		}

		public void TestJobInvoicingStatusFilter()
		{
			var masterbillNumber1 = Factory.NewWithValidTestData<JobMawb>();
			masterbillNumber1.JM_Airline3DigitPrefix = "123";
			masterbillNumber1.JM_MAWB = "SomeNum0";

			var job = new JobHeader.Loader(masterbillNumber1).TryLoadOrCreate();
			AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);

			Factory.Save();

			var filterCollection = new JobMawbCollection(Factory);
			var filterBO = new JobMawbFilterBusinessObject();
			var filter = (ModuleTextBaseFilter)filterBO["Invoicing Job Status"];
			filter.Property = JobHeaderStatus.Working.Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			filterCollection.Load(filterBO.Filter);

			AssertCollectionContains(masterbillNumber1, filterCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filterCollection.Load(filterBO.Filter);

			AssertCollectionNotContains(masterbillNumber1, filterCollection);
		}

		public void TestMasterbillNumberFilter()
		{
			JobMawb masterbillNumber1 = Factory.NewWithValidTestData<JobMawb>();
			JobMawb masterbillNumber2 = Factory.NewWithValidTestData<JobMawb>();
			JobMawb masterbillNumber3 = Factory.NewWithValidTestData<JobMawb>();
			JobMawb masterbillNumber4 = Factory.NewWithValidTestData<JobMawb>();
			masterbillNumber1.JM_Airline3DigitPrefix = "123";
			masterbillNumber1.JM_MAWB = "SomeNum0";
			masterbillNumber2.JM_Airline3DigitPrefix = "123";
			masterbillNumber2.JM_MAWB = "Num12300";
			masterbillNumber3.JM_Airline3DigitPrefix = "456";
			masterbillNumber3.JM_MAWB = "123Num00";
			masterbillNumber4.JM_Airline3DigitPrefix = "123";
			masterbillNumber4.JM_MAWB = "98765432";

			Factory.Save();

			JobMawbFilterBusinessObject filter = new JobMawbFilterBusinessObject();
			((ModuleTextFilter)filter["Masterbill Number"]).Property = "123-98765432";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			JobMawbCollection mawbs = new JobMawbCollection(Factory);
			mawbs.Load(filter.Filter);

			AssertCollectionNotContains(masterbillNumber1, mawbs);
			AssertCollectionNotContains(masterbillNumber2, mawbs);
			AssertCollectionNotContains(masterbillNumber3, mawbs);
			AssertCollectionContains(masterbillNumber4, mawbs);

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "12398765432";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			mawbs.Load(filter.Filter);

			AssertCollectionNotContains(masterbillNumber1, mawbs);
			AssertCollectionNotContains(masterbillNumber2, mawbs);
			AssertCollectionNotContains(masterbillNumber3, mawbs);
			AssertCollectionContains(masterbillNumber4, mawbs);

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "123-SomeNum";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			mawbs.Load(filter.Filter);

			AssertCollectionNotContains(masterbillNumber1, mawbs);
			AssertCollectionNotContains(masterbillNumber2, mawbs);
			AssertCollectionNotContains(masterbillNumber3, mawbs);
			AssertCollectionNotContains(masterbillNumber4, mawbs);

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "123";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			mawbs.Load(filter.Filter);

			AssertCollectionContains(masterbillNumber1, mawbs);
			AssertCollectionContains(masterbillNumber2, mawbs);
			AssertCollectionNotContains(masterbillNumber3, mawbs);
			AssertCollectionContains(masterbillNumber4, mawbs);

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "123Num00";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			mawbs.Load(filter.Filter);

			AssertCollectionNotContains(masterbillNumber1, mawbs);
			AssertCollectionNotContains(masterbillNumber2, mawbs);
			AssertCollectionContains(masterbillNumber3, mawbs);
			AssertCollectionNotContains(masterbillNumber4, mawbs);

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "123";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			mawbs.Load(filter.Filter);

			AssertCollectionContains(masterbillNumber1, mawbs);
			AssertCollectionContains(masterbillNumber2, mawbs);
			AssertCollectionContains(masterbillNumber3, mawbs);
			AssertCollectionContains(masterbillNumber4, mawbs);

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "-123";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			mawbs.Load(filter.Filter);

			AssertCollectionNotContains(masterbillNumber1, mawbs);
			AssertCollectionNotContains(masterbillNumber2, mawbs);
			AssertCollectionContains(masterbillNumber3, mawbs);
			AssertCollectionNotContains(masterbillNumber4, mawbs);

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "123Num";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			mawbs.Load(filter.Filter);

			AssertCollectionNotContains(masterbillNumber1, mawbs);
			AssertCollectionContains(masterbillNumber2, mawbs);
			AssertCollectionContains(masterbillNumber3, mawbs);
			AssertCollectionNotContains(masterbillNumber4, mawbs);

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "45";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.Contains;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			mawbs.Load(filter.Filter);

			AssertCollectionNotContains(masterbillNumber1, mawbs);
			AssertCollectionNotContains(masterbillNumber2, mawbs);
			AssertCollectionContains(masterbillNumber3, mawbs);
			AssertCollectionNotContains(masterbillNumber4, mawbs);

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "123Nu";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.Contains;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			mawbs.Load(filter.Filter);

			AssertCollectionContains(masterbillNumber1, mawbs);
			AssertCollectionContains(masterbillNumber2, mawbs);
			AssertCollectionContains(masterbillNumber3, mawbs);
			AssertCollectionNotContains(masterbillNumber4, mawbs);

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "23";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.Contains;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			mawbs.Load(filter.Filter);

			AssertCollectionContains(masterbillNumber1, mawbs);
			AssertCollectionContains(masterbillNumber2, mawbs);
			AssertCollectionContains(masterbillNumber3, mawbs);
			AssertCollectionContains(masterbillNumber4, mawbs);

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "321";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.Contains;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			mawbs.Load(filter.Filter);

			AssertCollectionNotContains(masterbillNumber1, mawbs);
			AssertCollectionNotContains(masterbillNumber2, mawbs);
			AssertCollectionNotContains(masterbillNumber3, mawbs);
			AssertCollectionNotContains(masterbillNumber4, mawbs);
		}

		public void TestMasterbillNumberFilter_EdgeCases()
		{
			var masterbillNumber1 = Factory.NewWithValidTestData<JobMawb>();
			var masterbillNumber2 = Factory.NewWithValidTestData<JobMawb>();
			var masterbillNumber3 = Factory.NewWithValidTestData<JobMawb>();

			masterbillNumber1.JM_Airline3DigitPrefix = "123";
			masterbillNumber1.JM_MAWB = "TESTTEST";
			masterbillNumber2.JM_Airline3DigitPrefix = "123";
			masterbillNumber2.JM_MAWB = "45678901";
			masterbillNumber3.JM_Airline3DigitPrefix = "456";
			masterbillNumber3.JM_MAWB = "ABCDEFGH";

			Factory.Save();

			var mawbs = new JobMawbCollection(Factory);
			var filter = new JobMawbFilterBusinessObject();

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "1234-TESTTES";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			try
			{
				AssertNoExceptionThrown(() =>
				{
					mawbs.Load(filter.Filter);
				});
			}
			finally
			{
				AssertCollectionContains(masterbillNumber1, mawbs);
				AssertCollectionNotContains(masterbillNumber2, mawbs);
				AssertCollectionNotContains(masterbillNumber3, mawbs);
			}

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "123456789012";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			try
			{
				AssertNoExceptionThrown(() =>
				{
					mawbs.Load(filter.Filter);
				});
			}
			finally
			{
				AssertCollectionNotContains(masterbillNumber1, mawbs);
				AssertCollectionContains(masterbillNumber2, mawbs);
				AssertCollectionNotContains(masterbillNumber3, mawbs);
			}

			((ModuleTextFilter)filter["Masterbill Number"]).Property = "456-ABCDEFGH";
			((ModuleTextFilter)filter["Masterbill Number"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			((ModuleTextFilter)filter["Masterbill Number"]).IsActive = true;

			try
			{
				AssertNoExceptionThrown(() =>
				{
					mawbs.Load(filter.Filter);
				});
			}
			finally
			{
				AssertCollectionNotContains(masterbillNumber1, mawbs);
				AssertCollectionNotContains(masterbillNumber2, mawbs);
				AssertCollectionContains(masterbillNumber3, mawbs);
			}
		}

		#endregion

		#region RelatedItemFiltersTests

		public void TestPortOfLoadingFilter()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();

			JobMawb port1 = Factory.NewWithValidTestData<JobMawb>();
			JobMawb port2 = Factory.NewWithValidTestData<JobMawb>();

			port1.JM_GB = branch1.PK;
			port2.JM_GB = branch2.PK;

			branch1.GB_RL_NKHomePort = "AUSYD";
			branch2.GB_RL_NKHomePort = "UAIEV";

			Factory.Save();

			JobMawbFilterBusinessObject filter = new JobMawbFilterBusinessObject();
			((ModuleNkFilter)filter["Port of Loading"]).Property = "AUSYD";
			((ModuleNkFilter)filter["Port of Loading"]).IsActive = true;

			JobMawbCollection mawbs = new JobMawbCollection(Factory, filter.Filter);
			mawbs.Load();

			AssertCollectionContains(port1, mawbs);
			AssertCollectionNotContains(port2, mawbs);
		}

		public void TestBorrowedFromFilter()
		{
			OrgHeader orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			JobMawb from0 = Factory.NewWithValidTestData<JobMawb>();
			from0.JM_OA_From = orgHeader1.MainAddress.PK;

			JobMawb from1 = Factory.NewWithValidTestData<JobMawb>();
			from1.JM_OA_From = orgHeader2.MainAddress.PK;

			JobMawb from2 = Factory.NewWithValidTestData<JobMawb>();
			from2.JM_OA_From = orgHeader2.MainAddress.PK;

			Factory.Save();

			JobMawbFilterBusinessObject orgHeader1Filter = new JobMawbFilterBusinessObject();
			((ModuleGuidFilter)orgHeader1Filter["Borrowed 'in' From"]).Property = orgHeader1.PK;
			((ModuleGuidFilter)orgHeader1Filter["Borrowed 'in' From"]).IsActive = true;

			JobMawbCollection mawbs = new JobMawbCollection(Factory, orgHeader1Filter.Filter);
			mawbs.Load();

			AssertCollectionContains(from0, mawbs);
			AssertCollectionNotContains(from1, mawbs);
			AssertCollectionNotContains(from2, mawbs);

			JobMawbFilterBusinessObject orgHeader2Filter = new JobMawbFilterBusinessObject();
			((ModuleGuidFilter)orgHeader2Filter["Borrowed 'in' From"]).Property = orgHeader2.PK;
			((ModuleGuidFilter)orgHeader2Filter["Borrowed 'in' From"]).IsActive = true;

			mawbs = new JobMawbCollection(Factory, orgHeader2Filter.Filter);
			mawbs.Load();

			AssertCollectionContains(from1, mawbs);
			AssertCollectionContains(from2, mawbs);
			AssertCollectionNotContains(from0, mawbs);

			JobMawbFilterBusinessObject emptyOrgHeaderFilter = new JobMawbFilterBusinessObject();
			((ModuleGuidFilter)emptyOrgHeaderFilter["Borrowed 'in' From"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)emptyOrgHeaderFilter["Borrowed 'in' From"]).IsActive = false;

			mawbs = new JobMawbCollection(Factory, emptyOrgHeaderFilter.Filter);
			mawbs.Load();

			AssertCollectionContains(from0, mawbs);
			AssertCollectionContains(from1, mawbs);
			AssertCollectionContains(from2, mawbs);
		}

		public void TestBorrowedToFilter()
		{
			OrgHeader orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			JobMawb to0 = Factory.NewWithValidTestData<JobMawb>();
			to0.JM_OH_AllocatedTo = orgHeader1.PK;

			JobMawb to1 = Factory.NewWithValidTestData<JobMawb>();
			to1.JM_OH_AllocatedTo = orgHeader2.PK;

			JobMawb to2 = Factory.NewWithValidTestData<JobMawb>();
			to2.JM_OH_AllocatedTo = orgHeader2.PK;

			Factory.Save();

			JobMawbFilterBusinessObject orgHeader1Filter = new JobMawbFilterBusinessObject();
			((ModuleGuidFilter)orgHeader1Filter["Borrowed 'out' To"]).Property = orgHeader1.PK;
			((ModuleGuidFilter)orgHeader1Filter["Borrowed 'out' To"]).IsActive = true;

			JobMawbCollection mawbs = new JobMawbCollection(Factory, orgHeader1Filter.Filter);
			mawbs.Load();

			AssertCollectionContains(to0, mawbs);
			AssertCollectionNotContains(to1, mawbs);
			AssertCollectionNotContains(to2, mawbs);

			JobMawbFilterBusinessObject orgHeader2Filter = new JobMawbFilterBusinessObject();
			((ModuleGuidFilter)orgHeader2Filter["Borrowed 'out' To"]).Property = orgHeader2.PK;
			((ModuleGuidFilter)orgHeader2Filter["Borrowed 'out' To"]).IsActive = true;

			mawbs = new JobMawbCollection(Factory, orgHeader2Filter.Filter);
			mawbs.Load();

			AssertCollectionContains(to1, mawbs);
			AssertCollectionContains(to2, mawbs);
			AssertCollectionNotContains(to0, mawbs);

			JobMawbFilterBusinessObject emptyOrgHeaderFilter = new JobMawbFilterBusinessObject();
			((ModuleGuidFilter)emptyOrgHeaderFilter["Borrowed 'out' To"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)emptyOrgHeaderFilter["Borrowed 'out' To"]).IsActive = false;

			mawbs = new JobMawbCollection(Factory, emptyOrgHeaderFilter.Filter);
			mawbs.Load();

			AssertCollectionContains(to0, mawbs);
			AssertCollectionContains(to1, mawbs);
			AssertCollectionContains(to2, mawbs);
		}

		#endregion

		#region FlagsFiltersTests

		public void TestPrintedFilter()
		{
			JobMawb printedMAWB = Factory.NewWithValidTestData<JobMawb>();
			JobMawb nonPrintedMAWB = Factory.NewWithValidTestData<JobMawb>();
			printedMAWB.JM_IsPrinted = true;
			nonPrintedMAWB.JM_IsPrinted = false;

			JobMawbFilterBusinessObject filter = new JobMawbFilterBusinessObject();
			((ModuleTextFilter)filter["Printed"]).IsActive = true;
			AssertEquals("Default value", "Printed", ((ModuleTextFilter)filter["Printed"]).Property);

			JobMawbCollection mawbs = new JobMawbCollection(Factory);
			mawbs.Load(filter.Filter);
			AssertCollectionContains(printedMAWB, mawbs);
			AssertCollectionNotContains(nonPrintedMAWB, mawbs);

			((ModuleTextFilter)filter["Printed"]).Property = "Not Printed";
			((ModuleTextFilter)filter["Printed"]).IsActive = true;

			mawbs.Load(filter.Filter);
			AssertCollectionNotContains(printedMAWB, mawbs);
			AssertCollectionContains(nonPrintedMAWB, mawbs);

			((ModuleTextFilter)filter["Printed"]).Property = "All";
			((ModuleTextFilter)filter["Printed"]).IsActive = true;

			mawbs.Load(filter.Filter);
			AssertCollectionContains(printedMAWB, mawbs);
			AssertCollectionContains(nonPrintedMAWB, mawbs);
		}

		public void TestAllocatedToConsolFilter()
		{
			JobMawb allocatedToConsol = Factory.NewWithValidTestData<JobMawb>();
			JobMawb notAllocatedToConsol = Factory.NewWithValidTestData<JobMawb>();
			allocatedToConsol.JM_ParentTableCode = JobConsolSchema.Constants.Prefix;
			allocatedToConsol.JM_ParentID = new ZGuid("23433453-3456-2312-2313-678909877890");
			notAllocatedToConsol.JM_ParentTableCode = "";
			notAllocatedToConsol.JM_ParentID = new ZGuid("23433453-3456-2312-2313-678902877891");

			JobMawbFilterBusinessObject filter = new JobMawbFilterBusinessObject();
			ModuleFlagsFilter flagsFilter = ((ModuleFlagsFilter)filter["Allocated to Consol / Show Branches"]);
			flagsFilter.IsActive = true;
			AssertEquals("Default value", false, flagsFilter.Property0);
			flagsFilter.Property1 = false;

			JobMawbCollection mawbs = new JobMawbCollection(Factory);
			mawbs.Load(filter.Filter);
			AssertCollectionNotContains(allocatedToConsol, mawbs);
			AssertCollectionContains(notAllocatedToConsol, mawbs);

			flagsFilter.Property0 = true;
			flagsFilter.IsActive = true;

			mawbs.Load(filter.Filter);
			AssertCollectionContains(allocatedToConsol, mawbs);
			AssertCollectionNotContains(notAllocatedToConsol, mawbs);
		}

		public void TestShowCurrentBranchOnlyFilter()
		{
			JobMawb currentBranch = Factory.NewWithValidTestData<JobMawb>();
			JobMawb nonCurrentBranch = Factory.NewWithValidTestData<JobMawb>();
			currentBranch.JM_GB = GlbBranch.CurrentBranch.PK;

			JobMawbFilterBusinessObject filter = new JobMawbFilterBusinessObject();
			ModuleFlagsFilter flagsFilter = ((ModuleFlagsFilter)filter["Allocated to Consol / Show Branches"]);
			flagsFilter.IsActive = true;
			AssertEquals("Default value", true, flagsFilter.Property1);

			JobMawbCollection mawbs = new JobMawbCollection(Factory);
			mawbs.Load(filter.Filter);
			AssertCollectionContains(currentBranch, mawbs);
			AssertCollectionNotContains(nonCurrentBranch, mawbs);

			flagsFilter.Property1 = false;
			flagsFilter.IsActive = true;

			mawbs.Load(filter.Filter);
			AssertCollectionContains(currentBranch, mawbs);
			AssertCollectionContains(nonCurrentBranch, mawbs);
		}

		#endregion

		public void TestProfitLossReasonFilterWithOperators()
		{
			var masterbillNumber1 = Factory.NewWithValidTestData<JobMawb>();
			var masterbillNumber2 = Factory.NewWithValidTestData<JobMawb>();
			var masterbillNumber3 = Factory.NewWithValidTestData<JobMawb>();

			var job1 = new JobHeader.Loader(masterbillNumber1).TryLoadOrCreate();
			job1.JH_ProfitLossReasonCode = "ND1";

			var job2 = new JobHeader.Loader(masterbillNumber2).TryLoadOrCreate();
			job2.JH_ProfitLossReasonCode = "CD1";

			var job3 = new JobHeader.Loader(masterbillNumber3).TryLoadOrCreate();
			job3.JH_ProfitLossReasonCode = string.Empty;

			Factory.Save();

			var mawbs = new JobMawbCollection(Factory);
			var filter = new JobMawbFilterBusinessObject();

			var profitLossReasonFilter = (ModuleTextFilter)filter["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			mawbs.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { masterbillNumber1 }, mawbs);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			mawbs.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { masterbillNumber1 }, mawbs);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			mawbs.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { masterbillNumber1, masterbillNumber2 }, mawbs);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			mawbs.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { masterbillNumber2, masterbillNumber3 }, mawbs);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			mawbs.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { masterbillNumber2, masterbillNumber3 }, mawbs);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			mawbs.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { masterbillNumber2, masterbillNumber3 }, mawbs);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "ND1";

			mawbs.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { masterbillNumber3 }, mawbs);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			mawbs.Load(filter.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { masterbillNumber1, masterbillNumber2 }, mawbs);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobMawbFilterBusinessObject();
		}

		#endregion
	}
}
