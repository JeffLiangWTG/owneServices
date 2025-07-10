using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class HarmonizedNumberCollection : CusCodeDataCollection<HarmonizedNumber>
	{
		public HarmonizedNumberCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.HarmonizedNumber)
		{
		}
	}
}
