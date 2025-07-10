using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class OrgCusCodeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.OrgCusCode; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.OrgCusCode; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgCusCode); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			SetInitialTabPageNameToSelectWhenAFormIsShown(OrganisationTabPages.Details.Name);
			return new ZOrganisationsForm(((OrgCusCode)businessEntity).Header);
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			if (((OrgCusCode)businessEntity).Header != null)
			{
				return base.ShowFormForNewEntityCore(businessEntity);
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("70f8ffa3-4062-42a3-9e20-598bc275dc78", "Please create a new Customs Registration Number by editing the organization that the contact belongs to."), Res.GetString("0e9af039-f6a9-4c8d-9075-f90dc441aaea", "Unable to create new Customs Registration Number"));
				return null;
			}
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Organisation; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrganisationModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrganisationModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrganisationModify; }
		}
	}
}
