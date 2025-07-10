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
	public class CusPersonController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CusPerson; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CusPerson; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlbPerson); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusPersonForm(businessEntity as GlbPerson);
		}
		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}
		#endregion

		protected override string AlreadyDeletedOrIrreversiblyChangedMessageCore
		{
			get { return Res.GetString("GlbPersonController.AlreadyDeletedOrIrreversiblyChangedMessageCore", "This Person is not in the database, most likely because the new record has not yet been saved. To use F3 to edit new records you must first save."); }
			// Stupid F3 edit uses a different factory, so will never see new records, lame. 
		}
	}
}
