using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaPackedItemValidation : ASYCUDA.Business.AsycudaPackedItemValidation
	{
		public AsycudaPackedItemValidation(ASYCUDA.Business.AsycudaPackedItem parent) : base(parent)
		{
		}

		protected new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		protected override void CheckAPI_Tariff()
		{
			base.CheckAPI_Tariff();
			if (Parent.Header.AMA_ManifestType != TRManifestTypes.Codes.CIKONC)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.API_TariffInfo);
			}
		}

		protected override void TariffListValidationCore()
		{
			if (!Parent.API_Tariff.IsEmpty)
			{
				var length = Parent.API_Tariff.Length;

				if (length != 4 && length != 6 && length != 8 && length != 12)
				{
					Parent.API_TariffInfo.AddMessageError(Res.GetString("5EC11BC8-B5C3-4CEC-961F-E73E8DB5CE04", "Only 4, 6, 8, 12 digits are allowed"));
				}
				else
				{
					var tariffList = Parent.Lookups.TariffList;
					var filter = tariffList.CompleteFilter;
					filter.AddToFilter(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, Parent.API_Tariff);
					var result = Parent.Factory.LoadTop1<TariffView>(filter);

					if (result == null)
					{
						Parent.API_TariffInfo.AddMessageError(Res.GetString("B46AB98D-3237-4901-B87B-600EEBFC8390", "The code you have selected is not in the list."));
					}
				}
			}
		}

		protected override void CheckAPI_GoodsDescription()
		{
			base.CheckAPI_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_GoodsDescriptionInfo);
		}

		protected override void CheckAPI_GrossWeight()
		{
			base.CheckAPI_GrossWeight();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_GrossWeightInfo);
		}

		protected override void CheckMandatoryAPI_CustomsUQ()
		{
		}
	}
}
