using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public class QuickAddressHelper : IDisposable
	{
		public QuickAddressHelper(ZDropButtonOnly button)
		{
			Button = button;
			HookButton();
		}
		readonly ZDropButtonOnly Button;

		public QuickAddressHelperStrategy Parent { get; set; }

		void HookButton()
		{
			Button.Click += new EventHandler(Button_Click);
			Button.KeyUp += new KeyEventHandler(Button_KeyUp);
		}

		void UnHookButton()
		{
			Button.Click -= new EventHandler(Button_Click);
			Button.KeyUp -= new KeyEventHandler(Button_KeyUp);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			UnHookButton();
		}

		void Button_Click(object sender, EventArgs e)
		{
			ShowAddressSelectionMenu();
		}

		void Button_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Down)
			{
				ShowAddressSelectionMenu();
			}
		}

		void ShowAddressSelectionMenu()
		{
			if (Parent != null)
			{
				var context = new ContextMenu();

				foreach (var element in Parent.GetAddressElements())
				{
					var addressTypeMenu = FindOrCreateMenu(context.MenuItems, element.OrgTypeDescription, element.OrgTypeDescription);
					var menu = new AddressSelectionMenu(element);
					addressTypeMenu.MenuItems.Add(menu);
					menu.Click += delegate
					{ Parent.Info.Value = menu.Element.DocOrOrgAddressPK; };
				}

				context.MenuItems.Add("-");

				var openAddressMenu = FindOrCreateMenu(context.MenuItems, "OpenOrganization", Res.GetString("9b1669bc-d58d-4d39-a385-461ae3cb6cf5", "Open Organization"));
				var docAddress = Parent.Factory.Load<JobDocAddress>((ZGuid)Parent.Info.Value);

				if (docAddress == null)
				{
					openAddressMenu.Enabled = false;
				}
				else
				{
					openAddressMenu.Click += delegate
					{
						if (!docAddress.E2_AddressOverride && docAddress.Organisation != null)
						{
							var controller = ZControllerFactory.Create(ControllerIDs.Organisation);
#if DEBUG
							SetControllerForTest(controller);
#endif
							controller.ShowEditForm(docAddress.Organisation);
						}
						else
						{
							Globals.Message.Show(Res.GetString("711ba612-8dbc-4c72-ac25-9e69f8f4ccd7", "This address is either blank or an overridden free text address. No Organization record exists to open."), Res.GetString("74a618f3-8594-4cbc-b3c5-1da413917238", "No Organization record exists"), MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
					};
				}

				var clearAddressMenu = FindOrCreateMenu(context.MenuItems, "ClearAddress", Res.GetString("74210017-08aa-4b08-acc6-e5e8a343811c", "Clear"));
				if (Parent.Info.Value.IsEmpty)
				{
					clearAddressMenu.Enabled = false;
				}
				else
				{
					clearAddressMenu.Click += delegate
					{ Parent.Info.Value = ZGuid.Empty; };
				}

				var newAddressMenu = FindOrCreateMenu(context.MenuItems, "NewAddress", Res.GetString("d1263286-3073-4c3c-b2fe-d649404d2a4b", "New Address..."));
				newAddressMenu.Click += delegate
				{ ShowDocAddressCreatorForm(); };
				ShowContextMenu(context);
			}
		}

		MenuItem FindOrCreateMenu(Menu.MenuItemCollection menus, string key, string menuText)
		{
			var addressTypeMenu = menus[key];

			if (addressTypeMenu == null)
			{
				addressTypeMenu = new ZMenuItem(menuText);
				addressTypeMenu.Name = key;
				menus.Add(addressTypeMenu);
			}

			return addressTypeMenu;
		}

		void ShowDocAddressCreatorForm()
		{
			var host = new DocAddressCreatorHost(Parent.Parent, (c) => CommonCartageAddressHelper.GetOrgTypeFromCartageDocAddressType(c), Parent.DefaultAddressType, Parent.Factory);
			using (QuickAddressForm addressForm = new QuickAddressForm(host, Button))
			{
				ZFormModaliser.ShowDialogWithoutDispose(addressForm);

				if (addressForm.DialogResult == DialogResult.OK)
				{
					Parent.Info.Value = host.DocAddress.PK;
				}
				else
				{
					host.DocAddress.Delete();
				}
			}
		}

		void ShowContextMenu(ContextMenu context)
		{
#if DEBUG
			SetContextForTest(context);
#endif

			if (!Globals.IsTest)
			{
#if !WINZOR
				context.Show(Button, ControlDpiScalingHelper.NewScaledPoint(0, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Button.Height)));
#else
				context.Show(Button, ControlDpiScalingHelper.NewScaledPoint(0, Button.Location.Y + ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Button.Height)), isScreenPoint: false);
#endif
			}
		}

#if DEBUG
		void SetContextForTest(ContextMenu menu)
		{
			ContextForTest = menu;
		}

		void SetControllerForTest(ZController controller)
		{
			ControllerForTest = controller;
		}

		public ContextMenu ContextForTest { get; private set; }
		public ZController ControllerForTest { get; private set; }
#endif
	}
}
