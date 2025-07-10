using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefTransitTimeFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestFilterGridColumns()
		{
			using (var module = new RefTransitTimeModule())
			using (var form = new ZForm())
			{
				var grid = SetupAndAssertColumns(module, form, false);
				AssertColumn(grid, "TransitTimeFormatted");
			}

			using (var module = new RefTransitTimeModule())
			using (var form = new ZForm())
			{
				SetupAndAssertColumns(module, form, true);
			}
		}

		ZDisplayGrid SetupAndAssertColumns(RefTransitTimeModule module, ZForm form, bool calculateDelivery)
		{
			ZDisplayGrid grid = null;

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = calculateDelivery, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				grid = filterControl.FilteredGrid;
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();

				AssertColumn(grid, "RTT_RS_NKServiceLevel");
				AssertColumn(grid, "OriginZone+SelectedZone+ZoneCode");
				AssertColumn(grid, "OriginZone+IsDomestic");
				AssertColumn(grid, "OriginZone+SelectedZoneRelatedOrg+OH_Code");
				AssertColumn(grid, "DestinationZone+SelectedZone+ZoneCode");
				AssertColumn(grid, "DestinationZone+IsDomestic");
				AssertColumn(grid, "DestinationZone+SelectedZoneRelatedOrg+OH_Code");
				AssertColumn(grid, "RTT_Mode");
			}

			return grid;
		}

		static void AssertColumn(ZDisplayGrid grid, string columnName)
		{
			var column = grid.Columns[columnName];
			AssertNotNull(columnName + " column should exist", column);
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
