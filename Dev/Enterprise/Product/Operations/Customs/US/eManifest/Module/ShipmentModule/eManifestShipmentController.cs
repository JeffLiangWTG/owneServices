using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.eManifest.Module
{
	public class eManifestShipmentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.US.eManifestShipment; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.US.eManifestShipment; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Shipment); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
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
