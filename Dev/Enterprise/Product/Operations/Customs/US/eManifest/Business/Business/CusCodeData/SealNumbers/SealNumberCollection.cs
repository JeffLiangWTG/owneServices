using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class SealNumberCollection : CusCodeDataCollection<SealNumber>
	{
		public SealNumberCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.SealNumber)
		{
		}
	}
}
