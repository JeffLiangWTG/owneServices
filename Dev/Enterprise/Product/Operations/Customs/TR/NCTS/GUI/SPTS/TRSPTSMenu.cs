using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.GUI.MessagingProcess;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.Customs.TR.NCTS.Business.MessagingProcess;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public class TRSPTSMenu : ZMenuItem
	{
		public TRSPTSMenu(SPTSHeader sptsheader) : base(Captions.SPTS)
		{
			this.header = sptsheader;

			if (header != null)
			{
				BuildMenus();
			}
		}
		readonly SPTSHeader header;
		ZForm MainForm => (ZForm)GetMainMenu()?.GetForm();

		void BuildMenus()
		{
			MenuItems.Clear();
			var menuItems = new List<ZMenuItem>();
			var sendforSPTSDeclarationMenuItem = AddMenuItem(MainForm, header, menuItems, Captions.SendforSPTSDeclaration, () => SendMessage(), false);
			if (!header.RegistrationNumber.IsEmpty)
			{
				AddMenuItem(MainForm, header, menuItems, Captions.AmendSPTSDeclaration, () => ChangeStatusAmendSPTS(header), false);
			}
			MenuItems.AddRange(menuItems.ToArray());
		}

		void SendMessage()
		{
			var providerFactory = new TRCustomsMessagingProviderFactory(SPTSCustomsMessagingProvider.New, string.Empty);
			_ = TRCustomsMessagingGui.SendMessages(header, providerFactory, MainForm);
		}

		void ChangeStatusAmendSPTS(SPTSHeader header)
		{
			if (MainForm != null && header != null && Enterprise.Customs.GUI.SaveDataFirst.Confirm(header, MainForm))
			{
				var response = Globals.Message.Show(ResString.GetMultilingualString("7039FD39-909C-4292-9C5C-A2C188878DF6", "Do you want to amend the registered SPTS Declaration?"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Information, DialogResult.No);
				if (response == DialogResult.Yes)
				{
					try
					{
						header.ChangeToAmmendSPTS();
						MainForm.FireSaveButton();
						Globals.Message.Show(ResString.GetMultilingualString("46561A0F-4B03-41A5-9B9E-5FE4D661ADA0", "The SPTS Declaration is amended and ready for sending to Customs.\r\nPlease Send the SPTS to Customs with Send for SPTS Declaration option"));
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		ZMenuItem AddMenuItem(ZForm mainForm, SPTSHeader header, List<ZMenuItem> menuItems, ResourceString caption, Action send, bool needValidation)
		{
			var menuItem = new ZMenuItem(caption);
			menuItem.Click += delegate
			{
				send();
			};
			menuItems.Add(menuItem);

			return menuItem;
		}

		protected override void OnPopup(EventArgs e)
		{
			BuildMenus();
			base.OnPopup(e);
		}

		static class Captions
		{
			public static ResourceString SPTS => ResString.GetMultilingualString("93FAF85A-2B3E-4CA9-9244-E41344D144D5", "SPTS");
			public static ResourceString SendforSPTSDeclaration => ResString.GetMultilingualString("6F2C01DE-0E6B-4C39-9194-20D5318BAD90", "Send for SPTS Declaration");
			public static ResourceString AmendSPTSDeclaration => ResString.GetMultilingualString("17086207-4502-4884-93EC-B305CC7B6D75", "Amend SPTS Declaration");
		}
	}
}
