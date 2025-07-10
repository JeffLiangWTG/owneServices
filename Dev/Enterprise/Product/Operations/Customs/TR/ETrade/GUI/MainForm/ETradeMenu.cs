using System;
using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.ETrade.Business;
using Enterprise.Customs.TR.ETrade.Business.MessagingProcess;
using Enterprise.Customs.TR.GUI.MessagingProcess;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	public class ETradeMenu : ZMenuItem
	{
		public ETradeMenu(AsycudaManifestHeader header) : base(Captions.ETrade)
		{
			this.header = header;
			if (this.header != null)
			{
				BuildMenus();
			}
		}

		protected virtual ZForm MainForm => (ZForm)GetMainMenu()?.GetForm();
		readonly AsycudaManifestHeader header;
		ZMenuItem queryForRegistrationNoMenuItem;
		ZMenuItem calculateDutyMenuItem;
		ZMenuItem sendforDischargeListMenuItem;
		ZMenuItem queryRemainingBillsforImpDecMenuItem;
		ZMenuItem sendForComplementaryDecMenuItem;
		ZMenuItem temporaryRegistrationMenuItem;
		ZMenuItem queryForInspectionClerkMenuItem;
		ZMenuItem queryForInspectionLineMenuItem;
		ZMenuItem registrationNoForExportMenuItem;

		void BuildMenus()
		{
			MenuItems.Clear();

			var menuItems = new List<ZMenuItem>();
			calculateDutyMenuItem = MenuBuilderHelper.AddMenuItem(MainForm, menuItems, Captions.CalculateDuty, header, () => CalculateDuty(true), false);
			temporaryRegistrationMenuItem = MenuBuilderHelper.AddMenuItem(MainForm, menuItems, Captions.SendForTempReg, header, () => SendProcess(TRMessageTypes.Codes.TRE), false);
			queryForRegistrationNoMenuItem = MenuBuilderHelper.AddMenuItem(MainForm, menuItems, Captions.QueryForRegNo, header, () => SendProcess(TRMessageTypes.Codes.TRQ), false);
			registrationNoForExportMenuItem = MenuBuilderHelper.AddMenuItem(MainForm, menuItems, Captions.SendForRegistrationNo, header, () => SendProcess(TRMessageTypes.Codes.TRS), false);
			queryForInspectionClerkMenuItem = MenuBuilderHelper.AddMenuItem(MainForm, menuItems, Captions.QueryForInspectionClerk, header, () => SendProcess(TRMessageTypes.Codes.TRI), false);
			queryForInspectionLineMenuItem = MenuBuilderHelper.AddMenuItem(MainForm, menuItems, Captions.QueryForInspectionLine, header, () => SendProcess(TRMessageTypes.Codes.TRL), false);
			sendforDischargeListMenuItem = MenuBuilderHelper.AddMenuItem(MainForm, menuItems, Captions.SendforDischargeList, header, () => SendProcess(TRMessageTypes.Codes.TRD), false);
			queryRemainingBillsforImpDecMenuItem = MenuBuilderHelper.AddMenuItem(MainForm, menuItems, Captions.QueryRemainingBillsforImpDec, header, () => SendProcess(TRMessageTypes.Codes.TRB), false);
			sendForComplementaryDecMenuItem = MenuBuilderHelper.AddMenuItem(MainForm, menuItems, Captions.SendForComplementaryDec, header, () => SendProcess(TRMessageTypes.Codes.TCD), false);

			header.AMA_NatureInfo.ValueChanged += AMA_NatureInfo_ValueChanged;
			header.MessageModeInfo.ValueChanged += MessageModeInfo_ValueChanged;
			MenuItems.AddRange(menuItems.ToArray());
			ResetMenuItemsVisible();
		}

		void SendProcess(string messageType)
		{
			if (messageType == TRMessageTypes.Codes.TRE)
			{
				CalculateDuty(false);
			}

			var providerFactory = new TRCustomsMessagingProviderFactory(ETradeCustomsMessagingProvider.New, messageType);
			_ = TRCustomsMessagingGui.SendMessages(header, providerFactory, MainForm);
		}

		void CalculateDuty(bool shouldShowCalculationResult)
		{
			if (!header.DateAtCustomsOffice.IsEmpty)
			{
				header.CalculateDuties();
				if (shouldShowCalculationResult)
				{
					Globals.Message.Show(Res.GetString("TR.ETrade.Business.AsycudaBillAndAsycudaManifestHeader|SpecialConsuptionTaxRateSuccess", "Duties have been calculated."));
				}
			}
			else if (shouldShowCalculationResult)
			{
				Globals.Message.Show(Res.GetString("1BD66A81-DFA2-4542-A2EC-FD066A762E2F", "Arrival Date is required for Duty Calculation"));
			}
		}

		void AMA_NatureInfo_ValueChanged(object sender, EventArgs e)
		{
			ResetMenuItemsVisible();
		}

		void ResetMenuItemsVisible()
		{
			SetMenuVisible(calculateDutyMenuItem, header.IsImport);
			SetMenuVisible(temporaryRegistrationMenuItem, header.MessageMode == TRMessageTypes.Codes.TRE);
			SetMenuVisible(queryForRegistrationNoMenuItem, header.IsImport && !header.TempRegNo.IsEmpty && header.MessageMode == TRMessageTypes.Codes.TRQ);
			SetMenuVisible(registrationNoForExportMenuItem, header.IsExport && !header.TempRegNo.IsEmpty && header.MessageMode == TRMessageTypes.Codes.TRS);
			SetMenuVisible(queryForInspectionClerkMenuItem, !header.RegistrationNumber.IsEmpty && header.MessageMode == TRMessageTypes.Codes.TRI);
			SetMenuVisible(queryForInspectionLineMenuItem, !header.RegistrationNumber.IsEmpty && header.MessageMode == TRMessageTypes.Codes.TRL);
			SetMenuVisible(queryRemainingBillsforImpDecMenuItem, header.IsImport && !header.RegistrationNumber.IsEmpty && header.MessageMode == TRMessageTypes.Codes.TRB);
			SetMenuVisible(sendforDischargeListMenuItem, header.IsImport && !header.RegistrationNumber.IsEmpty && header.MessageMode == TRMessageTypes.Codes.TRD);
			SetMenuVisible(sendForComplementaryDecMenuItem, header.IsImport && !header.RegistrationNumber.IsEmpty && header.MessageMode == TRMessageTypes.Codes.TCD);
		}

		void SetMenuVisible(ZMenuItem menu, bool isVisible)
		{
			if (menu != null)
			{
				menu.Visible = isVisible;
			}
		}

		protected override void OnPopup(EventArgs e)
		{
			BuildMenus();
			base.OnPopup(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (header != null)
			{
				header.AMA_NatureInfo.ValueChanged -= AMA_NatureInfo_ValueChanged;
				header.MessageModeInfo.ValueChanged -= MessageModeInfo_ValueChanged;
			}

			base.Dispose(disposing);
		}

		void MessageModeInfo_ValueChanged(object sender, EventArgs e)
		{
			ResetMenuItemsVisible();
		}

		static class Captions
		{
			public static ResourceString ETrade => ResString.GetMultilingualString("DDC022DA-CA9B-4713-BAB5-45175BA36742", "E-Trade");
			public static ResourceString SendForTempReg => ResString.GetMultilingualString("B7A5778F-8812-47BE-BA0E-F4E91CEBC1DA", "Send for Temporary Registration");
			public static ResourceString QueryForRegNo => ResString.GetMultilingualString("A57323AF-4F20-4F92-8774-78B0B9F5F97C", "Query for Registration No");
			public static ResourceString QueryForInspectionLine => ResString.GetMultilingualString("F1723166-71C6-483B-8A5C-C5F9A0DBBE86", "Query for Inspection Line");
			public static ResourceString CalculateDuty => ResString.GetMultilingualString("A2A5B7FB-13A3-462B-914D-90EDE9514848", "Calculate Duty");
			public static ResourceString QueryForInspectionClerk => ResString.GetMultilingualString("331B8E26-6137-4C36-8A25-ED91F773FAF0", "Query for Inspection Clerk");
			public static ResourceString SendForRegistrationNo => ResString.GetMultilingualString("0A0ADCCB-0CAA-4539-84BF-56CD910BB598", "Send for Registration No");
			public static ResourceString SendforDischargeList => ResString.GetMultilingualString("E4099381-3A1C-4448-9B46-05A0A490E4E9", "Send for Discharge List");
			public static ResourceString QueryRemainingBillsforImpDec => ResString.GetMultilingualString("F195520A-5A3C-4015-9136-065A2AA5ABB6", "Query Remaining Bills for Imp. Dec.");
			public static ResourceString SendForComplementaryDec => ResString.GetMultilingualString("35CA78AE-6C23-4C3D-BCED-9554E8837387", "Send for Complementary Declaration");
		}
	}
}
