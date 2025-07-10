using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class LandedCostHistoryFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForLoadChildEditableObjects()
		{
			var testDec = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			var landedCostHeader = Factory.New<LandedCostHeader>();
			landedCostHeader.LT_ParentID = testDec.PK;
			landedCostHeader.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			var landedCostHistory = AddLandedCostHistoryAndItems(landedCostHeader);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var header = newFactory.Load<LandedCostHeader>(landedCostHeader.PK);
			newFactory.ResetDatabaseLoadCount();
			header.LoadChildEditableObjects();

			AssertEquals("FetchForLoadChildEditableObjects Hints should be added for LandedLineCostItem", 1, newFactory.GetTableHitCount(LandedLineCostItemSchema.Constants.TableName));
			AssertEquals("FetchForLoadChildEditableObjects Hints should be added for LandedLineCostItem", 1, newFactory.GetTableHitCount(LandedLineCostItemSchema.Constants.TableName));
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory) => new LandedCostHistoryCollection(Factory.New<LandedCostHeader>());

		LandedCostHistory AddLandedCostHistoryAndItems(LandedCostHeader landedCostHeader)
		{
			var jobDec = (EnterpriseBusinessObject)landedCostHeader.Parent;
			var invHeader = (EnterpriseBusinessObject)Factory.New<Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invHeader[JobComInvoiceHeaderSchema.JZ_JE] = jobDec.PK;
			var invLine = (EnterpriseBusinessObject)Factory.New<Integration.Customs.IBaseJobComInvoiceLine>();
			invLine[JobComInvoiceLineSchema.JI_JZ] = invHeader.PK;

			var landedCostHistory1 = landedCostHeader.Histories.AddNew();
			landedCostHistory1.LH_ParentID = invLine.PK;
			landedCostHistory1.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			var landedCostItem1 = landedCostHistory1.LandedLineCostItems.AddNew();
			landedCostItem1.LZ_LH = landedCostHistory1.PK;
			landedCostItem1.LZ_CostAmount = 0.5m;

			var landedCostItem2 = landedCostHistory1.LandedLineCostItems.AddNew();
			landedCostItem2.LZ_LH = landedCostHistory1.PK;
			landedCostItem2.LZ_CostAmount = 0.5m;

			return landedCostHistory1;
		}
	}
}
