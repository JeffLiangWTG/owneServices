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
	/// Module Controller for AccGLHeader.
	/// </summary>
	public class AccGLHeaderController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AccGLHeaderController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AccGLHeader;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccGLHeader; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccGLHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccGLHeaderForm((AccGLHeader)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get
			{
				return Env.Security.GLAccountsModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get
			{
				return Env.Security.GLAccountsModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get
			{
				return Env.Security.GLAccountsModify;
			}
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get
			{
				return Env.Security.GLAccounts;
			}
		}
	}
}
