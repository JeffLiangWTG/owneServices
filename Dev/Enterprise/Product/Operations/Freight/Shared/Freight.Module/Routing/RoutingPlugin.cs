using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration.Routing;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Module
{
	sealed class RoutingPlugin : ZPlugIn, IRoutingPlugin
	{
		/// <summary>
		/// Used by Forwarding / Customs / Agency
		/// </summary>
		public RoutingPlugin(IRoutingSupport routingSupport)
			: base((IBusiness)routingSupport)
		{
			ScheduleUpdateGuiQueryProvider.Set(routingSupport.Factory, routingSupport.AdditionalETDUpdateMsg, routingSupport.AdditionalETAUpdateMsg);
			UserSailingManagerQueryProvider.Register(routingSupport.Factory);

			this.routingSupport = routingSupport;
		}

		// Used by Transport Booking 
		public RoutingPlugin(ITransportParentCore parent)
			: base((IBusiness)parent)
		{
			ScheduleUpdateGuiQueryProvider.Set(parent.Factory, "", ""); // can't change etd/eta on the Booking Details Tab.
			UserSailingManagerQueryProvider.Register(parent.Factory);
			this.parent = parent;
		}

		readonly ITransportParentCore parent;
		readonly IRoutingSupport routingSupport;

		public override string Name
		{
			get { return Res.GetString("c17283a8-5b9c-4bfd-a990-ad2cbb10a493", "Routing"); }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			return new RoutingPluginControl();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			if (parent != null)
			{
				var transportCollection = new TransportCollection(parent);
				transportCollection.Load();
				return transportCollection;
			}
			else
			{
				var routingCollection = routingSupport.TransportsIncludingRelated;
				routingCollection.PluginBehaviourEnabled = true;
				return routingCollection;
			}
		}

		public void DisableAllControls()
		{
			DisableScheduleAndFlightButtonsWithMessage();
			DisableControlAndAllChildren(UserControl);
			DisableRoutingControlNewRows();
		}

		void DisableControlAndAllChildren(Control control)
		{
			if (control is ZGrid grid)
			{
				grid.SetReadOnlyIncludingColumnStyles(true);
			}
			else if ((control is ZCodeFindBox || control.Controls.Count == 0) && (string)control.Tag != "ShouldNotDisable")
			{
				control.Enabled = false;
			}
			foreach (Control subControl in control.Controls)
			{
				DisableControlAndAllChildren(subControl);
			}
		}

		void DisableScheduleAndFlightButtonsWithMessage()
		{
			RoutingPluginControl.DisableScheduleAndFlightButtonsWithMessage();
		}

		void DisableRoutingControlNewRows()
		{
			RoutingPluginControl.RoutingCollectionAllowNew = false;
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		RoutingPluginControl RoutingPluginControl => (RoutingPluginControl)UserControl;
	}
}
