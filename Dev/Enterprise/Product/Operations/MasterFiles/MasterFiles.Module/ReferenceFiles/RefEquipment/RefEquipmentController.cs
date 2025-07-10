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
	public class RefEquipmentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public RefEquipmentController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.RefEquipment;
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefEquipment; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefEquipment); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefEquipmentForm((RefEquipment)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.EquipmentModify; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.EquipmentModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.EquipmentModify; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Equipment; }
		}
	}
}
