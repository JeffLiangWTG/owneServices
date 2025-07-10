using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Business.Testing
{
	[TestedType(typeof(LandedLineCostItem))]
	sealed class LandedLineCostItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookupsType()
		{
			var item = Factory.New<LandedLineCostItem>();
			AssertType<LandedLineCostItemLookups>(item.Lookups);
		}

		public void TestRemoveEmptyRecordsOnSaving()
		{
			var testHelper = new TestHelper(Factory);
			var header = testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();
			var history = testHelper.GetHistory(header);
			var item1 = history.LandedLineCostItems.AddNew();
			var pk = item1.PK;
			item1.LZ_CostType = "ABC";
			item1.LZ_CostAmount = 69m;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var itemReloaded = factory2.Load<LandedLineCostItem>(pk);
			AssertNotNull(itemReloaded);

			var item2 = history.LandedLineCostItems.AddNew();
			pk = item2.PK;
			item2.LZ_CostType = "ABC";
			item2.LZ_CostAmount = 0m;
			Factory.Save();
			itemReloaded = factory2.Load<LandedLineCostItem>(pk);
			AssertNull("Should not have been saved as some properties were empty, so could not be reloaded", itemReloaded);

			var item3 = history.LandedLineCostItems.AddNew();
			pk = item3.PK;
			item3.LZ_CostType = "";
			item3.LZ_CostAmount = 69m;
			Factory.Save();
			itemReloaded = factory2.Load<LandedLineCostItem>(pk);
			AssertNull("Should not have been saved as some properties were empty, so could not be reloaded", itemReloaded);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var testHelper = new TestHelper(factory);
			var header = testHelper.GetLCHeaderWithDistributionObjectsPluggedIn();
			var history = testHelper.GetHistory(header);
			return history.LandedLineCostItems.AddNew("ENT", 69m);
		}
	}

	[TestedType(typeof(LandedLineCostItem))]
	sealed class LandedLineCostItemClusterKeyWorkerMandatoryTest : ClusterKeyWorkerMandatoryTest
	{
		#region Overrides of ClusterKeyEntityTest

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var landedLineCostItem = ((LandedCostHistory)NewParentObject()).LandedLineCostItems.AddNew();
			landedLineCostItem.LZ_CostType = "ABC";
			landedLineCostItem.LZ_CostAmount = 69m;

			return landedLineCostItem;
		}

		#endregion

		#region Overrides of ClusterKeyWorkerMandatoryTest

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var jobDec = (EnterpriseBusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			var header = Factory.New<LandedCostHeader>();
			header.LT_ParentID = jobDec.PK;
			header.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			var invHeader = (EnterpriseBusinessObject)Factory.New<Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invHeader[JobComInvoiceHeaderSchema.JZ_JE] = jobDec.PK;
			var invLine = (EnterpriseBusinessObject)Factory.New<Integration.Customs.IBaseJobComInvoiceLine>();
			invLine[JobComInvoiceLineSchema.JI_JZ] = invHeader.PK;

			var history = header.Histories.AddNew();
			history.LH_ParentID = invLine.PK;
			history.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			return history;
		}

		#endregion
	}
}
