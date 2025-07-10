using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	class OrgContactsStmALogController : ZController
	{
		public override ControllerID ID => ControllerIDs.OrgContactsStmALog;

		public override ModuleIdentifier ModuleID => ModuleIDs.OrgContactsStmALog;

		public override Type TypeOfTopLevelBusinessObject => typeof(StmALog);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

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

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			return null;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			return null;
		}

		public override IZForm ShowNewForm()
		{
			return null;
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			return null;
		}
	}
}
