using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondContainerLookups : Customs.Business.CusInBondContainerLookups
	{
		public CusInBondContainerLookups(CusInBondContainer parent)
			: base(parent)
		{
		}

		public RefContainerCollection ContainerTypes
		{
			get { return new RefContainerCollection(Factory); }
		}

		public ImportMessageStatusList MessageStatusList
		{
			get { return Factory.GetCachedValue<ImportMessageStatusList>(); }
		}
	}
}
