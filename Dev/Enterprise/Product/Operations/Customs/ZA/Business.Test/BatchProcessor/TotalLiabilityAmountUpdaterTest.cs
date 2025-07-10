using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(TotalLiabilityAmountUpdater))]
	sealed class TotalLiabilityAmountUpdaterTest : TestCaseWithFactory
	{
		[TestDate(2024, 5, 23, 12, 34, 56)]
		public void TestUpdateTotalLiabilityAmount()
		{
			SetupMockDutyAndTaxCalculator();

			var helper = new WhsDataTestHelper(Factory);
			var yesterday = ZDateTime.Now.AddDays(-1).ToDateTime();

			var zaWarehouse = CreateWarehouse(helper, CountryCodes.SouthAfrica, "ZWH");
			var zaReceive = helper.GetNewWhsReceive(zaWarehouse.PK, helper.Importer.PK, "RCV1", yesterday);
			var zaReceiveLine = helper.GetNewReceiveInventory(zaReceive, helper.Part, "PACKAGE1", 1m, 111m, 111m);
			var zaWarehouseAttribute = helper.GetNewWhsBondedWarehouseAttribute(zaReceiveLine.PK, 50m, 100m, "UQ1", 200m, "UQ2", 300m, "UQ3",
				CountryCodes.HongKong, 111m, "UNT", string.Empty, string.Empty, 1);
			zaWarehouseAttribute.WB_Tariff = "3920790500";

			var gbWarehouse = CreateWarehouse(helper, CountryCodes.UnitedKingdom, "GWH");
			var gbReceive = helper.GetNewWhsReceive(gbWarehouse.PK, helper.Importer.PK, "RCV2", yesterday);
			var gbReceiveLine = helper.GetNewReceiveInventory(gbReceive, helper.Part, "PACKAGE1", 1m, 111m, 111m);
			var gbWarehouseAttribute = helper.GetNewWhsBondedWarehouseAttribute(gbReceiveLine.PK, 50m, 100m, "UQ1", 200m, "UQ2", 300m, "UQ3",
				CountryCodes.HongKong, 111m, "UNT", string.Empty, string.Empty, 1);
			gbWarehouseAttribute.WB_Tariff = "3920790500";

			Factory.Save();

			new TotalLiabilityAmountUpdater(new DummyLogger()).Process(new CancellationToken());
			Factory.ReloadAll<WhsBondedWarehouseAttribute>();

			AssertEquals("WB_AllDutiesAmount has non-zero value", 10m, zaWarehouseAttribute.WB_AllDutiesAmount);
			AssertEquals("WB_VATAmount has non-zero value", 2.3m, zaWarehouseAttribute.WB_VATAmount);
			AssertEquals("WB_AllDutiesAmount has default value", 0m, gbWarehouseAttribute.WB_AllDutiesAmount);
			AssertEquals("WB_VATAmount has default value", 0m, gbWarehouseAttribute.WB_VATAmount);
			mockDutyAndTaxCalculator.VerifyAll();
		}

		void SetupMockDutyAndTaxCalculator()
		{
			mockDutyAndTaxCalculator = new Mock<IWhsInventoryDutyAndTaxCalculator>();
			mockDutyAndTaxCalculator
				.Setup(provider => provider.Calculate(It.IsAny<Dictionary<ZString, IZType>>(),
					It.Is<ZDecimal>(value => value == 1m), // ratio
					It.Is<ZDateTime>(value => value == ZDateTime.Now.AddDays(-1)), // arrivalDate
					It.Is<ZDateTime>(value => value == ZDateTime.Now), // valuationDate
					It.Is<ZString>(value => value == "3920790500"), // tariff
					It.Is<ZString>(value => value == "HK"), // countryOfOrigin
					It.Is<ZDecimal>(value => value == 50m), // customsValue
					It.Is<ZDecimal>(value => value == 100m), // customsQty1
					It.Is<ZString>(value => value == "UQ1"), // customsUQ1
					It.Is<ZDecimal>(value => value == 200m), // customsQty2
					It.Is<ZString>(value => value == "UQ2"), // customsUQ2
					It.Is<ZDecimal>(value => value == 300m), // customsQty3
					It.Is<ZString>(value => value == "UQ3"))) // customsUQ3
				.Callback((Dictionary<ZString, IZType> data, ZDecimal ratio, ZDateTime arrivalDate, ZDateTime valuationDate,
					ZString tariff, ZString countryOfOrigin, ZDecimal customsValue,
					ZDecimal customsQty1, ZString customsUQ1, ZDecimal customsQty2, ZString customsUQ2, ZDecimal customsQty3, ZString customsUQ3) =>
				{
					data.Add(CusEntryPayTypes.AllDuties, (ZDecimal)10m);
					data.Add(CusEntryPayTypes.ValueAddedTax, (ZDecimal)2.3m);
				});
			var mockDutyAndTaxCalculatorProvider = new Mock<IWhsInventoryDutyAndTaxCalculatorProvider>();
			mockDutyAndTaxCalculatorProvider
				.Setup(provider => provider.GetProviderFor(It.IsAny<BusinessObjectFactory>(), It.Is<ZString>(value => value == CountryCodes.SouthAfrica)))
				.Returns(mockDutyAndTaxCalculator.Object);
			ObjectFactory.Substitute(mockDutyAndTaxCalculatorProvider.Object);
		}

		IWhsWarehouse CreateWarehouse(WhsDataTestHelper helper, string countryCode, string warehouseCode)
		{
			var warehouseAddress = Factory.NewWithValidTestData<OrgAddress>();
			warehouseAddress.OA_RN_NKCountryCode = countryCode;
			return helper.GetNewWhsWarehouse(warehouseAddress.PK, isVirtualWarehouse: true, warehouseCode);
		}

		Mock<IWhsInventoryDutyAndTaxCalculator> mockDutyAndTaxCalculator;
	}
}
