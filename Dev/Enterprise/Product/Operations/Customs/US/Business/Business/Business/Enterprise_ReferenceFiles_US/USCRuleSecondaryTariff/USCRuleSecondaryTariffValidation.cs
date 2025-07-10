//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCRuleSecondaryTariffValidation
//
//    This class should be used for overriding validation in AutoUSCRuleSecondaryTariffValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USCRuleSecondaryTariffValidation : AutoUSCRuleSecondaryTariffValidation
	{
		public USCRuleSecondaryTariffValidation(AutoUSCRuleSecondaryTariff parent)
			: base(parent)
		{
		}

		public new USCRuleSecondaryTariff Parent
		{
			get { return (USCRuleSecondaryTariff)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateFormattedTariffFrom();
			ValidateFormattedTariffTo();
			ValidateFormattedTariff2();
			ValidateFormattedTariff3();
		}

		public void ValidateFormattedTariffFrom()
		{
			ValidateCalculatedProperty(Parent.FormattedTariffFromInfo);
		}

		public void ValidateFormattedTariffTo()
		{
			ValidateCalculatedProperty(Parent.FormattedTariffToInfo);
		}

		public void ValidateFormattedTariff2()
		{
			ValidateCalculatedProperty(Parent.FormattedTariff2Info);
		}

		public void ValidateFormattedTariff3()
		{
			ValidateCalculatedProperty(Parent.FormattedTariff3Info);
		}

		protected void CheckFormattedTariffTo()
		{
			if (!Parent.U3_TariffTo.IsEmpty)
			{
				if (!PartialTariffValidator.IsValidPartialTariff(Parent.Factory, Parent.U3_TariffTo))
				{
					Parent.FormattedTariffToInfo.AddWarning(USCRuleSecondaryTariffValidation.InvalidTariffNumber);
				}

				if (Parent.U3_TariffTo.CompareTo(Parent.U3_TariffFrom) <= 0)
				{
					Parent.FormattedTariffToInfo.AddError(USCRuleSecondaryTariffValidation.TariffToNumberShouldBeGreaterThanTariffNumberFrom);
				}

				if (!Parent.U3_Tariff2.IsEmpty || !Parent.U3_Tariff3.IsEmpty)
				{
					Parent.FormattedTariffToInfo.AddError(CannotDefineAsRangeOfTariffNumbersWhenThereIsAdditionalTariffNumber);
				}
			}
			ValidateFormattedTariffFrom();
		}

		internal const string InvalidTariffNumber = "The selected tariff number is not valid. There are no complete tariff numbers starting with this number.";
		internal const string TariffToNumberShouldBeGreaterThanTariffNumberFrom = "This number should be greater than the 'Tariff From'.";
		internal const string CannotDefineAsRangeOfTariffNumbersWhenThereIsAdditionalTariffNumber = "'Tariff To' should not be entered when other secondary tariff numbers are entered.";

		protected void CheckFormattedTariffFrom()
		{
			MandatoryValidation.CheckEntered(Parent.FormattedTariffFromInfo, "Tariff From number");

			if (!Parent.U3_TariffFrom.IsEmpty)
			{
				if (!PartialTariffValidator.IsValidPartialTariff(Parent.Factory, Parent.U3_TariffFrom))
				{
					Parent.FormattedTariffFromInfo.AddWarning(USCTariffRuleValidation.InvalidTariffNumber);
				}

				USCTariffRule tariffRule = Parent.TariffRule;

				if (tariffRule != null)
				{
					if (!tariffRule.U1_Tariff.IsEmpty)
					{
						if (tariffRule.U1_Tariff == Parent.U3_TariffFrom)
						{
							Parent.FormattedTariffFromInfo.AddError(TariffsForRuleAndSecondaryTariffCannotBeTheSame);
						}
					}

					USCRuleSecondaryTariff duplicate = tariffRule.SecondaryTariffs.GetABroaderSecondaryTariffThan(Parent);

					if (duplicate != null)
					{
						ZStringBuilder explanationToAdd = new ZStringBuilder();

						explanationToAdd.Append("Tariff Number: ");
						explanationToAdd.Append(duplicate.FormattedTariffFrom);

						if (duplicate.U3_DateFrom.IsValid)
						{
							explanationToAdd.Append(" Effective from: ");
							explanationToAdd.Append(duplicate.U3_DateFrom.ToShortDateString());
						}

						if (duplicate.U3_DateTo.IsValid)
						{
							explanationToAdd.Append(" Effective to: ");
							explanationToAdd.Append(duplicate.U3_DateTo.ToShortDateString());
						}
						explanationToAdd.Append("\r\n");

						Parent.FormattedTariffFromInfo.AddError(DuplicateRecordAlreadyExists + "\r\n" + explanationToAdd.ToString());
					}
				}
			}

			ValidateFormattedTariff2();
		}

		internal const string TariffsForRuleAndSecondaryTariffCannotBeTheSame = "This number cannot be the same as its rule's tariff number.";
		internal const string DuplicateRecordAlreadyExists = "There already exists a record with the tariff number that is present in list for the effective dates.";

		protected override void CheckU3_DateFromIsNotEmpty()
		{
			//validated below
		}

		protected override void CheckU3_DateFromIsValidZDateTimeRange()
		{
			//do not want this validation. If users need to enter 10 years old records, then they should be able to
		}

		protected override void CheckU3_DateFrom()
		{
			base.CheckU3_DateFrom();
			MandatoryValidation.CheckEntered(Parent.U3_DateFromInfo, "date this tariff is effective from");
			ValidateU3_DateTo();
			ValidateFormattedTariffFrom();
		}

		protected override void CheckU3_DateTo()
		{
			base.CheckU3_DateTo();
			if (Parent.U3_DateTo.IsValid && Parent.U3_DateFrom.IsValid && Parent.U3_DateTo < Parent.U3_DateFrom)
			{
				Parent.U3_DateToInfo.AddError(DateToShouldBeLaterThanDateFrom);
			}
			ValidateFormattedTariffFrom();
		}

		internal const string DateToShouldBeLaterThanDateFrom = "You have entered 'Date To' earlier than 'Date From'. 'Date To' should be later than 'Date From'";

		protected override void CheckU3_DateToIsValidZDateTimeRange()
		{
			//do not want this validation. If users need to enter 10 years old records, then they should be able to
		}

		protected void CheckFormattedTariff2()
		{
			if (!Parent.U3_Tariff2.IsEmpty && Parent.U3_TariffFrom.IsEmpty)
			{
				Parent.FormattedTariff2Info.AddError(ASetOfSecondaryTariffsEntered + " You have entered 'Tariff 2' without the first tariff.");
			}

			ValidateFormattedTariffTo();
			ValidateFormattedTariff3();
		}

		protected void CheckFormattedTariff3()
		{
			if (!Parent.U3_Tariff3.IsEmpty && Parent.U3_Tariff2.IsEmpty)
			{
				Parent.FormattedTariff3Info.AddError(ASetOfSecondaryTariffsEntered + " You have entered 'Tariff 3' without the second tariff.");
			}

			ValidateFormattedTariffTo();
			ValidateFormattedTariff2();
		}

		internal const string ASetOfSecondaryTariffsEntered = "A set of secondary tariff numbers should be entered in this order, 'Tariff From', 'Tariff 2' if any, and then 'Tariff 3' if any.";
	}
}
