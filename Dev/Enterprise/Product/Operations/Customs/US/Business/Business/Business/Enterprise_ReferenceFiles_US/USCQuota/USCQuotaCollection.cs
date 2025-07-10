using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USCQuotaCollection : BusinessObjectCollection<USCQuota>
	{
		public USCQuotaCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
