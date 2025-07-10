using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	#region Abstract Class: DocketFilterBusinessObjectTest

	public abstract class DocketFilterBusinessObjectTest<TDocketFilter, TDocket> : FilterStripBusinessObjectTestCase
		where TDocketFilter : DocketFilterBusinessObject, new()
		where TDocket : WhsDocket
	{
		// ************************************************************************************************** //
		//                                                                                                    //
		//   when adding new tests, use the Asserter which superceeds the DocketAssert and related methods.   //
		//                                                                                                    //
		// ************************************************************************************************** //

		#region Filter Clear

		public virtual void TestFilterClear()
		{
			SetupTestData();
			SetupTestLineData();
			Factory.Save(); // DBOnlyQuery used
			DocketAssert(true, true, true, true);
		}

		#endregion

		#region Filter Warehouse

		public virtual void TestFilterWarehouse()
		{
			SetupTestData();
			Factory.Save(); // DBOnlyQuery used

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Warehouse", Whs1.PK);
			DocketAssert(true, false, true, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Warehouse", Whs2.PK);
			DocketAssert(false, true, false, true);
		}

		public virtual void TestFilterWarehouseVisibility()
		{
			AssertEquals("Precondition: WhsAllowedWarehouses", true, Env.Security.WhsAllowedWarehouses.IsAllowed);
			ModuleGuidFilter filter = (ModuleGuidFilter)DocketFilter["Warehouse"];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);

			Env.Security.WhsAllowedWarehouses.IsAllowed = false;
			try
			{
				TDocketFilter docketFilter1 = new TDocketFilter();
				ModuleGuidFilter filter1 = (ModuleGuidFilter)docketFilter1["Warehouse"];
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter1.Visibility);

				Globals.IsWeb = true;
				try
				{
					TDocketFilter docketFilter2 = new TDocketFilter();
					ModuleGuidFilter filter2 = (ModuleGuidFilter)docketFilter2["Warehouse"];
					AssertEquals("Visibility", FilterVisibility.Visible, filter2.Visibility);
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
			finally
			{
				Env.Security.WhsAllowedWarehouses.IsAllowed = true;
			}
		}

		#endregion

		#region Filter Client

		public virtual void TestFilterClient()
		{
			SetupTestData();
			Factory.Save(); // DBOnlyQuery used

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Client", Org1.PK);
			DocketAssert(true, true, false, false);
			AssertNoWarning(((ModuleGuidFilter)FilterStripBizO["Client"]).PropertyInfo, "Organization is in-active.");

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Client", Org2.PK);
			DocketAssert(false, false, true, true);
			AssertNoWarning(((ModuleGuidFilter)FilterStripBizO["Client"]).PropertyInfo, "Organization is in-active.");

			var originalActiveStatus1 = Org1.OH_IsActive;
			var originalActiveStatus2 = Org2.OH_IsActive;

			Org1.OH_IsActive = false;
			Org2.OH_IsActive = false;
			Factory.Save(); // DBOnlyQuery used

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Client", Org1.PK);
			DocketAssert(true, true, false, false);
			AssertHasWarning(((ModuleGuidFilter)DocketFilter["Client"]).PropertyInfo, "Organization is in-active.");

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Client", Org2.PK);
			DocketAssert(false, false, true, true);
			AssertHasWarning(((ModuleGuidFilter)DocketFilter["Client"]).PropertyInfo, "Organization is in-active.");

			Org1.OH_IsActive = originalActiveStatus1;
			Org2.OH_IsActive = originalActiveStatus2;
		}

		public virtual void TestFilterClientVisibility()
		{
			AssertEquals("Precondition: WhsAllowedClients", true, Env.Security.WhsAllowedClients.IsAllowed);
			ModuleGuidFilter filter = (ModuleGuidFilter)DocketFilter["Client"];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);

			Env.Security.WhsAllowedClients.IsAllowed = false;
			try
			{
				TDocketFilter docketFilter1 = new TDocketFilter();
				ModuleGuidFilter filter1 = (ModuleGuidFilter)docketFilter1["Client"];
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter1.Visibility);

				Globals.IsWeb = true;
				try
				{
					TDocketFilter docketFilter2 = new TDocketFilter();
					ModuleGuidFilter filter2 = (ModuleGuidFilter)docketFilter2["Client"];
					AssertEquals("Visibility", FilterVisibility.Visible, filter2.Visibility);
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
			finally
			{
				Env.Security.WhsAllowedClients.IsAllowed = true;
			}
		}

		public void TestFilterClient_HasComparisonOperator()
		{
			var filter = (ModuleGuidFilter)DocketFilter["Client"];
			Assert("Filter has comparison operator.", filter.HasComparisonOperator);

			AssertEquals(3, filter.AllowedComparisonOperators.Count);
			AssertCollectionContains("Default comparison operator.", string.Empty, filter.AllowedComparisonOperators);
			AssertCollectionContains(ModuleTextBaseFilter.ComparisonConstants.Exact, filter.AllowedComparisonOperators);
			AssertCollectionContains(ModuleTextBaseFilter.ComparisonConstants.NotEqual, filter.AllowedComparisonOperators);
		}

		public void TestFilterClient_WithComparisonOperator_Equals()
		{
			TestFilterClient_WithComparisonOperator_EqualsCore();
		}

		protected virtual void TestFilterClient_WithComparisonOperator_EqualsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var org1 = Helper.CreateClient("Client1");
			var org2 = Helper.CreateClient("Client2");
			var org3 = Helper.CreateClient("Client3");
			Factory.Save();

			var docket1 = CreateDocket(org1, data.Whs1, "R1");
			var docket2 = CreateDocket(org2, data.Whs1, "R2");
			var docket3 = CreateDocket(org3, data.Whs1, "R3");
			var docketCollection = GetDocketCollection();
			Factory.Save();

			var filter = (ModuleGuidFilter)DocketFilter["Client"];
			Assert("Filter has comparison operator.", filter.HasComparisonOperator);
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = org1.PK;

			docketCollection.AdditionalFilter = DocketFilter.Filter;
			AssertContainsExactElementsInAnyOrder("Only docket1 is returned.", new[] { docket1 }, docketCollection);
		}

		public void TestFilterClient_WithComparisonOperator_NotEquals()
		{
			TestFilterClient_WithComparisonOperator_NotEqualsCore();
		}

		protected virtual void TestFilterClient_WithComparisonOperator_NotEqualsCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var org1 = Helper.CreateClient("Client1");
			var org2 = Helper.CreateClient("Client2");
			var org3 = Helper.CreateClient("Client3");
			Factory.Save();

			var docket1 = CreateDocket(org1, data.Whs1, "R1");
			var docket2 = CreateDocket(org2, data.Whs1, "R2");
			var docket3 = CreateDocket(org3, data.Whs1, "R3");
			var docketCollection = GetDocketCollection();
			Factory.Save();

			var filter = (ModuleGuidFilter)DocketFilter["Client"];
			Assert("Filter has comparison operator.", filter.HasComparisonOperator);
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = org1.PK;

			docketCollection.AdditionalFilter = DocketFilter.Filter;
			AssertContainsExactElementsInAnyOrder("All dockets except docket1 are returned.", new[] { docket2, docket3 }, docketCollection);
		}

		#endregion

		#region Filter Product

		public virtual void TestFilterProduct()
		{
			SetupTestData();
			SetupTestLineData();
			Factory.Save(); // DBOnlyQuery used

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Product", Part11.PK);
			DocketAssert(true, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Product", Part21.PK);
			DocketAssert(false, false, true, true);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Product", Part.PK);
			DocketAssert(false, true, true, false);
		}

		#endregion

		#region Filter Product Category

		public virtual void TestFilterProductCategory()
		{
			var warehouse = Helper.CreateWarehouse("Warehouse", "A");
			var client = Helper.CreateClient("Client");

			// product categories and products
			var categoryBeverage = Helper.CreateProductCategory("BEV", "Beverages");
			var categorySoftdrink = helper.CreateProductCategory("SOFTDRK", "Soft Drinks", categoryBeverage);
			var categoryBeer = helper.CreateProductCategory("BEER", "All Beers", categoryBeverage);
			var categoryDarkBeer = helper.CreateProductCategory("DARKBEER", "Dark Beers", categoryBeer);

			var productCoke = Helper.CreateProduct(client, "Coke");
			var relationCoke = productCoke.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationCoke.OU_OPC_Category = categorySoftdrink.PK;

			var productTea = Helper.CreateProduct(client, "Tea");
			var relationTea = productTea.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationTea.OU_OPC_Category = categorySoftdrink.PK;

			var productVB = Helper.CreateProduct(client, "VB");
			var relationVB = productVB.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationVB.OU_OPC_Category = categoryBeer.PK;

			var productGuinness = Helper.CreateProduct(client, "Guinness");
			var relationGuinness = productGuinness.RelatedOrganisations.FindByOrganisationAndRelationship(client, OrgPartRelation.RelationshipTypes.Owner);
			relationGuinness.OU_OPC_Category = categoryDarkBeer.PK;
			Factory.Save();

			// dockets
			var docketCoke = CreateDocket(client, warehouse, "docketCoke");
			var docketCokeLine = CreateDocketLine(docketCoke, productCoke, 10m);

			var docketTea = CreateDocket(client, warehouse, "docketTea");
			var docketTeaLine = CreateDocketLine(docketTea, productTea, 10m);

			var docketVB = CreateDocket(client, warehouse, "docketVB");
			var docketVBLine = CreateDocketLine(docketVB, productVB, 10m);

			var docketGuinness = CreateDocket(client, warehouse, "docketGuinness");
			var docketGuinnessLine = CreateDocketLine(docketGuinness, productGuinness, 10m);

			DocketCollection = GetDocketCollection();
			Factory.Save();

			// filter result
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Product Category", categorySoftdrink.PK);
			DocketCollection.AdditionalFilter = DocketFilter.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { docketCoke, docketTea }, DocketCollection);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Product Category", categoryBeer.PK);
			DocketCollection.AdditionalFilter = DocketFilter.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { docketVB, docketGuinness }, DocketCollection);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Product Category", categoryDarkBeer.PK);
			DocketCollection.AdditionalFilter = DocketFilter.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { docketGuinness }, DocketCollection);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Product Category", categoryBeverage.PK);
			DocketCollection.AdditionalFilter = DocketFilter.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { docketCoke, docketTea, docketVB, docketGuinness }, DocketCollection);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Product Category", ZGuid.Empty);
			DocketCollection.AdditionalFilter = DocketFilter.Filter;
			AssertContainsExactElementsInAnyOrder(new[] { docketCoke, docketTea, docketVB, docketGuinness }, DocketCollection);
		}

		#endregion

		#region Filter Commodity Code

		public virtual void TestFilterCommodityCode()
		{
			SetupTestData();
			SetupTestLineData();
			SetupCommodityCodes();
			Factory.Save(); // DBOnlyQuery used

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Commodity", (ZString)"11");
			DocketAssert(true, true, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Commodity", (ZString)"21");
			DocketAssert(false, false, true, true);
		}

		#endregion

		#region Filter DocketID

		public virtual void TestFilterDocketID()
		{
			SetupTestData();
			Factory.Save(); // need DocketIDs generated

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Docket ID", (ZString)"W00000001");
			DocketAssert(true, false, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Docket ID", (ZString)"W00000002");
			DocketAssert(false, true, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Docket ID", (ZString)"W00000003");
			DocketAssert(false, false, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Docket ID", (ZString)"W00000004");
			DocketAssert(false, false, false, true);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Docket ID", (ZString)"W00000005");
			DocketAssert(false, false, false, false);

			// other formats
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Docket ID", (ZString)"1");
			DocketAssert(true, false, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Docket ID", (ZString)"002");
			DocketAssert(false, true, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Docket ID", (ZString)"00003");
			DocketAssert(false, false, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Docket ID", (ZString)"00000004");
			DocketAssert(false, false, false, true);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Docket ID", (ZString)"5");
			DocketAssert(false, false, false, false);

			Docket22.WD_OH_Client = Org1.PK;
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Client", Org2.PK);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Docket ID", (ZString)"4");
			DocketAssert(false, false, false, true);
		}

		#endregion

		#region Filter Attributes

		public void TestLoggedInOrgIsBeingPassedWhileGettingAttributes()
		{
			DocketFilter = null;
			Enterprise.ZArchitecture.Environment.Globals.IsWeb = true;
			try
			{
				OrgHeader testLoggedInOrg = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
				DocketFilter.LoggedInWebUsersOrg = testLoggedInOrg;

				foreach (ModuleFilter filter in DocketFilter)
				{
					AssertNotEquals("Any attribute filter should not exist", FilterCategories.AttributeSearch, filter.Category);
				}

				var loCA2 = testLoggedInOrg.CustomLabels.AddNew();
				loCA2.OT_FieldName = Constants.CustomLabels.WhsDocket.CustomAttribute2;
				loCA2.OT_Caption = "LoggedInOrgsCA2";

				Factory.Save();
				DocketFilter = null;
				DocketFilter.LoggedInWebUsersOrg = testLoggedInOrg;

				if (SupportsCustomAttribFilters)
				{
					AssertNotNull("LoggedInOrg`s filter for CustomAttribute2 is here", DocketFilter["LoggedInOrgsCA2 - CA"]);
					AssertEquals("Filter category", FilterCategories.AttributeSearch, DocketFilter["LoggedInOrgsCA2 - CA"].Category);
				}
				else
				{
					AssertNull(DocketFilter["LoggedInOrgsCA2 - CA"]);
				}
			}
			finally
			{
				Enterprise.ZArchitecture.Environment.Globals.IsWeb = false;
			}
		}

		public void TestFilterHeaderAttributes()
		{
			if (SupportsCustomAttribFilters)
			{
				SetupTestData();

				Docket11.WD_CustomAttrib1 = "CA111";
				Docket12.WD_CustomAttrib1 = "CA112";
				Docket21.WD_CustomAttrib1 = "CA121";
				Docket22.WD_CustomAttrib1 = "CA122";

				Docket11.WD_CustomAttrib5 = "CA511";
				Docket12.WD_CustomAttrib5 = "CA512";
				Docket21.WD_CustomAttrib5 = "CA521";
				Docket22.WD_CustomAttrib5 = "CA522";

				Factory.Save();

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "WhsDocket.CustomAttrib1", (ZString)"CA111");
				DocketAssert(true, false, false, false);

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "WhsDocket.CustomAttrib1", (ZString)"CA11");
				DocketAssert(true, true, false, false);

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "WhsDocket.CustomAttrib1", (ZString)"CA12");
				DocketAssert(false, false, true, true);

				DocketFilter = null;
				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "WhsDocket.CustomAttrib5", (ZString)"CA511");
				DocketAssert(true, false, false, false);

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "WhsDocket.CustomAttrib5", (ZString)"CA51");
				DocketAssert(true, true, false, false);

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "WhsDocket.CustomAttrib5", (ZString)"CA52");
				DocketAssert(false, false, true, true);

				AssertEquals(FilterCategories.AttributeSearch, DocketFilter["WhsDocket.CustomAttrib3"].Category);
			}
			else
			{
				AssertNull(DocketFilter["WhsDocket.CustomAttrib1"]);
				AssertNull(DocketFilter["WhsDocket.CustomAttrib2"]);
				AssertNull(DocketFilter["WhsDocket.CustomAttrib3"]);
				AssertNull(DocketFilter["WhsDocket.CustomAttrib4"]);
				AssertNull(DocketFilter["WhsDocket.CustomAttrib5"]);
			}
		}

		protected virtual bool SupportsCustomAttribFilters => true;

		public virtual void TestFilterLineAttributes()
		{
			SetupTestData();
			SetupTestLineData();
			SetAttributesForTestFilterLine();
			Factory.Save(); // DBOnlyQuery used

			// attribute 1
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 1", (ZString)"PA1");
			DocketAssert(true, true, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 1", (ZString)"PA12");
			DocketAssert(true, false, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 1", (ZString)"PA15");
			DocketAssert(false, false, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 1");

			// attribute 2
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 2", (ZString)"PA2");
			DocketAssert(true, true, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 2", (ZString)"PA21");
			DocketAssert(true, false, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 2", (ZString)"PA24");
			DocketAssert(false, true, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 2");

			// attribute 3
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 3", (ZString)"PA3");
			DocketAssert(true, true, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 3", (ZString)"PA33");
			DocketAssert(false, true, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 3", (ZString)"PA36");
			DocketAssert(false, false, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Part Attribute 3");

			// any
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Text Attribute", (ZString)"PA");
			DocketAssert(true, true, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Text Attribute", (ZString)"PA2");
			DocketAssert(true, true, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Text Attribute", (ZString)"PA3");
			DocketAssert(true, true, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Text Attribute", (ZString)"PA26");
			DocketAssert(false, false, true, false);

			AssertEquals(FilterCategories.AttributeSearch, DocketFilter["Part Attribute 3"].Category);
		}

		public void TestSerialNumberFilter()
		{
			TestSerialNumberFilterCore();
		}

		protected virtual void TestSerialNumberFilterCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket1 = (TDocket)CreateDocket(data.Org1, data.Whs1, "1");
			var docket2 = (TDocket)CreateDocket(data.Org1, data.Whs1, "2");
			Factory.Save();

			var docket1Line = CreateDocketLine(docket1, data.Part1, 1);
			var docket2Line = CreateDocketLine(docket2, data.Part1, 1);

			docket1Line.WE_SerialNumber = "SN11";
			docket2Line.WE_SerialNumber = "SN12";
			Factory.Save(); // DBOnlyQuery used

			Asserter.AddToScope(docket1);
			Asserter.AddToScope(docket2);
			var filter = (ModuleTextFilter)FilterStripBizO["Serial Number"];

			// equal
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "SN11";
			Asserter.AssertMatches("Equals 'SN11' should return only docket1.", filter, docket1);
			filter.Property = "SN12";
			Asserter.AssertMatches("Equals 'SN12' should return only docket2.", filter, docket2);
			filter.Property = "SN1";
			Asserter.AssertMatches("Equals 'SN1' should return no dockets.", filter);

			// starts with
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "SN11";
			Asserter.AssertMatches("Starts with 'SN11' should return only docket1.", filter, docket1);
			filter.Property = "SN12";
			Asserter.AssertMatches("Starts with 'SN12' should return only docket2.", filter, docket2);
			filter.Property = "SN1";
			Asserter.AssertMatches("Starts with 'SN1' should return all dockets.", filter, docket1, docket2);
			filter.Property = "SN2";
			Asserter.AssertMatches("Starts with 'SN2' should return no dockets.", filter);

			// contains
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "SN11";
			Asserter.AssertMatches("Contains 'SN11' should return only docket1.", filter, docket1);
			filter.Property = "SN12";
			Asserter.AssertMatches("Contains 'SN12' should return only docket2.", filter, docket2);
			filter.Property = "SN1";
			Asserter.AssertMatches("Contains 'SN1' should return all dockets.", filter, docket1, docket2);
			filter.Property = "SN2";
			Asserter.AssertMatches("Contains 'SN2' should return no dockets.", filter);

			// not contains
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "SN11";
			Asserter.AssertMatches("Not contains 'SN11' should return only docket2.", filter, docket2);
			filter.Property = "SN12";
			Asserter.AssertMatches("Not contains 'SN12' should return only docket1 and docket2.", filter, docket1);
			filter.Property = "SN1";
			Asserter.AssertMatches("Not contains 'SN1' should return no dockets.", filter);
			filter.Property = "SN2";
			Asserter.AssertMatches("Not contains 'SN2' should return all dockets.", filter, docket1, docket2);
		}

		protected virtual void SetAttributesForTestFilterLine()
		{
			// attribute 1
			Line111.WE_PartAttrib1 = "PA11";
			Line112.WE_PartAttrib1 = "PA12";
			Line121.WE_PartAttrib1 = "PA13";
			Line122.WE_PartAttrib1 = "PA14";
			Line211.WE_PartAttrib1 = "PA15";
			Line212.WE_PartAttrib1 = "PA16";

			// attribute 2
			Line111.WE_PartAttrib2 = "PA21";
			Line112.WE_PartAttrib2 = "PA22";
			Line121.WE_PartAttrib2 = "PA23";
			Line122.WE_PartAttrib2 = "PA24";
			Line211.WE_PartAttrib2 = "PA25";
			Line212.WE_PartAttrib2 = "PA26";

			// attribute 3
			Line111.WE_PartAttrib3 = "PA31";
			Line112.WE_PartAttrib3 = "PA32";
			Line121.WE_PartAttrib3 = "PA33";
			Line122.WE_PartAttrib3 = "PA34";
			Line211.WE_PartAttrib3 = "PA35";
			Line212.WE_PartAttrib3 = "PA36";
		}

		public virtual void TestFilterEntryKey()
		{
			SetupTestData();
			SetupTestLineData();
			SetCustomsDataForTestFilterEntryKey();
			Factory.Save(); // DBOnlyQuery used

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Customs Entry Key", (ZString)"BE1");
			DocketAssert(true, true, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Customs Entry Key", (ZString)"BE12");
			DocketAssert(true, false, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Customs Entry Key", (ZString)"BE15");
			DocketAssert(false, false, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Customs Entry Key");
		}

		protected virtual void SetCustomsDataForTestFilterEntryKey()
		{
			// attribute 1
			Line111.WE_BondedEntryKey = "BE11";
			Line112.WE_BondedEntryKey = "BE12";
			Line121.WE_BondedEntryKey = "BE13";
			Line122.WE_BondedEntryKey = "BE14";
			Line211.WE_BondedEntryKey = "BE15";
			Line212.WE_BondedEntryKey = "BE16";
		}

		#endregion

		#region Filter Docket Status

		public void TestDocketStatusFilter_Description()
		{
			if (RunTestDocketStatusFilter_Description)
			{
				var docketStatusFilter = DocketFilter[GetStatusFilterCaption()];
				AssertNotNull("Docket Status Filter should not be null.", docketStatusFilter);
				AssertEquals("Description:", GetStatusFilterCaption(), docketStatusFilter.Description);
				AssertEquals("MultilingualDescription:", GetStatusFilterMultilingualDescription(), docketStatusFilter.MultilingualDescription);
			}
			else
			{
				Assert("Not required test", true);
			}
		}

		public void TestFilterDocketStatus()
		{
			if (RunTestFilterDocketStatus)
			{
				AssertContainsExactElementsInAnyOrder(GetExpectedDocketStatus(), DocketFilter.DocketStatuses); 
			}
			else
			{
				Assert("Not required test", true);
			}
		}

		public void TestFilterDocketStatus_Query()
		{
			if (SupportsDocketStatus)
			{
				SetupTestData();

				Docket12.WD_DocketStatus = DocketStatus.Codes.Cancelled;
				Docket21.WD_DocketStatus = DocketStatus.Codes.Error;
				Docket22.WD_DocketStatus = DocketStatus.Codes.Held;

				Factory.Save();

				var filter = new TDocketFilter();
				var statusFilter = (ModuleTextFilter)filter[GetStatusFilterCaption()];
				DocketCollection.AdditionalFilter = statusFilter.Query;
				AssertContainsExactElementsInAnyOrder("Pre-condition: no status filter property, should get every docket include finalised and cancelled docket.", new[] { Docket11, Docket12, Docket21, Docket22 }, DocketCollection);

				statusFilter.Property = DocketStatus.Codes.Cancelled;
				DocketCollection.AdditionalFilter = statusFilter.Query;
				AssertContainsExactElementsInAnyOrder("status filter property is Un-Finalised, should not include finalised and cancelled docket", new[] { Docket12 }, DocketCollection);

				statusFilter.Property = DocketStatus.Codes.Error;
				DocketCollection.AdditionalFilter = statusFilter.Query;
				AssertContainsExactElementsInAnyOrder("status filter property is Un-Finalised, should not include finalised and cancelled docket", new[] { Docket21 }, DocketCollection);

				statusFilter.Property = DocketStatus.Codes.Held;
				DocketCollection.AdditionalFilter = statusFilter.Query;
				AssertContainsExactElementsInAnyOrder("status filter property is Un-Finalised, should not include finalised and cancelled docket", new[] { Docket22 }, DocketCollection);
			}
			else
			{
				Assert("Not required test", true);
			}
		}

		public void TestFilterDocketStatusEntered()
		{
			if (SupportsFilterForEnteredStatus)
			{
				SetupTestData();
				Docket11.WD_DocketStatus = DocketStatus.Codes.Finalised;
				Docket12.WD_DocketStatus = DocketStatus.Codes.Entered;
				Docket21.WD_DocketStatus = DocketStatus.Codes.Error;
				Docket22.WD_DocketStatus = DocketStatus.Codes.Entered;

				var availableStatuses = DocketFilter.DocketStatuses;
				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, GetStatusFilterCaption(), (ZString)DocketStatus.Codes.Entered);
				foreach (CodeDescriptionPair status in availableStatuses)
				{
					Docket11.WD_DocketStatus = DocketStatus.Codes.Entered;
					Docket11.WD_DocketStatus = status.Code;
					DocketAssert((status.Code == DocketStatus.Codes.Entered || (status.Code == DocketStatus.Codes.Error && SupportsFilterForErrorStatus)), true, SupportsFilterForErrorStatus, true);
				}
				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, GetStatusFilterCaption());
			}
			else
			{
				Assert("Not required test", true);
			}
		}

		protected virtual bool RunTestFilterDocketStatus => true;

		protected virtual bool RunTestDocketStatusFilter_Description => true;

		protected virtual bool SupportsDocketStatus => true;

		protected virtual bool SupportsFilterForEnteredStatus => true;

		protected virtual bool SupportsFilterForErrorStatus => true;

		protected virtual CodeDescriptionPairList GetExpectedDocketStatus()
			=> new DocketStatus();

		protected virtual ZString GetStatusFilterCaption() => "Status";

		protected virtual MultilingualString GetStatusFilterMultilingualDescription()
			=> (NoResString)"Status";

		#endregion

		#region Filter Docket Task Planning Status

		public void TestFilterDocketTaskPlanningStatus_RegistryEnabled()
		{
			TestFilterDocketTaskPlanningStatusCore(true);
		}

		public void TestFilterDocketTaskPlanningStatus_RegistryNotEnabled()
		{
			TestFilterDocketTaskPlanningStatusCore(false);
		}

		void TestFilterDocketTaskPlanningStatusCore(bool registryEnabled)
		{
			if (SupportsDocketPlanningStatus && registryEnabled)
			{
				var data = new TestDataSimpleEnvironment(Factory, 2, 1);
				var org1 = Helper.CreateClient("Client1");
				var org2 = Helper.CreateClient("Client2");
				var org3 = Helper.CreateClient("Client3");
				var org4 = Helper.CreateClient("Client4");
				Factory.Save();

				var docket1 = CreateDocket(org1, data.Whs1, "R1");
				var docket2 = CreateDocket(org2, data.Whs1, "R2");
				var docket3 = CreateDocket(org3, data.Whs1, "R3");
				var docket4 = CreateDocket(org4, data.Whs1, "R3");
				var docketCollection = GetDocketCollection();
				Factory.Save();

				docket2.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
				docket3.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				docket4.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

				using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEnabled))
				{
					var filter = (ModuleTextFilter)DocketFilter["Task Planning Status"];
					filter.IsActive = true;
					docketCollection.AdditionalFilter = DocketFilter.Filter;
					AssertEquals("TaskPlanningStatus filter category should be StatusAndFlags.", filter.Category, FilterCategories.StatusAndFlags);

					filter.Property = TaskPlanningStatus.Codes.NotReady;
					docketCollection.AdditionalFilter = DocketFilter.Filter;
					AssertContainsExactElementsInAnyOrder("Only docket2 is returned.", new[] { docket2 }, docketCollection);

					filter.Property = TaskPlanningStatus.Codes.Ready;
					docketCollection.AdditionalFilter = DocketFilter.Filter;
					AssertContainsExactElementsInAnyOrder("Only docket3 is returned.", new[] { docket3 }, docketCollection);

					filter.Property = TaskPlanningStatus.Codes.Planned;
					docketCollection.AdditionalFilter = DocketFilter.Filter;
					AssertContainsExactElementsInAnyOrder("Only docket4 is returned.", new[] { docket4 }, docketCollection);

					filter.Property = "";
					docketCollection.AdditionalFilter = DocketFilter.Filter;
					AssertContainsExactElementsInAnyOrder("ALl dockets are returned", new[] { docket1, docket2, docket3, docket4 }, docketCollection);
				}
			}
			else
			{
				AssertNull("Task Planning Status filter should be null", DocketFilter["Task Planning Status"]);
			}
		}

		protected virtual bool SupportsDocketPlanningStatus => false;

		#endregion

		#region Filter Not Invoiced

		#region TestFilterNotInvoiced

		public void TestFilterNotInvoiced()
		{
			if (IsNotInvoicedFilterUsed)
			{
				TestFilterNotInvoicedCore();
			}
			else
			{
				AssertNull(DocketFilter["Not Invoiced"]);
			}
		}

		protected void TestFilterNotInvoicedCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			var chargeCode = GetJobSpecificChargeCode();

			var docketWithoutCharges = (TDocket)CreateDocket(data.Org1, data.Whs1, "D1");
			var docketWithoutChargesHeader = Helper.CreateRatingJob(docketWithoutCharges, WhsDocketSchema.Constants.Prefix);
			AssertEquals("Precondition - this docket should have no charges.", 0, docketWithoutChargesHeader.Charges.Count);

			var docketWithNotPostedCharges = (TDocket)CreateDocket(data.Org1, data.Whs1, "D2");
			var docketWithNotPostedChargesHeader = Helper.CreateRatingJob(docketWithNotPostedCharges, WhsDocketSchema.Constants.Prefix);
			var docketWithNotPostedCharges_UnpostedCharge = Helper.CreateJobCharge(docketWithNotPostedChargesHeader, chargeCode, 5m);
			AssertEquals("Precondition - this docket should have 1 unposted charge.", 1, docketWithNotPostedChargesHeader.Charges.Count);
			AssertEquals("Precondition - this charge should not be posted.", false, docketWithNotPostedCharges_UnpostedCharge.IsCostPosted || docketWithNotPostedCharges_UnpostedCharge.IsRevenuePosted);

			var docketWithPostedCharges = (TDocket)CreateDocket(data.Org1, data.Whs1, "D3");
			var docketWithPostedChargesHeader = Helper.CreateRatingJob(docketWithPostedCharges, WhsDocketSchema.Constants.Prefix);
			var docketWithPostedCharges_PostedCharge = Helper.CreateJobCharge(docketWithPostedChargesHeader, chargeCode, 10m);
			Helper.PostInvoice(docketWithPostedChargesHeader);
			AssertEquals("Precondition - this docket should have 1 posted charge.", 1, docketWithPostedChargesHeader.Charges.Count);
			AssertEquals("Precondition - this charge should be posted.", true, docketWithPostedCharges_PostedCharge.IsCostPosted || docketWithPostedCharges_PostedCharge.IsRevenuePosted);

			var docketWithNotPostedAndPostedCharges = (TDocket)CreateDocket(data.Org1, data.Whs1, "D4");
			var docketWithNotPostedAndPostedChargesHeader = Helper.CreateRatingJob(docketWithNotPostedAndPostedCharges, WhsDocketSchema.Constants.Prefix);
			var docketWithNotPostedAndPostedCharges_PostedCharge = Helper.CreateJobCharge(docketWithNotPostedAndPostedChargesHeader, chargeCode, 15m);
			Helper.PostInvoice(docketWithNotPostedAndPostedChargesHeader);
			var docketWithNotPostedAndPostedCharges_UnpostedCharge = Helper.CreateJobCharge(docketWithNotPostedAndPostedChargesHeader, chargeCode, 20m);
			AssertEquals("Precondition - this docket should have 1 unposted and 1 posted charge.", 2, docketWithNotPostedAndPostedChargesHeader.Charges.Count);
			AssertEquals("Precondition - this charge should be posted.", true, docketWithNotPostedAndPostedCharges_PostedCharge.IsCostPosted || docketWithNotPostedAndPostedCharges_PostedCharge.IsRevenuePosted);
			AssertEquals("Precondition - this charge should not be posted.", false, docketWithNotPostedAndPostedCharges_UnpostedCharge.IsCostPosted || docketWithNotPostedAndPostedCharges_UnpostedCharge.IsRevenuePosted);

			Factory.Save();

			Asserter.AddToScope(docketWithoutCharges);
			Asserter.AddToScope(docketWithNotPostedCharges);
			Asserter.AddToScope(docketWithPostedCharges);
			Asserter.AddToScope(docketWithNotPostedAndPostedCharges);

			var filter = (ModuleFlagsFilter)FilterStripBizO["Not Invoiced"];
			filter.Property0 = false;
			Asserter.AssertMatches("When filter set to false, then only posted jobs should be found.", filter, docketWithPostedCharges);

			filter.Property0 = true;
			Asserter.AssertMatches("When filter set to true, then only un-posted jobs should be found.", filter, docketWithoutCharges, docketWithNotPostedCharges, docketWithNotPostedAndPostedCharges);
		}

		#endregion

		#region TestFilterNotInvoiced_WithPeriodic

		[TestDate(2012, 6, 1)]
		public void TestFilterNotInvoiced_WithPeriodic()
		{
			if (IsNotInvoicedFilterUsed)
			{
				TestFilterNotInvoiced_WithPeriodicCore();
			}
			else
			{
				AssertNull(DocketFilter["Not Invoiced"]);
			}
		}

		protected void TestFilterNotInvoiced_WithPeriodicCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var chargeCode = GetJobSpecificChargeCode();
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;
			var rateEntry = Helper.CreateRateEntry(clientRate, new ZDate(2011, 1, 1), new ZDate(2013, 1, 1));
			var rateLine = Helper.CreateRateLine(rateEntry, chargeCode, Constants.PkgUnit.Unit, 5m);

			Factory.Save();

			// No charges for Job and no charges in its Periodic
			var docketWithoutCharges = CreateFinalisedDocket(data, "D1", new ZDateTime(2012, 1, 3));
			var invoiceWithouCharges = CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(2012, 1, 1), new ZDateTime(2012, 1, 7));
			var invoiceWithouChargesHeader = Helper.CreateRatingJob(invoiceWithouCharges);
			AssertEquals("Precondition - this invoice should have no charges.", 0, invoiceWithouChargesHeader.Charges.Count);

			// No charges for Job and related unposted charge in its Periodic
			var docketWithRelatedNotPostedCharges = CreateFinalisedDocket(data, "D2", new ZDateTime(2012, 1, 10));
			var invoiceWithRelatedNotPostedCharges = CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(2012, 1, 8), new ZDateTime(2012, 1, 14));
			Factory.Save();
			invoiceWithRelatedNotPostedCharges.AutoRateJobHeader(null);
			AssertEquals("Precondition - this invoice should have 1 related unposted charge.", 1, invoiceWithRelatedNotPostedCharges.JobHeader.Charges.Count);
			AssertEquals("Precondition - this charge should not be posted.", false, invoiceWithRelatedNotPostedCharges.JobHeader.Charges[0].JR_IsPosted);
			AssertEquals("Precondition - this charge should be related to a docket.", "D2", invoiceWithRelatedNotPostedCharges.JobHeader.Charges[0].JobChargeAttrib_DocketReference);

			// No charges for Job and related posted charge in its Periodic
			var docketWithRelatedPostedCharges = CreateFinalisedDocket(data, "D3", new ZDateTime(2012, 1, 17));
			var invoiceWithRelatedPostedCharges = CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(2012, 1, 15), new ZDateTime(2012, 1, 21));
			Factory.Save();
			invoiceWithRelatedPostedCharges.AutoRateJobHeader(null);
			invoiceWithRelatedPostedCharges.PostInvoice();
			AssertEquals("Precondition - this invoice should have 1 related posted charge.", 1, invoiceWithRelatedPostedCharges.JobHeader.Charges.Count);
			AssertEquals("Precondition - this charge should be posted.", true, invoiceWithRelatedPostedCharges.JobHeader.Charges[0].JR_IsPosted);
			AssertEquals("Precondition - this charge should be related to a docket.", "D3", invoiceWithRelatedPostedCharges.JobHeader.Charges[0].JobChargeAttrib_DocketReference);

			// No charges for Job and not related posted charge in its Periodic
			var docketWithNoRelatedPostedCharges = CreateFinalisedDocket(data, "D4", new ZDateTime(2012, 1, 24));
			var invoiceWithNoRelatedPostedCharges = CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(2012, 1, 22), new ZDateTime(2012, 1, 28));
			var invoiceWithNoRelatedPostedChargesHeader = Helper.CreateRatingJob(invoiceWithNoRelatedPostedCharges);
			Helper.CreateJobCharge(invoiceWithNoRelatedPostedChargesHeader, chargeCode, 10m);
			invoiceWithNoRelatedPostedCharges.PostInvoice();
			AssertEquals("Precondition - this invoice should have 1 unrelated posted charge.", 1, invoiceWithNoRelatedPostedChargesHeader.Charges.Count);
			AssertEquals("Precondition - this charge should be posted.", true, invoiceWithNoRelatedPostedChargesHeader.Charges[0].JR_IsPosted);
			AssertEquals("Precondition - this charge should be not related to a docket.", "", invoiceWithNoRelatedPostedChargesHeader.Charges[0].JobChargeAttrib_DocketReference);

			// No charges for Job and Related unposted and posted charges for its Periodic.
			var docketWithRelatedNotPostedAndPostedCharges = CreateFinalisedDocket(data, "D5", new ZDateTime(2012, 1, 31));
			var invoiceWithRelatedNotPostedAndPostedCharges = CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(2012, 1, 29), new ZDateTime(2012, 2, 4));
			Factory.Save();
			invoiceWithRelatedNotPostedAndPostedCharges.AutoRateJobHeader(null);
			invoiceWithRelatedNotPostedAndPostedCharges.PostInvoice();
			var strategy = new AutoRateInvoicingStrategy(invoiceWithRelatedNotPostedAndPostedCharges, invoiceWithRelatedNotPostedAndPostedCharges.JobHeader);
			var context = new RatingContext();
			var adaptersProvider = new Mock<IRatingAdaptersProvider>();
			var interactor = new LoggerDecorator(context.Logger);
			var autoRatesCollection = new AutoRatingRunner(invoiceWithRelatedNotPostedAndPostedCharges, context).RetrieveAllCharges(invoiceWithRelatedNotPostedAndPostedCharges.GetRatingAdapters().ToArray(), adaptersProvider.Object, CostSell.Revenue);
			strategy.AddAutoRates(interactor, autoRatesCollection, CostSell.Revenue, new[] { invoiceWithRelatedNotPostedAndPostedCharges.ET_StorageJobNumber });
			var invoiceWithRelatedNotPostedAndPostedCharges_PostedCharge = invoiceWithRelatedNotPostedAndPostedCharges.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_IsPosted);

			//			var invoiceWithRelatedNotPostedAndPostedCharges_UnpostedCharge = invoiceWithRelatedNotPostedAndPostedChargesHeader.Charges.Cast<Charge>().Single(c => !c.JR_IsPosted);
			//			AssertEquals("Precondition - this invoice should have 1 related unposted and 1 related posted charge.", 2, invoiceWithRelatedNotPostedAndPostedChargesHeader.Charges.Count);

			AssertEquals("Precondition - this charge should be posted.", true, invoiceWithRelatedNotPostedAndPostedCharges_PostedCharge.JR_IsPosted);

			//			AssertEquals("Precondition - this charge should not be posted.", false, invoiceWithRelatedNotPostedAndPostedCharges_UnpostedCharge.JR_IsPosted);

			AssertEquals("Precondition - this charge should be related to a docket.", "D5", invoiceWithRelatedNotPostedAndPostedCharges_PostedCharge.JobChargeAttrib_DocketReference);

			//			AssertEquals("Precondition - this charge should be related to a docket.", "D5", invoiceWithRelatedNotPostedAndPostedCharges_UnpostedCharge.JobChargeAttrib_DocketReference);

			Factory.Save();

			Asserter.AddToScope(docketWithoutCharges);
			Asserter.AddToScope(docketWithRelatedNotPostedCharges);
			Asserter.AddToScope(docketWithRelatedPostedCharges);
			Asserter.AddToScope(docketWithNoRelatedPostedCharges);
			//			Asserter.AddToScope(docketWithRelatedNotPostedAndPostedCharges);

			var filter = (ModuleFlagsFilter)FilterStripBizO["Not Invoiced"];
			filter.Property0 = false;
			Asserter.AssertMatches("When filter set to false, then only posted jobs should be found.", filter, docketWithRelatedPostedCharges);

			filter.Property0 = true;
			//			Asserter.AssertMatches("When filter set to true, then only un-posted jobs should be found.", filter, docketWithoutCharges, docketWithRelatedNotPostedCharges, docketWithNoRelatedPostedCharges, docketWithRelatedNotPostedAndPostedCharges);
			Asserter.AssertMatches("When filter set to true, then only un-posted jobs should be found.", filter, docketWithoutCharges, docketWithRelatedNotPostedCharges, docketWithNoRelatedPostedCharges);
			//	All the above will be uncommented and fixed in WI00065720
		}

		protected virtual TDocket CreateFinalisedDocket(TestDataSimpleEnvironment data, ZString reference, ZDateTime finalisedDate)
		{
			return null;
		}

		WhsInvoice CreateWhsInvoice(OrgHeader client, WhsWarehouse warehouse, ZDateTime storageFromDate, ZDateTime storageToDate)
		{
			var result = Factory.New<WhsInvoice>();
			result.ET_OH_Client = client.PK;
			result.ET_WW = warehouse.PK;
			result.ET_StorageFromDate = storageFromDate;
			result.ET_StorageToDate = storageToDate;

			result.Validation.ValidateET_StorageFromDate(); // when setting from date before to date we get error on from date field, that need to be cleared.
			return result;
		}

		#endregion

		#region TestFilterNotInvoiced_StorageCharge

		[TestDate(2012, 02, 03)]
		public void TestFilterNotInvoiced_StorageCharge_WithCharge()
		{
			TestFilterNotInvoiced_StorageCharge(true);
		}

		[TestDate(2012, 02, 03)]
		public void TestFilterNotInvoiced_StorageCharge_WithoutCharge()
		{
			TestFilterNotInvoiced_StorageCharge(false);
		}

		void TestFilterNotInvoiced_StorageCharge(bool hasStorageCharge)
		{
			if (IsNotInvoicedFilterUsed)
			{
				TestFilterNotInvoiced_StorageCharge_Core(hasStorageCharge);
			}
			else
			{
				AssertNull(DocketFilter["Not Invoiced"]);
			}
		}

		void TestFilterNotInvoiced_StorageCharge_Core(bool hasStorageCharge)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var chargeCode = GetJobSpecificChargeCode();
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;
			var rateEntry = Helper.CreateRateEntry(clientRate, new ZDate(2011, 1, 1), new ZDate(2013, 1, 1));
			var rateLine = Helper.CreateRateLine(rateEntry, chargeCode, Constants.PkgUnit.Unit, 5m);

			Factory.Save();

			// No charges for Job and no charges in its Periodic
			var docket = CreateFinalisedDocket(data, "D1", new ZDateTime(2012, 2, 3));
			Factory.Save();

			if (hasStorageCharge)
			{
				// posted charges for its Periodic invoice.
				var invoiceWithRelatedNotPostedAndPostedCharges = CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(2012, 1, 29), new ZDateTime(2012, 2, 4));
				Factory.Save();

				invoiceWithRelatedNotPostedAndPostedCharges.AutoRateJobHeader(null);
				invoiceWithRelatedNotPostedAndPostedCharges.PostInvoice();
				var strategy = new AutoRateInvoicingStrategy(invoiceWithRelatedNotPostedAndPostedCharges, invoiceWithRelatedNotPostedAndPostedCharges.JobHeader);
				var context = new RatingContext();
				var adaptersProvider = new Mock<IRatingAdaptersProvider>();
				var interactor = new LoggerDecorator(context.Logger);
				var autoRatesCollection = new AutoRatingRunner(invoiceWithRelatedNotPostedAndPostedCharges, context).RetrieveAllCharges(invoiceWithRelatedNotPostedAndPostedCharges.GetRatingAdapters().ToArray(), adaptersProvider.Object, CostSell.Revenue);
				strategy.AddAutoRates(interactor, autoRatesCollection, CostSell.Revenue, new[] { invoiceWithRelatedNotPostedAndPostedCharges.ET_StorageJobNumber });
				var invoiceWithRelatedNotPostedAndPostedCharges_PostedCharge = invoiceWithRelatedNotPostedAndPostedCharges.JobHeader.Charges.Cast<Charge>().Single(c => c.JR_IsPosted);

				AssertEquals("Precondition - this charge should be posted.", true, invoiceWithRelatedNotPostedAndPostedCharges_PostedCharge.JR_IsPosted);
				AssertEquals("Precondition - this charge should be related to a docket.", "D1", invoiceWithRelatedNotPostedAndPostedCharges_PostedCharge.JobChargeAttrib_DocketReference);

				Factory.Save();
			}

			Asserter.AddToScope(docket);

			var filter = (ModuleFlagsFilter)GetNewFilterStripBusinessObject()["Not Invoiced"];
			filter.Property0 = false;
			if (hasStorageCharge)
			{
				Asserter.AssertMatches("Job has charge. Should show when select invoiced job.", filter, docket);
			}
			else
			{
				Asserter.AssertMatches("Job does not have charge. Should not show when select invoiced job.", filter);
			}

			filter.Property0 = true;
			if (hasStorageCharge)
			{
				Asserter.AssertMatches("Job has charge. Should not show when select not invoiced job.", filter);
			}
			else
			{
				Asserter.AssertMatches("Job does not have charge. Should show when select not invoiced job.", filter, docket);
			}
		}

		#endregion

		#region IsNotInvoicedFilterUsed

		protected virtual bool IsNotInvoicedFilterUsed => false;

		#endregion

		#region GetJobSpecificChargeCode

		protected virtual AccChargeCode GetJobSpecificChargeCode()
		{
			return null;
		}

		#endregion

		#endregion

		#region Filter Reference

		#region TestOnlyAdditionalReferencesStartWithPrefix

		public void TestOnlyAdditionalReferencesStartWithPrefix()
		{
			var errorList = new List<string>();
			var regex = new Regex(@"Ref.\s(?<Description>.+)\s\((?<Code>.+)\)");
			foreach (var filter in DocketFilter.ModuleFilters)
			{
				if (filter.Description.StartsWith("Ref."))
				{
					var match = regex.Match(filter.Description);
					if (!match.Success || !WarehouseDataRegistry.Instance.AdditionalReferenceType.Value.ContainsCode(match.Groups["Code"].Value))
					{
						errorList.Add(filter.Description);
					}
				}
			}

			Assert("No module filter should start with 'Ref.' as it may conflict with a user-created Additional Reference.\r\n" +
				"Filters that incorrectly start with 'Ref.' are:\r\n" + string.Join("\r\n", errorList), errorList.Count == 0);
		}

		#endregion

		#region TestFilterDocketReference

		public void TestFilterDocketReference()
		{
			if (HasAdditionalReferences)
			{
				SetupTestData();

				Docket12.WD_CustomerReference = "CUST12";
				Docket12.WD_TransportReference = "TRANS12";
				WhsDocketReference ref121 = Docket12.References.AddNew();
				ref121.WX_Reference = "TRANS12";
				ref121.WX_RefType = "OTH";
				WhsDocketReference ref122 = Docket12.References.AddNew();
				ref122.WX_Reference = "HB555";
				ref122.WX_RefType = "HSB";
				WhsDocketReference ref123 = Docket12.References.AddNew();
				ref123.WX_Reference = "CUST12";
				ref123.WX_RefType = "OTH";

				Docket21.WD_CustomerReference = "CUST21";
				Docket21.WD_TransportReference = "TRANS21";
				WhsDocketReference ref211 = Docket21.References.AddNew();
				ref211.WX_Reference = "TRANS21";
				ref211.WX_RefType = "OTH";
				WhsDocketReference ref212 = Docket21.References.AddNew();
				ref212.WX_Reference = "1555";
				ref212.WX_RefType = "MAR";
				WhsDocketReference ref213 = Docket21.References.AddNew();
				ref213.WX_Reference = "CUST21";
				ref213.WX_RefType = "OTH";

				Docket22.WD_CustomerReference = "CUST22";
				Docket22.WD_TransportReference = "TRANS22";
				WhsDocketReference ref221 = Docket22.References.AddNew();
				ref221.WX_Reference = "TRANS12";
				ref221.WX_RefType = "OTH";
				WhsDocketReference ref222 = Docket22.References.AddNew();
				ref222.WX_Reference = "HB555";
				ref222.WX_RefType = "OTH";
				WhsDocketReference ref223 = Docket22.References.AddNew();
				ref223.WX_Reference = "CUST12";
				ref223.WX_RefType = "OTH";

				Factory.Save(); // DBOnlyQuery used

				TestFilterDocketReferenceHouseBill();
				TestFilterDocketReferenceWithEqualsOperator();
				TestFilterDocketReferenceMarks();
				TestFilterDocketReferenceCustomerReference();
				TestFilterDocketReferenceTransportReference();
				TestFilterDocketReferenceAny();
				TestFilterDocketRefernceIgnoringRefType();
			}
			else
			{
				Assert("Additional references do not apply", true);
			}
		}

		void TestFilterDocketReferenceTransportReference()
		{
			if (SupportsTransportReferenceFilter)
			{
				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Transport Reference", (ZString)"TRANS12");
				DocketAssert(false, true, false, false);
				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Transport Reference");
			}
			else
			{
				AssertNull(DocketFilter["Transport Reference"]);
			}

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Ref. Other (OTH)", (ZString)"TRANS12");
			DocketAssert(false, true, false, true);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Ref. Other (OTH)");

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference", (ZString)"TRANS");
			DocketAssert(false, true, true, true);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference");
		}

		void TestFilterDocketReferenceCustomerReference()
		{
			if (SupportsCustomerReferenceFilter)
			{
				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Customer Reference", (ZString)"CUST12");
				DocketAssert(false, true, false, false);
				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Customer Reference");
			}
			else
			{
				AssertNull(DocketFilter["Customer Reference"]);
			}

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Ref. Other (OTH)", (ZString)"CUST12");
			DocketAssert(false, true, false, true);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Ref. Other (OTH)");

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference", (ZString)"CUST");
			DocketAssert(false, true, true, true);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference");
		}

		void TestFilterDocketReferenceHouseBill()
		{
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Ref. House Bill (HSB)", (ZString)"HB555");
			DocketAssert(false, true, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Ref. House Bill (HSB)");

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Ref. Other (OTH)", (ZString)"HB555");
			DocketAssert(false, false, false, true);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Ref. Other (OTH)");

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference", (ZString)"HB5");
			DocketAssert(false, true, false, true);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference");
		}

		void TestFilterDocketReferenceWithEqualsOperator()
		{
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference", (ZString)"HB5", (ZString)"exact");
			DocketAssert(false, false, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference", (ZString)"HB555", (ZString)"exact");
			DocketAssert(false, true, false, true);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference");
		}

		void TestFilterDocketReferenceMarks()
		{
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Ref. Marks and Numbers (MAR)", (ZString)"1555");
			DocketAssert(false, false, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Ref. Marks and Numbers (MAR)");
		}

		void TestFilterDocketReferenceAny()
		{
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference", (ZString)"1");
			DocketAssert(true, true, true, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference");
		}

		void TestFilterDocketRefernceIgnoringRefType()
		{
			var ref12_33 = Docket12.References.AddNew();
			ref12_33.WX_Reference = "33Reference";
			ref12_33.WX_RefType = "FAK"; // not a valid additional reference type, but the filter should still find it, as it doesn't include type

			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference", (ZString)"33Reference");
			DocketAssert(false, true, false, false);
		}

		public virtual void TestFilterDocketReferenceMatchesExternalReferenceEvenWithNoDocketReferences()
		{
			SetupTestData();
			Factory.Save(); // DBOnlyQuery used

			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, ExternalReferenceFieldName, (ZString)"1");
			DocketAssert(true, true, false, false);
			FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, ExternalReferenceFieldName);
		}

		protected virtual bool HasAdditionalReferences
		{
			get { return true; }
		}

		protected virtual ZString ExternalReferenceFieldName
		{
			get { return "Reference"; }
		}

		protected virtual bool SupportsCustomerReferenceFilter
		{
			get { return true; }
		}

		protected virtual bool SupportsTransportReferenceFilter
		{
			get { return true; }
		}

		public virtual void TestFilterDocketReferenceAnyForLongValue()
		{
			if (HasAdditionalReferences)
			{
				var whs1 = Helper.CreateWarehouse("WH1", "A");
				var whs2 = Helper.CreateWarehouse("WH2", "A");
				var org1 = Helper.CreateClient("O1", "O1");
				var org2 = Helper.CreateClient("O2", "O2");

				var docket1 = CreateDocket(org1, whs1, "D1");
				var container1 = docket1.Containers.AddNew();
				container1.WC_ContainerNum = "LongStringXXXXXXXX20"; // Length:20

				var docket2 = CreateDocket(org2, whs2, "D2");
				var ref2 = docket2.References.AddNew();
				ref2.WX_Reference = "LongStringXXXXXXXX2022";  // Length:22
				ref2.WX_RefType = "OTH";

				var docket3 = CreateDocket(org2, whs2, "D3");
				var ref3 = docket3.References.AddNew();
				ref3.WX_Reference = "LongStringXXXXXXXX2022X25";  // Length:25
				ref3.WX_RefType = "OTH";

				var docket4 = CreateDocket(org1, whs2, "D4");
				docket4.WD_CustomerReference = "LongStringXXXXXXXX2022X25XXX30";   // Length:30

				var docket5 = CreateDocket(org1, whs2, "D5");
				docket5.WD_CustomerReference = "LongStringXXXXXXXX2022X25XXX30XXX35";   // Length:35

				var docket6 = CreateDocket(org2, whs1, "D6");
				docket6.WD_TransportReference = "LongStringXXXXXXXX2022X25XXX30";  // Length:30

				var docket7 = CreateDocket(org2, whs1, "D7");
				docket7.WD_TransportReference = "LongStringXXXXXXXX2022X25XXX30XXX35";  // Length:35

				var docket8 = CreateDocket(org2, whs1, "D8");
				docket8.WD_ExternalReference = "LongStringXXXXXXXX2022X25XXX30";  // Length:30

				var docket9 = CreateDocket(org2, whs1, "D9");
				docket9.WD_ExternalReference = "LongStringXXXXXXXX2022X25XXX30XXX35";  // Length:35

				var bizOs = GetBizOsForFilterDocketReferenceAny(docket1, docket2, docket3, docket4, docket5, docket6, docket7, docket8, docket9);

				Factory.Save(); // DBOnlyQuery used

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference", (ZString)"LongStringXXXXXXXX20");   // Length:20
				var collection1 = GetCollectionWithFilter(DocketFilter.Filter);
				var docketsToAssert1 = new[]
				{
					new { Include = SupportsContainerNoFilter, Docket = bizOs[0] },
					new { Include = true, Docket = bizOs[1] },
					new { Include = true, Docket = bizOs[2] },
					new { Include = SupportsCustomerReferenceFilter, Docket = bizOs[3] },
					new { Include = SupportsCustomerReferenceFilter, Docket = bizOs[4] },
					new { Include = SupportsTransportReferenceFilter, Docket = bizOs[5] },
					new { Include = SupportsTransportReferenceFilter, Docket = bizOs[6] },
					new { Include = true, Docket = bizOs[7] },
					new { Include = true, Docket = bizOs[8] },
				};
				AssertContainsExactElementsInAnyOrder(docketsToAssert1.Where(o => o.Include).Select(o => o.Docket), collection1);
				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference");

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference", (ZString)"LongStringXXXXXXXX2022");   // Length:22
				var collection2 = GetCollectionWithFilter(DocketFilter.Filter);
				var docketsToAssert2 = new[]
				{
					new { Include = true, Docket = bizOs[1] },
					new { Include = true, Docket = bizOs[2] },
					new { Include = SupportsCustomerReferenceFilter, Docket = bizOs[3] },
					new { Include = SupportsCustomerReferenceFilter, Docket = bizOs[4] },
					new { Include = SupportsTransportReferenceFilter, Docket = bizOs[5] },
					new { Include = SupportsTransportReferenceFilter, Docket = bizOs[6] },
					new { Include = true, Docket = bizOs[7] },
					new { Include = true, Docket = bizOs[8] },
				};
				AssertContainsExactElementsInAnyOrder(docketsToAssert2.Where(o => o.Include).Select(o => o.Docket), collection2);
				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference");

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference", (ZString)"LongStringXXXXXXXX2022X25");   // Length:25
				var collection3 = GetCollectionWithFilter(DocketFilter.Filter);
				var docketsToAssert3 = new[]
				{
					new { Include = true, Docket = bizOs[2] },
					new { Include = SupportsCustomerReferenceFilter, Docket = bizOs[3] },
					new { Include = SupportsCustomerReferenceFilter, Docket = bizOs[4] },
					new { Include = SupportsTransportReferenceFilter, Docket = bizOs[5] },
					new { Include = SupportsTransportReferenceFilter, Docket = bizOs[6] },
					new { Include = true, Docket = bizOs[7] },
					new { Include = true, Docket = bizOs[8] },
				};
				AssertContainsExactElementsInAnyOrder(docketsToAssert3.Where(o => o.Include).Select(o => o.Docket), collection3);
				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference");

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference", (ZString)"LongStringXXXXXXXX2022X25XXX30");   // Length:30

				var collection4 = GetCollectionWithFilter(DocketFilter.Filter);
				var docketsToAssert4 = new[]
				{
					new { Include = SupportsCustomerReferenceFilter, Docket = bizOs[3] },
					new { Include = SupportsCustomerReferenceFilter, Docket = bizOs[4] },
					new { Include = SupportsTransportReferenceFilter, Docket = bizOs[5] },
					new { Include = SupportsTransportReferenceFilter, Docket = bizOs[6] },
					new { Include = true, Docket = bizOs[7] },
					new { Include = true, Docket = bizOs[8] },
				};
				AssertContainsExactElementsInAnyOrder(docketsToAssert4.Where(o => o.Include).Select(o => o.Docket), collection4);
				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference");

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference", (ZString)"LongStringXXXXXXXX2022X25XXX30XXX35");   // Length:35

				var collection5 = GetCollectionWithFilter(DocketFilter.Filter);
				var docketsToAssert5 = new[]
				{
					new { Include = SupportsCustomerReferenceFilter, Docket = bizOs[4] },
					new { Include = SupportsTransportReferenceFilter, Docket = bizOs[6] },
					new { Include = true, Docket = bizOs[8] }
				};
				AssertContainsExactElementsInAnyOrder(docketsToAssert5.Where(o => o.Include).Select(o => o.Docket), collection5);
				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Any Reference");
			}
			else
			{
				Assert("Additional references do not apply", true);
			}
		}

		protected virtual IBusinessObjectCollection GetCollectionWithFilter(ZQuery filter)
		{
			var collection = GetDocketCollection();
			collection.AdditionalFilter = filter;

			return collection;
		}

		protected virtual BusinessObject[] GetBizOsForFilterDocketReferenceAny(params WhsDocket[] dockets)
		{
			return dockets;
		}

		#endregion

		#endregion

		#region Filter Container

		public void TestFilterDocketContainerNumber()
		{
			if (SupportsContainerNoFilter)
			{
				SetupTestData();

				WhsDocketContainer con = Docket12.Containers.AddNew();
				con.WC_ContainerNum = "ACT12";

				con = Docket21.Containers.AddNew();
				con.WC_ContainerNum = "ZZZ21";

				con = Docket22.Containers.AddNew();
				con.WC_ContainerNum = "ZZZ22";

				Factory.Save(); // DBOnlyQuery used

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Container No.", (ZString)"ACT");
				DocketAssert(false, true, false, false);

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, "Container No.", (ZString)"ZZZ");
				DocketAssert(false, false, true, true);
			}
			else
			{
				AssertNull(DocketFilter["Container No."]);
			}
		}

		protected virtual bool SupportsContainerNoFilter
		{
			get { return true; }
		}

		#endregion

		#region Filter FinalizedDate

		public void TestFinalizedDateFilter()
		{
			TestFinalizedDateFilterCore();
		}

		protected virtual void TestFinalizedDateFilterCore()
		{
			var whs1 = Helper.CreateWarehouse("WH1", "A");
			var whs2 = Helper.CreateWarehouse("WH2", "A");
			var org1 = Helper.CreateClient("O1", "O1");
			var org2 = Helper.CreateClient("O2", "O2");
			var docket11 = (TDocket)CreateDocket(org1, whs1, "11");
			var docket12 = (TDocket)CreateDocket(org1, whs2, "12");
			var docket21 = (TDocket)CreateDocket(org2, whs1, "21");
			var docket22 = (TDocket)CreateDocket(org2, whs2, "22");
			Factory.Save();

			docket11.WD_DocketStatus = DocketStatus.Codes.Finalised;
			docket11.WD_FinalisedDate = ZDateTimeOffset.Now;
			docket12.WD_DocketStatus = DocketStatus.Codes.Finalised;
			docket12.WD_FinalisedDate = ZDateTimeOffset.Now.AddDays(5);
			docket21.WD_DocketStatus = DocketStatus.Codes.Finalised;
			docket21.WD_FinalisedDate = ZDateTimeOffset.Now.AddDays(-30);
			docket22.WD_DocketStatus = DocketStatus.Codes.Finalised;
			docket22.WD_FinalisedDate = ZDateTimeOffset.Now.AddDays(-2);

			Asserter.AddToScope(docket11, docket12, docket21, docket22);

			var docketFilterStrip = GetNewFilterStripBusinessObject();
			var dateFilter = (ModuleDateFilter)docketFilterStrip["Finalized Date"];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Now.AddDays(-4);
			dateFilter.Property2 = ZDateTime.Now;
			Asserter.AssertMatches("These dockets should have been filtered through.", dateFilter, docket11, docket22);
		}

		#endregion

		#region Filter WorkflowCustomFields

		public void TestWorkflowCustomFields()
		{
			ModuleFilterCollection filterCollection = DocketFilter.ModuleFilters;
			AssertNull(filterCollection["stringField"]);
			AssertNull(filterCollection["intField"]);
			AssertNull(filterCollection["dateTimeField"]);
			AssertNull(filterCollection["boolField"]);

			if (!string.IsNullOrEmpty(WorkflowDescriptorCode))
			{
				var template = Helper.CreateWorkflowTemplate("T1", WorkflowDescriptorCode);
				Helper.AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
				Helper.AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
				Helper.AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
				Helper.AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);
				Factory.Save();
				WorkflowCustomFieldsFilter.ClearCache();
				filterCollection = GetNewFilterStripBusinessObject().ModuleFilters;
				AssertEquals(typeof(ModuleTextFilter), filterCollection["stringField"].GetType());
				AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["intField"].GetType());
				AssertEquals(typeof(ModuleDateFilter), filterCollection["dateTimeField"].GetType());
				AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
			}
			else
			{
				// Workflow Custom Fields not supported
			}
		}

		protected virtual string WorkflowDescriptorCode
		{
			get { return ""; }
		}

		#endregion

		#region Filter PackageID

		public void TestPackageID()
		{
			if (SupportsPackageIdFilter)
			{
				TestPackageIDCore();
			}
			else
			{
				AssertNull(DocketFilter["Package ID"]);
			}
		}

		protected virtual void TestPackageIDCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// setup 2 orders with a package on each

			var order1 = (TDocket)CreateDocket(data.Org1, data.Whs1, "D1");
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)order1);
			var order1Package = packageJob1.Packages.AddNew("PLT");
			order1Package.KP_PackageID = "o11";

			var order2 = (TDocket)CreateDocket(data.Org1, data.Whs1, "D2");
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)order2);
			var order2Package = packageJob2.Packages.AddNew().Packages.AddNew("PLT"); // intentionally a child-Package
			order2Package.KP_PackageID = "o12";

			Factory.Save(); // for dbonlyquery

			Asserter.AddToScope(order1);
			Asserter.AddToScope(order2);
			var filter = (ModuleTextFilter)FilterStripBizO["Package ID"];

			// equal

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "o11";
			Asserter.AssertMatches("Equals 'o11' should return only Order1 (Order1 has a Package with ID 'o11').", filter, order1);
			filter.Property = "o12";
			Asserter.AssertMatches("Equals 'o12' should return only Order2 (Order2 has a Package with ID 'o12').", filter, order2);
			filter.Property = "1";
			Asserter.AssertMatches("Equals '1' should return no Orders (each has Package ID 'o11' and 'o12' respectively).", filter);

			// starts with

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "o11";
			Asserter.AssertMatches("Starts With 'o11' should return only Order1 (Order1 has a Package with ID 'o11').", filter, order1);
			filter.Property = "o12";
			Asserter.AssertMatches("Starts With 'o12' should return only Order2 (Order2 has a Package with ID 'o12').", filter, order2);
			filter.Property = "o";
			Asserter.AssertMatches("Starts With 'o' should return both Order1 and Order2 (each has Package ID 'o11' and 'o12' respectively).", filter, order1, order2);
			filter.Property = "1";
			Asserter.AssertMatches("Starts With '1' should return no Orders (each has Package ID 'o11' and 'o12' respectively).", filter);

			// contains

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "o11";
			Asserter.AssertMatches("Contains 'o11' should return only Order1 (Order1 has a Package with ID 'o11').", filter, order1);
			filter.Property = "o12";
			Asserter.AssertMatches("Contains 'o12' should return only Order2 (Order2 has a Package with ID 'o12').", filter, order2);
			filter.Property = "1";
			Asserter.AssertMatches("Contains '1' should return both Order1 and Order2 (each has Package ID 'o11' and 'o12' respectively).", filter, order1, order2);
		}

		protected virtual bool SupportsPackageIdFilter => false;

		#endregion

		#region Filter Handling Unit

		public void TestHandlingUnit()
		{
			if (SupportsHandlingUnitFilter)
			{
				TestHandlingUnitCore();
			}
			else
			{
				AssertNull(DocketFilter["Handling Unit"]);
			}
		}

		protected virtual void TestHandlingUnitCore()
		{
			Assert(true);
		}

		protected virtual bool SupportsHandlingUnitFilter => false;

		#endregion

		#region Test Filter Package Type

		public void TestPackageTypeFilter_ExistInFilterStrip()
		{
			var filter = DocketFilter["Package Type"];

			if (SupportsPackageTypeFilter)
			{
				AssertNotNull("Package Type filter should not be null", filter);
			}
			else
			{
				AssertNull("Package Type filter should be null", filter);
			}
		}

		public void TestPackageTypeFilter_Description()
		{
			var packageTypeFilter = DocketFilter["Package Type"];

			if (SupportsPackageTypeFilter)
			{
				AssertNotNull("Package Type Filter should not be null.", packageTypeFilter);
				var multilingualDescription = (NoResString)"Package Type";
				AssertEquals("Description:", "Package Type", packageTypeFilter.Description);
				AssertEquals("MultilingualDescription:", multilingualDescription, packageTypeFilter.MultilingualDescription);
			}
			else
			{
				AssertNull("Package Type filter should be null", packageTypeFilter);
			}
		}

		public void TestPackageTypeFilter_Category()
		{
			var packageTypeFilter = DocketFilter["Package Type"];

			if (SupportsPackageTypeFilter)
			{
				AssertNotNull("Package Type Filter should not be null.", packageTypeFilter);
				AssertEquals("Package Type Filter should be Status and flags.", FilterCategories.StatusAndFlags, packageTypeFilter.Category);
			}
			else
			{
				AssertNull("Package Type filter should be null", packageTypeFilter);
			}
		}

		public void TestPackageTypes_Lookup()
		{
			AssertNotNull("Package Types should not be null", DocketFilter.PackageTypes);
			var packageTypes = new RefPackTypeCollection(Factory);
			AssertContainsExactElementsInAnyOrder("Package Types should be same", packageTypes.GetAsCodeDescriptionPair(), DocketFilter.PackageTypes.GetAsCodeDescriptionPair());
		}

		public void TestPackageTypeFilter_MaxLength()
		{
			var packageTypeFilter = DocketFilter["Package Type"];

			if (SupportsPackageTypeFilter)
			{
				AssertNotNull("Package Type Filter should not be null.", packageTypeFilter);
				AssertEquals("MaxLength of Package Type should be set correctly.", PkgPackageSchema.KP_F3_NKPackType.MaxLength, packageTypeFilter.MaxLength);
			}
			else
			{
				AssertNull("Package Type filter should be null", packageTypeFilter);
			}
		}

		public void TestPackageTypeFilter_ResultNoMatch()
		{
			if (SupportsPackageTypeFilter)
			{
				TestPackageTypeFilter_ResultNoMatchCore();
			}
			else
			{
				Assert("Not required test", true);
			}
		}

		protected virtual void TestPackageTypeFilter_ResultNoMatchCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = (TDocket)CreateDocket(data.Org1, data.Whs1, "D1");
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)order1);
			var order1Package = packageJob1.Packages.AddNew("PLT");
			order1Package.KP_PackageID = "o11";

			var order2 = (TDocket)CreateDocket(data.Org1, data.Whs1, "D2");
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)order2);
			var order2Package = packageJob2.Packages.AddNew("CRT");
			order2Package.KP_PackageID = "o12";

			Factory.Save();

			Asserter.AddToScope(order1);
			Asserter.AddToScope(order2);
			var filter = (ModuleTextFilter)DocketFilter["Package Type"];

			filter.Property = "ABC";
			Asserter.AssertMatches("Should not return any orders for Package Type 'ABC'.", filter);
		}

		public void TestPackageTypeFilter_ResultPartialMatch()
		{
			if (SupportsPackageTypeFilter)
			{
				TestPackageTypeFilter_ResultPartialMatchCore();
			}
			else
			{
				Assert("Not required test", true);
			}
		}

		protected virtual void TestPackageTypeFilter_ResultPartialMatchCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order1 = (TDocket)CreateDocket(data.Org1, data.Whs1, "D1");
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)order1);
			var order1Package = packageJob1.Packages.AddNew("PLT");
			order1Package.KP_PackageID = "o11";

			var order2 = (TDocket)CreateDocket(data.Org1, data.Whs1, "D2");
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)order2);
			var order2Package = packageJob2.Packages.AddNew("PLT");
			order2Package.KP_PackageID = "o12";

			var order3 = (TDocket)CreateDocket(data.Org1, data.Whs1, "D3");
			var packageJob3 = PkgPackageJob.LoadOrCreatePackageJob((IPackingParent)order3);
			var order3Package = packageJob3.Packages.AddNew("CRT");
			order3Package.KP_PackageID = "o13";

			Factory.Save();

			Asserter.AddToScope(order1);
			Asserter.AddToScope(order2);
			Asserter.AddToScope(order3);
			var filter = (ModuleTextFilter)DocketFilter["Package Type"];

			filter.Property = "CRT";
			Asserter.AssertMatches("Should return only order3, which has a Package with type 'CRT'.", filter, order3);

			filter.Property = "PLT";
			Asserter.AssertMatches("Should return only order1 and order2, which has a Package with type 'PLT'.", filter, order1, order2);
		}

		protected virtual bool SupportsPackageTypeFilter => false;

		#endregion

		#region PalletID

		public void TestPalletID()
		{
			TestPalletIDCore();
		}

		protected virtual void TestPalletIDCore()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("WHS", "A");
			var part = Helper.CreateProduct(client, "P1");

			var docket1 = (TDocket)CreateDocket(client, whs, "Docket1");
			var docket2 = (TDocket)CreateDocket(client, whs, "Docket2");

			Factory.Save();

			var docketLine1 = CreateDocketLine(docket1, part, 10, "ID123");
			var docketLine2 = CreateDocketLine(docket2, part, 10, "ID456");

			Factory.Save(); // DBOnlyQuery used

			Asserter.AddToScope(docket1);
			Asserter.AddToScope(docket2);
			var filter = (ModuleTextFilter)FilterStripBizO["Pallet ID"];

			// equal

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ID123";
			Asserter.AssertMatches("Equals 'ID123' should return only Docket1 (Docket1 has a Pallet ID 'ID123').", filter, docket1);
			filter.Property = "ID456";
			Asserter.AssertMatches("Equals 'ID456' should return only Docket2 (Docket2 has a Pallet ID 'ID456').", filter, docket2);
			filter.Property = "ID";
			Asserter.AssertMatches("Equals 'ID' should return no dockets (each has Pallet ID 'ID123' and 'ID456' respectively).", filter);

			// starts with

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "ID123";
			Asserter.AssertMatches("Starts With 'ID123' should return only Docket1 (Docket1 has a Pallet ID 'ID123').", filter, docket1);
			filter.Property = "ID456";
			Asserter.AssertMatches("Starts With 'ID456' should return only Docket2 (Docket2 has a Pallet ID 'ID456').", filter, docket2);
			filter.Property = "ID";
			Asserter.AssertMatches("Starts With 'ID' should return both Docket1 and Docket2 (each has Pallet ID 'ID123' and 'ID456' respectively).", filter, docket1, docket2);
			filter.Property = "D";
			Asserter.AssertMatches("Starts With 'D' should return no dockets (each has Pallet ID 'ID123' and 'ID456' respectively).", filter);

			// contains

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "ID123";
			Asserter.AssertMatches("Contains 'ID123' should return only Docket1 (Docket1 has a Pallet ID 'ID123').", filter, docket1);
			filter.Property = "ID456";
			Asserter.AssertMatches("Contains 'ID456' should return only Docket2 (Docket2 has a Pallet ID 'ID456').", filter, docket2);
			filter.Property = "ID";
			Asserter.AssertMatches("Contains 'ID' should return both Docket1 and Docket2 (each has Pallet ID 'ID123' and 'ID456' respectively).", filter, docket1, docket2);
		}

		// Required to be overriden for receive test
		protected virtual WhsDocketLine CreateDocketLine(WhsDocket docket, OrgSupplierPart part, ZDecimal units, ZString palletID)
		{
			var docketLine = CreateDocketLine(docket, part, units);
			docketLine.WE_PalletID = palletID;
			return docketLine;
		}

		#endregion

		#region TestServiceLevelFilter

		public void TestServiceLevelFilter()
		{
			if (IsServiceLevelUsed)
			{
				var serviceLevelFilterName = "Service Level";
				SetupTestData();
				var serviceLevel = Factory.New<RefServiceLevel>();
				serviceLevel.RS_Code = "Svc";
				serviceLevel.RS_Description = "Test Svc. Lvl.";
				Docket11.WD_RS_NKServiceLevel = "Svc";
				Docket12.WD_RS_NKServiceLevel = "Inv";
				Docket21.WD_RS_NKServiceLevel = "";
				Factory.Save();

				var filter = DocketFilter[serviceLevelFilterName];
				AssertNotNull(filter);
				AssertEquals(FilterCategories.Other, filter.Category);

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, serviceLevelFilterName, (ZString)"Svc");
				DocketAssert(true, false, false, false);

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, serviceLevelFilterName, (ZString)"");
				DocketAssert(true, true, true, true);

				FilterBusinessObjectTestHelper.SetFilterProperty(DocketFilter, serviceLevelFilterName, (ZString)"Inv");
				DocketAssert(false, true, false, false);
			}
			else
			{
				AssertEquals("Service Level is not used.", false, IsServiceLevelUsed);
			}
		}

		protected virtual bool IsServiceLevelUsed
		{
			get { return false; }
		}

		#endregion

		#region TestFilterTransportCo

		public void TestFilterTransportCo() => TestFilterTransportCoCore();

		protected virtual void TestFilterTransportCoCore()
		{
			if (SupportsTransportCoFilters)
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);

				var transportCo1 = Helper.CreateClient("TRC1");
				var transportCo2 = Helper.CreateClient("TRC2");

				var product = SetupAndGetProductForTransportCoFilterTesting(data);
				var docket1 = GetDocketForTransportCoFilterTesting(data, "D1", product, transportCo1);
				var docket2 = GetDocketForTransportCoFilterTesting(data, "D2", product, transportCo2);
				var docket3 = GetDocketForTransportCoFilterTesting(data, "D3", product);

				Factory.Save();

				Asserter.AddToScope(docket1, docket2, docket3);

				var filter = (ModuleGuidFilter)FilterStripBizO["Transport Co"];

				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = ZGuid.Empty;
				Asserter.AssertMatches("No Transport Co specified should return all dockets.", filter, docket1, docket2, docket3);

				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = transportCo1.PK;
				Asserter.AssertMatches("Transport Co 1 specified should return docket1", filter, docket1);

				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = transportCo2.PK;
				Asserter.AssertMatches("Transport Co 2 specified should return docket2.", filter, docket2);
			}
			else
			{
				Assert("Business object does not support Filter by Transport Co.", true);
			}
		}

		public void TestFilterTransportCo_NotEqual() => TestFilterTransportCo_NotEqualCore();

		protected virtual void TestFilterTransportCo_NotEqualCore()
		{
			if (SupportsTransportCoFiltersWithComparisonOperator)
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);

				var transportCo1 = Helper.CreateClient("TRC1");
				var transportCo2 = Helper.CreateClient("TRC2");

				var product = SetupAndGetProductForTransportCoFilterTesting(data);
				var docket1 = GetDocketForTransportCoFilterTesting(data, "D1", product, transportCo: transportCo1);
				var docket2 = GetDocketForTransportCoFilterTesting(data, "D2", product, transportCo: transportCo2);
				var docket3 = GetDocketForTransportCoFilterTesting(data, "D3", product);

				Factory.Save();

				Asserter.AddToScope(docket1, docket2, docket3);

				var filter = (ModuleGuidFilter)FilterStripBizO["Transport Co"];
				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				filter.Property = ZGuid.Empty;
				Asserter.AssertMatches("No Transport Co specified should return all dockets.", filter, docket1, docket2, docket3);

				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				filter.Property = transportCo2.PK;
				Asserter.AssertMatches("Transport Co 2 specified should return docket1", filter, docket1);

				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				filter.Property = transportCo1.PK;
				Asserter.AssertMatches("Transport Co 1 specified should return docket2.", filter, docket2);
			}
			else
			{
				Assert("Business object does not support Filter by Transport Co. with comparison operators.", true);
			}
		}

		public void TestFilterTransportCo_IsBlank() => TestFilterTransportCo_IsBlankCore();

		protected virtual void TestFilterTransportCo_IsBlankCore()
		{
			if (SupportsTransportCoFiltersWithComparisonOperator)
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);

				var transportCo1 = Helper.CreateClient("TRC1");
				var transportCo2 = Helper.CreateClient("TRC2");

				var product = SetupAndGetProductForTransportCoFilterTesting(data);
				var docket1 = GetDocketForTransportCoFilterTesting(data, "D1", product, transportCo: transportCo1);
				var docket2 = GetDocketForTransportCoFilterTesting(data, "D2", product, transportCo: transportCo2);
				var docket3 = GetDocketForTransportCoFilterTesting(data, "D3", product);

				Factory.Save();

				Asserter.AddToScope(docket1, docket2, docket3);

				var filter = (ModuleGuidFilter)FilterStripBizO["Transport Co"];
				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				Asserter.AssertMatches("IsBlank should return only docket3.", filter, docket3);
			}
			else
			{
				Assert("Business object does not support Filter by Transport Co. with comparison operators.", true);
			}
		}

		public void TestFilterTransportCo_IsNotBlank() => TestFilterTransportCo_IsNotBlankCore();

		protected virtual void TestFilterTransportCo_IsNotBlankCore()
		{
			if (SupportsTransportCoFiltersWithComparisonOperator)
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);

				var transportCo1 = Helper.CreateClient("TRC1");
				var transportCo2 = Helper.CreateClient("TRC2");

				var product = SetupAndGetProductForTransportCoFilterTesting(data);
				var docket1 = GetDocketForTransportCoFilterTesting(data, "D1", product, transportCo: transportCo1);
				var docket2 = GetDocketForTransportCoFilterTesting(data, "D2", product, transportCo: transportCo2);
				var docket3 = GetDocketForTransportCoFilterTesting(data, "D3", product);

				Factory.Save();

				Asserter.AddToScope(docket1, docket2, docket3);

				var filter = (ModuleGuidFilter)FilterStripBizO["Transport Co"];
				filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				Asserter.AssertMatches("IsNotBlank should return docket1 and docket2.", filter, docket1, docket2);
			}
			else
			{
				Assert("Business object does not support Filter by Transport Co. with comparison operators.", true);
			}
		}

		public void TestFilterTransportCo_HasComparisonOperators()
		{
			if (SupportsTransportCoFiltersWithComparisonOperator)
			{
				var filter = (ModuleGuidFilter)FilterStripBizO["Transport Co"];
				AssertEquals("Should show comparison operators.", true, filter.HasComparisonOperator);
				AssertContainsExactElementsInAnyOrder(new[] { "exact", "not equal", "is blank", "is not blank" }, filter.ComparisonOperator_List.GetAllCodes());
			}
			else
			{
				Assert("Business object does not support Filter by Transport Co. with comparison operators.", true);
			}
		}

		protected virtual OrgSupplierPart SetupAndGetProductForTransportCoFilterTesting(TestDataSimpleEnvironment data) => data.Part1;

		protected abstract TDocket GetDocketForTransportCoFilterTesting(TestDataSimpleEnvironment data, string docketReference, OrgSupplierPart product, OrgHeader transportCo = null);

		protected virtual bool SupportsTransportCoFilters => false;
		protected virtual bool SupportsTransportCoFiltersWithComparisonOperator => SupportsTransportCoFilters;

		#endregion

		#region TestProductFilter_Validation

		public void TestProductFilter_Validation()
		{
			if (TestProductFilter_ShouldProductFilterEnsureClientFilterIsEnteredFirst)
			{
				var client = Helper.CreateClient("CLIENT1");
				var part = Helper.CreateProduct(client, "P1");

				Factory.Save();

				var filter = GetNewFilterStripBusinessObject();
				var productFilter = (ModuleGuidFilter)filter["Product"];
				productFilter.IsActive = true;
				productFilter.Property = part.PK; // need to double set so validation will be run on empty value
				productFilter.Property = ZGuid.Empty;
				AssertNoError(productFilter.PropertyInfo, "Product Code can not be entered without a Client Code.");
				AssertNoError(productFilter.PropertyInfo, "Enter a valid selection.");

				productFilter.Property = part.PK;
				AssertHasError(productFilter.PropertyInfo, "Product Code can not be entered without a Client Code.");
				AssertHasError(productFilter.PropertyInfo, "Enter a valid selection.");
				productFilter.Property = ZGuid.Empty; // clean up

				var clientFilter = (ModuleGuidFilter)filter["Client"];
				clientFilter.IsActive = true;
				clientFilter.Property = client.PK;
				productFilter.Property = part.PK;
				AssertNoError(productFilter.PropertyInfo, "Product Code can not be entered without a Client Code.");
				AssertNoError(productFilter.PropertyInfo, "Enter a valid selection.");
			}
			else
			{
				Assert("This validation should not be run for this filter.", true);
			}
		}

		public void TestProductFilter_Validation_ClientFilterWithNotEqualComparisonOperator()
		{
			if (TestProductFilter_ShouldProductFilterEnsureClientFilterIsEnteredFirst)
			{
				var client = Helper.CreateClient("CLIENT1");
				var part = Helper.CreateProduct(client, "P1");

				Factory.Save();

				var filter = GetNewFilterStripBusinessObject();
				var productFilter = (ModuleGuidFilter)filter["Product"];
				productFilter.IsActive = true;
				productFilter.Property = ZGuid.Empty;
				AssertNoError(productFilter.PropertyInfo, "Product Code can not be entered with a Client filter with a 'not equal' comparison operator.");

				var clientFilter = (ModuleGuidFilter)filter["Client"];
				clientFilter.IsActive = true;
				clientFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				clientFilter.Property = client.PK;
				productFilter.Property = part.PK;
				AssertHasError(productFilter.PropertyInfo, "Product Code can not be entered with a Client filter with a 'not equal' comparison operator.");

				clientFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				AssertNoError(productFilter.PropertyInfo, "Product Code can not be entered with a Client filter with a 'not equal' comparison operator.");
			}
			else
			{
				Assert("This validation should not be run for this filter.", true);
			}
		}

		public void TestProductFilter_Validation_MultipleClientFilters()
		{
			if (TestProductFilter_ShouldProductFilterEnsureClientFilterIsEnteredFirst)
			{
				var client = Helper.CreateClient("CLIENT1");
				var part = Helper.CreateProduct(client, "P1");

				Factory.Save();

				var filter = GetNewFilterStripBusinessObject();
				var productFilter = (ModuleGuidFilter)filter["Product"];
				productFilter.IsActive = true;
				productFilter.Property = ZGuid.Empty;
				AssertNoError(productFilter.PropertyInfo, "Product Code can not be entered if there are multiple Client filters.");

				var clientFilter1 = (ModuleGuidFilter)filter["Client"];
				clientFilter1.IsActive = true;
				clientFilter1.SqlComparisonOperator = SQLComparisonOperator.Equal;
				clientFilter1.Property = client.PK;

				var clientFilter2 = filter.AddGuidFilterStrip("Client");
				clientFilter2.IsActive = true;
				clientFilter2.SqlComparisonOperator = SQLComparisonOperator.Equal;
				clientFilter2.Property = client.PK;

				productFilter.Property = part.PK;
				AssertHasError(productFilter.PropertyInfo, "Product Code can not be entered if there are multiple Client filters.");

				clientFilter1.Property = ZGuid.Empty;
				AssertNoError(productFilter.PropertyInfo, "Product Code can not be entered if there are multiple Client filters.");
			}
			else
			{
				Assert("This validation should not be run for this filter.", true);
			}
		}

		protected virtual bool TestProductFilter_ShouldProductFilterEnsureClientFilterIsEnteredFirst => true;

		#endregion

		#region TestProductFilter_ReValidation

		public void TestProductFilter_ReValidation()
		{
			var client = Helper.CreateClient("C1");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();
			var clientFilter = (ModuleGuidFilter)filter["Client"];
			var productFilter = (ModuleGuidFilter)filter["Product"];
			clientFilter.IsActive = true;
			productFilter.IsActive = true;
			FieldInvalidTextMemory.SetInvalidText(productFilter, productFilter.PropertyInfo.Name, "AA");
			productFilter.Property = ZGuid.Missing;
			AssertNoExceptionThrown("No Exception should be thrown for invalid product code.", () => clientFilter.Property = client.PK);
			AssertEquals("Product PK should not be changed.", ZGuid.Missing, productFilter.Property);
			AssertEquals("Invalid Product code should not be cleared out.", "AA", FieldInvalidTextMemory.GetInvalidText(productFilter, productFilter.PropertyInfo.Name));
		}

		#endregion

		#region TestProductFilter_MultipleItemsWithSameCode

		public virtual void TestProductFilter_MultipleItemsWithSameCode()
		{
			var whs1 = Helper.CreateWarehouse("1", "A");
			var org1 = Helper.CreateClient("O1", "O1");
			var docket11 = (TDocket)CreateDocket(org1, whs1, "11");
			var docket12 = (TDocket)CreateDocket(org1, whs1, "12");
			var docket13 = (TDocket)CreateDocket(org1, whs1, "13");

			var part11 = Helper.CreateProduct(org1, "DIFFCODE1");
			var part12 = Helper.CreateProduct(org1, "DIFFCODE2");
			var part = Helper.CreateProduct(org1, "SAMECODE");
			Factory.Save();

			var line111 = CreateDocketLine(docket11, part11, 10);
			var line112 = CreateDocketLine(docket11, part, 10);
			var line121 = CreateDocketLine(docket12, part11, 10);
			var line122 = CreateDocketLine(docket12, part12, 10);
			var line123 = CreateDocketLine(docket12, part, 10);
			var line131 = CreateDocketLine(docket13, part11, 10);
			var line132 = CreateDocketLine(docket13, part12, 10);

			Factory.Save();

			Asserter.AddToScope(docket11, docket12, docket13);

			var filter = (ModuleGuidFilter)FilterStripBizO["Client"];
			filter.Property = org1.PK;
			Asserter.AssertMatches("No filter specified should return all dockets", filter, new[] { docket11, docket12, docket13 });

			filter = (ModuleGuidFilter)FilterStripBizO["Product"];
			filter.Property = part.PK;
			Asserter.AssertMatches("Product and client specified should return two dockets", filter, new[] { docket11, docket12 });

			part11.OP_PartNum = "SAMECODE";
			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				Factory.Save();
			}

			Asserter.AssertMatches("Product and client specified should return all dockets", filter, new[] { docket11, docket12, docket13 });
		}

		#endregion

		#region TestFilterGroupMember

		public void TestFilterGroupMemberMaxLength()
		{
			if (SupportsContainerNoFilter)
			{
				AssertEquals("Has MaxLength", WhsDocketContainerSchema.WC_ContainerNum.MaxLength, DocketFilter["Container No."].MaxLength);
			}
			else
			{
				AssertNull(DocketFilter["Container No."]);
			}

			if (SupportsCustomerReferenceFilter)
			{
				AssertEquals("Has MaxLength", WhsDocketSchema.WD_CustomerReference.MaxLength, DocketFilter["Customer Reference"].MaxLength);
			}
			else
			{
				AssertNull(DocketFilter["Customer Reference"]);
			}

			if (SupportsTransportReferenceFilter)
			{
				AssertEquals("Has MaxLength", WhsDocketSchema.WD_TransportReference.MaxLength, DocketFilter["Transport Reference"].MaxLength);
			}
			else
			{
				AssertNull(DocketFilter["Transport Reference"]);
			}

			if (HasAdditionalReferences)
			{
				AssertEquals("Has MaxLength", 35, DocketFilter["Any Reference"].MaxLength);
				foreach (CodeDescriptionBool pair in WarehouseDataRegistry.Instance.AdditionalReferenceType.Value)
				{
					var description = $"Ref. {pair.Description} ({(NoResString)pair.Code})";
					AssertEquals("Has MaxLength", WhsDocketReferenceSchema.WX_Reference.MaxLength, DocketFilter[description].MaxLength);
				}
			}
			else
			{
				AssertNull(DocketFilter["Any Reference"]);
			}
		}

		#endregion

		#region Test Filter Max Length

		public virtual void TestFilterMaxLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("MaxLength of Customs Entry Key should be set correctly.", WhsDocketLineSchema.WE_BondedEntryKey.MaxLength, FilterStripBizO["Customs Entry Key"].MaxLength);
				AssertEquals("MaxLength of Pallet ID should be set correctly.", WhsDocketLineSchema.WE_PalletID.MaxLength, FilterStripBizO["Pallet ID"].MaxLength);
			});
		}

		#endregion

		#region TestClient

		public void TestClient_MultipleClientFilters()
		{
			var client1 = Helper.CreateClient("CLIENT1");
			var client2 = Helper.CreateClient("CLIENT2");
			Factory.Save();

			var filter = GetNewFilterStripBusinessObject();

			var clientFilter1 = (ModuleGuidFilter)filter["Client"];
			clientFilter1.IsActive = true;
			clientFilter1.SqlComparisonOperator = SQLComparisonOperator.Equal;
			clientFilter1.Property = client1.PK;

			var clientFilter2 = filter.AddGuidFilterStrip("Client");
			clientFilter2.IsActive = true;
			clientFilter2.SqlComparisonOperator = SQLComparisonOperator.Equal;
			clientFilter2.Property = client2.PK;

			AssertEquals(client1.PK, (filter as DocketFilterBusinessObject).WD_OH_Client);

			clientFilter1.Property = ZGuid.Empty;
			AssertEquals(client2.PK, (filter as DocketFilterBusinessObject).WD_OH_Client);
		}

		#endregion

		#region Test Distribution Centre Filters

		protected virtual bool SupportsDistributionCentreFilters => false;

		#region TestFilterDistributionCentreName

		public void TestFilterDistributionCentreName()
		{
			if (SupportsDistributionCentreFilters)
			{
				TestFilterDistributionCentreNameCore();
			}
			else
			{
				AssertNull(DocketFilter["Distribution Center Name"]);
			}
		}

		protected virtual void TestFilterDistributionCentreNameCore()
		{
			Assert(true);
		}

		#endregion

		#region TestFilterDistributionCentre

		public void TestFilterDistributionCentre()
		{
			if (SupportsDistributionCentreFilters)
			{
				TestFilterDistributionCentreCore();
			}
			else
			{
				AssertNull(DocketFilter["Distribution Center"]);
			}
		}

		protected virtual void TestFilterDistributionCentreCore()
		{
			Assert(true);
		}

		public void TestFilterDistributionCentre_NotEqual()
		{
			TestFilterDistributionCentre_NotEqual_Core();
		}

		protected virtual void TestFilterDistributionCentre_NotEqual_Core()
		{
			Assert(true);
		}

		public void TestFilterDistributionCentre_IsBlank()
		{
			TestFilterDistributionCentre_IsBlank_Core();
		}

		protected virtual void TestFilterDistributionCentre_IsBlank_Core()
		{
			Assert(true);
		}

		public void TestFilterDistributionCentre_IsNotBlank()
		{
			TestFilterDistributionCentre_IsNotBlank_Core();
		}

		protected virtual void TestFilterDistributionCentre_IsNotBlank_Core()
		{
			Assert(true);
		}

		public void TestFilterDistributionCentre_HasComparisonOperators()
		{
			if (SupportsDistributionCentreFilters)
			{
				var filter = (ModuleGuidFilter)FilterStripBizO["Distribution Center"];
				AssertEquals("Should show comparison operators.", true, filter.HasComparisonOperator);
				AssertContainsExactElementsInAnyOrder(new[] { "exact", "not equal", "is blank", "is not blank" }, filter.ComparisonOperator_List.GetAllCodes());
			}
			else
			{
				AssertNull(DocketFilter["Distribution Center"]);
			}
		}

		#endregion

		#endregion

		#region Implementation

		protected FilterStripAsserter<TDocket> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<TDocket>(Factory, (d) => d.WD_DocketID)); }
		}

		FilterStripAsserter<TDocket> asserter;

		protected virtual void DocketAssert(bool d11, bool d12, bool d21, bool d22)
		{
			DocketCollection.AdditionalFilter = DocketFilter.Filter;

			var dockets = new[]
			{
				new { Include = d11, Docket = Docket11 },
				new { Include = d12, Docket = Docket12 },
				new { Include = d21, Docket = Docket21 },
				new { Include = d22, Docket = Docket22 },
			};
			AssertContainsExactElementsInAnyOrder(dockets.Where(o => o.Include).Select(o => GetDocketIDPlusExternalRef(o.Docket)), DocketCollection.Select(d => GetDocketIDPlusExternalRef(d)));
		}

		string GetDocketIDPlusExternalRef(WhsDocket docket) => $"{docket.WD_DocketID} - {docket.WD_ExternalReference}";

		protected virtual void SetupTestData()
		{
			Whs1 = Helper.CreateWarehouse("WH1", "A");
			Whs2 = Helper.CreateWarehouse("WH2", "A");
			Org1 = Helper.CreateClient("O1", "O1");
			Org2 = Helper.CreateClient("O2", "O2");
			Docket11 = CreateDocket(Org1, Whs1, "11");
			Docket12 = CreateDocket(Org1, Whs2, "12");
			Docket21 = CreateDocket(Org2, Whs1, "21");
			Docket22 = CreateDocket(Org2, Whs2, "22");
			DocketCollection = GetDocketCollection();
			Factory.Save();
		}

		protected virtual void SetupTestLineData()
		{
			Part11 = Helper.CreateProduct(Org1, "P11");
			Part12 = Helper.CreateProduct(Org1, "P12");
			Part21 = Helper.CreateProduct(Org2, "P21");
			Part22 = Helper.CreateProduct(Org2, "P22");
			Part = Helper.CreateProduct(Org1, "P");
			OrgPartRelation relation = Part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation.OU_OH = Org2.PK;

			Line111 = CreateDocketLine(Docket11, Part11, 10);
			Line112 = CreateDocketLine(Docket11, Part12, 10);
			Line121 = CreateDocketLine(Docket12, Part11, 10);
			Line122 = CreateDocketLine(Docket12, Part12, 10);
			Line123 = CreateDocketLine(Docket12, Part, 10);
			Line211 = CreateDocketLine(Docket21, Part21, 10);
			Line212 = CreateDocketLine(Docket21, Part22, 10);
			Line213 = CreateDocketLine(Docket21, Part, 10);
			Line221 = CreateDocketLine(Docket22, Part21, 10);
			Line222 = CreateDocketLine(Docket22, Part22, 10);
		}

		protected void SetupCommodityCodes()
		{
			Part11.OP_RH_NKCommodityCode = "11";
			Part12.OP_RH_NKCommodityCode = "12";
			Part21.OP_RH_NKCommodityCode = "21";
			Part22.OP_RH_NKCommodityCode = "22";
		}

		protected abstract WhsDocket CreateDocket(OrgHeader org, WhsWarehouse whs, string @ref);
		protected abstract WhsDocketLine CreateDocketLine(WhsDocket docket, OrgSupplierPart part, ZDecimal units);
		protected abstract ActiveBusinessObjectCollection<WhsDocket> GetDocketCollection();

		protected WhsWarehouse Whs1;
		protected WhsWarehouse Whs2;
		protected OrgHeader Org1;
		protected OrgHeader Org2;
		protected OrgSupplierPart Part;
		protected OrgSupplierPart Part11;
		protected OrgSupplierPart Part12;
		protected OrgSupplierPart Part21;
		protected OrgSupplierPart Part22;
		protected OrgSupplierPart Part33;
		protected WhsDocket Docket11;
		protected WhsDocket Docket12;
		protected WhsDocket Docket13;
		protected WhsDocket Docket21;
		protected WhsDocket Docket22;
		protected WhsDocketLine Line111;
		protected WhsDocketLine Line112;
		protected WhsDocketLine Line121;
		protected WhsDocketLine Line122;
		protected WhsDocketLine Line131;
		protected WhsDocketLine Line132;
		protected WhsDocketLine Line123;
		protected WhsDocketLine Line211;
		protected WhsDocketLine Line212;
		protected WhsDocketLine Line213;
		protected WhsDocketLine Line221;
		protected WhsDocketLine Line222;
		protected ActiveBusinessObjectCollection<WhsDocket> DocketCollection;

		protected WhsTestHelperFunctions Helper
		{
			get { return (helper = helper ?? new WhsTestHelperFunctions(Factory)); }
		}

		protected TDocketFilter DocketFilter
		{
			get { return docketFilter ?? (docketFilter = new TDocketFilter()); }
			set { docketFilter = value; }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TDocketFilter();
		}

		protected FilterStripBusinessObject FilterStripBizO
		{
			get { return filterStripBizO ?? (filterStripBizO = GetNewFilterStripBusinessObject()); }
		}

		WhsTestHelperFunctions helper;
		TDocketFilter docketFilter;
		FilterStripBusinessObject filterStripBizO;

		#endregion
	}

	#endregion
}
