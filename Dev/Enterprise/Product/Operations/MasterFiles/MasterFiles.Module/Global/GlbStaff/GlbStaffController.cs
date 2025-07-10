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
	public class GlbStaffController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public GlbStaffController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GlbStaff; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbStaff; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlbStaff); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbStaffForm((GlbStaff)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.StaffModifyAll; }
		}

		internal SecurityCheckpoint TestCheckPointForNewInternal()
		{
			return CheckPointForNew;
		}

		#region Show Forms

		protected override bool AllowCreateNewWithoutSecurityRight
		{
			get { return GlbStaff.CurrentUser.IsCurrentUserLocalAdminForAtLeastOneGroup; }
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (sourceEntity.CanDelete)
			{
				ShowLoadedForm(sourceEntity, FormAction.Delete);
			}
			else
			{
				Env.Security.StaffModifyAll.ShowError();
				LastShownForm = null;
			}
			return LastShownForm;
		}

		#endregion

		#region Security CheckPoints

		#region CheckPointForDelete

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject sourceEntity)
		{
			if (sourceEntity == null)
			{
				return Env.Security.StaffModifyAll;
			}

			return
				sourceEntity.PK != Env.CurrentUser.PK
					? !sourceEntity.CanDelete && !string.IsNullOrEmpty(sourceEntity.ReasonForNotAbleToDelete)
						? new DeniedSecurityCheckpoint(code: Env.Security.Staff.Code, displayText: sourceEntity.ReasonForNotAbleToDelete)
						: Env.Security.StaffModifyAll
					: new DeniedSecurityCheckpoint(code: Env.Security.Staff.Code, displayText: ResString.GetMultilingualString("1a524e4d-f873-4355-b01b-ded8d03ff866", "You cannot delete a user currently logged in."));
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { throw new NotSupportedException("Access depends on both StaffModifyAll and StaffModifyOwn"); }
		}

		#endregion

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject sourceEntity)
		{
			if (sourceEntity is GlbStaff glbStaffSourceEntity)
			{
				return glbStaffSourceEntity.IsCurrentUserLocalAdminForThisStaff
					? Env.Security.StaffLocalAdministratorPlaceholder
					: Env.Security.StaffEdit;
			}

			return Env.Security.StaffEdit;
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { throw new NotSupportedException("Access depends on both StaffEdit and StaffLocalAdministratorPlaceholder"); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.StaffView; }
		}

		internal SecurityCheckpoint TestCheckPointForViewInternal()
		{
			return CheckPointForView;
		}

		#endregion
	}
}
