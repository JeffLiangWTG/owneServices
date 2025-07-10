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
	public class HVLVConsignmentController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.HVLVConsignment;

		public override Type TypeOfTopLevelBusinessObject => typeof(HVLVConsignment);

		protected override IZForm GetForm(IBusiness businessEntity) => new HVLVConsignmentForm((HVLVConsignment)businessEntity);

		public override ModuleIdentifier ModuleID => ModuleIDs.HVLVConsignment;

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.HVLVBookingHeader;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.HVLVBookingHeaderEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.HVLVBookingHeader;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.HVLVBookingHeaderView;

		#endregion
	}
}
