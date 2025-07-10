using System;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI;

public class CreateTestHVLShipmentMenuItem : ZMenuItem
{
	public CreateTestHVLShipmentMenuItem(BusinessObject parentBizo)
		: base(ResString.GetMultilingualString("d859469e-78db-44b4-abb7-3058fdb1f32e", "Create Test HVL Shipment(s)"))
	{
		this.parentBizo = parentBizo;
	}

	readonly BusinessObject parentBizo;

	protected override void OnClick(EventArgs e)
	{
		base.OnClick(e);

		var saveBeforeCreatingTestConsignmentsMessage = Res.GetString("fe41f3bb-b9cd-411f-a533-f8aa0219c7bd", "Please save the form before creating test consignments");
		if (UserPromptCheckingHelper.CheckBusinessObjectHasNoChangesOrNotify(parentBizo, saveBeforeCreatingTestConsignmentsMessage))
		{
			var creatorForm = new HVLVTestShipmentDataCreatorForm(new HVLVTestDataConfiguration(parentBizo));
			var dialogResult = ZFormModaliser.ShowDialogAndDispose(creatorForm);
			if(dialogResult == System.Windows.Forms.DialogResult.OK)
			{
				Globals.Message.ShowInformation(Res.GetString("996f3da2-cdb1-47a8-ac8f-4533331e6602", "Test HVL Shipment(s) created successfully"));
			}
			else
			{
				var addedShipments = parentBizo.Factory.GetAddedBusinessObjects<ForwardingShipment>();
				addedShipments.DeleteAll();
				parentBizo.HasChanges = false;
			}
		}
	}
}
