using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	public class OrgRailCarrierModule : ZArchitecture.Web.Modules.OrganisationModule
	{
		public OrgRailCarrierModule(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

		public override Type FilterBusinessObjectType
		{
			get { return typeof(OrganisationFilterBusinessObject); }
		}

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.OrgRailCarrierTracking; }
		}

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults fFilterBusinessObjectDefaults = base.GetNewFilterBusinessObjectDefaults();
			fFilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(AutoOrganisationFilterBusinessObject.Schema.OH_IsShippingProvider, ZBool.True));
			fFilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(AutoOrganisationFilterBusinessObject.Schema.OH_IsRailProvider, ZBool.True));
			return fFilterBusinessObjectDefaults;
		}
	}
}
