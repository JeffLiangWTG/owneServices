using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class DocumentSelectionControl : ZUserControl
	{
		public DocumentSelectionControl()
		{
			InitializeComponent();
		}

		public void DisableTwoButtonsWhenQuotedBookingIsForwardRegistered()
		{
			this.AddSelectedPagesButton.Enabled = false;
			this.RemoveSelectedPagesButton.Enabled = false;
		}

		#region Implementation

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (CurrentQuote != null)
			{
				CurrentQuote.QuoteAttachmentsNotifications -= new QuoteAttachmentNotificationsEventHandler(OnCurrentQuote_AttachmentsNotification);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentQuote != null)
			{
				CurrentQuote.QuoteAttachmentsNotifications += new QuoteAttachmentNotificationsEventHandler(OnCurrentQuote_AttachmentsNotification);
			}
		}

		void SelectedAvailablePagesGrid_Enter(object sender, EventArgs e)
		{
			if (sender == SelectedPagesGrid)
			{
				if (!SelectedImagesPreviewControl.Visible)
				{
					SelectedImagesPreviewControl.Visible = true;
				}
				if (AvailableImagesPreviewControl.Visible)
				{
					AvailableImagesPreviewControl.Visible = false;
				}
			}
			else if (sender == AvailablePagesGrid)
			{
				if (SelectedImagesPreviewControl.Visible)
				{
					SelectedImagesPreviewControl.Visible = false;
				}
				if (!AvailableImagesPreviewControl.Visible)
				{
					AvailableImagesPreviewControl.Visible = true;
				}
			}
		}

		void AddSelectedPagesButton_Click(object sender, EventArgs e)
		{
			List<RateAttachmentSet> availablePages = new List<RateAttachmentSet>();

			foreach (RateAttachmentSet attachmentSet in AvailablePagesGrid.SelectedElements)
			{
				availablePages.Add(attachmentSet);
			}

			CurrentQuote.SelectedPages.AddPagesToSelectedPagesCollection(availablePages);
		}

		void RemoveSelectedPagesButton_Click(object sender, EventArgs e)
		{
			List<RateAttachment> selectedPages = new List<RateAttachment>();
			foreach (RateAttachment attachment in SelectedPagesGrid.SelectedElements)
			{
				selectedPages.Add(attachment);
			}

			CurrentQuote.SelectedPages.RemovePagesFromSelectedPagesCollection(selectedPages);
		}

		void SelectedPagesGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && e.Clicks == 2)
			{
				DataGrid.HitTestInfo info = SelectedPagesGrid.HitTest(e.Location);

				if (info != null && info.Row >= 0 && info.Row < SelectedPagesGrid.List.Count)
				{
					RateAttachment[] sets = new RateAttachment[] { (RateAttachment)SelectedPagesGrid.List[info.Row] };
					CurrentQuote.SelectedPages.RemovePagesFromSelectedPagesCollection(sets);
				}
			}
		}

		void AvailablePagesGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && e.Clicks == 2)
			{
				DataGrid.HitTestInfo info = AvailablePagesGrid.HitTest(e.Location);

				if (info != null && info.Row >= 0 && info.Row < AvailablePagesGrid.List.Count)
				{
					RateAttachmentSet[] sets = new RateAttachmentSet[] { (RateAttachmentSet)AvailablePagesGrid.List[info.Row] };
					CurrentQuote.SelectedPages.AddPagesToSelectedPagesCollection(sets);
				}
			}
		}

		void OnCurrentQuote_AttachmentsNotification(object sender, QuoteAttachmentNotificationsEventArgs args)
		{
			string caption = null;
			string message = null;
			if (args.ErrorType == QuoteAttachmentsErrorType.MandatoryRemoved)
			{
				caption = Res.GetString("75dca463-6dc2-4b5f-84e8-d5c851805f81", "Can't Remove Mandatory Pages");
				message = Res.GetString("7349ac84-b572-44af-af73-a6c6efaaa454", "The following page(s) cannot be removed because they are mandatory.") + "\r\n\r\n";

				foreach (RateAttachment mandatoryAttachment in args.MandatoryAttachments)
				{
					message += "   " + mandatoryAttachment.TA_RateAttachmentName;
				}
			}
			else if (args.ErrorType == QuoteAttachmentsErrorType.PricingTemplateRemoved)
			{
				caption = Res.GetString("e0967784-7e4b-49df-8e2c-cfe661d9a528", "Pricing Page is required");
				message = Res.GetString("c519ec03-7535-45e0-9fb6-c240be170abd", "A pricing page must be included. To remove this one, first add another pricing page.");
			}
			else if (args.ErrorType == QuoteAttachmentsErrorType.WrongPricingTemplateAdded)
			{
				caption = Res.GetString("7a5d43b6-5cc4-4afd-b6d1-183ce58b796d", "Incorrect pricing page type");
				message = Res.GetString("80ced16d-09ef-4eb5-8e91-0c83e9404e97", "You can only add a standard pricing page to a standard quote and a one-off pricing page to a one-off quote.");
			}
			if (message != null)
			{
				Globals.Message.ShowInformation(message, caption);
			}
		}

		Quote CurrentQuote
		{
			get { return (Quote)CurrentDataItem; }
		}

		#endregion
	}
}
