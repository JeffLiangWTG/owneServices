using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Business
{
	public static class DataExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3002:Prefer direct member access over linq.", Justification = "Any() results in a extension method dealing with null that reads easy")]
		public static bool HasApprovedTIBExtension(this EDIMessage message)
		{
			return message?.GetMessageBlocks<Messaging.Business.MessageBuildingBlocks.ACE.Output.ATIBE1>(x => x.ConditionCode == ACETIBMessageConditionCodeList.Codes._995).Any() ?? false;
		}

		public static bool ShouldTrimSCACFromBills(this ZString billNumber, IEnumerable<ZString> allValidSCACs)
		{
			return allValidSCACs.Contains(billNumber.Left(4));
		}

		public static IEnumerable<ZString> GetValidSCACIssuerCodes(this IBillDetails billDetails, ZString transportMode)
		{
			return billDetails?.SCACIssuers.GetValidSCACs(transportMode) ?? Enumerable.Empty<ZString>();
		}
	}
}
