using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USImportTTBLineAddInfoValidation : USTTBLineAddInfoValidation
	{
		public USImportTTBLineAddInfoValidation(USTTBLineAddInfo parent)
			: base(parent)
		{
		}

		protected new USTTBLineAddInfo Parent
		{
			get { return (USTTBLineAddInfo)base.Parent; }
		}

		protected TTBLine TTBLine
		{
			get { return Parent.Parent; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateCOLANumberIsEnteredWhenRequiredByTariffRule();
			ValidateForeignCertificateIsEnteredWhenRequiredByTariffRule();
			ValidateCigarIsEnteredWhenRequiredByTariffRule();
		}

		void ValidateCOLANumberIsEnteredWhenRequiredByTariffRule()
		{
			var ttbLine = TTBLine;
			if (!ttbLine.IsTobaccoProgramType && ttbLine.COLAAndCertificates.Count != 1 && IsPGAValidation)
			{
				var invoiceLine = ttbLine.InvoiceLine;
				var tariff = invoiceLine == null ? null : invoiceLine.ImportTariff;
				if (tariff != null && tariff.IsCOLANumberRequiredForTTB(invoiceLine.EffectiveDateForDutyRate) && !ttbLine.COLAAndCertificates.Cast<TTBCOLAAndCertificate>().Any(x => !x.US_COLA.IsEmpty || !x.US_COLAExemptionCode.IsEmpty))
				{
					Parent.AddRowMessageError(ValidationConstants.TTB.ACOLANumberIsRequiredForThisTTBEntry);
				}
			}
		}

		void ValidateForeignCertificateIsEnteredWhenRequiredByTariffRule()
		{
			var ttbLine = TTBLine;
			if (!ttbLine.IsTobaccoProgramType && ttbLine.COLAAndCertificates.Count != 1 && IsPGAValidation)
			{
				var invoiceLine = ttbLine.InvoiceLine;
				var tariff = invoiceLine == null ? null : invoiceLine.ImportTariff;
				if (tariff != null && tariff.IsForeignCertificateRequiredForTTB(invoiceLine.EffectiveDateForDutyRate) && !ttbLine.COLAAndCertificates.Cast<TTBCOLAAndCertificate>().Any(x => !x.US_ForeignCertificateCountry.IsEmpty))
				{
					Parent.AddRowWarning(ValidationConstants.TTB.AForeignCertificateMaybeRequiredForThisTTBEntry);
				}
			}
		}

		void ValidateCigarIsEnteredWhenRequiredByTariffRule()
		{
			var ttbLine = TTBLine;
			if (ttbLine.IsTobaccoProgramType && ttbLine.Cigars.Count == 0 && IsPGAValidation)
			{
				var invoiceLine = ttbLine.InvoiceLine;
				var tariff = invoiceLine != null ? invoiceLine.ImportTariff : null;
				if (tariff != null && tariff.IsCigarRequiredForTTB(invoiceLine.EffectiveDateForDutyRate))
				{
					Parent.AddRowMessageError(ValidationConstants.TTB.CigarIsRequiredForThisTTBEntry);
				}
			}
		}

		protected override void CheckUS_ProgramCode()
		{
			base.CheckUS_ProgramCode();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_ProgramCodeInfo, Parent.Lookups.ProgramCodes);
				ValidateUS_PermitExemptionCode();
				ValidateUS_NumberForIRC();
				ValidateUS_OA_ConsigneeAddress();
				ValidateUS_QuantityInPCS();
			}
		}

		protected override void CheckUS_ProcessingCode()
		{
			base.CheckUS_ProcessingCode();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_ProcessingCodeInfo, Parent.Lookups.ProcessingCodes);
				ValidateUS_QuantityInPCS();
			}
		}

		protected override void CheckUS_IsReleaseUnderBond()
		{
			base.CheckUS_IsReleaseUnderBond();
			ValidateUS_NumberForIRC();
			ValidateUS_OA_ConsigneeAddress();
		}

		protected override void CheckUS_NumberForIRC()
		{
			base.CheckUS_NumberForIRC();
			if (Parent.US_IsReleaseUnderBond && IsPGAValidation)
			{
				if (Parent.US_NumberForIRC.IsEmpty)
				{
					if (TTBLine.IsTobaccoProgramType)
					{
						Parent.US_NumberForIRCInfo.AddMessageError(ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);
					}
					else
					{
						Parent.US_NumberForIRCInfo.AddWarning(ValidationConstants.TTB.IRCRegistryNumberIsRequired);
					}
				}
				else
				{
					var formatMessageText = TTIRegistrationNumberValidator.Validate(Parent.US_NumberForIRC);
					if (!formatMessageText.IsEmpty)
					{
						Parent.US_NumberForIRCInfo.AddMessageError(formatMessageText);
					}
				}
			}
		}

		protected override void CheckUS_OA_ConsigneeAddress()
		{
			base.CheckUS_OA_ConsigneeAddress();
			if (Parent.US_IsReleaseUnderBond && Parent.US_OA_ConsigneeAddress.IsValid && IsPGAValidation)
			{
				var consigneeAddress = TTBLine.ConsigneeAddress;
				if (consigneeAddress != null)
				{
					var organisationType = TTBLine.IsTobaccoProgramType ? "receiving manufacturer or export warehouse proprietor" : "receiving brewery, bonded wine cellar, or DSP";
					var consignee = consigneeAddress.Header;
					if (consignee != null && consignee.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, Core.Constants.CountryCodes.UnitedStates).IsEmpty)
					{
						Parent.US_OA_ConsigneeAddressInfo.AddMessageError(ValidationConstants.TTB.EINIsRequired(organisationType, consignee.OH_Code));
					}
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_ConsigneeAddressInfo, consigneeAddress);
				}
			}
		}

		protected override void CheckUS_PermitNumber()
		{
			base.CheckUS_PermitNumber();

			if (IsPGAValidation)
			{
				if (Parent.US_PermitNumber.IsEmpty)
				{
					if (IsPermitNumberRequired)
					{
						Parent.US_PermitNumberInfo.AddMessageError(ValidationConstants.TTB.ImporterPermitNumberIsRequiredForThisTTBEntry);
					}
				}
				else
				{
					if (!Parent.US_PermitExemptionCode.IsEmpty)
					{
						Parent.US_PermitNumberInfo.AddMessageError(ValidationConstants.TTB.ImporterPermitNumberIsNotRequiredWhenExemptionEntered);
					}
				}
			}

			ValidateUS_PermitExemptionCode();
		}

		bool IsPermitNumberRequired
		{
			get
			{
				var result = Parent.US_PermitExemptionCode.IsEmpty;
				if (result)
				{
					var ttbLine = TTBLine;
					var invoiceLine = ttbLine == null ? null : ttbLine.InvoiceLine;
					var tariff = invoiceLine == null ? null : invoiceLine.ImportTariff;
					result = tariff != null && tariff.IsPermitNumberRequiredForTTB(invoiceLine.EffectiveDateForDutyRate);
				}
				return result;
			}
		}

		protected override void CheckUS_PermitExemptionCode()
		{
			base.CheckUS_PermitExemptionCode();
			if (!Parent.US_PermitExemptionCode.IsEmpty && IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_PermitExemptionCodeInfo, Parent.Lookups.PermitExemptionCodes);
				if (!Parent.US_PermitNumber.IsEmpty)
				{
					Parent.US_PermitExemptionCodeInfo.AddMessageError(ValidationConstants.TTB.EitherPermitNumberOrExemptionCodeIsRequiredButNotBoth);
				}
			}

			ValidateUS_PermitNumber();
		}

		protected override void CheckUS_QuantityInPCS()
		{
			base.CheckUS_QuantityInPCS();
			if (Parent.US_QuantityInPCS.IsEmpty && TTBLine.IsTobaccoProgramType && TTBLine.IsQuantityRequired && IsPGAValidation)
			{
				Parent.US_QuantityInPCSInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Quantity in PCS"));
			}
		}

		bool IsPGAValidation
		{
			get
			{
				var result = false;
				var ttbLine = Parent.Parent;
				if (ttbLine != null)
				{
					var invoiceLine = ttbLine.InvoiceLine;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}
	}
}
