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
	public class OrgMatchApprovalController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.OrgMatchApproval; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.OrgMatchApproval; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgMatchApproval); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new OrgMatchApprovalForm((OrgMatchApproval)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrgMatchApproval; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.OrgMatchApproval; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrgMatchApproval; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrgMatchApproval; }
		}
	}
}
