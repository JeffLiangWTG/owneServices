using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(WhsInventoryHeldCodeFilterBusinessObject))]
	public class InventoryHeldFilterTestCase : FilterStripBusinessObjectTestCase
	{
		public void TestCodeFilter()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var heldCode1 = Helper.CreateInventoryHeldCode("TEST1", "Blah1");
			var heldCode2 = Helper.CreateInventoryHeldCode("TEST2", "Blah2");
			var heldCode3 = Helper.CreateInventoryHeldCode("TEST3", "TEST3 for client 1", client1.PK);
			var heldCode4 = Helper.CreateInventoryHeldCode("TEST4", "TEST4 description", client1.PK);
			var heldCode5 = Helper.CreateInventoryHeldCode("TEST5", "TEST5 for client 2", client2.PK);
			var heldCode6 = Helper.CreateInventoryHeldCode("TEST6", "TEST6 description", client2.PK);
			Factory.Save();
			Asserter.AddToScope(heldCode1, heldCode2, heldCode3, heldCode4, heldCode5, heldCode6);

			var filterBizO1 = GetNewFilterStripBusinessObject();
			var moduleFilter1 = (ModuleTextFilter)filterBizO1[WhsInventoryHeldCodeFilterBusinessObject.Schema.Code];
			moduleFilter1.IsActive = true;
			moduleFilter1.Property = "TEST1";
			Asserter.AssertMatches("Shoud  return only heldCode1", filterBizO1.Filter, heldCode1);

			moduleFilter1.Property = "TEST2";
			Asserter.AssertMatches("Shoud return only heldCode2", filterBizO1.Filter, heldCode2);

			moduleFilter1.Property = "TEST3";
			Asserter.AssertMatches("Shoud return only heldCode3", filterBizO1.Filter, heldCode3);

			moduleFilter1.Property = "TEST4";
			Asserter.AssertMatches("Shoud return only heldCode4", filterBizO1.Filter, heldCode4);

			moduleFilter1.Property = "TEST5";
			Asserter.AssertMatches("Shoud return only heldCode5", filterBizO1.Filter, heldCode5);

			moduleFilter1.Property = "TEST6";
			Asserter.AssertMatches("Shoud return only heldCode6", filterBizO1.Filter, heldCode6);
		}

		public void TestDescriptionFilter()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var heldCode1 = Helper.CreateInventoryHeldCode("TEST1", "Blah1");
			var heldCode2 = Helper.CreateInventoryHeldCode("TEST2", "Blah2");
			var heldCode3 = Helper.CreateInventoryHeldCode("TEST3", "TEST3 for client 1", client1.PK);
			var heldCode4 = Helper.CreateInventoryHeldCode("TEST4", "TEST4 description", client1.PK);
			var heldCode5 = Helper.CreateInventoryHeldCode("TEST5", "TEST3 for client 2", client2.PK);
			var heldCode6 = Helper.CreateInventoryHeldCode("TEST6", "TEST4 description", client2.PK);
			var heldCode7 = Helper.CreateInventoryHeldCode("TEST7", "Blah2", client2.PK);
			Factory.Save();
			Asserter.AddToScope(heldCode1, heldCode2, heldCode3, heldCode4, heldCode5, heldCode6, heldCode7);

			var filterBizO = GetNewFilterStripBusinessObject();
			var moduleFilter = (ModuleTextFilter)filterBizO[WhsInventoryHeldCodeFilterBusinessObject.Schema.Description];
			moduleFilter.IsActive = true;

			moduleFilter.Property = "Blah1";
			Asserter.AssertMatches("Shoud  return only heldCode1", filterBizO.Filter, heldCode1);

			moduleFilter.Property = "Blah2";
			Asserter.AssertMatches("Shoud return heldCode2 and heldCode7", filterBizO.Filter, heldCode2, heldCode7);

			moduleFilter.Property = "TEST3 for client 1";
			Asserter.AssertMatches("Shoud return only heldCode3", filterBizO.Filter, heldCode3);

			moduleFilter.Property = "TEST4 description";
			Asserter.AssertMatches("Shoud return heldCode4 and heldCode6", filterBizO.Filter, heldCode4, heldCode6);
		}

		public void TestIsSystemFilter()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var heldCode1 = Helper.CreateInventoryHeldCode("TEST1", "Blah1", null, true);
			var heldCode2 = Helper.CreateInventoryHeldCode("TEST2", "Blah2", null, false);
			var heldCode3 = Helper.CreateInventoryHeldCode("TEST3", "TEST3 for client 1", client1.PK, true);
			var heldCode4 = Helper.CreateInventoryHeldCode("TEST4", "TEST4 for client 2", client2.PK, false);
			Factory.Save();
			Asserter.AddToScope(heldCode1, heldCode2, heldCode3, heldCode4);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsInventoryHeldCodes))
			{
				var filterBizO = module.FilterBusinessObject;
				var moduleFilter = (ModuleTextFilter)filterBizO["Is System Defined"];
				moduleFilter.IsActive = true;
				moduleFilter.Property = "System";
				Asserter.AssertMatches("Shoud return heldCode1 and heldCode3", filterBizO.Filter, heldCode1, heldCode3);

				moduleFilter.Property = "Not System";
				Asserter.AssertMatches("Shoud return heldCode2 and heldCode4", filterBizO.Filter, heldCode2, heldCode4);

				moduleFilter.Property = "All";
				Asserter.AssertMatches("Shoud return heldCode1, heldCode2, heldCode3 and heldCode4", filterBizO.Filter,
					heldCode1, heldCode2, heldCode3, heldCode4);
			}
		}

		public void TestClientFilterWithEqualOperator()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var heldCode1 = Helper.CreateInventoryHeldCode("TEST1", "Blah1");
			var heldCode2 = Helper.CreateInventoryHeldCode("TEST2", "Blah2");
			var heldCode3 = Helper.CreateInventoryHeldCode("TEST3", "TEST3 for client 1", client1.PK);
			var heldCode4 = Helper.CreateInventoryHeldCode("TEST4", "TEST4 description", client1.PK);
			var heldCode5 = Helper.CreateInventoryHeldCode("TEST5", "TEST5 for client 2", client2.PK);
			var heldCode6 = Helper.CreateInventoryHeldCode("TEST6", "TEST6 description", client2.PK);
			var heldCode7 = Helper.CreateInventoryHeldCode("TEST7", "TEST7 for client 2", client2.PK);
			Factory.Save();
			Asserter.AddToScope(heldCode1, heldCode2, heldCode3, heldCode4, heldCode5, heldCode6, heldCode7);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsInventoryHeldCodes))
			{
				var filterBizO = module.FilterBusinessObject;
				var moduleFilter = (ModuleGuidFilter)filterBizO["Client"];
				moduleFilter.IsActive = true;
				AssertEquals("ClientPK: ", ZGuid.Empty, moduleFilter.Property);
				AssertEquals(FilterCategories.Organisations, moduleFilter.Category);
				Asserter.AssertMatches("Should return heldCode1, heldCode2, heldCode3, heldCode4, heldCode5, heldCode6 and heldCode7", moduleFilter,
					heldCode1, heldCode2, heldCode3, heldCode4, heldCode5, heldCode6, heldCode7);

				moduleFilter.Property = client1.PK;
				Asserter.AssertMatches("Shoud return heldCode3 and heldCode4", filterBizO.Filter,
					heldCode3, heldCode4);

				moduleFilter.Property = client2.PK;
				Asserter.AssertMatches("Shoud return heldCode5, heldCode6 and heldCode7", filterBizO.Filter,
					heldCode5, heldCode6, heldCode7);
			}
		}

		public void TestClientFilterWithNotEqualOperator()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var heldCode1 = Helper.CreateInventoryHeldCode("TEST1", "Blah1");
			var heldCode2 = Helper.CreateInventoryHeldCode("TEST2", "Blah2");
			var heldCode3 = Helper.CreateInventoryHeldCode("TEST3", "TEST3 for client 1", client1.PK);
			var heldCode4 = Helper.CreateInventoryHeldCode("TEST4", "TEST4 description", client1.PK);
			var heldCode5 = Helper.CreateInventoryHeldCode("TEST5", "TEST5 for client 2", client2.PK);
			var heldCode6 = Helper.CreateInventoryHeldCode("TEST6", "TEST6 description", client2.PK);
			var heldCode7 = Helper.CreateInventoryHeldCode("TEST7", "TEST7 for client 2", client2.PK);
			Factory.Save();
			Asserter.AddToScope(heldCode1, heldCode2, heldCode3, heldCode4, heldCode5, heldCode6, heldCode7);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsInventoryHeldCodes))
			{
				var filterBizO = module.FilterBusinessObject;
				var moduleFilter = (ModuleGuidFilter)filterBizO["Client"];
				moduleFilter.IsActive = true;
				moduleFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				AssertEquals("ClientPK: ", ZGuid.Empty, moduleFilter.Property);
				AssertEquals(FilterCategories.Organisations, moduleFilter.Category);
				Asserter.AssertMatches("Should return heldCode1, heldCode2, heldCode3, heldCode4, heldCode5, heldCode6 and heldCode7", moduleFilter,
					heldCode1, heldCode2, heldCode3, heldCode4, heldCode5, heldCode6, heldCode7);

				moduleFilter.Property = client1.PK;
				Asserter.AssertMatches("Shoud return heldCode1, heldCode2, heldCode5, heldCode6 and heldCode7", filterBizO.Filter,
					heldCode1, heldCode2, heldCode5, heldCode6, heldCode7);
			}
		}

		public void TestClientFilterWithIsBlankOperator()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var heldCode1 = Helper.CreateInventoryHeldCode("TEST1", "Blah1");
			var heldCode2 = Helper.CreateInventoryHeldCode("TEST2", "Blah2");
			var heldCode3 = Helper.CreateInventoryHeldCode("TEST3", "TEST3 for client 1", client1.PK);
			var heldCode4 = Helper.CreateInventoryHeldCode("TEST4", "TEST4 description", client1.PK);
			var heldCode5 = Helper.CreateInventoryHeldCode("TEST5", "TEST5 for client 2", client2.PK);
			var heldCode6 = Helper.CreateInventoryHeldCode("TEST6", "TEST6 description", client2.PK);
			var heldCode7 = Helper.CreateInventoryHeldCode("TEST7", "TEST7 for client 2", client2.PK);
			Factory.Save();
			Asserter.AddToScope(heldCode1, heldCode2, heldCode3, heldCode4, heldCode5, heldCode6, heldCode7);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsInventoryHeldCodes))
			{
				var filterBizO = module.FilterBusinessObject;
				var moduleFilter = (ModuleGuidFilter)filterBizO["Client"];
				moduleFilter.IsActive = true;
				moduleFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				Asserter.AssertMatches("Shoud return heldCode1 and heldCode2", filterBizO.Filter,
					heldCode1, heldCode2);
			}
		}

		public void TestClientFilterWithIsNotBlankOperator()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var heldCode1 = Helper.CreateInventoryHeldCode("TEST1", "Blah1");
			var heldCode2 = Helper.CreateInventoryHeldCode("TEST2", "Blah2");
			var heldCode3 = Helper.CreateInventoryHeldCode("TEST3", "TEST3 for client 1", client1.PK);
			var heldCode4 = Helper.CreateInventoryHeldCode("TEST4", "TEST4 description", client1.PK);
			var heldCode5 = Helper.CreateInventoryHeldCode("TEST5", "TEST5 for client 2", client2.PK);
			var heldCode6 = Helper.CreateInventoryHeldCode("TEST6", "TEST6 description", client2.PK);
			var heldCode7 = Helper.CreateInventoryHeldCode("TEST7", "TEST7 for client 2", client2.PK);
			Factory.Save();
			Asserter.AddToScope(heldCode1, heldCode2, heldCode3, heldCode4, heldCode5, heldCode6, heldCode7);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsInventoryHeldCodes))
			{
				var filterBizO = module.FilterBusinessObject;
				var moduleFilter = (ModuleGuidFilter)filterBizO["Client"];
				moduleFilter.IsActive = true;
				moduleFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				Asserter.AssertMatches("Shoud return heldCode3, heldCode4, heldCode5, heldCode6 and heldCode7", filterBizO.Filter,
					heldCode3, heldCode4, heldCode5, heldCode6, heldCode7);
			}
		}

		#region Implementation

		protected WhsTestHelperFunctions Helper => helper = helper ?? new WhsTestHelperFunctions(Factory);
		WhsTestHelperFunctions helper;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WhsInventoryHeldCodeFilterBusinessObject();
		}

		FilterStripAsserter<WhsInventoryHeldCode> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsInventoryHeldCode>(Factory, w => w.WHC_Code));
		FilterStripAsserter<WhsInventoryHeldCode> asserter;

		#endregion
	}
}
