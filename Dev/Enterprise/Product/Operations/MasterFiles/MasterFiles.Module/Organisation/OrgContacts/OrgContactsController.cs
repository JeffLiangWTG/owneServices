using System;
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
	public class OrgContactsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public OrgContactsController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.OrgContacts; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.OrgContacts; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgContact); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			SetInitialTabPageNameToSelectWhenAFormIsShown(OrganisationTabPages.Contacts.Name);
			ZOrganisationsForm form = GetFormCore(((OrgContact)businessEntity).Header);
			OrgContact contact = (OrgContact)businessEntity;
			lastSavedPK = contact.PK;
			form.InitialContactToSelect = contact;
			return form;
		}

		protected virtual ZOrganisationsForm GetFormCore(OrgHeader org)
		{
			return new ZOrganisationsForm(org);
		}

		public override ZGuid LastSavedPK
		{
			get { return lastSavedPK; }
		}
		ZGuid lastSavedPK;

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			if (((OrgContact)businessEntity).Header != null)
			{
				return base.ShowFormForNewEntityCore(businessEntity);
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("8e0056a6-5883-4308-9ab9-c4cc85646090", "The quick contact details option is available when a valid Organization and a linked Contact are available."), Res.GetString("25fc7015-208f-42e3-96b0-05cf225af4a8", "Unable to create new contact"));
				return null;
			}
		}

		protected override void SetControllerID(IZForm form, ControllerID proposedControllerID)
		{
			// The form has its own ControllerID - we don't want to set it to OrgContacts.
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrgContactView; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrgContactModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrgContactNew; }
		}

		#region CRM Security

		readonly OrgContactsCRMSecurityProvider SecurityProvider = new OrgContactsCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrgContact, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrgContact, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as OrgContact, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
