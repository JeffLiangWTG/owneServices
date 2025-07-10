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
	public class AccGroupsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AccGroupsController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.AccGroups; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccGroups; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccGroups); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccGroupsForm((AccGroups)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AccountingGroupsModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AccountingGroupsModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AccountingGroupsModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AccountingGroups; }
		}
	}
}
