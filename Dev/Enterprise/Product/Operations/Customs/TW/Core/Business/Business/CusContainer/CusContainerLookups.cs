
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusContainerLookups : Customs.Business.CusContainerLookups
	{
		public CusContainerLookups(CusContainer parent)
			: base(parent)
		{
		}

		public CusContainer Container
		{
			get { return Parent; }
		}

		protected new CusContainer Parent
		{
			get { return (CusContainer)base.Parent; }
		}

		public override CodeDescriptionPairList CO_FCL_LCL_NCT_List => Factory.GetCachedValue<TWContainerModesList>();
	}
}
