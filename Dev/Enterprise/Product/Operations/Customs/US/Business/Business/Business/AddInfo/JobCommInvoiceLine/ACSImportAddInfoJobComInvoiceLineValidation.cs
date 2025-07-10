using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ACSImportAddInfoJobComInvoiceLineValidation : FormalImportAddInfoJobComInvoiceLineValidation
	{
		public ACSImportAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine addInfoInvoiceLine)
			: base(addInfoInvoiceLine)
		{
		}

		public override void ValidateSetIndicator()
		{
			base.ValidateSetIndicator();
			ValidateUS_SecondarySPI();
		}

		protected override void CheckUS_CAExportCertificate()
		{
			base.CheckUS_CAExportCertificate();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.IsSetXLine)
				{
					ValidateDataNotNeededForXLine(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit, Parent.US_CAExportCertificateInfo);
				}
				else
				{
					var exportCertificate = InvoiceLine.US_CAExportCertificate;
					if (!exportCertificate.IsEmpty)
					{
						var errorTextIfInvalid = new CanadaExportSugarCertificateValidator(InvoiceLine).GetErrorTextIfInvalidFormatOrNotRequired(exportCertificate);
						if (!errorTextIfInvalid.IsEmpty)
						{
							InvoiceLine.US_CAExportCertificateInfo.AddMessageError(errorTextIfInvalid);
						}
					}
				}
			}
		}

		protected override void CheckUS_AgricultureLicNo()
		{
			base.CheckUS_AgricultureLicNo();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.IsSetXLine)
				{
					ValidateDataNotNeededForXLine(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit, Parent.US_AgricultureLicNoInfo);
				}
				else
				{
					var errorText = new AgriculturalLicenseValidator(InvoiceLine).GetErrorTextIfInvalidFormatOrNotRequired(InvoiceLine.US_AgricultureLicNo);
					if (!errorText.IsEmpty)
					{
						InvoiceLine.US_AgricultureLicNoInfo.AddMessageError(errorText);
					}
				}
			}
		}

		protected override void CheckUS_SecondarySPI()
		{
			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (InvoiceLine.US_SecondarySPI == SecondarySpecProgIndicatorList.Codes.X || InvoiceLine.US_SecondarySPI == SecondarySpecProgIndicatorList.Codes.V)
				{
					ValidateSetXAndVAgainstRelationship(InvoiceLine.US_SecondarySPIInfo);
				}
			}

			base.CheckUS_SecondarySPI();

			Parent.Validation.ValidateJI_ParentID();
		}

		protected override void CheckUS_WoolLicenceNo()
		{
			base.CheckUS_WoolLicenceNo();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.IsSetXLine)
				{
					ValidateDataNotNeededForXLine(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit, Parent.US_WoolLicenceNoInfo);
				}
				else
				{
					var woolLicenseNumber = InvoiceLine.US_WoolLicenceNo;
					var woolLicenseValidator = new WoolLicenseValidator(InvoiceLine);
					var errorText = woolLicenseValidator.GetErrorTextForRequirement(woolLicenseNumber);
					if (errorText.IsEmpty)
					{
						errorText = woolLicenseValidator.GetErrorTextIfInvalidFormatOrNotRequired(woolLicenseNumber);
					}

					if (!errorText.IsEmpty)
					{
						InvoiceLine.US_WoolLicenceNoInfo.AddMessageError(errorText);
					}
				}
			}
		}

		protected override void CheckUS_CBTPACertificateNo()
		{
			base.CheckUS_CBTPACertificateNo();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (Parent.IsSetXLine)
				{
					ValidateDataNotNeededForXLine(CommonImportAddInfoJobComInvoiceLineValidation.Constants.OGA.LicenseAndPermit, Parent.US_CBTPACertificateNoInfo);
				}
				else
				{
					var certificateNumber = Parent.US_CBTPACertificateNo;
					var validator = new CaribbeanBasinTradePartnershipActCertificateValidator(InvoiceLine);
					var errorText = validator.GetErrorTextForRequirement(certificateNumber);
					if (errorText.IsEmpty)
					{
						errorText = validator.GetErrorTextIfInvalidFormatOrNotRequired(certificateNumber);
					}

					if (!errorText.IsEmpty)
					{
						Parent.US_CBTPACertificateNoInfo.AddMessageError(errorText);
					}
				}
			}
		}

		protected override bool ShouldValidateUS_MiscPermitNo
		{
			get { return InvoiceLine.IsEntrySummaryValidationMode; }
		}

		protected override void CheckUS_ADDuty()
		{
			base.CheckUS_ADDuty();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (InvoiceLine.IsADDManual && InvoiceLine.US_ADDuty == 0m)
				{
					InvoiceLine.US_ADDutyInfo.AddMessageError(NoSpecificDutyAmount);
				}
			}
		}

		internal const string NoSpecificDutyAmount = "You have indicated this case is on a specific rate, but have not entered the actual duty amount.";

		protected override void CheckUS_CVDuty()
		{
			base.CheckUS_CVDuty();

			if (InvoiceLine.IsEntrySummaryValidationMode)
			{
				if (InvoiceLine.IsCVDManual && InvoiceLine.US_CVDuty == 0m)
				{
					InvoiceLine.US_CVDutyInfo.AddMessageError(NoSpecificDutyAmount);
				}
			}
		}

		protected override void CheckUS_CVDCaseNo()
		{
			base.CheckUS_CVDCaseNo();
			CheckADD_CVDForRemoteLocationFiling(Parent.US_CVDCaseNoInfo);
		}

		protected override void CheckUS_ADDCaseNo()
		{
			base.CheckUS_ADDCaseNo();
			CheckADD_CVDForRemoteLocationFiling(Parent.US_ADDCaseNoInfo);
		}

		void CheckADD_CVDForRemoteLocationFiling(ZPropertyInfo info)
		{
			if (InvoiceLine.IsEntrySummaryValidationMode && !info.Value.IsEmpty)
			{
				if (Parent.Declaration.IsRemoteLocationFiling)
				{
					info.AddMessageError(ADD_CVDNotAllowedForRLF);
				}
			}
		}
		internal const string ADD_CVDNotAllowedForRLF = "ADD/CVD details may not be reported for remote location filing.";
	}
}
