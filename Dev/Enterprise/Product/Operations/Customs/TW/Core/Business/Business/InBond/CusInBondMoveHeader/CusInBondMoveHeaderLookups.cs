using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondMoveHeaderLookups : Customs.Business.CusInBondMoveHeaderLookups
	{
		public CusInBondMoveHeaderLookups(CusInBondMoveHeader parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList EntryTypeList => Factory.GetCachedValue<EntryTypeList>();

		public CodeDescriptionPairList TranshipmentTransportCodeList => Factory.GetCachedValue<TranshipmentTransportCodeList>();

		public IBusinessObjectCollection FacilityCollection => TWRefCusCodeListTypes.GetGoodsLocationCollection(Factory, null);

		public RefVesselCollection Vessels => new RefVesselCollection(Factory);
	}
}
