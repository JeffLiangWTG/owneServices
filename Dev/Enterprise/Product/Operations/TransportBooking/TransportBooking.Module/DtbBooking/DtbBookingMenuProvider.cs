using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI.Options.QueryProvider;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.Module
{
	public sealed partial class DtbBookingMenuProvider : IDtbBookingMenuProvider
	{
		public DtbBookingMenuProvider(Func<IDtbBookingParent> transportBookingParentGetter)
		{
			this.TransportBookingParentGetter = Argument.NotNull(transportBookingParentGetter, "transportBookingParentGetter");
		}

		readonly Func<IDtbBookingParent> TransportBookingParentGetter;
		Func<bool> Callback;

		public static DtbBookingMenuProvider New(IDtbBookingParent transportBookingParent)
		{
			return new DtbBookingMenuProvider(() => transportBookingParent);
		}

		public ZMenuItem ConstructMenu(Func<bool> callback = null)
		{
			Callback = callback;

			const string TransportBookingName = "TransportBooking";

			var menu = new ZMenuItem(ResString.GetMultilingualString("52bc8a1a-648c-4fcb-b2b8-36d5f771ad6d", "Transport Booking"));
			menu.Name = TransportBookingName;
			menu.Popup += delegate
			{ RefreshMenuItems(menu); };

			ZFormMenuStrategy.FlagActionMenuAsAlwaysEnabled(TransportBookingName);

			AddNoActionsMenuItem(menu);

			return menu;
		}

		IMenuItem IDtbBookingMenuProvider.ConstructMenu(Func<bool> callback) => ConstructMenu(callback);

		void RefreshMenuItems(ZMenuItem menu)
		{
			menu.MenuItems.Clear();
			var transportBookingParent = TransportBookingParentGetter();
			if (transportBookingParent != null && transportBookingParent.CanCreateTransportBooking)
			{
				var addSeparator = false;
				foreach (var direction in transportBookingParent.GetSupportedDirections())
				{
					if (addSeparator)
					{
						menu.MenuItems.Add(ZMenuItem.Separator);
					}

					addSeparator = AddMenuItem(menu, transportBookingParent, direction);
				}

				if (menu.MenuItems.Count == 0)
				{
					AddDisabledMenuItem(menu, ResString.GetMultilingualString("7d189612-0063-4a6d-9df5-e02843b02636", "No Transport Bookings"));
				}
			}
			else
			{
				AddNoActionsMenuItem(menu);
			}
		}

		void AddNoActionsMenuItem(ZMenuItem menu)
		{
			AddDisabledMenuItem(menu, ResString.GetMultilingualString("bcd4bb67-869c-4cfd-bac2-b8dbe119d842", "No Actions Available"));
		}

		void AddDisabledMenuItem(ZMenuItem menu, ResourceString menuItemText)
		{
			var menuItem = new ZMenuItem(menuItemText);
			menu.MenuItems.Add(menuItem);
			menuItem.Enabled = false;
		}

		bool AddMenuItem(ZMenuItem menu, IDtbBookingParent transportBookingParent, DtbBookingDirection direction)
		{
			bool itemAdded = false;

			var cancellableParent = transportBookingParent as ICancellable;
			bool isParentEnabled = (cancellableParent == null || !cancellableParent.IsCancelled);

			var bookingConsolidationInNewFactory = DtbBookingConsolidation.FindExistingTransportBookingConsolidation(new BusinessObjectFactory(), transportBookingParent, direction);
			if (bookingConsolidationInNewFactory != null && bookingConsolidationInNewFactory.Bookings.Any())
			{
				var actionText = isParentEnabled ? DtbDeliveryManager.ViewText.Caption : DtbDeliveryManager.ViewTextWhenParentCancelled.Caption;
				var menuItemText = ResString.GetMultilingualString("DtbBookingPlugin|ViewEditTransportBooking", "{0} {1} Transport Booking", actionText, DtbBookingDirectionDescription.GetDescription(direction));
				menu.MenuItems.Add(new ZMenuItem(menuItemText, ViewTransportBookingDelegate(transportBookingParent, direction, false)));

				itemAdded = true;
			}

			if (isParentEnabled)
			{
				AddMenuItemCore(menu, transportBookingParent, direction, bookingConsolidationInNewFactory, combineContainers: false);

				if (transportBookingParent.RequiresMultiContainerBooking)
				{
					AddMenuItemCore(menu, transportBookingParent, direction, bookingConsolidationInNewFactory, combineContainers: true);
				}

				itemAdded = true;
			}

			return itemAdded;
		}

		void AddMenuItemCore(ZMenuItem menu, IDtbBookingParent transportBookingParent, DtbBookingDirection direction, DtbBookingConsolidation bookingConsolidationInOtherFactory, bool combineContainers)
		{
			var description = DtbBookingDirectionDescription.GetDescription(direction);

			if (combineContainers)
			{
				description = description + " " + DtbDeliveryManager.MultiContainerText.Caption;
			}

			var actionText = DtbDeliveryManager.HasExistingBooking(bookingConsolidationInOtherFactory, combineContainers)
				? DtbDeliveryManager.CreateTextWhenHasExistingBooking.Caption
				: DtbDeliveryManager.CreateText.Caption;

			description = ResString.GetMultilingualString("DtbBookingPlugin|CreateOverrideTransportBooking", "{0} {1} Transport Booking", actionText, description);

			menu.MenuItems.Add(new ZMenuItem(description, CreateTransportBookingDelegate(transportBookingParent, direction, combineContainers)));
		}

		public bool ShowFormsFromMainThread { get; set; }

		EventHandler ViewTransportBookingDelegate(IDtbBookingParent transportBookingParent, DtbBookingDirection direction, bool combineContainers)
		{
			return delegate
			{
				var deliveryManager = new DtbDeliveryManager(transportBookingParent.Factory, transportBookingParent, direction, combineContainers)
				{
					ShowFormsFromMainThread = ShowFormsFromMainThread
				};

				SetDeliveryManagerForTest(deliveryManager);
				deliveryManager.ViewTransportBooking();
			};
		}

		EventHandler CreateTransportBookingDelegate(IDtbBookingParent transportBookingParent, DtbBookingDirection direction, bool combineContainers)
		{
			return delegate
			{
				var factory = new BusinessObjectFactory();
				DtbDeliveryManager.CreateTransportBooking(factory, transportBookingParent, direction, combineContainers, manager =>
				{
					manager.ShowFormsFromMainThread = ShowFormsFromMainThread;
					SetDeliveryManagerForTest(manager);
				}, Callback);
			};
		}

		partial void SetDeliveryManagerForTest(DtbDeliveryManager deliveryManager);
	}
}

#if DEBUG

namespace Enterprise.TransportBookings.Module
{
	public partial class DtbBookingMenuProvider
	{
		partial void SetDeliveryManagerForTest(DtbDeliveryManager deliveryManager)
		{
			LastDeliveryManagerForTest = deliveryManager;
		}

		public DtbDeliveryManager LastDeliveryManagerForTest { get; set; }
	}
}

#endif
