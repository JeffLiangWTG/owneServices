using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefServiceLevelLookups : AutoRefServiceLevelLookups
	{
		public RefServiceLevelLookups(AutoRefServiceLevel parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ServiceDeliveryTypeList
		{
			get { return new ServiceLevelDeliveryTypeList(); }
		}
	}
}
