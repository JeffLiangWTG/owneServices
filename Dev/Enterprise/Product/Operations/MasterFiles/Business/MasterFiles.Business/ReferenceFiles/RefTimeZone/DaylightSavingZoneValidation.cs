using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class DaylightSavingZoneValidation : RefTimeZoneValidation
	{
		public DaylightSavingZoneValidation(DaylightSavingTimeZone daylightSavingZone)
			: base(daylightSavingZone)
		{
		}

		public new DaylightSavingTimeZone Parent
		{
			get { return (DaylightSavingTimeZone)base.Parent; }
		}

		public enum RuleValidationReturnTypes
		{
			Success,
			OverlapsExistInStartRules,
			OverlapsExistInEndRules,
			ValidationErrors,
			StartAndEndRuleYearsDiffer
		}

		/// <summary>
		/// Determines whether there is an end rule for each start rule. This is done by checking whether each year in a start rule
		/// has a corresponding year in an end rule. Note however, that in the northern hemisphere, daylight saving starts and ends in the 
		/// same year, whilst in the southern hemisphere, daylight savings starts in one year and ends on the next. This is somewhat 
		/// acommodated below, although full validation is not possible.
		/// </summary>
		/// <returns></returns>
		public RuleValidationReturnTypes DoStartAndEndRuleYearsCorrespond()
		{
			RuleValidationReturnTypes result = RuleValidationReturnTypes.Success;

			// if no validation errors in entered year values
			if (!ErrorsExistInRules(Parent.StartDateRules) && !ErrorsExistInRules(Parent.EndDateRules))
			{
				List<int> startRuleYears = GetAllYearsCoveredByRulesIfNoOverlaps(Parent.StartDateRules);
				List<int> endRuleYears = GetAllYearsCoveredByRulesIfNoOverlaps(Parent.EndDateRules);

				// if no overlaps in Parent.StartDateRules or Parent.EndDateRules
				if (startRuleYears != null && endRuleYears != null)
				{
					RefTimeZoneSet timeZoneSet = Parent.Factory.LoadTop1<RefTimeZoneSet>(new ZQuery(RefTimeZoneSetSchema.R3_R2_DaylightSavingZone, Parent.PK));
					if (timeZoneSet != null)
					{
						List<int> modifiedStartRuleYears = new List<int>();
						List<int> modifiedEndRuleYears = new List<int>();
						modifiedStartRuleYears.AddRange(startRuleYears);
						modifiedEndRuleYears.AddRange(endRuleYears);

						RemoveAllCommonYears(ref modifiedStartRuleYears, ref modifiedEndRuleYears, (startRuleYears.Contains(0) && endRuleYears.Contains(0)));

						RefUNLOCO uNLOCO = Parent.Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_R3, timeZoneSet.PK));

						// if a UNLOCO exists and is in northern hemisphere (and so daylight saving starts and ends in the same year)
						if (uNLOCO != null && !uNLOCO.RL_GeoLocation.IsEmpty && uNLOCO.RL_GeoLocation.Latitude.Value >= 0)
						{
							result = (modifiedEndRuleYears.Count > 0) ? RuleValidationReturnTypes.StartAndEndRuleYearsDiffer : RuleValidationReturnTypes.Success;
						}
						else // if hemisphere where Zone applies is unknown (or South) and so daylight saving could start in one year and finish in the next
						{
							int leastNumOfRules = (Parent.StartDateRules.Count < Parent.EndDateRules.Count) ? Parent.StartDateRules.Count : Parent.EndDateRules.Count;

							if (startRuleYears.Contains(0) && endRuleYears.Contains(0))
							{
								bool isYearsBetweenInfiniteRulesValid = DoYearsBetweenFromYearsOfInfRulesExist(startRuleYears, endRuleYears);
								result = (!isYearsBetweenInfiniteRulesValid || modifiedEndRuleYears.Count > leastNumOfRules || modifiedStartRuleYears.Count > leastNumOfRules) ? RuleValidationReturnTypes.StartAndEndRuleYearsDiffer : RuleValidationReturnTypes.Success;
							}
							else
							{
								result = (modifiedEndRuleYears.Count > leastNumOfRules || modifiedStartRuleYears.Count > leastNumOfRules) ? RuleValidationReturnTypes.StartAndEndRuleYearsDiffer : RuleValidationReturnTypes.Success;
							}

							#region Explanation

							// If daylight saving starts and ends in the same year, there should be nothing left in StartRuleYears or EndRuleYears
							//
							// If daylight saving starts and ends on *different* years, however (as in the southern hemisphere), then the 
							// maximum number of Years that should be left in StartRuleYears or EndRuleYears is equal to the minimum number of 
							// rules in either the StartRules or EndRules.
							//
							// e.g. if there are three start and end rules:
							//
							//		Start Rules: 1995 -> 2000, 2003 -> 2005, 2008 -> 2010
							//		End Rules  : 1996 -> 2001, 2004 -> 2006, 2009 -> 2011
							//
							// Then after the above code, StarRuleYears will still contain 1995, 2003 and 2008, while EndRuleYears will 
							// contain 2001, 2006, 2011. If there is one start rule, but three end rules however:
							//
							//		Start Rules: 1995 -> 2010
							//		End Rules  : 1996 -> 2001, 2002 -> 2006, 2007 -> 2011
							//
							// Then StartRuleYears will contain 1995 and EndRuleYears will contain 2011.
							//
							// If in any of the above two examples a start rule or end rule contained an extra year, then the number of elements
							// within either StartRules or EndRules would be greater than the minimum number of rules...and would hence 
							// return the type RuleValidationReturnTypes.StartAndEndRuleYearsDiffer. This has been used as the basis of validating
							// whether or not the Start and End Rules differ.

							#endregion
						}
					}
				}
				else
				{
					result = (startRuleYears == null) ? RuleValidationReturnTypes.OverlapsExistInStartRules : RuleValidationReturnTypes.OverlapsExistInEndRules;
				}
			}
			else
			{
				result = RuleValidationReturnTypes.ValidationErrors;
			}

			return result;
		}

		#region Implementation

		bool ErrorsExistInRules(RefTimeZoneRuleCollection collection)
		{
			bool errorsExist = false;

			foreach (RefTimeZoneRule rule in collection)
			{
				rule.Validation.ValidateR4_FromYear();
				rule.Validation.ValidateR4_ToYear();

				if (rule.R4_FromYearInfo.HasErrors() || rule.R4_ToYearInfo.HasErrors())
				{
					errorsExist = true;
					break;
				}
			}

			return errorsExist;
		}

		/// <summary>
		/// Gets the FromYear (of the rule which has the ToYear set to 0....and hence sets the rule from the FromYear to infinity)
		/// </summary>
		/// <param name="startOrEndRuleCollection"></param>
		/// <returns>The FromYear</returns>
		int GetFromYearOfInfiniteRule(RefTimeZoneRuleCollection startOrEndRuleCollection)
		{
			int result = 0;
			foreach (RefTimeZoneRule rule in startOrEndRuleCollection)
			{
				if (rule.R4_ToYear == 0)
				{
					result = rule.R4_FromYear;
					break;
				}
			}
			return result;
		}

		/// <summary>
		/// Removes all the common years that exist in the two lists that are passed in
		/// </summary>
		/// <param name="startRuleYears"></param>
		/// <param name="endRuleYears"></param>
		/// <param name="startAndEndRuleYearsContainZero"></param>
		void RemoveAllCommonYears(ref List<int> startRuleYears, ref List<int> endRuleYears, bool startAndEndRuleYearsContainZero)
		{
			if (startAndEndRuleYearsContainZero)
			{
				int startRuleFromYearWhereToYearIsZero = GetFromYearOfInfiniteRule(Parent.StartDateRules);
				int endRuleFromYearWhereToYearIsZero = GetFromYearOfInfiniteRule(Parent.EndDateRules);

				// Remove all years after the FromYear (of the Rule which has the ToYear as 0)
				if (startRuleFromYearWhereToYearIsZero < endRuleFromYearWhereToYearIsZero)
				{
					for (int i = endRuleYears.Count - 1; i >= 0; i--)
					{
						int year = endRuleYears[i];
						if (year > startRuleFromYearWhereToYearIsZero)
						{
							endRuleYears.Remove(year);
						}
					}
				}
				else
				{
					for (int i = startRuleYears.Count - 1; i >= 0; i--)
					{
						int year = startRuleYears[i];
						if (year > endRuleFromYearWhereToYearIsZero)
						{
							startRuleYears.Remove(year);
						}
					}
				}
			}

			// Remove all other common years from StartRuleYears and EndRuleYears
			for (int i = startRuleYears.Count - 1; i >= 0; i--)
			{
				int year = startRuleYears[i];
				if (endRuleYears.Contains(year))
				{
					startRuleYears.Remove(year);
					endRuleYears.Remove(year);
				}
			}
		}

		/// <summary>
		/// Determines if the years between the smallest FromYear of the Infinite Rules (a rule with the ToYear set to 0) and the biggest 
		/// FromYear exist.
		/// 
		/// i.e.
		///  if a rule has:
		///
		///		- Start Rules:  2006 -> 0
		///		- End Rules:	2000 -> 2009, 2010 -> 0
		/// 
		/// this method checks whether the years 2007, 2008 and 2009 exist. This is to ensure that if someone enters:
		/// 
		///		- Start Rules:	2000 -> 0
		///		- End Rules:	2002 -> 0
		/// 
		/// then the lack of an end rule for 2001 can be detected.
		/// </summary>
		/// <param name="startRuleYears"></param>
		/// <param name="endRuleYears"></param>
		/// <returns></returns>
		bool DoYearsBetweenFromYearsOfInfRulesExist(List<int> startRuleYears, List<int> endRuleYears)
		{
			bool result = true;

			int startRuleFromYearWhereToYearIsZero = GetFromYearOfInfiniteRule(Parent.StartDateRules);
			int endRuleFromYearWhereToYearIsZero = GetFromYearOfInfiniteRule(Parent.EndDateRules);

			if (startRuleFromYearWhereToYearIsZero < endRuleFromYearWhereToYearIsZero)
			{
				for (int year = startRuleFromYearWhereToYearIsZero + 1; year < endRuleFromYearWhereToYearIsZero; year++)
				{
					if (!endRuleYears.Contains(year))
					{
						result = false;
						break;
					}
				}
			}
			else if (startRuleFromYearWhereToYearIsZero > endRuleFromYearWhereToYearIsZero)
			{
				for (int year = endRuleFromYearWhereToYearIsZero + 1; year < startRuleFromYearWhereToYearIsZero; year++)
				{
					if (!startRuleYears.Contains(year))
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Gets all years specified in the rules contained within the collection. Returns null if it finds overlaps.
		/// </summary>
		/// <returns>int[] Years</returns>
		List<int> GetAllYearsCoveredByRulesIfNoOverlaps(RefTimeZoneRuleCollection collection)
		{
			List<int> yearsInAllRules = new List<int>();

			foreach (RefTimeZoneRule rule in collection)
			{
				List<int> yearsInThisRule = new List<int>();

				yearsInThisRule.Add(rule.R4_FromYear);

				if (rule.R4_FromYear != rule.R4_ToYear)
				{
					yearsInThisRule.Add(rule.R4_ToYear);
				}

				if (rule.R4_ToYear != 0)
				{
					yearsInThisRule.AddRange(GetAllYearsInBetween(rule.R4_FromYear, rule.R4_ToYear));
				}

				if (yearsInAllRules.Count == 0)
				{
					yearsInAllRules.AddRange(yearsInThisRule);
				}
				else
				{
					foreach (int year in yearsInThisRule)
					{
						if (yearsInAllRules.Contains(year))
						{
							yearsInAllRules = null;
							break;
						}
						else
						{
							yearsInAllRules.Add(year);
						}
					}
				}

				if (yearsInAllRules == null)
				{
					break;
				}
			}

			return yearsInAllRules;
		}

		/// <summary>
		/// Gets all the years in between two specified years. If the ToYear is 0 - i.e. infinity - null is returned.
		/// </summary>
		/// <param name="fromYear">The year after which the returned range starts</param>
		/// <param name="toYear">The year before which the returned range ends</param>
		/// <returns>A list of Years. Returns null if the ToYear was 0</returns>
		List<int> GetAllYearsInBetween(int fromYear, int toYear)
		{
			List<int> years = new List<int>();

			if (toYear != 0)
			{
				int yearInBetween = (fromYear != toYear) ? fromYear + 1 : toYear;

				while (yearInBetween != toYear)
				{
					years.Add(yearInBetween);
					yearInBetween++;
				}
			}
			return years;
		}

		#endregion
	}
}
