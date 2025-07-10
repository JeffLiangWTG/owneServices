using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class GlbPersonDuplicationFinderProxy : DuplicationFinderProxy<GlbPerson, GlbPerson>
	{
		protected GlbPersonDuplicationFinderProxy(GlbPerson masterBizO)
			: base(masterBizO)
		{
		}

		protected GlbPersonDuplicationFinderProxy(GlbPerson masterBizO, DeduplicationProxyConfig config)
			: base(masterBizO, config)
		{
		}

		public static GlbPersonDuplicationFinderProxy GetInstance(GlbPerson person)
		{
			return new GlbPersonDuplicationFinderProxy(person);
		}

		public static GlbPersonDuplicationFinderProxy GetInstance(GlbPerson person, DeduplicationProxyConfig config)
		{
			return new GlbPersonDuplicationFinderProxy(person, config);
		}
	}
}
