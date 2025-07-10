using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class CusCustomsOfficeLookups : ZLookups
	{
		public CusCustomsOfficeLookups(CusCustomsOffice parent)
			 : base(parent)
		{
		}

		protected new CusCustomsOffice Parent => (CusCustomsOffice)base.Parent;

		protected override BusinessObjectFactory Factory => Parent.Factory;

		public IBusinessObjectCollection CustomsOfficeList => TWRefCusCodeListTypes.GetCustomsOfficeCollection(Factory);
	}
}
