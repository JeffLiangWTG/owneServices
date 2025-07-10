using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class LandedCostHistoryCollectionFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView()
		{
			var testDec = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			var landedCostHeader = Factory.New<LandedCostHeader>();
			landedCostHeader.LT_ParentID = testDec.PK;
			landedCostHeader.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			AddLandedCostHistoryAndItems(landedCostHeader);
			AddLandedCostHistoryAndItems(landedCostHeader);
			Factory.Save();

			var tc1 = new TableColumn("", "RoundedPerUnitCustomsDisbursementCharges");
			var tc2 = new TableColumn("", "RoundedPerUnitLandingCost");
			var tc3 = new TableColumn("", "RoundedPerUnitTotalCost");
			var tc4 = new TableColumn("", "RoundedSellPrice1ExGST");
			var tc5 = new TableColumn("", "TotalCost");
			var tc6 = new TableColumn("", "LH_LandedCostGroup1");
			var tc7 = new TableColumn("", "RoundedSellPrice1IncGST");
			var tc8 = new TableColumn("", "InvoiceQuantity");

			AssertLandedLineCostItemHints("tc1 is LandedLineCostItem related column", landedCostHeader, 1, 2, new[] { tc1 });
			AssertLandedLineCostItemHints("tc2 is LandedLineCostItem related column", landedCostHeader, 1, 2, new[] { tc2 });
			AssertLandedLineCostItemHints("tc3 is LandedLineCostItem related column", landedCostHeader, 1, 2, new[] { tc3 });
			AssertLandedLineCostItemHints("tc4 is LandedLineCostItem related column", landedCostHeader, 1, 2, new[] { tc4 });
			AssertLandedLineCostItemHints("tc5 is LandedLineCostItem related column", landedCostHeader, 1, 2, new[] { tc5 });
			AssertLandedLineCostItemHints("tc6 is LandedLineCostItem related column", landedCostHeader, 1, 2, new[] { tc6 });
			AssertLandedLineCostItemHints("tc7 is LandedLineCostItem related column", landedCostHeader, 1, 2, new[] { tc7 });
			AssertLandedLineCostItemHints("tc8 is not LandedLineCostItem related column", landedCostHeader, 0, 0, new[] { tc8 });
			AssertLandedLineCostItemHints("tc1-8 are multi columns include LandedLineCostItem related column", landedCostHeader, 1, 2, new[] { tc1, tc2, tc3, tc4, tc5, tc6, tc7, tc8 });
		}

		void AssertLandedLineCostItemHints(string message, LandedCostHeader landedCostHeader, int extraFetchHints, int landedLineCostItemHints, TableColumn[] columns)
		{
			var newFactory = new BusinessObjectFactory();
			var header = newFactory.Load<LandedCostHeader>(landedCostHeader.PK);
			var collection = new LandedCostHistoryCollection(header);
			AssertEquals(2, collection.Count);
			var factory = collection.Factory;
			using (factory.SuspendCustomsValuesFetchHint(typeof(LandedCostHistory)))
			using (factory.SuspendCustomsValuesFetchHint(typeof(LandedCostHeader)))
			{
				var strategy = new LandedCostHistoryCollectionFetchStrategy(collection);
				var count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), columns);

				CombineAssertions(() =>
				{
					AssertEquals(message + ": Added 1 extra fetch hint.", count + extraFetchHints, factory.ActiveTableFetchHints);
					AssertEquals(message + ": LandedCostHistory was loaded, so fetch hint is 0.", 0,
						factory.ActiveFetchHintsForTable(LandedCostHistorySchema.Constants.TableName));
					AssertEquals(message + ": LandedCostHeader was loaded, so fetch hint is 0.", 0,
						factory.ActiveFetchHintsForTable(LandedCostHeaderSchema.Constants.TableName));
					AssertEquals(message + ": LandedLineCostItem hints added.", landedLineCostItemHints,
						factory.ActiveFetchHintsForTable(LandedLineCostItemSchema.Constants.TableName));
				});
			}
		}

		LandedCostHistory AddLandedCostHistoryAndItems(LandedCostHeader landedCostHeader)
		{
			var jobDec = (EnterpriseBusinessObject)landedCostHeader.Parent;
			var invHeader = (EnterpriseBusinessObject)Factory.New<Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invHeader[JobComInvoiceHeaderSchema.JZ_JE] = jobDec.PK;
			var invLine = (EnterpriseBusinessObject)Factory.New<Integration.Customs.IBaseJobComInvoiceLine>();
			invLine[JobComInvoiceLineSchema.JI_JZ] = invHeader.PK;

			var landedCostHistory = landedCostHeader.Histories.AddNew();
			landedCostHistory.LH_ParentID = invLine.PK;
			landedCostHistory.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			landedCostHistory.LandedLineCostItems.AddNew("1", 0.5m);
			landedCostHistory.LandedLineCostItems.AddNew("2", 0.5m);

			return landedCostHistory;
		}
	}
}
