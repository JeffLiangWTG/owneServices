using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.eManifest.Module
{
	public class eManifestController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.US.eManifest; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.US.eManifest; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Trip); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ManifestForm((Trip)businessEntity);
		}

		protected override IZForm ShowLoadedForm(IBusiness entity, FormAction action)
		{
			return base.ShowLoadedForm(entity, action == FormAction.Edit && !((Trip)entity).BH_IsActive ? FormAction.View : action);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.USeManifestView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.USeManifestNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.USeManifestEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.USeManifestEdit; }
		}

		#endregion
	}
}
