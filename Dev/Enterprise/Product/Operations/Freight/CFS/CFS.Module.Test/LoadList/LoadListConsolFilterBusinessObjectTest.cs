using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Module
{
	[TestedType(typeof(LoadListConsolFilterBusinessObject))]
	sealed class LoadListConsolFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Default Filter Test

		public void TestDefaultFilter()
		{
			CommonConsol loadList = Factory.NewWithValidTestData<CommonConsol>();

			Factory.Save();

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList", !FilterCollection.Contains(loadList));
			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));
			Assert("Expecting collection to contain LoadList3", FilterCollection.Contains(LoadList3));
		}

		#endregion

		#region Flag Filter Tests

		public void TestNoShipmentsFilter()
		{
			LoadList1.Shipments.RemoveAll();
			LoadList2.Shipments.Add(Shipment2);

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["All / With / Without Shipments"];

			filter.Property = "ALL";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.Property = "LWS";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.Property = "LNS";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		#endregion

		#region Number Filter Tests

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

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestHouseBillNumberFilter()
		{
			Shipment1.JS_HouseBill = "11100011";
			Shipment2.JS_HouseBill = "22000222";

			LoadList1.Shipments.Add(Shipment1);
			LoadList2.Shipments.Add(Shipment2);

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.HouseBill];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestShipmentNumberFilter()
		{
			Shipment1.JS_UniqueConsignRef = "11100011";
			Shipment2.JS_UniqueConsignRef = "22000222";

			LoadList1.Shipments.Add(Shipment1);
			LoadList2.Shipments.Add(Shipment2);

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.Shipment];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestContainerNumberFilter()
		{
			Container1.JC_ContainerNum = "11100011";
			Container2.JC_ContainerNum = "22000222";

			LoadList1.Containers.Add(Container1);
			LoadList2.Containers.Add(Container2);

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.Container];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestContainerJobNumberFilter()
		{
			Container1.JC_ContainerJobID = "11100011";
			Container2.JC_ContainerJobID = "22000222";

			LoadList1.Containers.Add(Container1);
			LoadList2.Containers.Add(Container2);

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.ContainerJob];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestInterimReceiptNumberFilter()
		{
			Shipment1.JS_InterimReceipt = "11100011";
			Shipment2.JS_InterimReceipt = "22000222";

			LoadList1.Shipments.Add(Shipment1);
			LoadList2.Shipments.Add(Shipment2);

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.InterimReceipt];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestCustomsEntryNumberFilter()
		{
			LoadList1.JK_CustomsReference = "11100011";
			LoadList2.JK_CustomsReference = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.CustomsEntryNumber];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestClientRefFilter()
		{
			LoadList1.JK_AgentsReference = "11100011";
			LoadList2.JK_AgentsReference = "22000222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.ClientRef];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22000222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "000";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestCommonNumbersFilter()
		{
			LoadList1.JK_UniqueConsignRef = "11111111";
			LoadList2.JK_UniqueConsignRef = "22222222";

			Container1.JC_ContainerNum = "33333333";
			Container2.JC_ContainerNum = "44444444";

			LoadList1.Containers.Add(Container1);
			LoadList2.Containers.Add(Container2);

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Common Numbers and References"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "22222222";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "444";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "555";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestReferenceNumberFilter()
		{
			NewReferenceNumber(LoadList1, "CA", "CCN", "1234");
			NewReferenceNumber(LoadList2, "CA", "PCN", "3456");
			NewReferenceNumber(LoadList3, "CN", "SLD", "1234");

			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			FilterCollection = new CFSLoadListConsolCollection(Factory);
			FilterBO = (LoadListConsolFilterBusinessObject)GetNewFilterStripBusinessObject();
			ReferenceNumberFilter filter = (ReferenceNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.AdditionalReferenceNumbers];

			AssertNull("The filter should be invisible when current company is not a CA company", filter);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);
			FilterCollection = new CFSLoadListConsolCollection(Factory);
			FilterBO = (LoadListConsolFilterBusinessObject)GetNewFilterStripBusinessObject();
			filter = (ReferenceNumberFilter)FilterBO[ConstantsAndReusables.NumberFilterTypes.AdditionalReferenceNumbers];
			AssertNotNull("The filter should be visible when current company is a CA company", filter);

			filter.IsActive = true;

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "", "", "");
			var results = Factory.Load<CFSLoadListConsol>(filter.Query);
			AssertCollectionContains("Expecting collection to contain LoadList1", LoadList1, results);
			AssertCollectionContains("Expecting collection to contain LoadList2", LoadList2, results);
			AssertCollectionContains("Expecting collection to contain LoadList3", LoadList3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "", "", "12");
			results = Factory.Load<CFSLoadListConsol>(filter.Query);
			AssertCollectionContains("Expecting collection to contain LoadList1", LoadList1, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList2", LoadList2, results);
			AssertCollectionContains("Expecting collection to contain LoadList3", LoadList3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.Contains, "CA", "", "3");
			results = Factory.Load<CFSLoadListConsol>(filter.Query);
			AssertCollectionContains("Expecting collection to contain LoadList1", LoadList1, results);
			AssertCollectionContains("Expecting collection to contain LoadList2", LoadList2, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList3", LoadList3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.Contains, "", "CCN", "3");
			results = Factory.Load<CFSLoadListConsol>(filter.Query);
			AssertCollectionContains("Expecting collection to contain LoadList1", LoadList1, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList2", LoadList2, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList3", LoadList3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "CA", "PCN", "");
			results = Factory.Load<CFSLoadListConsol>(filter.Query);
			AssertCollectionNotContains("Expecting collection not to contain LoadList1", LoadList1, results);
			AssertCollectionContains("Expecting collection to contain LoadList2", LoadList2, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList3", LoadList3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "CA", "PCN", "34");
			results = Factory.Load<CFSLoadListConsol>(filter.Query);
			AssertCollectionNotContains("Expecting collection not to contain LoadList1", LoadList1, results);
			AssertCollectionContains("Expecting collection to contain LoadList2", LoadList2, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList3", LoadList3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "CN", "SLD", "34");
			results = Factory.Load<CFSLoadListConsol>(filter.Query);
			AssertCollectionNotContains("Expecting collection not to contain LoadList1", LoadList1, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList2", LoadList2, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList3", LoadList3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.Contains, "CN", "SLD", "34");
			results = Factory.Load<CFSLoadListConsol>(filter.Query);
			AssertCollectionNotContains("Expecting collection not to contain LoadList1", LoadList1, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList2", LoadList2, results);
			AssertCollectionContains("Expecting collection to contain LoadList3", LoadList3, results);
		}

		void SetReferenceNumberFilter(ReferenceNumberFilter filter, SQLComparisonOperator op, string country, string type, string property)
		{
			filter.SqlComparisonOperator = op;
			filter.Property = property;
			filter.Country = country;
			filter.Type = type;
		}

		#endregion

		#region Billing Filters

		public void TestAPInvoiceNumberFilter()
		{
			LoadListConsolFilterBusinessObject filterBO = new LoadListConsolFilterBusinessObject();
			AssertNotNull(filterBO["AP Invoice #"]);

			CFSLoadListConsol loadList1 = Factory.New<CFSLoadListConsol>();
			CFSLoadListConsol loadList2 = Factory.New<CFSLoadListConsol>();

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = loadList1.PK;
			job.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.Parent = loadList1;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "00001001";

			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = "00001001";
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";

			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)filterBO["AP Invoice #"];
			filter.Property = "00001001";
			filter.IsActive = true;

			CFSLoadListConsolCollection loadLists = new CFSLoadListConsolCollection(Factory);
			loadLists.Load(filterBO.Filter);

			AssertEquals("Shipment1 is in Collection", true, loadLists.Contains(loadList1.PK));
			AssertEquals("Shipment2 is not in Collection", false, loadLists.Contains(loadList2.PK));

			filter.Property = "00001002";
			filter.IsActive = true;

			loadLists = new CFSLoadListConsolCollection(Factory);
			loadLists.Load(filterBO.Filter);

			AssertEquals("Shipment1 is not in Collection", false, loadLists.Contains(loadList1.PK));
			AssertEquals("Shipment2 is not in Collection", false, loadLists.Contains(loadList2.PK));

			filter.Property = "";
			filter.IsActive = true;

			loadLists = new CFSLoadListConsolCollection(Factory);
			loadLists.Load(filterBO.Filter);

			AssertEquals("Shipment1 is in Collection", true, loadLists.Contains(loadList1.PK));
			AssertEquals("Shipment2 is in Collection", true, loadLists.Contains(loadList2.PK));
		}

		public void TestARTransactionFilter()
		{
			LoadListConsolFilterBusinessObject filterBO = new LoadListConsolFilterBusinessObject();
			AssertNotNull(filterBO["AR Transaction #"]);

			CFSLoadListConsol loadList1 = Factory.New<CFSLoadListConsol>();
			CFSLoadListConsol loadList2 = Factory.New<CFSLoadListConsol>();

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = loadList1.PK;
			job.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.Parent = loadList1;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "00001005";

			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = "00001005";
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";

			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			ModuleFountainFilter filter = (ModuleFountainFilter)filterBO["AR Transaction #"];
			filter.Property = "00001005";
			filter.IsActive = true;

			CFSLoadListConsolCollection loadLists = new CFSLoadListConsolCollection(Factory);
			loadLists.Load(filterBO.Filter);

			AssertEquals("Shipment1 is in Collection", true, loadLists.Contains(loadList1.PK));
			AssertEquals("Shipment2 is not in Collection", false, loadLists.Contains(loadList2.PK));

			filter.Property = "00001001";
			filter.IsActive = true;

			loadLists = new CFSLoadListConsolCollection(Factory);
			loadLists.Load(filterBO.Filter);

			AssertEquals("Shipment1 is not in Collection", false, loadLists.Contains(loadList1.PK));
			AssertEquals("Shipment2 is not in Collection", false, loadLists.Contains(loadList2.PK));

			filter.Property = "";
			filter.IsActive = true;

			loadLists = new CFSLoadListConsolCollection(Factory);
			loadLists.Load(filterBO.Filter);

			AssertEquals("Shipment1 is in Collection", true, loadLists.Contains(loadList1.PK));
			AssertEquals("Shipment2 is in Collection", true, loadLists.Contains(loadList2.PK));
		}

		#endregion

		#region Date Filter Tests

		public void TestETAFilter()
		{
			VoyageDestination destination1 = Factory.NewWithValidTestData<VoyageDestination>();
			VoyageDestination destination2 = Factory.NewWithValidTestData<VoyageDestination>();

			destination1.JB_E_ARV = new ZDateTime(2000, 1, 1);
			destination2.JB_E_ARV = new ZDateTime(2000, 2, 2);

			JobSailing sailing1 = Factory.NewWithValidTestData<JobSailing>();
			JobSailing sailing2 = Factory.NewWithValidTestData<JobSailing>();

			sailing1.JX_JB = destination1.PK;
			sailing2.JX_JB = destination2.PK;

			Transport transport1 = Factory.NewWithValidTestData<Transport>();
			Transport transport2 = Factory.NewWithValidTestData<Transport>();

			LoadList1.Transports.Add(transport1);
			LoadList2.Transports.Add(transport2);

			transport1.JW_JX = sailing1.PK;
			transport1.JW_IsLinked = true;
			transport2.JW_JX = sailing2.PK;
			transport2.JW_IsLinked = true;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.ETA];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestETDFilter()
		{
			VoyageOrigin origin1 = Factory.NewWithValidTestData<VoyageOrigin>();
			VoyageOrigin origin2 = Factory.NewWithValidTestData<VoyageOrigin>();

			origin1.JA_E_DEP = new ZDateTime(2000, 1, 1);
			origin2.JA_E_DEP = new ZDateTime(2000, 2, 2);

			JobSailing sailing1 = Factory.NewWithValidTestData<JobSailing>();
			JobSailing sailing2 = Factory.NewWithValidTestData<JobSailing>();

			sailing1.JX_JA = origin1.PK;
			sailing2.JX_JA = origin2.PK;

			Transport transport1 = Factory.NewWithValidTestData<Transport>();
			Transport transport2 = Factory.NewWithValidTestData<Transport>();

			LoadList1.Transports.Add(transport1);
			LoadList2.Transports.Add(transport2);

			transport1.JW_JX = sailing1.PK;
			transport1.JW_IsLinked = true;
			transport2.JW_JX = sailing2.PK;
			transport2.JW_IsLinked = true;

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO[ConstantsAndReusables.DateFilterTypes.ETD];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		#endregion

		#region Organisation Filter Tests

		public void TestClientFilter()
		{
			OrgHeader client1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader client3 = Factory.NewWithValidTestData<OrgHeader>();

			LoadList1.JK_OH_Forwarder = client1.PK;
			LoadList2.JK_OH_Forwarder = client2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO[ConstantsAndReusables.OrgFilterTypes.Client];

			filter.Property = client1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.Property = client3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestConsignorFilter()
		{
			OrgHeader consignor1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignor3 = Factory.NewWithValidTestData<OrgHeader>();

			consignor1.OH_IsConsignor = ZBool.True;
			consignor2.OH_IsConsignor = ZBool.True;
			consignor3.OH_IsConsignor = ZBool.True;

			OrgAddress orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();

			orgAddress1.OA_OH = consignor1.PK;
			orgAddress2.OA_OH = consignor2.PK;

			orgAddress1.OA_Code = "1";
			orgAddress2.OA_Code = "2";

			JobDocAddress docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			JobDocAddress docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();

			docAddress1.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			docAddress2.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;

			docAddress1.E2_OA_Address = orgAddress1.PK;
			docAddress2.E2_OA_Address = orgAddress2.PK;

			docAddress1.E2_ParentID = Shipment1.PK;
			docAddress2.E2_ParentID = Shipment2.PK;

			LoadList1.Shipments.Add(Shipment1);
			LoadList2.Shipments.Add(Shipment2);

			Factory.Save();

			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterBO[String.Format("{0} / {1}", ConstantsAndReusables.OrgFilterTypes.Consignor, ConstantsAndReusables.OrgFilterTypes.Consignee)];

			filter.Property1 = consignor1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.Property1 = consignor3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestConsigneeFilter()
		{
			OrgHeader consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee3 = Factory.NewWithValidTestData<OrgHeader>();

			consignee1.OH_IsConsignee = ZBool.True;
			consignee2.OH_IsConsignee = ZBool.True;
			consignee3.OH_IsConsignee = ZBool.True;

			OrgAddress orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			OrgAddress orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_Code = "1";
			orgAddress2.OA_Code = "2";

			orgAddress1.OA_OH = consignee1.PK;
			orgAddress2.OA_OH = consignee2.PK;

			JobDocAddress docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			JobDocAddress docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();

			docAddress1.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			docAddress2.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;

			docAddress1.E2_OA_Address = orgAddress1.PK;
			docAddress2.E2_OA_Address = orgAddress2.PK;

			docAddress1.E2_ParentID = Shipment1.PK;
			docAddress2.E2_ParentID = Shipment2.PK;

			LoadList1.Shipments.Add(Shipment1);
			LoadList2.Shipments.Add(Shipment2);

			Factory.Save();

			ModuleGuidsFilter filter = (ModuleGuidsFilter)FilterBO[String.Format("{0} / {1}", ConstantsAndReusables.OrgFilterTypes.Consignor, ConstantsAndReusables.OrgFilterTypes.Consignee)];

			filter.Property2 = consignee1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.Property2 = consignee3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		#endregion

		#region Test Location Filters

		public void TestLoadDischargeFilter()
		{
			VoyageOrigin origin1 = Factory.NewWithValidTestData<VoyageOrigin>();
			VoyageOrigin origin2 = Factory.NewWithValidTestData<VoyageOrigin>();
			VoyageDestination destination1 = Factory.NewWithValidTestData<VoyageDestination>();
			VoyageDestination destination2 = Factory.NewWithValidTestData<VoyageDestination>();

			origin1.JA_RL_NKPortOfLoading = "KRSEL";
			origin2.JA_RL_NKPortOfLoading = "AUBNE";
			destination1.JB_RL_NKPortOfDischarge = "AUSYD";
			destination2.JB_RL_NKPortOfDischarge = "AUSYD";

			JobSailing sailing1 = Factory.NewWithValidTestData<JobSailing>();
			JobSailing sailing2 = Factory.NewWithValidTestData<JobSailing>();

			sailing1.JX_JA = origin1.PK;
			sailing2.JX_JA = origin2.PK;
			sailing1.JX_JB = destination1.PK;
			sailing2.JX_JB = destination2.PK;

			Transport transport1 = Factory.NewWithValidTestData<Transport>();
			Transport transport2 = Factory.NewWithValidTestData<Transport>();

			LoadList1.Transports.Add(transport1);
			LoadList2.Transports.Add(transport2);

			transport1.JW_JX = sailing1.PK;
			transport1.JW_IsLinked = true;
			transport2.JW_JX = sailing2.PK;
			transport2.JW_IsLinked = true;

			Factory.Save();

			ModuleLocationFilter filter = (ModuleLocationFilter)FilterBO[ConstantsAndReusables.PortFilterTypes.LoadDischarge];

			filter.Property1 = "KR";
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.Property1 = "AUBNE";
			filter.Property2 = "AUSYD";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.Property1 = ZString.Empty;
			filter.Property2 = "AUSYD";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.Property1 = "AUSYD";
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestOriginDestinationQuery()
		{
			Shipment1.JS_RL_NKOrigin = "KRSEL";
			Shipment2.JS_RL_NKOrigin = "AUBNE";

			Shipment1.JS_RL_NKDestination = "AUSYD";
			Shipment2.JS_RL_NKDestination = "AUSYD";

			LoadList1.Shipments.Add(Shipment1);
			LoadList2.Shipments.Add(Shipment2);

			Factory.Save();

			ModuleLocationFilter filter = (ModuleLocationFilter)FilterBO[ConstantsAndReusables.PortFilterTypes.OriginDestination];

			filter.Property1 = "KR";
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.Property1 = "AUBNE";
			filter.Property2 = "AUSYD";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.Property1 = ZString.Empty;
			filter.Property2 = "AUSYD";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.Property1 = "AUSYD";
			filter.Property2 = ZString.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		#endregion

		#region Vessel / Voyage Filter Tests

		public void TestVesselFilter()
		{
			var sailing1 = Factory.NewWithValidTestData<JobSailing>();
			var sailing2 = Factory.NewWithValidTestData<JobSailing>();

			var voyage1 = sailing1.Voyage;
			var voyage2 = sailing2.Voyage;

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "XAAAX";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "YAAAY";

			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage1.JV_VoyageFlight = "XAAAX";
			voyage2.JV_VoyageFlight = "YAAAY";

			var transport1 = Factory.NewWithValidTestData<Transport>();
			var transport2 = Factory.NewWithValidTestData<Transport>();

			LoadList1.Transports.Add(transport1);
			LoadList2.Transports.Add(transport2);

			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing1.PK;
			transport2.JW_IsLinked = true;
			transport2.JW_JX = sailing2.PK;

			Factory.Save();

			var filter = (VoyageVesselModuleFilter)FilterBO["Voyage / Flight / Vessel"];

			filter.Vessel = "XAAAX";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.Vessel = "ZZZ";

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.Vessel = "YAA";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.Vessel = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));
		}

		public void TestVoyageFilter()
		{
			var sailing1 = Factory.NewWithValidTestData<JobSailing>();
			var sailing2 = Factory.NewWithValidTestData<JobSailing>();

			var voyage1 = sailing1.Voyage;
			var voyage2 = sailing2.Voyage;

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "XAAAX";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "YAAAY";

			voyage1.JV_RV_NKVessel = vessel1.RV_FK;
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage1.JV_VoyageFlight = "XAAAX";
			voyage2.JV_VoyageFlight = "YAAAY";

			var transport1 = Factory.NewWithValidTestData<Transport>();
			var transport2 = Factory.NewWithValidTestData<Transport>();

			LoadList1.Transports.Add(transport1);
			LoadList2.Transports.Add(transport2);

			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing1.PK;
			transport2.JW_IsLinked = true;
			transport2.JW_JX = sailing2.PK;

			Factory.Save();

			var filter = (VoyageVesselModuleFilter)FilterBO["Voyage / Flight / Vessel"];

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.VoyageFlightNo = "XAA";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.VoyageFlightNo = "YAAAY";

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.VoyageFlightNo = "AAA";

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.VoyageFlightNo = "ZZZ";

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.VoyageFlightNo = "ZZZ";

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", FilterCollection.Contains(LoadList2));
		}

		#endregion

		#region Type / Mode / Status Filter Tests

		public void TestTransportModeFilter()
		{
			LoadList1.JK_TransportMode = Constants.TransportModes.Air;
			LoadList2.JK_TransportMode = Constants.TransportModes.Sea;

			Factory.Save();

			LoadListModeFilter filter = (LoadListModeFilter)FilterBO["Transport / Container Modes"];

			filter.Property1 = Constants.TransportModes.Air;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.Property1 = Constants.TransportModes.Sea;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.Property1 = Constants.TransportModes.Storage;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestContainerModeFilter()
		{
			LoadList1.JK_ConsolMode = Constants.ContainerModes.LCL;
			LoadList2.JK_ConsolMode = Constants.ContainerModes.FCL;

			Factory.Save();

			LoadListModeFilter filter = (LoadListModeFilter)FilterBO["Transport / Container Modes"];

			filter.Property2 = Constants.ContainerModes.LCL;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.Property2 = Constants.ContainerModes.FCL;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.Property2 = Constants.ContainerModes.Unaccompanied;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		public void TestJobStatusFilter()
		{
			JobHeader job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobHeader job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobHeader job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			job1.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			job2.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			job3.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;

			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			job3.JH_GC = GlbCompany.CurrentCompany.PK;

			job1.JH_Status = JobHeaderStatus.Working.Code;
			job2.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			job3.JH_Status = JobHeaderStatus.ScheduledForArchive.Code;

			job1.JH_ParentID = LoadList1.PK;
			job2.JH_ParentID = LoadList2.PK;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Job Status"];

			filter.Property = JobHeaderStatus.Working.Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain LoadList1", FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));

			filter.Property = JobHeaderStatus.WorkOnHold.Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection to contain LoadList2", FilterCollection.Contains(LoadList2));

			filter.Property = JobHeaderStatus.ScheduledForArchive.Code;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain LoadList1", !FilterCollection.Contains(LoadList1));
			Assert("Expecting collection not to contain LoadList2", !FilterCollection.Contains(LoadList2));
		}

		#endregion

		#region TestFreightConsolCanBeSharedWithCFS

		public void TestFreightConsolCanBeSharedWithCFS()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			factory2.RefreshEnabled = false; // DO NOT UPDATE THE ZENVIRONMENT FACTORY BY DATA REFRESH!

			OrgHeader org1 = factory2.New<OrgHeader>();
			org1.OH_IsForwarder = true;
			org1.OH_Code = "EDI_SYD";
			org1.OH_FullName = "Company";
			org1.OH_RL_NKClosestPort = "AUBNE";
			org1.MainAddress.OA_Address1 = "SYDNEY";
			factory2.Save();

			GlbBranch branch1 = factory2.Load<GlbBranch>(GlbCompany.CurrentCompany.Branches[0].PK);
			branch1.GB_OH_OrgProxy = org1.PK;

			factory2.Save();

			CommonConsol forwardingConsol = factory2.New<CommonConsol>();
			forwardingConsol.JK_IsForwarding = true;
			forwardingConsol.JK_IsCFS = false;
			forwardingConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			Transport transport1 = forwardingConsol.Transports[0];
			transport1.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			transport1.JW_IsLinked = true;
			transport1.JW_JX = CreateNewSailing(false, factory2).PK;
			forwardingConsol.JK_RL_NKLoadPort = transport1.JW_RL_NKLoadPort;
			forwardingConsol.JK_RL_NKDischargePort = transport1.JW_RL_NKDiscPort;

			CFSLoadListConsol cFSConsol = factory2.New<CFSLoadListConsol>();
			cFSConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Transport transport2 = cFSConsol.Transports[0];
			transport2.JW_JX = CreateNewSailing(true, factory2).PK;
			cFSConsol.JK_RL_NKLoadPort = transport2.JW_RL_NKLoadPort;
			cFSConsol.JK_RL_NKDischargePort = transport2.JW_RL_NKDiscPort;

			CommonConsol forwardingAndCFSConsol = factory2.New<CommonConsol>();
			forwardingAndCFSConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			Transport transport3 = forwardingAndCFSConsol.Transports[0];
			transport3.JW_JX = CreateNewSailing(false, factory2).PK;
			forwardingAndCFSConsol.JK_RL_NKLoadPort = transport3.JW_RL_NKLoadPort;
			forwardingAndCFSConsol.JK_RL_NKDischargePort = transport3.JW_RL_NKDiscPort;
			forwardingAndCFSConsol.JK_OA_PackDepotAddress = org1.MainAddress.PK;

			factory2.Save();

			Assert("Expecting ForwardingConsol to be Forwarding", forwardingConsol.JK_IsForwarding);
			Assert("Not expecting CFSConsol to be Forwarding", !cFSConsol.JK_IsForwarding);
			Assert("Expecting ForwardingAndCFSConsol to be Forwarding", forwardingAndCFSConsol.JK_IsForwarding);

			Assert("Not expecting ForwardingConsol to be CFS", !forwardingConsol.JK_IsCFS);
			Assert("Expecting CFSConsol to be CFS", cFSConsol.JK_IsCFS);
			Assert("Expecting ForwardingAndCFSConsol to be CFS", forwardingAndCFSConsol.JK_IsCFS);

			FilterBO.ResetToDefaultValues();
			CFSLoadListConsolCollection results = new CFSLoadListConsolCollection(factory2);
			results.Load(FilterBO.Filter);
			AssertEquals("Should not include Forwarding Consol.", false, results.Contains(forwardingConsol.PK));
			AssertEquals("CFS Consol should be included.", true, results.Contains(cFSConsol.PK));
			AssertEquals("CFS and Forwarding Consol should be included.", true, results.Contains(forwardingAndCFSConsol.PK));
		}

		JobSailing CreateNewSailing(bool import, BusinessObjectFactory newFactory)
		{
			ZString domesticPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString foreignPort;

			if (GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode == Constants.CountryCodes.Singapore)
			{
				foreignPort = "AUBNE";
			}
			else
			{
				foreignPort = "SGSIN";
			}

			if (import)
			{
				return CreateNewSailing(foreignPort, domesticPort, newFactory);
			}
			else
			{
				return CreateNewSailing(domesticPort, foreignPort, newFactory);
			}
		}

		JobSailing CreateNewSailing(ZString portOfLoading, ZString portOfDischarge, BusinessObjectFactory newFactory)
		{
			JobVoyage voyage = newFactory.New<JobVoyage>();
			RefVessel vessel = newFactory.LoadTop1<RefVessel>(new ZQuery());
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123" + new Random().Next(0, 9);

			VoyageOrigin origin = newFactory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = portOfLoading;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(-10);
			voyage.Origins.Add(origin);

			VoyageDestination destination = newFactory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = portOfDischarge;
			destination.JB_E_ARV = ZDateTime.Today;
			voyage.Destinations.Add(destination);

			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		#endregion

		public void TestProfitLossReasonFilterWithOperators()
		{
			var job1 = new JobHeader.Loader(Shipment1).TryLoadOrCreateWithoutMutexForTestOnly();
			job1.JH_ParentID = LoadList1.PK;
			job1.JH_ProfitLossReasonCode = "ND1";

			var job2 = new JobHeader.Loader(Shipment2).TryLoadOrCreateWithoutMutexForTestOnly();
			job2.JH_ParentID = LoadList2.PK;
			job2.JH_ProfitLossReasonCode = "CD1";

			Factory.Save();

			var profitLossReasonFilter = (ModuleTextFilter)FilterBO["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { LoadList1 }, FilterCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { LoadList1 }, FilterCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { LoadList1, LoadList2 }, FilterCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { LoadList2 }, FilterCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { LoadList2 }, FilterCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { LoadList2 }, FilterCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "ND1";

			FilterCollection.Load(FilterBO.Filter);
			AssertEquals(0, FilterCollection.Count);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			FilterCollection.Load(FilterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { LoadList1, LoadList2 }, FilterCollection);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new LoadListConsolFilterBusinessObject();
		}

		CFSShipment Shipment1;
		CFSShipment Shipment2;

		CFSContainer Container1;
		CFSContainer Container2;

		CFSLoadListConsol LoadList1;
		CFSLoadListConsol LoadList2;
		CFSLoadListConsol LoadList3;

		CFSLoadListConsolCollection FilterCollection;
		LoadListConsolFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			Shipment1 = Factory.NewWithValidTestData<CFSShipment>();
			Shipment2 = Factory.NewWithValidTestData<CFSShipment>();

			Container1 = Factory.NewWithValidTestData<CFSContainer>();
			Container2 = Factory.NewWithValidTestData<CFSContainer>();

			LoadList1 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			LoadList2 = Factory.NewWithValidTestData<CFSLoadListConsol>();
			LoadList3 = Factory.NewWithValidTestData<CFSLoadListConsol>();

			FilterCollection = new CFSLoadListConsolCollection(Factory);
			FilterBO = (LoadListConsolFilterBusinessObject)GetNewFilterStripBusinessObject();
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
