using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.Business.Testing
{
	internal class TestQuoteRateEntryValidation : BusinessObjectValidationTestCase
	{
		public void TestPendingQuoteForSameTradeLaneValidation()
		{
			var pendingQuote = Helper.NewQuote(Helper.NewOrgHeader());
			pendingQuote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");

			Factory.Save();

			var newQuote = Helper.NewQuote(pendingQuote.Header);
			var newEntry1 = newQuote.AddRateEntry("FCL", "SEA", "AUSYD", "INBOM", "", "20GP");

			Factory.Save();

			AssertEquals("No warnings present on new rate entry as different destination", 0, newEntry1.RowWarnings.Count());

			newEntry1.TI_DestinationLRC = "USLAX";

			AssertEquals("Warning present regarding pending matching quote", "Unaccepted quotes for this trade lane were found in quotations " + pendingQuote.TH_QuoteNumber, newEntry1.RowWarnings.GetFirstMessage());
		}

		public void TestPendingQuoteForSameTradeLaneValidationWithDifferentCompany()
		{
			var client = Helper.NewOrgHeader();

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "NEW";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company2.Branches.Add(branch1);
			Factory.Save();

			Quote pendingQuote;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				pendingQuote = Helper.NewQuote(client);
				pendingQuote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
				Factory.Save();
			}

			var newQuote = Helper.NewQuote(client);
			var newEntry = newQuote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			AssertEquals("No warnings present on new quote entry as different glbcompany", 0, newEntry.RowWarnings.Count());

			newQuote.TH_GC = pendingQuote.TH_GC;
			newEntry.Validation.ValidateTI_Mode();
			AssertEquals("Warning present regarding pending matching quote", "Unaccepted quotes for this trade lane were found in quotations " + pendingQuote.TH_QuoteNumber, newEntry.RowWarnings.GetFirstMessage());
		}

		public void TestIncoTermValidationForQuote()
		{
			var whsCharge = Helper.ChargeCodes.New("WHS111", "WHS111", FlatCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			var requiredFields = new AutoRatingRequiredFields { RequireIncoterm = true };
			RatingDataRegistry.Instance.QuotationsRequiredFields.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, requiredFields);
			var org = SetupOrgHeader();

			var quote = Helper.NewQuote(org);
			var entry1 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AU", "US", "", "20GP");
			var entry2 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AU", "US", "", "40GP");
			var entry3 = quote.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			var entry4 = quote.AddRateEntry(RatingConstants.RateCategory.WHS);

			entry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 100m;
			entry2.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 120m;
			entry3.RateLines[0].GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 10m;
			entry4.AddRateLine(whsCharge).GetCalculator<FlatCalculator>().BaseRate = 1m;

			quote.QuoteFormatEntries.LoadEntries();

			AssertEquals("Precondition: quote has 4 actual entries", 4, quote.SummaryRateEntries.Count);
			AssertEquals("Precondition: quote format collection groups entries so less entries appear", 3, quote.QuoteFormatEntries.Count);

			foreach (var quoteEntry in quote.AllEntries)
			{
				quoteEntry.Validation.ValidateTI_QuotePageIncoTerm();
			}

			var expectedError = $"Please enter a {entry1.TI_QuotePageIncoTermInfo.Description}.";

			AssertHasError(entry1.TI_QuotePageIncoTermInfo, expectedError);
			AssertHasError(entry2.TI_QuotePageIncoTermInfo, expectedError);
			AssertHasError(entry3.TI_QuotePageIncoTermInfo, expectedError);
			AssertNoError("WHS entries are not freight", entry4.TI_QuotePageIncoTermInfo, expectedError);

			quote = Helper.NewQuote(org);
			entry1 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "US", "", "20GP");
			entry2 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AU", "US", "", "40GP");
			entry3 = quote.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			entry4 = quote.AddRateEntry(RatingConstants.RateCategory.WHS);

			entry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 100m;
			entry2.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 120m;
			entry3.RateLines[0].GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 10m;
			entry4.AddRateLine(whsCharge).GetCalculator<FlatCalculator>().BaseRate = 1m;
			quote.QuoteFormatEntries.LoadEntries();

			foreach (QuoteEntry quoteEntry in quote.AllEntries)
			{
				quoteEntry.Validation.ValidateTI_QuotePageIncoTerm();
			}

			AssertHasError(entry1.TI_QuotePageIncoTermInfo, expectedError);
			AssertHasError(entry2.TI_QuotePageIncoTermInfo, expectedError);
			AssertHasError(entry3.TI_QuotePageIncoTermInfo, expectedError);
			AssertNoError(entry4.TI_QuotePageIncoTermInfo, expectedError);

			entry1.TI_QuotePageIncoTerm = Core.Constants.IncoTerms.ExWorks;
			entry2.TI_QuotePageIncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			entry3.TI_QuotePageIncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;

			foreach (QuoteEntry quoteEntry in quote.AllEntries)
			{
				quoteEntry.Validation.ValidateTI_QuotePageIncoTerm();
			}

			AssertNoError(entry1.TI_QuotePageIncoTermInfo, expectedError);
			AssertNoError(entry2.TI_QuotePageIncoTermInfo, expectedError);
			AssertNoError(entry3.TI_QuotePageIncoTermInfo, expectedError);
		}

		public void TestIncoTermValidationForQuote_OccursWithoutExplicitlyLoadingQuoteFormatEntries()
		{
			var whsCharge = Helper.ChargeCodes.New("WHS111", "WHS111", FlatCalculator.Code, ChargeCodeGroupList.Codes.WHSInwards);
			var requiredFields = new AutoRatingRequiredFields { RequireIncoterm = true };
			RatingDataRegistry.Instance.QuotationsRequiredFields.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, requiredFields);
			var org = SetupOrgHeader();

			var quote = Helper.NewQuote(org);
			var entry1 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AU", "GB", "", "20GP");
			var entry2 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AU", "GB", "", "40GP");
			var entry3 = quote.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			var entry4 = quote.AddRateEntry(RatingConstants.RateCategory.WHS);

			entry1.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 100m;
			entry2.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 120m;
			entry3.RateLines[0].GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 10m;
			entry4.AddRateLine(whsCharge).GetCalculator<FlatCalculator>().BaseRate = 1m;

			quote.RunPreSaveValidation();

			AssertEquals("Precondition: running pre-save validation loads QuoteFormatEntries", 3, quote.QuoteFormatEntries.Count);

			var expectedError = $"Please enter a {entry1.TI_QuotePageIncoTermInfo.Description}.";

			AssertHasError(entry1.TI_QuotePageIncoTermInfo, expectedError);
			AssertHasError(entry2.TI_QuotePageIncoTermInfo, expectedError);
			AssertHasError(entry3.TI_QuotePageIncoTermInfo, expectedError);
			AssertNoError("WHS entries are not freight so won't have this error", entry4.TI_QuotePageIncoTermInfo, expectedError);

			quote = Helper.NewQuote(org);
			var entry5 = quote.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "CN", "");
			quote.RunPreSaveValidation();

			AssertHasError("Saving should also trigger error (on the GUI this will stop the saving)", entry5.TI_QuotePageIncoTermInfo, expectedError);
		}

		OrgHeader SetupOrgHeader()
		{
			var org = Helper.NewOrgHeader();
			org.OH_IsConsignee = true;
			Factory.Save();
			return org;
		}

		#region Implementation

		protected TestHelper Helper
		{
			get { return fHelper ?? (fHelper = new TestHelper(Factory)); }
		}

		TestHelper fHelper;

		#endregion
	}
}
