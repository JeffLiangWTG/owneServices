using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif

namespace Enterprise.MasterData.GUI
{
	internal class FilterHelper
	{
		internal static bool PersonEmailFilter(IDeduplicationGlowObject viewModel, FilterCondition filterCondition, string filterText)
		{
			var result = false;

			if (viewModel is DeduplicationGlbPerson deduplicationGlbPerson)
			{
				var propertiesList = new List<string>
				{
					deduplicationGlbPerson.PER_EmailAddress,
					deduplicationGlbPerson.PER_EmailAddress2
				};

				deduplicationGlbPerson.GlbStaffs?.ForEach(x => propertiesList.Add(x.GS_EmailAddress));
				deduplicationGlbPerson.OrgContacts?.ForEach(x => propertiesList.Add(x.OC_Email));
				deduplicationGlbPerson.HRJobApplicants?.ForEach(x => propertiesList.Add(x.HA_EmailAddress));

				result = Filter(filterCondition, filterText, propertiesList);
			}

			return result;
		}

		internal static bool PersonPhoneFilter(IDeduplicationGlowObject viewModel, FilterCondition filterCondition, string filterText)
		{
			var result = false;
			var deduplicationGlbPerson = viewModel as DeduplicationGlbPerson;

			if (deduplicationGlbPerson != null)
			{
				var propertiesList = new List<string>
				{
					deduplicationGlbPerson.PER_MobilePhone,
					deduplicationGlbPerson.PER_MobilePhone2,
					deduplicationGlbPerson.PER_HomePhone,
					deduplicationGlbPerson.PER_FaxNumber
				};

				deduplicationGlbPerson.GlbStaffs?.ForEach(x => propertiesList.AddRange(new List<string>()
				{
					x.GS_MobilePhone,
					x.GS_WorkPhone,
					x.GS_HomePhone,
					x.GS_FaxNum
				}));

				deduplicationGlbPerson.OrgContacts?.ForEach(x => propertiesList.AddRange(new List<string>()
				{
					x.OC_Mobile,
					x.OC_HomePhone,
					x.OC_OtherPhone,
					x.OC_Phone,
					x.OC_Fax
				}));

				deduplicationGlbPerson.HRJobApplicants?.ForEach(x => propertiesList.AddRange(new List<string>()
				{
					x.HA_WorkPhone
				}));

				result = Filter(filterCondition, filterText, propertiesList);
			}

			return result;
		}

		internal static bool PersonNameFilter(IDeduplicationGlowObject viewModel, FilterCondition filterCondition, string filterText)
		{
			var result = false;

			if (viewModel is DeduplicationGlbPerson deduplicationGlbPerson)
			{
				var propertiesList = new List<string> { deduplicationGlbPerson.PER_FullName, };

				deduplicationGlbPerson.GlbStaffs?.ForEach(x => propertiesList.Add(x.GS_FullName));
				deduplicationGlbPerson.OrgContacts?.ForEach(x => propertiesList.Add(x.OC_ContactName));

				result = Filter(filterCondition, filterText, propertiesList);
			}

			return result;
		}

		#region Winform Filter

		internal static bool PersonActiveFilter(DuplicationPersonCandidate candidate, string selectedActiveState)
		{
			var met = false;

			if (selectedActiveState == PersonFilterActivesList.Descriptions.All.GetUnresolvedString())
			{
				met = true;
			}
			else if (selectedActiveState == PersonFilterActivesList.Descriptions.Active.GetUnresolvedString())
			{
				met = candidate.IsActive;
			}
			else if (selectedActiveState == PersonFilterActivesList.Descriptions.Inactive.GetUnresolvedString())
			{
				met = !candidate.IsActive;
			}

			return met;
		}

		internal static bool PersonMultiConfidenceFilter(DuplicationPersonCandidate candidate, List<string> selectedRatings)
		{
			var met = false;

			if (!selectedRatings?.Any() ?? true)
			{
				met = true;
			}
			else
			{
				if (selectedRatings.Contains(nameof(ConfidenceRating.High)))
				{
					selectedRatings.Add(nameof(ConfidenceRating.Exact));
				}
				met = selectedRatings.Any(sr => sr != null && sr.Equals(candidate.Confidence.ToString(), StringComparison.OrdinalIgnoreCase));
			}

			return met;
		}

		#endregion

		internal static bool Filter(FilterCondition filterCondition, string filterText, IEnumerable<string> properties)
		{
			var meet = false;

			switch (filterCondition.ConditionType)
			{
				case FilterConditionType.Contains:
					meet = properties.Any(p => p != null && p.Contains(filterText, StringComparison.OrdinalIgnoreCase));
					break;
				case FilterConditionType.ExactMatch:
					meet = properties.Any(p => p != null && p.Equals(filterText, StringComparison.OrdinalIgnoreCase));
					break;
			}

			return meet;
		}
	}

	public enum FilterStyle
	{
		Default,
		Input,
		CheckBox
	}

	public enum DynamicFilterConditionType
	{
		StartsWith,
		Contains,
		ExactMatch,
		NotContain,
		NotEqual,
		NotStartWith
	}

	public enum FilterConditionType
	{
		Contains,
		ExactMatch
	}

	public class FilterCondition
	{
		public FilterCondition(FilterConditionType conditionType)
		{
			ConditionType = conditionType;

			switch (ConditionType)
			{
				case FilterConditionType.Contains:
					ConditionName = TextConstant.Contains;
					break;
				case FilterConditionType.ExactMatch:
					ConditionName = TextConstant.ExactMatch;
					break;
			}
		}

		public FilterConditionType ConditionType { get; }
		public ZString ConditionName { get; }
	}
}
