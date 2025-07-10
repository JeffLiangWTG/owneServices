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
	/// Module Controller for StmFeatureTest.
	/// </summary>
	public class StmFeatureTestController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public StmFeatureTestController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.StmFeatureTest; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(StmFeatureTest); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new StmFeatureTestForm((StmFeatureTest)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.SystemFeatureTest; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.SystemFeatureTest; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.SystemFeatureTest; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.SystemFeatureTest; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.StmFeatureTest; }
		}
	}
}
