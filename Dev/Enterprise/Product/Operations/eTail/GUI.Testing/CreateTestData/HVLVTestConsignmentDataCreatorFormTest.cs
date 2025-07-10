using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing;

[TestedType(typeof(HVLVTestConsignmentDataCreatorForm))]
public class HVLVTestConsignmentDataCreatorFormTest : ZFormBasherTest
{
	public void TestConsignmentDataCreator_WhenEmptyConsignment_DeleteExistingConsignmentIsDisabled()
	{
		var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
		shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
		Factory.Save();

		using (var hVLVTestConsignmentDataCreatorForm = new HVLVTestConsignmentDataCreatorForm(shipment.JS_UniqueConsignRef, hasConsignment: false))
		{
			var isChecked = hVLVTestConsignmentDataCreatorForm.Controls.Find("checkBoxRemoveAllExistingConsignments", true)[0] as ZCheckBox;
			AssertEquals("Check whether checkbox 'removeAllExistingConsignments' is disabled ", true, isChecked.Enabled);
		}
	}

	protected override Form GetFormToBashCore()
	{
		return new HVLVTestConsignmentDataCreatorForm();
	}
}
