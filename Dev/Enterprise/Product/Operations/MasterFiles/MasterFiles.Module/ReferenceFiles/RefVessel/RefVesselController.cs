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
	/// Module Controller for RefVessel.
	/// </summary>
	public class RefVesselController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public RefVesselController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.RefVessel;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefVessel; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefVessel); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefVesselForm((RefVessel)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.VesselsModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.VesselsModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.VesselsModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Vessels; }
		}
	}
}
