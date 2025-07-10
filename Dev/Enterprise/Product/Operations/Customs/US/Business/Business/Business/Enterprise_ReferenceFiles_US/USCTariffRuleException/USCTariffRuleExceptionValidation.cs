//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCTariffRuleExceptionValidation
//
//    This class should be used for overriding validation in AutoUSCTariffRuleExceptionValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USCTariffRuleExceptionValidation : AutoUSCTariffRuleExceptionValidation
	{
		public USCTariffRuleExceptionValidation(AutoUSCTariffRuleException parent)
			: base(parent)
		{
		}

		public new USCTariffRuleException Parent
		{
			get { return (USCTariffRuleException)base.Parent; }
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

		protected void CheckFormattedTariffTo()
		{
			if (!Parent.U2_TariffTo.IsEmpty)
			{
				if (!PartialTariffValidator.IsValidPartialTariff(Parent.Factory, Parent.U2_TariffTo))
				{
					Parent.FormattedTariffToInfo.AddWarning(USCTariffRuleValidation.InvalidTariffNumber);
				}

				if (Parent.U2_TariffTo.CompareTo(Parent.U2_Tariff) <= 0)
				{
					Parent.FormattedTariffToInfo.AddError(USCTariffRuleValidation.TariffToNumberShouldBeGreaterThanTariffNumberFrom);
				}

				USCTariffRule tariffRule = Parent.TariffRule;

				if (tariffRule != null)
				{
					if (tariffRule.U1_TariffTo.IsEmpty)
					{
						if (!Parent.U2_TariffTo.StartsWith(tariffRule.U1_Tariff))
						{
							Parent.FormattedTariffToInfo.AddError(ExceptionTariffToShouldStartWithRuleTariff + tariffRule.FormattedTariff);
						}
					}
					else
					{
						if (!Parent.U2_TariffTo.StartsWith(tariffRule.U1_TariffTo) && Parent.U2_TariffTo.CompareTo(tariffRule.U1_TariffTo) > 0)
						{
							Parent.FormattedTariffToInfo.AddError(ExceptionTariffToShouldBeLessThanOrStartWithRuleTariff + tariffRule.FormattedTariffTo);
						}
					}
				}
			}
			ValidateFormattedTariff();
		}

		protected void CheckFormattedTariff()
		{
			MandatoryValidation.CheckEntered(Parent.FormattedTariffInfo, "tariff number");

			if (!Parent.U2_Tariff.IsEmpty)
			{
				if (!PartialTariffValidator.IsValidPartialTariff(Parent.Factory, Parent.U2_Tariff))
				{
					Parent.FormattedTariffInfo.AddWarning(USCTariffRuleValidation.InvalidTariffNumber);
				}

				USCTariffRule tariffRule = Parent.TariffRule;

				if (tariffRule != null)
				{
					if (!tariffRule.U1_Tariff.IsEmpty)
					{
						if (tariffRule.U1_Tariff == Parent.U2_Tariff)
						{
							Parent.FormattedTariffInfo.AddError(TariffsForRuleAndExceptionCannotBeTheSame);
						}
						else
						{
							if (tariffRule.U1_TariffTo.IsEmpty)
							{
								if (!Parent.U2_Tariff.StartsWith(tariffRule.U1_Tariff))
								{
									Parent.FormattedTariffInfo.AddError(ExceptionTariffShouldStartWithRuleTariff + tariffRule.FormattedTariff);
								}
							}
							else
							{
								if (!Parent.U2_Tariff.StartsWith(tariffRule.U1_Tariff) && Parent.U2_Tariff.CompareTo(tariffRule.U1_Tariff) < 0)
								{
									Parent.FormattedTariffInfo.AddError(ExceptionTariffShouldBeGreaterThanOrStartWithRuleTariff + tariffRule.FormattedTariff);
								}

								if (!Parent.U2_Tariff.StartsWith(tariffRule.U1_TariffTo) && Parent.U2_Tariff.CompareTo(tariffRule.U1_TariffTo) > 0)
								{
									Parent.FormattedTariffInfo.AddError(ExceptionTariffShouldBeLessThanOrStartWithRuleTariffTo + tariffRule.FormattedTariffTo);
								}
							}
						}
					}

					USCTariffRuleException duplicate = tariffRule.RuleExceptions.GetABroaderExceptionThan(Parent);

					if (duplicate != null)
					{
						ZStringBuilder explanationToAdd = new ZStringBuilder();

						explanationToAdd.Append("Tariff Number: ");
						explanationToAdd.Append(duplicate.FormattedTariff);

						if (duplicate.U2_DateFrom.IsValid)
						{
							explanationToAdd.Append(" Effective from: ");
							explanationToAdd.Append(duplicate.U2_DateFrom.ToShortDateString());
						}

						if (duplicate.U2_DateTo.IsValid)
						{
							explanationToAdd.Append(" Effective to: ");
							explanationToAdd.Append(duplicate.U2_DateTo.ToShortDateString());
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
		internal const string ExceptionTariffToShouldBeLessThanOrStartWithRuleTariff = "This number should be less than or start with its rule's 'Tariff From', ";
		internal const string ExceptionTariffToShouldStartWithRuleTariff = "This number should start with its rule's 'Tariff From', ";
		internal const string DuplicateRecordAlreadyExists = "There already exists a record with the tariff number that is excluded from the rule for the effective dates.";

		protected override void CheckU2_DateFrom()
		{
			base.CheckU2_DateFrom();

			USCTariffRule tariffRule = Parent.TariffRule;

			if (tariffRule != null && tariffRule.U1_DateFrom.IsValid)
			{
				MandatoryValidation.CheckEntered(Parent.U2_DateFromInfo, "date this exception is effective from");

				if (Parent.U2_DateFrom < tariffRule.U1_DateFrom)
				{
					Parent.U2_DateFromInfo.AddError(ExceptionDateFromShouldBeLaterOrEqualToRuleDateFrom);
				}
			}

			ValidateFormattedTariff();
			ValidateU2_DateTo();
		}

		protected override void CheckU2_DateFromIsValidZDateTimeRange()
		{
			//do not want this validation. If users need to enter 10 years old records, then they should be able to
		}

		internal const string ExceptionDateFromShouldBeLaterOrEqualToRuleDateFrom = "'Date From' of the exception should be later than or equal to its rule's 'Date From'.";

		protected override void CheckU2_DateTo()
		{
			base.CheckU2_DateTo();

			USCTariffRule tariffRule = Parent.TariffRule;

			if (tariffRule != null && tariffRule.U1_DateTo.IsValid)
			{
				MandatoryValidation.CheckEntered(Parent.U2_DateToInfo, "date on which this exception is no longer effective");

				if (Parent.U2_DateTo.IsValid && Parent.U2_DateTo > tariffRule.U1_DateTo)
				{
					Parent.U2_DateToInfo.AddError(ExceptionDateToShouldBeEarlierOrEqualToRuleDateTo);
				}
			}

			if (Parent.U2_DateFrom.IsValid && Parent.U2_DateTo.IsValid && Parent.U2_DateTo < Parent.U2_DateFrom)
			{
				Parent.U2_DateToInfo.AddError(DateToShouldBeLaterThanDateFrom);
			}

			ValidateFormattedTariff();
		}

		internal const string ExceptionDateToShouldBeEarlierOrEqualToRuleDateTo = "'Date To' of the exception should be earlier than or equal to its rule's 'Date To'.";
		internal const string DateToShouldBeLaterThanDateFrom = "You have entered 'Date To' earlier than 'Date From'. 'Date To' should be later than 'Date From'";

		protected override void CheckU2_DateToIsValidZDateTimeRange()
		{
			//do not want this validation. If users need to enter 10 years old records, then they should be able to
		}
	}
}
