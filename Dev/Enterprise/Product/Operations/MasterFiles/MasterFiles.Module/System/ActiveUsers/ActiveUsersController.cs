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
	public class ActiveUsersController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ActiveUserForm((ActiveUser)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ActiveUsers; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ActiveUsers; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ActiveUser); }
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			return sourceEntity;
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new ActiveUser(Factory);
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ActiveUser; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ActiveUser; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ActiveUser; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ActiveUser; }
		}

		#endregion
	}
}
