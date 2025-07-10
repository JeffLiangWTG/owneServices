using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI
{
	public class PortMessagingPlugIn : ZPlugIn
	{
		public PortMessagingPlugIn(IBusiness hostEntity)
			: base(hostEntity)
		{
			hostEntity.Factory.Saving += new BusinessObjectFactory.SavingEventHandler(SetEnabledOnSave);
			Enabled = ShouldShowPortMessaging;
		}

		#region Menus Providers

		DakosyPortMessagingMenusProvider DakosyPortMessagingMenusProvider
		{
			get
			{
				if (dakosyPortMessagingMenusProvider == null)
				{
					dakosyPortMessagingMenusProvider = new DakosyPortMessagingMenusProvider(HostBusinessEntity);
				}

				return dakosyPortMessagingMenusProvider;
			}
		}
		DakosyPortMessagingMenusProvider dakosyPortMessagingMenusProvider;

		FormsPortMessagingMenusProvider FormsPortMessagingMenusProvider
		{
			get
			{
				if (formsPortMessagingMenusProvider == null)
				{
					formsPortMessagingMenusProvider = new FormsPortMessagingMenusProvider(HostBusinessEntity);
				}

				return formsPortMessagingMenusProvider;
			}
		}
		FormsPortMessagingMenusProvider formsPortMessagingMenusProvider;

		IEnumerable<IPortMessagingMenusProvider> MenusProviders
		{
			get
			{
				if (menusProviders == null)
				{
					menusProviders = new IPortMessagingMenusProvider[]
					{
						DakosyPortMessagingMenusProvider,
						FormsPortMessagingMenusProvider
					};
				}
				return menusProviders;
			}
		}
		IEnumerable<IPortMessagingMenusProvider> menusProviders;

		#endregion

		#region Disposing

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				HostBusinessEntity.Factory.Saving -= SetEnabledOnSave;

				if (portMenuItem != null)
				{
					portMenuItem.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Plug In Properties

		#region Enabled / Active

		protected override ZBool IsActive
		{
			get { return ShouldShowPortMessaging; }
		}

		bool ShouldShowPortMessaging => MenusProviders.Any(x => x.Enabled);

		void SetEnabledOnSave(BusinessObjectFactory factory)
		{
			Enabled = IsActive;
		}

		#endregion

		#region PlugIn Overrrides

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Forwarder; }
		}

		public override string Name
		{
			get { return "PortMessaging"; }
		}

		#region Port Messaging Menu

		protected override MenuItem GetNewTopLevelMenu()
		{
			return portMenuItem ?? (portMenuItem = GetPortMessagingMenuItem());
		}
		MenuItem portMenuItem;

		MenuItem GetPortMessagingMenuItem()
		{
			if (!ShouldShowPortMessaging)
			{
				return null;
			}

			var result = new ZMenuItem(ResString.GetMultilingualString("276849b9-c1b9-433a-8a6e-91bf01d48f63", "Port Messaging"));

			foreach (var menuProvider in MenusProviders.Where(x => x.Enabled))
			{
				menuProvider.AddMenuItems(result);
			}

			return result;
		}

		#endregion

		public override ZString PlugInNotDisplayedMessage
		{
			get { return Res.GetString("c14b3d36-f9b4-469b-a101-df9a69e4389d", "Port Messaging is supported only by Forwarding Shipment and Consol"); }
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return DakosyPortMessagingMenusProvider?.PortMessagingManager != null;
		}

		protected override bool ShouldPluginDropdownMenuBeCreated()
		{
			return ShouldPlugInGUIAndBusinessEntityBeCreatedCore();
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return DakosyPortMessagingMenusProvider?.PortMessagingManager;
		}

		protected override Control GetNewUserControl()
		{
			return new PortMessagingControl();
		}

		public override void OnGUIShown()
		{
			base.OnGUIShown();

			((PortMessagingControl)UserControl).ProcessContent();
		}

		#endregion

		#endregion
	}
}
