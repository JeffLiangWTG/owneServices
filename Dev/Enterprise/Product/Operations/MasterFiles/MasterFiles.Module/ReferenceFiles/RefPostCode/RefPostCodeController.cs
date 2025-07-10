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
	public class RefPostCodeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.PostCodeDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.PostCodeModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.PostCodeNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.PostCodeView; }
		}

		#endregion

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefPostCodeForm((RefPostCode)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefPostCode; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefPostCode; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefPostCode); }
		}
	}
}
