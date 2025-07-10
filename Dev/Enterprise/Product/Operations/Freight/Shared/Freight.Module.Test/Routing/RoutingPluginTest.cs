using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.GUI;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module.Testing
{
	sealed class RoutingPluginTest : BaseFreightTest
	{
		public void TestSetCorrectSchedulUpdateQueryProvider()
		{
			IRoutingSupport support = Factory.New<CommonShipment>();

			AssertEquals("ScheduleUpdateNullQueryProvider", ScheduleUpdateQueryProviderFactory.Get(Factory).GetType().Name);

			using (RoutingPlugin plugin = new RoutingPlugin(support))
			{
				AssertType(typeof(ScheduleUpdateGuiQueryProvider), ScheduleUpdateQueryProviderFactory.Get(Factory));
			}
		}

		public void TestSetCorrectSchedulUpdateQueryProvider_TransportBooking()
		{
			var support = (ITransportParentCore)Factory.New<IDtbBookingConsolidation>();

			AssertEquals("ScheduleUpdateNullQueryProvider", ScheduleUpdateQueryProviderFactory.Get(Factory).GetType().Name);

			using (RoutingPlugin plugin = new RoutingPlugin(support))
			{
				AssertType(typeof(ScheduleUpdateGuiQueryProvider), ScheduleUpdateQueryProviderFactory.Get(Factory));
			}
		}

		public void TestEnablePluginBehaviour()
		{
			IRoutingSupport support = Factory.New<CommonShipment>();

			AssertEquals("plugin behaviour should not be enabled without the plugin", false, support.TransportsIncludingRelated.PluginBehaviourEnabled);

			using (RoutingPlugin plugin = new RoutingPlugin(support))
			{
				AssertEquals("plugin behaviour should be enabled by the plugin", true, ((RoutingCollection)plugin.BusinessEntity).PluginBehaviourEnabled);
				AssertEquals("plugin behaviour should be enabled by the plugin", true, support.TransportsIncludingRelated.PluginBehaviourEnabled);
			}
		}

		public void TestSetCorrectQueryProviderOverride()
		{
			IRoutingSupport support = Factory.New<CommonShipment>();

			AssertEquals("precondition: ", "DefaultSailingManagerQueryProvider", SailingManagerQueryProviderFactory.Get(Factory).GetType().Name);

			using (RoutingPlugin plugin = new RoutingPlugin(support))
			{
				AssertType(typeof(UserSailingManagerQueryProvider), SailingManagerQueryProviderFactory.Get(Factory));
			}
		}

		public void TestDontLoadTransportsEarly()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			Transport transport = shipment.Transports.AddNew();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonShipment shipmentInNewFactory = Factory.Load<CommonShipment>(shipment.PK);

			AssertEquals("precondition: should not have hit the collection yet", false, RoutingCollectionLoaded(shipment));

			using (RoutingPlugin plugin = new RoutingPlugin(shipmentInNewFactory))
			{
				AssertEquals("should not have hit the collection yet", false, RoutingCollectionLoaded(shipment));
				AssertNotNull("Hit property", plugin.BusinessEntity);
				AssertEquals("should now have hit the collection", true, RoutingCollectionLoaded(shipment));
			}
		}

		public void TestGetBusinessEntityForPlugin()
		{
			IRoutingSupport support = Factory.New<CommonConsol>();
			using (var plugin = new RoutingPlugin(support))
			{
				AssertEquals(support.TransportsIncludingRelated, plugin.BusinessEntity);
				AssertEquals(typeof(RoutingCollection), plugin.BusinessEntity.GetType());
			}

			var coreSupport = (ITransportParentCore)Factory.New<IDtbBookingConsolidation>();
			using (var routingPlugin = new RoutingPlugin(coreSupport))
			{
				AssertEquals(typeof(TransportCollection), routingPlugin.BusinessEntity.GetType());
			}
		}

		public void TestGetBusinessEntityForPlugin_RequiresLoad()
		{
			var coreSupport = (ITransportParentCore)Factory.New<IDtbBookingConsolidation>();
			using (var routingPlugin = new RoutingPlugin(coreSupport))
			{
				AssertEquals(typeof(TransportCollection), routingPlugin.BusinessEntity.GetType());

				var transports = (TransportCollection)routingPlugin.BusinessEntity;
				transports.AddNew();
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			var coreSupport_NewFactory = (ITransportParentCore)Factory.Load<IDtbBookingConsolidation>(coreSupport.PK);
			using (var routingPlugin = new RoutingPlugin(coreSupport_NewFactory))
			{
				AssertEquals(typeof(TransportCollection), routingPlugin.BusinessEntity.GetType());

				var transports = (TransportCollection)routingPlugin.BusinessEntity;
				AssertEquals(1, transports.Count);
			}
		}

		public void TestUserControl()
		{
			IRoutingSupport support = Factory.New<CommonConsol>();
			using (var plugin = new RoutingPlugin(support))
			{
				AssertEquals(true, plugin.Enabled);
				AssertNotNull(plugin.UserControl);
				AssertEquals(typeof(RoutingPluginControl), plugin.UserControl.GetType());
			}

			var coreSupport = (ITransportParentCore)Factory.New<IDtbBookingConsolidation>();
			using (var plugin = new RoutingPlugin(coreSupport))
			{
				AssertEquals(true, plugin.Enabled);
				AssertNotNull(plugin.UserControl);
				AssertEquals(typeof(RoutingPluginControl), plugin.UserControl.GetType());
			}
		}

		public void TestDisableAllControls()
		{
			IRoutingSupport support = Factory.New<CommonConsol>();
			using (var plugin = new RoutingPlugin(support))
			{
				var routingControl = (RoutingPluginControl)plugin.UserControl;
				AssertEquals("Precondition: RoutingPluginControl property RoutingCollectionAllowNew is true", true, routingControl.RoutingCollectionAllowNew);

				plugin.DisableAllControls();

				CombineAssertions(() =>
				{
					AssertEquals("Should set RoutingPluginControl property RoutingCollectionAllowNew to false", false, routingControl.RoutingCollectionAllowNew);
					AssertCorrectControlsAreDisabled(plugin.UserControl);

					CheckButtonWithMessage(routingControl, "SelectScheduleButton", "Error You can only view Schedules via the parent");
					CheckButtonWithMessage(routingControl, "GlobalSchedulesButton", "Error You can only import Global Flight Schedules via the parent");
					CheckButtonWithMessage(routingControl, "ImportGlobalScheduleButton", "Error You can only import Global Sailing Schedules via the parent");
				});
			}
		}

		public void AssertCorrectControlsAreDisabled(Control control)
		{
			var controlsToExclude = new List<string>() { "SelectScheduleButton", "GlobalSchedulesButton", "ImportGlobalScheduleButton" };

			if (control is ZGrid grid)
			{
				var notReadOnlyColumnStyles = Enumerable.Where(grid.ColumnStyles.ToArray(), x => !((ZGridColumnInfo)x).IsReadOnly);
				AssertContainsExactElementsInAnyOrder("There should be no ColumnStyles where ReadOnly equals false.", Enumerable.Empty<ZGridColumnInfo>(), notReadOnlyColumnStyles);
				AssertEquals(grid.Name + " should be enabled", true, grid.Enabled);
			}
			else if ((control is ZCodeFindBox || control.Controls.Count == 0) && !controlsToExclude.Contains(control.Name))
			{
				AssertEquals(control.Name + " should be disabled.", false, control.Enabled);
			}
			else
			{
				AssertEquals(control.Name + " should be enabled.", true, control.Enabled);
			}

			foreach (Control subControl in control.Controls)
			{
				AssertCorrectControlsAreDisabled(subControl);
			}
		}

		static void CheckButtonWithMessage(Control routing, string buttonName, string message)
		{
			var button = (Button)routing.Controls.Find(buttonName, true).First();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			button.PerformClick();
			AssertEquals("Reason", message, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		#region Implementation

		bool RoutingCollectionLoaded(CommonShipment shipment)
		{
			FieldInfo field = typeof(CommonShipment).GetField("transportsIncludingRelated", BindingFlags.NonPublic | BindingFlags.Instance);
			return field.GetValue(shipment) != null;
		}

		#endregion
	}
}
