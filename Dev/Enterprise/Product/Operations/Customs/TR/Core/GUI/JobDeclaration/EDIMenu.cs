using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.GUI.MessagingProcess;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get => (JobDeclaration)base.Declaration;
			set => base.Declaration = value;
		}

		MenuItem exportUnionDeclarationsItemMenuItem;

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			exportUnionDeclarationsItemMenuItem.Visible = Declaration != null && Declaration.IsExport;
		}

		protected override bool DisplayGenerateEntriesMenuOption => true;

		protected override void AddAdditionalMenuItems()
		{
			AddSendControlMessageItem();
			AddExportUnionDeclarationsItem();
			AddSendRegistrationMessageItem();
		}

		#region Send Control Message

		void AddSendControlMessageItem()
		{
			MenuItems.Add(new ZMenuItem(ZMenuItem.Separator));
			var sendRegistrationMenuItem = new ZMenuItem(ResString.GetMultilingualString("F2549C01-A1BB-4305-AD07-91C398876349", "Send Control Message"));
			sendRegistrationMenuItem.Click += SendControlMenuItem_Click;
			MenuItems.Add(sendRegistrationMenuItem);
		}

		void SendControlMenuItem_Click(object sender, EventArgs e)
		{
			var declaration = Declaration;
			if (declaration != null && MergeAndPreSave(declaration) && CheckValuationDate(declaration) && CheckExchangeRate(declaration))
			{
				SendEntryHeaderMessage(declaration.CusEntryHeader, TRMessageTypes.Codes.DKO);
			}
		}

		#endregion

		#region Export Union Declarations

		void AddExportUnionDeclarationsItem()
		{
			exportUnionDeclarationsItemMenuItem = new ZMenuItem(ResString.GetMultilingualString("B866D1B8-9B71-4F8F-80DC-DB231F766025", "Export Union Declaration"));
			exportUnionDeclarationsItemMenuItem.Click += ExportUnionDeclarationsMenuItem_Click;
			MenuItems.Add(exportUnionDeclarationsItemMenuItem);
		}

		void ExportUnionDeclarationsMenuItem_Click(object sender, EventArgs e)
		{
			var declaration = Declaration;
			if (declaration != null && CheckValuationDate(declaration) && MergeAndPreSave(declaration))
			{
				SendEntryHeaderMessage(declaration.CusEntryHeader, TRMessageTypes.Codes.EUT);
			}
		}

		#endregion

		#region Send Registration Message

		void AddSendRegistrationMessageItem()
		{
			var sendRegistrationMenuItem = new ZMenuItem(ResString.GetMultilingualString("A195729B-6178-4978-B397-8F6BC5088F8B", "Send Registration Message"));
			sendRegistrationMenuItem.Click += SendRegistrationMenuItem_Click;
			MenuItems.Add(sendRegistrationMenuItem);
		}

		void SendRegistrationMenuItem_Click(object sender, EventArgs e)
		{
			var declaration = Declaration;
			if (declaration != null && MergeAndPreSave(declaration) && CheckValuationDate(declaration) && CheckExchangeRate(declaration))
			{
				SendEntryHeaderMessage(declaration.CusEntryHeader, TRMessageTypes.Codes.DTE);
			}
		}

		#endregion

		bool SendEntryHeaderMessage(CusEntryHeader entryHeader, string messageType)
		{
			var providerFactory = new TRCustomsMessagingProviderFactory(CusEntryHeaderCustomsMessagingProvider.New, messageType);

			return TRCustomsMessagingGui.SendMessages(entryHeader, providerFactory, Form).Success;
		}

		bool MergeAndPreSave(JobDeclaration declaration)
		{
			var needMerge = declaration != null && (!declaration.IsMergeDone || declaration.MergeManager.RequiresMerge);
			return (!needMerge || PerformMerge()) && PreSaveDeclaration(declaration);
		}

		bool CheckValuationDate(JobDeclaration declaration)
		{
			var result = true;

			if (declaration != null && declaration.JE_ValuationDate != ZDate.Today)
			{
				result = false;

				var response = Globals.Message.Show(Res.GetString("782D46CF-51B1-4980-B431-DD1D27441DDD", "The Valuation date is different from Today, do you confirm to update the Valuation Date?"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Information, DialogResult.Yes);
				if (response == DialogResult.Yes)
				{
					declaration.JE_ValuationDate = ZDate.Today;
					result = true;
				}
			}

			return result;
		}

		bool CheckExchangeRate(JobDeclaration declaration)
		{
			var result = ZBool.True;
			if (declaration.Invoices.Count > 0)
			{
				var rateConverter = declaration.Invoices[0].CurrencyConverter;
				var actualRate = CurrencyHelper.GetExchangeRate(rateConverter, Core.Constants.CurrencyCodes.UnitedStates);
				if (actualRate.IsEmpty)
				{
					Globals.Message.ShowError(Res.GetString("D20F0229-9C3A-47C1-9E94-C682F14CE58B", "There is no exchange rate for today; the duty calculations can not be done correctly, and message sending is aborted.Please enter exchange rates for today."));
					result = ZBool.False;
				}
			}

			return result;
		}
	}
}
