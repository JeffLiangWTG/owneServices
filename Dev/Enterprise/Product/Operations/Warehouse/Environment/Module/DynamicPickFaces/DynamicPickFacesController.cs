using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	[ControllerDoesNotSupportForm]
	public class DynamicPickFacesController : WhsControllerBase
	{
		public override ControllerID ID => ControllerIDs.WhsConfigDynamicPickFaces;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsConfigDynamicPickFaces;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsDynamicPickFaceView);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsConfigDynamicPickFacesView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsConfigDynamicPickFacesEdit;

		protected override SecurityCheckpoint CheckPointForNew => throw new NotSupportedException();

		protected override SecurityCheckpoint CheckPointForDelete => throw new NotSupportedException();

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			OrgSupplierPartForm form = null;

			var wdpViewBO = businessEntity as WhsDynamicPickFaceView;
			if (!wdpViewBO.WDP_OP_Product.IsEmpty)
			{
				form = new OrgSupplierPartForm(Factory.Load<OrgSupplierPart>(wdpViewBO.WDP_OP_Product));
				SetInitialTabPageNameToSelectWhenAFormIsShown(form.GetWarehouseTabPageName());
			}
			else
			{
				Globals.Message.Show(Res.GetString("D4553DF1-915A-475B-8F6E-C30353BFC392", "Use the product module to assign products to this dynamic pick face."));
			}

			return form;
		}

		protected override void SetControllerID(IZForm form, ControllerID proposedControllerID)
		{
			// This is called when showing a new form. e.g. View Product
			// The base method sets the form's ControllerID to WhsDynamicPickFaceView
			// We do not want that. Leave it as is
		}
	}
}
