//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSDrawbackNAFTAAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSDrawbackNAFTAAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USDrawbackNAFTAAddInfoValidation : AutoUSDrawbackNAFTAAddInfoValidation
	{
		public USDrawbackNAFTAAddInfoValidation(AutoUSDrawbackNAFTAAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_DRWNAFTACountryImportEntry()
		{
			base.CheckUS_DRWNAFTACountryImportEntry();

			if (IsACEDrawback)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWNAFTACountryImportEntryInfo);
			}
		}

		protected override void CheckUS_DRWNAFTACountryImportEntryDate()
		{
			base.CheckUS_DRWNAFTACountryImportEntryDate();

			if (IsACEDrawback)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWNAFTACountryImportEntryDateInfo);
			}
		}

		protected override void CheckUS_DRWNAFTACountryImportDuty()
		{
			base.CheckUS_DRWNAFTACountryImportDuty();

			if (IsACEDrawback)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWNAFTACountryImportDutyInfo);
			}
		}

		protected override void CheckUS_DRWNAFTACountryDutyRate()
		{
			base.CheckUS_DRWNAFTACountryDutyRate();

			if (IsACEDrawback)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWNAFTACountryDutyRateInfo);
			}
		}

		protected override void CheckUS_DRWNAFTACountryTariffNumber()
		{
			base.CheckUS_DRWNAFTACountryTariffNumber();

			if (IsACEDrawback)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DRWNAFTACountryTariffNumberInfo);
			}
		}

		protected override void CheckUS_DRWNAFTACountryOfExport()
		{
			base.CheckUS_DRWNAFTACountryOfExport();

			if (IsACEDrawback)
			{
				if (Parent.US_DRWNAFTACountryOfExport != Core.Constants.CountryCodes.Canada
					&& Parent.US_DRWNAFTACountryOfExport != Core.Constants.CountryCodes.Mexico)
				{
					Parent.US_DRWNAFTACountryOfExportInfo.AddMessageError(CountryOfExportMustBeCAOrMX);
				}
			}
		}
		internal const string CountryOfExportMustBeCAOrMX = "Country of Export must be CA or MX.";

		bool IsACEDrawback
		{
			get
			{
				var nafta = Parent.Parent as DrawbackNAFTA;
				var invoiceLine = nafta != null ? nafta.InvoiceLine : null;
				var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
				return declaration != null && declaration.IsACEDrawback;
			}
		}
	}
}
