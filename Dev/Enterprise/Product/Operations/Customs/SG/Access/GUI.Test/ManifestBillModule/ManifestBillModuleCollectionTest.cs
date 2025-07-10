using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Module;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(ManifestBillModuleCollection))]
	class ManifestBillModuleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultFilter()
		{
			var manifestHeader1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			manifestHeader1.AMA_RN_NKCountry = "SG";
			var bill1 = manifestHeader1.Bills.AddNew();
			bill1.ABL_IsActive = false;
			bill1.ABL_BolType = "STD";
			Factory.Save();
			var collection = new ASYCUDAManifestBillModuleCollection(Factory);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ZGuid>(), collection.Select(x => x.PK));
			var collection2 = new ManifestBillModuleCollection(Factory);
			collection2.Load();
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { bill1.PK }, collection2.Select(x => x.PK));
		}

		public void TestFetchForView()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "MAM001";
			header1.AMA_MasterBill = "MN001";
			header1.AMA_TransportMode = "SEA";
			header1.RegistrationNumber = "RN001";
			var bill1 = header1.Bills.AddNew();
			bill1.ABL_BillNumber = "BIL001";
			var billEntryNumber1 = bill1.CustomsEntryNumbers.AddNew();
			billEntryNumber1.CE_EntryType = "ASY";
			billEntryNumber1.CE_EntryNum = "BEN001";
			bill1.CycleDate = new ZDateTime(2018, 05, 08);
			bill1.CycleNumber = "CN001";
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "MAM002";
			header2.AMA_MasterBill = "MN002";
			header2.AMA_TransportMode = "SEA";
			header2.RegistrationNumber = "RN002";
			var bill2 = header2.Bills.AddNew();
			bill2.ABL_BillNumber = "BIL002";
			var billEntryNumber2 = bill2.CustomsEntryNumbers.AddNew();
			billEntryNumber2.CE_EntryType = "ASY";
			billEntryNumber2.CE_EntryNum = "BEN002";
			bill2.CycleDate = new ZDateTime(2018, 05, 08);
			bill2.CycleNumber = "CN002";
			Factory.Save();
			var factory = new BusinessObjectFactory();
			using (factory.SuspendCustomsValuesFetchHint(typeof(AsycudaBill)))
			using (factory.SuspendCustomsValuesFetchHint(typeof(AsycudaManifestHeader)))
			{
				var collection = new ManifestBillModuleCollection(factory);
				collection.Load();
				AssertEquals(2, collection.Count);
				AssertEquals(1, factory.ActiveTableFetchHints);
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				var strategy = new ASYCUDAManifestBillModuleCollectionFetchStrategy(collection);
				var tc1 = new TableColumn("", "Header+AMA_JobReference");
				var tc2 = new TableColumn("", "Header+AMA_MasterBill");
				var tc3 = new TableColumn("", "ABL_BillNumber");
				var tc4 = new TableColumn("", "CusEntryNumber+CE_EntryStatus");
				var tc5 = new TableColumn("", "Header+AMA_ManifestType");
				var tc6 = new TableColumn("", "ABL_BillIssuer");
				var tc7 = new TableColumn("", "CustomsEntryNumber");
				var tc8 = new TableColumn("", "Header+CycleDate");
				var tc9 = new TableColumn("", "CycleDate");
				var tc10 = new TableColumn("", "StatusDescription");
				strategy.FetchForView(collection.ToArray(), new[] { tc1 });
				AssertEquals("Added 1 extra fetch hint.", 0, factory.ActiveTableFetchHints);
				AssertEquals("Bill was loaded, so fetch hint is 0.", 0, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				var count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc2 });
				AssertEquals("Added 1 extra fetch hint.", count + 1, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc3 });
				AssertEquals("No new fetch hint needed.", count, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc4 });
				AssertEquals("Added 1 extra fetch hint.", count + 1, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc5 });
				AssertEquals("Added 1 extra fetch hint.", count, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc6 });
				AssertEquals("No new fetch hint needed.", count, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc7 });
				AssertEquals("No new fetch hint needed.", count, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc8 });
				AssertEquals("Add 1 fetch hint for GenAddOnColumn, reduce 1 fetch hint for header country after header country is loaded.", count, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(GenAddOnColumnSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc9 });
				AssertEquals("No new fetch hint needed.", count + 1, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(GenAddOnColumnSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc10 });
				AssertEquals("Added 1 extra fetch hint.", count + 1, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(GenAddOnColumnSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(StmALogSchema.Constants.TableName));
			}
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ManifestBillModuleCollection(Factory);
		}
		#endregion
	}
}
