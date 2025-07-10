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
	public class SalesEnquiryController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public SalesEnquiryController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.SalesEnquiry; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(SalesEnquiry); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new SalesEnquiryForm((SalesEnquiry)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.SalesEnquiry; }
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.InquiryManagerView; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.InquiryManagerDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.InquiryManagerEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.InquiryManagerNew; }
		}

		#endregion

		#region CRM Security

		readonly SalesEnquiryCRMSecurityProvider SecurityProvider = new SalesEnquiryCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as SalesEnquiry, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as SalesEnquiry, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as SalesEnquiry, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
