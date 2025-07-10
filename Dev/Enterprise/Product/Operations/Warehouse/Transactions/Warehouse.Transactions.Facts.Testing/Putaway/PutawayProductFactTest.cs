using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Moq;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehousePutaway;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class PutawayProductFactTest : ProductFactTest
	{
		public void TestPutawayGroupCode()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			productMock.SetupGet(p => p.PK).Returns(ZGuid.BrettsGuid);

			var productFact = new PutawayProductFact(productMock.Object, GetPartRelationMock(), "", "ABC", "Style1", "M", "RED", "SML", null, false);
			AssertEquals(nameof(PutawayProductFact.PutawayGroupCode), "ABC", productFact.PutawayGroupCode);
			AssertEquals(nameof(PutawayProductFact.PutawayGroupCode), "ABC", ((IPutawayProductFact)productFact).PutawayGroupCode);
		}

		public void TestPutawayGroupCode_NullString()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			productMock.SetupGet(p => p.PK).Returns(ZGuid.BrettsGuid);

			var productFact = new PutawayProductFact(productMock.Object, GetPartRelationMock(), "", null, "Style1", "M", "RED", "SML", null, false);
			AssertEquals(nameof(PutawayProductFact.PutawayGroupCode), "", productFact.PutawayGroupCode);
			AssertEquals(nameof(PutawayProductFact.PutawayGroupCode), "", ((IPutawayProductFact)productFact).PutawayGroupCode);
		}

		public void TestStockUQ()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			productMock.SetupGet(p => p.PK).Returns(ZGuid.BrettsGuid);
			productMock.SetupGet(p => p.OP_StockKeepingUnit).Returns("UNT");

			var productFact = new PutawayProductFact(productMock.Object, GetPartRelationMock(), "", "", "Style1", "M", "RED", "SML", null, false);
			AssertEquals(nameof(PutawayProductFact.StockUQ), "UNT", productFact.StockUQ);
			AssertEquals(nameof(IPutawayProductFact.StockUQ), "UNT", ((IPutawayProductFact)productFact).StockUQ);
			productMock.VerifyGet(p => p.OP_StockKeepingUnit);
		}

		public void TestABCCategoryCode()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			productMock.SetupGet(p => p.PK).Returns(ZGuid.BrettsGuid);

			var productFact = new PutawayProductFact(productMock.Object, GetPartRelationMock(), "ABC", "", "Style1", "M", "RED", "SML", null, false);
			AssertEquals(nameof(PutawayProductFact.ABCCategoryCode), "ABC", productFact.ABCCategoryCode);
			AssertEquals(nameof(IPutawayProductFact.ABCCategoryCode), "ABC", ((IPutawayProductFact)productFact).ABCCategoryCode);
		}

		public void TestABCCategoryCode_NullString()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			productMock.SetupGet(p => p.PK).Returns(ZGuid.BrettsGuid);

			var productFact = new PutawayProductFact(productMock.Object, GetPartRelationMock(), null, "", "Style1", "M", "RED", "SML", null, false);
			AssertEquals(nameof(PutawayProductFact.ABCCategoryCode), "", productFact.ABCCategoryCode);
			AssertEquals(nameof(IPutawayProductFact.ABCCategoryCode), "", ((IPutawayProductFact)productFact).ABCCategoryCode);
		}

		public void TestUnitPrice()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			productMock.SetupGet(p => p.PK).Returns(ZGuid.BrettsGuid);

			var partRelationMock = new Mock<IOrgPartRelation>();
			partRelationMock.SetupGet(pr => pr.OU_UnitPrice).Returns(484.596m);
			partRelationMock.SetupGet(pr => pr.OU_RX_NKUnitPriceCurrency).Returns("USD");
			partRelationMock.Setup(opr => opr.PK).Returns(Guid.NewGuid());

			var productFact = new PutawayProductFact(productMock.Object, partRelationMock.Object, "", "", "Style1", "M", "RED", "SML", null, false);
			AssertEquals(nameof(PutawayProductFact.UnitPrice) + "Value", 484.596m, productFact.UnitPrice.Value);
			AssertEquals(nameof(PutawayProductFact.UnitPrice) + "Unit", "USD", productFact.UnitPrice.Unit);
			partRelationMock.VerifyGet(pr => pr.OU_UnitPrice);
			partRelationMock.VerifyGet(pr => pr.OU_RX_NKUnitPriceCurrency);
		}

		public void TestUnitPrice_EmptyPrice()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			productMock.SetupGet(p => p.PK).Returns(ZGuid.BrettsGuid);

			var productFact = new PutawayProductFact(productMock.Object, GetPartRelationMock(), "", "", "Style1", "M", "RED", "SML", null, false);
			AssertEquals(nameof(PutawayProductFact.UnitPrice) + "Value", 0m, productFact.UnitPrice.Value);
			AssertEquals(nameof(PutawayProductFact.UnitPrice) + "Unit", "", productFact.UnitPrice.Unit);
		}

		public void TestDGInfo()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			productMock.SetupGet(p => p.PK).Returns(ZGuid.BrettsGuid);

			var firstDGFact = Mock.Of<IDangerousGoodsFact>();

			var productFact1 = new PutawayProductFact(productMock.Object, GetPartRelationMock(), "", "", "Style1", "M", "RED", "SML", firstDGFact, true);
			AssertEquals(nameof(PutawayProductFact.FirstDG), firstDGFact, productFact1.FirstDG.Fact);
			AssertEquals(nameof(PutawayProductFact.HasMultipleDangerousGoods), true, productFact1.HasMultipleDangerousGoods);

			var productFact2 = new PutawayProductFact(productMock.Object, GetPartRelationMock(), "", "", "Style1", "M", "RED", "SML", firstDGFact, false);
			AssertEquals(nameof(PutawayProductFact.HasMultipleDangerousGoods), false, productFact2.HasMultipleDangerousGoods);
		}

		protected override ProductFact GetProductFact(IOrgSupplierPart part, IOrgPartRelation relation)
		{
			return new PutawayProductFact(part, relation, "", "", "Style1", "M", "RED", "SML", null, false);
		}
	}
}
