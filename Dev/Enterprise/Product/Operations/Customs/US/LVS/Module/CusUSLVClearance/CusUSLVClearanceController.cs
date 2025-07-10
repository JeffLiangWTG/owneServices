using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.LVS.Module
{
	public class CusUSLVClearanceController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity) => new CusUSLVClearanceForm((CusUSLVClearance)businessEntity);

		public override ControllerID ID => ControllerIDs.Customs.US.USLowValueEntries;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.USLowValueEntries;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusUSLVClearance);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.USLVClearanceView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.USLVClearanceNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.USLVClearanceEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.USLVClearanceDelete;

		#endregion
	}
}
