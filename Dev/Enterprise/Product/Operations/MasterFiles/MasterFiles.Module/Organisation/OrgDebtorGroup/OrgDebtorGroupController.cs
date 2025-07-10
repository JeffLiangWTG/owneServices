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
	/// <summary>
	/// Module Controller for OrgDebtorGroup.
	/// </summary>
	public class OrgDebtorGroupController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public OrgDebtorGroupController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.OrgDebtorGroup;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.OrgDebtorGroup; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgDebtorGroup); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new OrgDebtorGroupForm((OrgDebtorGroup)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.DebtorGroupsModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.DebtorGroupsModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.DebtorGroupsModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.DebtorGroups; }
		}
	}
}
