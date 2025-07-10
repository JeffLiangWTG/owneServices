using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.GUI.PickFaces;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	[ControllerDoesNotSupportForm]
	public class PickFacesController : WhsControllerBase
	{
		public override ControllerID ID => ControllerIDs.WhsConfigPickFaces;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsConfigPickFaces;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsPickFaceView);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsConfigPickFacesView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsConfigPickFacesEdit;

		protected override SecurityCheckpoint CheckPointForNew => throw new NotSupportedException();

		protected override SecurityCheckpoint CheckPointForDelete => throw new NotSupportedException();

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			PickFaceForm form = null;
			var pickFaceViewBO = businessEntity as WhsPickFaceView;

			if (pickFaceViewBO == null || pickFaceViewBO.WPV_OP.IsEmpty)
			{
				Globals.Message.Show(Res.GetString("PickFaceForm|0d473482-15a6-4048-89a2-b890a42a7bfe",
					"Use the product module to assign products to this pick face."));
			}
			else
			{
				var pickFaceBizO = Factory.Load<WhsPickFace>(pickFaceViewBO.PK);
				form = new PickFaceForm(pickFaceBizO);
			}

			return form;
		}

		protected override void SetControllerID(IZForm form, ControllerID proposedControllerID)
		{
			// The form has it's own ControllerID - we don't want to set it to WhsPickFaceView.
		}
	}
}
