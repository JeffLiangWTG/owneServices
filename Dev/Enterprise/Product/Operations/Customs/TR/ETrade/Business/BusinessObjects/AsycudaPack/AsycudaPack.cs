using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack, ICustomsDutyCalculationData
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static class PackTypes
		{
			public const string Bin = "BI";
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public new AsycudaPackPackedItemPivotCollection PackedItems => (AsycudaPackPackedItemPivotCollection)base.PackedItems;

		public new AsycudaPackedItem PackedItem => (AsycudaPackedItem)base.PackedItem;

		public AsycudaManifestHeader Header => Bill?.Header;

		protected override ManifestBase.AsycudaPackPackedItemPivotCollection CreateNewAsycudaPackCollection() => new AsycudaPackPackedItemPivotCollection(this);

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		#region ICustomsDutyCalculationData

		ZString ICustomsDutyCalculationData.DataGrouping => Core.Constants.CountryCodes.Turkey;

		ZDateTime ICustomsDutyCalculationData.EffectiveDate => Header?.DateAtCustomsOffice ?? ZDateTime.Today;

		ZString ICustomsDutyCalculationData.RateType => TaxCodeList.RelatedMiscCodes.SpecialCustomsDutyCode;

		ZString ICustomsDutyCalculationData.RateCode => TaxCodeList.Codes.CustomsDuty;

		ZString ICustomsDutyCalculationData.TariffType => TaxCodeList.RelatedMiscCodes.TariffType;

		ZString ICustomsDutyCalculationData.TariffCode => PackedItem?.API_Tariff ?? ZString.Empty;

		ZDecimal ICustomsDutyCalculationData.CustomsValue
		{
			get { return customsValue; }
			set { customsValue = value; }
		}
		ZDecimal customsValue;

		ZString ICustomsDutyCalculationData.ExportCountry => Bill.ExportCountry;

		ZString ICustomsDutyCalculationData.AdditionalCode => PackedItem?.API_ChemicalSubstanceCode ?? ZString.Empty;

		ZString ICustomsDutyCalculationData.GetFixedDutyFormula()
		{
			return (PackedItem?.API_Tariff.IsEmpty ?? false) ? ZString.Empty : Bill.SCDRateFixForETrade;
		}

		#endregion
	}
}
