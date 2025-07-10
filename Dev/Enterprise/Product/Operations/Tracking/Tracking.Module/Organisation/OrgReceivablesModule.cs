using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	public class OrgReceivablesModule : OrganisationModule
	{
		public OrgReceivablesModule(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

		public override Type FilterBusinessObjectType => typeof(OrganisationFilterBusinessObject);

		public override ModuleIdentifier ID => WebModuleIDs.OrgReceivablesTracking;

		protected override FilterBusinessObjectDefaults GetNewFilterBusinessObjectDefaults()
		{
			var fFilterBusinessObjectDefaults = base.GetNewFilterBusinessObjectDefaults();
			fFilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(AutoOrganisationFilterBusinessObject.Schema.OH_IsReceivable, ZBool.True));

			return fFilterBusinessObjectDefaults;
		}
	}
}
