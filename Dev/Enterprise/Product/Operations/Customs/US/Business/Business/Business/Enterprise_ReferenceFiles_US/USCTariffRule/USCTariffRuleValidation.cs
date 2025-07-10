//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCTariffRuleValidation
//
//    This class should be used for overriding validation in AutoUSCTariffRuleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class USCTariffRuleValidation : AutoUSCTariffRuleValidation
	{
		public USCTariffRuleValidation(AutoUSCTariffRule parent)
			: base(parent)
		{
		}

		new USCTariffRule Parent
		{
			get { return (USCTariffRule)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateFormattedTariff();
			ValidateFormattedTariffTo();
		}

		public void ValidateFormattedTariff()
		{
			ValidateCalculatedProperty(Parent.FormattedTariffInfo);
		}

		public void ValidateFormattedTariffTo()
		{
			ValidateCalculatedProperty(Parent.FormattedTariffToInfo);
		}

		protected void CheckFormattedTariff()
		{
			MandatoryValidation.CheckEntered(Parent.FormattedTariffInfo, "tariff number");

			if (!Parent.U1_Tariff.IsEmpty)
			{
				if (Parent.U1_RuleCode == TariffRuleList.Codes.EligibleForSecondaryTariffNumbers)
				{
					//If you want to change this, then USCTariffRuleAdhocCollection.Load method should change accordingly.
					//Due to the number of USCTariffRule records for STN, this limit is put on.
					if (Parent.Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, Parent.U1_Tariff)) == null)
					{
						Parent.FormattedTariffInfo.AddError(FullTariffNumberRequiredForSTNRule);
					}
				}
				else if (!PartialTariffValidator.IsValidPartialTariff(Parent.Factory, Parent.U1_Tariff))
				{
					Parent.FormattedTariffInfo.AddWarning(InvalidTariffNumber);
				}

				USCTariffRule[] duplicates = new USCTariffRule.Loader(Parent.Factory).LoadDuplicates(Parent.U1_RuleCode, Parent.U1_Tariff, Parent.EffectiveTariffTo, Parent.U1_DateFrom, Parent.U1_DateTo);
				if (duplicates.Length > 1)
				{
					ZStringBuilder explanationToAdd = new ZStringBuilder();
					foreach (USCTariffRule tariffRule in duplicates)
					{
						if (tariffRule != Parent)
						{
							explanationToAdd.Append("Tariff Number: ");
							explanationToAdd.Append(tariffRule.FormattedTariff);

							if (!tariffRule.U1_TariffTo.IsEmpty)
							{
								explanationToAdd.Append(" To ");
								explanationToAdd.Append(tariffRule.FormattedTariffTo);
							}

							if (tariffRule.U1_DateFrom.IsValid)
							{
								explanationToAdd.Append(" Effective from: ");
								explanationToAdd.Append(tariffRule.U1_DateFrom.ToShortDateString());
							}

							if (tariffRule.U1_DateTo.IsValid)
							{
								explanationToAdd.Append(" Effective to: ");
								explanationToAdd.Append(tariffRule.U1_DateTo.ToShortDateString());
							}
							explanationToAdd.Append("\r\n");
						}
					}

					Parent.FormattedTariffInfo.AddError(DuplicateRecordExist + "\r\n" + explanationToAdd.ToString());
				}
			}
		}

		internal const string FullTariffNumberRequiredForSTNRule = "A full tariff number including a stat code, if any, is required for a 'STN' tariff rule.";
		internal const string InvalidTariffNumber = "The selected tariff number is not valid. There are no complete tariff numbers starting with this number.";
		internal const string DuplicateRecordExist = "There already exists a record with the combination of the rule code and tariff that are valid for these effective dates.";

		protected void CheckFormattedTariffTo()
		{
			if (!Parent.U1_TariffTo.IsEmpty)
			{
				if (!PartialTariffValidator.IsValidPartialTariff(Parent.Factory, Parent.U1_TariffTo))
				{
					Parent.FormattedTariffToInfo.AddWarning(InvalidTariffNumber);
				}

				if (Parent.U1_TariffTo.CompareTo(Parent.U1_Tariff) <= 0)
				{
					Parent.FormattedTariffToInfo.AddError(TariffToNumberShouldBeGreaterThanTariffNumberFrom);
				}
			}
			ValidateFormattedTariff();
		}

		internal const string TariffToNumberShouldBeGreaterThanTariffNumberFrom = "This number should be greater than the 'Tariff From'.";

		protected override void CheckU1_RuleCode()
		{
			base.CheckU1_RuleCode();
			MandatoryValidation.CheckEntered(Parent.U1_RuleCodeInfo, "rule code");
			ListValidation.ErrorIfInvalidCode(Parent.U1_RuleCodeInfo, Parent.Lookups.RuleList);
			ValidateFormattedTariff();
		}

		protected override void CheckU1_DateFromIsNotEmpty()
		{
			//validated below
		}

		protected override void CheckU1_DateFromIsValidZDateTimeRange()
		{
			//do not want this validation. If users need to enter 10 years old records, then they should be able to
		}

		protected override void CheckU1_DateFrom()
		{
			base.CheckU1_DateFrom();
			MandatoryValidation.CheckEntered(Parent.U1_DateFromInfo, "date this rule is effective from");
			ValidateU1_DateTo();
			ValidateFormattedTariff();
		}

		protected override void CheckU1_DateTo()
		{
			base.CheckU1_DateTo();
			if (Parent.U1_DateTo.IsValid && Parent.U1_DateFrom.IsValid && Parent.U1_DateTo < Parent.U1_DateFrom)
			{
				Parent.U1_DateToInfo.AddError(DateToShouldBeLaterThanDateFrom);
			}
			ValidateFormattedTariff();
		}

		internal const string DateToShouldBeLaterThanDateFrom = "You have entered 'Date To' earlier than 'Date From'. 'Date To' should be later than 'Date From'";

		protected override void CheckU1_DateToIsValidZDateTimeRange()
		{
			//do not want this validation. If users need to enter 10 years old records, then they should be able to
		}
	}
}
