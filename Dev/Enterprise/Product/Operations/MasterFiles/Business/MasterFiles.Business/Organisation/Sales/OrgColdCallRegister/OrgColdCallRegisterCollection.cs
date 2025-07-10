using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Obsoleted by SalesEnquiryCollection. Do not use.
	/// Exists only because of the auto-generated code in AutoOrgOpportunityLookups.Enquiries.
	/// </summary>
	public class OrgColdCallRegisterCollection : BusinessObjectCollection<SalesEnquiry>
	{
		public OrgColdCallRegisterCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgColdCallRegisterCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
