using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.TransportBookings.Module
{
	// write a test that ensures parents of this plugin have document menus for booking cartage advice

	public partial class DtbBookingConsolidationPlugIn : ZPlugIn
	{
		public DtbBookingConsolidationPlugIn(IBusiness transportBookingParent)
			: base(transportBookingParent)
		{
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			// we need to decide somehow when to show this.
			ZFormMenuStrategy.AddActionsMenuItem(Form, new ZMenuItem("-"));

			var menuItemsProvider = Form as IFileMenuItemsProvider;
			if (menuItemsProvider != null && menuItemsProvider.ActionsMenuItem != null)
			{
				menuItemsProvider.ActionsMenuItem.AddDeferredMenuItems(Form, GetActionMenuItems);
			}

			return base.GetNewTopLevelMenu();
		}

		IEnumerable<MenuItem> GetActionMenuItems()
		{
			var menuProvider = new DtbBookingMenuProvider(() => TransportBookingParent);
			SetMenuProviderForTest(menuProvider);

			var menuItem = menuProvider.ConstructMenu();

			return new List<MenuItem> { menuItem };
		}

		protected override ZBool HasUserControl
		{
			get { return false; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.TransportBookings; }
		}

		public override string Name
		{
			get { return (NoResString)"Transport Booking Plug-in"; }
		}

		IDtbBookingParent TransportBookingParent
		{
			get { return (IsCurrentDependent ? Current : HostBusinessEntity) as IDtbBookingParent; }
		}

		partial void SetMenuProviderForTest(DtbBookingMenuProvider menuProvider);
	}
}

#if DEBUG

namespace Enterprise.TransportBookings.Module
{
	public partial class DtbBookingConsolidationPlugIn
	{
		partial void SetMenuProviderForTest(DtbBookingMenuProvider menuProvider)
		{
			LastMenuProviderForTest = menuProvider;
		}

		public DtbBookingMenuProvider LastMenuProviderForTest { get; set; }

		public IDtbBookingParent ExposedTransportBookingParent
		{
			get { return TransportBookingParent; }
		}
	}
}

#endif
