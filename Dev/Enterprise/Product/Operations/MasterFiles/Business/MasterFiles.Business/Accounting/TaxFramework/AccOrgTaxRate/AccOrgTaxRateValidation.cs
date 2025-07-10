//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccOrgTaxRateValidation
//
//    This class should be used for overriding validation in AutoAccOrgTaxRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using System.Linq;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;

	public class AccOrgTaxRateValidation : AutoAccOrgTaxRateValidation
	{
		public AccOrgTaxRateValidation(AutoAccOrgTaxRate parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			Validate_Rate();
		}

		protected override void CheckOTR_EndDate()
		{
			base.CheckOTR_EndDate();
			MandatoryValidation.CheckEntered(Parent.OTR_EndDateInfo);

			if (!Parent.OTR_EndDateInfo.HasErrors())
			{
				if (Parent.OTR_StartDate > Parent.OTR_EndDate && !Parent.OTR_StartDate.IsEmpty)
				{
					Parent.OTR_EndDateInfo.AddError(Res.GetString("98C401E2-AAAA-447B-84AA-0135B61C5F3A", "End Date cannot be an earlier date than the start date."));
				}
			}
		}

		protected override void CheckOTR_StartDate()
		{
			base.CheckOTR_StartDate();
			MandatoryValidation.CheckEntered(Parent.OTR_StartDateInfo);

			if (!Parent.OTR_StartDateInfo.HasErrors())
			{
				if (Parent.OTR_StartDate > Parent.OTR_EndDate && !Parent.OTR_EndDate.IsEmpty)
				{
					Parent.OTR_StartDateInfo.AddError(Res.GetString("6BBFFED2-8C04-41BE-B0E2-09BCB974FAE2", "Start Date cannot be a later date than the End Date."));
				}

				if (Parent.OTR_OTC.IsValid && !Parent.OTR_StartDate.IsEmpty && !Parent.OTR_EndDate.IsEmpty && !Parent.OTR_Source.IsEmpty)
				{
					if (Parent.OrgTaxConfiguration != null)
					{
						if (Parent.OrgTaxConfiguration.TaxRates != null)
						{
							if (Parent.OrgTaxConfiguration.TaxRates.Any(accOrgTaxRate => Parent != accOrgTaxRate &&
								accOrgTaxRate.OTR_StartDate <= Parent.OTR_EndDate &&
								accOrgTaxRate.OTR_EndDate >= Parent.OTR_StartDate &&
								accOrgTaxRate.OTR_Source.Equals(Parent.OTR_Source) &&
								accOrgTaxRate.OTR_OTC.Equals(Parent.OTR_OTC)))
							{
								Parent.OTR_StartDateInfo.AddError(Res.GetString("B3115434-C74C-46C1-A7A8-3ED99CD8ED7C", "Date Ranges overlap for same Tax Code & Source."));
							}
						}
					}
				}
			}
		}

		protected override void CheckOTR_RateDenominator()
		{
			base.CheckOTR_RateDenominator();
			if (Parent.OTR_RateDenominator <= 0)
			{
				Parent.OTR_RateDenominatorInfo.AddError(Res.GetString("E87390EE-C75C-4FC4-B908-6D78E31ABC4B", "Denominator cannot be equal or less than zero."));
			}
		}

		protected override void CheckOTR_RateNumerator()
		{
			base.CheckOTR_RateNumerator();
			if (Parent.OTR_RateNumerator < 0)
			{
				Parent.OTR_RateNumeratorInfo.AddError(Res.GetString("260ADDA3-1FB1-4D47-90E2-F95331E9F3D1", "Numerator cannot be a negative number, only zero value is allowed."));
			}
		}

		protected override void CheckOTR_Source()
		{
			base.CheckOTR_Source();
			MandatoryValidation.CheckEntered(Parent.OTR_SourceInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OTR_SourceInfo);
		}

		public void Validate_Rate() => ValidateCalculatedProperty(Parent.RateInfo);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method is used via reflection (see GetValidationMethod() in ZValidation.cs)")]
		void CheckRate()
		{
			if (Parent.Rate > 100)
			{
				Parent.RateInfo.AddError(Res.GetString("06715fcb-9b58-4b48-bc92-5d0a0573eae7", "Rate must be less than 100%. Please correct the Numerator and Denominator entered."));
			}
		}
		new AccOrgTaxRate Parent
		{
			get { return base.Parent as AccOrgTaxRate; }
		}
	}
}
