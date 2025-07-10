using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using JobSailing = Enterprise.Freight.Business.JobSailing;
using JobVoyage = Enterprise.Freight.Business.JobVoyage;
using RefUNLOCO = Enterprise.MasterFiles.Business.RefUNLOCO;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(ManifestTallyFilterBusinessObject))]
	sealed class ManifestTallyFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region IsCFSLoadList Test

		public void TestIsCFSLoadListFilter()
		{
			LoadList1.JK_IsCFS = ZBool.True;
			LoadList2.JK_IsCFS = ZBool.False;

			Factory.Save();

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		#endregion

		#region Flag Filter Tests

		public void TestFullyUnpackedFilter()
		{
			Pack1.JL_Outturn = Pack1.JL_PackageCount - 1;
			Pack2.JL_Outturn = Pack2.JL_PackageCount;
			Pack3.JL_Outturn = Pack3.JL_PackageCount + 1;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Not Fully Unpacked"];

			filter.Property = "ALL";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));
			Assert("Expecting collection to contain Container3", FilterCollection.Contains(Container3));

			filter.Property = "IFU";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));
			Assert("Expecting collection to contain Container3", FilterCollection.Contains(Container3));

			filter.Property = "NFU";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
			Assert("Expecting collection not to contain Container3", !FilterCollection.Contains(Container3));
		}

		public void TestDamagedPillagedFilter()
		{
			Pack1.JL_Damaged = 1;
			Pack2.JL_Damaged = 0;
			Pack3.JL_Damaged = 0;

			Pack1.JL_Pillaged = 0;
			Pack2.JL_Pillaged = 1;
			Pack3.JL_Pillaged = 0;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Damaged / Pillaged"];

			filter.Property = "ALL";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));
			Assert("Expecting collection to contain Container3", FilterCollection.Contains(Container3));

			filter.Property = "IDP";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));
			Assert("Expecting collection not to contain Container3", !FilterCollection.Contains(Container3));

			filter.Property = "NDP";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
			Assert("Expecting collection to contain Container3", FilterCollection.Contains(Container3));
		}

		public void TestShortSurplusFilter()
		{
			Pack1.JL_Outturn = Pack1.JL_PackageCount - 1;
			Pack2.JL_Outturn = Pack2.JL_PackageCount;
			Pack3.JL_Outturn = Pack3.JL_PackageCount + 1;

			Factory.Save();

			ModuleTextFilter surplusFilter = (ModuleTextFilter)FilterBO["Short / Surplus"];

			surplusFilter.Property = "ALL";
			surplusFilter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));
			Assert("Expecting collection to contain Container3", FilterCollection.Contains(Container3));

			surplusFilter.Property = "ISS";
			surplusFilter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
			Assert("Expecting collection to contain Container3", FilterCollection.Contains(Container3));

			surplusFilter.Property = "NSS";
			surplusFilter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));
			Assert("Expecting collection not to contain Container3", !FilterCollection.Contains(Container3));
		}

		#endregion

		#region Number Filter Tests

		public void TestContainerNumberFilter()
		{
			Container1.JC_ContainerNum = "11100011";
			Container2.JC_ContainerNum = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.Container];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestJobNumberFilter()
		{
			Container1.JC_ContainerJobID = "11100011";
			Container2.JC_ContainerJobID = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.ContainerJob];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestShipmentNumberFilter()
		{
			Shipment1.JS_UniqueConsignRef = "11100011";
			Shipment2.JS_UniqueConsignRef = "22000222";

			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.Shipment];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestHouseBillNumberFilter()
		{
			Shipment1.JS_HouseBill = "11100011";
			Shipment2.JS_HouseBill = "22000222";

			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.HouseBill];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestLoadListNumberFilter()
		{
			LoadList1.JK_UniqueConsignRef = "11100011";
			LoadList2.JK_UniqueConsignRef = "22000222";

			Factory.Save();
			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.LoadList];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestCommonNumbersFilter()
		{
			Container1.JC_ContainerNum = "11111111";
			Container2.JC_ContainerNum = "22222222";

			Container1.JC_ContainerJobID = "33333333";
			Container2.JC_ContainerJobID = "44444444";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Common Numbers"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22222222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "444";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "555";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestReferenceNumberFilter()
		{
			NewReferenceNumber(LoadList1, "CA", "CCN", "1234");
			NewReferenceNumber(LoadList2, "CA", "PCN", "3456");
			NewReferenceNumber(LoadList3, "CN", "SLD", "1234");

			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			FilterCollection = new TallyContainerCollection(Factory);
			FilterBO = (ManifestTallyFilterBusinessObject)GetNewFilterStripBusinessObject();
			ReferenceNumberFilter filter = (ReferenceNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.AdditionalReferenceNumbers];

			AssertNull("The filter should be invisible when current company is not a CA company", filter);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);
			FilterCollection = new TallyContainerCollection(Factory);
			FilterBO = (ManifestTallyFilterBusinessObject)GetNewFilterStripBusinessObject();
			filter = (ReferenceNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.AdditionalReferenceNumbers];
			AssertNotNull("The filter should be visible when current company is a CA company", filter);

			filter.IsActive = true;

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "", "", "");
			var results = Factory.Load<TallyContainer>(filter.Query);
			AssertCollectionContains("Expecting collection to contain Container1", Container1, results);
			AssertCollectionContains("Expecting collection to contain Container2", Container2, results);
			AssertCollectionContains("Expecting collection to contain Container3", Container3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "", "", "12");
			results = Factory.Load<TallyContainer>(filter.Query);
			AssertCollectionContains("Expecting collection to contain Container1", Container1, results);
			AssertCollectionNotContains("Expecting collection not to contain Container2", Container2, results);
			AssertCollectionContains("Expecting collection to contain Container3", Container3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.Contains, "CA", "", "3");
			results = Factory.Load<TallyContainer>(filter.Query);
			AssertCollectionContains("Expecting collection to contain Container1", Container1, results);
			AssertCollectionContains("Expecting collection to contain Container2", Container2, results);
			AssertCollectionNotContains("Expecting collection not to contain Container3", Container3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.Contains, "", "CCN", "3");
			results = Factory.Load<TallyContainer>(filter.Query);
			AssertCollectionContains("Expecting collection to contain Container1", Container1, results);
			AssertCollectionNotContains("Expecting collection not to contain Container2", Container2, results);
			AssertCollectionNotContains("Expecting collection not to contain Container3", Container3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "CA", "PCN", "");
			results = Factory.Load<TallyContainer>(filter.Query);
			AssertCollectionNotContains("Expecting collection not to contain Container1", Container1, results);
			AssertCollectionContains("Expecting collection to contain Container2", Container2, results);
			AssertCollectionNotContains("Expecting collection not to contain Container3", Container3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "CA", "PCN", "34");
			results = Factory.Load<TallyContainer>(filter.Query);
			AssertCollectionNotContains("Expecting collection not to contain Container1", Container1, results);
			AssertCollectionContains("Expecting collection to contain Container2", Container2, results);
			AssertCollectionNotContains("Expecting collection not to contain Container3", Container3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "CN", "SLD", "34");
			results = Factory.Load<TallyContainer>(filter.Query);
			AssertCollectionNotContains("Expecting collection not to contain Container1", Container1, results);
			AssertCollectionNotContains("Expecting collection not to contain Container2", Container2, results);
			AssertCollectionNotContains("Expecting collection not to contain Container3", Container3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.Contains, "CN", "SLD", "34");
			results = Factory.Load<TallyContainer>(filter.Query);
			AssertCollectionNotContains("Expecting collection not to contain Container1", Container1, results);
			AssertCollectionNotContains("Expecting collection not to contain Container2", Container2, results);
			AssertCollectionContains("Expecting collection to contain Container3", Container3, results);
		}

		void SetReferenceNumberFilter(ReferenceNumberFilter filter, SQLComparisonOperator op, string country, string type, string property)
		{
			filter.SqlComparisonOperator = op;
			filter.Property = property;
			filter.Country = country;
			filter.Type = type;
		}
		#endregion

		#region Date Filter Tests

		public void TestUnpackDateFilter()
		{
			Container1.JC_LCLUnpack = new ZDateTime(2000, 1, 1);
			Container2.JC_LCLUnpack = new ZDateTime(2000, 2, 2);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.Unpack];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestAvailableDateFilter()
		{
			Container1.JC_LCLAvailable = new ZDateTime(2000, 1, 1);
			Container2.JC_LCLAvailable = new ZDateTime(2000, 2, 2);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.Available];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		public void TestStorageDateFilter()
		{
			Container1.JC_LCLStorageCommences = new ZDateTime(2000, 1, 1);
			Container2.JC_LCLStorageCommences = new ZDateTime(2000, 2, 2);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.Storage];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		#endregion

		#region Organisation Filter Tests

		public void TestClientFilter()
		{
			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO[ConstantsAndReusables.OrgFilterTypes.Client];

			filter.Property = Client1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.Property = Client3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		#endregion

		#region Location Filter Tests

		public void TestLoadDischargeFilter()
		{
			Origin1.JA_RL_NKPortOfLoading = "KRSEL";
			Origin2.JA_RL_NKPortOfLoading = "AUBNE";

			Destination1.JB_RL_NKPortOfDischarge = "AUSYD";
			Destination2.JB_RL_NKPortOfDischarge = "AUSYD";

			Factory.Save();

			ModuleLocationFilter filter = (ModuleLocationFilter)FilterBO[ConstantsAndReusables.PortFilterTypes.LoadDischarge];

			filter.Property1 = "KR";
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.Property1 = "AUBNE";
			filter.Property2 = "AUSYD";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.Property1 = ZString.Empty;
			filter.Property2 = "AUSYD";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.Property1 = "AUSYD";
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		#endregion

		#region Voyage / Vessel Filter Tests

		public void TestVoyageFilter()
		{
			Voyage1.JV_RV_NKVessel = "XAAAX";
			Voyage2.JV_RV_NKVessel = "YAAAY";
			Voyage1.JV_VoyageFlight = "XAAAX";
			Voyage2.JV_VoyageFlight = "YAAAY";

			Factory.Save();

			var filter = (VoyageVesselModuleFilter)FilterBO["Voyage / Flight / Vessel"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.VoyageFlightNo = "XAA";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.VoyageFlightNo = "YAAAY";

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.VoyageFlightNo = "AAA";

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.VoyageFlightNo = "ZZZ";

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", FilterCollection.Contains(Container2));
		}

		public void TestVesselFilter()
		{
			Voyage1.JV_RV_NKVessel = "XAAAX";
			Voyage2.JV_RV_NKVessel = "YAAAY";
			Voyage1.JV_VoyageFlight = "XAAAX";
			Voyage2.JV_VoyageFlight = "YAAAY";

			Factory.Save();

			var filter = (VoyageVesselModuleFilter)FilterBO["Voyage / Flight / Vessel"];

			filter.Vessel = "XAAAX";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.Vessel = "ZZZ";

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.Vessel = "YAA";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.Vessel = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));
		}

		#endregion

		#region Mode / Type Filter Tests

		public void TestContainerModeFilter()
		{
			Container1.JC_ContainerMode = Constants.ContainerModes.FCL;
			Container2.JC_ContainerMode = Constants.ContainerModes.LCL;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Container Mode"];

			filter.Property = Constants.ContainerModes.FCL;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Container1", FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));

			filter.Property = Constants.ContainerModes.LCL;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection to contain Container2", FilterCollection.Contains(Container2));

			filter.Property = Constants.ContainerModes.Bulk;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Container1", !FilterCollection.Contains(Container1));
			Assert("Expecting collection not to contain Container2", !FilterCollection.Contains(Container2));
		}

		#endregion

		#region List Tests

		public void TestListProperties()
		{
			AssertNotNull(FilterBO.ContainerMode_List);
			AssertNotNull(FilterBO.Forwarder_List);
			AssertNotNull(FilterBO.Vessel_List);
			AssertNotNull(FilterBO.Location_List);
			AssertNotNull(FilterBO.UnpackedStatus_List);
			AssertNotNull(FilterBO.DamagedStatus_List);
			AssertNotNull(FilterBO.ShortStatus_List);
		}

		#endregion

		#region TestContainersOnForwardingOnlyConsolsShouldNotBeShown

		public void TestContainersOnForwardingOnlyConsolsShouldNotBeShown()
		{
			ManifestTallyFilterBusinessObject manifestTallyFilter = new ManifestTallyFilterBusinessObject();

			GlbBranch testBranch = Factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_RL_NKHomePort = "AUSYD";
			OrgHeader depotOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			depotOrgProxy.OH_IsMiscFreightServices = true;
			depotOrgProxy.OH_IsPackDepot = true;
			depotOrgProxy.OH_RL_NKClosestPort = "AUSYD";
			testBranch.GB_OH_OrgProxy = depotOrgProxy.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName.ToString(), testBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				manifestTallyFilter.ResetToDefaultValues();
				CommonShipment shipment = (CommonShipment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
				CommonConsol fWConsol = shipment.Consols.AddNew();
				fWConsol.JK_RL_NKLoadPort = "AUSYD";
				fWConsol.JK_RL_NKDischargePort = "NZAKA";
				AssertEquals("Preconditions: Consol should be an Export consol in this test", true, fWConsol.IsExport());
				CommonContainer container = fWConsol.Containers.AddNew();
				PackLine pack = shipment.OuterPackLines.AddNew();
				pack.JL_Outturn = 2;
				pack.JL_PackageCount = 4;
				pack.Containers.Add(container);
				AssertEquals("Early Check: Container should be related to the only packline on the shipment.", true, container.PackLines.Contains(pack));
				Factory.Save();

				TallyContainerCollection collection = new TallyContainerCollection(Factory);
				collection.Load(manifestTallyFilter.Filter);
				AssertEquals("The conainer should not be in the list", false, collection.Contains(container));

				fWConsol.JK_OA_PackDepotAddress = depotOrgProxy.MainAddress.PK; //Attempt to make the consol a loadlist in a natural way rather than setting JK_IsCFS.
				ZGuid sailingPK = CreateSailing(true).PK;
				Factory.Save();
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				PackUnpackLoadListConsol sameConsolAsloadList = newFactory.Load<PackUnpackLoadListConsol>(fWConsol.PK);
				AssertEquals("Precondition: IsCFS should be true", true, sameConsolAsloadList.JK_IsCFS);
				SetupSailing(sameConsolAsloadList, sailingPK);
				newFactory.Save();

				TallyContainerCollection collection2 = new TallyContainerCollection(newFactory);
				collection2.Load(manifestTallyFilter.Filter);
				AssertEquals("Now the conainer should be in the list", true, collection2.Contains(container));
			}
		}

		void SetupSailing(PackUnpackLoadListConsol loadList, ZGuid sailingPK)
		{
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			Transport transport = loadList.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_JX = sailingPK;
		}

		JobSailing CreateSailing(bool import)
		{
			JobSailing sailing = Factory.New<JobSailing>();
			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			VoyageDestination destination = Factory.New<VoyageDestination>();
			JobVoyage voyage = Factory.New<JobVoyage>();

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;

			if (import)
			{
				destination.JB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

				if (GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode == Constants.CountryCodes.Singapore)
				{
					origin.JA_RL_NKPortOfLoading = "AUBNE";
				}
				else
				{
					origin.JA_RL_NKPortOfLoading = "SGSIN";
				}
			}
			else
			{
				origin.JA_RL_NKPortOfLoading = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

				if (GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode == Constants.CountryCodes.Singapore)
				{
					destination.JB_RL_NKPortOfDischarge = "AUBNE";
				}
				else
				{
					destination.JB_RL_NKPortOfDischarge = "SGSIN";
				}
			}

			return sailing;
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ManifestTallyFilterBusinessObject();
		}

		void EnsureCurrentCompanyMatchesCurrentBranch()
		{
			ZQuery locoFilter = new ZQuery(RefUNLOCOSchema.RL_PortName, GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			var loco = Factory.LoadTop1<RefUNLOCO>(locoFilter);

			if (loco != null)
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = loco.RL_RN_NKCountryCode;
			}
		}

		#region Test Business Objects

		TallyContainer Container1;
		TallyContainer Container2;
		TallyContainer Container3;

		PackUnpackLoadListConsol LoadList1;
		PackUnpackLoadListConsol LoadList2;
		PackUnpackLoadListConsol LoadList3;

		PackUnpackShipment Shipment1;
		PackUnpackShipment Shipment2;
		PackUnpackShipment Shipment3;

		TallyPackLine Pack1;
		TallyPackLine Pack2;
		TallyPackLine Pack3;

		VoyageOrigin Origin1;
		VoyageOrigin Origin2;
		VoyageOrigin Origin3;

		VoyageDestination Destination1;
		VoyageDestination Destination2;
		VoyageDestination Destination3;

		JobVoyage Voyage1;
		JobVoyage Voyage2;
		JobVoyage Voyage3;

		JobSailing Sailing1;
		JobSailing Sailing2;
		JobSailing Sailing3;

		Transport Transport1;
		Transport Transport2;
		Transport Transport3;

		OrgHeader Client1;
		OrgHeader Client2;
		OrgHeader Client3;

		#endregion

		TallyContainerCollection FilterCollection;
		ManifestTallyFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			EnsureCurrentCompanyMatchesCurrentBranch();

			Shipment1 = Factory.NewWithValidTestData<PackUnpackShipment>();
			Shipment2 = Factory.NewWithValidTestData<PackUnpackShipment>();
			Shipment3 = Factory.NewWithValidTestData<PackUnpackShipment>();

			Pack1 = Factory.NewWithValidTestData<TallyPackLine>();
			Pack2 = Factory.NewWithValidTestData<TallyPackLine>();
			Pack3 = Factory.NewWithValidTestData<TallyPackLine>();

			Container1 = Factory.NewWithValidTestData<TallyContainer>();
			Container2 = Factory.NewWithValidTestData<TallyContainer>();
			Container3 = Factory.NewWithValidTestData<TallyContainer>();

			LoadList1 = Factory.NewWithValidTestData<PackUnpackLoadListConsol>();
			LoadList2 = Factory.NewWithValidTestData<PackUnpackLoadListConsol>();
			LoadList3 = Factory.NewWithValidTestData<PackUnpackLoadListConsol>();

			Origin1 = Factory.NewWithValidTestData<VoyageOrigin>();
			Origin2 = Factory.NewWithValidTestData<VoyageOrigin>();
			Origin3 = Factory.NewWithValidTestData<VoyageOrigin>();

			Destination1 = Factory.NewWithValidTestData<VoyageDestination>();
			Destination2 = Factory.NewWithValidTestData<VoyageDestination>();
			Destination3 = Factory.NewWithValidTestData<VoyageDestination>();

			Sailing1 = Factory.NewWithValidTestData<JobSailing>();
			Sailing2 = Factory.NewWithValidTestData<JobSailing>();
			Sailing3 = Factory.NewWithValidTestData<JobSailing>();

			Transport1 = Factory.NewWithValidTestData<Transport>();
			Transport2 = Factory.NewWithValidTestData<Transport>();
			Transport3 = Factory.NewWithValidTestData<Transport>();

			Voyage1 = Factory.NewWithValidTestData<JobVoyage>();
			Voyage2 = Factory.NewWithValidTestData<JobVoyage>();
			Voyage3 = Factory.NewWithValidTestData<JobVoyage>();

			Client1 = Factory.NewWithValidTestData<OrgHeader>();
			Client2 = Factory.NewWithValidTestData<OrgHeader>();
			Client3 = Factory.NewWithValidTestData<OrgHeader>();

			Client1.OH_IsForwarder = ZBool.True;
			Client2.OH_IsForwarder = ZBool.True;
			Client3.OH_IsForwarder = ZBool.True;

			LoadList1.JK_IsCFS = ZBool.True;
			LoadList2.JK_IsCFS = ZBool.True;
			LoadList3.JK_IsCFS = ZBool.True;

			LoadList1.JK_OH_Forwarder = Client1.PK;
			LoadList2.JK_OH_Forwarder = Client2.PK;
			LoadList3.JK_OH_Forwarder = Client3.PK;

			Shipment1.Consols.Add(LoadList1);
			Shipment2.Consols.Add(LoadList2);
			Shipment3.Consols.Add(LoadList3);

			Container1.JC_JK = LoadList1.PK;
			Container2.JC_JK = LoadList2.PK;
			Container3.JC_JK = LoadList3.PK;

			Pack1.JL_Outturn = 5;
			Pack2.JL_Outturn = 5;
			Pack3.JL_Outturn = 5;

			Pack1.JL_PackageCount = 10;
			Pack2.JL_PackageCount = 10;
			Pack3.JL_PackageCount = 10;

			Pack1.JL_JS = Shipment1.PK;
			Pack2.JL_JS = Shipment2.PK;
			Pack3.JL_JS = Shipment3.PK;

			Container1.PackLines.Add(Pack1);
			Container2.PackLines.Add(Pack2);
			Container3.PackLines.Add(Pack3);

			Sailing1.JX_JA = Origin1.PK;
			Sailing2.JX_JA = Origin2.PK;
			Sailing3.JX_JA = Origin3.PK;

			Sailing1.JX_JB = Destination1.PK;
			Sailing2.JX_JB = Destination2.PK;
			Sailing3.JX_JB = Destination3.PK;

			Destination1.JB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Destination2.JB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			Destination3.JB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			LoadList1.Transports.Add(Transport1);
			LoadList2.Transports.Add(Transport2);
			LoadList3.Transports.Add(Transport3);

			Transport1.JW_IsLinked = true;
			Transport1.JW_JX = Sailing1.PK;
			Transport2.JW_IsLinked = true;
			Transport2.JW_JX = Sailing2.PK;
			Transport3.JW_IsLinked = true;
			Transport3.JW_JX = Sailing3.PK;

			Origin1.JA_JV = Voyage1.PK;
			Origin2.JA_JV = Voyage2.PK;
			Origin3.JA_JV = Voyage3.PK;

			Destination1.JB_JV = Voyage1.PK;
			Destination2.JB_JV = Voyage2.PK;
			Destination3.JB_JV = Voyage3.PK;

			FilterCollection = new TallyContainerCollection(Factory);
			FilterBO = (ManifestTallyFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		static CusEntryNumber NewReferenceNumber(CFSLoadListConsol loadList, string countryCode, string type, string number)
		{
			CusEntryNumber result = loadList.Numbers.AddNew();
			result.CE_RN_NKCountryCode = countryCode;
			result.CE_EntryType = type;
			result.CE_EntryNum = number;
			return result;
		}
		#endregion
	}
}
