using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.GUI;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Licensing;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using InternalCartageManager = Enterprise.Freight.LocalCartage.GUI.InternalCartageManager;

namespace Enterprise.Freight.LocalCartage.Module
{
	public partial class CartagePlugin : ZPlugIn, INotifications, INotificationSubscriberQueryUser
	{
		public CartagePlugin(ICartageParent cartageParent)
			: base((IBusiness)cartageParent)
		{
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			topLevelMenu = new ZMenuItem(ResString.GetMultilingualString("2A6175AF-D12B-4b7b-B2B8-539BA38AF2A0", "Port Transport"));
			if (CartageParent.RebuildLocalCartageMenuOnClick)
			{
				topLevelMenu.Click += new EventHandler(topLevelMenu_Click);
			}
			else
			{
				AddMenuItemsToLocalCartageMenu(topLevelMenu);
				HookEvents();
			}
			return topLevelMenu;
		}

		void topLevelMenu_Click(object sender, EventArgs e)
		{
			RefreshLocalCartageMenu();
		}

		MenuItem topLevelMenu;

		void RefreshLocalCartageMenu()
		{
			topLevelMenu.MenuItems.Clear();
			AddMenuItemsToLocalCartageMenu(topLevelMenu);
		}

		protected override void UnHookFormEventsCore()
		{
			base.UnHookFormEventsCore();
			if (hookedCartageParent != null)
			{
				hookedCartageParent.CartageTypesChanged -= CartageParent_CartageTypesChanged;
			}
			UnHookCartageTypeEvents();
		}

