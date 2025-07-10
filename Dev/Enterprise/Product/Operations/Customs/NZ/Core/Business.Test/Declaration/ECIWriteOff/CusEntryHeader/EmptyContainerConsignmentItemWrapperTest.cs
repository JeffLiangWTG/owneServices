using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	public class EmptyContainerConsignmentItemWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new EmptyContainerConsignmentItemWrapper(null);
		}

		public void TestCREIsEmptyContainer()
		{
			Assert(creConsignmentItem.IsEmptyContainer);
		}

		public void TestCREGoodsDescription()
		{
			AssertEquals("EMPTY CONTAINER", creConsignmentItem.GoodsDescription);
		}

		public void TestCREIdentifiers()
		{
			AssertEquals(Enumerable.Empty<ICommodity>(), creConsignmentItem.Identifiers);
		}

		public void TestCREValue()
		{
			AssertEquals(ZDecimal.Zero, creConsignmentItem.Value);
		}

		public void TestCRECurrency()
		{
			Assert(creConsignmentItem.Currency.IsEmpty);
		}

		public void TestCREClassifications()
		{
			AssertEquals(Enumerable.Empty<IClassification>(), creConsignmentItem.Classifications);
		}

		public void TestCREGrossWeightInKg()
		{
			AssertEquals(ZDecimal.Zero, creConsignmentItem.GrossWeightInKg);
		}

		public void TestCREGoodsOriginCountry()
		{
			Assert(creConsignmentItem.GoodsOriginCountry.IsEmpty);
		}

		public void TestCREPackageQty()
		{
			AssertEquals(ZInt.Zero, creConsignmentItem.PackageQty);
		}

		public void TestCREPackageType()
		{
			Assert(creConsignmentItem.PackageType.IsEmpty);
		}

		public void TestCREContainerNumber()
		{
			Assert(creConsignmentItem.ContainerNumber.IsEmpty);
			container.CO_ContainerNumber = "HKFU0029385";
			AssertEquals("HKFU0029385", creConsignmentItem.ContainerNumber);
		}

		public void TestCREUNDGHazardousGoodsCode()
		{
			Assert(creConsignmentItem.UNDGHazardousGoodsCode.IsEmpty);
		}

		public void TestICRGoodsDescription()
		{
			AssertEquals("EMPTY CONTAINER", icrConsignmentItem.GoodsDescription);
		}

		public void TestICRIdentityNumber()
		{
			Assert(icrConsignmentItem.IdentityNumber.IsEmpty);
		}

		public void TestICRValue()
		{
			AssertEquals(ZDecimal.Zero, icrConsignmentItem.Value);
		}

		public void TestICRCurrency()
		{
			Assert(icrConsignmentItem.Currency.IsEmpty);
		}

		public void TestICRIdentityType()
		{
			Assert(icrConsignmentItem.IdentityType.IsEmpty);
		}

		public void TestICRClassifications()
		{
			AssertEquals(Enumerable.Empty<IClassification>(), icrConsignmentItem.Classifications);
		}

		public void TestICRFlashpointTempInCelsius()
		{
			AssertEquals(ZDecimal.Zero, icrConsignmentItem.FlashpointTempInCelsius);
		}

		public void TestICRTemperatures()
		{
			AssertNull(icrConsignmentItem.Temperatures);
		}

		public void TestICRGrossWeightInKg()
		{
			AssertEquals(ZDecimal.Zero, icrConsignmentItem.GrossWeightInKg);
		}

		public void TestICRGoodsOriginCountry()
		{
			Assert(icrConsignmentItem.GoodsOriginCountry.IsEmpty);
		}

		public void TestIREPackageQty()
		{
			AssertEquals(ZInt.Zero, icrConsignmentItem.PackageQty);
		}

		public void TestICEPackageType()
		{
			Assert(icrConsignmentItem.PackageType.IsEmpty);
		}

		public void TestICRContainerNumber()
		{
			Assert(icrConsignmentItem.ContainerNumber.IsEmpty);
			container.CO_ContainerNumber = "HKFU0029385";
			AssertEquals("HKFU0029385", icrConsignmentItem.ContainerNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<CusContainer>();
			var wrappedEmptyContainer = new EmptyContainerConsignmentItemWrapper(container);
			creConsignmentItem = wrappedEmptyContainer;
			icrConsignmentItem = wrappedEmptyContainer;
		}
		CusContainer container;
		ICREConsignmentItem creConsignmentItem;
		IICRConsignmentItem icrConsignmentItem;
	}
}
