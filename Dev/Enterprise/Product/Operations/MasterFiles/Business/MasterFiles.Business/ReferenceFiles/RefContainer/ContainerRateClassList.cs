using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ContainerFreightRateClassList : CodeDescriptionPairList
	{
		public ContainerFreightRateClassList() : base()
		{
			AddRange(Env.Registry.Rating.ContainerFreightRateClassList);
			Sort();
		}
	}

	public class ContainerHandlingRateClassList : CodeDescriptionPairList
	{
		public ContainerHandlingRateClassList() : base()
		{
			AddRange(Env.Registry.Rating.ContainerHandlingRateClassList);
			Sort();
		}
	}
}
