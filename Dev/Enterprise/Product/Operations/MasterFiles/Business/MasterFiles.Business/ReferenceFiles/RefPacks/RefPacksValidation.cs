using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.MasterFiles.Business
{
	public class RefPacksValidation : AutoRefPacksValidation
	{
		public RefPacksValidation(AutoRefPacks parent) : base(parent)
		{
		}

		protected override void CheckRP_Type()
		{
			base.ValidateRP_Type();
			ListValidation.ErrorIfInvalidCode(Parent.RP_TypeInfo);
		}

		protected override void CheckRP_CommercialPack()
		{
			base.ValidateRP_CommercialPack();
			ListValidation.ErrorIfInvalidCode(Parent.RP_CommercialPackInfo);
		}

		protected override void CheckRP_CustomsPack()
		{
			base.ValidateRP_CustomsPack();
			ListValidation.ErrorIfInvalidCode(Parent.RP_CustomsPackInfo);
			MandatoryValidation.CheckEntered(Parent.RP_CustomsPackInfo);
			if (Parent.RP_CustomsPack.Length > 3 && Parent.RP_Type != RPTypeList.Codes.CommercialInvoice)
			{
				Parent.RP_CustomsPackInfo.AddError(Res.GetString("DEEB192D-6877-46BA-A783-172DC2579790", "Customs Pack Unit cannot be longer than 3 characters when Pack Conversion Type is different than CIP."));
			}
		}

		protected override void CheckRP_ConversionFactor()
		{
			base.ValidateRP_ConversionFactor();
			MandatoryValidation.CheckEntered(Parent.RP_ConversionFactorInfo);
			if (Parent.RP_ConversionFactor < 0)
			{
				Parent.RP_ConversionFactorInfo.AddError(Res.GetString("f3e4e9a1-90ab-496f-b214-80d3fc8951a5", "Conversion factor should be greater than zero"));
			}
		}
	}
}
