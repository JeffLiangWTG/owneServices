using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.GUI
{
	class AttachBookingModuleGridHelper_ForTest : AttachBookingModuleGridHelper
	{
		public AttachBookingModuleGridHelper_ForTest(ForwardingConsol consol, BusinessObject objectsToAttach) : base(consol)
		{
			this.forwardingConsol = consol;
			this.objectsToAttach = objectsToAttach;
		}

		readonly BusinessObject objectsToAttach;
		readonly ForwardingConsol forwardingConsol;

		public static EventHandler NewEventHandler(ForwardingConsol consol, BusinessObject objectToAttach)
		{
			return delegate
			{
				AttachBookingModuleGridHelper_ForTest helper = new AttachBookingModuleGridHelper_ForTest(consol, objectToAttach);
				helper.ShowBookingGrid();
			};
		}

		void BookingsSelectedTest(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			var helper = new BuildConsolHelper();
			helper.AddBookingsToConsol(forwardingConsol, e.SelectedBusinessObjects.Select(booking => booking.PK).ToArray());
		}

		protected override void AddBookingsSelectedEvent(EmbeddedModulePopup embeddedModulePopup)
		{
			embeddedModulePopup.Selected += this.BookingsSelectedTest;
		}

		public void CallBookingsSelected(EmbeddedModulePopup.SelectedEventArgs e)
		{
			BookingsSelectedTest(null, e);
		}

		protected override void ShowEmbededModulePopupAndAttachEvent(ZFilterModule module, VirtualFindBox findBox, AttachBookingsModuleButtonGridDecisionProvider decisionProvider)
		{
			module.OverrideModuleDecisionProvider(decisionProvider);
			using (var embeddedModulePopup = new EmbeddedModulePopup(module))
			{
				this.AddBookingsSelectedEvent(embeddedModulePopup);
				if (objectsToAttach != null)
				{
					var newArgs = new EmbeddedModulePopup.SelectedEventArgs(new[] { objectsToAttach });
					CallBookingsSelected(newArgs);
				}
			}
		}
	}
}
