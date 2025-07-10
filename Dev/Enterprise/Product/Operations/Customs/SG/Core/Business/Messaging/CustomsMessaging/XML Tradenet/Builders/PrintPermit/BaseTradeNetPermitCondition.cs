using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class BaseTradeNetPermitCondition : ITN41PermitConditions
	{
		public BaseTradeNetPermitCondition(string condition)
		{
			Condition = condition;
		}

		public ZString Condition { get; }

		public static IEnumerable<ITN41PermitConditions> GetPermitConditions(IApprovalCondition[] conditions)
		{
			if (conditions != null && conditions.Any())
			{
				foreach (var condition in conditions)
				{
					var pos = 73;

					ZString description = condition.ConditionDescription.Replace("\\", "/");
					yield return new BaseTradeNetPermitCondition(FormattableString.Invariant($"<b><ExpandToFit>{((ZString)condition.ConditionCode).SubstringSafe(0, 4),-4}</b> - {description.SubstringSafe(0, pos)}"));

					while (pos < description.Length)
					{
						yield return new BaseTradeNetPermitCondition(description.SubstringSafe(pos, 80));
						pos += 80;
					}
				}
			}
		}
	}
}
