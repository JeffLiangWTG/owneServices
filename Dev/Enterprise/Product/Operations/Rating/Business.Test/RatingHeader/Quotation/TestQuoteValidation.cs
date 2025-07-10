using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.Testing
{
	internal class TestQuoteValidation : BusinessObjectValidationTestCase
	{
		#region TH_QuoteCancellationReason

		public void TestValidateTH_QuoteCancellationReason()
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			quote.Validation.ValidateTH_QuoteCancellationReason();
			AssertNoErrors("Quote is not cancelled: No error", quote.TH_QuoteCancellationReasonInfo);

			Factory.Save();

			quote.TH_QuoteCancellationReason = "AAA";
			quote.Validation.ValidateTH_QuoteCancellationReason();
			AssertNoErrors("Quote is not cancelled: No error", quote.TH_QuoteCancellationReasonInfo);

			quote.CancelQuote();
			quote.Validation.ValidateTH_QuoteCancellationReason();
			AssertHasErrors("Quote is cancelled: Error of invalid code", quote.TH_QuoteCancellationReasonInfo);

			quote.TH_QuoteCancellationReason = "";
			quote.Validation.ValidateTH_QuoteCancellationReason();
			AssertNoErrors("Quote is cancelled: No error because code is not mandatory", quote.TH_QuoteCancellationReasonInfo);

			RatingDataRegistry.Instance.IsQuoteCancellationReasonCodeRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			quote.Validation.ValidateTH_QuoteCancellationReason();
			AssertHasErrors("Quote is cancelled: Error because code is mandatory", quote.TH_QuoteCancellationReasonInfo);

			var list = new CodeDescriptionBoolCollection();
			list.Add("DDD", (NoResString)"Test", true);
			RatingDataRegistry.Instance.QuoteCancellationReasonCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			quote.TH_QuoteCancellationReason = "DDD";
			quote.Validation.ValidateTH_QuoteCancellationReason();
			AssertNoErrors("Quote is cancelled: No error because it is valid code", quote.TH_QuoteCancellationReasonInfo);

			Factory.Save();

			list = new CodeDescriptionBoolCollection();
			list.Add("AAA", (NoResString)"Test 2", true);
			RatingDataRegistry.Instance.QuoteCancellationReasonCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			quote.Validation.ValidateTH_QuoteCancellationReason();
			AssertNoErrors("Quote is cancelled: No error for quote already cancelled", quote.TH_QuoteCancellationReasonInfo);
		}

		#endregion

		#region Signatures

		public void TestSignatures()
		{
			var org1 = Helper.NewOrgHeader();

			var rep = Factory.New<GlbStaff>();
			rep.GS_FullName = "Test Rep";
			rep.GS_Code = "TR";
			org1.StaffAssignments.OverallSalesRep = rep.GS_Code;

			var testQuote = Factory.New<Quote>();
			testQuote.Validation.ValidateTH_GS_NKFirstSignatory();
			testQuote.Validation.ValidateTH_GS_NKSecondSignatory();
			AssertHasError(testQuote.TH_GS_NKFirstSignatoryInfo, "Please enter a First Signatory.");
			AssertNoErrors(testQuote.TH_GS_NKSecondSignatoryInfo);

			testQuote.TH_OH = org1.PK;
			testQuote.TH_GS_NKSecondSignatory = GlbStaff.CurrentUser.GS_Code;
			AssertNoErrors(testQuote.TH_GS_NKFirstSignatoryInfo);
			AssertNoErrors(testQuote.TH_GS_NKSecondSignatoryInfo);

			testQuote.TH_GS_NKFirstSignatory = "###";
			testQuote.TH_GS_NKSecondSignatory = "###";
			AssertHasError(testQuote.TH_GS_NKFirstSignatoryInfo, "Enter a valid " + testQuote.TH_GS_NKFirstSignatoryInfo.Description + ".");
			AssertHasError(testQuote.TH_GS_NKSecondSignatoryInfo, "Enter a valid " + testQuote.TH_GS_NKSecondSignatoryInfo.Description + ".");
		}

		public void TestSignatories_GivenOneOffQuote_ShouldNotValidate()
		{
			var org1 = Helper.NewOrgHeader();
			var rep = Factory.New<GlbStaff>();
			rep.GS_FullName = "Test Rep";
			rep.GS_Code = "TR";
			org1.StaffAssignments.OverallSalesRep = rep.GS_Code;

			var testQuote = Factory.New<Quote>();
			testQuote.TH_OneTimeQuote = true;
			testQuote.TH_OH = org1.PK;
			testQuote.TH_GS_NKFirstSignatory = string.Empty;
			testQuote.TH_GS_NKSecondSignatory = string.Empty;
			Factory.Save();

			testQuote.Validation.ValidateTH_GS_NKFirstSignatory();
			testQuote.Validation.ValidateTH_GS_NKSecondSignatory();
			AssertNoErrors(testQuote.TH_GS_NKFirstSignatoryInfo);
			AssertNoErrors(testQuote.TH_GS_NKSecondSignatoryInfo);
		}

		#endregion

		#region Helper

		protected TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}
}
