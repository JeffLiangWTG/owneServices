using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class OrgOpportunityController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		#region Standard Controller Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Opportunity; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Opportunity; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgOpportunity); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new OpportunityForm((OrgOpportunity)businessEntity);
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OpportunityManagementNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OpportunityManagementView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OpportunityManagementEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OpportunityManagementDelete; }
		}

		#endregion

		#region CRM Security

		readonly OrgOpportunityCRMSecurityProvider SecurityProvider = new OrgOpportunityCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrgOpportunity, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrgOpportunity, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrgOpportunity, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
