using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateAttachmentCollection))]
	internal sealed class RateAttachmentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultsAddedOnLoadIfCountIsZero()
		{
			CoverPage1.TS_IsDefault = true;
			StandardPricingPage.TS_IsDefault = true;
			OneOffPricingPage.TS_IsDefault = true;
			TrailingPage1.TS_IsDefault = true;

			AssertEquals("Default SelectedPages.Count, shouldn't include Pricing Pages, although StandardPricingPage will be included", 3, TestQuote.SelectedPages.Count);
			AssertEquals("CoverPage1", CoverPage1.PK, TestQuote.SelectedPages[0].TA_TS);
			AssertEquals("StandardPricingPage", StandardPricingPage.PK, TestQuote.SelectedPages[1].TA_TS);
			AssertEquals("TrailingPage1", TrailingPage1.PK, TestQuote.SelectedPages[2].TA_TS);

			AssertPages("AvailablePages.Count", new[] { "Cover2", "OneOffPricing", "OneOffPricingMultipleCarriers", "TrailingPage2" }, TestQuote.AvailablePages);
		}

		public void TestDefaultsNotAddedOnLoadIfCountIsNotZero()
		{
			CoverPage1.TS_IsDefault = true;
			StandardPricingPage.TS_IsDefault = true;

			var existingAttachment = Factory.New<RateAttachment>();
			existingAttachment.TA_TH = TestQuote.PK;
			existingAttachment.TA_TS = StandardPricingPage.PK;

			AssertPages("SelectedPages", new[] { "StandardPricing" }, TestQuote.SelectedPages);
			AssertPages("AvailablePages", new[] { "Cover1", "Cover2", "OneOffPricing", "OneOffPricingMultipleCarriers", "TrailingPage1", "TrailingPage2" }, TestQuote.AvailablePages);
		}

		public void TestAddPagesToRateAttachmentCollection()
		{
			AssertPages("SelectedPages.Count", new[] { "StandardPricing" }, TestQuote.SelectedPages);
			AssertEquals("Standard Pricing Template", StandardPricingPage.PK, TestQuote.SelectedPages[0].TA_TS);
			AssertPages("AvailablePages.Count", new[] { "Cover2", "Cover1", "OneOffPricing", "OneOffPricingMultipleCarriers", "TrailingPage1", "TrailingPage2" }, TestQuote.AvailablePages);

			TestQuote.SelectedPages.AddPagesToSelectedPagesCollection(new RateAttachmentSet[]
			{
				CoverPage1,
				CoverPage2,
				TrailingPage1,
				TrailingPage2
			});
			AssertPages("SelectedPages.Count", new[] { "Cover2", "Cover1", "StandardPricing", "TrailingPage1", "TrailingPage2" }, TestQuote.SelectedPages);
			AssertPages("AvailablePages.Count", new[] { "OneOffPricing", "OneOffPricingMultipleCarriers" }, TestQuote.AvailablePages);
		}

		public void TestRemovePagesFromRateAttachmentCollection()
		{
			TestQuote.SelectedPages.AddPagesToSelectedPagesCollection(new RateAttachmentSet[]
			{
				CoverPage1,
				CoverPage2,
				TrailingPage1,
				TrailingPage2
			});
			AssertPages("SelectedPages.Count", new[] { "Cover1", "Cover2", "StandardPricing", "TrailingPage1", "TrailingPage2" }, TestQuote.SelectedPages);
			AssertPages("AvailablePages.Count", new[] { "OneOffPricing", "OneOffPricingMultipleCarriers" }, TestQuote.AvailablePages);

			TestQuote.SelectedPages.RemovePagesFromSelectedPagesCollection(new RateAttachment[]
			{
				FindRateAttachmentByRateAttachmentSetPK(CoverPage1.PK),
				FindRateAttachmentByRateAttachmentSetPK(TrailingPage1.PK)
			});
			AssertPages("SelectedPages.Count", new[] { "Cover2", "StandardPricing", "TrailingPage2" }, TestQuote.SelectedPages);
			AssertPages("AvailablePages.Count", new[] { "Cover1", "TrailingPage1", "OneOffPricing", "OneOffPricingMultipleCarriers" }, TestQuote.AvailablePages);
		}

		void AssertPages(string message, string[] expected, RateAttachmentSetCollection rateAttachmentSetCollection)
			=> AssertContainsExactElementsInAnyOrder(message, expected, rateAttachmentSetCollection.Select(rateAttachmentSet => rateAttachmentSet.TS_AttachmentName));

		void AssertPages(string message, string[] expected, RateAttachmentCollection rateAttachmentCollection)
			=> AssertContainsExactElementsInAnyOrder(message, expected, rateAttachmentCollection.Cast<RateAttachment>().Select(rateAttachment => rateAttachment.TA_RateAttachmentName));

		public void TestQuoteAttachmentNotification_MandatoryRemoved()
		{
			var mandatoryPage = TestQuote.AvailablePages.AddNew();
			mandatoryPage.TS_TemplateType = RatingConstants.DocTemplateTypes.CoverPage;
			mandatoryPage.TS_IsMandatory = true;

			var attached = TestQuote.SelectedPages.AddPagesToSelectedPagesCollection(new RateAttachmentSet[] { mandatoryPage });
			AssertEquals("Contains mandatory page after add", true, ContainsRateAttachmentByRateAttachmentSetPK(mandatoryPage.PK));

			TestQuote.QuoteAttachmentsNotifications += new QuoteAttachmentNotificationsEventHandler(OnQuoteAttachmentNotifications);
			TestQuote.SelectedPages.RemovePagesFromSelectedPagesCollection(new RateAttachment[] { attached[0] });
			AssertEquals("Should raise error event", QuoteAttachmentsErrorType.MandatoryRemoved, LastQuoteAttachmentNotification.ErrorType);
			AssertEquals("Still contains mandatory page after remove", true, ContainsRateAttachmentByRateAttachmentSetPK(mandatoryPage.PK));
		}

		public void TestQuoteAttachmentNotification_PricingTemplateRemoved()
		{
			AssertEquals("Contains pricing page", 1, TestQuote.SelectedPages.Count);
			AssertEquals("Contains pricing page", true, ContainsRateAttachmentByRateAttachmentSetPK(StandardPricingPage.PK));

			RateAttachment attachment = null;

			foreach (RateAttachment otherAttachment in TestQuote.SelectedPages)
			{
				if (otherAttachment.TA_TS == StandardPricingPage.PK)
				{
					attachment = otherAttachment;
				}
			}

			TestQuote.QuoteAttachmentsNotifications += new QuoteAttachmentNotificationsEventHandler(OnQuoteAttachmentNotifications);
			TestQuote.SelectedPages.RemovePagesFromSelectedPagesCollection(new RateAttachment[] { attachment });
			AssertEquals("Should raise error event", QuoteAttachmentsErrorType.PricingTemplateRemoved, LastQuoteAttachmentNotification.ErrorType);
			AssertEquals("Still contains mandatory pricing page after remove", true, ContainsRateAttachmentByRateAttachmentSetPK(StandardPricingPage.PK));
		}

		public void TestQuoteAttachmentNotification_WrongPricingTemplateAdded()
		{
			TestQuote.QuoteAttachmentsNotifications += new QuoteAttachmentNotificationsEventHandler(OnQuoteAttachmentNotifications);
			TestQuote.SelectedPages.AddPagesToSelectedPagesCollection(new RateAttachmentSet[] { OneOffPricingPage });
			AssertEquals("Should raise error event", QuoteAttachmentsErrorType.WrongPricingTemplateAdded, LastQuoteAttachmentNotification.ErrorType);
			AssertEquals("Still not contain wrong pricing page after add", false, ContainsRateAttachmentByRateAttachmentSetPK(OneOffPricingPage.PK));
		}

		public void TestSetupPricingPageSelectedPage()
		{
			CombineAssertions(() =>
			{
				TestQuote.TH_OneTimeQuote = true;
				AssertEquals("Enable TH_OneTimeQuote: Should not contain standard pricing page", false, ContainsRateAttachmentByRateAttachmentSetPK(StandardPricingPage.PK));
				AssertEquals("Enable TH_OneTimeQuote: Should contain one off pricing page", true, ContainsRateAttachmentByRateAttachmentSetPK(OneOffPricingPage.PK));
				AssertEquals("Enable TH_OneTimeQuote: Should not contain standard pricing page multiple carriers", false, ContainsRateAttachmentByRateAttachmentSetPK(OneOffPricingPageMultipleCarriers.PK));
				TestQuote.TH_OneTimeQuote = false;
				AssertEquals("Disable TH_OneTimeQuote: Should contain standard pricing page", true, ContainsRateAttachmentByRateAttachmentSetPK(StandardPricingPage.PK));
				AssertEquals("Disable TH_OneTimeQuote: Should not contain one off pricing page", false, ContainsRateAttachmentByRateAttachmentSetPK(OneOffPricingPage.PK));
				AssertEquals("Disable TH_OneTimeQuote: Should not contain standard pricing page multiple carriers", false, ContainsRateAttachmentByRateAttachmentSetPK(OneOffPricingPageMultipleCarriers.PK));
				TestQuote.TH_OneTimeQuote = true;
				AssertEquals("Re-enable TH_OneTimeQuote: Should not contain standard pricing page", false, ContainsRateAttachmentByRateAttachmentSetPK(StandardPricingPage.PK));
				AssertEquals("Re-enable TH_OneTimeQuote: Should contain one off pricing page", true, ContainsRateAttachmentByRateAttachmentSetPK(OneOffPricingPage.PK));
				AssertEquals("Re-enable TH_OneTimeQuote: Should not contain standard pricing page multiple carriers", false, ContainsRateAttachmentByRateAttachmentSetPK(OneOffPricingPageMultipleCarriers.PK));
			});
		}

		public void TestIsCorrectPricingPage()
		{
			var company1 = Factory.New<GlbCompany>();
			TestQuote.TH_OneTimeQuote = false;

			var rateAttachmentCollection = new RateAttachmentCollection(TestQuote);

			Factory.Save();
			rateAttachmentCollection.Load();
			AssertEquals("Should be equal StandardPricingPage", StandardPricingPage, rateAttachmentCollection[0].CurrentAttachment);

			StandardPricingPage.TS_GC = company1.PK;
			Factory.Save();
			rateAttachmentCollection.Load();
			AssertEquals("Count value should not be 0", 0, rateAttachmentCollection.Count);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RateAttachmentCollection(Factory.New<Quote>());
		}

		Quote TestQuote;
		RateAttachmentSet CoverPage1;
		RateAttachmentSet CoverPage2;
		RateAttachmentSet StandardPricingPage;
		RateAttachmentSet OneOffPricingPage;
		RateAttachmentSet OneOffPricingPageMultipleCarriers;
		RateAttachmentSet TrailingPage1;
		RateAttachmentSet TrailingPage2;
		QuoteAttachmentNotificationsEventArgs LastQuoteAttachmentNotification;

		protected override void SetUp()
		{
			base.SetUp();
			ClearRateAttachmentSetTable();
			TestQuote = Factory.New<Quote>();

			CoverPage1 = CreateRateAttachmentSet("Cover1", RatingConstants.DocTemplateTypes.CoverPage, (short)0, false);
			CoverPage2 = CreateRateAttachmentSet("Cover2", RatingConstants.DocTemplateTypes.CoverPage, (short)1, false);
			StandardPricingPage = CreateRateAttachmentSet("StandardPricing", RatingConstants.DocTemplateTypes.StandardPricingPage, (short)2, true);
			OneOffPricingPage = CreateRateAttachmentSet("OneOffPricing", RatingConstants.DocTemplateTypes.OneOffPricingPage, (short)3, false);
			OneOffPricingPageMultipleCarriers = CreateRateAttachmentSet("OneOffPricingMultipleCarriers", RatingConstants.DocTemplateTypes.OneOffMultiCarriersPricingPage, (short)4, false);
			TrailingPage1 = CreateRateAttachmentSet("TrailingPage1", RatingConstants.DocTemplateTypes.TrailingPage, (short)5, false);
			TrailingPage2 = CreateRateAttachmentSet("TrailingPage2", RatingConstants.DocTemplateTypes.TrailingPage, (short)6, false);

			Factory.Save();
		}

		void ClearRateAttachmentSetTable()
		{
			TestCaseHelper.ClearTable(RateAttachmentSchema.Constants.TableName);

			var availablePages = new RateAttachmentSetCollection(Factory);
			availablePages.Load();
			availablePages.RemoveAndDeleteAll();
			Factory.Save();
		}

		RateAttachmentSet CreateRateAttachmentSet(ZString name, ZString templateType, ZShort sequence, bool @default)
		{
			var newAttachmentSet = TestQuote.AvailablePages.AddNew();
			newAttachmentSet.TS_AttachmentName = name;
			newAttachmentSet.TS_IsDefault = @default;
			newAttachmentSet.TS_IsMandatory = false;
			newAttachmentSet.TS_TemplateType = templateType;
			newAttachmentSet.TS_Sequence = sequence;
			newAttachmentSet.TS_SU = Factory.LoadTop1<DocumentCommand>(new ZQuery(StmMenuItemSchema.SU_BusinessContext, DataContext.Quotation)).PK;
			return newAttachmentSet;
		}

		bool ContainsRateAttachmentByRateAttachmentSetPK(ZGuid rateAttachmentSetPK)
		{
			var filter = new ZQuery(RateAttachmentSchema.TA_TS, rateAttachmentSetPK);
			return TestQuote.SelectedPages.Find(filter).Length > 0;
		}

		RateAttachment FindRateAttachmentByRateAttachmentSetPK(ZGuid rateAttachmentSetPK)
		{
			var filter = new ZQuery(RateAttachmentSchema.TA_TS, rateAttachmentSetPK);
			var rateAttachments = TestQuote.SelectedPages.Find(filter);
			if (rateAttachments.Length > 1)
			{
				ErrorReporter.ReportOnce("RateAttachmentFoundMoreThanOne" + rateAttachmentSetPK, "Should not find more than 1");
			}
			return rateAttachments.Length == 1 ? (RateAttachment)rateAttachments[0] : null;
		}

		void OnQuoteAttachmentNotifications(object sender, QuoteAttachmentNotificationsEventArgs e)
		{
			LastQuoteAttachmentNotification = e;
		}

		#endregion
	}
}
