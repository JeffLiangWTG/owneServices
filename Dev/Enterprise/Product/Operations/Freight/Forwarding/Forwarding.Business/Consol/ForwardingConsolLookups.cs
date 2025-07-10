using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolLookups : JobConsolLookups
	{
		public ForwardingConsolLookups(AutoJobConsol parent) : base(parent)
		{
		}

		public override RefServiceLevelCollection GatewayServiceLevels =>
			new GatewayServiceLevelCollection(Factory);

		#region BillOfLadingBillStatusList

		public CodeDescriptionPairList BillOfLadingBillStatusList => Factory.GetCachedValue("FreightCodePairLists.BillOfLadingBillStatusList", () => FreightCodePairLists.BillOfLadingBillStatusList());

		#endregion
	}
}
