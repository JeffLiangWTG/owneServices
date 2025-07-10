using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class OrganisationController : ZController, IOrganisationController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public OrganisationController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Organisation; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Organisation; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ZOrganisationsForm((OrgHeader)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrganisationView; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrganisationDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrganisationModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrganisationNew; }
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			return ShowForm(sourceEntity, TabPageToShow, FormAction.Edit);
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			return ShowForm(sourceEntity, TabPageToShow, FormAction.Delete);
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			return ShowForm(sourceEntity, TabPageToShow, FormAction.View);
		}

		string TabPageToShow
		{
			get
			{
				return InitialTabPageNameToSelectWhenAFormIsShown.IsEmpty ? OrganisationTabPages.Details.Name : InitialTabPageNameToSelectWhenAFormIsShown.ToString();
			}
		}

		#region IOrganisationController Members

		public IZForm ShowForm(BusinessObject bizObj, OrganisationTabPageType tabPageNameToShow, FormAction formDisplayMode)
		{
			Argument.NotNull(bizObj, "bizObj");
			return ShowForm(bizObj, tabPageNameToShow.Name, formDisplayMode);
		}

		IZForm ShowForm(BusinessObject bizObj, ZString tabPageNameToShowName, FormAction formDisplayMode)
		{
			bool isSystemOrg = SelectedObjectIsSystemDefinedOrganisation(bizObj);
			IZForm result = null;

			SetInitialTabPageNameToSelectWhenAFormIsShown(tabPageNameToShowName);

			if (!isSystemOrg)
			{
				if (formDisplayMode == FormAction.Edit)
				{
					result = base.ShowEditForm(bizObj);
				}
				else if (formDisplayMode == FormAction.Delete)
				{
					result = base.ShowDeleteForm(bizObj);
				}
			}

			if (formDisplayMode == FormAction.View || (isSystemOrg && ShouldViewInsteadOfEdit == DialogResult.Yes))
			{
				result = base.ShowViewForm(bizObj);
			}

			return result;
		}

		bool SelectedObjectIsSystemDefinedOrganisation(BusinessObject selectedBusinessObject)
		{
			OrgHeader org = selectedBusinessObject as OrgHeader;
			return org != null ? org.IsSystemDefinedOrganisation : ZBool.False;
		}

		DialogResult ShouldViewInsteadOfEdit
		{
			get { return Globals.Message.Show(CannotModifySystemDefinedOrganisationText, CannotModifySystemDefinedOrganisationCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation); }
		}

		internal static string CannotModifySystemDefinedOrganisationText
		{
			get { return Res.GetString("1f704870-36ce-4f7c-921b-30b0b7a16f80", "The selected organization is a special, system defined organization and cannot be modified. Would you like to View it instead?"); }
		}

		internal static string CannotModifySystemDefinedOrganisationCaption
		{
			get { return Res.GetString("8b605c1d-c2fa-4997-b8f3-0bf6fc3b91d0", "Cannot Modify Organization"); }
		}

		#endregion

		#region CRM Security

		readonly OrganisationCRMSecurityProvider SecurityProvider = new OrganisationCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrgHeader, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrgHeader, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrgHeader, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion

	}
}
