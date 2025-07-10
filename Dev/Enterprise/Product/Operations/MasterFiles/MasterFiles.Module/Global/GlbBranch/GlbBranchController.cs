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
	public class GlbBranchController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GlbBranch; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbBranch; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlbBranch); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbBranchForm((GlbBranch)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.BranchDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.BranchModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.BranchNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.BranchView; }
		}

		#endregion
	}
}
