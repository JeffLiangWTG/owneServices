using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity))]
	sealed class LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodityTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAdditionalDocument()
		{
			var permit1 = invoiceLine.PermitCusSupportingCollection.AddNew();
			permit1.CSI_Code = "111";
			permit1.CSI_LineNo = 1;
			var permit2 = invoiceLine.PermitCusSupportingCollection.AddNew();
			permit2.CSI_Code = "222";
			permit2.CSI_LineNo = 2;
			var additionalDocument = goodsShipmentGovernmentAgencyGoodsItemCommodity.AdditionalDocuments.ToArray();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(additionalDocument.Length, NUnit.Framework.Is.EqualTo(2));
				NUnit.Framework.Assert.That(additionalDocument[0].SequenceNumeric, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(additionalDocument[1].SequenceNumeric, NUnit.Framework.Is.EqualTo(2).Using(CustomComparers.TypeComparison));
			});
		}

		public void TestHandlingInstructionsCodes()
		{
			var storageAndShippingCondition1 = invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.AddNew();
			storageAndShippingCondition1.JG_ReferenceNumber = "1";
			var storageAndShippingCondition2 = invoiceLine.StorageAndShippingConditionJobComInvLineRefsCollection.AddNew();
			storageAndShippingCondition2.JG_ReferenceNumber = "2";
			AssertContainsExactElementsInAnyOrder(new[] { "1", "2" }, goodsShipmentGovernmentAgencyGoodsItemCommodity.HandlingInstructionsCodes);
		}

		[ExpectNoExceptions]
		public void TestFood()
		{
			invoiceLine.JI_PHValueNumeric = 2m;
			invoiceLine.JI_SterilizationValueNumeric = 9m;
			var foodData1 = invoiceLine.FoodDataCollection.AddNew();
			foodData1.CY_Data = "A";
			foodData1.Content = 2m;
			var foodData2 = invoiceLine.FoodDataCollection.AddNew();
			foodData2.CY_Data = "B";
			foodData2.Content = 5m;
			CombineAssertions(() =>
			{
				var food = goodsShipmentGovernmentAgencyGoodsItemCommodity.Food;
				NUnit.Framework.Assert.That(food.PHValueNumeric, NUnit.Framework.Is.EqualTo(2m).Using(CustomComparers.TypeComparison), "PHValueNumeric");
				NUnit.Framework.Assert.That(food.SterilizationValueNumeric, NUnit.Framework.Is.EqualTo(9m).Using(CustomComparers.TypeComparison), "SterilizationValueNumeric");
				NUnit.Framework.Assert.That(food.Constituents.Count(), NUnit.Framework.Is.EqualTo(2), "Constituents count");
				NUnit.Framework.Assert.That(food.Constituents.Any(x => x.ElementName == "A" && x.ElementPercentNumeric == 2m), NUnit.Framework.Is.True, "Constituents contain A/2");
				NUnit.Framework.Assert.That(food.Constituents.Any(x => x.ElementName == "B" && x.ElementPercentNumeric == 5m), NUnit.Framework.Is.True, "Constituents contain B/5");
			});
		}

		public void TestQuarantine()
		{
			invoiceLine.JI_QuarantineFeatures = "Yellow";
			invoiceLine.JI_QuarantineTreatment = "None";
			var slaughterDate1 = invoiceLine.SlaughterDateCollection.AddNew();
			slaughterDate1.CY_Date = new ZDateTime(2024, 2, 23, 16, 42, 0);
			var slaughterDate2 = invoiceLine.SlaughterDateCollection.AddNew();
			slaughterDate2.CY_Date = new ZDateTime(2024, 2, 24, 16, 42, 0);
			var packingHouse1 = invoiceLine.PackingHouseCollection.AddNew();
			packingHouse1.CY_Code = "XXXX0121";
			var packingHouse2 = invoiceLine.PackingHouseCollection.AddNew();
			packingHouse2.CY_Code = "XXXX0122";
			var packingDate1 = invoiceLine.PackingDateCollection.AddNew();
			packingDate1.CY_Date = new ZDateTime(2024, 2, 25, 16, 42, 0);
			var packingDate2 = invoiceLine.PackingDateCollection.AddNew();
			packingDate2.CY_Date = new ZDateTime(2024, 2, 26, 16, 42, 0);
			invoiceLine.JI_AnimalAgeMonth = 10;
			invoiceLine.JI_AnimalAgeYear = 6;
			invoiceLine.JI_AnimalFemaleQty = 23;
			invoiceLine.JI_AnimalMaleQty = 28;
			invoiceLine.JI_MicrochipID = "9912030001";
			invoiceLine.JI_VaccinationTypeDate = "2011-08-02";
			CombineAssertions(() =>
			{
				var quarantine = goodsShipmentGovernmentAgencyGoodsItemCommodity.Quarantine;
				NUnit.Framework.Assert.That(quarantine.ObjectFeature, NUnit.Framework.Is.EqualTo("Yellow").Using(CustomComparers.TypeComparison), "ObjectFeature");
				NUnit.Framework.Assert.That(quarantine.Treatment, NUnit.Framework.Is.EqualTo("None").Using(CustomComparers.TypeComparison), "Treatment");
				NUnit.Framework.Assert.That(quarantine.AdditionalDocument.Cast<IAdditionalDocument>().Select(x => x.SlaughterDateTime.ToShortDateString()), NUnit.Framework.Is.EquivalentTo(new[] { "23-Feb-24", "24-Feb-24" }), "AdditionalDocument.SlaughterDateTime");
				AssertContainsExactElementsInAnyOrder("AdditionalInformation.PackingHouse", new[] { "XXXX0121", "XXXX0122" }, quarantine.AdditionalInformation.Cast<IAdditionalInformation>().Select(x => x.PackingHouse));
				NUnit.Framework.Assert.That(quarantine.Packing.Cast<IPackaging>().Select(x => x.PackingDateTime.ToShortDateString()), NUnit.Framework.Is.EquivalentTo(new[] { "25-Feb-24", "26-Feb-24" }), "Packing.PackingDateTime");

				var animal = quarantine.Animal;
				NUnit.Framework.Assert.That(animal.AgeMonthNumeric, NUnit.Framework.Is.EqualTo(10).Using(CustomComparers.TypeComparison), "AgeMonthNumeric");
				NUnit.Framework.Assert.That(animal.AgeYearNumeric, NUnit.Framework.Is.EqualTo(6).Using(CustomComparers.TypeComparison), "AgeYearNumeric");
				NUnit.Framework.Assert.That(animal.FemaleQuantity, NUnit.Framework.Is.EqualTo(23).Using(CustomComparers.TypeComparison), "FemaleQuantity");
				NUnit.Framework.Assert.That(animal.MaleQuantity, NUnit.Framework.Is.EqualTo(28).Using(CustomComparers.TypeComparison), "MaleQuantity");
				NUnit.Framework.Assert.That(animal.MicrochipID, NUnit.Framework.Is.EqualTo("9912030001").Using(CustomComparers.TypeComparison), "MicrochipID");
				NUnit.Framework.Assert.That(animal.Vaccination, NUnit.Framework.Is.EqualTo("2011-08-02").Using(CustomComparers.TypeComparison), "Vaccination");
			});
		}

		[ExpectNoExceptions]
		public void TestClassification()
		{
			invoiceLine.JI_Tariff = "1921681212";
			NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.Classification.ID, NUnit.Framework.Is.EqualTo("1921681212").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestConstituent()
		{
			invoiceLine.JI_Compositions = "Compositions";
			invoiceLine.JI_ProductGrade = "X123";
			invoiceLine.JI_ProductThickness = "Thickness";
			CombineAssertions(() =>
			{
				var constituent = goodsShipmentGovernmentAgencyGoodsItemCommodity.Constituent;
				NUnit.Framework.Assert.That(constituent.ElementDescription, NUnit.Framework.Is.EqualTo("Compositions").Using(CustomComparers.TypeComparison), "ElementDescription");
				NUnit.Framework.Assert.That(constituent.LevelID, NUnit.Framework.Is.EqualTo("X123").Using(CustomComparers.TypeComparison), "LevelID");
				NUnit.Framework.Assert.That(constituent.Thickness, NUnit.Framework.Is.EqualTo("Thickness").Using(CustomComparers.TypeComparison), "Thickness");
			});
		}

		[TestDate(2024, 03, 05)]
		[ExpectNoExceptions]
		public void TestInvoiceLine()
		{
			var originalReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
			try
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = ZBool.True;
				CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30.13m, new ZDateTime(2024, 03, 05), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
				Factory.Save();

				var invoice1 = invoiceLine.InvoiceHeader;
				invoice1.JZ_InvoiceAmount = 10000m;
				var invoice2 = declaration.Invoices.AddNew();
				invoice2.JZ_InvoiceAmount = 7000m;
				var invoiceLine2 = (JobComInvoiceLine)invoice2.InvoiceLines.AddNew();
				invoiceLine2.AssignCMHeaderToInvoices(header);

				invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Japan;
				invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
				invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CarriagePaidTo;
				invoiceLine.JI_EnteredUnitPrice = 1234m;
				invoiceLine.JI_InvoiceQuantity = 2m;
				CombineAssertions(() =>
				{
					var goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine = goodsShipmentGovernmentAgencyGoodsItemCommodity.InvoiceLine;
					NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine.ChargesTypeCode, NUnit.Framework.Is.EqualTo(Core.Constants.IncoTerms.FreeOnBoard).Using(CustomComparers.TypeComparison), "ChargesTypeCode");
					NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine.CurrencyTypeCode, NUnit.Framework.Is.EqualTo(Core.Constants.CurrencyCodes.Taiwan).Using(CustomComparers.TypeComparison), "CurrencyTypeCode");
					NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine.UnitPriceAmount, NUnit.Framework.Is.EqualTo(1234m).Using(CustomComparers.TypeComparison), "UnitPriceAmount");
					NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine.SubTotalAmount, NUnit.Framework.Is.EqualTo(2468m).Using(CustomComparers.TypeComparison), "SubTotalAmount");
					NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(74360.84m).Using(CustomComparers.TypeComparison), "ItemChargeAmount");
				});

				invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
				invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
				CombineAssertions(() =>
				{
					var goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine = goodsShipmentGovernmentAgencyGoodsItemCommodity.InvoiceLine;
					NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine.ChargesTypeCode, NUnit.Framework.Is.EqualTo(Core.Constants.IncoTerms.CostAndFreight).Using(CustomComparers.TypeComparison), "ChargesTypeCode");
					NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine.CurrencyTypeCode, NUnit.Framework.Is.EqualTo(Core.Constants.CurrencyCodes.UnitedStates).Using(CustomComparers.TypeComparison), "CurrencyTypeCode");
					NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine.UnitPriceAmount, NUnit.Framework.Is.EqualTo(1234m).Using(CustomComparers.TypeComparison), "UnitPriceAmount");
					NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine.SubTotalAmount, NUnit.Framework.Is.EqualTo(2468m).Using(CustomComparers.TypeComparison), "SubTotalAmount");
					NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityInvoiceLine.ItemChargeAmount, NUnit.Framework.Is.EqualTo(74360.84m).Using(CustomComparers.TypeComparison), "ItemChargeAmount");
				});
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = originalReciprocal;
			}
		}

		[ExpectNoExceptions]
		public void TestCommercialCategorizationID()
		{
			invoiceLine.JI_Model = "Test Model";
			NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.CommercialCategorizationID, NUnit.Framework.Is.EqualTo("Test Model").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDescription()
		{
			invoiceLine.JI_Group = "Bicycle Parts";
			invoiceLine.JI_DeclarationGoodsDescription = "Wheels";
			NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.Description, NUnit.Framework.Is.EqualTo(ZString.Empty));
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.Description, NUnit.Framework.Is.EqualTo("Bicycle Parts\r\n\r\nWheels").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFee()
		{
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			invoiceLine.CusEntryLine.CL_CustomsValue = 101m;
			NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.DutyTaxFee.AdValoremTaxBaseAmount, NUnit.Framework.Is.EqualTo(101m).Using(CustomComparers.TypeComparison), "DutyTaxFee.AdValoremTaxBaseAmount");
		}

		[ExpectNoExceptions]
		public void TestData()
		{
			invoiceLine.JI_BrandName = "Brand Name";
			invoiceLine.JI_NDescription = "謝謝你";
			invoiceLine.JI_TariffExtensionCode = "2";
			invoiceLine.PreviousPermitNo = "P0001";
			invoiceLine.JI_GoodsType = "1";
			invoiceLine.JI_BarCode = "BarCode";
			invoiceLine.JI_Description = "Description";
			invoiceLine.JI_FormattedTariff = "1213.00.00.00-1";
			invoiceLine.JI_Procedure = "31";
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.Name, NUnit.Framework.Is.EqualTo("Brand Name").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.GoodsGroupNameCode, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "GoodsGroupNameCode");
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.BarCode, NUnit.Framework.Is.EqualTo("BarCode").Using(CustomComparers.TypeComparison), "BarCode");
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.EnglishDescription, NUnit.Framework.Is.EqualTo("Description").Using(CustomComparers.TypeComparison), "EnglishDescription");
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.Classification.ID, NUnit.Framework.Is.EqualTo("12130000001").Using(CustomComparers.TypeComparison), "Classification.ID");
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.ChineseDescription, NUnit.Framework.Is.EqualTo("謝謝你").Using(CustomComparers.TypeComparison), "ChineseDescription");
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.TariffCodeExtensionCode, NUnit.Framework.Is.EqualTo("2").Using(CustomComparers.TypeComparison), "TariffCodeExtensionCode");
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.PreviousDocument.ID, NUnit.Framework.Is.EqualTo("P0001").Using(CustomComparers.TypeComparison), "PreviousDocument.ID");
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodity.GovernmentProcedure.CurrentCode, NUnit.Framework.Is.EqualTo("31").Using(CustomComparers.TypeComparison), "GovernmentProcedure.CurrentCode");
			});
		}

		[ExpectNoExceptions]
		public void TestCommodityRelatedPackaging()
		{
			invoiceLine.JI_InnerPackType = "1";
			invoiceLine.JI_InnerPackingMaterial = "999";
			invoiceLine.JI_InnerPackDescription = "inner pack desc";
			CombineAssertions(() =>
			{
				var commodityRelatedPackaging = goodsShipmentGovernmentAgencyGoodsItemCommodity.CommodityRelatedPackaging;
				NUnit.Framework.Assert.That(commodityRelatedPackaging.PackingMethodDescription, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "PackingMethodDescription");
				NUnit.Framework.Assert.That(commodityRelatedPackaging.MaterialCode, NUnit.Framework.Is.EqualTo("999").Using(CustomComparers.TypeComparison), "MaterialCode");
				NUnit.Framework.Assert.That(commodityRelatedPackaging.Specification, NUnit.Framework.Is.EqualTo("inner pack desc").Using(CustomComparers.TypeComparison), "Specification");
			});
		}

		[ExpectNoExceptions]
		public void TestWine()
		{
			invoiceLine.JI_AlcoholAge = 10;
			invoiceLine.JI_AlcoholPercentage = 10.5m;
			invoiceLine.JI_BottledDate = new ZDateTime(2020, 2, 23);
			invoiceLine.JI_AlteredLotNoAmt = 1050m;
			invoiceLine.JI_AlcoholCountryRegion = "US";
			invoiceLine.JI_NoOriginalLotNoAmt = 900m;
			invoiceLine.JI_ExpirationDate = new ZDateTime(2020, 2, 24);
			invoiceLine.JI_AlcoholEndOfShelfLife = new ZDateTime(2020, 2, 25);
			invoiceLine.JI_RemovedLotNoAmt = 800m;
			invoiceLine.JI_AlcoholYear = 2005;
			CombineAssertions(() =>
			{
				var goodsShipmentGovernmentAgencyGoodsItemCommodityWine = goodsShipmentGovernmentAgencyGoodsItemCommodity.Wine;
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityWine.AgeNumeric, NUnit.Framework.Is.EqualTo(10).Using(CustomComparers.TypeComparison), "AgeNumeric");
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityWine.AlcoholContentNumeric, NUnit.Framework.Is.EqualTo(10.5m).Using(CustomComparers.TypeComparison), "AlcoholContentNumeric");
				NUnit.Framework.Assert.That(new ZDateTime(2020, 2, 23), NUnit.Framework.Is.EqualTo(goodsShipmentGovernmentAgencyGoodsItemCommodityWine.BottledDate), "BottledDate");
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityWine.CoverLotNumberAmount, NUnit.Framework.Is.EqualTo(1050m).Using(CustomComparers.TypeComparison), "CoverLotNumberAmount");
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityWine.GeographicRegion, NUnit.Framework.Is.EqualTo("US").Using(CustomComparers.TypeComparison), "GeographicRegion");
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityWine.OriginalNonLotNumberAmount, NUnit.Framework.Is.EqualTo(900m).Using(CustomComparers.TypeComparison), "OriginalNonLotNumberAmount");
				NUnit.Framework.Assert.That(new ZDateTime(2020, 2, 24), NUnit.Framework.Is.EqualTo(goodsShipmentGovernmentAgencyGoodsItemCommodityWine.ProductBestBeforeDateTime), "ProductBestBeforeDateTime");
				NUnit.Framework.Assert.That(new ZDateTime(2020, 2, 25), NUnit.Framework.Is.EqualTo(goodsShipmentGovernmentAgencyGoodsItemCommodityWine.ProductExpiryDateTime), "ProductExpiryDateTime");
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityWine.RemoveLotNumberAmount, NUnit.Framework.Is.EqualTo(800m).Using(CustomComparers.TypeComparison), "RemoveLotNumberAmount");
				NUnit.Framework.Assert.That(goodsShipmentGovernmentAgencyGoodsItemCommodityWine.YearNumeric, NUnit.Framework.Is.EqualTo(2005).Using(CustomComparers.TypeComparison), "YearNumeric");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.AssignCMHeaderToInvoices(header);
			goodsShipmentGovernmentAgencyGoodsItemCommodity = new LicensingMessageGoodsShipmentGovernmentAgencyGoodsItemCommodity(header, invoiceLine);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		JobComInvoiceLine invoiceLine;
		ICommodity goodsShipmentGovernmentAgencyGoodsItemCommodity;
	}
}
