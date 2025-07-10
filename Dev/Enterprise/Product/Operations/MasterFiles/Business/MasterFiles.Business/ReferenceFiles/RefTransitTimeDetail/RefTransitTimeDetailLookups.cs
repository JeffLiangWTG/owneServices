using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefTransitTimeDetailLookups : AutoRefTransitTimeDetailLookups
	{
		public RefTransitTimeDetailLookups(AutoRefTransitTimeDetail parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Days
		{
			get
			{
				return new DayOfWeekCodeList();
			}
		}
	}
}
