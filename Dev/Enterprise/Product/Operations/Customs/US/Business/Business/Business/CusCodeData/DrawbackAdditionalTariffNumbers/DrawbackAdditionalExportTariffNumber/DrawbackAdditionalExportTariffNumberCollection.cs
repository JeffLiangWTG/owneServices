
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class DrawbackAdditionalExportTariffNumberCollection : CusCodeDataCollection<DrawbackAdditionalExportTariffNumber>
	{
		public DrawbackAdditionalExportTariffNumberCollection(BusinessObject bizObj)
			: base(bizObj, CusCodeDataTypeList.Codes.DrawbackAdditionalExportTariffNumber)
		{
		}
	}
}
