using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondContainerLookups : Customs.Business.CusInBondContainerLookups
	{
		public CusInBondContainerLookups(CusInBondContainer parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ModeList => Factory.GetCachedValue<CusInBondContainerModeList>();

		public RefContainerCollection ContainerTypes => new RefContainerCollection(Factory);
	}
}
