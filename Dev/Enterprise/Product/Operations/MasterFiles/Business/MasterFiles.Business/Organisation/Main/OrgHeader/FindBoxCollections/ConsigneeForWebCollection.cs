using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	// Can be deleted once Web Orgs uses FilterStrips
	public class ConsigneeForWebCollection : ConsigneeCollection
	{
		public ConsigneeForWebCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ConsigneeForWebCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ConsigneeForWebCollection(BusinessObjectFactory factory, OrganisationDefaults defaults)
			: base(factory, defaults)
		{
		}

		public ConsigneeForWebCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults)
			: base(factory, filter, defaults)
		{
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject 
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgHeader.Schema.OH_IsConsignee, ZBool.True));
		}
	}
}
