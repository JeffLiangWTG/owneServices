using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsSerialNumberPivotCollectionFetchStrategyTest : TestCaseWithFactory
	{
		#region TestWhsSerialNumberPivotCollectionFetchStrategy_AddFetchHintsForParentDocketLines

		public void TestWhsSerialNumberPivotCollectionFetchStrategy_AddFetchHintsForParentDocketLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1m, true, false);
			var serialNumbers = receive.Lines[0].SerialNumbers;
			AddSerialNumber(serialNumbers, "SN1");
			AddSerialNumber(serialNumbers, "SN2");
			Factory.Save();

			var viewFactory = new BusinessObjectFactory();
			var collection = viewFactory.Load<WhsReceiveLine>(receive.Lines[0].PK).SerialNumbers;
			AssertEquals("Precondition", 0, viewFactory.ActiveFetchHintsForTable(WhsSerialNumberSchema.Constants.TableName));
			CallFetchForView(collection);

			AssertEquals("Should add fetch hint when accessing column.", 1, viewFactory.ActiveFetchHintsForTable(WhsSerialNumberSchema.Constants.TableName));

			AssertEquals("It should return true because the Strategy has already added fetch hints.", true,
				viewFactory.GetCachedValue<bool>($"WhsSerialNumberPivotCollectionFetchStrategy|AddFetchHintsForParentDocketLines|{receive.PK}", () => throw new Exception("Should not call it is already cached.")));
		}

		#endregion

		#region TestWhsSerialNumberPivotCollectionFetchStrategy_AddFetchHintsForParentAsnLines

		public void TestWhsSerialNumberPivotCollectionFetchStrategy_AddFetchHintsForParentAsnLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1m, true, false);
			var asnLine1 = helper.CreateAsnLine(receive, data.Part1, 2m);
			var sn1PK = AddSerialNumber(receive.Lines[0].SerialNumbers, "SN1");
			var sn2PK = AddSerialNumber(receive.Lines[0].SerialNumbers, "SN2");

			AddSerialNumber(asnLine1, sn1PK);
			AddSerialNumber(asnLine1, sn2PK);
			Factory.Save();

			var viewFactory = new BusinessObjectFactory();
			var collection = viewFactory.Load<WhsAsnLine>(asnLine1.PK).SerialNumbers;
			AssertEquals("Precondition", 0, viewFactory.ActiveFetchHintsForTable(WhsSerialNumberSchema.Constants.TableName));
			CallFetchForView(collection);

			AssertEquals("Should add fetch hint when accessing column.", 1, viewFactory.ActiveFetchHintsForTable(WhsSerialNumberSchema.Constants.TableName));

			AssertEquals("It should return true because the Strategy has already added fetch hints.", true,
				viewFactory.GetCachedValue<bool>($"WhsSerialNumberPivotCollectionFetchStrategy|AddFetchHintsForParentAsnLines|{receive.PK}", () => throw new Exception("Should not call it is already cached.")));
		}

		static void AddSerialNumber(WhsAsnLine asnLine, ZGuid snPK)
		{
			var snPivot = asnLine.SerialNumbers.AddNew();
			snPivot.WSV_WSN_SerialNumber = snPK;
		}

		#endregion

		#region TestWhsSerialNumberPivotCollectionFetchStrategy_AddFetchHintsForView_NotSaved

		public void TestWhsSerialNumberPivotCollectionFetchStrategy_AddFetchHintsForView_NotSaved()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1m, true, false);
			var collection = receive.Lines[0].SerialNumbers;
			AddSerialNumber(collection, "SN1");

			CallFetchForView(collection);
			AssertEquals("Should not add fetch hint for in memory data.", 0, Factory.ActiveFetchHintsForTable(WhsSerialNumberSchema.Constants.TableName));
		}

		#endregion

		#region TestWhsSerialNumberPivotCollectionFetchStrategy_AddFetchHintsForView_NotSerialNumberUsed

		public void TestWhsSerialNumberPivotCollectionFetchStrategy_AddFetchHintsForView_NotSerialNumberUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1m, true, false);
			Factory.Save();
			var collection = receive.Lines[0].SerialNumbers;
			AddSerialNumber(collection, "SN1");
			data.Part1.RelatedOrganisations[0].OU_UseSerialNumber = false;

			CallFetchForView(collection);
			AssertEquals("Should not add fetch hint for in serial number not used.", 0, Factory.ActiveFetchHintsForTable(WhsSerialNumberSchema.Constants.TableName));
		}

		#endregion

		#region TestWhsSerialNumberPivotCollectionFetchStrategy_LoadAllRelatedSerialNumbers

		public void TestWhsSerialNumberPivotCollectionFetchStrategy_LoadAllRelatedSerialNumbers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 1m, true, false);
			var receiveLine1 = receive.Lines[0];
			var receiveLine2 = helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var collection1 = receiveLine1.SerialNumbers;
			var collection2 = receiveLine2.SerialNumbers;
			AssertEquals("Precondition", 0, collection1.Count);
			AssertEquals("Precondition", 0, collection2.Count);

			var sn1PK = AddSerialNumber(receiveLine1.SerialNumbers, "SN1");
			var sn2PK = AddSerialNumber(receiveLine1.SerialNumbers, "SN2");
			var sn3PK = AddSerialNumber(receiveLine2.SerialNumbers, "SN3");

			Factory.Save();

			var dbHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsSerialNumberSchema.Constants.TableName, 1 },
				{ WhsSerialNumberPivotSchema.Constants.TableName, 2 },
			};

			var viewFactory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(dbHits, viewFactory))
			{
				var receiveInNewFactory = viewFactory.Load<WhsReceive>(receive.PK);
				CallFetchForView(receiveInNewFactory.Lines[0].SerialNumbers);

				AssertContainsExactElementsInAnyOrder(new[] { sn1PK, sn2PK }, receiveInNewFactory.Lines[0].SerialNumbers.Select(s => s.WSV_WSN_SerialNumber));
				AssertContainsExactElementsInAnyOrder(new[] { "SN1", "SN2" }, receiveInNewFactory.Lines[0].SerialNumbers.Select(s => s.SerialNumberValue));
				AssertContainsExactElementsInAnyOrder(new[] { sn3PK }, receiveInNewFactory.Lines[1].SerialNumbers.Select(s => s.WSV_WSN_SerialNumber));
				AssertContainsExactElementsInAnyOrder(new[] { "SN3" }, receiveInNewFactory.Lines[1].SerialNumbers.Select(s => s.SerialNumberValue));
			}
		}

		#endregion

		#region TestWhsSerialNumberPivotCollectionFetchStrategy_LoadAllRelatedAsnSerialNumbers

		public void TestWhsSerialNumberPivotCollectionFetchStrategy_LoadAllRelatedAsnSerialNumbers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 2m, true, false);
			helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var asnLine1 = helper.CreateAsnLine(receive, data.Part1, 2m);
			var asnLine2 = helper.CreateAsnLine(receive, data.Part1, 1m);
			var collection1 = asnLine1.SerialNumbers;
			var collection2 = asnLine2.SerialNumbers;
			AssertEquals("Precondition", 0, collection1.Count);
			AssertEquals("Precondition", 0, collection2.Count);

			var sn1PK = AddSerialNumber(receive.Lines[0].SerialNumbers, "SN1");
			var sn2PK = AddSerialNumber(receive.Lines[0].SerialNumbers, "SN2");
			var sn3PK = AddSerialNumber(receive.Lines[1].SerialNumbers, "SN3");

			AddSerialNumber(asnLine1, sn1PK);
			AddSerialNumber(asnLine1, sn2PK);
			AddSerialNumber(asnLine2, sn3PK);

			Factory.Save();

			var dbHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsSerialNumberSchema.Constants.TableName, 1 },
				{ WhsSerialNumberPivotSchema.Constants.TableName, 2 },
			};

			var viewFactory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(dbHits, viewFactory))
			{
				var receiveInNewFactory = viewFactory.Load<WhsReceive>(receive.PK);
				CallFetchForView(receiveInNewFactory.AsnLines[0].SerialNumbers);

				AssertContainsExactElementsInAnyOrder(new[] { sn1PK, sn2PK }, receiveInNewFactory.AsnLines[0].SerialNumbers.Select(s => s.WSV_WSN_SerialNumber));
				AssertContainsExactElementsInAnyOrder(new[] { "SN1", "SN2" }, receiveInNewFactory.AsnLines[0].SerialNumbers.Select(s => s.SerialNumberValue));
				AssertContainsExactElementsInAnyOrder(new[] { sn3PK }, receiveInNewFactory.AsnLines[1].SerialNumbers.Select(s => s.WSV_WSN_SerialNumber));
				AssertContainsExactElementsInAnyOrder(new[] { "SN3" }, receiveInNewFactory.AsnLines[1].SerialNumbers.Select(s => s.SerialNumberValue));
			}
		}

		#endregion

		#region TestWhsSerialNumberPivotCollectionFetchStrategy_LoadAllRelatedLinesAndAsnSerialNumbers

		public void TestWhsSerialNumberPivotCollectionFetchStrategy_LoadAllRelatedLinesAndAsnSerialNumbers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
			var numberOfLines = 5;
			var numberOfSNs = 10;
			var allSerialNumbers = new string[numberOfLines * numberOfSNs];
			var i = 0;
			for (int j = 0; j < numberOfLines; j++)
			{
				var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, numberOfSNs);
				var asnLine = helper.CreateAsnLine(receive, data.Part1, numberOfSNs);
				for (int k = 0; k < numberOfSNs; k++)
				{
					var serialNumber = $"SN{j}-{k}";
					var serialNumberPK = AddSerialNumber(receiveLine.SerialNumbers, serialNumber);
					var pivot = asnLine.SerialNumbers.AddNew();
					pivot.WSV_WSN_SerialNumber = serialNumberPK;
					allSerialNumbers[i] = serialNumber;
					i++;
				}
			}

			Factory.Save();
			AssertEquals("Precondition", 50, allSerialNumbers.Length);

			var dbHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsSerialNumberSchema.Constants.TableName, 1 },
				{ WhsSerialNumberPivotSchema.Constants.TableName, 4 },
			};

			var viewFactory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(dbHits, viewFactory))
			{
				var receiveInNewFactory = viewFactory.Load<WhsReceive>(receive.PK);
				CallFetchForView(receiveInNewFactory.AsnLines[0].SerialNumbers);
				CallFetchForView(receiveInNewFactory.Lines[0].SerialNumbers);

				AssertContainsExactElementsInAnyOrder(allSerialNumbers, receiveInNewFactory.AsnLines.Cast<WhsAsnLine>().SelectMany(a => a.SerialNumbers.Select(s => s.SerialNumberValue)));
				AssertContainsExactElementsInAnyOrder(allSerialNumbers, receiveInNewFactory.Lines.Cast<WhsReceiveLine>().SelectMany(a => a.SerialNumbers.Select(s => s.SerialNumberValue)));
			}
		}

		#endregion

		#region Implementation

		ZGuid AddSerialNumber(WhsSerialNumberPivotCollection serialNumbers, string sn)
		{
			var pivot = serialNumbers.AddNew();
			pivot.SerialNumberValue = sn;
			return pivot.WSV_WSN_SerialNumber;
		}

		static void CallFetchForView(WhsSerialNumberPivotCollection collection)
		{
			var collectionFetchStrategy = new WhsSerialNumberPivotCollectionFetchStrategy(collection);
			var columns = new[] { new TableColumn(WhsSerialNumberPivotSchema.Constants.TableName, nameof(WhsSerialNumberPivot.SerialNumberValue)) };
			collectionFetchStrategy.FetchForView(collection.ToArray(), columns);
		}

		protected override void SetUp()
		{
			base.SetUp();
			enableSchemaRedesignChanges = WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			enableSchemaRedesignChanges.Dispose();
			base.TearDown();
		}

		IDisposable enableSchemaRedesignChanges;

		#endregion
	}
}
