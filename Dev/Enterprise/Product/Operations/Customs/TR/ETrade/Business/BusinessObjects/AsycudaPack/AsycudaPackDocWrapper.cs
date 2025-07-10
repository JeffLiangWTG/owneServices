using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaPackDocWrapper : DocumentWrapper
	{
		public AsycudaPackDocWrapper(AsycudaPack pack)
			: base(pack, pack.Factory)
		{
			this.pack = pack;
		}
		readonly AsycudaPack pack;
		public ZString BillNumber => pack.Bill.ABL_BillNumber;
		public ZString ShipperName => pack.Bill.ABL_ShipperName;
		public ZString ConsigneeName => pack.Bill.ABL_ConsigneeName;
		public ZString TradeCountry => GetCodeOfCountry(pack.Bill.TradeCountry);
		public ZString OriginCountry => GetCodeOfCountry(pack.Bill.ExportCountry);
		public ZString DischargeCountry => GetCodeOfCountry(pack.Bill.ArrivalCountry);
		public ZString LoadingCountry => GetCodeOfCountry(pack.Bill.DepartureCountry);
		public ZInt Container => pack.Bill.ABL_ManifestQty;
		public ZString ContainerType => pack.Bill.ABL_ManifestUQ;
		public ZString Description => pack.PackedItem.API_GoodsDescription;

		AsycudaTax CustumsDutyTax => pack.Bill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.CustomsDuty);
		public ZDecimal TotalTaxRate => CustumsDutyTax.IsNull ? 0 : CustumsDutyTax.AET_Rate;
		public ZDecimal TaxBase => CustumsDutyTax.IsNull ? 0 : CustumsDutyTax.AET_BaseValue;
		public ZDecimal TaxAmount => CustumsDutyTax.IsNull ? 0 : CustumsDutyTax.AET_ChargeAmount;
		public ZDecimal GrossWeight => pack.Bill.GrossWeightInKG;
		public ZString Procedure => pack.Bill.ABL_Procedure;
		public ZDecimal CustomsValue => pack.Bill.ABL_CustomsValue;
		public ZString Tariff => pack.PackedItem.API_Tariff;
		public ZDecimal TotalQuantity => pack.PackedItem.API_CustomsQty2;
		public ZString Unit => pack.PackedItem.API_CustomsUQ2;
		public ZString CustomsValueCurrency => pack.Bill.ABL_RX_NKCustomsValueCurrency;
		ZString GetCodeOfCountry(ZString countryCode)
		{
			var code = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(Factory, Core.Constants.CountryCodes.Turkey, RefCusMapTypeName, countryCode, ZDateTime.Now);
			return code.IsEmpty ? ZString.Empty : code;
		}

		const string RefCusMapTypeName = "CNTRY";
	}
}

