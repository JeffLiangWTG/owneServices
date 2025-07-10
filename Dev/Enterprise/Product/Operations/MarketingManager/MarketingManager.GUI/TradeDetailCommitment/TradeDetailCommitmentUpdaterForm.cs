using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class TradeDetailCommitmentUpdaterForm : ZChildForm
	{
		[Obsolete("This just for designer.")]
		public TradeDetailCommitmentUpdaterForm()
		{
			InitializeComponent();
		}

		public TradeDetailCommitmentUpdaterForm(TradeDetailCommitmentUpdater updater)
			: base(updater)
		{
			InitializeComponent();
		}

		public override string FormVerb => string.Empty;

		TradeDetailCommitmentUpdater Updater => BusinessEntity as TradeDetailCommitmentUpdater;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!Updater.TradeDetailItems.Cast<TradeDetailCommitmentItem>().Any(x => !x.IsSuperceded))
			{
				confirmButton.Enabled = false;
			}

			EstimateValueExpiryMenuItemBuilder.SetupContextMenu(tradeDetailsGrid, GetSelectedTradeDetails);
		}

		IEnumerable<OrgTradeDetail> GetSelectedTradeDetails()
		{
			IEnumerable<OrgTradeDetail> selectedDetails = Enumerable.Empty<OrgTradeDetail>();
			if (tradeDetailsGrid.ListManager != null)
			{
				if (tradeDetailsGrid.SelectedElements.Length > 0)
				{
					selectedDetails = tradeDetailsGrid.SelectedElements.Cast<TradeDetailCommitmentItem>().Select(x => x.TradeDetail);
				}
				else
				{
					var current = tradeDetailsGrid.ListManager.GetCurrent() as TradeDetailCommitmentItem;
					if (current != null)
					{
						selectedDetails = new[] { current.TradeDetail };
					}
				}
			}

			return selectedDetails;
		}

		void ConfirmButton_Click(object sender, EventArgs e)
		{
			if (Updater != null)
			{
				Updater.RunPreSaveValidation();
				if (Updater.HasErrors)
				{
					ShowErrorsDialog();
				}
				else
				{
					bool canConfirmChange = true;
					var itemsWarningMessage = Updater.SupercedingWarningMessage;
					if (!itemsWarningMessage.IsEmpty)
					{
						canConfirmChange = (DialogResult.Yes == TradeDetailCommitmentUpdaterGUIManager.ShowSupercedingConfirmation(itemsWarningMessage));
					}

					if (canConfirmChange)
					{
						Updater.ConfirmChange();
						DialogResult = DialogResult.OK;
						Close();
					}
				}
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
