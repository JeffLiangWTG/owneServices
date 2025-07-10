using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaPackedItemTaxValidation : ASYCUDA.Business.AsycudaTaxValidation
	{
		public AsycudaPackedItemTaxValidation(AsycudaPackedItemTax parent) : base(parent)
		{
		}

		protected new AsycudaPackedItemTax Parent => (AsycudaPackedItemTax)base.Parent;

		protected override void CheckAET_ChargeType()
		{
			base.CheckAET_ChargeType();
			var targetInfo = Parent.AET_ChargeTypeInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
			var parent = Parent;
			if (parent.PackedItem?.AsycudaTaxes?.Cast<AsycudaPackedItemTax>().Any(tax => tax.AET_ChargeType == parent.AET_ChargeType && tax.PK != parent.PK) ?? false)
			{
				targetInfo.AddMessageError(ValidationConstants.InvoiceLineTax.DuplicateTariffTypeFound(parent.AET_ChargeTypeDesc));
			}
		}

		protected override void CheckAET_Tariff()
		{
			base.CheckAET_Tariff();
			var parent = Parent;
			if (parent.IsTaxCharge)
			{
				var targetInfo = parent.AET_TariffInfo;
				var tariff = parent.AET_Tariff;
				var tariffType = parent.AET_ChargeType;
				var packedItem = parent.PackedItem;
				var mainTariff = packedItem?.API_Tariff ?? ZString.Empty;
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				if (!tariff.IsEmpty)
				{
					if (parent.UniversalTariffType == null)
					{
						ListValidation.MessageErrorIfInvalidCode(targetInfo);
					}
					else if (parent.UniversalTariff == null)
					{
						targetInfo.AddMessageError(ValidationConstants.InvoiceLineTax.TariffDoesNotBelongToType(tariff, tariffType));
					}

					if (!mainTariff.IsEmpty)
					{
						var tariffValidBasedOnMainTariff = packedItem?.UniversalTariff?.GetEffectiveChildTariffs(parent.EffectiveAssessmentDate)?.Any(x => string.Equals(x.ZZ1_TariffCode, tariff, System.StringComparison.OrdinalIgnoreCase)) ?? false;
						if (!tariffValidBasedOnMainTariff)
						{
							targetInfo.AddMessageError(ValidationConstants.InvoiceLineTax.ChildTariffDoesNotBelongToMainTariff(tariff, mainTariff));
						}
					}
				}
			}
		}

		protected override void CheckAET_BaseValue()
		{
			base.CheckAET_BaseValue();
			MandatoryValidation.CheckEntered(Parent.AET_BaseValueInfo);
		}
	}
}
