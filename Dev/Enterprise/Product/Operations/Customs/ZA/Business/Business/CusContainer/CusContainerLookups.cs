using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class CusContainerLookups : Customs.Business.CusContainerLookups
	{
		public CusContainerLookups(CusContainer parent)
			: base(parent)
		{
		}

		public CusContainer Container
		{
			get { return (CusContainer)Parent; }
		}

		public override CodeDescriptionPairList CO_FCL_LCL_NCT_List
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(Core.Constants.ContainerModes.Empty, "Empty");
				result.AddPair("FCG", "Containerised Cargo, FCL Groupage");
				result.AddPair(Core.Constants.ContainerModes.LCL, "Full, mixed consignment");
				result.AddPair(Core.Constants.ContainerModes.FCL, "Full, single consignment");
				return result;
			}
		}
	}
}
