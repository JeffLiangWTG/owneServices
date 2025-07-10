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
	public class ServiceLevelController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ServiceLevel; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ServiceLevel; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefServiceLevel); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefServiceLevelForm((RefServiceLevel)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ServiceLevelsModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ServiceLevelsModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ServiceLevelsModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ServiceLevels; }
		}
	}
}
