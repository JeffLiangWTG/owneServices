using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business
{
	public class FTZAddInfoJobComInvoiceLineValidation : CommonImportAddInfoJobComInvoiceLineValidation
	{
		public FTZAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine addInfo)
			: base(addInfo)
		{
		}

		protected override bool IsManifestQtyRequired
		{
			get { return IsFTZAdmissionValidationMode && Parent.JI_CustomsUnitQty == ABIUnitOfMeasureList.Codes.NoUnitRequired; }
		}

		protected override string GetManifestQtyNotification()
		{
			return ManifestQtyRequired;
		}
		internal const string ManifestQtyRequired = "This quantity is required when Customs UQ is X.";

		protected override bool ShouldCheckUS_UC_NKCountryOfExport
		{
			get { return IsFTZAdmissionValidationMode; }
		}

		protected override bool ShouldCheckUS_UC_NKCountryOfOrigin
		{
			get { return IsFTZAdmissionValidationMode; }
		}

		protected override bool ShouldCheckUS_SPI
		{
			get { return IsFTZAdmissionValidationMode; }
		}

		protected override void CheckUS_ZoneStatus()
		{
			base.CheckUS_ZoneStatus();

			if (IsFTZAdmissionValidationMode)
			{
				CheckUS_ZoneStatusCommonForImport();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ZoneStatusInfo, ZoneStatusShouldBeEntered);

				var applicableSupTariffs = Parent.ApplicableSupTariffs;
				if (applicableSupTariffs.Length > 0 && Parent.US_ZoneStatus != ZoneStatusList.Codes.PrivilegedForeign)
				{
					Parent.US_ZoneStatusInfo.AddMessageError(ZoneStatusShouldBePForA99);
				}
			}

			if (Parent.IsFTZADDCVD && Parent.US_ZoneStatus != ZoneStatusList.Codes.PrivilegedForeign)
			{
				Parent.US_ZoneStatusInfo.AddMessageError(ZoneStatusShouldBePriviledgedForeign);
			}
		}
		internal const string ZoneStatusShouldBeEntered = "Zone Status";
		internal const string ZoneStatusShouldBePForA99 = "Zone Status must be \"P\" when a provisional/program tariff applies.";
		internal const string ZoneStatusShouldBePriviledgedForeign = "Zone Status should be Privileged Foreign Merchandise when ADD/CVD Case Number is completed.";

		protected override void CheckUS_UC_NKCountryOfOrigin()
		{
			base.CheckUS_UC_NKCountryOfOrigin();
			ValidateUS_ZoneStatus();

			if (ShouldCheckUS_UC_NKCountryOfOrigin)
			{
				ValidateUS_ADDCaseNo();
				ValidateUS_CVDCaseNo();
			}
		}

		protected override void CheckUS_SupTariff()
		{
			base.CheckUS_SupTariff();
			if (IsFTZAdmissionValidationMode && CalculateDutyForSetsHelper.Is9903Tariff(Parent.US_SupTariff))
			{
				Parent.US_SupTariffInfo.AddWarning(ProvisionalTariffShouldNotBeUsedIn214);
			}
		}
		internal const string ProvisionalTariffShouldNotBeUsedIn214 = "Provisional/Program Tariff should not be used in a 214 message. It is only needed on the Entry Type 06 withdrawal from the zone.";

		protected override void CheckUS_F_PNDisclaimer()
		{
			base.CheckUS_F_PNDisclaimer();

			ListValidation.MessageErrorIfInvalidCode(Parent.US_F_PNDisclaimerInfo);
		}

		protected override bool ShouldValidateUS_MiscPermitNo
		{
			get { return true; }
		}

		bool IsFTZAdmissionValidationMode
		{
			get { return Parent.Declaration.IsFTZAdmissionValidationMode; }
		}

		protected override void CheckUS_ADDCaseNo()
		{
			base.CheckUS_ADDCaseNo();
			ValidateUS_ZoneStatus();
		}

		protected override void CheckUS_CVDCaseNo()
		{
			base.CheckUS_CVDCaseNo();
			ValidateUS_ZoneStatus();
		}

		protected override void CheckUS_LicenseType()
		{
			base.CheckUS_LicenseType();
			ListValidation.MessageErrorIfInvalidCode(InvoiceLine.US_LicenseTypeInfo);
			var licenseType = InvoiceLine.LicenseTypeForFTZ;
			if (!licenseType.IsEmpty && licenseType != InvoiceLine.US_LicenseType)
			{
				InvoiceLine.US_LicenseTypeInfo.AddMessageError(LicenseNotApplicableToTariff);
			}
		}
		internal const string LicenseNotApplicableToTariff = "License/Certificate Type not applicable to this tariff";
	}
}
