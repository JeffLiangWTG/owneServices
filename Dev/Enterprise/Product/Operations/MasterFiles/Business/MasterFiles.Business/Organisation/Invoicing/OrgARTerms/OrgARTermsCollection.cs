using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgARTermsCollection : ActiveBusinessObjectCollection<OrgARTerms>
	{
		public OrgARTermsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgARTermsCollection(OrgCompanyData master)
			: base(master)
		{
		}
	}
}
