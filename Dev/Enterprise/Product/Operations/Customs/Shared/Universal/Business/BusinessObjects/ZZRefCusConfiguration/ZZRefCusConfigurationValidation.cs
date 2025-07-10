//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoZZRefCusConfigurationValidation
//
//    This class should be used for overriding validation in AutoZZRefCusConfigurationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class ZZRefCusConfigurationValidation : AutoZZRefCusConfigurationValidation
	{
		public ZZRefCusConfigurationValidation(AutoZZRefCusConfiguration parent) : base(parent)
		{
		}

		protected override void CheckZZC_IsReciprocalExchangeRate()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ZZC_IsReciprocalExchangeRateInfo);
			base.CheckZZC_IsReciprocalExchangeRate();
		}

		protected override void CheckZZC_CustomsValueCode()
		{
			MandatoryValidation.CheckEntered(Parent.ZZC_CustomsValueCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ZZC_CustomsValueCodeInfo);

			if (Parent.ZZC_VATValueCode == VATValueCodeList.Codes.FOB && Parent.ZZC_CustomsValueCode != CustomsValueCodeList.Codes.FOB)
			{
				Parent.ZZC_CustomsValueCodeInfo.AddError(Res.GetString("F08969C1-EDB5-4DD1-AF1C-D9D94FC3A896", "When the VAT Value Code is FOB then Customs Value Code must be FOB"));
			}
			base.CheckZZC_CustomsValueCode();
		}

		protected override void CheckZZC_VATValueCode()
		{
			MandatoryValidation.CheckEntered(Parent.ZZC_VATValueCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ZZC_VATValueCodeInfo);
			base.CheckZZC_VATValueCode();
		}

		protected override void CheckZZC_CustomsValueCodeForExport()
		{
			MandatoryValidation.CheckEntered(Parent.ZZC_CustomsValueCodeForExportInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ZZC_CustomsValueCodeForExportInfo);
			base.CheckZZC_CustomsValueCodeForExport();
		}

		protected override void CheckZZC_DefaultExportValuationDate()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ZZC_DefaultExportValuationDateInfo);
			base.CheckZZC_DefaultExportValuationDate();
		}

		protected override void CheckZZC_DefaultImportValuationDate()
		{
			ListValidation.ErrorIfInvalidCode(Parent.ZZC_DefaultImportValuationDateInfo);
			base.CheckZZC_DefaultImportValuationDate();
		}
	}
}
