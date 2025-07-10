using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	class LicenceValidator
	{
		public void ValidateRequirementForACE(ZPropertyInfo info, JobComInvoiceLine invoiceLine, USCTariff importTariff)
		{
			if (importTariff != null)
			{
				if (!importTariff.UE_PermitLicenseIndicator.IsEmpty)
				{
					var permitDescription = LicencePermitTypeList.GetLicencePermitTypeList(invoiceLine.Factory).GetDescriptionFromCode(importTariff.UE_PermitLicenseIndicator);
					var validator = PermitValidatorHelper.GetPermitValidator(importTariff.UE_PermitLicenseIndicator, invoiceLine);
					if (validator != null)
					{
						if (!HasLicence(invoiceLine.LicenceAndPermits, importTariff.UE_PermitLicenseIndicator))
						{
							var messageError = validator.GetErrorTextForRequirement(ZString.Empty);
							if (!messageError.IsEmpty)
							{
								info.AddMessageError(messageError);
							}
							else if (importTariff.UE_PermitLicenseIndicator == LicencePermitTypeList.Codes._11)
							{
								info.AddWarning(SoftwoodLumberMaybeRequired);
							}
							else if (!validator.IsPermitNoNotRequired())
							{
								info.AddWarning(string.Format(PermitPossiblyRequiredForSelectedTariff, permitDescription));
							}
						}
						else if (validator.IsPermitNoNotRequired())
						{
							var messageError = validator.GetErrorTextForPermitNotRequired();
							if (!messageError.IsEmpty)
							{
								info.AddMessageError(messageError);
							}
						}
					}
				}
				else
				{
					if (importTariff.IsEligibleForAGOATextileBenefits(invoiceLine.EffectiveDateForDutyRate) && !invoiceLine.LicenceAndPermits.Cast<LicenceAndPermit>().Any(x => x.CY_Code == LicencePermitTypeList.Codes._19))
					{
						info.AddMessageError(RequiredForAGOATextileClaims);
					}
				}
			}
		}
		internal const string SoftwoodLumberMaybeRequired = "Softwood Lumber information may need to be reported based on the Softwood Lumber Act of 2008.\r\nPlease refer to the legislation, CBP policy statements, and other information regarding the Softwood Lumber Importer Declaration Program.";
		internal const string RequiredForAGOATextileClaims = "AGOA Textile Permit No is required for this tariff no.";

		bool HasLicence(LicenceAndPermitCollection licenceAndPermits, ZString permitLicenseIndicator)
		{
			return licenceAndPermits.GetFirstElementHaving(permitLicenseIndicator) != null;
		}

		internal const string PermitRequiredForSelectedTariff = "Permit/License({0}) is required for the selected tariff number.";
		internal const string PermitPossiblyRequiredForSelectedTariff = "Permit/License({0}) may be required for the selected tariff number.";

		/// <summary>
		/// For ACE, the passed info is not a field where users enter Misc permit no.
		/// </summary>
		public void ValidateMiscPermitTypeRequirement(ZPropertyInfo info, JobComInvoiceLine invoiceLine, bool isPermitEntered)
		{
			if (!isPermitEntered)
			{
				if (invoiceLine.IsFTZAdmission)
				{
					if (!invoiceLine.US_LicenseType.IsEmpty && invoiceLine.US_MiscPermitNo.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfNotEntered(info);
					}
				}
				else
				{
					foreach (var importTariff in new USCTariff[] { invoiceLine.ImportSupTariff, invoiceLine.ImportTariff })
					{
						if (importTariff != null)
						{
							var validator = PermitValidatorHelper.GetPermitValidator(importTariff.UE_PermitLicenseIndicator, invoiceLine);
							if (validator != null)
							{
								var errorText = validator.GetErrorTextForRequirement(invoiceLine.US_MiscPermitNo);
								if (!errorText.IsEmpty)
								{
									info.AddMessageError(errorText);
								}
								else
								{
									info.AddWarning(string.Format(PermitPossiblyRequiredForSelectedTariff, LicencePermitTypeList.GetLicencePermitTypeList(invoiceLine.Factory).GetDescriptionFromCode(importTariff.UE_PermitLicenseIndicator)));
								}
							}
							else if (!importTariff.UE_PermitLicenseIndicator.IsEmpty)
							{
								info.AddMessageError(FormalImportAddInfoJobComInvoiceLineValidation.MiscPermitNoRequired);
							}
						}
					}
				}
			}
		}

		public void ValidateCottonCertificate(ZPropertyInfo info, JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.ImportTariff is USCTariff importTariff && importTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton))
			{
				var permitNo = (ZString)info.Value;
				var importer = invoiceLine.Declaration.Importer;
				if (!permitNo.IsEmpty && importer != null && importer.RequiredDocuments.Count > 0)
				{
					var cottonDocuments = importer.RequiredDocuments.Cast<JobRequiredDocument>().Where(x => x.EQ_DocType == Core.Constants.RefDocTypes.Cotton && x.EQ_DocNumber.EqualsIgnoringCase(permitNo));
					if (!cottonDocuments.Any())
					{
						info.AddMessageError(CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.MatchCottonCertificateOnImporter);
					}
					else if (cottonDocuments.Cast<JobRequiredDocument>().Any(x => x.EQ_ValidToDate < invoiceLine.EffectiveDateForDutyRate))
					{
						info.AddMessageError(CommonImportAddInfoJobComInvoiceLineValidation.Constants.PrimarySPI.CheckCottonCertificate);
					}
				}
			}
		}

		public void ValidateMiscPermitTypeFormat(ZPropertyInfo info, JobComInvoiceLine invoiceLine, Func<USCTariff, ZString> getPermitLicenseIndicator)
		{
			var permitNo = (ZString)info.Value;

			if (!permitNo.IsEmpty)
			{
				var hasValidTariff = false;
				var miscPermitRequired = false;

				foreach (var importTariff in new USCTariff[] { invoiceLine.ImportSupTariff, invoiceLine.ImportTariff })
				{
					if (importTariff != null)
					{
						var validator = PermitValidatorHelper.GetPermitValidator(getPermitLicenseIndicator(importTariff), invoiceLine);
						if (validator != null)
						{
							var errorText = validator.GetErrorTextIfInvalidFormatOrNotRequired(permitNo);
							if (!string.IsNullOrEmpty(errorText))
							{
								info.AddMessageError(errorText);
								break;
							}

							var warningText = validator.GetWarningText(permitNo);
							if (!string.IsNullOrEmpty(warningText))
							{
								info.AddWarning(warningText);
								break;
							}

							validator.ValidateForExtraCondition(info);
						}

						hasValidTariff = true;
						miscPermitRequired |= !importTariff.UE_PermitLicenseIndicator.IsEmpty;
					}
				}

				if (hasValidTariff && !miscPermitRequired)
				{
					info.AddWarning(FormalImportAddInfoJobComInvoiceLineValidation.MiscPermitNoIsNotRequired);
				}
			}
		}
	}
}
