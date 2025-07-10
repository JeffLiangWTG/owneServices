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
	/// Module Controller for OrgCreditorGroup.
	/// </summary>
	public class OrgCreditorGroupController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public OrgCreditorGroupController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.OrgCreditorGroup; }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.OrgCreditorGroup;
			}
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgCreditorGroup); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new OrgCreditorGroupForm((OrgCreditorGroup)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CreditorGroupsModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CreditorGroupsModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CreditorGroupsModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CreditorGroups; }
		}
	}
}
