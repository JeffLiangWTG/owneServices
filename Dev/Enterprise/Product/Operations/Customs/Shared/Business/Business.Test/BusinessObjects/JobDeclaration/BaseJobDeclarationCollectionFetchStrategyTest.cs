using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class BaseJobDeclarationCollectionFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForViewActiveTableFetchHints()
		{
			var dec1 = Factory.New<BaseJobDeclaration>();
			var bill1 = dec1.Bills.AddNew();
			var packingGroup1 = bill1.PackingGroups.AddNew();
			var package1 = packingGroup1.Packages.AddNew();
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.MergedLines.AddNew();
			var container1 = dec1.CusContainers.AddNew();
			var dec2 = Factory.New<BaseJobDeclaration>();
			var bill2 = dec2.Bills.AddNew();
			var packingGroup2 = bill2.PackingGroups.AddNew();
			var package2 = packingGroup2.Packages.AddNew();
			var entry2 = dec2.CustomsEntryHeaders.AddNew();
			var entryLine2 = entry2.MergedLines.AddNew();
			var container2 = dec2.CusContainers.AddNew();
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var collection = new BaseJobDeclarationCollection(factory);
			collection.Load();
			AssertEquals(2, collection.Count);

			AssertEquals(0, factory.ActiveFetchHintsForTable(CusEntryHeaderSchema.Constants.TableName));
			AssertEquals(0, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
			AssertEquals(0, factory.ActiveFetchHintsForTable(CusContainerSchema.Constants.TableName));

			int count = factory.ActiveTableFetchHints;
			var strategy = new BaseJobDeclarationCollectionFetchStrategy(collection);
			var tc1 = new TableColumn(JobDeclarationSchema.Constants.TableName, BaseJobDeclaration.Schema.DeclarationNumber);
			var tc2 = new TableColumn(JobDeclarationSchema.Constants.TableName, BaseJobDeclaration.Schema.EarliestCustomsEntryIssueDate);
			strategy.FetchForView(collection.ToArray(), new[] { tc1, tc2 });
			AssertEquals("1 extra fetch hint should be added after FetchForView of declaration collection was called", count + 1, factory.ActiveTableFetchHints);
			AssertEquals(0, factory.ActiveFetchHintsForTable(CusEntryHeaderSchema.Constants.TableName));
			AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));

			count = factory.ActiveTableFetchHints;
			strategy.FetchForView(collection.ToArray(), new[] { tc1 });
			AssertEquals("No new fetch hint needed as DeclarationNumber should already added the fetch hint required", count, factory.ActiveTableFetchHints);
			AssertEquals(0, factory.ActiveFetchHintsForTable(CusEntryHeaderSchema.Constants.TableName));
			AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));

			count = factory.ActiveTableFetchHints;
			strategy.FetchForView(collection.ToArray(), new[] { tc2 });
			AssertEquals("No new fetch hint needed as EarliestCustomsEntryIssueDate should already added the fetch hint required", count, factory.ActiveTableFetchHints);
			AssertEquals(0, factory.ActiveFetchHintsForTable(CusEntryHeaderSchema.Constants.TableName));
			AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));

			count = factory.ActiveTableFetchHints;
			var tc3 = new TableColumn(JobDeclarationSchema.Constants.TableName, BaseJobDeclaration.Schema.FreightContainerMode);
			strategy.FetchForView(collection.ToArray(), new[] { tc3 });
			AssertEquals("1 extra fetch hint should be added after FetchForView of declaration collection was called", count + 1, factory.ActiveTableFetchHints);
			AssertEquals("2 for CO_ClusterKey", 2, factory.ActiveFetchHintsForTable(CusContainerSchema.Constants.TableName));

			count = factory.ActiveTableFetchHints;
			strategy.FetchForView(collection.ToArray(), new[] { tc3 });
			AssertEquals("No new fetch hint needed as FreightContainerMode should already added the fetch hint required", count, factory.ActiveTableFetchHints);
			AssertEquals("2 for CO_ClusterKey", 2, factory.ActiveFetchHintsForTable(CusContainerSchema.Constants.TableName));

			count = factory.ActiveTableFetchHints;
			var tc4 = new TableColumn(JobDeclarationSchema.Constants.TableName, BaseJobDeclaration.Schema.PackagesActualPackageCount);
			strategy.FetchForView(collection.ToArray(), new[] { tc4 });
			AssertEquals("1 extra fetch hint should be added after FetchForView of declaration collection was called", count + 1, factory.ActiveTableFetchHints);
			AssertEquals("2 for CO_ClusterKey", 2, factory.ActiveFetchHintsForTable(CusContainerSchema.Constants.TableName));

			count = factory.ActiveTableFetchHints;
			strategy.FetchForView(collection.ToArray(), new[] { tc4 });
			AssertEquals("No new fetch hint needed as PackagesActualPackageCount should already added the fetch hint required", count, factory.ActiveTableFetchHints);
			AssertEquals("2 for CO_ClusterKey", 2, factory.ActiveFetchHintsForTable(CusContainerSchema.Constants.TableName));
		}
	}
}
