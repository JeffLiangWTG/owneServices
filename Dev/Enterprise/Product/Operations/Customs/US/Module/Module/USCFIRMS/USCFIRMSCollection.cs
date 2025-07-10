using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.Module
{
	sealed class USCFIRMSCollection : BusinessObjectCollection<USCFIRMS>
	{
		public USCFIRMSCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
