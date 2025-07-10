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
	public class RefCarrierConsortiumController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public RefCarrierConsortiumController()
		{
		}

		#region Standard Controller Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefCarrierConsortium; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefCarrierConsortium; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefCarrierConsortium); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefCarrierConsortiumForm((RefCarrierConsortium)businessEntity);
		}

		#endregion

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.VesselConsortiumModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.VesselConsortiumModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.VesselConsortiumModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.VesselConsortium; }
		}

		#endregion
	}
}
