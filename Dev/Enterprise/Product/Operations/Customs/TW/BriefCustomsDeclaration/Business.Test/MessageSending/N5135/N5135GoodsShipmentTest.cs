using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(N5135GoodsShipment))]
	sealed class N5135GoodsShipmentTest : BCDGoodsShipmentTest<N5135GoodsShipment, N5135MessageSendingObject>
	{
		public void TestSequenceNumeric()
		{
			AssertEquals(1, goodsShipment.SequenceNumeric);
		}

		public void TestSupplier()
		{
			bill.ABL_ShipperName = "supplier name";
			bill.ABL_ShipperLocalName = "供應商名字";
			CombineAssertions(() =>
			{
				var supplier = goodsShipment.Supplier;
				AssertEquals("Name", "supplier name", supplier.Name);
				AssertEquals("ChineseName", "供應商名字", supplier.ChineseName);
			});
		}

		public void TestInvoiceAmount()
		{
			bill.ABL_GoodsValue = 1m;
			AssertEquals(1m, goodsShipment.InvoiceAmount);
		}

		public void TestTaxFeeDeclared()
		{
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, goodsShipment.TaxFeeDeclared);
				var tax = bill.AsycudaTaxes.AddNew();
				tax.AET_MethodOfPayment = TaxFeePaymentMethodList.Codes.CAS;
				AssertEquals(ZString.Empty, goodsShipment.TaxFeeDeclared);
				tax.AET_ChargeAmount = 1m;
				AssertEquals(YesNoList.Codes.Yes, goodsShipment.TaxFeeDeclared);
			});
		}

		public void TestTotalGrossMassMeasure()
		{
			bill.ABL_GrossWeight = 1000m;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(1m, goodsShipment.TotalGrossMassMeasure);
		}

		public void TestGovernmentAgencyGoodsItems()
		{
			var packedItem1 = bill.PackedItems.AddNew();
			packedItem1.API_LineNo = 2;
			packedItem1.API_Brand = "brand 2";
			var packedItem2 = bill.PackedItems.AddNew();
			packedItem2.API_LineNo = 1;
			packedItem2.API_Brand = "brand 1";

			var goodsItems = goodsShipment.GovernmentAgencyGoodsItems;
			CombineAssertions(() =>
			{
				AssertEquals("Should generate 2 GovernmentAgencyGoodsItems", 2, goodsItems.Count());
				AssertEquals("The first one", "brand 1", goodsItems.First().Commodity.Name);
			});
		}

		public void TestCurrencyExchange()
		{
			bill.ABL_RX_NKGoodsValueCurrency = "USD";
			AssertEquals("USD", ((ICurrencyExchange)goodsShipment).CurrencyTypeCode);
		}

		public void TestDutyTaxFees()
		{
			var tax1 = bill.AsycudaTaxes.AddNew();
			tax1.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyLevied;
			tax1.AET_ChargeType = ChargeTypeCASList.Codes.BusinessTax;
			tax1.AET_ChargeAmount = 888.88m;

			var tax2 = bill.AsycudaTaxes.AddNew();
			tax2.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyNotLevied;
			tax2.AET_ChargeType = ChargeTypeCASList.Codes.AntiDumpingDuty;
			tax2.AET_ChargeAmount = 999.99m;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, goodsShipment.DutyTaxFees.Count());
				Assert("B40", goodsShipment.DutyTaxFees.Any(x => x.TypeCode == ChargeTypeCASList.Codes.BusinessTax && x.AdValoremTaxBaseAmount == 888.88m));
				Assert("A30", goodsShipment.DutyTaxFees.Any(x => x.TypeCode == ChargeTypeCASList.Codes.AntiDumpingDuty && x.AdValoremTaxBaseAmount == 999.99m));
			});
		}

		public void TestCustomsValuation()
		{
			bill.ABL_InsuranceValue = 1000m;
			bill.ABL_FreightValue = 5000m;
			bill.ABL_OtherValue = 200m;
			bill.ABL_OtherDeductions = 100m;
			bill.ABL_CustomsValue = 600m;

			var tax1 = bill.AsycudaTaxes.AddNew();
			tax1.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyLevied;
			tax1.AET_ChargeType = ChargeTypeCASList.Codes.ImportDuty;
			tax1.AET_ChargeAmount = 101m;

			var tax2 = bill.AsycudaTaxes.AddNew();
			tax2.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyNotLevied;
			tax2.AET_ChargeType = ChargeTypeCASList.Codes.TobaccoAndAlcoholTax;
			tax2.AET_ChargeAmount = 102m;

			var tax3 = bill.AsycudaTaxes.AddNew();
			tax3.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyNotLevied;
			tax3.AET_ChargeType = ChargeTypeCASList.Codes.HealthAndWelfareSurcharge;
			tax3.AET_ChargeAmount = 103m;

			var tax4 = bill.AsycudaTaxes.AddNew();
			tax4.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyNotLevied;
			tax4.AET_ChargeType = ChargeTypeCASList.Codes.CommodityTax;
			tax4.AET_ChargeAmount = 104m;

			var tax5 = bill.AsycudaTaxes.AddNew();
			tax5.AET_MethodOfPayment = MethodOfPaymentList.Codes.DutyNotLevied;
			tax5.AET_ChargeType = ChargeTypeCASList.Codes.AntiDumpingDuty;
			tax5.AET_ChargeAmount = 105m;
			CombineAssertions(() =>
			{
				var customsValuation = (ICustomsValuation)goodsShipment;
				AssertEquals("ExitToEntryChargeAmount", 1000m, customsValuation.ExitToEntryChargeAmount);
				AssertEquals("FreightChargeAmount", 5000m, customsValuation.FreightChargeAmount);
				AssertEquals("OtherChargeAmount", 200m, customsValuation.OtherChargeAmount);
				AssertEquals("OtherDeductionAmount", 100m, customsValuation.OtherDeductionAmount);
				AssertEquals("OtherChargeDeductionAmount", 1010m, customsValuation.OtherChargeDeductionAmount);
				AssertEquals("TotalDutyTaxFeeAmount", 515m, customsValuation.TotalDutyTaxFeeAmount);
			});
		}

		AsycudaBill bill;
		IN5135GoodsShipment goodsShipment;

		protected override void SetUp()
		{
			base.SetUp();
			(goodsShipment, _, _, bill) = SetupData();
		}

		protected override List<BCDGoodsShipment> GetAdditionalDocuments(N5135MessageSendingObject messageSendingObject) => ((IN5135Declaration)messageSendingObject).GoodsShipments.ToList<BCDGoodsShipment>();

		protected override N5135GoodsShipment GetGoodsShipment(N5135MessageSendingObject messageSendingObject) => ((IN5135Declaration)messageSendingObject).GoodsShipments.ToList<N5135GoodsShipment>()[0];

		protected override N5135MessageSendingObject GetSendingObject(AsycudaManifestHeader header) => new N5135MessageSendingObject(header);
	}
}
