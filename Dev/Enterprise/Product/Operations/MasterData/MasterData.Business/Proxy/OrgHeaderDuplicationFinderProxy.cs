using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class OrgHeaderDuplicationFinderProxy : DuplicationFinderProxy<OrgHeader, OrgHeader>
	{
		protected OrgHeaderDuplicationFinderProxy(OrgHeader masterBizO) : base(masterBizO)
		{
		}

		protected OrgHeaderDuplicationFinderProxy(OrgHeader masterBizO, DeduplicationProxyConfig config)
			: base(masterBizO, config)
		{
		}

		public static OrgHeaderDuplicationFinderProxy GetInstance(OrgHeader header)
		{
			return new OrgHeaderDuplicationFinderProxy(header);
		}

		public static OrgHeaderDuplicationFinderProxy GetInstance(OrgHeader header, DeduplicationProxyConfig config)
		{
			return new OrgHeaderDuplicationFinderProxy(header, config);
		}
	}
}
