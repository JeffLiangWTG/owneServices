using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class ProductFactTest_NonAbstract : ProductFactTest
	{
		protected override ProductFact GetProductFact(IOrgSupplierPart part, IOrgPartRelation relation)
			=> new ProductFact(part, relation, "Style1", "M", "RED", "SML");
	}

	class ProductFactTest_NonAbstractWithStyle : ProductFactTest
	{
		protected override void TestProductStyleCore() => Assert(true);

		protected override ProductFact GetProductFact(IOrgSupplierPart part, IOrgPartRelation relation)
			=> new ProductFact(part, relation);
	}

	abstract class ProductFactTest : TestCase
	{
		public void TestNullObject_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => GetProductFact(null, null));
			AssertExceptionThrown<ArgumentNullException>(() => GetProductFact(Mock.Of<IOrgSupplierPart>(), null));
			AssertExceptionThrown<ArgumentNullException>(() => GetProductFact(null, GetPartRelationMock()));
		}

		public void TestPK()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			var relationMock = new Mock<IOrgPartRelation>();
			relationMock.SetupGet(r => r.PK).Returns(ZGuid.BrettsGuid);

			var productFact = GetProductFact(productMock.Object, relationMock.Object);
			AssertEquals(nameof(ProductFact.PK), ZGuid.BrettsGuid, productFact.PK);
			AssertEquals(nameof(IProductFact.PK), ZGuid.BrettsGuid, ((IProductFact)productFact).PK);
			relationMock.VerifyGet(r => r.PK);
		}

		public void TestCode()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			productMock.SetupGet(p => p.PK).Returns(ZGuid.BrettsGuid);
			productMock.SetupGet(p => p.OP_PartNum).Returns("PR1");

			var productFact = GetProductFact(productMock.Object, GetPartRelationMock());
			AssertEquals(nameof(ProductFact.Code), "PR1", productFact.Code);
			AssertEquals(nameof(IProductFact.Code), "PR1", ((IProductFact)productFact).Code);
			productMock.VerifyGet(p => p.OP_PartNum);
		}

		public void TestCommodityCode()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			productMock.SetupGet(p => p.PK).Returns(ZGuid.BrettsGuid);
			productMock.SetupGet(p => p.OP_RH_NKCommodityCode).Returns("123");

			var productFact = GetProductFact(productMock.Object, GetPartRelationMock());
			AssertEquals(nameof(ProductFact.CommodityCode), "123", productFact.CommodityCode);
			AssertEquals(nameof(IProductFact.CommodityCode), "123", ((IProductFact)productFact).CommodityCode);
			productMock.VerifyGet(p => p.OP_RH_NKCommodityCode);
		}

		public void TestCategoryCode()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			productMock.SetupGet(p => p.PK).Returns(ZGuid.BrettsGuid);

			var partRelationMock = new Mock<IOrgPartRelation>();
			partRelationMock.SetupGet(pr => pr.CategoryCode).Returns("XYZ");
			partRelationMock.Setup(opr => opr.PK).Returns(Guid.NewGuid());

			var productFact = GetProductFact(productMock.Object, partRelationMock.Object);
			AssertEquals(nameof(ProductFact.CategoryCode), "XYZ", productFact.CategoryCode);
			AssertEquals(nameof(IProductFact.CategoryCode), "XYZ", ((IProductFact)productFact).CategoryCode);
			partRelationMock.VerifyGet(pr => pr.CategoryCode);
		}

		public void TestProductStyle()
		{
			TestProductStyleCore();
		}

		protected virtual void TestProductStyleCore()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			productMock.SetupGet(p => p.PK).Returns(ZGuid.NewZGuid());

			var partRelationMock = new Mock<IOrgPartRelation>();
			partRelationMock.SetupGet(pr => pr.CategoryCode).Returns("XYZ");
			partRelationMock.Setup(opr => opr.PK).Returns(Guid.NewGuid());

			var productFact = GetProductFact(productMock.Object, partRelationMock.Object);
			AssertEquals(nameof(ProductFact.ProductStyleCode), "Style1", productFact.ProductStyleCode);
			AssertEquals(nameof(ProductFact.ProductStyleClassificationCode), "M", productFact.ProductStyleClassificationCode);
			AssertEquals(nameof(ProductFact.ProductStyleColorCode), "RED", productFact.ProductStyleColorCode);
			AssertEquals(nameof(ProductFact.ProductStyleSizeCode), "SML", productFact.ProductStyleSizeCode);
			AssertEquals(nameof(IProductStyleFact.ProductStyleCode), "Style1", ((IProductStyleFact)productFact).ProductStyleCode);
			AssertEquals(nameof(IProductStyleFact.ProductStyleClassificationCode), "M", ((IProductStyleFact)productFact).ProductStyleClassificationCode);
			AssertEquals(nameof(IProductStyleFact.ProductStyleColorCode), "RED", ((IProductStyleFact)productFact).ProductStyleColorCode);
			AssertEquals(nameof(IProductStyleFact.ProductStyleSizeCode), "SML", ((IProductStyleFact)productFact).ProductStyleSizeCode);
		}

		public void TestCategoryCode_NullPartRelation()
		{
			var productMock = new Mock<IOrgSupplierPart>();
			productMock.SetupGet(p => p.PK).Returns(ZGuid.BrettsGuid);

			var productFact = GetProductFact(productMock.Object, GetPartRelationMock());
			AssertEquals(nameof(ProductFact.CategoryCode), "", productFact.CategoryCode);
			AssertEquals(nameof(IProductFact.CategoryCode), "", ((IProductFact)productFact).CategoryCode);
		}

		protected IOrgPartRelation GetPartRelationMock()
		{
			var partRelationMock = new Mock<IOrgPartRelation>();
			partRelationMock.Setup(opr => opr.PK).Returns(Guid.NewGuid());
			return partRelationMock.Object;
		}

		protected abstract ProductFact GetProductFact(IOrgSupplierPart part, IOrgPartRelation relation);
	}
}
