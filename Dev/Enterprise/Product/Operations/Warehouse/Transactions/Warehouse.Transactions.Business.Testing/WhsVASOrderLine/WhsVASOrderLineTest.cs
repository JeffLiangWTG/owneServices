using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsVASOrderLine))]
	class WhsVASOrderLineTest : WhsBusinessObjectTestCase
	{
		#region Related Entities

		#region TestVASOrder

		public void TestVASOrder()
		{
			AssertNull("Precondition", Factory.New<WhsVASOrderLine>().VASOrder);

			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var line = vasOrder.Lines.AddNew();
			AssertEquals(vasOrder, line.VASOrder);
		}

		#endregion

		#endregion

		#region TestFetchForLoad

		public override void TestFetchForLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			factory2.Load<WhsVASOrderLine>(vasOrderLine.PK);

			var dbHits = new Dictionary<string, int>();
			dbHits.Add(WhsVASOrderLineSchema.Constants.TableName, 1);
			AssertDbHits(dbHits, factory2);

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			AssertNotNull("Precondition", initialTransfer);
			AssertEquals("Precondition", 1, initialTransfer.Lines.Count);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(initialTransfer);
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			AssertNotNull("Precondition", returnTransfer);

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(returnTransfer);

			var factory3 = new BusinessObjectFactory { RefreshEnabled = false };
			factory3.Load<WhsVASOrderLine>(vasOrderLine.PK);

			AssertDbHits(dbHits, factory3);
		}

		#endregion

		#region TestFetchForView

		public void TestFetchForView()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var newPart1 = Helper.CreateProduct(data.Org1, "NewPart1");
			var newPart2 = Helper.CreateProduct(data.Org1, "NewPart2");
			var newPart3 = Helper.CreateProduct(data.Org1, "NewPart3");
			var newPart4 = Helper.CreateProduct(data.Org1, "NewPart4");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 200m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 200m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", newPart1, 200m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", newPart2, 200m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", newPart3, 200m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", newPart4, 200m, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);

			for (int index = 0; index < 10; index++)
			{
				var vasOrder1 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
				Helper.CreateWhsVASOrderLine(vasOrder1, data.Part1, 10m);
				Helper.CreateWhsVASOrderLine(vasOrder1, data.Part2, 10m);
				Helper.CreateWhsVASOrderLine(vasOrder1, newPart1, 10m);
				Helper.CreateWhsVASOrderLine(vasOrder1, newPart2, 10m);
				Helper.CreateWhsVASOrderLine(vasOrder1, newPart3, 10m);
				Helper.CreateWhsVASOrderLine(vasOrder1, newPart4, 10m);

				var vasOrder2 = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
				Helper.CreateWhsVASOrderLine(vasOrder2, data.Part1, 10m);
				Helper.CreateWhsVASOrderLine(vasOrder2, data.Part2, 10m);
				Helper.CreateWhsVASOrderLine(vasOrder2, newPart1, 10m);
				Helper.CreateWhsVASOrderLine(vasOrder2, newPart2, 10m);
				Helper.CreateWhsVASOrderLine(vasOrder2, newPart3, 10m);
				Helper.CreateWhsVASOrderLine(vasOrder2, newPart4, 10m);
			}
			Factory.Save();

			var viewFactory = new BusinessObjectFactory();
			var vasOrderLines = viewFactory.Load<WhsVASOrderLine>(new ZQuery());
			int beforeFetchForView = viewFactory.DatabaseLoadCount;
			foreach (var line in vasOrderLines)
			{
				line.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(WhsVASOrderLineSchema.Constants.TableName, WhsVASOrderLineSchema.Constants.WVL_ExpiryDate),
					new TableColumn(WhsVASOrderLineSchema.Constants.TableName, WhsVASOrderLineSchema.Constants.WVL_LineNumber),
					new TableColumn(WhsVASOrderLineSchema.Constants.TableName, WhsVASOrderLineSchema.Constants.WVL_PackingDate),
					new TableColumn(WhsVASOrderLineSchema.Constants.TableName, WhsVASOrderLineSchema.Constants.WVL_PartAttrib1),
					new TableColumn(WhsVASOrderLineSchema.Constants.TableName, WhsVASOrderLineSchema.Constants.WVL_PartAttrib2),
					new TableColumn(WhsVASOrderLineSchema.Constants.TableName, WhsVASOrderLineSchema.Constants.WVL_PartAttrib3),
					new TableColumn(WhsVASOrderLineSchema.Constants.TableName, WhsVASOrderLineSchema.Constants.WVL_Quantity),
					new TableColumn(WhsVASOrderLineSchema.Constants.TableName, WhsVASOrderLineSchema.Constants.WVL_SerialNumber),
					new TableColumn(WhsVASOrderLineSchema.Constants.TableName, "Product"),
					new TableColumn(WhsVASOrderLineSchema.Constants.TableName, "ProductDescription"),
					new TableColumn(WhsVASOrderLineSchema.Constants.TableName, "WarehouseProduct"),
					new TableColumn(WhsVASOrderLineSchema.Constants.TableName, "VASOrder"),
				});
			}

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsVASOrderSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsVASOrderLineSchema.Constants.TableName, 1);
			expectedDbHits.Add(OrgSupplierPartSchema.Constants.TableName, 1);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, viewFactory))
			{
				foreach (var line in vasOrderLines)
				{
					var pokeExpiryDate = line.WVL_ExpiryDate;
					var pokeLineNumber = line.WVL_LineNumber;
					var pokePackingDate = line.WVL_PackingDate;
					var pokePartAttrib1 = line.WVL_PartAttrib1;
					var pokePartAttrib2 = line.WVL_PartAttrib2;
					var pokePartAttrib3 = line.WVL_PartAttrib3;
					var pokeQuantity = line.WVL_Quantity;
					var pokeSerialNumber = line.WVL_SerialNumber;
					var pokeProduct = line.Product;
					var pokeProductDescription = line.ProductDescription;
					var pokeWarehouseProduct = line.WarehouseProduct;
					var pokeVASOrder = line.VASOrder;
				}
			}
		}

		#endregion

		#region TestWVL_LineNumber

		public void TestWVL_LineNumber()
		{
			var line = Factory.New<WhsVASOrderLine>();
			Assert(line.WVL_LineNumberInfo.ReadOnly);
		}

		#endregion

		#region TestWVL_ExpiryDate

		public void TestWVL_ExpiryDate_ReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			var partRelation = vasOrderLine.WarehouseProduct.Parent.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);

			AssertEquals("WVL_ExpiryDate should be readonly", true, vasOrderLine.WVL_ExpiryDateInfo.ReadOnly);

			partRelation.OU_UseExpiryDate = true;
			vasOrder.Client.PartAttributeManager.Organisation.MiscServ.OM_IMUseExpiryDate = true;

			AssertEquals("WVL_ExpiryDate should be editable", false, vasOrderLine.WVL_ExpiryDateInfo.ReadOnly);
		}

		#endregion

		#region TestWVL_PackingDate

		public void TestWVL_PackingDate_ReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			var partRelation = vasOrderLine.WarehouseProduct.Parent.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);

			AssertEquals("WVL_PackingDate should be readonly", true, vasOrderLine.WVL_PackingDateInfo.ReadOnly);

			partRelation.OU_UsePackingDate = true;
			vasOrder.Client.PartAttributeManager.Organisation.MiscServ.OM_IMUsePackingDate = true;

			AssertEquals("WVL_PackingDate should be editable", false, vasOrderLine.WVL_PackingDateInfo.ReadOnly);
		}

		#endregion

		#region TestWVL_PartAttrib

		public void TestWVL_PartAttrib1_ReadOnly()
		{
			TestPartAttribReadOnly(1);
		}

		public void TestWVL_PartAttrib2_ReadOnly()
		{
			TestPartAttribReadOnly(2);
		}

		public void TestWVL_PartAttrib3_ReadOnly()
		{
			TestPartAttribReadOnly(3);
		}

		void TestPartAttribReadOnly(int attributeNumber)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			var partRelation = vasOrderLine.WarehouseProduct.Parent.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1, OrgPartRelation.RelationshipTypes.Owner);

			switch (attributeNumber)
			{
				case 1:
					AssertEquals("WVL_PartAttrib1 should be readonly", true, vasOrderLine.WVL_PartAttrib1Info.ReadOnly);
					break;
				case 2:
					AssertEquals("WVL_PartAttrib2 should be readonly", true, vasOrderLine.WVL_PartAttrib2Info.ReadOnly);
					break;
				case 3:
					AssertEquals("WVL_PartAttrib3 should be readonly", true, vasOrderLine.WVL_PartAttrib3Info.ReadOnly);
					break;
			}

			vasOrder.Client.PartAttributeManager.Organisation.MiscServ["OM_IMPartAttrib" + attributeNumber + "Type"] = PartAttributeTypeList.Codes.BatchNumber;
			partRelation["OU_UsePartAttrib" + attributeNumber] = true;

			switch (attributeNumber)
			{
				case 1:
					AssertEquals("WVL_PartAttrib1 should be editable", false, vasOrderLine.WVL_PartAttrib1Info.ReadOnly);
					break;
				case 2:
					AssertEquals("WVL_PartAttrib2 should be editable", false, vasOrderLine.WVL_PartAttrib2Info.ReadOnly);
					break;
				case 3:
					AssertEquals("WVL_PartAttrib3 should be editable", false, vasOrderLine.WVL_PartAttrib3Info.ReadOnly);
					break;
			}
		}

		#endregion

		#region TestWVL_PartAttrib1_MaxLength

		public void TestWVL_PartAttrib1_MaxLength()
		{
			var line = Factory.New<WhsVASOrderLine>();
			AssertNoExceptionThrown(() => line.WVL_PartAttrib1 = "".PadLeft(WhsVASOrderLineSchema.WVL_PartAttrib1.MaxLength, 'A'));
		}

		#endregion

		#region TestWVL_PartAttrib1_Exceed_MaxLength

		public void TestWVL_PartAttrib1_Exceed_MaxLength()
		{
			var line = Factory.New<WhsVASOrderLine>();
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					line.WVL_PartAttrib1 = ZString.Replicate('A', WhsVASOrderLineSchema.WVL_PartAttrib1.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWVL_PartAttrib2_MaxLength

		public void TestWVL_PartAttrib2_MaxLength()
		{
			var line = Factory.New<WhsVASOrderLine>();
			AssertNoExceptionThrown(() => line.WVL_PartAttrib2 = "".PadLeft(WhsVASOrderLineSchema.WVL_PartAttrib2.MaxLength, 'A'));
		}

		#endregion

		#region TestWVL_PartAttrib2_Exceed_MaxLength

		public void TestWVL_PartAttrib2_Exceed_MaxLength()
		{
			var line = Factory.New<WhsVASOrderLine>();
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					line.WVL_PartAttrib2 = ZString.Replicate('A', WhsVASOrderLineSchema.WVL_PartAttrib2.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWVL_PartAttrib3_MaxLength

		public void TestWVL_PartAttrib3_MaxLength()
		{
			var line = Factory.New<WhsVASOrderLine>();
			AssertNoExceptionThrown(() => line.WVL_PartAttrib3 = "".PadLeft(WhsVASOrderLineSchema.WVL_PartAttrib3.MaxLength, 'A'));
		}

		#endregion

		#region TestWVL_PartAttrib3_Exceed_MaxLength

		public void TestWVL_PartAttrib3_Exceed_MaxLength()
		{
			var line = Factory.New<WhsVASOrderLine>();
			try
			{
				AssertExceptionThrown<MaxLengthExceededException>(() =>
					line.WVL_PartAttrib3 = ZString.Replicate('A', WhsVASOrderLineSchema.WVL_PartAttrib3.MaxLength + 1));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestWarehouseProduct

		public void TestWarehouseProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			AssertEquals(data.Part1, vasOrderLine.WarehouseProduct.Parent);
		}

		#endregion

		#region TestProductDescription

		public void TestProductDescription()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine1 = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			AssertEquals("Descriptions should match #1", data.Part1.OP_Desc, vasOrderLine1.ProductDescription);

			data.Part2.OP_Desc = "Part2 Desc";
			var vasOrderLine2 = Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 8m);
			AssertEquals("Descriptions should match #2", "Part2 Desc", vasOrderLine2.ProductDescription);
		}

		#endregion

		#region TestReadOnly

		public void TestReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLine = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 5m);
			AssertEquals("Order Line should be editable by default.", false, vasOrderLine.ReadOnly);

			vasOrder.ReadOnly = true;
			AssertEquals("Order Line should be read-only if the Order is read-only.", true, vasOrderLine.ReadOnly);
		}

		#endregion

		#region TestReadOnly_WVLSerialNumber

		public void TestReadOnly_WVLSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, false);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var vasOrderLineWithSerialNumber = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			var vasOrderLineWithoutSerialNumber = Helper.CreateWhsVASOrderLine(vasOrder, data.Part2, 1m);
			var vasOrderLineWithoutVasOrder = Factory.New<WhsVASOrderLine>();
			var vasOrderLineWithoutProduct = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			vasOrderLineWithoutProduct.WVL_OP_Product = ZGuid.Empty;
			AssertEquals(true, vasOrderLineWithoutSerialNumber.WVL_SerialNumberInfo.ReadOnly);
			AssertEquals(true, vasOrderLineWithoutVasOrder.WVL_SerialNumberInfo.ReadOnly);
			AssertEquals(true, vasOrderLineWithoutProduct.WVL_SerialNumberInfo.ReadOnly);
			AssertEquals(false, vasOrderLineWithSerialNumber.WVL_SerialNumberInfo.ReadOnly);
		}

		#endregion

		#region TestVasOrderLinePropertiesModifiedAfterVasOrderTransferIsCreated

		public void TestVasOrderLinePropertiesModifiedAfterVasOrderTransferIsCreated()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateArea(data.Whs1, "A2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var line = Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			AssertNotNull(vasOrder.GetOrCreateInitialTransfer(Notify));
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var vasOrderLineInNewFactory = newFactory.Load<WhsVASOrderLine>(line.PK);

			AssertChangedProperties(newFactory, vasOrderLineInNewFactory, WhsVASOrderLineSchema.WVL_OP_Product, data.Part2.PK);
			AssertChangedProperties(newFactory, vasOrderLineInNewFactory, WhsVASOrderLineSchema.WVL_Quantity, (ZDecimal)2m);
			AssertChangedProperties(newFactory, vasOrderLineInNewFactory, WhsVASOrderLineSchema.WVL_LineNumber, (ZInt)10);
			AssertChangedProperties(newFactory, vasOrderLineInNewFactory, WhsVASOrderLineSchema.WVL_PartAttrib1, (ZString)"P1");
			AssertChangedProperties(newFactory, vasOrderLineInNewFactory, WhsVASOrderLineSchema.WVL_PartAttrib2, (ZString)"P2");
			AssertChangedProperties(newFactory, vasOrderLineInNewFactory, WhsVASOrderLineSchema.WVL_PartAttrib3, (ZString)"P3");
			AssertChangedProperties(newFactory, vasOrderLineInNewFactory, WhsVASOrderLineSchema.WVL_ExpiryDate, ZDateTime.Now.AddDays(4));
			AssertChangedProperties(newFactory, vasOrderLineInNewFactory, WhsVASOrderLineSchema.WVL_PackingDate, ZDateTime.Now.AddDays(-4));
		}

		static void AssertChangedProperties(BusinessObjectFactory newFactory, WhsVASOrderLine lineInNewFactory, SchemaColumn column, IZType differentValue)
		{
			var originalValue = lineInNewFactory[column];
			var helper = new WhsTestHelperFunctions(newFactory);
			lineInNewFactory[column] = differentValue;
			Assert(lineInNewFactory.HasChanges);
			helper.AssertZCannotSaveExceptionThrown("Cannot save as fields are modified after Into Service Area Transfer is created.", newFactory.Save);
			lineInNewFactory[column] = originalValue;
			lineInNewFactory.HasChanges = false;
			Assert(!lineInNewFactory.HasChanges);
			AssertNoExceptionThrown(() => newFactory.Save());
		}

		#endregion

		#region ICanDelete Members

		public void TestICanDelete()
		{
			AssertEquals(true, Factory.New<WhsVASOrderLine>().CanDelete);

			var vasOrder = Factory.New<WhsVASOrder>();
			var line = vasOrder.Lines.AddNew();
			AssertEquals(true, line.CanDelete);

			vasOrder.WVO_CancelledTimeUtc = ZDateTime.UtcNow;
			AssertEquals(false, line.CanDelete);
			AssertEquals("VAS Order lines cannot be deleted on Inactive VAS Orders.", line.ReasonForNotAbleToDelete);

			vasOrder.WVO_CancelledTimeUtc = ZDateTime.Empty;
			AssertEquals(true, line.CanDelete);

			vasOrder.WVO_WD_TransferIntoServiceArea = Factory.New<WhsTransfer>().PK;
			AssertEquals(false, line.CanDelete);
			AssertEquals("Transfer has already started. VAS Order line cannot be deleted.", line.ReasonForNotAbleToDelete);

			vasOrder.WVO_CancelledTimeUtc = ZDateTime.UtcNow;
			AssertEquals(false, line.CanDelete);
			AssertEquals("VAS Order lines cannot be deleted on Inactive VAS Orders.", line.ReasonForNotAbleToDelete);
		}

		#endregion
	}
}
