using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Module
{
	class HVLVOuterPackageController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.HVLVOuterPackage;

		public override Type TypeOfTopLevelBusinessObject => typeof(HVLVOuterPackage);

		public override ModuleIdentifier ModuleID => ModuleIDs.HVLVOuterPackage;

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("We do not have any forms for HVLV Outer Package.");

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.HVLVOuterPackage;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.HVLVOuterPackageEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.HVLVOuterPackage;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.HVLVOuterPackageView;

		#endregion
	}
}
