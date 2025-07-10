using System;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing;

public class ServiceLevelFilterControlTest : BaseFilterControlTest
{
	[RequiresSTA]
	public void TestFilterGridColumnsWithCalculateDeliveryDueDateByTransportModeActive()
	{
		using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty,
				   Guid.Empty, Guid.Empty,
				   new CalculateDeliveryDueDateOptions
				   {
					   IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption()
				   }))
		using (var module = new ServiceLevelModule())
		using (var form = new ZForm())
		{
			var filterControl = (ZFilterStripControl)module.EmbeddedControl;
			var grid = filterControl.FilteredGrid;
			form.Controls.Add(filterControl);
			form.Show();
			Application.DoEvents();

			AssertColumn(grid, "RS_Code");
			AssertColumn(grid, "RS_DescriptionMultilingual");
			AssertColumn(grid, "RS_IsActive");
			AssertColumn(grid, "RS_IsDoorToDoor");
			AssertColumn(grid, "RS_IsGateway");
			AssertColumn(grid, "RS_IsSystem");
			AssertColumn(grid, "RS_ServiceDeliveryType");
			AssertColumn(grid, "ServiceDeliveryTypeDescription");
			AssertColumn(grid, "RS_ServiceDeliveryPercentage");
			AssertColumn(grid, "DefaultTransitTimeFormatted");
			AssertColumn(grid, "DefaultArrivalTime");
			AssertColumn(grid, "ServiceDeliveryDueTime");
			AssertColumn(grid, "DeliverOnWeekend");
		}
	}

	[RequiresSTA]
	public void TestFilterGridColumnsWithCalculateDeliveryDueDateByTransportModeInactive()
	{
		using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty,
				   Guid.Empty, Guid.Empty,
				   new CalculateDeliveryDueDateOptions
				   {
					   IsActive = false
				   }))
		using (var module = new ServiceLevelModule())
		using (var form = new ZForm())
		{
			var filterControl = (ZFilterStripControl)module.EmbeddedControl;
			var grid = filterControl.FilteredGrid;
			form.Controls.Add(filterControl);
			form.Show();
			Application.DoEvents();

			AssertColumn(grid, "RS_Code");
			AssertColumn(grid, "RS_DescriptionMultilingual");
			AssertColumn(grid, "RS_IsActive");
			AssertColumn(grid, "RS_IsDoorToDoor");
			AssertColumn(grid, "RS_IsGateway");
			AssertColumn(grid, "RS_IsSystem");
			AssertColumn(grid, "RS_ServiceDeliveryType");
			AssertColumn(grid, "ServiceDeliveryTypeDescription");
			AssertColumn(grid, "RS_ServiceDeliveryPercentage");
			AssertNoColumn(grid, "DefaultTransitTimeFormatted");
			AssertNoColumn(grid, "DefaultArrivalTime");
			AssertNoColumn(grid, "ServiceDeliveryDueTime");
			AssertNoColumn(grid, "DeliverOnWeekend");
		}
	}
}
