using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	class GatewayAgentTypeComparer : BaseRateLineComparer
	{
		public override int Compare(FastLine line1, FastLine line2) =>
			GatewayAgentPriority(line1.ParentRateEntry) - GatewayAgentPriority(line2.ParentRateEntry);

		int GatewayAgentPriority(IRateEntry entry) =>
			Priorities.TryGetValue(entry.TI_GatewayAgentType, out var priority) ? priority : 1;

		protected override string GetName()
		{
			return (NoResString)"Gateway Agent Type"; // log message, subject to change, more for support people as of now
		}

		readonly Dictionary<string, int> Priorities = new Dictionary<string, int>
		{
			{ GatewayAgentType.Codes.SendingAgent, 2 },
			{ GatewayAgentType.Codes.ReceivingAgent, 2 }
		};
	}
}
