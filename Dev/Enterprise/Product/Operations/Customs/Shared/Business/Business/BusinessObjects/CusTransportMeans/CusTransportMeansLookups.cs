using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusTransportMeansLookups : AutoCusTransportMeansLookups
	{
		public CusTransportMeansLookups(AutoCusTransportMeans parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList TransportStateList
		{
			get { return Factory.GetCachedValue<CusUnloadedStateList>(); }
		}
	}
}
