using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTaxPairList;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class N5135GoodsShipment : BCDGoodsShipment,
		IN5135GoodsShipment,
		ICurrencyExchange,
		ICustomsValuation
	{
		public N5135GoodsShipment(AsycudaBill bill, MessageSendingObject sendingObject) : base(bill, sendingObject)
		{
		}

		ZDecimal IN5135GoodsShipment.InvoiceAmount => HouseBill.ABL_GoodsValue;

		ZString IN5135GoodsShipment.TaxFeeDeclared => HouseBill.AsycudaTaxes.Cast<AsycudaTax>().Any(tax => tax.AET_MethodOfPayment == TaxFeePaymentMethodList.Codes.CAS && tax.AET_ChargeAmount > 0m) ? YesNoList.Codes.Yes : string.Empty;

		ICurrencyExchange IN5135GoodsShipment.CurrencyExchange => this;

		IPartyDetails IN5135GoodsShipment.Supplier => new PartyWrapper(name: HouseBill.ABL_ShipperName, chineseName: HouseBill.ABL_ShipperLocalName);

		ICustomsValuation IGoodsShipment.CustomsValuation => this;

		public IEnumerable<IGoodsShipmentDutyTaxFee> DutyTaxFees => HouseBill.AsycudaTaxes.Cast<AsycudaTax>().Select(x => new GoodsShipmentDutyTaxFeeWrapper(x.AET_ChargeType, x.AET_ChargeAmount));

		#region ICurrencyExchange members
		ZString ICurrencyExchange.CurrencyTypeCode => HouseBill.ABL_RX_NKGoodsValueCurrency;

		ZDecimal ICurrencyExchange.RateNumeric => ZDecimal.Zero;

		#endregion

		#region ICustomsValuation members

		ZDecimal ICustomsValuation.InvoiceAmount => ZDecimal.Zero;

		ZDecimal ICustomsValuation.ExitToEntryChargeAmount => HouseBill.ABL_InsuranceValue;

		ZDecimal ICustomsValuation.FreightChargeAmount => HouseBill.ABL_FreightValue;

		ZDecimal ICustomsValuation.OtherChargeDeductionAmount => HouseBill.ABL_CustomsValue
			+ HouseBill.AsycudaTaxes.Cast<AsycudaTax>().Where(x => IsVatTax(x)).Sum(s => s.AET_ChargeAmount);

		bool IsVatTax(AsycudaTax tax)
		{
			switch (tax.AET_ChargeType)
			{
				case ChargeTypeCASList.Codes.ImportDuty:
				case ChargeTypeCASList.Codes.TobaccoAndAlcoholTax:
				case ChargeTypeCASList.Codes.HealthAndWelfareSurcharge:
				case ChargeTypeCASList.Codes.CommodityTax:
					return true;
				default:
					return false;
			}
		}

		ZString ICustomsValuation.PartyRelationshipCode => ZString.Empty;

		ZDecimal ICustomsValuation.OtherChargeAmount => HouseBill.ABL_OtherValue;

		ZDecimal ICustomsValuation.OtherDeductionAmount => HouseBill.ABL_OtherDeductions;

		ZDecimal ICustomsValuation.TotalDutyTaxFeeAmount => DutyTaxFees.Sum(x => x.AdValoremTaxBaseAmount);

		#endregion
	}
}
