using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCompanyDataCollection : BusinessObjectCollection<OrgCompanyData>
	{
		public OrgCompanyDataCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgCompanyDataCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
