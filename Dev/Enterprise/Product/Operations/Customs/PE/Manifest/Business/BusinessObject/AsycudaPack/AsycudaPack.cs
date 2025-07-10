using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.PE.Manifest.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack , Integration.Customs.ASYCUDA.PEManifest.IAsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;

		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);

		public override ZDecimal LinePrice
		{
			get => PackedItem.API_GoodsValue;
			set
			{
				var oldValue = LinePrice;
				if (!IsCopying && oldValue != value)
				{
					PackedItem.API_GoodsValue = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateLinePrice();
				}
				LinePriceInfo.RefreshBinding();
			}
		}

		public override ZString LinePriceCurrency
		{
			get => PackedItem.API_RX_NKGoodsValueCurrency;
			set
			{
				var oldValue = LinePriceCurrency;
				if (!IsCopying && oldValue != value)
				{
					PackedItem.API_RX_NKGoodsValueCurrency = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateLinePriceCurrency();
				}
				LinePriceCurrencyInfo.RefreshBinding();
			}
		}

		protected override bool IsPackUQNeedToConvertCore => false;
	}
}
