using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Rating.GUI
{
	#region IClientRateForm Interface

	public interface IClientRateForm : IZForm
	{
		void SetupFormForQuoteAcceptance(ZString quoteNumber);
	}

	#endregion

	public sealed partial class ActiveRatesForm : RatingForm, IFindBox, IClientRateForm
	{
		public ActiveRatesForm(ClientRate clientRate, bool shouldBeIncludedInRecentItems = true)
			: base(clientRate)
		{
			InitializeComponent();

			CurrentHeader.TH_OHInfo.ValueChanged += TH_OHInfo_ValueChanged;

			UpdateUnacceptedQuotesControls();

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			this.shouldBeIncludedInRecentItems = shouldBeIncludedInRecentItems;

			ratingTabControl1.WorkflowTabPage.Initialize(clientRate);
		}

		readonly bool shouldBeIncludedInRecentItems;

		protected override ZTabControl TopLevelTabControl
		{
			get { return ratingTabControl1.TopLevelTabControl; }
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			this.SetReadOnlyIncludingChildren(new[] { rateEntryFilterStripControl.Name }.ToList());
		}

		protected override void SaveToRecentItems()
		{
			if (shouldBeIncludedInRecentItems)
			{
				base.SaveToRecentItems();
			}
		}

		public new ClientRate CurrentHeader
		{
			get { return base.CurrentHeader as ClientRate; }
		}

		#region Data Transfer

		protected override IValueObjectDataAdapter ValueObjectDataAdapter
		{
			get { return new ClientRatesValueObjectDataAdapter(); }
		}

		#endregion

		#region Quotation Accept Form

		public void SetupFormForQuoteAcceptance(ZString quoteNumber)
		{
			ControllerID = null;
			Text = Res.GetString("5657aa23-4bd0-4331-b88d-72a9dc9b8958", "Accept Quote {0}", quoteNumber.TrimStart('0'));
			PostingButtonsUserControl.SaveButton.Visible = false;
			PostingButtonsUserControl.SaveAndCloseButton.Text = Res.GetString("702461a6-b410-41c7-bfa2-7a30ff13d5fd", "Accept");
			if (!CurrentHeader.TH_NewRateEndDate.IsEmpty)
			{
				RateEndDateEdit.Visible = true;
			}
			UnacceptedQuotesButton.Visible = false;
			UnacceptedQuotesWarningLabel.Visible = false;
		}

		#endregion

		#region Unaccepted Quotes

		EmbeddedModulePopup UnacceptedQuotesPopup;
		void UnacceptedQuotesButton_Click(object sender, EventArgs e)
		{
			UpdateUnacceptedQuotesControls();
			if (CurrentHeader.UnacceptedQuotesCollection.Count > 0)
			{
				var quoteModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Quotations);
				quoteModule.OverrideModuleDecisionProvider(new PopupModuleDecisionProvider(this));
				UnacceptedQuotesPopup = new EmbeddedModulePopup(quoteModule);
				UnacceptedQuotesPopup.ShowModal(this, this);
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("9dbd7709-e81a-402b-9222-252f31d48b99", "There are no unaccepted quotes for this client."), Res.GetString("05659ea6-c3b9-4969-873e-13ff9814cc8b", "Unaccepted Quotes"));
			}
		}

		void UpdateUnacceptedQuotesControls()
		{
			UnacceptedQuotesWarningLabel.Visible = CurrentHeader.UnacceptedQuotesCollection.Count > 0;
		}

		void TH_OHInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateUnacceptedQuotesControls();
		}

		#region IFindBox Members

		// We trick ZArchitecture into thinking this form is a findbox so that we can display Unaccepted quotes
		// Properties return "" in some cases as we don't care about what is chosen in the popup - it has no
		// impact on the main rate form. We only display quotes for viewing purposes.

		public IFindBoxPopup PopupForm
		{
			get { return UnacceptedQuotesPopup; }
		}

		public IFindBoxListProvider ListProvider
		{
			get { return CurrentHeader.UnacceptedQuotesCollection; }
		}

		public string Description
		{
			get { return ""; }
			set { }
		}

		public string Code
		{
			get { return ""; }
			set { }
		}

		#endregion

		#endregion

		#region Dispose

		readonly System.ComponentModel.Container components;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				CurrentHeader.TH_OHInfo.ValueChanged -= TH_OHInfo_ValueChanged;

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

