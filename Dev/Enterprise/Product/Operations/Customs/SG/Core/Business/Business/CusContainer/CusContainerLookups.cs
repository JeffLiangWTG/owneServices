using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
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

		public override CodeDescriptionPairList CO_FCL_LCL_NCT_List
		{
			get { return Factory.GetCachedValue<ContainerTypeCodeList>(); }
		}

		public override CodeDescriptionPairList ContainerSizeList
		{
			get { return Factory.GetCachedValue<SGContainerSizeList>(); }
		}
	}
}
