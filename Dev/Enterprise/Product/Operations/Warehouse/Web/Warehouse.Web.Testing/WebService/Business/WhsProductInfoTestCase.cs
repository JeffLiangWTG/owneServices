using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsProductInfo))]
	public class WhsProductInfoTestCase : DataObjectInfoTestCase<WhsProductInfo>
	{
		#region TestAdditionalConstructors

		public void TestAdditionalConstructors()
		{
			var productInfo = new WhsProductInfo(null);
			AssertEquals(Guid.Empty, productInfo.PK);
			AssertEquals("", productInfo.Code);
			AssertEquals(0m, productInfo.Cubic);
			AssertEquals("", productInfo.CubicUQ);
			AssertEquals("", productInfo.Description);
			AssertEquals(0m, productInfo.PalletSize);
			AssertNotNull(productInfo.ProductUnits);
			AssertEquals(0, productInfo.ProductUnits.Count);
			AssertEquals("", productInfo.StockUnit);
			AssertEquals(0m, productInfo.Weight);
			AssertEquals("", productInfo.WeightUQ);
			AssertEquals(0m, productInfo.Depth);
			AssertEquals(0m, productInfo.Width);
			AssertEquals(0m, productInfo.Height);
			AssertEquals("", productInfo.MeasureUQ);
			AssertEquals(0, productInfo.DecimalPlaces);

			var factory = new BusinessObjectFactory();
			var part = factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART NO";
			part.OP_Depth = 30m;
			part.OP_Width = 40m;
			part.OP_Height = 50m;
			part.OP_MeasureUQ = "CM";
			part.OP_Cubic = 10m;
			part.OP_CubicUQ = "M3";
			part.OP_Desc = "Description";
			part.OP_StockKeepingUnit = "UNT";
			part.OP_Weight = 20m;
			part.OP_WeightUQ = "KG";
			part.OP_CountDecimalPlaces = 2;

			part.PartUnits.AddNew();
			part.PartUnits.AddNew();
			part.PartUnits[0].OF_PackType = "UNT";
			part.PartUnits[0].OF_ParentPackType = "PLT";
			part.PartUnits[0].OF_QuantityInParent = 10m;

			part.PartUnits[1].OF_PackType = "P1";
			part.PartUnits[1].OF_ParentPackType = "PP1";
			part.PartUnits[1].OF_QuantityInParent = 20m;
			AssertEquals(10m, part.OP_StockKeepingUnitPerPallet);

			part.PartBarcodes.AddNew();
			part.PartBarcodes.AddNew();
			part.PartBarcodes[0].PH_Barcode = "12345";
			part.PartBarcodes[0].PH_F3_NKPackType = "CTN";

			part.PartBarcodes[1].PH_Barcode = "67890";
			part.PartBarcodes[1].PH_F3_NKPackType = "PLT";

			productInfo = new WhsProductInfo(part);
			AssertEquals(part.PK, productInfo.PK);
			AssertEquals("PART NO", productInfo.Code);
			AssertEquals(10m, productInfo.Cubic);
			AssertEquals("M3", productInfo.CubicUQ);
			AssertEquals("Description", productInfo.Description);
			AssertEquals(10m, productInfo.PalletSize);
			AssertNotNull(productInfo.ProductUnits);
			AssertEquals(2, productInfo.ProductUnits.Count);
			AssertEquals(2, productInfo.DecimalPlaces);
			AssertEquals(2, productInfo.ProductBarcodes.Count);

			AssertEquals("UNT", productInfo.ProductUnits[0].Package);
			AssertEquals("PLT", productInfo.ProductUnits[0].Parent);
			AssertEquals(10m, productInfo.ProductUnits[0].Units);

			AssertEquals("P1", productInfo.ProductUnits[1].Package);
			AssertEquals("PP1", productInfo.ProductUnits[1].Parent);
			AssertEquals(20m, productInfo.ProductUnits[1].Units);

			AssertEquals("UNT", productInfo.StockUnit);
			AssertEquals(20m, productInfo.Weight);
			AssertEquals("KG", productInfo.WeightUQ);
			AssertEquals(30m, productInfo.Depth);
			AssertEquals(40m, productInfo.Width);
			AssertEquals(50m, productInfo.Height);
			AssertEquals("CM", productInfo.MeasureUQ);

			var barcodeInfo1 = GetProductBarcodeInfoFromCollection("12345", productInfo.ProductBarcodes);
			AssertEquals("12345", barcodeInfo1.Barcode);
			AssertEquals("CTN", barcodeInfo1.PackType);

			var barcodeInfo2 = GetProductBarcodeInfoFromCollection("67890", productInfo.ProductBarcodes);
			AssertEquals("67890", barcodeInfo2.Barcode);
			AssertEquals("PLT", barcodeInfo2.PackType);
		}

		WhsProductBarcodeInfo GetProductBarcodeInfoFromCollection(string barcode, WhsProductBarcodeInfoCollection collection)
		{
			foreach (var barcodeInfo in collection)
			{
				if (barcodeInfo.Barcode == barcode)
				{
					return barcodeInfo;
				}
			}
			return null;
		}

		public void TestAdditionalConstructor_ClientInfo()
		{
			var clientOwner = Helper.CreateClient("C1");
			var clientSupplier = Helper.CreateClient("C2");
			var clientBoth = Helper.CreateClient("C3");
			var product = Helper.CreateProduct(clientOwner, "P1");
			Helper.CreateProductClientRelationShip(clientSupplier, product, OrgPartRelation.RelationshipTypes.Supplier);
			Helper.CreateProductClientRelationShip(clientBoth, product, OrgPartRelation.RelationshipTypes.Both);

			var productInfo = new WhsProductInfo(product);
			AssertContainsExactElementsInAnyOrder(new[] { clientOwner.PK, clientBoth.PK }, productInfo.ClientPKs);
			AssertContainsExactElementsInAnyOrder(new[] { clientOwner.OH_Code, clientBoth.OH_Code }, productInfo.ClientNames);
		}
		#endregion

		#region TestGetInfo

		public void TestGetInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var emptyInfo = WhsProductInfo.GetInfo(null, GoodsHandlingInstructionsType.None);
			AssertEquals(Guid.Empty, emptyInfo.PK);
			AssertNotEquals("Null arguments should not cache result.", emptyInfo, WhsProductInfo.GetInfo(null, GoodsHandlingInstructionsType.None));

			var part1Info = WhsProductInfo.GetInfo(data.Part1, GoodsHandlingInstructionsType.None);
			AssertEquals(data.Part1.PK.ToGuid(), part1Info.PK);
			AssertEquals("Non null arguments should cache result.", part1Info, WhsProductInfo.GetInfo(data.Part1, GoodsHandlingInstructionsType.None));

			var part2Info = WhsProductInfo.GetInfo(data.Part2, GoodsHandlingInstructionsType.None);
			AssertEquals(data.Part2.PK.ToGuid(), part2Info.PK);
			AssertEquals("Non null arguments should cache result.", part2Info, WhsProductInfo.GetInfo(data.Part2, GoodsHandlingInstructionsType.None));
			AssertNotEquals("Caches should be per Product.", part1Info, part2Info);
		}

		public void TestGetInfo_GoodsHandlingInstructions_None()
		{
			TestGetInfo_GoodsHandlingInstructions(
				GoodsHandlingInstructionsType.None,
				"",
				"",
				"",
				"",
				"",
				"",
				"");
		}

		public void TestGetInfo_GoodsHandlingInstructions_Unload()
		{
			TestGetInfo_GoodsHandlingInstructions(
				GoodsHandlingInstructionsType.Unload,
				"",
				"Testing Goods Instructions For All Module All Direction All Freight",
				"Testing Goods Instructions For Warehouse Module All Direction All Freight",
				"Testing Goods Instructions For Warehouse Module Inbound Direction All Freight",
				"Testing Goods Instructions For Warehouse Module Inbound Direction Receive Freight",
				"Testing Goods Instructions For Warehouse Module Inbound Direction Receive Freight",
				"Testing Goods Instructions For Warehouse Module Inbound Direction Receive Freight");
		}

		public void TestGetInfo_GoodsHandlingInstructions_OrderPicking()
		{
			TestGetInfo_GoodsHandlingInstructions(
				GoodsHandlingInstructionsType.OrderPicking,
				"",
				"Testing Goods Instructions For All Module All Direction All Freight",
				"Testing Goods Instructions For Warehouse Module All Direction All Freight",
				"Testing Goods Instructions For Warehouse Module All Direction All Freight",
				"Testing Goods Instructions For Warehouse Module All Direction All Freight",
				"Testing Goods Instructions For Warehouse Module Outbound Direction All Freight",
				"Testing Goods Instructions For Warehouse Module Outbound Direction Orders Freight");
		}

		void TestGetInfo_GoodsHandlingInstructions(
			GoodsHandlingInstructionsType noteType,
			string expectedResult1,
			string expectedResult2,
			string expectedResult3,
			string expectedResult4,
			string expectedResult5,
			string expectedResult6,
			string expectedResult7)
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var partInfo = new WhsProductInfo(data.Part1, noteType);
			AssertEquals(expectedResult1, partInfo.GoodsHandlingInstructions);

			var warehouseNotes = data.Part1.Notes;
			var allNote = warehouseNotes.AddNew();
			allNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			allNote.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			allNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.A);
			allNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
			allNote.ST_NoteDataAsText = "Testing Goods Instructions For All Module All Direction All Freight";

			partInfo = new WhsProductInfo(data.Part1, noteType);
			AssertEquals(expectedResult2, partInfo.GoodsHandlingInstructions);

			var warehouseNote = warehouseNotes.AddNew();
			warehouseNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			warehouseNote.ST_NoteContextModule = nameof(StmNoteContextModule.W);
			warehouseNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.A);
			warehouseNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
			warehouseNote.ST_NoteDataAsText = "Testing Goods Instructions For Warehouse Module All Direction All Freight";

			partInfo = new WhsProductInfo(data.Part1, noteType);
			AssertEquals(expectedResult3, partInfo.GoodsHandlingInstructions);

			var inboundAllNote = warehouseNotes.AddNew();
			inboundAllNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			inboundAllNote.ST_NoteContextModule = nameof(StmNoteContextModule.W);
			inboundAllNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.R);
			inboundAllNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
			inboundAllNote.ST_NoteDataAsText = "Testing Goods Instructions For Warehouse Module Inbound Direction All Freight";

			partInfo = new WhsProductInfo(data.Part1, noteType);
			AssertEquals(expectedResult4, partInfo.GoodsHandlingInstructions);

			var inboundReceiveNote = warehouseNotes.AddNew();
			inboundReceiveNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			inboundReceiveNote.ST_NoteContextModule = nameof(StmNoteContextModule.W);
			inboundReceiveNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.R);
			inboundReceiveNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.R);
			inboundReceiveNote.ST_NoteDataAsText = "Testing Goods Instructions For Warehouse Module Inbound Direction Receive Freight";

			partInfo = new WhsProductInfo(data.Part1, noteType);
			AssertEquals(expectedResult5, partInfo.GoodsHandlingInstructions);

			var outboundAllNote = warehouseNotes.AddNew();
			outboundAllNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			outboundAllNote.ST_NoteContextModule = nameof(StmNoteContextModule.W);
			outboundAllNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.O);
			outboundAllNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
			outboundAllNote.ST_NoteDataAsText = "Testing Goods Instructions For Warehouse Module Outbound Direction All Freight";

			partInfo = new WhsProductInfo(data.Part1, noteType);
			AssertEquals(expectedResult6, partInfo.GoodsHandlingInstructions);

			var outboundOrdersNote = warehouseNotes.AddNew();
			outboundOrdersNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			outboundOrdersNote.ST_NoteContextModule = nameof(StmNoteContextModule.W);
			outboundOrdersNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.O);
			outboundOrdersNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.O);
			outboundOrdersNote.ST_NoteDataAsText = "Testing Goods Instructions For Warehouse Module Outbound Direction Orders Freight";

			partInfo = new WhsProductInfo(data.Part1, noteType);
			AssertEquals(expectedResult7, partInfo.GoodsHandlingInstructions);
		}

		public void TestGetInfo_GoodsHandlingInstructions_OrderPicking_WOR()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var partInfo = new WhsProductInfo(data.Part1, GoodsHandlingInstructionsType.OrderPicking);
			AssertNullOrEmpty(partInfo.GoodsHandlingInstructions);

			var warehouseNotes = data.Part1.Notes;
			var allNote = warehouseNotes.AddNew();
			allNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			allNote.ST_NoteContextModule = nameof(StmNoteContextModule.A);
			allNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.A);
			allNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
			allNote.ST_NoteDataAsText = "Testing Goods Instructions For All Module All Direction All Freight";

			partInfo = new WhsProductInfo(data.Part1, GoodsHandlingInstructionsType.OrderPicking);
			AssertEquals("Testing Goods Instructions For All Module All Direction All Freight", partInfo.GoodsHandlingInstructions);

			var outboundOrdersNote = warehouseNotes.AddNew();
			outboundOrdersNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			outboundOrdersNote.ST_NoteContextModule = nameof(StmNoteContextModule.W);
			outboundOrdersNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.O);
			outboundOrdersNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.O);
			outboundOrdersNote.ST_NoteDataAsText = "Testing Goods Instructions For Warehouse Module Outbound Direction Orders Freight";

			partInfo = new WhsProductInfo(data.Part1, GoodsHandlingInstructionsType.OrderPicking);
			AssertEquals("Testing Goods Instructions For Warehouse Module Outbound Direction Orders Freight", partInfo.GoodsHandlingInstructions);

			var outboundReleaseNote = warehouseNotes.AddNew();
			outboundReleaseNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			outboundReleaseNote.ST_NoteContextModule = nameof(StmNoteContextModule.W);
			outboundReleaseNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.O);
			outboundReleaseNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.R);
			outboundReleaseNote.ST_NoteDataAsText = "Testing Goods Instructions For Warehouse Module Outbound Direction Release Freight";

			partInfo = new WhsProductInfo(data.Part1, GoodsHandlingInstructionsType.OrderPicking);
			AssertEquals("Testing Goods Instructions For Warehouse Module Outbound Direction Release Freight", partInfo.GoodsHandlingInstructions);
		}

		public void TestGetInfo_GoodsHandlingInstructions_OrderPicking_WOT()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var partInfo = new WhsProductInfo(data.Part1, GoodsHandlingInstructionsType.OrderPicking);
			AssertNullOrEmpty(partInfo.GoodsHandlingInstructions);

			var warehouseNotes = data.Part1.Notes;
			var outboundTransferNote = warehouseNotes.AddNew();
			outboundTransferNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			outboundTransferNote.ST_NoteContextModule = nameof(StmNoteContextModule.W);
			outboundTransferNote.ST_NoteContextDirection = nameof(StmNoteContextDirection.O);
			outboundTransferNote.ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.T);
			outboundTransferNote.ST_NoteDataAsText = "Testing Goods Instructions For Warehouse Module Outbound Direction Transfer Freight";

			partInfo = new WhsProductInfo(data.Part1, GoodsHandlingInstructionsType.OrderPicking);
			AssertNullOrEmpty(partInfo.GoodsHandlingInstructions);
		}

		#endregion

		#region Properties

		#region TestPK

		public void TestPK()
		{
			AssertEquals(Guid.Empty, Parent.PK);

			Guid newGuid = new Guid();
			Parent.PK = newGuid;
			AssertEquals(newGuid, Parent.PK);
		}

		#endregion

		#region TestCode

		public void TestCode()
		{
			AssertEquals("", Parent.Code);

			Parent.Code = "1234";
			AssertEquals("1234", Parent.Code);

			Parent.Code = "4321";
			AssertEquals("4321", Parent.Code);
		}

		#endregion

		#region TestDesription

		public void TestDesription()
		{
			AssertEquals("", Parent.Description);

			Parent.Description = "1234";
			AssertEquals("1234", Parent.Description);

			Parent.Description = "4321";
			AssertEquals("4321", Parent.Description);
		}

		#endregion

		#region TestStockUnit

		public void TestStockUnit()
		{
			AssertEquals("", Parent.StockUnit);

			Parent.StockUnit = "1234";
			AssertEquals("1234", Parent.StockUnit);

			Parent.StockUnit = "4321";
			AssertEquals("4321", Parent.StockUnit);
		}

		#endregion

		#region TestWeight

		public void TestWeight()
		{
			AssertEquals(0m, Parent.Weight);

			Parent.Weight = 10m;
			AssertEquals(10m, Parent.Weight);

			Parent.Weight = 20m;
			AssertEquals(20m, Parent.Weight);
		}

		#endregion

		#region TestWeightUQ

		public void TestWeightUQ()
		{
			AssertEquals("", Parent.WeightUQ);

			Parent.WeightUQ = "1234";
			AssertEquals("1234", Parent.WeightUQ);

			Parent.WeightUQ = "4321";
			AssertEquals("4321", Parent.WeightUQ);
		}

		#endregion

		#region TestDepth

		public void TestDepth()
		{
			AssertEquals(0m, Parent.Depth);

			Parent.Depth = 10m;
			AssertEquals(10m, Parent.Depth);

			Parent.Depth = 20m;
			AssertEquals(20m, Parent.Depth);
		}

		#endregion

		#region TestWidth

		public void TestWidth()
		{
			AssertEquals(0m, Parent.Width);

			Parent.Width = 10m;
			AssertEquals(10m, Parent.Width);

			Parent.Width = 20m;
			AssertEquals(20m, Parent.Width);
		}

		#endregion

		#region TestHeight

		public void TestHeight()
		{
			AssertEquals(0m, Parent.Height);

			Parent.Height = 10m;
			AssertEquals(10m, Parent.Height);

			Parent.Height = 20m;
			AssertEquals(20m, Parent.Height);
		}

		#endregion

		#region TestMeasureUQ

		public void TestMeasureUQ()
		{
			AssertEquals("", Parent.MeasureUQ);

			Parent.MeasureUQ = "1234";
			AssertEquals("1234", Parent.MeasureUQ);

			Parent.MeasureUQ = "4321";
			AssertEquals("4321", Parent.MeasureUQ);
		}

		#endregion

		#region TestCubic

		public void TestCubic()
		{
			AssertEquals(0m, Parent.Cubic);

			Parent.Cubic = 10m;
			AssertEquals(10m, Parent.Cubic);

			Parent.Cubic = 20m;
			AssertEquals(20m, Parent.Cubic);
		}

		#endregion

		#region TestCubicUQ

		public void TestCubicUQ()
		{
			AssertEquals("", Parent.CubicUQ);

			Parent.CubicUQ = "1234";
			AssertEquals("1234", Parent.CubicUQ);

			Parent.CubicUQ = "4321";
			AssertEquals("4321", Parent.CubicUQ);
		}

		#endregion

		#region TestPalletSize

		public void TestPalletSize()
		{
			AssertEquals(0m, Parent.PalletSize);

			Parent.PalletSize = 10m;
			AssertEquals(10m, Parent.PalletSize);

			Parent.PalletSize = 20m;
			AssertEquals(20m, Parent.PalletSize);
		}

		#endregion

		#region TestDecimalPlaces

		public void TestDecimalPlaces()
		{
			AssertEquals(0, Parent.DecimalPlaces);

			Parent.DecimalPlaces = 2;
			AssertEquals(2, Parent.DecimalPlaces);

			Parent.DecimalPlaces = 3;
			AssertEquals(3, Parent.DecimalPlaces);
		}

		#endregion

		#region TestIsBarcoded

		public void TestIsBarcoded()
		{
			AssertEquals(true, Parent.IsBarcoded);

			Parent.IsBarcoded = false;
			AssertEquals(false, Parent.IsBarcoded);

			Parent.IsBarcoded = true;
			AssertEquals(true, Parent.IsBarcoded);
		}

		#endregion

		#region TestProductUnits

		public void TestProductUnits()
		{
			AssertNotNull(Parent.ProductUnits);
			AssertEquals(0, Parent.ProductUnits.Count);

			var productUnit1 = new WhsUnitRateInfo();
			var productUnit2 = new WhsUnitRateInfo();
			AssertNotEquals(productUnit1, productUnit2);
			Parent.ProductUnits.Add(productUnit1);
			Parent.ProductUnits.Add(productUnit2);
			AssertCollectionContains(productUnit1, Parent.ProductUnits);
			AssertCollectionContains(productUnit2, Parent.ProductUnits);

			var productUnits = new WhsUnitRateInfoCollection();
			productUnits.Add(new WhsUnitRateInfo());
			productUnits.Add(new WhsUnitRateInfo());
			AssertNotEquals(Parent.ProductUnits, productUnits);
			Parent.ProductUnits = productUnits;
			AssertEquals(Parent.ProductUnits, productUnits);
		}

		#endregion

		#region TestPickFaces

		public void TestPickFaces()
		{
			AssertNotNull(Parent.PickFaces);
			AssertEquals(0, Parent.PickFaces.Count);

			var pickFace1 = new WhsPickFaceInfo();
			var pickFace2 = new WhsPickFaceInfo();
			AssertNotEquals(pickFace1, pickFace2);
			Parent.PickFaces.Add(pickFace1);
			Parent.PickFaces.Add(pickFace2);
			AssertCollectionContains(pickFace1, Parent.PickFaces);
			AssertCollectionContains(pickFace2, Parent.PickFaces);

			var pickFaces = new WhsPickFaceInfoCollection();
			pickFaces.Add(new WhsPickFaceInfo());
			pickFaces.Add(new WhsPickFaceInfo());
			AssertNotEquals(Parent.PickFaces, pickFaces);
			Parent.PickFaces = pickFaces;
			AssertEquals(Parent.PickFaces, pickFaces);
		}

		#endregion

		#region TestGoodsHandlingInstructions

		public void TestGoodsHandlingInstructions()
		{
			AssertEquals(null, Parent.GoodsHandlingInstructions);

			Parent.GoodsHandlingInstructions = "Only move while dancing.";
			AssertEquals("Only move while dancing.", Parent.GoodsHandlingInstructions);

			Parent.GoodsHandlingInstructions = "Carefully";
			AssertEquals("Carefully", Parent.GoodsHandlingInstructions);
		}

		#endregion

		#endregion

		#region Implementation

		protected new WhsProductInfo Parent
		{
			get
			{
				return (WhsProductInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsProductInfo();
		}

		#endregion
	}
}
