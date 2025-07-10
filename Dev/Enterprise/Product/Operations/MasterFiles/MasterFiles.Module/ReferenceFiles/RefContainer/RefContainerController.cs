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
	public class RefContainerController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefContainer; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefContainer; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefContainer); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefContainerForm((RefContainer)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ContainersModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ContainersModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ContainersModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Containers; }
		}
	}
}
