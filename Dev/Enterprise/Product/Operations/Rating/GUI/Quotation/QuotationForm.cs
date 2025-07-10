using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI
{
	#region IQuotationForm Interface

	public interface IQuotationForm : IZForm
	{
		void SetupFormForQuoteCancellation();
	}

	#endregion

	public partial class QuotationForm : RatingForm, IQuotationForm
	{
		public QuotationForm()
		{
			InitializeComponent();
		}

		public QuotationForm(Quote quote)
			: base(quote)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				if (!quote.TH_OH.IsEmpty && quote.TH_GS_NKFirstSignatory.IsEmpty)
				{
					quote.SetDefaultSignatures();
				}

				if (quote.IsCancelled)
				{
					quotationDateControl1.ShowQuoteCancellationReasonDropEdit();
				}

				CurrentHeader.SetFinalMode += new EventHandler(CurrentHeader_SetFinalMode);
				CurrentHeader.NoTradeLanesToPrint += new EventHandler(CurrentHeader_NoTradeLanesToPrint);
				CurrentHeader.MinimumCostMarkupNotMet += new Quote.MinimumCostMarkupNotMetEventHandler(ShowMinimumCostMarkupNotMetDialog);
				CurrentHeader.ValidateDocument += ValidateQuote;
				CurrentHeader.ShowApprovalDialog += new EventHandler<Quote.ApprovalDialogEventArgs>(CurrentHeader_ShowApprovalDialog);
				CurrentHeader.ShowApprovalMessage += new Quote.ApprovalMessageEventHandler(CurrentHeader_ShowApprovalMessage);
				CurrentHeader.QuoteApprovalSecurityNotGranted += new EventHandler<Quote.QuoteApprovalSecurityEventArgs>(CurrentHeader_QuoteApprovalSecurityNotGranted);

				CurrentHeader.HookupNoteAddedListener();

				ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);

				PlugIns.Add(ControllerIDs.eDocsPlugIn);
				PlugIns.Add(ControllerIDs.DocDataPlugIn);

				QuotationTabControl1.WorkflowTabPage.Initialize(quote);
			}
		}

		#region Data Transfer

		protected override IValueObjectDataAdapter ValueObjectDataAdapter
		{
			get { return new QuotationValueObjectDataAdapter(); }
		}

		#endregion

		#region Implementation

		protected override ZTabControl TopLevelTabControl
		{
			get { return this.QuotationTabControl1.TopLevelTabControl; }
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			this.SetReadOnlyIncludingChildren(new[] { rateEntryFilterStripControl.Name }.ToList());
		}

		public new Quote CurrentHeader
		{
			get { return base.CurrentHeader as Quote; }
		}

		#endregion

		#region Print Quote Controls

		/// <summary>
		/// Allow the 'Print Quote' functionality to be available in all display modes.
		/// </summary>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			PrintQuoteButton.Enabled = true;
			if (CurrentHeader.TH_IsLocked)
			{
				PrintQuoteButton.Text = Res.GetString("QuotationForm|PrintQuoteButton|Reprint", "Re-print");
			}

			if (!DesignModeFinder.IsDesigning)
			{
				tabConfigurationManager = new TabConfigurationManager(null, QuotationTabControl1.TopLevelTabControl);
				tabConfigurationManager.Enabled = true;
			}
		}

		TabConfigurationManager tabConfigurationManager;

		#endregion

		#region Cancel Quote

		public void SetupFormForQuoteCancellation()
		{
			CurrentHeader.CancelQuote();
			Text = Res.GetString("671c63d2-81ff-4498-b8f9-440c04db5b75", "Cancel Quote") + " " + CurrentHeader.TH_QuoteNumber.Trim('0');
			PostingButtonsUserControl.SaveButton.Visible = false;
			PostingButtonsUserControl.SaveAndCloseButton.Text = Res.GetString("10dcdfc2-f825-4dd7-8c43-3ae211da2ca4", "Confirm");
			quotationDateControl1.ShowQuoteCancellationReasonDropEdit();
		}

		#endregion

		#region Approve Quote

		void ApproveQuoteButton_Click(object sender, EventArgs e)
		{
			CurrentHeader.InternalApproveQuote();
		}

		void CurrentHeader_ShowApprovalDialog(object sender, Quote.ApprovalDialogEventArgs e)
		{
			string message = "";
			string caption = Res.GetString("3c429e4d-762a-4f9e-ab43-239504232c98", "Approval");

			switch (e.Dialog)
			{
				case Quote.ApprovalDialog.WishToApprove:
					message = Res.GetString("d8380ca4-7c49-4458-94cd-b13dc054249c",
@"Marking a quotation as Approved means that it can be printed in Final mode, and can be used for Auto-Rating purposes.

Do you wish to Approve this quote?");
					break;

				case Quote.ApprovalDialog.WishToApproveWhenAccepting:
					message = Res.GetString("f8f2ff8f-55e3-4149-83f0-5a153033155e",
@"This Quotation is not internally approved.
It must be internally Approved first before it can be Accepted.

Do you wish to Approve this quote first?");
					break;

				case Quote.ApprovalDialog.WishToApproveWhenFinalizing:
					message = Res.GetString("5fae37e6-7b42-43d7-bcbf-b340e4c4c6e0",
@"This Quotation is not internally approved.
Marking a quotation as Approved means that it can be printed in Final mode, and can be used for Auto-Rating purposes.

Do you wish to Approve this quote for printing in Final mode? Otherwise this quote will be printed in Draft mode.");
					break;

				case Quote.ApprovalDialog.WishToApproveWhenSaving:
					message = Res.GetString("3363cd0f-91e8-4bce-8517-89343a3a8a6f",
@"This Quotation is not internally approved.
Marking a quotation as Approved means that it can be printed in Final mode, and can be used for Auto-Rating purposes.

Do you wish to Approve this quote?");
					break;
			}

			if (!string.IsNullOrEmpty(message) && Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				e.Cancel = true;
			}
		}

		void CurrentHeader_ShowApprovalMessage(Quote.ApprovalMessage approvalMessage)
		{
			string message = "";

			switch (approvalMessage)
			{
				case Quote.ApprovalMessage.MissingLocalClientMessage:
					message = Res.GetString("77f4e4a4-4aa0-47b7-ab4b-3ef6ab06cab1",
						"A local client or overseas agent is required to approve this Quotation.");
					Globals.Message.ShowError(message);
					return;

				case Quote.ApprovalMessage.MissingOverseasAgentMessage:
					message = Res.GetString("33529a8a-cff8-41c1-b357-9ef81f45e659",
						"An overseas agent is required to approve this Quotation.");
					Globals.Message.ShowError(message);
					return;

				case Quote.ApprovalMessage.AlreadyApprovedMessage:
					message = Res.GetString("ffc87fbd-70fc-42f8-a68f-153a79ae0a04",
						"This Quotation is already approved.");
					break;

				case Quote.ApprovalMessage.HaveNoRightsMessage:
					message = Res.GetString("672530f9-3caa-42ea-999f-30ef7ee5709c",
						"The login details entered do not have security rights to approve Quotations.");
					break;

				case Quote.ApprovalMessage.LoginFailedMessage:
					message = Res.GetString("a8c17ff2-ff4b-4b86-bab3-5ad99e5c1644",
						"The login details entered are incorrect or password is expired.");
					break;
			}

			if (!string.IsNullOrEmpty(message))
			{
				Globals.Message.Show(message);
			}
		}

		void CurrentHeader_QuoteApprovalSecurityNotGranted(object sender, Quote.QuoteApprovalSecurityEventArgs e)
		{
			string message = Res.GetString("5b13bb81-fffc-4dac-94f3-711927657283",
@"You do not have the appropriate security rights to mark this Quotation as Approved.
If you do not mark this quotation as approved you cannot use it for Auto Rating Purposes and it can only be printed in Draft Mode.

You can either continue, or have a user with higher security rights enter their credentials.

Do you wish to have a user with higher rights enter their credentials?");

			string caption = Res.GetString("dd892808-5f8f-448f-929b-aa760e9c0c4f", "Approval");

			if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Hand) != DialogResult.Yes)
			{
				e.Cancel = true;
			}
			else
			{
				if (ZFormModaliser.ShowDialogAndDispose(new LoginForm(e.OverrideLogin)) != DialogResult.OK)
				{
					e.Cancel = true;
				}
			}
		}

		#endregion

		#region Quotation Document

		void PrintQuoteButton_Click(object sender, EventArgs e)
		{
			var documentSupporter = (Quote.QuoteDocumentSupporter)((IDocumentSupportable)CurrentHeader).DocumentSupporter;
			var documentCommand = DocumentCommand.GetDocumentCommand(CurrentHeader.Factory, CurrentHeader, Core.Constants.MenuNameConstantsForPrinting.QuotationPack);

			if (documentCommand != null)
			{
				documentCommand.Parent = CurrentHeader;

				var task = documentSupporter.BuildPrintTask(documentCommand);
				if (task != null)
				{
					documentSupporter.RunTask(task);
				}
			}
		}

		void CurrentHeader_SetFinalMode(object sender, EventArgs e)
		{
			DialogResult finalModeDialogResult = Globals.Message.Show(Res.GetString("e5d74bce-eeb4-4055-bad0-ab1504675bc3", "Do you want to print this {0} in Final mode? This will mark this {1} as locked and final.\r\n\r\nClick Yes for final, and No for draft.", Env.Registry.Rating.QuoteTitleText, Env.Registry.Rating.QuoteTitleText),
				Res.GetString("fb220a27-3e88-4bcd-9e61-8b46a4274e32", "Print Mode"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			((Quote.SetFinalModeArgs)e).Result = finalModeDialogResult == DialogResult.Yes;
		}

		void CurrentHeader_NoTradeLanesToPrint(object sender, EventArgs e)
		{
			Globals.Message.Show(Res.GetString("a9987c3f-0e80-467e-9ed4-7f496eb26b11", "There are no trade lanes to print."));
		}

		#endregion

		#region Validation

		bool ValidateQuote(RatingHeader header)
		{
			bool validationFailed = false;
			ZString errorMessage = "";

			if (DisplayMode != ODisplayMode.ReadOnly)
			{
				if (header.TH_QuoteNumber.IsEmpty)
				{
					errorMessage = Res.GetString("fed53e36-55da-41e6-bfbc-2e6a2a2beb28", "Please save your quotation before printing.");
					validationFailed = true;
				}
				else
				{
					BusinessEntity.RunPreSaveValidation();
					if (BusinessEntity.HasErrors())
					{
						ShowErrorsDialog();
						BusinessEntity.HasChanges = true;   // allow save button to be clicked
						validationFailed = true;
					}
				}
			}

			if (validationFailed && !errorMessage.IsEmpty)
			{
				Globals.Message.ShowError(errorMessage, Res.GetString("e86a2dde-6223-47d5-99c8-796f0d4f178c", "Print"));
			}

			return !validationFailed;
		}

		#endregion

		#region Cost Markup

		static public bool ShowMinimumCostMarkupNotMetDialog()
		{
			DialogResult result = Globals.Message.Show(Res.GetString("15390688-2fb9-4e40-8670-3303c15ff1b7", "Some rates in this quotation have not met the minimum markup on costs. Do you wish to continue with these rates?"), Res.GetString("27ef55a4-b37a-482d-ba05-ba9b08a871a8", "Cost Markup Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Error);
			if (result == DialogResult.Yes)
			{
				if (Env.Security.QuotationAllowRateBelowCostMarkup.IsAllowed)
				{
					return true;
				}
				else
				{
					SecurityOverridenLogin login = new SecurityOverridenLogin();
					if (ZFormModaliser.ShowDialogAndDispose(new LoginForm(login)) == DialogResult.OK)
					{
						if (login.UserSecurity == null)
						{
							Globals.Message.Show(Res.GetString("defeaa16-c1bf-4626-80c8-5208b892f706", "The login details entered are incorrect or password is expired."));
						}
						else if (!login.UserSecurity.QuotationAllowRateBelowCostMarkup.IsAllowed)
						{
							Globals.Message.Show(Res.GetString("90ddd000-23b1-4e0d-93ab-7eca597a457f", "The login details entered do not have security rights to specify rates that are below the recommended cost markup."));
						}
						else
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		#endregion

		#region IDisposable Members

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				CurrentHeader.SetFinalMode -= new EventHandler(CurrentHeader_SetFinalMode);
				CurrentHeader.NoTradeLanesToPrint -= new EventHandler(CurrentHeader_NoTradeLanesToPrint);
				CurrentHeader.MinimumCostMarkupNotMet -= new Quote.MinimumCostMarkupNotMetEventHandler(ShowMinimumCostMarkupNotMetDialog);
				CurrentHeader.ValidateDocument -= ValidateQuote;
				CurrentHeader.ShowApprovalDialog -= new EventHandler<Quote.ApprovalDialogEventArgs>(CurrentHeader_ShowApprovalDialog);
				CurrentHeader.ShowApprovalMessage -= new Quote.ApprovalMessageEventHandler(CurrentHeader_ShowApprovalMessage);
				CurrentHeader.QuoteApprovalSecurityNotGranted -= new EventHandler<Quote.QuoteApprovalSecurityEventArgs>(CurrentHeader_QuoteApprovalSecurityNotGranted);

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}

