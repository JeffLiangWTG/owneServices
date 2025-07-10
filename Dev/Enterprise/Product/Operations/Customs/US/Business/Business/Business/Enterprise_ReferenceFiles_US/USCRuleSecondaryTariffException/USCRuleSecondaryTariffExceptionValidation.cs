//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCRuleSecondaryTariffExceptionValidation
//
//    This class should be used for overriding validation in AutoUSCRuleSecondaryTariffExceptionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USCRuleSecondaryTariffExceptionValidation : AutoUSCRuleSecondaryTariffExceptionValidation
	{
		public USCRuleSecondaryTariffExceptionValidation(AutoUSCRuleSecondaryTariffException parent) : base(parent)
		{
		}

		new USCRuleSecondaryTariffException Parent
		{
			get { return (USCRuleSecondaryTariffException)base.Parent; }
		}

		public void ValidateFormattedTariff()
		{
			ValidateCalculatedProperty(Parent.FormattedTariffInfo);
		}

		protected void CheckFormattedTariff()
		{
			MandatoryValidation.CheckEntered(Parent.FormattedTariffInfo, "tariff number");

			if (!Parent.U4_Tariff.IsEmpty)
			{
				if (!PartialTariffValidator.IsValidPartialTariff(Parent.Factory, Parent.U4_Tariff))
				{
					Parent.FormattedTariffInfo.AddWarning(USCTariffRuleValidation.InvalidTariffNumber);
				}

				USCRuleSecondaryTariff tariffRule = Parent.SecondaryTariffRule;

				if (tariffRule != null)
				{
					if (!tariffRule.U3_TariffFrom.IsEmpty)
					{
						if (tariffRule.U3_TariffFrom == Parent.U4_Tariff)
						{
							Parent.FormattedTariffInfo.AddError(TariffsForRuleAndExceptionCannotBeTheSame);
						}
						else
						{
							if (tariffRule.U3_TariffTo.IsEmpty)
							{
								if (!Parent.U4_Tariff.StartsWith(tariffRule.U3_TariffFrom))
								{
									Parent.FormattedTariffInfo.AddError(ExceptionTariffShouldStartWithRuleTariff + tariffRule.FormattedTariffFrom);
								}
							}
							else
							{
								if (!Parent.U4_Tariff.StartsWith(tariffRule.U3_TariffFrom) && Parent.U4_Tariff.CompareTo(tariffRule.U3_TariffFrom) < 0)
								{
									Parent.FormattedTariffInfo.AddError(ExceptionTariffShouldBeGreaterThanOrStartWithRuleTariff + tariffRule.FormattedTariffFrom);
								}

								if (!Parent.U4_Tariff.StartsWith(tariffRule.U3_TariffTo) && Parent.U4_Tariff.CompareTo(tariffRule.U3_TariffTo) > 0)
								{
									Parent.FormattedTariffInfo.AddError(ExceptionTariffShouldBeLessThanOrStartWithRuleTariffTo + tariffRule.FormattedTariffTo);
								}
							}
						}
					}

					USCRuleSecondaryTariffException duplicate = tariffRule.Exceptions.GetABroaderExceptionThan(Parent);

					if (duplicate != null)
					{
						ZStringBuilder explanationToAdd = new ZStringBuilder();

						explanationToAdd.Append("Tariff Number: ");
						explanationToAdd.Append(duplicate.FormattedTariff);

						if (duplicate.U4_DateFrom.IsValid)
						{
							explanationToAdd.Append(" Effective from: ");
							explanationToAdd.Append(duplicate.U4_DateFrom.ToShortDateString());
						}

						if (duplicate.U4_DateTo.IsValid)
						{
							explanationToAdd.Append(" Effective to: ");
							explanationToAdd.Append(duplicate.U4_DateTo.ToShortDateString());
						}
						explanationToAdd.Append("\r\n");

						Parent.FormattedTariffInfo.AddError(DuplicateRecordAlreadyExists + "\r\n" + explanationToAdd.ToString());
					}
				}
			}
		}

		internal const string TariffsForRuleAndExceptionCannotBeTheSame = "This number cannot be the same as its rule's tariff number as it indicates that the rule applies to this tariff and is also excluded from the rule.";
		internal const string ExceptionTariffShouldBeGreaterThanOrStartWithRuleTariff = "This number should be greater than or start with its rule's 'Tariff From', ";
		internal const string ExceptionTariffShouldStartWithRuleTariff = "This number should start with its rule's 'Tariff From', ";
		internal const string ExceptionTariffShouldBeLessThanOrStartWithRuleTariffTo = "This number should be less than or start with its rule's 'Tariff To', ";
		internal const string DuplicateRecordAlreadyExists = "There already exists a record with the tariff number that is excluded from the rule for the effective dates.";

		protected override void CheckU4_DateFrom()
		{
			base.CheckU4_DateFrom();

			USCRuleSecondaryTariff tariffRule = Parent.SecondaryTariffRule;

			if (tariffRule != null && tariffRule.U3_DateFrom.IsValid)
			{
				MandatoryValidation.CheckEntered(Parent.U4_DateFromInfo, "date this exception is effective from");

				if (Parent.U4_DateFrom < tariffRule.U3_DateFrom)
				{
					Parent.U4_DateFromInfo.AddError(ExceptionDateFromShouldBeLaterOrEqualToRuleDateFrom);
				}
			}

			ValidateFormattedTariff();
			ValidateU4_DateTo();
		}

		protected override void CheckU4_DateFromIsValidZDateTimeRange()
		{
			//do not want this validation. If users need to enter 10 years old records, then they should be able to
		}

		internal const string ExceptionDateFromShouldBeLaterOrEqualToRuleDateFrom = "'Date From' of the exception should be later than or equal to its rule's 'Date From'.";

		protected override void CheckU4_DateTo()
		{
			base.CheckU4_DateTo();

			USCRuleSecondaryTariff tariffRule = Parent.SecondaryTariffRule;

			if (tariffRule != null && tariffRule.U3_DateTo.IsValid)
			{
				MandatoryValidation.CheckEntered(Parent.U4_DateToInfo, "date on which this exception is no longer effective");

				if (Parent.U4_DateTo.IsValid && Parent.U4_DateTo > tariffRule.U3_DateTo)
				{
					Parent.U4_DateToInfo.AddError(ExceptionDateToShouldBeEarlierOrEqualToRuleDateTo);
				}
			}

			if (Parent.U4_DateFrom.IsValid && Parent.U4_DateTo.IsValid && Parent.U4_DateTo < Parent.U4_DateFrom)
			{
				Parent.U4_DateToInfo.AddError(DateToShouldBeLaterThanDateFrom);
			}

			ValidateFormattedTariff();
		}

		internal const string ExceptionDateToShouldBeEarlierOrEqualToRuleDateTo = "'Date To' of the exception should be earlier than or equal to its rule's 'Date To'.";
		internal const string DateToShouldBeLaterThanDateFrom = "You have entered 'Date To' earlier than 'Date From'. 'Date To' should be later than 'Date From'";

		protected override void CheckU4_DateToIsValidZDateTimeRange()
		{
			//do not want this validation. If users need to enter 10 years old records, then they should be able to
		}
	}
}
