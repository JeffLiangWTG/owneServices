using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class C4CodeCollection : CusCodeDataCollection<C4Code>
	{
		public C4CodeCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.C4Code)
		{
		}
	}
}
