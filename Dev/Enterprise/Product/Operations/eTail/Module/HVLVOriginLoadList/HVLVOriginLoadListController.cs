using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Module
{
	class HVLVOriginLoadListController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.HVLVOriginLoadList;

		public override Type TypeOfTopLevelBusinessObject => typeof(HVLVOriginLoadList);

		public override ModuleIdentifier ModuleID => ModuleIDs.HVLVOriginLoadList;

		protected override IZForm GetForm(IBusiness businessEntity) => new HVLVOriginLoadListForm((HVLVOriginLoadList)businessEntity);

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.HVLVOriginLoadList;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.HVLVOriginLoadListEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.HVLVOriginLoadList;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.HVLVOriginLoadListView;

		#endregion
	}
}
