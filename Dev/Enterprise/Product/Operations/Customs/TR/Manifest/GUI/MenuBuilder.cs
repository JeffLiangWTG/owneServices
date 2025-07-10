using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.GUI.MessagingProcess;
using Enterprise.Customs.TR.Manifest.Business.MessagingProcess;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.Manifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("43828B7E-256F-484E-8B3E-04D700F5B788", "TR Manifest");

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();

			if (IsValidForMessage())
			{
				var messageLabel = Res.GetString("FE27E477-8A2B-468C-AB9C-B7B2C9FDBD80", "Manifest");
				MenuBuilderHelper.AddSendManifestMenuItem(mainForm, menuItems, Header, CreateManifestLevelMessage, messageLabel, false);
			}
			else
			{
				var menuItem = GetInvalidMessageMenuItem();
				menuItems.Add(menuItem);
			}

			if (!Header.RegistrationNumber.IsEmpty)
			{
				var amendMenuItemLabel = ResString.GetMultilingualString("C198C49F-D8DA-408B-B084-45EE42D5FA04", "Amend Manifest");
				MenuBuilderHelper.AddMenuItem(mainForm, menuItems, amendMenuItemLabel, Header, () => ChangeStatusAmendManifest(Header), false);
			}

			return menuItems.ToArray();
		}

		void CreateManifestLevelMessage(AsycudaManifestHeader header, string messageSubType)
		{
			if (header is Business.AsycudaManifestHeader trHeader && CheckRegisteredUser(trHeader))
			{
				var providerFactory = new TRCustomsMessagingProviderFactory(TRManifestCustomsMessagingProvider.New, TRMessageTypes.Codes.TRO);
				_ = TRCustomsMessagingGui.SendMessages(trHeader, providerFactory, mainForm);
			}
		}

		void ChangeStatusAmendManifest(AsycudaManifestHeader header)
		{
			if (header != null && mainForm != null)
			{
				if (CheckRegisteredUser(header) && Enterprise.Customs.GUI.SaveDataFirst.Confirm(header, mainForm))
				{
					var response = Globals.Message.Show(Res.GetString("B7B393D8-B131-45B4-BB67-3949FE2192AD", "Do you want to amend the registered manifest?"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Information, DialogResult.No);
					if (response == DialogResult.Yes)
					{
						try
						{
							var trHeader = (Business.AsycudaManifestHeader)header;
							trHeader.ChangeToAmmendManifest();
							mainForm.FireSaveButton();
							Globals.Message.Show(Res.GetString("B84822E3-AC7B-43A9-ACEF-D1D865113062", "The Manifest is amended and ready for sending to the Customs.\r\nPlease Send the Manifest to Customs with Send Manifest option"));
						}
						catch (ZSaveException ex)
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
				}
			}
		}

		bool CheckRegisteredUser(AsycudaManifestHeader header)
		{
			var result = ZBool.True;
			var registeredUser = header.AMA_GS_NKCustomsAgent;
			if (!registeredUser.IsEmpty && registeredUser != GlbStaff.CurrentUser.GS_Code)
			{
				var userName = header.CustomsAgent.GS_FullName;
				Globals.Message.ShowError(Res.GetString("0E764B0E-0C4C-4973-A6F7-5F4CE2CC0513", "The Global Manifest is declared by {0} you cannot declare this global manifest.", userName));
				result = ZBool.False;
			}

			return result;
		}
	}
}
