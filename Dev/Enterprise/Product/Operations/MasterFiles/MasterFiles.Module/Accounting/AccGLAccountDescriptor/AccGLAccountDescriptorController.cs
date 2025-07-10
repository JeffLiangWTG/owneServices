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
	/// Summary description for AccGLAccountDescriptorController.
	/// </summary>
	public class AccGLAccountDescriptorController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AccGLAccountDescriptorController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AccGLAccountDescriptor;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AccGLAccountDescriptor; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(AccGLAccountDescriptor); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new AccGLAccountDescriptorForm((AccGLAccountDescriptor)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.GLAccountsModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.GLAccountsModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.GLAccountsModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.GLAccounts; }
		}
	}
}
