using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class CPSCReportCollection : DependentCusAddInfoCollection<CPSCReport, BusinessObject>
	{
		public CPSCReportCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USCPSCReport)
		{
		}
	}
}