		void AddMenuItemsToLocalCartageMenu(MenuItem localCartageMenu)
		{
			foreach (CartageType cartageType in CartageParent.CartageTypes)
			{
				MenuItem cartageTypeMenu = new ZMenuItem(ResString.GetMultilingualString("FDC7B2EF-47AB-4997-B69A-DDBD8FD4D4C6", "{0} Port Transport", cartageType.DescriptionForMenu));
				localCartageMenu.MenuItems.Add(cartageTypeMenu);

				CommonCartage cartage = CartageParent.IsInDatabase ? GetNewCartageManager(cartageType).FindCartage() : null;

				if (cartage == null)
				{
					cartageTypeMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("95336EE1-E6BC-44EE-A718-9DCC1485555B", "Create Port Transport"), CreateCartageDelegate(cartageType)));
				}
				else
				{
					cartageTypeMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("CCDF4D06-11C6-4559-A5E4-DAF0F28CE136", "Create & Overwrite Port Transport"), CreateCartageDelegate(cartageType)));
					cartageTypeMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("5C11322B-5BC3-4440-828F-491BDEA56A3A", "View Port Transport"), ViewCartageDelegate(cartageType)));
				}

				cartageTypeMenu.MenuItems.Add("-");
				cartageTypeMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("D994F07F-BAD8-4564-BD88-C4B71CEBEEE3", "Export Port Transport Booking To XML"), ExportCartageBookingDelegate(cartageType, CancellationToken.None)));
				cartageTypeMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("760A2858-76FD-4E03-A30D-2EA9A2202181", "Export Port Transport Booking To XML: Store As File"), ExportCartageBookingAsFileDelegate(cartageType, CancellationToken.None)));
			}

			if (CartageParent.CartageTypes.Count == 0)
			{
				MenuItem notSupportedMenu = new ZMenuItem(ResString.GetMultilingualString("CartageNotSupported|EE3B9F08-D31F-4538-B936-7428CE770429", "Not Supported"));
				localCartageMenu.MenuItems.Add(notSupportedMenu);
				notSupportedMenu.Click += delegate
				{ Globals.Message.ShowError(Res.GetString("4205c001-5683-43ff-bf94-53d89d1e3d6f", "This {0} doesn't support a Port Transport.", CartageParent.HumanReadableName)); };
			}

			if (CartagePluginExtras != null)
			{
				CartagePluginExtras.MenuAdditions(topLevelMenu);
			}
		}

		ICartagePluginExtras CartagePluginExtras
		{
			get { return Form as ICartagePluginExtras; }
		}

		void HookEvents()
		{
			CartageParent.CartageTypesChanged += CartageParent_CartageTypesChanged;
			hookedCartageParent = CartageParent;
			HookCartageTypeEvents();
		}

		void HookCartageTypeEvents()
		{
			foreach (CartageType cartageType in CartageParent.CartageTypes)
			{
				Tuple<ZPropertyInfo, EventHandler> infoWithHandler;
				if (!handlerLookup.TryGetValue(cartageType, out infoWithHandler))
				{
					if (cartageType.CartageAddressInfo != null)
					{
						infoWithHandler = new Tuple<ZPropertyInfo, EventHandler>(cartageType.CartageAddressInfo, CreateCartageOrganisationChangedDelegate(cartageType));
						handlerLookup.Add(cartageType, infoWithHandler);
						cartageType.CartageAddressInfo.ValueChanged += infoWithHandler.Item2;
					}
				}
			}
		}

		void UnHookCartageTypeEvents()
		{
			foreach (CartageType cartageType in handlerLookup.Keys)
			{
				Tuple<ZPropertyInfo, EventHandler> infoWithHandler;
				if (handlerLookup.TryGetValue(cartageType, out infoWithHandler))
				{
					infoWithHandler.Item1.ValueChanged -= infoWithHandler.Item2;
				}
			}
			handlerLookup.Clear();
		}

		readonly Dictionary<CartageType, Tuple<ZPropertyInfo, EventHandler>> handlerLookup = new Dictionary<CartageType, Tuple<ZPropertyInfo, EventHandler>>();

		void CartageParent_CartageTypesChanged(object sender, EventArgs e)
		{
			UnHookCartageTypeEvents();
			HookCartageTypeEvents();
			RefreshLocalCartageMenu();
		}

		EventHandler CreateCartageOrganisationChangedDelegate(CartageType cartageType)
		{
			return delegate(object sender, EventArgs e)
			{
				ValueChangedEventArgs ve = e as ValueChangedEventArgs;
				if (ve != null)
				{
					CartageOrganisationChanged(GetNewCartageManager(cartageType), (ZGuid)ve.OldValue, (ZGuid)ve.NewValue);
				}
			};
		}

		InternalCartageManager ManagerCartageOrgChange { get; set; }

		void CartageOrganisationChanged(InternalCartageManager manager, ZGuid oldValue, ZGuid newValue)
		{
			if (!settingCartageOrg)
			{
				try
				{
					settingCartageOrg = true;

					if (oldValue != newValue)
					{
						ZString statement = "";
						ZString question = "";

						if (CartageParent.IsInDatabase)
						{
							CommonCartage existingCartage = manager.FindCartage();

							if (existingCartage != null && !manager.DeactivateExisting(this, existingCartage, CartageParent.Factory))
							{
								manager.CartageType.CartageAddressInfo.Value = oldValue;
								return;
							}

							if (existingCartage != null && existingCartage.IsCancelled)
							{
								statement = Res.GetString("c087e023-c625-423a-bec9-19d702bcad1b", "Cancellation of old cartage job successful.") + " ";
							}
						}

						if (manager.CartageType.LocalTransportProviderAddress != null && manager.CartageType.LocalTransportProviderAddress.Header.IsProxyOrgOfAnyCompany() && TransportRegistry.Instance.PromptToCreateLocalTransportJob.Value && IsOKToShowMessagebox())
						{
							ManagerCartageOrgChange = manager;
							Setup();
						}
						if (!statement.IsEmpty)
						{
							this.Notify(new InfoNotification(statement));
							RefreshLocalCartageMenu();
						}
					}
				}
				finally
				{
					settingCartageOrg = false;
				}
			}
		}

		public override void OnSaveCompletedOrAborted(bool saved)
		{
			base.OnSaveCompletedOrAborted(saved);
			if (saved)
			{
				if (ManagerCartageOrgChange != null)
				{
					var question = Res.GetString("03e2e36d-0f88-4b3d-a2be-44da981ec92b", "Would you like to create a Related Port Transport Job?");
					string caption = Res.GetString("5aa4b424-b765-4be0-a66a-948a8ad68a4c", "Local Transport Organization Changed");
					QueryUserYesNoEventArgs queryArgs = new QueryUserYesNoEventArgs(caption, question, false);
					this.QueryUser(queryArgs);

					if (queryArgs.Response)
					{
						CreateCartage(ManagerCartageOrgChange.CartageType);
					}
				}
			}
			ManagerCartageOrgChange = null;
		}

		internal int ValidTransactionCount;
		bool IsOKToShowMessagebox()
		{
			// In TransactionedTestCases transaction level is expected to 1, otherwise 0 is the limit.
			return (Db.Connection.AppTransactionCount <= ValidTransactionCount);
		}

		bool settingCartageOrg;

		EventHandler CreateCartageDelegate(CartageType cartageType)
		{
			return delegate
			{
				CreateCartage(cartageType);
			};
		}

		EventHandler ViewCartageDelegate(CartageType cartageType)
		{
			return delegate
			{
				ViewCartage(cartageType);
			};
		}

		EventHandler ExportCartageBookingDelegate(CartageType cartageType, CancellationToken token)
		{
			return delegate
			{
				ExportCartageBooking(cartageType, token);
			};
		}

		EventHandler ExportCartageBookingAsFileDelegate(CartageType cartageType, CancellationToken token)
		{
			return delegate
			{
				ExportCartageBookingAsFile(cartageType, token);
			};
		}

		void CreateCartage(CartageType cartageType)
		{
			var manager = GetNewCartageManager(cartageType);

#if DEBUG
			if (Globals.IsTest)
			{
				manager.DoNotShowFormOnCreateCartageTestOnly = DoNotShowFormOnCreateCartageTestOnly;
			}
#endif
			manager.CreateCartageAndShow(this, topLevelMenu.GetMainMenu().GetForm(), delegate
			{ RefreshLocalCartageMenu(); });
		}

		void ViewCartage(CartageType cartageType)
		{
			GetNewCartageManager(cartageType).ViewCartage(this, topLevelMenu.GetMainMenu().GetForm());
		}

		void ExportCartageBooking(CartageType cartageType, CancellationToken token)
		{
			GetNewCartageManager(cartageType).ExportCartageBooking(this, token);
		}

		void ExportCartageBookingAsFile(CartageType cartageType, CancellationToken token)
		{
			GetNewCartageManager(cartageType).ExportCartageBookingAsFile(this, token);
		}

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification.Message);
		}

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			QueryUserMsgBoxEventArgs msgBoxArgs = e as QueryUserMsgBoxEventArgs;
			if (msgBoxArgs != null)
			{
				var defaultResponse = (msgBoxArgs.Response ? DialogResult.Yes : DialogResult.No);

#if DEBUG
				if (OverrideQueryUserResponse.HasValue)
				{
					defaultResponse = OverrideQueryUserResponse.Value;
				}
#endif

				msgBoxArgs.Response = Globals.Message.Show(msgBoxArgs.Message, msgBoxArgs.Caption, MessageBoxButtons.YesNo, defaultResponse) == DialogResult.Yes;
			}
		}

#if DEBUG
		// null == don't override
		public DialogResult? OverrideQueryUserResponse { get; set; }
		public bool DoNotShowFormOnCreateCartageTestOnly { get; set; }
#endif

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.Core; }
		}

		protected InternalCartageManager GetNewCartageManager(CartageType cartageType)
		{
			return new InternalCartageManager(cartageType) { LicenceCheckpointForViewOverride = LicenceCheckpointForViewCartage };
		}

		internal LicenceCheckpoint LicenceCheckpointForViewCartage
		{
			get { return Env.Licence.LocalTransport; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return (IBusiness)CartageParent;
		}

		ICartageParent CartageParent
		{
			get { return (ICartageParent)HostBusinessEntity; }
		}

		ICartageParent hookedCartageParent;

		protected override ZBool HasUserControl
		{
			get { return false; }
		}

		public override string Name
		{
			get { return (NoResString)"Cartage Plugin"; }
		}
	}
}
