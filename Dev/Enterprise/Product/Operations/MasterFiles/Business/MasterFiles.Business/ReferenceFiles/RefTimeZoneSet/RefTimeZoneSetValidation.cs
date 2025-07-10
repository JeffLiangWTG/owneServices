using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefTimeZoneSetValidation : AutoRefTimeZoneSetValidation
	{
		public RefTimeZoneSetValidation(AutoRefTimeZoneSet parent) : base(parent)
		{
		}

		new RefTimeZoneSet Parent
		{
			get { return (RefTimeZoneSet)base.Parent; }
		}

		#region R3_TimeZoneSetName

		protected override void CheckR3_TimeZoneSetName()
		{
			base.CheckR3_TimeZoneSetName();
			MandatoryValidation.CheckEntered(Parent.R3_TimeZoneSetNameInfo);
			ValidateDuplicateTimeZoneSetName();
		}

		public void ValidateDuplicateTimeZoneSetName()
		{
			ZQuery query = new ZQuery(RefTimeZoneSetSchema.R3_TimeZoneSetName, Parent.R3_TimeZoneSetName);
			query.AddToFilter(JoinCondition.And, RefTimeZoneSetSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			if (Parent.Factory.ExistsInDatabase(nameof(RefTimeZoneSet), query))
			{
				Parent.R3_TimeZoneSetNameInfo.AddError(Res.GetString("28B69391-36C1-4C93-888B-BBD46859F350", "There is already a time zone with this name"));
			}
		}

		#endregion

		#region HasDaylightSavings

		public void ValidateHasDaylightSavings()
		{
			ValidateCalculatedProperty(Parent.HasDaylightSavingsInfo);
		}

		protected void CheckHasDaylightSavings()
		{
			if (Parent.DaylightSavingZone != null)
			{
				if (Parent.DaylightSavingZone.StartDateRules.Count == 0 || Parent.DaylightSavingZone.EndDateRules.Count == 0)
				{
					Parent.HasDaylightSavingsInfo.AddError(Res.GetString("4d03dce2-7821-4c7c-8bf8-d19a138e856a", "A daylight saving zone must have at least one start rule and one end rule."));
				}

				DaylightSavingZoneValidation.RuleValidationReturnTypes validationResponse = Parent.DaylightSavingZone.Validation.DoStartAndEndRuleYearsCorrespond();

				if (validationResponse == DaylightSavingZoneValidation.RuleValidationReturnTypes.StartAndEndRuleYearsDiffer)
				{
					Parent.HasDaylightSavingsInfo.AddError(Res.GetString("b82e65c5-81a6-4383-899a-88b904d161b1", "All start rules must have a corresponding end rule."));
				}
				else if (validationResponse == DaylightSavingZoneValidation.RuleValidationReturnTypes.OverlapsExistInEndRules)
				{
					Parent.HasDaylightSavingsInfo.AddError(Res.GetString("71183382-4b72-43b3-a5cf-647e2bffe5f4", "The year ranges of the end rules specified contain overlaps."));
				}
				else if (validationResponse == DaylightSavingZoneValidation.RuleValidationReturnTypes.OverlapsExistInStartRules)
				{
					Parent.HasDaylightSavingsInfo.AddError(Res.GetString("71183382-4b72-43b3-a5cf-647e2bffe5f4", "The year ranges of the end rules specified contain overlaps."));
				}
				else if (validationResponse == DaylightSavingZoneValidation.RuleValidationReturnTypes.ValidationErrors)
				{
					Parent.HasDaylightSavingsInfo.AddError(Res.GetString("b97f8240-ae8b-4a92-a718-b5a82b226589", "Validation errors exist within the specified start rules or end rules."));
				}
			}
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateHasDaylightSavings();
			base.ValidateAll();
		}
	}
}
