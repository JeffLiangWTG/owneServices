using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgTransactionsModuleFilter))]
	sealed class OrgTransactionsModuleFilterTest : NonPersistentBusinessObjectTestCase
	{
		#region Test Xml Serialise

		public void TestDeserializePropertiesFromToXml()
		{
			OrgTransactionsModuleFilter filter = new OrgTransactionsModuleFilter("min trac");

			DummyFilterStripBusinessObject filterStripBizO = new DummyFilterStripBusinessObject();

			filterStripBizO.AddModuleFilterForTest(filter);

			FilterStrip strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.NumberOfTransactions = 7;

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = "";
			strip.Delete();

			OrgTransactionsModuleFilter loadedFilter = (OrgTransactionsModuleFilter)filterStripBizO[filter.Description];

			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals(7, loadedFilter.NumberOfTransactions);
		}

		#endregion

		#region OrgTransactionsModuleFilter

		OrgTransactionsModuleFilter OrgTransactionsModuleFilterForTest
		{
			get
			{
				if (fOrgTransactionsModuleFilterForTest == null)
				{
					fOrgTransactionsModuleFilterForTest = new OrgTransactionsModuleFilter("Transactions");
				}

				return fOrgTransactionsModuleFilterForTest;
			}
		}

		OrgTransactionsModuleFilter fOrgTransactionsModuleFilterForTest;

		public void TestXTransactionInYMonth()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			AccTransactionHeader acc1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader acc2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader acc3 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader acc4 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader acc5 = Factory.NewWithValidTestData<AccTransactionHeader>();

			//valid org1 transaction
			acc1.AH_OH = org1.PK;
			acc1.AH_PostDate = new ZDateTime(2005, 2, 2);

			//valid org1 transaction
			acc2.AH_OH = org1.PK;
			acc2.AH_PostDate = new ZDateTime(2005, 2, 22);

			//Invalid org1 transaction
			acc3.AH_OH = org1.PK;
			acc3.AH_PostDate = new ZDateTime(2006, 2, 12);

			//valid org2 transaction
			acc4.AH_OH = org2.PK;
			acc4.AH_PostDate = new ZDateTime(2005, 2, 12);

			//Invalid org2 transaction				
			acc5.AH_OH = org2.PK;
			acc5.AH_PostDate = new ZDateTime(2006, 2, 12);

			Factory.Save();

			OrgTransactionsModuleFilterForTest.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			OrgTransactionsModuleFilterForTest.Property1 = new ZDateTime(2005, 1, 1);
			OrgTransactionsModuleFilterForTest.Property2 = new ZDateTime(2005, 3, 3);

			//has minimum 2 transaction
			OrgTransactionsModuleFilterForTest.NumberOfTransactions = 2;
			OrgTransactionsModuleFilterForTest.IsActive = true;

			ZQuery findOrgQuery = new ZQuery(OrgHeaderSchema.PK, new ZGuid[] { org1.PK, org2.PK });

			OrgHeaderCollection orgCollection = new OrgHeaderCollection(Factory, findOrgQuery);
			orgCollection.Load();
			AssertEquals("Precondition: 2 orgs are loaded", 2, orgCollection.Count);

			findOrgQuery.AddToFilter(OrgTransactionsModuleFilterForTest.Query);
			orgCollection = new OrgHeaderCollection(Factory, findOrgQuery);
			orgCollection.Load();

			Assert("Expect collection to contain org1", orgCollection.Contains(org1));
			Assert("Expect collection not to contain org2", !orgCollection.Contains(org2));
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgTransactionsModuleFilter("moo");
		}
	}
}
