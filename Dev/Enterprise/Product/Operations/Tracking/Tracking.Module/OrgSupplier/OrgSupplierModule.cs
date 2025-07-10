using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Module
{
	public class OrgSupplierModule : OrganisationModule
	{
		public OrgSupplierModule(BusinessObjectFactory factory, ZPage page) : base(factory, page)
		{
		}

		public override Type FilterBusinessObjectType => typeof(OrgSupplierFilterBusinessObject);
	}
}
