using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateAttachmentCollection : DependentBusinessObjectCollection<RateAttachment, Quote>
	{
		public RateAttachmentCollection(Quote master)
			: base(master)
		{
		}

		new Quote Master
		{
			get { return base.Master; }
		}

		#region BusinessObjectCollection Overrides

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public bool RateAttachmentAllowSort;
		protected override bool AllowSort
		{
			get { return RateAttachmentAllowSort; }
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			base.Load(alternativeAdditionalFilter);
			if (Count == 0)
			{
				LoadDefaults();
			}

			foreach (RateAttachment selectedPage in this)
			{
				if (Master.AvailablePages.Contains(selectedPage.CurrentAttachment))
				{
					Master.AvailablePages.Remove(selectedPage.CurrentAttachment);
				}
			}
			SortBothCollections();

			SetupPricingPageSelectedPage(Master.TH_OneTimeQuote);
			HasChanges = false;
			Master.AvailablePages.HasChanges = false;
		}

		#endregion

		#region Default Pricing Page

		public void SetupPricingPageSelectedPage(bool isOneOff)
		{
			var hasCorrectPricingPage = false;
			var pagesToRemove = new List<RateAttachment>();
			foreach (RateAttachment attachment in this)
			{
				if (attachment.CurrentAttachment != null)
				{
					if (IsCorrectPricingPage(attachment.CurrentAttachment, isOneOff, false))
					{
						hasCorrectPricingPage = true;
					}
					else if (RatingConstants.DocTemplateTypes.IsPricingPage(attachment.CurrentAttachment.TS_TemplateType))
					{
						pagesToRemove.Add(attachment);
					}
				}
			}
			RemovePagesFromSelectedPagesCollection(pagesToRemove, true);

			if (!hasCorrectPricingPage)
			{
				var attachmentSets = Factory.Load<RateAttachmentSet>(new ZQuery());
				foreach (var set in attachmentSets)
				{
					if (IsCorrectPricingPage(set, isOneOff, true))
					{
						AddPagesToSelectedPagesCollection((new RateAttachmentSet[] { set }));
						break;
					}
				}
			}
		}

		bool IsCorrectPricingPage(RateAttachmentSet set, bool isOneOff, bool defaultOnly)
		{
			var result = false;

			if (isOneOff)
			{
				result = set.TS_IsOneOffPricingPage;
			}
			else if (set.TS_IsStandardPricingPage && (!defaultOnly || set.TS_IsDefault))
			{
				result = true;
			}

			result &= set.TS_GC.IsEmpty || set.TS_GC == Master.TH_GC;

			return result;
		}

		#endregion

		#region Add To Selected Pages

		public List<RateAttachment> AddPagesToSelectedPagesCollection(IList<RateAttachmentSet> pagesToAdd)
		{
			return AddPagesToSelectedPagesCollection(pagesToAdd, false);
		}

		protected List<RateAttachment> AddPagesToSelectedPagesCollection(IList<RateAttachmentSet> pagesToAdd, bool suppressNotifications)
		{
			var result = new List<RateAttachment>();
			var triedToAddWrongPricingPage = false;

			foreach (var pageToAdd in pagesToAdd)
			{
				if (!suppressNotifications &&
					((pageToAdd.TS_IsStandardPricingPage && Master.TH_OneTimeQuote) ||
					(pageToAdd.TS_IsOneOffPricingPage && !Master.TH_OneTimeQuote)))
				{
					triedToAddWrongPricingPage = true;
				}
			}

			if (triedToAddWrongPricingPage)
			{
				Master.OnQuoteAttachmentNotifications(new QuoteAttachmentNotificationsEventArgs(QuoteAttachmentsErrorType.WrongPricingTemplateAdded));
			}
			else
			{
				foreach (var pageToAdd in pagesToAdd)
				{
					var newAttachment = AddNew();
					newAttachment.TA_TS = pageToAdd.PK;

					if (Master.AvailablePages.Contains(pageToAdd))
					{
						Master.AvailablePages.Remove(pageToAdd);
					}

					SortBothCollections();
					result.Add(newAttachment);
				}
			}

			return result;
		}

		#endregion

		#region Remove From Selected Pages

		public void RemovePagesFromSelectedPagesCollection(IList<RateAttachment> pagesToRemove)
		{
			RemovePagesFromSelectedPagesCollection(pagesToRemove, false);
		}

		protected void RemovePagesFromSelectedPagesCollection(IList<RateAttachment> pagesToRemove, bool suppressNotifications)
		{
			var lastQuoteTemplateAttemptedRemoval = false;
			var mandatoryItems = new List<RateAttachment>();

			foreach (var pageToRemove in pagesToRemove)
			{
				var newAvailablePage = pageToRemove.CurrentAttachment;
				if (!suppressNotifications && pageToRemove.TA_IsMandatory)
				{
					mandatoryItems.Add(pageToRemove);
				}
				else if (
					!suppressNotifications && RatingConstants.DocTemplateTypes.IsPricingPage(pageToRemove.CurrentAttachment.TS_TemplateType) &&
					NumberOfQuoteTemplates == 1)
				{
					lastQuoteTemplateAttemptedRemoval = true;
				}
				else
				{
					if (Contains(pageToRemove))
					{
						RemoveAndDelete(pageToRemove);
					}
					Master.AvailablePages.Add(newAvailablePage);
				}
			}

			if (mandatoryItems.Count > 0)
			{
				Master.OnQuoteAttachmentNotifications(new QuoteAttachmentNotificationsEventArgs(QuoteAttachmentsErrorType.MandatoryRemoved, mandatoryItems));
			}

			if (lastQuoteTemplateAttemptedRemoval)
			{
				Master.OnQuoteAttachmentNotifications(new QuoteAttachmentNotificationsEventArgs(QuoteAttachmentsErrorType.PricingTemplateRemoved));
			}

			SortBothCollections();
		}

		int NumberOfQuoteTemplates
		{
			get
			{
				var count = 0;
				foreach (RateAttachment page in Master.SelectedPages)
				{
					if (RatingConstants.DocTemplateTypes.IsPricingPage(page.CurrentAttachment.TS_TemplateType))
					{
						count++;
					}
				}

				return count;
			}
		}

		#endregion

		#region Implementation

		protected void LoadDefaults()
		{
			var filter = new ZQuery(RateAttachmentSetSchema.TS_IsDefault, ZBool.True);
			var defaultQuoteDocumentPages = (RateAttachmentSet[])Master.AvailablePages.Find(filter);
			Array.Sort(defaultQuoteDocumentPages, new RateAttachmentComparer());

			foreach (var defaultQuoteDocumentPage in defaultQuoteDocumentPages)
			{
				var newAttachment = AddNew();
				newAttachment.TA_TS = defaultQuoteDocumentPage.PK;
			}

			HasChanges = false;
		}

		protected void SortBothCollections()
		{
			Master.AvailablePages.RateAttachmentSetAllowSort = true;
			RateAttachmentAllowSort = true;

			Sort(new RateAttachmentComparer());
			Master.AvailablePages.Sort(new RateAttachmentComparer());

			Master.AvailablePages.RateAttachmentSetAllowSort = false;
			RateAttachmentAllowSort = false;
		}

		#endregion

		#region Testing Methods

		public bool Contains(ZString attachmentName)
		{
			foreach (RateAttachment rateAttachment in this)
			{
				if (rateAttachment.TA_RateAttachmentName == attachmentName)
				{
					return true;
				}
			}

			return false;
		}

		#endregion
	}
}

