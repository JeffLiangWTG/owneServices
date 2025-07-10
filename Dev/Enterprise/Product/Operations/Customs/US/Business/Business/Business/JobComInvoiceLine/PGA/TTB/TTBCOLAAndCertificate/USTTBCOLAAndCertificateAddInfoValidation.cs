//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSTTBCOLAAndCertificateAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSTTBCOLAAndCertificateAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	using CargoWise.EntityFramework;

	public class USTTBCOLAAndCertificateAddInfoValidation : AutoUSTTBCOLAAndCertificateAddInfoValidation
	{
		public USTTBCOLAAndCertificateAddInfoValidation(AutoUSTTBCOLAAndCertificateAddInfo parent)
			: base(parent)
		{
		}

		public void ValidateHasForeignCertificate()
		{
			ValidateCalculatedProperty(Parent.HasForeignCertificateInfo);
		}

		protected new USTTBCOLAAndCertificateAddInfo Parent
		{
			get { return (USTTBCOLAAndCertificateAddInfo)base.Parent; }
		}

		protected TTBCOLAAndCertificate COLAAndCertificate
		{
			get { return Parent.Parent; }
		}

		protected TTBLine TTBLine
		{
			get { return COLAAndCertificate.Parent; }
		}

		protected void CheckHasForeignCertificate()
		{
			if (Parent.HasForeignCertificate && Parent.US_ForeignCertificateCountry.IsEmpty)
			{
				Parent.HasForeignCertificateInfo.AddWarning(ValidationConstants.TTB.MissingForeignCertificateCountry);
			}
		}

		protected override void CheckUS_COLA()
		{
			base.CheckUS_COLA();
			if (IsPGAValidation && Parent.US_COLA.IsEmpty && Parent.US_COLAExemptionCode.IsEmpty)
			{
				var ttbLine = TTBLine;
				if (ttbLine != null && !ttbLine.IsTobaccoProgramType && (ttbLine.COLAAndCertificates.Count == 0 || (ttbLine.COLAAndCertificates.Count == 1 && ttbLine.COLAAndCertificates[0] == COLAAndCertificate)))
				{
					var invoiceLine = ttbLine.InvoiceLine;
					if (invoiceLine != null)
					{
						var tariff = invoiceLine.ImportTariff;
						if (tariff != null && tariff.IsCOLANumberRequiredForTTB(invoiceLine.EffectiveDateForDutyRate))
						{
							Parent.US_COLAInfo.AddMessageError(ValidationConstants.TTB.ACOLANumberIsRequiredForThisTTBEntry);
						}
					}
				}
			}
			ValidateUS_COLAExemptionCode();
		}

		protected override void CheckUS_COLAExemptionCode()
		{
			base.CheckUS_COLAExemptionCode();
			if (IsPGAValidation && !Parent.US_COLAExemptionCode.IsEmpty)
			{
				var ttbLine = TTBLine;
				if (ttbLine != null && !ttbLine.IsTobaccoProgramType)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_COLAExemptionCodeInfo, Parent.Lookups.COLAExemptionCodes);
					if (!Parent.US_COLA.IsEmpty)
					{
						Parent.US_COLAExemptionCodeInfo.AddMessageError(ValidationConstants.TTB.EitherCOLAOrExemptionCodeIsRequiredButNotBoth);
					}
				}
			}
			ValidateUS_COLA();
		}

		protected override void CheckUS_ForeignCertificateCountry()
		{
			base.CheckUS_ForeignCertificateCountry();
			var ttbLine = TTBLine;
			if (IsPGAValidation && ttbLine != null && !ttbLine.IsTobaccoProgramType)
			{
				var invoiceLine = ttbLine.InvoiceLine;
				if (invoiceLine != null)
				{
					if (Parent.US_ForeignCertificateCountry.IsEmpty)
					{
						var tariff = invoiceLine.ImportTariff;
						if (tariff != null && tariff.IsForeignCertificateRequiredForTTB(invoiceLine.EffectiveDateForDutyRate))
						{
							Parent.US_ForeignCertificateCountryInfo.AddWarning(ValidationConstants.TTB.AForeignCertificateMaybeRequiredForThisTTBEntry);
						}
					}
					else
					{
						ListValidation.MessageErrorIfInvalidCode(Parent.US_ForeignCertificateCountryInfo, Parent.Lookups.Countries);
					}
				}
			}
			ValidateHasForeignCertificate();
		}

		bool IsPGAValidation
		{
			get
			{
				var result = false;
				var ttbLine = TTBLine;
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
