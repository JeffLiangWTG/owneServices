using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class AMSLotCodeCollection : Customs.Business.CusCodeDataCollection<AMSLotCode>
	{
		public AMSLotCodeCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.AMSLotCode)
		{
		}
	}
}
