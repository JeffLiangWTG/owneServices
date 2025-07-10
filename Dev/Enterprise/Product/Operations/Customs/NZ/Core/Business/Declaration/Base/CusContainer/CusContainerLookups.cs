using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusContainerLookups : Customs.Business.CusContainerLookups
	{
		public CusContainerLookups(CusContainer container)
			: base(container)
		{
		}

		public override CodeDescriptionPairList CO_FCL_LCL_NCT_List
		{
			get { return Factory.GetCachedValue<ContainerModeList>(); }
		}

		public CodeDescriptionPairList CO_ContainerSizeList
		{
			get { return Factory.GetCachedValue<ContainerSizeList>(); }
		}

		public CodeDescriptionPairList MAFContainerTypeList
		{
			get
			{
				return Factory.GetCachedValue<NZContainerCodeList>();
			}
		}

		public ContainerYardCollection PackingLocationList
		{
			get { return fPackingLocationList ?? (fPackingLocationList = new ContainerYardCollection(Factory)); }
		}
		ContainerYardCollection fPackingLocationList;
	}
}
