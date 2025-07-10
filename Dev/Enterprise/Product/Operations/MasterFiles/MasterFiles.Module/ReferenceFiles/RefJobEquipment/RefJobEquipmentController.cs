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
	public class RefJobEquipmentController : ZController
	{
		public override ControllerID ID => ControllerIDs.RefJobEquipment;

		public override ModuleIdentifier ModuleID => ModuleIDs.RefJobEquipment;

		public override Type TypeOfTopLevelBusinessObject => typeof(JobEquipment);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.RefJobEquipmentView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.RefJobEquipmentNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.RefJobEquipmentEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.RefJobEquipmentDelete;

		protected override IZForm GetForm(IBusiness businessEntity) => new RefJobEquipmentForm((JobEquipment)businessEntity);
	}
}
