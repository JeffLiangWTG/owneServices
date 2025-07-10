using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Bonded.Testing
{
	internal class WhsBondedWarehouseLinkTest : BondedWarehouseLinkTest
	{
		#region Nature 20s

		public override void TestCreateOrUpdateInwardMovement()
		{
			var testDataBuilder = new WhsBondedTransactionProcessorTest.DummyBondedWarehouseDataBuilder(Factory);
			var receiveTransaction = testDataBuilder.GetSimpleTestData();
			var link = GetNewLink();
			var putawayEngineMock = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();

			using (ObjectFactory.Substitute(putawayEngineMock.Object))
			{
				link.CreateOrUpdateInwardMovement(receiveTransaction);
				var receive = WhsBondedTransactionProcessorTest.BondedHelperClass.LoadLastCreatedReceive(receiveTransaction,
					Factory);
				AssertNotNull("New docket is created", receive);
				AssertNotNull("Docket is finalised", receive.IsFinalised);
			}
		}

		public override void TestUpdateDeclarationReference()
		{
			var link = GetNewLink();
			Assert("incomplete test", true);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestUpdateDeclarationReferenceThrowsExceptionIfGuidEmpty()
		{
			var link = GetNewLink();
			link.UpdateDeclarationReference(ZGuid.Empty, "Dec");
		}

		public override void TestLoad()
		{
			var link = GetNewLink();
			Assert("incomplete test", true); // important
		}

		#endregion

		#region Nature 30s

		public override void TestCreateOrUpdateOutwardMovement()
		{
			var link = GetNewLink();
			var input = new WhsBondedWarehouseTransaction();
			AssertEquals("Precondition: Input should have no errors", false, input.HasErrors);
			var output = link.CreateOrUpdateOutwardMovement(input, false);
			AssertEquals("Input not returned", input, output);
			AssertEquals("Input should now have validation errors", true, input.HasErrors);
			input = new WhsBondedWarehouseTransaction();
			AssertEquals("Precondition: Input should have no errors", false, input.HasErrors);
			output = link.CreateOrUpdateOutwardMovement(input, true);
			Assert("Input returned", input != output);
		}

		#region TestGetOutwardMovementDetail

		public void TestGetOutwardMovementDetail()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.EnableWarehouseForBond(data.Whs1, true);
			helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, AreaTypes.Codes.FreeStore);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, "ENTRY1");
			var inventory2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, "ENTRY1");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();
			var input = CreateBondedTransaction(data.Org1, data.Whs1);
			var inputLine = CreateBondedTransactionLine(input, data.Part1, 10m);
			Factory.Save();
			var numberOfOrders = Factory
				.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order)).Length;
			var numberOfPicks = Factory.Load<WhsPick>(new ZQuery()).Length;
			var link = GetNewLink();
			IWhsBondedWarehouseTransaction result;
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				result = link.GetOutwardMovementDetail(input);
			}

			AssertNotEquals("Operation should have been successful.", input, result);
			AssertEquals("We should get back as many lines as went in.", 1, result.Lines.Count);
			AssertEquals("No new Orders should be created in on the Factory or saved to DB.", numberOfOrders,
				Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order)).Length);
			AssertEquals("No new Picks should be created in on the Factory or saved to DB.", numberOfPicks,
				Factory.Load<WhsPick>(new ZQuery()).Length);
			AssertEquals("No stock should have been committed or taken from warehouse.", 5m,
				inventory1.WI_AvailableToPickQuantity);
			AssertEquals("No stock should have been committed or taken from warehouse.", 5m,
				inventory2.WI_AvailableToPickQuantity);
		}

		WhsBondedWarehouseTransaction CreateBondedTransaction(OrgHeader client, WhsWarehouse whs)
		{
			var transaction = new WhsBondedWarehouseTransaction();
			transaction.Client = client;
			transaction.Warehouse = whs.WarehouseAddress;
			return transaction;
		}

		WhsBondedWarehouseTransactionLine CreateBondedTransactionLine(WhsBondedWarehouseTransaction bondedTransaction,
			OrgSupplierPart part, ZDecimal qty)
		{
			var line = new WhsBondedWarehouseTransactionLine();
			line.Product = part;
			line.Quantity = qty;
			line.Warehouse = bondedTransaction.Warehouse;
			bondedTransaction.Lines.Add(line);
			return line;
		}

		#endregion

		public override void TestCancelOutwardMovement()
		{
			var link = GetNewLink();
			Assert("incomplete test", true);
		}

		public override void TestNotifyGoodsAreClearedForRelease()
		{
			var link = GetNewLink();
			Assert("incomplete test", true);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestNotifyGoodsAreClearedForReleaseThrowsExeceptionIfInfoIsNull()
		{
			var link = GetNewLink();
			link.NotifyGoodsAreClearedForRelease(ZGuid.NewZGuid(), null);
		}

		#endregion

		#region Findboxes

		public override void TestGetEntryKeyLookup()
		{
			var link = (BondedWarehouseLink)GetNewLink();
			var line = new WhsBondedWarehouseTransactionLine();
			line.EntryLineNumber = (ZShort)1;
			AssertEquals(typeof(BondedEntryKeyLookupCollection), link.GetEntryKeyLookup(line).GetType());
		}

		public void TestDefaultsAreAddedForLookupCollections()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var link = GetNewLink();
			var line = new WhsBondedWarehouseTransactionLine();
			var org = helper.CreateClient();
			var address = Factory.New<OrgAddress>();
			var part = helper.CreateProduct(org, "P1");
			var whs = helper.CreateWarehouse("1");
			whs.WW_OA_WarehouseAddress = address.PK;
			line.EntryKey = "EK";
			line.EntryLineNumber = 2;
			line.PartAttrib1 = "PA1";
			line.PartAttrib2 = "PA2";
			line.PartAttrib3 = "PA3";
			line.Product = part;
			line.Warehouse = address;
			var collection = (BondedEntryKeyLookupCollection)link.GetEntryKeyLookup(line);
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer:Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Product:Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Warehouse:Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Bond ID 1:Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Bond ID 2:Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Entry Number:Property"));
			Assert(collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Entry Line Number:Property"));
		}

		#endregion

		#region Implementation

		protected override IBondedWarehouseLink GetNewLink()
		{
			return new WhsBondedWarehouseLink(Factory);
		}

		#endregion
	}
}
