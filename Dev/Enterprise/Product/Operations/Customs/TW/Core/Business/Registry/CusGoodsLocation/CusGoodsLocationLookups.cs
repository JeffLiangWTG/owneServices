using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CusGoodsLocationLookups : ZLookups
	{
		public CusGoodsLocationLookups(CusGoodsLocation parent)
			 : base(parent)
		{
		}

		protected new CusGoodsLocation Parent => (CusGoodsLocation)base.Parent;

		protected override BusinessObjectFactory Factory => Parent.Factory;

		public IBusinessObjectCollection GoodsLocationList => TWRefCusCodeListTypes.GetGoodsLocationCollection(Factory, Parent.CustomsOffice);

		public IBusinessObjectCollection CustomsOfficeList => TWRefCusCodeListTypes.GetCustomsOfficeCollection(Factory);

		public CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<TWJobMessageTypeList>();
	}
}
