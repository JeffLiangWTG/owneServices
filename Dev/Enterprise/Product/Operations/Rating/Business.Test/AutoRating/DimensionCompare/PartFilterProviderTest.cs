using System;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Rating.Rateable;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.Business
{
	public class PartFilterProviderTest : TestCase
	{
		#region TestCreateForDocketPackage

		public void TestCreateForDocketPackage_Equals()
		{
			var partListMock = new Mock<IHasPartDimensions>();

			var rateablePart1Mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var rateablePart2Mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForDocketPackage();

			var partWithDimensions1 = new PartWithDimensions(partListMock.Object, rateablePart1Mock.Object);
			var partWithDimensions2 = new PartWithDimensions(partListMock.Object, rateablePart2Mock.Object);

			AssertEquals("part vs itself", true, filter.Equals(partWithDimensions1, partWithDimensions1));
			AssertEquals("part 1 vs 2", false, filter.Equals(partWithDimensions1, partWithDimensions2));

			AssertNoExceptionThrown(() => { filter.GetHashCode(partWithDimensions1); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(partWithDimensions2); });
		}

		public void TestCreateForDocketPackage_Satisfies()
		{
			var partListMock = new Mock<IHasPartDimensions>();

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForDocketPackage();

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partListMock.Object, part2mock.Object);

			AssertEquals("part vs itself", true, filter.Satisfies(part1, part1));
			AssertEquals("part with same value", false, filter.Satisfies(part1, part2));
		}

		#endregion

		#region TestCreateForDocket

		public void TestCreateForDocket_Equals()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasDocketReference).Returns(true);
			partListMock.Setup(x => x.HasProduct).Returns(true);
			partListMock.Setup(x => x.HasPackageType).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part3mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForDocket();

			part1mock.Setup(x => x.DocketReference).Returns("100");
			part2mock.Setup(x => x.DocketReference).Returns("100");
			part3mock.Setup(x => x.DocketReference).Returns("9999");
			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partListMock.Object, part2mock.Object);
			var part3 = new PartWithDimensions(partListMock.Object, part3mock.Object);

			AssertEquals("part vs itself", true, filter.Equals(part1, part1));
			AssertEquals("part 1 vs 2", true, filter.Equals(part1, part2));
			AssertEquals("part 1 vs 3", false, filter.Equals(part1, part3));

			AssertNoExceptionThrown(() => { filter.GetHashCode(part1); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part2); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part3); });
		}

		public void TestCreateForDocket_Satisfies()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasDocketReference).Returns(true);
			partListMock.Setup(x => x.HasProduct).Returns(true);
			partListMock.Setup(x => x.HasPackageType).Returns(true);

			var partList2Mock = new Mock<IHasPartDimensions>();
			partList2Mock.Setup(x => x.HasProduct).Returns(true);
			partList2Mock.Setup(x => x.HasPackageType).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part3mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part4mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForDocket();

			part1mock.Setup(x => x.DocketReference).Returns("100");
			part2mock.Setup(x => x.DocketReference).Returns("100");
			part3mock.Setup(x => x.DocketReference).Returns("9999");
			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partListMock.Object, part2mock.Object);
			var part3 = new PartWithDimensions(partListMock.Object, part3mock.Object);
			var partHasNoDocket = new PartWithDimensions(partList2Mock.Object, part4mock.Object);

			AssertEquals("part vs itself", true, filter.Satisfies(part1, part1));
			AssertEquals("part with same value", true, filter.Satisfies(part1, part2));
			AssertEquals("part with different value", false, filter.Satisfies(part1, part3));
			AssertEquals("part with no value", true, filter.Satisfies(part1, partHasNoDocket));
		}

		#endregion

		#region TestCreateForDocketProduct

		public void TestCreateForDocketProduct_Equals()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasDocketReference).Returns(true);
			partListMock.Setup(x => x.HasProduct).Returns(true);
			partListMock.Setup(x => x.HasProductAttributes).Returns(true);
			partListMock.Setup(x => x.HasPackageType).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part3mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part4mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part5mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForDocketProduct();
			var product1 = Guid.NewGuid();
			var product2 = Guid.NewGuid();
			var attributes1 = new ProductAttributesMeasure("AAA");
			var attributes2 = new ProductAttributesMeasure("BBB");

			part1mock.Setup(x => x.DocketReference).Returns("100");
			part1mock.Setup(x => x.ProductPk).Returns(product1);
			part1mock.Setup(x => x.ProductAttributes).Returns(attributes1);

			part2mock.Setup(x => x.DocketReference).Returns("100");
			part2mock.Setup(x => x.ProductPk).Returns(product1);
			part2mock.Setup(x => x.ProductAttributes).Returns(attributes1);

			part3mock.Setup(x => x.DocketReference).Returns("9999");
			part3mock.Setup(x => x.ProductPk).Returns(product1);
			part3mock.Setup(x => x.ProductAttributes).Returns(attributes1);

			part4mock.Setup(x => x.DocketReference).Returns("100");
			part4mock.Setup(x => x.ProductPk).Returns(product2);
			part4mock.Setup(x => x.ProductAttributes).Returns(attributes1);

			part5mock.Setup(x => x.DocketReference).Returns("100");
			part5mock.Setup(x => x.ProductPk).Returns(product2);
			part5mock.Setup(x => x.ProductAttributes).Returns(attributes2);

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partListMock.Object, part2mock.Object);
			var part3 = new PartWithDimensions(partListMock.Object, part3mock.Object);
			var part4 = new PartWithDimensions(partListMock.Object, part4mock.Object);
			var part5 = new PartWithDimensions(partListMock.Object, part5mock.Object);

			AssertEquals("part vs itself", true, filter.Equals(part1, part1));
			AssertEquals("part vs equal part", true, filter.Equals(part1, part2));
			AssertEquals("differ only in docket", false, filter.Equals(part1, part3));
			AssertEquals("differ only in product PK", false, filter.Equals(part1, part4));
			AssertEquals("differ only in product attributes", false, filter.Equals(part4, part5));
			AssertEquals("differ in all", false, filter.Equals(part3, part5));

			AssertNoExceptionThrown(() => { filter.GetHashCode(part1); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part2); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part3); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part4); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part5); });
		}

		public void TestCreateForDocketProduct_Satisfies_SamePartList()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasDocketReference).Returns(true);
			partListMock.Setup(x => x.HasProduct).Returns(true);
			partListMock.Setup(x => x.HasProductAttributes).Returns(true);
			partListMock.Setup(x => x.HasPackageType).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part3mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part4mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part5mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForDocketProduct();
			var product1 = Guid.NewGuid();
			var product2 = Guid.NewGuid();
			var attributes1 = new ProductAttributesMeasure("AAA");
			var attributes2 = new ProductAttributesMeasure("BBB");

			part1mock.Setup(x => x.DocketReference).Returns("100");
			part1mock.Setup(x => x.ProductPk).Returns(product1);
			part1mock.Setup(x => x.ProductAttributes).Returns(attributes1);

			part2mock.Setup(x => x.DocketReference).Returns("100");
			part2mock.Setup(x => x.ProductPk).Returns(product1);
			part2mock.Setup(x => x.ProductAttributes).Returns(attributes1);

			part3mock.Setup(x => x.DocketReference).Returns("9999");
			part3mock.Setup(x => x.ProductPk).Returns(product1);
			part3mock.Setup(x => x.ProductAttributes).Returns(attributes1);

			part4mock.Setup(x => x.DocketReference).Returns("100");
			part4mock.Setup(x => x.ProductPk).Returns(product2);
			part4mock.Setup(x => x.ProductAttributes).Returns(attributes1);

			part5mock.Setup(x => x.DocketReference).Returns("100");
			part5mock.Setup(x => x.ProductPk).Returns(product2);
			part5mock.Setup(x => x.ProductAttributes).Returns(attributes2);

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partListMock.Object, part2mock.Object);
			var part3 = new PartWithDimensions(partListMock.Object, part3mock.Object);
			var part4 = new PartWithDimensions(partListMock.Object, part4mock.Object);
			var part5 = new PartWithDimensions(partListMock.Object, part5mock.Object);

			AssertEquals("part vs itself", true, filter.Satisfies(part1, part1));
			AssertEquals("part vs equal part", true, filter.Satisfies(part1, part2));
			AssertEquals("differ only in docket", false, filter.Satisfies(part1, part3));
			AssertEquals("differ only in product PK", false, filter.Satisfies(part1, part4));
			AssertEquals("differ only in product attributes", false, filter.Satisfies(part4, part5));
			AssertEquals("differ in all", false, filter.Satisfies(part3, part5));
		}

		public void TestCreateForDocketProduct_Satisfies_PartListWithDocketOnly()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasDocketReference).Returns(true);
			partListMock.Setup(x => x.HasProduct).Returns(true);
			partListMock.Setup(x => x.HasProductAttributes).Returns(true);
			partListMock.Setup(x => x.HasPackageType).Returns(true);

			var partList2Mock = new Mock<IHasPartDimensions>();
			partList2Mock.Setup(x => x.HasDocketReference).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part3mock = new Mock<IRateablePart>(MockBehavior.Strict);

			var filter = PartFilterProvider.CreateForDocketProduct();
			var product1 = Guid.NewGuid();
			var attributes1 = new ProductAttributesMeasure("AAA");

			part1mock.Setup(x => x.DocketReference).Returns("100");
			part1mock.Setup(x => x.ProductPk).Returns(product1);
			part1mock.Setup(x => x.ProductAttributes).Returns(attributes1);

			part2mock.Setup(x => x.DocketReference).Returns("100");

			part3mock.Setup(x => x.DocketReference).Returns("9999");

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partList2Mock.Object, part2mock.Object);
			var part3 = new PartWithDimensions(partList2Mock.Object, part3mock.Object);

			AssertEquals("part with same docket", true, filter.Satisfies(part1, part2));
			AssertEquals("part with different docket", false, filter.Satisfies(part1, part3));
		}

		public void TestCreateForDocketProduct_Satisfies_PartListWithProductOnly()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasDocketReference).Returns(true);
			partListMock.Setup(x => x.HasProduct).Returns(true);
			partListMock.Setup(x => x.HasProductAttributes).Returns(true);
			partListMock.Setup(x => x.HasPackageType).Returns(true);

			var partList2Mock = new Mock<IHasPartDimensions>();
			partList2Mock.Setup(x => x.HasProduct).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part3mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part4mock = new Mock<IRateablePart>(MockBehavior.Strict);

			var filter = PartFilterProvider.CreateForDocketProduct();
			var product1 = Guid.NewGuid();
			var product2 = Guid.NewGuid();
			var attributes1 = new ProductAttributesMeasure("AAA");
			var attributes2 = new ProductAttributesMeasure("BBB");

			part1mock.Setup(x => x.DocketReference).Returns("100");
			part1mock.Setup(x => x.ProductPk).Returns(product1);
			part1mock.Setup(x => x.ProductAttributes).Returns(attributes1);

			part2mock.Setup(x => x.ProductPk).Returns(product1);
			part2mock.Setup(x => x.ProductAttributes).Returns(attributes1);

			part3mock.Setup(x => x.ProductPk).Returns(product2);
			part3mock.Setup(x => x.ProductAttributes).Returns(attributes1);

			part4mock.Setup(x => x.ProductPk).Returns(product1);
			part4mock.Setup(x => x.ProductAttributes).Returns(attributes2);

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partList2Mock.Object, part2mock.Object);
			var part3 = new PartWithDimensions(partList2Mock.Object, part3mock.Object);
			var part4 = new PartWithDimensions(partList2Mock.Object, part4mock.Object);

			AssertEquals("part with same product", true, filter.Satisfies(part1, part2));
			AssertEquals("part with different product", false, filter.Satisfies(part1, part3));
			AssertEquals("part with different product attributes", false, filter.Satisfies(part1, part4));
		}

		public void TestCreateForDocketProduct_Satisfies_PartListWithNoMatchingDimension()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasDocketReference).Returns(true);
			partListMock.Setup(x => x.HasProduct).Returns(true);
			partListMock.Setup(x => x.HasProductAttributes).Returns(true);
			partListMock.Setup(x => x.HasPackageType).Returns(true);

			var partList2Mock = new Mock<IHasPartDimensions>();
			partList2Mock.Setup(x => x.HasPackageType).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);

			var filter = PartFilterProvider.CreateForDocketProduct();
			var product1 = Guid.NewGuid();
			var attributes1 = new ProductAttributesMeasure("AAA");

			part1mock.Setup(x => x.DocketReference).Returns("100");
			part1mock.Setup(x => x.ProductPk).Returns(product1);
			part1mock.Setup(x => x.ProductAttributes).Returns(attributes1);

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partList2Mock.Object, part2mock.Object);

			AssertEquals("part with no matching dimensions", true, filter.Satisfies(part1, part2));
		}

		#endregion

		#region TestCreateForDocketProductPackageType

		public void TestCreateForDocketProductPackageType_Equals()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasDocketReference).Returns(true);
			partListMock.Setup(x => x.HasProduct).Returns(true);
			partListMock.Setup(x => x.HasProductAttributes).Returns(true);
			partListMock.Setup(x => x.HasPackageType).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part3mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForDocketProduct();
			var product1 = Guid.NewGuid();
			var product2 = Guid.NewGuid();
			var attributes1 = new ProductAttributesMeasure("AAA");
			var attributes2 = new ProductAttributesMeasure("BBB");

			part1mock.Setup(x => x.DocketReference).Returns("100");
			part1mock.Setup(x => x.ProductPk).Returns(product1);
			part1mock.Setup(x => x.ProductAttributes).Returns(attributes1);
			part1mock.Setup(x => x.PackageType).Returns("P01");

			part2mock.Setup(x => x.DocketReference).Returns("100");
			part2mock.Setup(x => x.ProductPk).Returns(product1);
			part2mock.Setup(x => x.ProductAttributes).Returns(attributes1);
			part1mock.Setup(x => x.PackageType).Returns("P02");

			part3mock.Setup(x => x.DocketReference).Returns("9999");
			part3mock.Setup(x => x.ProductPk).Returns(product2);
			part3mock.Setup(x => x.ProductAttributes).Returns(attributes2);
			part3mock.Setup(x => x.PackageType).Returns("P02");

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partListMock.Object, part2mock.Object);
			var part3 = new PartWithDimensions(partListMock.Object, part3mock.Object);

			AssertEquals("part vs itself", true, filter.Equals(part1, part1));
			AssertEquals("part vs equal part", true, filter.Equals(part1, part2));
			AssertEquals("differ in all", false, filter.Equals(part1, part3));

			AssertNoExceptionThrown(() => { filter.GetHashCode(part1); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part2); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part3); });
		}

		public void TestCreateForDocketProductPackageType_Satisfies_SamePartList()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasDocketReference).Returns(true);
			partListMock.Setup(x => x.HasProduct).Returns(true);
			partListMock.Setup(x => x.HasProductAttributes).Returns(true);
			partListMock.Setup(x => x.HasPackageType).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part3mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForDocketProduct();
			var product1 = Guid.NewGuid();
			var product2 = Guid.NewGuid();
			var attributes1 = new ProductAttributesMeasure("AAA");
			var attributes2 = new ProductAttributesMeasure("BBB");

			part1mock.Setup(x => x.DocketReference).Returns("100");
			part1mock.Setup(x => x.ProductPk).Returns(product1);
			part1mock.Setup(x => x.ProductAttributes).Returns(attributes1);
			part1mock.Setup(x => x.PackageType).Returns("P01");

			part2mock.Setup(x => x.DocketReference).Returns("100");
			part2mock.Setup(x => x.ProductPk).Returns(product1);
			part2mock.Setup(x => x.ProductAttributes).Returns(attributes1);
			part1mock.Setup(x => x.PackageType).Returns("P02");

			part3mock.Setup(x => x.DocketReference).Returns("9999");
			part3mock.Setup(x => x.ProductPk).Returns(product2);
			part3mock.Setup(x => x.ProductAttributes).Returns(attributes2);
			part3mock.Setup(x => x.PackageType).Returns("P02");

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partListMock.Object, part2mock.Object);
			var part3 = new PartWithDimensions(partListMock.Object, part3mock.Object);

			AssertEquals("part vs itself", true, filter.Satisfies(part1, part1));
			AssertEquals("part vs equal part", true, filter.Satisfies(part1, part2));
			AssertEquals("differ in all", false, filter.Satisfies(part1, part3));
		}

		public void TestCreateForDocketProductPackageType_Satisfies_PartListWithNoMatchingDimensions()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasDocketReference).Returns(true);
			partListMock.Setup(x => x.HasProduct).Returns(true);
			partListMock.Setup(x => x.HasProductAttributes).Returns(true);
			partListMock.Setup(x => x.HasPackageType).Returns(true);

			var partList2Mock = new Mock<IHasPartDimensions>();
			partList2Mock.Setup(x => x.HasContainerType).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForDocketProduct();
			var product1 = Guid.NewGuid();
			var attributes1 = new ProductAttributesMeasure("AAA");

			part1mock.Setup(x => x.DocketReference).Returns("100");
			part1mock.Setup(x => x.ProductPk).Returns(product1);
			part1mock.Setup(x => x.ProductAttributes).Returns(attributes1);
			part1mock.Setup(x => x.PackageType).Returns("P01");

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partList2Mock.Object, part2mock.Object);

			AssertEquals("part with no matching dimensions", true, filter.Satisfies(part1, part2));
		}

		#endregion

		#region TestCreateForLocation

		public void TestCreateForLocation_Equals()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasLocation).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part3mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part4mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForLocation();

			var loc1 = new LocationMeasure(ZGuid.NewZGuid(), "AAA", "LOC1");
			var loc2 = new LocationMeasure(ZGuid.NewZGuid(), "BBB", "LOC2");

			part1mock.Setup(x => x.Location).Returns(loc1);
			part2mock.Setup(x => x.Location).Returns(loc1);
			part3mock.Setup(x => x.Location).Returns(loc2);
			part4mock.Setup(x => x.Location).Returns((LocationMeasure)null);

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partListMock.Object, part2mock.Object);
			var part3 = new PartWithDimensions(partListMock.Object, part3mock.Object);
			var part4 = new PartWithDimensions(partListMock.Object, part4mock.Object);

			AssertEquals("part vs itself", true, filter.Equals(part1, part1));
			AssertEquals("part vs equal part", true, filter.Equals(part1, part2));
			AssertEquals("loc1 vs loc2", false, filter.Equals(part1, part3));
			AssertEquals("loc1 vs null", false, filter.Equals(part1, part4));
			AssertEquals("part with null vs itself", true, filter.Equals(part4, part4));

			AssertNoExceptionThrown(() => { filter.GetHashCode(part1); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part2); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part3); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part4); });
		}

		public void TestCreateForLocation_Satisfies()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasLocation).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part3mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part4mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForLocation();

			var loc1 = new LocationMeasure(ZGuid.NewZGuid(), "AAA", "LOC1");
			var loc2 = new LocationMeasure(ZGuid.NewZGuid(), "BBB", "LOC2");

			part1mock.Setup(x => x.Location).Returns(loc1);
			part2mock.Setup(x => x.Location).Returns(loc1);
			part3mock.Setup(x => x.Location).Returns(loc2);
			part4mock.Setup(x => x.Location).Returns((LocationMeasure)null);

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partListMock.Object, part2mock.Object);
			var part3 = new PartWithDimensions(partListMock.Object, part3mock.Object);
			var part4 = new PartWithDimensions(partListMock.Object, part4mock.Object);

			AssertEquals("part vs itself", true, filter.Satisfies(part1, part1));
			AssertEquals("part vs equal part", true, filter.Satisfies(part1, part2));
			AssertEquals("loc1 vs loc2", false, filter.Satisfies(part1, part3));
			AssertEquals("loc1 vs null", false, filter.Satisfies(part1, part4));
			AssertEquals("part with null vs itself", true, filter.Satisfies(part4, part4));
		}

		public void TestCreateForLocation_Satisfies_PartListWithNoMatchingDimensions()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasLocation).Returns(true);

			var partList2Mock = new Mock<IHasPartDimensions>();
			partList2Mock.Setup(x => x.HasPackageType).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForLocation();

			var loc1 = new LocationMeasure(ZGuid.NewZGuid(), "AAA", "LOC1");

			part1mock.Setup(x => x.Location).Returns(loc1);

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partList2Mock.Object, part2mock.Object);

			AssertEquals("part with no location", true, filter.Satisfies(part1, part2));
		}

		#endregion

		#region TestCreateForLocalTransport

		public void TestCreateForLocalTransport_Equals()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasCartageLegPK).Returns(true);
			partListMock.Setup(x => x.HasContainerType).Returns(true);
			partListMock.Setup(x => x.HasContainerNumber).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part3mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part4mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part5mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForLocalTransport();

			var leg1 = Guid.NewGuid();
			var leg2 = Guid.NewGuid();
			var containerType1 = Guid.NewGuid();
			var containerType2 = Guid.NewGuid();
			var num1 = "AAAA1234566";
			var num2 = "BBBB9999991";

			part1mock.Setup(x => x.CartageLegPK).Returns(leg1);
			part1mock.Setup(x => x.ContainerTypePk).Returns(containerType1);
			part1mock.Setup(x => x.ContainerNumber).Returns(num1);

			part2mock.Setup(x => x.CartageLegPK).Returns(leg1);
			part2mock.Setup(x => x.ContainerTypePk).Returns(containerType1);
			part2mock.Setup(x => x.ContainerNumber).Returns(num1);

			part3mock.Setup(x => x.CartageLegPK).Returns(leg1);
			part3mock.Setup(x => x.ContainerTypePk).Returns(containerType1);
			part3mock.Setup(x => x.ContainerNumber).Returns(num2);

			part4mock.Setup(x => x.CartageLegPK).Returns(leg1);
			part4mock.Setup(x => x.ContainerTypePk).Returns(containerType2);
			part4mock.Setup(x => x.ContainerNumber).Returns(num1);

			part5mock.Setup(x => x.CartageLegPK).Returns(leg2);
			part5mock.Setup(x => x.ContainerTypePk).Returns(containerType1);
			part5mock.Setup(x => x.ContainerNumber).Returns(num1);

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partListMock.Object, part2mock.Object);
			var part3 = new PartWithDimensions(partListMock.Object, part3mock.Object);
			var part4 = new PartWithDimensions(partListMock.Object, part4mock.Object);
			var part5 = new PartWithDimensions(partListMock.Object, part5mock.Object);

			AssertEquals("part vs itself", true, filter.Equals(part1, part1));
			AssertEquals("part vs equal part", true, filter.Equals(part1, part2));
			AssertEquals("different number", false, filter.Equals(part1, part3));
			AssertEquals("different container type", false, filter.Equals(part1, part4));
			AssertEquals("different leg", false, filter.Equals(part1, part5));

			AssertNoExceptionThrown(() => { filter.GetHashCode(part1); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part2); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part3); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part4); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(part5); });
		}

		public void TestCreateForLocalTransport_Satisfies_PartListWithSomeMatchingDimensions()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasCartageLegPK).Returns(true);
			partListMock.Setup(x => x.HasContainerType).Returns(true);
			partListMock.Setup(x => x.HasContainerNumber).Returns(true);

			var partList2Mock = new Mock<IHasPartDimensions>();
			partList2Mock.Setup(x => x.HasContainerType).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part3mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForLocalTransport();

			var leg1 = Guid.NewGuid();
			var containerType1 = Guid.NewGuid();
			var containerType2 = Guid.NewGuid();
			var num1 = "AAAA1234566";

			part1mock.Setup(x => x.CartageLegPK).Returns(leg1);
			part1mock.Setup(x => x.ContainerTypePk).Returns(containerType1);
			part1mock.Setup(x => x.ContainerNumber).Returns(num1);

			part2mock.Setup(x => x.ContainerTypePk).Returns(containerType1);

			part3mock.Setup(x => x.ContainerTypePk).Returns(containerType2);

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partList2Mock.Object, part2mock.Object);
			var part3 = new PartWithDimensions(partList2Mock.Object, part3mock.Object);

			AssertEquals("part with same value", true, filter.Satisfies(part1, part2));
			AssertEquals("part with different value", false, filter.Equals(part1, part3));
		}

		public void TestCreateForLocalTransport_Satisfies_PartListWithNoMatchingDimensions()
		{
			var partListMock = new Mock<IHasPartDimensions>();
			partListMock.Setup(x => x.HasCartageLegPK).Returns(true);
			partListMock.Setup(x => x.HasContainerType).Returns(true);
			partListMock.Setup(x => x.HasContainerNumber).Returns(true);

			var partList2Mock = new Mock<IHasPartDimensions>();
			partList2Mock.Setup(x => x.HasPackageType).Returns(true);

			var part1mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var part2mock = new Mock<IRateablePart>(MockBehavior.Strict);
			var filter = PartFilterProvider.CreateForLocalTransport();

			var leg1 = Guid.NewGuid();
			var containerType1 = Guid.NewGuid();
			var num1 = "AAAA1234566";

			part1mock.Setup(x => x.CartageLegPK).Returns(leg1);
			part1mock.Setup(x => x.ContainerTypePk).Returns(containerType1);
			part1mock.Setup(x => x.ContainerNumber).Returns(num1);

			var part1 = new PartWithDimensions(partListMock.Object, part1mock.Object);
			var part2 = new PartWithDimensions(partList2Mock.Object, part2mock.Object);

			AssertEquals("part with no matching dimensions", true, filter.Satisfies(part1, part2));
		}

		#endregion

		#region TestCreateForContainer

		public void TestCreateForContainer_Equals()
		{
			var filter = PartFilterProvider.CreateForContainer();
			AssertCreateForContainerMatch(false, (part1, part2) => filter.Equals(part1, part2));
		}

		public void TestCreateForContainer_Satisfies()
		{
			var filter = PartFilterProvider.CreateForContainer();
			AssertCreateForContainerMatch(true, (part1, part2) => filter.Satisfies(part1, part2));
		}

		void AssertCreateForContainerMatch(bool isLooseMatch, Func<PartWithDimensions, PartWithDimensions, bool> testAction)
		{
			var parts = CreateForContainerTestData();

			AssertEquals("part vs itself", true, testAction(parts.PartType1Num1, parts.PartType1Num1));
			AssertEquals("same container type but different container number", false, testAction(parts.PartType1Num1, parts.PartType1Num2));
			AssertEquals("same container number but different container type", false, testAction(parts.PartType1Num1, parts.PartType2Num1));
			AssertEquals("same container number and same container type", true, testAction(parts.PartType2Num1, parts.PartType2Num1_2));

			AssertEquals("nothing defined vs nothing defined", true, testAction(parts.PartNone, parts.PartNone2));
			AssertEquals("defined container number & type vs nothing defined", isLooseMatch, testAction(parts.PartType1Num1, parts.PartNone));
			AssertEquals("defined container number vs nothing defined", isLooseMatch, testAction(parts.PartNoType, parts.PartNone));
			AssertEquals("defined container type vs nothing defined", isLooseMatch, testAction(parts.PartNoNumber, parts.PartNone));
			AssertEquals("defined container type vs defined container number", false, testAction(parts.PartNoNumber, parts.PartNoType));
		}

		public void TestCreateForContainer_Implementation()
		{
			var parts = CreateForContainerTestData();
			var filter = PartFilterProvider.CreateForContainer();

			AssertNoExceptionThrown(() => { filter.GetHashCode(parts.PartType1Num1); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(parts.PartType1Num2); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(parts.PartType2Num1); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(parts.PartType2Num1_2); });

			AssertNoExceptionThrown(() => { filter.GetHashCode(parts.PartNoType); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(parts.PartNoNumber); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(parts.PartNone); });
			AssertNoExceptionThrown(() => { filter.GetHashCode(parts.PartNone2); });

			AssertEquals(filter.GetHashCode(parts.PartType1Num1), filter.GetHashCode(parts.PartType1Num1));
			AssertNotEquals(filter.GetHashCode(parts.PartType1Num1), filter.GetHashCode(parts.PartType1Num2));
			AssertNotEquals(filter.GetHashCode(parts.PartType1Num1), filter.GetHashCode(parts.PartType2Num1));
			AssertEquals(filter.GetHashCode(parts.PartType2Num1), filter.GetHashCode(parts.PartType2Num1_2));

			AssertEquals(filter.GetHashCode(parts.PartNone), filter.GetHashCode(parts.PartNone2));
			AssertNotEquals(filter.GetHashCode(parts.PartType1Num1), filter.GetHashCode(parts.PartNone));
			AssertNotEquals(filter.GetHashCode(parts.PartNoType), filter.GetHashCode(parts.PartNone));
			AssertNotEquals(filter.GetHashCode(parts.PartNoNumber), filter.GetHashCode(parts.PartNone));
			AssertNotEquals(filter.GetHashCode(parts.PartNoNumber), filter.GetHashCode(parts.PartNoType));

			AssertEquals(true, filter.HasAnyDimensions(parts.PartType1Num1.HasDimensions));
			AssertEquals(true, filter.HasAnyDimensions(parts.PartType1Num2.HasDimensions));
			AssertEquals(true, filter.HasAnyDimensions(parts.PartType2Num1.HasDimensions));
			AssertEquals(true, filter.HasAnyDimensions(parts.PartType2Num1_2.HasDimensions));

			AssertEquals(false, filter.HasAnyDimensions(parts.PartNone.HasDimensions));
			AssertEquals(true, filter.HasAnyDimensions(parts.PartNoType.HasDimensions));
			AssertEquals(true, filter.HasAnyDimensions(parts.PartNoNumber.HasDimensions));
			AssertEquals(false, filter.HasAnyDimensions(parts.PartNone2.HasDimensions));
		}

		PartWithDimensionsTestData CreateForContainerTestData()
		{
			var containerType1 = Guid.NewGuid();
			var containerType2 = Guid.NewGuid();
			var num1 = "AAAA1234566";
			var num2 = "AAAA1234567";

			var partList = GetHasPartDimensionsMockObject(true, true);
			var partListNone = GetHasPartDimensionsMockObject(false, false);
			var partListNoType = GetHasPartDimensionsMockObject(true, false);
			var partListNoNumber = GetHasPartDimensionsMockObject(false, true);

			var partType1Num1 = GetRateablePartMockObject(containerType1, num1);
			var partType1Num2 = GetRateablePartMockObject(containerType1, num2);
			var partType2Num1 = GetRateablePartMockObject(containerType2, num1);
			var partType2Num1_2 = GetRateablePartMockObject(containerType2, num1);

			var partNone = GetRateablePartMockObject(null, null);
			var partNoType = GetRateablePartMockObject(null, num1);
			var partNoNumber = GetRateablePartMockObject(containerType1, null);
			var partNone2 = GetRateablePartMockObject(null, null);

			var parts = new PartWithDimensionsTestData();
			parts.PartType1Num1 = new PartWithDimensions(partList, partType1Num1);
			parts.PartType1Num2 = new PartWithDimensions(partList, partType1Num2);
			parts.PartType2Num1 = new PartWithDimensions(partList, partType2Num1);
			parts.PartType2Num1_2 = new PartWithDimensions(partList, partType2Num1_2);

			parts.PartNone = new PartWithDimensions(partListNone, partNone);
			parts.PartNoType = new PartWithDimensions(partListNoType, partNoType);
			parts.PartNoNumber = new PartWithDimensions(partListNoNumber, partNoNumber);
			parts.PartNone2 = new PartWithDimensions(partListNone, partNone2);

			return parts;

			IHasPartDimensions GetHasPartDimensionsMockObject(bool hasContainerNumber, bool hasContainerType)
			{
				var hasPartDimensionsMock = new Mock<IHasPartDimensions>(MockBehavior.Strict);
				hasPartDimensionsMock.Setup(x => x.HasContainerNumber).Returns(hasContainerNumber);
				hasPartDimensionsMock.Setup(x => x.HasContainerType).Returns(hasContainerType);

				return hasPartDimensionsMock.Object;
			}

			IRateablePart GetRateablePartMockObject(Guid? containerType, string containerNumber)
			{
				var rateablePartMock = new Mock<IRateablePart>(MockBehavior.Strict);
				rateablePartMock.Setup(x => x.ContainerTypePk).Returns(containerType);
				rateablePartMock.Setup(x => x.ContainerNumber).Returns(containerNumber);

				return rateablePartMock.Object;
			}
		}

		class PartWithDimensionsTestData
		{
			public PartWithDimensions PartType1Num1 { get; set; }
			public PartWithDimensions PartType1Num2 { get; set; }
			public PartWithDimensions PartType2Num1 { get; set; }
			public PartWithDimensions PartType2Num1_2 { get; set; }
			public PartWithDimensions PartNone { get; set; }
			public PartWithDimensions PartNoType { get; set; }
			public PartWithDimensions PartNoNumber { get; set; }
			public PartWithDimensions PartNone2 { get; set; }
		}

		#endregion
	}
}
