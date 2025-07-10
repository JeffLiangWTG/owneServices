using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefAirlineEFreightRuleCollection : DependentBusinessObjectCollection<RefAirlineEFreightRule, RefAirline>
	{
		public RefAirlineEFreightRuleCollection(RefAirline parent)
			: base(parent)
		{ }

		#region Overried

		public override bool ReadOnly
		{
			get { return Master.ReadOnly || base.ReadOnly; }
		}

		#endregion

		#region GetBestMatchRule

		public RefAirlineEFreightRule GetBestMatchRule(RefUNLOCO origin, RefUNLOCO destination)
		{
			if ((origin == null && destination == null) || !this.Any())
			{
				return null;
			}

			var query = BuildBaseFindRuleQuery(origin, destination);
			var findRules = Find(query).Cast<RefAirlineEFreightRule>().ToArray();

			var result = findRules.FirstOrDefault();

			if (origin != null && destination != null)
			{
				var originAndDestinationCompare = new OriginAndDestinationCompare(origin, destination);

				if (result != null)
				{
					for (var i = findRules.Length - 1; i > 0; i--)
					{
						var rule = findRules[i];
						if (originAndDestinationCompare.Compare(rule, result) > 0)
						{
							result = rule;
						}
					}
				}
			}

			return result;
		}

		ZQuery BuildBaseFindRuleQuery(RefUNLOCO origin, RefUNLOCO destination)
		{
			var query = new ZQuery();
			var fullCodeQuery = new ZQuery();
			var countryCodeQuery = new ZQuery();
			var emptyCodeQuery = new ZQuery();

			if (origin != null)
			{
				fullCodeQuery.AddToFilter(RefAirlineEFreightRuleSchema.RME_OriginLocation, origin.Code);
				fullCodeQuery.AddToFilter(RefAirlineEFreightRuleSchema.RME_OriginLocation, SQLComparisonOperator.NotEqual, ZString.Empty);

				countryCodeQuery.AddToFilter(RefAirlineEFreightRuleSchema.RME_OriginLocation, origin.RL_RN_NKCountryCode);
				countryCodeQuery.AddToFilter(RefAirlineEFreightRuleSchema.RME_OriginLocation, SQLComparisonOperator.NotEqual, ZString.Empty);

				emptyCodeQuery.AddToFilter(RefAirlineEFreightRuleSchema.RME_OriginLocation, ZString.Empty);
			}

			if (destination != null)
			{
				fullCodeQuery.AddToFilter(RefAirlineEFreightRuleSchema.RME_DestinationLocation, new[] { destination.Code, destination.RL_RN_NKCountryCode, ZString.Empty });

				countryCodeQuery.AddToFilter(RefAirlineEFreightRuleSchema.RME_DestinationLocation, new[] { destination.Code, destination.RL_RN_NKCountryCode, ZString.Empty });

				emptyCodeQuery.AddToFilter(RefAirlineEFreightRuleSchema.RME_DestinationLocation, SQLComparisonOperator.NotEqual, ZString.Empty);
				emptyCodeQuery.AddToFilter(RefAirlineEFreightRuleSchema.RME_DestinationLocation, new[] { destination.Code, destination.RL_RN_NKCountryCode });
			}

			query.AddToFilter(fullCodeQuery, JoinCondition.Or);
			query.AddToFilter(countryCodeQuery, JoinCondition.Or);
			query.AddToFilter(emptyCodeQuery, JoinCondition.Or);

			return query;
		}

		class OriginAndDestinationCompare : Comparer<RefAirlineEFreightRule>
		{
			public OriginAndDestinationCompare(RefUNLOCO origin, RefUNLOCO destination)
			{
				this.origin = origin;
				this.destination = destination;
			}

			readonly RefUNLOCO origin;
			readonly RefUNLOCO destination;

			public override int Compare(RefAirlineEFreightRule x, RefAirlineEFreightRule y)
			{
				return GetPriorityValue(x) - GetPriorityValue(y);
			}

			int GetPriorityValue(RefAirlineEFreightRule rule)
			{
				var priorityValue = 0;

				if (rule.RME_OriginLocation == origin.Code)
				{
					priorityValue = 5;
				}
				else if (!origin.RL_RN_NKCountryCode.IsEmpty && rule.RME_OriginLocation == origin.RL_RN_NKCountryCode)
				{
					priorityValue = 2;
				}
				else if (rule.RME_OriginLocation.IsEmpty && !rule.RME_DestinationLocation.IsEmpty)
				{
					priorityValue = -1;
				}

				if (rule.RME_DestinationLocation == destination.Code)
				{
					priorityValue += 2;
				}
				else if (!destination.RL_RN_NKCountryCode.IsEmpty && rule.RME_DestinationLocation == destination.RL_RN_NKCountryCode)
				{
					priorityValue += 1;
				}
				else if (rule.RME_DestinationLocation.IsEmpty && !rule.RME_OriginLocation.IsEmpty)
				{
					priorityValue += 0;
				}

				return priorityValue;
			}
		}

		#endregion
	}
}
