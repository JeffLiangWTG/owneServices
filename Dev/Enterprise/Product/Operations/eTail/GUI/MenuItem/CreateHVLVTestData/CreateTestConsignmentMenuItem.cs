using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI;

public class CreateTestConsignmentMenuItem : BaseHVLVMenuItem
{
	public CreateTestConsignmentMenuItem(ForwardingShipment shipment)
		: base(ResString.GetMultilingualString("68f67cd0-51df-4e56-a71b-3ffaf32d53e2", "Create Test Consignment"), shipment)
	{
	}

	protected override Action MenuAction => () =>
	{
		var saveBeforeCreatingTestConsignmentsMessage = Res.GetString("fe41f3bb-b9cd-411f-a533-f8aa0219c7bd", "Please save the form before creating test consignments");
		if (UserPromptCheckingHelper.CheckBusinessObjectHasNoChangesOrNotify(shipment, saveBeforeCreatingTestConsignmentsMessage))
		{
			var creatorForm = new HVLVTestConsignmentDataCreatorForm(shipment.JS_UniqueConsignRef, hasConsignment: shipment.HVLVConsignments.Any());
			var dialogResult = ZFormModaliser.ShowDialogAndDispose(creatorForm);
			if (dialogResult == DialogResult.OK)
			{
				var controller = ZControllerFactory.Create(Form.ControllerID);
				Form.Close();
				controller.ShowEditForm(shipment);
			}
		}
	};

	public override void UpdateVisibilityAndCaption()
	{
		Visible = ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Test && GlbStaff.CurrentUser.IsSupportUser;
	}
}
