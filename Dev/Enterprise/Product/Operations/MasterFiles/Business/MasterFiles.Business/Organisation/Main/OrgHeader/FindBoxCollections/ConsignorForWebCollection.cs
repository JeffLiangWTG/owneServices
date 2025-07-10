using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	// Can be deleted once Web Orgs uses FilterStrips
	public class ConsignorForWebCollection : ConsignorCollection
	{
		public ConsignorForWebCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ConsignorForWebCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ConsignorForWebCollection(BusinessObjectFactory factory, OrganisationDefaults defaults)
			: base(factory, defaults)
		{
		}

		public ConsignorForWebCollection(BusinessObjectFactory factory, ZQuery filter, OrganisationDefaults defaults)
			: base(factory, filter, defaults)
		{
		}

		protected override void SetFilterBusinessObjectDefaults()
		{
			//TestCase in OrganisationFilterBusinessObject 
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgHeader.Schema.OH_IsConsignor, ZBool.True));
		}
	}
}
