using System;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class TransitTimeServiceLevelCombinationFilterControlTest : BaseFilterControlTest
	{
		[RequiresSTA]
		public void TestFilterGridColumns()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			using (var module = new TransitTimeServiceLevelCombinationModule())
			using (var form = new ZForm())
			{
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				var grid = filterControl.FilteredGrid;
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();

				AssertColumn(grid, "TSC_Code");
				AssertColumn(grid, "ServiceLevelDescription");
				AssertColumn(grid, "OriginZoneCode");
				AssertColumn(grid, "OriginZoneOwner");
				AssertColumn(grid, "DestinationZoneCode");
				AssertColumn(grid, "DestinationZoneOwner");
				AssertColumn(grid, "TransitTimeFormatted");
				AssertColumn(grid, "TSC_Mode");
				AssertNoColumn(grid, "OriginZone+IsDomestic");
				AssertNoColumn(grid, "DestinationZone+IsDomestic");
			}
		}
	}
}
