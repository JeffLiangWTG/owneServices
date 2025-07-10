using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefTransitTimeForm))]
	sealed class RefTransitTimeFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new RefTransitTimeForm(Factory.New<RefTransitTime>());
		}

		protected override bool AllowFormSizeFixed => true;

		public void TestRefTransitTimeGridColumns()
		{
			var refTransitTime = Factory.NewWithValidTestData<RefTransitTime>();
			Factory.Save();

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			using (var refTransitTimeForm = new RefTransitTimeForm(refTransitTime))
			{
				var grid = refTransitTimeForm.Controls.Find("RefTransitTimeDetailsGrid", true)[0] as ZGrid;
				var columns = grid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();

				AssertContainsExactElementsInExactOrder("grid Column Names",
				new[] { "DayOfWeek",
						"TransitDays",
						"TransitHours",
						"RTD_ArrivalTime",
						"RTD_EffectiveDate",
						"RTD_EndDate"
				}, columns.Select(x => x.ColumnName));
			}
		}

		[RequiresSTA]
		public void TestRefTransitTimeDDDPanelVisibility()
		{
			var refTransitTime = Factory.NewWithValidTestData<RefTransitTime>();
			Factory.Save();

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			using (var refTransitTimeForm = new RefTransitTimeForm(refTransitTime))
			{
				var panel = refTransitTimeForm.Controls.Find("panel1", true)[0] as KPanel;
				AssertNotNull(panel);

				var transitTimeBox = refTransitTimeForm.Controls.Find("transitTimeGroupBox", true);
				AssertEquals(transitTimeBox.Length == 0, true);
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false }))
			using (var refTransitTimeForm = new RefTransitTimeForm(refTransitTime))
			{
				var panel = refTransitTimeForm.Controls.Find("panel1", true);
				AssertEquals(panel.Length == 0, true);

				var transitTimeBox = refTransitTimeForm.Controls.Find("transitTimeGroupBox", true)[0] as ZGroupBox;
				AssertNotNull(transitTimeBox);
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
