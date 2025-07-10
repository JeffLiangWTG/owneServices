using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;

namespace Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	public class ContainerConsignmentItemWrapperTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorThrowsArgumentException()
		{
			new ContainerConsignmentItemWrapper(null, false);
		}

		public void TestCREIsEmptyContainer()
		{
			Assert(!creWrappedContainer.IsEmptyContainer);
		}

		public void TestGoodsDescription()
		{
			Assert(creWrappedContainer.GoodsDescription.IsEmpty);
			Assert(icrWrappedContainer.GoodsDescription.IsEmpty);
			var jobDeclaration = Factory.New<JobDeclaration>();
			container.CO_JE = jobDeclaration.PK;
			jobDeclaration.JE_GoodsDescription = "TEST GOODS DESCRIPTION";
			AssertEquals("TEST GOODS DESCRIPTION", creWrappedContainer.GoodsDescription);
			AssertEquals("TEST GOODS DESCRIPTION", icrWrappedContainer.GoodsDescription);
		}

		public void TestCREIdentifiers()
		{
			AssertEquals(Enumerable.Empty<ICommodity>(), creWrappedContainer.Identifiers);
		}

		public void TestValue()
		{
			AssertEquals(ZDecimal.Zero, creWrappedContainer.Value);
			AssertEquals(ZDecimal.Zero, icrWrappedContainer.Value);
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			jobDeclaration.JE_ECI_InvoiceAmount = 23.56m;
			jobDeclaration.CusContainers.Add(container);
			AssertEquals(ZDecimal.Zero, creWrappedContainer.Value);
			AssertEquals(ZDecimal.Zero, icrWrappedContainer.Value);
			var wrappedContainer2 = new ContainerConsignmentItemWrapper(container, false);
			AssertEquals(23.56m, ((ICREConsignmentItem)wrappedContainer2).Value);
			AssertEquals(23.56m, ((IICRConsignmentItem)wrappedContainer2).Value);
		}

		public void TestCRECurrency()
		{
			Assert(creWrappedContainer.Currency.IsEmpty);
			Assert(icrWrappedContainer.Currency.IsEmpty);
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			jobDeclaration.JE_ECI_InvoiceCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia).PK;
			jobDeclaration.CusContainers.Add(container);
			Assert(creWrappedContainer.Currency.IsEmpty);
			Assert(icrWrappedContainer.Currency.IsEmpty);
			var wrappedContainer2 = new ContainerConsignmentItemWrapper(container, false);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, ((ICREConsignmentItem)wrappedContainer2).Currency);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, ((IICRConsignmentItem)wrappedContainer2).Currency);
		}

		public void TestClassifications()
		{
			AssertEquals(Enumerable.Empty<IClassification>(), creWrappedContainer.Classifications);
			AssertEquals(Enumerable.Empty<IClassification>(), icrWrappedContainer.Classifications);
		}

		public void TestGrossWeightInKg()
		{
			AssertEquals(ZDecimal.Zero, creWrappedContainer.GrossWeightInKg);
			AssertEquals(ZDecimal.Zero, icrWrappedContainer.GrossWeightInKg);
			container.CO_Weight = 87.01m;
			container.CO_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(87.01m, creWrappedContainer.GrossWeightInKg);
			AssertEquals(87.01m, icrWrappedContainer.GrossWeightInKg);
			container.CO_Weight = 1000m;
			container.CO_WeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(1m, creWrappedContainer.GrossWeightInKg);
			AssertEquals(1m, icrWrappedContainer.GrossWeightInKg);
			container.CO_WeightUQ = "ZZ";
			AssertEquals(ZDecimal.Zero, creWrappedContainer.GrossWeightInKg);
			AssertEquals(ZDecimal.Zero, icrWrappedContainer.GrossWeightInKg);
		}

		public void TestGoodsOriginCountry()
		{
			Assert(creWrappedContainer.GoodsOriginCountry.IsEmpty);
			Assert(icrWrappedContainer.GoodsOriginCountry.IsEmpty);
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_RL_NKOrigin = "AUSYD";
			jobDeclaration.CusContainers.Add(container);
			AssertEquals(Core.Constants.CountryCodes.Australia, creWrappedContainer.GoodsOriginCountry);
			AssertEquals(Core.Constants.CountryCodes.Australia, icrWrappedContainer.GoodsOriginCountry);
		}

		public void TestCREPackageQty()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.CusContainers.Add(container);
			AssertEquals(ZInt.Zero, creWrappedContainer.PackageQty);
			AssertEquals(ZInt.Zero, icrWrappedContainer.PackageQty);
			var packingGroup1 = container.PackingGroups.AddNew();
			var package1 = packingGroup1.Packages.AddNew();
			package1.CW_PackQty = 12;
			var package2 = packingGroup1.Packages.AddNew();
			package2.CW_PackQty = 8;

			var packingGroup2 = container.PackingGroups.AddNew();
			var package3 = packingGroup2.Packages.AddNew();
			package3.CW_PackQty = 45;
			var package4 = packingGroup2.Packages.AddNew();
			package4.CW_PackQty = 23;
			AssertEquals(88, creWrappedContainer.PackageQty);
			AssertEquals(88, icrWrappedContainer.PackageQty);
		}

		public void TestCREPackageType()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.CusContainers.Add(container);
			Assert(creWrappedContainer.PackageType.IsEmpty);
			Assert(icrWrappedContainer.PackageType.IsEmpty);
			var packingGroup1 = container.PackingGroups.AddNew();
			var package1 = packingGroup1.Packages.AddNew();
			package1.CW_PackType = "TY1";
			var package2 = packingGroup1.Packages.AddNew();
			package2.CW_PackType = "TY2";

			var packingGroup2 = container.PackingGroups.AddNew();
			var package3 = packingGroup2.Packages.AddNew();
			package3.CW_PackType = "TY3";
			var package4 = packingGroup2.Packages.AddNew();
			package4.CW_PackType = "TY4";
			AssertEquals("TY1", creWrappedContainer.PackageType);
			AssertEquals("TY1", icrWrappedContainer.PackageType);
		}

		public void TestCREContainerNumber()
		{
			Assert(creWrappedContainer.ContainerNumber.IsEmpty);
			Assert(icrWrappedContainer.ContainerNumber.IsEmpty);
			container.CO_ContainerNumber = "DECCONT1234";
			AssertEquals("DECCONT1234", creWrappedContainer.ContainerNumber);
			AssertEquals("DECCONT1234", icrWrappedContainer.ContainerNumber);
		}

		public void TestCREUNDGHazardousGoodsCode()
		{
			Assert(creWrappedContainer.UNDGHazardousGoodsCode.IsEmpty);
		}

		public void TestICRIdentityNumber()
		{
			Assert(icrWrappedContainer.IdentityNumber.IsEmpty);
		}

		public void TestICRIdentityType()
		{
			Assert(icrWrappedContainer.IdentityType.IsEmpty);
		}

		public void TestICRFlashpointTempInCelsius()
		{
			AssertEquals(ZDecimal.Zero, icrWrappedContainer.FlashpointTempInCelsius);
		}

		public void TestICRTemperatures()
		{
			AssertNull(icrWrappedContainer.Temperatures);
		}

		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<CusContainer>();
			var consignmentItemWrapper = new ContainerConsignmentItemWrapper(container, true);
			creWrappedContainer = consignmentItemWrapper;
			icrWrappedContainer = consignmentItemWrapper;
		}
		CusContainer container;
		ICREConsignmentItem creWrappedContainer;
		IICRConsignmentItem icrWrappedContainer;
	}
}
