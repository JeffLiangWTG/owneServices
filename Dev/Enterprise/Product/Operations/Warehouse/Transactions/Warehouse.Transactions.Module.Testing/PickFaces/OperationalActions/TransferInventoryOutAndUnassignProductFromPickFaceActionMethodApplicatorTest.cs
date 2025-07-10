using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(TransferInventoryOutAndUnassignProductFromPickFaceActionMethodApplicator))]
	public class TransferInventoryOutAndUnassignProductFromPickFaceActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestApplicator

		public void TestApplicator()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var location3 = data.Whs1.FindLocation("A-1-3");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 100m, location2, "");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location2);
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);
			var pickFaceView2 = Factory.Load<WhsPickFaceView>(pickFace2.PK);

			ApplyApplicator(new[] { pickFaceView1, pickFaceView2 }, @"INFO: Transfer [HL W00000003] has been generated for selected pick faces.
INFO: Product: P1 has been un-assigned from location: A-1-1.
INFO: Product: P1 has been un-assigned from location: A-1-2.
INFO: Un-assign all products from pick faces successfully.");

			Assert("Un-assigned product from pickface successfully.", pickFace1.IsDeleted);
			Assert("Un-assigned product from pickface successfully.", pickFace2.IsDeleted);

			var transfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertEquals("PreCondition: Client", data.Org1.PK, transfer.WD_OH_Client);
			AssertEquals("PreCondition: Warehouse", data.Whs1.PK, transfer.WD_WW_Whs);
			AssertEquals("PreCondition: 2 Transfer lines", 2, transfer.Lines.Count);

			var transferLine1 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_WL_TransferFrom == location1.PK);
			AssertEquals("Product", data.Part1.PK, transferLine1.WE_OP);
			AssertEquals("Qty", 100m, transferLine1.QtyToMoveIncludingMatchingLines);
			AssertEquals("Docket line status", DocketLineStatus.Codes.Entered, transferLine1.WE_DocketLineStatus);
			AssertEquals("Inventory status", InventoryStatus.Codes.Available, transferLine1.WE_OriginalInventoryStatus);
			transferLine1.WE_WL = location3.PK;

			var transferLine2 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_WL_TransferFrom == location2.PK);
			AssertEquals("Product", data.Part1.PK, transferLine2.WE_OP);
			AssertEquals("Qty", 100m, transferLine2.QtyToMoveIncludingMatchingLines);
			AssertEquals("Docket line status", DocketLineStatus.Codes.Entered, transferLine2.WE_DocketLineStatus);
			AssertEquals("Inventory status", InventoryStatus.Codes.Available, transferLine2.WE_OriginalInventoryStatus);
			transferLine2.QtyToMoveIncludingMatchingLines = 30m;
			transferLine2.WE_WL = location3.PK;

			transfer.FinaliseDocketWithoutUserConfirmation();
			Assert("Transfer is finalised.", transfer.IsFinalised);
			AssertNoExceptionThrown("Save successfully without exception thrown.", Factory.Save);
		}

		#endregion

		#region TestApplicator_DBHits

		public void TestApplicator_DBHits()
		{
			var count = 100;
			var expectedLog = new ZStringBuilder();
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var helper = new WhsTestHelperFunctions(newFactory);
			var data = new TestDataSimpleEnvironment(newFactory, 1, (short)count);
			var pickFaces = new WhsPickFace[count];
			var pickFaceViews = new WhsPickFaceView[count];
			expectedLog.AppendLine("INFO: Transfer [HL W00000101] has been generated for selected pick faces.");
			for (var i = 0; i < count; i++)
			{
				var location = data.Whs1.FindLocation($"A-1-{i + 1}");
				helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i + 1}", ZDateTimeOffset.Now, data.Part1, 100m, location, "");
				pickFaces[i] = helper.CreateProductPickFace(data.Part1, data.Org1, location);
				expectedLog.AppendLine($"INFO: Product: P1 has been un-assigned from location: A-1-{i + 1}.");
			}

			helper.EnableWarehouseForBond(data.Whs1, true);
			newFactory.Save();
			expectedLog.Append(@"INFO: Un-assign all products from pick faces successfully.");

			for (var j = 0; j < count; j++)
			{
				pickFaceViews[j] = newFactory.Load<WhsPickFaceView>(pickFaces[j].PK);
			}

			using (RowFactory.SetCachedTables())
			{
				ApplyApplicator(pickFaceViews, expectedLog.ToString());
			}

			var expectedDbHits = new Dictionary<string, int>
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmDocDataOverrideSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 2 },
				{ StmNoteSchema.Constants.TableName, 2 },
				{ StmUniversalCopySchema.Constants.TableName, 4 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsBondedWarehouseAttributeSchema.Constants.TableName, 100 },	// Create docket line from inventory caused high DB hits, will repair by Work Item WI00193553 
				{ WhsDocketSchema.Constants.TableName, 100 },	// Create docket line from inventory caused high DB hits, will repair by Work Item WI00193553 
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsInventoryViewSchema.Constants.TableName, 101 },	// Create docket line from inventory caused high DB hits, will repair by Work Item WI00193553 
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsPickFaceSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsVASOrderSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
			};
			AssertDbHits(expectedDbHits, Factory);
		}

		#endregion

		#region TestApplicator_WithHeldInventory

		public void TestApplicator_WithHeldInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 15m, location1, "");
			var receiveLine = receive.Lines.Single();
			receiveLine.HeldCodeChangeQuantity = 5m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLine.ChangeInventoryHeldCode(true);

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);

			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			ApplyApplicator(new[] { pickFaceView }, @"INFO: Transfer [HL W00000002] has been generated for selected pick faces.
INFO: Product: P1 has been un-assigned from location: A-1-1.
INFO: Un-assign all products from pick faces successfully.");

			Assert("Un-assigned product from pickface successfully.", pickFace.IsDeleted);

			var transfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertEquals("PreCondition: Client", data.Org1.PK, transfer.WD_OH_Client);
			AssertEquals("PreCondition: Warehouse", data.Whs1.PK, transfer.WD_WW_Whs);
			AssertEquals("PreCondition: 2 Transfer lines", 2, transfer.Lines.Count);

			var transferLine1 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Available);
			AssertEquals("Product", data.Part1.PK, transferLine1.WE_OP);
			AssertEquals("Qty", 10m, transferLine1.QtyToMoveIncludingMatchingLines);
			AssertEquals("Docket line status", DocketLineStatus.Codes.Entered, transferLine1.WE_DocketLineStatus);
			AssertEquals("Location", location1.PK, transferLine1.WE_WL_TransferFrom);
			transferLine1.WE_WL = location2.PK;

			var transferLine2 = transfer.Lines.Cast<WhsTransferLine>().Single(l => l.WE_OriginalInventoryStatus == InventoryStatus.Codes.Held);
			AssertEquals("Product", data.Part1.PK, transferLine2.WE_OP);
			AssertEquals("Qty", 5m, transferLine2.QtyToMoveIncludingMatchingLines);
			AssertEquals("Docket line status", DocketLineStatus.Codes.Entered, transferLine2.WE_DocketLineStatus);
			AssertEquals("Location", location1.PK, transferLine2.WE_WL_TransferFrom);
			transferLine2.WE_WL = location2.PK;

			transfer.FinaliseDocketWithoutUserConfirmation();
			Assert("Transfer is finalised.", transfer.IsFinalised);
			AssertNoExceptionThrown("Save successfully without exception thrown.", Factory.Save);
		}

		#endregion

		#region TestApplicator_NoStockOnHand

		public void TestApplicator_NoStockOnHand()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var location3 = data.Whs1.FindLocation("A-1-3");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location1, "");

			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location2);
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);
			var pickFaceView2 = Factory.Load<WhsPickFaceView>(pickFace2.PK);

			ApplyApplicator(new[] { pickFaceView1, pickFaceView2 }, @"INFO: Transfer [HL W00000002] has been generated for selected pick faces.
INFO: Product: P1 has been un-assigned from location: A-1-1.
INFO: Product: P1 has been un-assigned from location: A-1-2.
INFO: Un-assign all products from pick faces successfully.");

			Assert("Un-assigned product from pickface successfully.", pickFace1.IsDeleted);
			Assert("Un-assigned product from pickface successfully.", pickFace2.IsDeleted);

			var transfer = Factory.LoadTop1<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertEquals("PreCondition: Client", data.Org1.PK, transfer.WD_OH_Client);
			AssertEquals("PreCondition: Warehouse", data.Whs1.PK, transfer.WD_WW_Whs);
			AssertEquals("PreCondition: 1 Transfer lines", 1, transfer.Lines.Count);

			var transferLine = transfer.Lines.Single();
			AssertEquals("Product", data.Part1.PK, transferLine.WE_OP);
			AssertEquals("Qty", 100m, ((WhsTransferLine)transferLine).QtyToMoveIncludingMatchingLines);
			AssertEquals("Docket line status", DocketLineStatus.Codes.Entered, transferLine.WE_DocketLineStatus);
			AssertEquals("Inventory status", InventoryStatus.Codes.Available, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Location", location1.PK, transferLine.WE_WL_TransferFrom);
			transferLine.WE_WL = location3.PK;

			transfer.FinaliseDocketWithoutUserConfirmation();
			Assert("Transfer is finalised.", transfer.IsFinalised);
			AssertNoExceptionThrown("Save successfully without exception thrown.", Factory.Save);
		}

		#endregion

		#region TestApplicator_DifferentClients

		public void TestApplicator_DifferentClients()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client2 = Helper.CreateClient("LIM");
			var part2 = Helper.CreateProduct(client2, "BMW");
			var location = data.Whs1.DefaultLocation;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location, "");
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", ZDateTimeOffset.Now, part2, 100m, location, "");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			var pickFace2 = Helper.CreateProductPickFace(part2, client2, location);
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);
			var pickFaceView2 = Factory.Load<WhsPickFaceView>(pickFace2.PK);

			ApplyApplicator(new[] { pickFaceView1, pickFaceView2 }, @"WARNING: Unable to transfer out and un-assign the products from pick faces when the selection contains multiple clients.
ERROR: No transfer has been generated and no products have been un-assigned.");

			Assert("Product is not un-assigned from pickface.", !pickFace1.IsDeleted);
			Assert("Product is not un-assigned from pickface.", !pickFace2.IsDeleted);
		}

		#endregion

		#region TestApplicator_DifferentWarehouses

		public void TestApplicator_DifferentWarehouses()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WHS2", "B");
			var location1 = data.Whs1.DefaultLocation;
			var location2 = whs2.DefaultLocation;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R2", ZDateTimeOffset.Now, data.Part1, 100m, location2, "");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location2);
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);
			var pickFaceView2 = Factory.Load<WhsPickFaceView>(pickFace2.PK);

			ApplyApplicator(new[] { pickFaceView1, pickFaceView2 }, @"WARNING: Unable to transfer out and un-assign the products from pick faces when the selection contains multiple warehouses.
ERROR: No transfer has been generated and no products have been un-assigned.");

			Assert("Product is not un-assigned from pickface.", !pickFace1.IsDeleted);
			Assert("Product is not un-assigned from pickface.", !pickFace2.IsDeleted);
		}

		#endregion

		#region TestApplicator_FixedLocation

		public void TestApplicator_FixedLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var fixedLocationType = Helper.CreateLocationType("XYZ", "Test", false, 1, LocationClasses.Codes.FIX);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			location1.WLV_WLT_LocationType = fixedLocationType.PK;
			location2.WLV_WLT_LocationType = fixedLocationType.PK;

			Factory.Save();

			var pickFaceView1 = Factory.New<WhsPickFaceView>();
			pickFaceView1.WPV_WL = location1.PK;
			var pickFaceView2 = Factory.New<WhsPickFaceView>();
			pickFaceView2.WPV_WL = location2.PK;

			ApplyApplicator(new[] { pickFaceView1, pickFaceView2 }, @"WARNING: Pick face location: A-1-1 has no product assigned, transfer out and un-assign has been ignored.
WARNING: Pick face location: A-1-2 has no product assigned, transfer out and un-assign has been ignored.
ERROR: No transfer has been generated and no products have been un-assigned.");
		}

		#endregion

		#region TestApplicator_InventoryIsCommitted

		public void TestApplicator_InventoryIsCommitted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 100m, location2, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, location1, location2);
			transfer.RunPreSaveValidation();

			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location2);
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);
			var pickFaceView2 = Factory.Load<WhsPickFaceView>(pickFace2.PK);

			AssertEquals("Committed is 30.", 30m, pickFaceView1.WPV_Committed);

			ApplyApplicator(new[] { pickFaceView1, pickFaceView2 }, @"WARNING: Unable to transfer out and un-assign the product: P1 from pick face location: A-1-1. Reason:
Pick face has committed inventories.
INFO: Transfer [HL W00000004] has been generated for selected pick faces.
INFO: Product: P1 has been un-assigned from location: A-1-2.
INFO: Un-assign all products from pick faces successfully.
INFO: Delete un-picked incoming transfer line [Product:P1, Quantity:30, From location: A-1-1 to location: A-1-2] on transfer [HL W00000003].
INFO: Delete all un-picked incoming transfer lines successfully.");

			Assert("Product is not un-assigned from pickface.", !pickFace1.IsDeleted);
			Assert("Un-assigned product from pickface successfully.", pickFace2.IsDeleted);
		}

		#endregion

		#region TestApplicator_Incoming

		public void TestApplicator_IncomingTransferInProcess()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 100m, location2, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, location2, location1);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, location2, location1);
			transfer.RunPreSaveValidation();
			transferLine1.PickLines.Single().WZ_GS_NKAssignedTo = "AAA";

			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location2);
			Factory.Save();

			var pickFaceView1 = Factory.Load<WhsPickFaceView>(pickFace1.PK);
			var pickFaceView2 = Factory.Load<WhsPickFaceView>(pickFace2.PK);

			AssertEquals("PreCondition: Incoming is 50.", 50m, pickFaceView1.WPV_Incoming);

			ApplyApplicator(new[] { pickFaceView1, pickFaceView2 }, @"WARNING: Unable to transfer out and un-assign the product: P1 from pick face location: A-1-1. Reason:
Pick face has picked but un-finalized incoming transfers.
WARNING: Unable to transfer out and un-assign the product: P1 from pick face location: A-1-2. Reason:
Pick face has committed inventories.
ERROR: No transfer has been generated and no products have been un-assigned.");

			Assert("Product is not un-assigned from pickface.", !pickFace1.IsDeleted);
			Assert("Product is not un-assigned from pickface.", !pickFace2.IsDeleted);

			Assert("Transfer line is not deleted.", !transferLine1.IsDeleted);
			Assert("Transfer line is not deleted.", !transferLine2.IsDeleted);
			AssertEquals("Transfer has 2 lines.", 2, transfer.Lines.Count);
		}

		public void TestApplicator_IncomingTransferlineNotStartAndShouldBeDeleted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var location3 = data.Whs1.FindLocation("A-1-3");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 100m, location2, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, location2, location3);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, location2, location1);
			transfer.RunPreSaveValidation();

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			AssertEquals("Precondition: Incoming is 30.", 30m, pickFaceView.WPV_Incoming);

			ApplyApplicator(new[] { pickFaceView }, @"INFO: Transfer [HL W00000004] has been generated for selected pick faces.
INFO: Product: P1 has been un-assigned from location: A-1-1.
INFO: Un-assign all products from pick faces successfully.
INFO: Delete un-picked incoming transfer line [Product:P1, Quantity:30, From location: A-1-2 to location: A-1-1] on transfer [HL W00000003].
INFO: Delete all un-picked incoming transfer lines successfully.");

			Assert("Un-assigned product from pickface successfully.", pickFace.IsDeleted);

			Assert("Transferline is not deleted.", !transferLine1.IsDeleted);
			Assert("Transferline is deleted.", transferLine2.IsDeleted);
			AssertEquals("Transfer has 1 line.", 1, transfer.Lines.Count);
		}

		public void TestApplicator_IncomingTransferOfAnotherClientInProcess()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var client2 = Helper.CreateClient("LIM");
			var relatedOrganisation = data.Part1.RelatedOrganisations.AddNew();
			relatedOrganisation.OU_OH = client2.PK;
			relatedOrganisation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 100m, location2, "");
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R3", ZDateTimeOffset.Now, data.Part1, 100m, location2, "");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 30m, location2, location1);
			transfer1.RunPreSaveValidation();

			var transfer2 = Helper.CreateWhsTransfer(client2, data.Whs1);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer2, data.Part1, 30m, location2, location1);
			transfer2.RunPreSaveValidation();
			transferLine2.PickLines.Single().WZ_GS_NKAssignedTo = "AAA";

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			AssertEquals("Precondition: Incoming is 30.", 30m, pickFaceView.WPV_Incoming);

			ApplyApplicator(new[] { pickFaceView }, @"INFO: Transfer [HL W00000006] has been generated for selected pick faces.
INFO: Product: P1 has been un-assigned from location: A-1-1.
INFO: Un-assign all products from pick faces successfully.
INFO: Delete un-picked incoming transfer line [Product:P1, Quantity:30, From location: A-1-2 to location: A-1-1] on transfer [HL W00000004].
INFO: Delete all un-picked incoming transfer lines successfully.");

			Assert("Un-assigned product from pickface successfully.", pickFace.IsDeleted);
			AssertEquals("Unpicked transfer has 0 line.", 0, transfer1.Lines.Count);
			AssertEquals("Picked transfer has 1 line.", 1, transfer2.Lines.Count);
		}

		public void TestApplicator_IncomingTransferIsFinalized()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 100m, location2, "");

			var staff = Helper.CreateGlbStaff("AAA", "StaffA");
			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 30m, location2, location1, staff);
			transfer1.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine1);
			AssertEquals("Precondition: Transferline is picked.", "AAA", transferLine1.PickLines.Single().WZ_GS_NKAssignedTo);

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 30m, location2, location1);
			transfer2.RunPreSaveValidation();

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			AssertEquals("Precondition: Incoming is 30.", 30m, pickFaceView.WPV_Incoming);

			ApplyApplicator(new[] { pickFaceView }, @"INFO: Transfer [HL W00000005] has been generated for selected pick faces.
INFO: Product: P1 has been un-assigned from location: A-1-1.
INFO: Un-assign all products from pick faces successfully.
INFO: Delete un-picked incoming transfer line [Product:P1, Quantity:30, From location: A-1-2 to location: A-1-1] on transfer [HL W00000004].
INFO: Delete all un-picked incoming transfer lines successfully.");

			Assert("Un-assigned product from pickface successfully.", pickFace.IsDeleted);
			AssertEquals("Finalised transfer has 1 line.", 1, transfer1.Lines.Count);
			AssertEquals("Unfinalised transfer has 0 line.", 0, transfer2.Lines.Count);
		}

		#endregion

		#region TestApplicator_ReceiveIsUnFinalised

		public void TestApplicator_ReceiveIsUnFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Now, data.Part1, 100m, location, "", finalise: false);

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			ApplyApplicator(new[] { pickFaceView }, @"WARNING: Unable to transfer out and un-assign the product: P1 from pick face location: A. Reason:
Pick face has un-finalized receives.
ERROR: No transfer has been generated and no products have been un-assigned.");

			Assert("Product is not un-assigned from pickface.", !pickFace.IsDeleted);
		}

		#endregion

		#region TestApplicator_NoInventoriesInThePickFace

		public void TestApplicator_NoInventoriesInThePickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			ApplyApplicator(new[] { pickFaceView }, @"WARNING: No transfer has been generated because there is no inventory on selected pick faces.
INFO: Product: P1 has been un-assigned from location: A.
INFO: Un-assign all products from pick faces successfully.");

			Assert("Un-assigned product from pickface successfully.", pickFace.IsDeleted);
			var hasTransfer = (Factory.Load<WhsTransfer>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer)).Length > 0);
			Assert("No transfer was created.", !hasTransfer);
		}

		#endregion

		#region TestApplicator_WithUnFinalisedAdjustmentIn

		public void TestApplicator_WithUnFinalisedAdjustmentIn()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var location = data.Whs1.DefaultLocation;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location, "");
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 10m, location);

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			Factory.Save();
			Assert("Adjustment line is not finalised.", !adjustmentLine.IsFinalised);

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			ApplyApplicator(new[] { pickFaceView }, @"WARNING: Unable to transfer out and un-assign the product: P1 from pick face location: A. Reason:
Pick face has un-finalized adjustments.
ERROR: No transfer has been generated and no products have been un-assigned.");

			Assert("Product is not un-assigned from pickface.", !pickFace.IsDeleted);
		}

		#endregion

		#region TestApplicator_WithUnFinalisedTransferOut

		public void TestApplicator_WithUnFinalisedTransferOut()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			var location3 = data.Whs1.FindLocation("A-1-3");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location1, "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var picker = Helper.CreateGlbStaff("AAA", "StaffA");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 30m, location1, location2, picker);
			transferLine.PickedTime = ZDateTimeOffset.Today;
			transfer.RunPreSaveValidation();
			Assert("Precondition: Transfer line is not finalised.", !transferLine.IsFinalised);
			AssertEquals("Precondition: Transfer line status is HFT.", DocketLineStatus.Codes.HeldForTransfer, transferLine.WE_DocketLineStatus);

			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			ApplyApplicator(new[] { pickFaceView }, @"INFO: Transfer [HL W00000003] has been generated for selected pick faces.
INFO: Product: P1 has been un-assigned from location: A-1-1.
INFO: Un-assign all products from pick faces successfully.");

			Assert("Un-assigned product from pickface successfully.", pickFace.IsDeleted);

			var transferOutQuery = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			transferOutQuery.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, transfer.PK);
			var transferOut = Factory.LoadTop1<WhsTransfer>(transferOutQuery);
			AssertEquals("PreCondition: Client", data.Org1.PK, transferOut.WD_OH_Client);
			AssertEquals("PreCondition: Warehouse", data.Whs1.PK, transferOut.WD_WW_Whs);
			AssertEquals("PreCondition: 1 Transfer lines", 1, transferOut.Lines.Count);

			var transferOutLine = transferOut.Lines.Single();
			AssertEquals("Product", data.Part1.PK, transferOutLine.WE_OP);
			AssertEquals("Qty", 70m, ((WhsTransferLine)transferOutLine).QtyToMoveIncludingMatchingLines);
			AssertEquals("Docket line status", DocketLineStatus.Codes.Entered, transferOutLine.WE_DocketLineStatus);
			AssertEquals("Inventory status", InventoryStatus.Codes.Available, transferOutLine.WE_OriginalInventoryStatus);
			AssertEquals("Location", location1.PK, transferOutLine.WE_WL_TransferFrom);
			transferOutLine.WE_WL = location3.PK;

			transferOut.FinaliseDocketWithoutUserConfirmation();
			Assert("Transfer is finalised.", transferOut.IsFinalised);
			AssertNoExceptionThrown("Save successfully without exception thrown.", Factory.Save);
		}

		#endregion

		#region TestApplicator_TransferHasErrors

		public void TestApplicator_TransferHasErrors()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", ZDateTimeOffset.Now, data.Part1, 100m, location, "");
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(pickFace.PK);

			Applicator.AddErrorsToTransferForTest += (transfer) =>
			{
				transfer.AddRowError("Only For testing1.");
				transfer.AddRowError("Only For testing2.");
				transfer.AddRowError("Only For testing3.");
			};
			ApplyApplicator(new[] { pickFaceView }, @"ERROR: Could not generate transfer for selected pick faces:
Error - Warehouse Transfer: Only For testing1.
Error - Warehouse Transfer: Only For testing2.
Error - Warehouse Transfer: Only For testing3.");

			Assert("Product is not un-assigned from pickface.", !pickFace.IsDeleted);
		}

		#endregion

		#region Implementation

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion

		new TransferInventoryOutAndUnassignProductFromPickFaceActionMethodApplicator Applicator => (TransferInventoryOutAndUnassignProductFromPickFaceActionMethodApplicator)base.Applicator;
	}
}
