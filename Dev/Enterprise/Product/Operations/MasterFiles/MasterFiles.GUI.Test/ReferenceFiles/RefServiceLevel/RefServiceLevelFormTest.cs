using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefServiceLevelForm))]
	sealed class RefServiceLevelFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefServiceLevelForm(Factory.New<RefServiceLevel>());
		}

		protected override bool AllowFormSizeFixed => true;

		[RequiresSTA]
		public void TestRefServiceLevelDDDPanel()
		{
			var refServiceLevel = Factory.NewWithValidTestData<RefServiceLevel>();
			Factory.Save();

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			using (var refServiceLevelForm = new RefServiceLevelForm(refServiceLevel))
			{
				var panel = refServiceLevelForm.Controls.Find("DDDPanel", true)[0] as KPanel;
				var firstLevelChildNames = panel.Controls
					.OfType<Control>()
					.Where(control => control.Parent == panel)
					.Select(control => control.Name)
					.ToList();

				AssertContainsExactElementsInExactOrder("new params",
				new[] { "transitTimeGroupBox",
						"RS_DefaultArrivalTime",
						"RS_DefaultDeliveryDueTime",
						"deliverOnWeekendGroupBox"
				}, firstLevelChildNames.ToArray());

				var transitTimeParamNames = panel.Controls
					.OfType<Control>()
					.FirstOrDefault(control => control.Parent == panel && control.Name == "transitTimeGroupBox")
					.Controls.OfType<Control>()
					.Select(control => control.Name)
					.ToList();

				AssertContainsExactElementsInExactOrder("new params",
				new[] { "transitDaysCalcEdit",
						"transitHoursCalcEdit",
				}, transitTimeParamNames.ToArray());

				var deliverOnWeekendControlNames = panel.Controls
					.OfType<Control>()
					.FirstOrDefault(control => control.Parent == panel && control.Name == "deliverOnWeekendGroupBox")
					.Controls.OfType<Control>()
					.Select(control => control.Name)
					.ToList();

				AssertContainsExactElementsInExactOrder("new params",
					new[] { "isDeliverOnSaturday",
						"isDeliverOnSunday",
					}, deliverOnWeekendControlNames.ToArray());
			}
		}

		[RequiresSTA]
		public void TestRefServiceLevelDDDPanelVisibility()
		{
			var refServiceLevel = Factory.NewWithValidTestData<RefServiceLevel>();
			Factory.Save();

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			using (var refServiceLevelForm = new RefServiceLevelForm(refServiceLevel))
			{
				var panel = refServiceLevelForm.Controls.Find("DDDPanel", true)[0] as KPanel;
				AssertNotNull(panel);
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false }))
			using (var refServiceLevelForm = new RefServiceLevelForm(refServiceLevel))
			{
				var panel = refServiceLevelForm.Controls.Find("DDDPanel", true);
				AssertEquals(panel.Length == 0, true);
			}
		}

		public void TestRefServiceLevelIs_DeliverOnSundayReadOnlyTrueIfIs_DeliverOnSaturdayIsChecked()
		{
			var refServiceLevel = Factory.NewWithValidTestData<RefServiceLevel>();
			Factory.Save();

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			using (var refServiceLevelForm = new RefServiceLevelForm(refServiceLevel))
			{
				var checkBoxDeliverOnSunday = refServiceLevelForm.Controls.Find("isDeliverOnSunday", true)[0] as ZCheckBox;
				var checkBoxDeliverOnSaturday = refServiceLevelForm.Controls.Find("isDeliverOnSaturday", true)[0] as ZCheckBox;

				AssertEquals(checkBoxDeliverOnSunday.ReadOnly, true);
				checkBoxDeliverOnSaturday.Checked = true;

				AssertEquals(checkBoxDeliverOnSunday.ReadOnly, false);
				checkBoxDeliverOnSunday.Checked = true;
				checkBoxDeliverOnSaturday.Checked = false;
				AssertEquals(checkBoxDeliverOnSunday.ReadOnly, true);
				AssertEquals(checkBoxDeliverOnSunday.ReadOnly, true);
			}
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}
	}
}
