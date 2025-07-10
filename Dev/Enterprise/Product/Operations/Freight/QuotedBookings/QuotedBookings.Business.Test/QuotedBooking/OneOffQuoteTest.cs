using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBooking))]
	class OneOffQuoteTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCanCancel_ApprovedQuote() => AssertNullOrEmpty("CanCancel", ((ICancellable)CreateApprovedOneOffQuote()).CanCancel());

		public void TestCanCancel_ActiveQuote() => AssertNullOrEmpty("CanCancel", ((ICancellable)CreateActiveOneOffQuote()).CanCancel());

		public void TestCanCancel_AcceptedQuote() => AssertNullOrEmpty("CanCancel", ((ICancellable)CreateAcceptedOneOffQuote()).CanCancel());

		public void TestCanCancel_ClientAcceptedQuote() => AssertEquals("CanCancel", "The selected one off quote has already been accepted by the client and cannot be deactivated.", ((ICancellable)CreateClientAcceptedOneOffQuote(clientAcceptedDateTime: ZDateTime.Today)).CanCancel());

		public void TestCanCopyAsAmendment_ActiveQuote() => AssertEquals(true, CreateActiveOneOffQuote().CanCopyAsOneOffQuoteAmendment);

		public void TestCanCopyAsAmendment_ApprovedQuote() => AssertEquals(true, CreateApprovedOneOffQuote().CanCopyAsOneOffQuoteAmendment);

		public void TestCanCopyAsAmendment_AcceptedQuote() => AssertEquals(true, CreateAcceptedOneOffQuote().CanCopyAsOneOffQuoteAmendment);

		public void TestCanCopyAsAmendment_FinalPrintedQuote() => AssertEquals(true, CreateFinalPrintedOneOffQuote().CanCopyAsOneOffQuoteAmendment);

		public void TestCanCopyAsAmendment_BookingWithQuote() => AssertEquals(false, CreateBookingWithQuote().CanCopyAsOneOffQuoteAmendment	);

		public void TestCanCopyAsAmendment_QuoteUsed() => AssertEquals(false, CreateConsumedOneOffQuote().CanCopyAsOneOffQuoteAmendment);

		public void TestOneOffQuoteValidateAllOnSaveFromQuotedBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, Guid.Empty, Factory);

			// Validation is suspended so that the error we check comes from
			// The ValidateAll rather than setting the entry value.
			// This is to simulate an invalid value already being present in the
			// database when the form loads.
			using (quotedBooking.Quote.CurrentOneOffQuote.GetValidationSuspender())
			{
				quotedBooking.Quote.CurrentOneOffQuote.TT_NumberOfEntries = -444;
			}
			AssertNoErrors(quotedBooking);

			quotedBooking.Validation.ValidateAll();
			AssertCollectionContains("Error - TT_NumberOfEntries: Number of Entries cannot be negative.", quotedBooking.GetErrors().Select(x => x.Message));
		}

		public void TestOneOffQuoteHasCountryRulesValidationNoteType()
		{
			var rule = Factory.NewWithValidTestData<RefCountryRules>();
			rule.R7_RN_NKOrigin = "AU";
			rule.R7_RN_NKDestination = "DE";
			rule.R7_Notes = "This is a client visible Notes";
			rule.R7_IsClientVisible = ZBool.True;
			rule.R7_IsError = ZBool.True;
			rule.R7_IsValidationRule = ZBool.True;
			Factory.Save();

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_FullName = "Consignor";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_Code = "CONSIGNOR1";
			consignor.OH_IsConsignor = true;
			Factory.Save();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = "DEHAM";
			consignee.OH_Code = "CONSIGNEE1";
			consignee.OH_IsConsignee = true;
			Factory.Save();

			var client = Factory.New<OrgHeader>();
			client.OH_Code = "NTC1";
			client.OH_FullName = "New Test Client";
			client.OH_RL_NKClosestPort = "AUSYD";

			client.OH_IsDebtor = true;
			client.OH_IsConsignor = true;
			client.OH_IsConsignee = true;

			var address = client.MainAddress;
			address.OA_Address1 = "123 Fake Street";
			address.OA_City = "Sydney";
			address.OA_State = "NSW";
			address.OA_PostCode = "2000";
			Factory.Save();

			var oneOffQuote = CreateQuotedBooking(Factory, "LSE", "CFR", client, consignor, consignee, null, "AUSYD", "DEHAM", 1m, 1m, QuotedBookingState.QuoteOnly);
			oneOffQuote.RunPreSaveValidation();

			var notes = oneOffQuote.Notes;
			var correctNoteText = PredefinedNoteTypes.Instance.CountryRulesValidation.Description;
			var note = notes.FindByDescription(correctNoteText).FirstOrDefault();
			AssertNoErrors(note.ST_DescriptionInfo);
			var noteText = note.ST_NoteText;
			Assert(noteText.Contains("<Reason>Australia to Germany:</Reason>\r\n<IsError>Y</IsError>"));
		}

		public QuotedBooking CreateQuotedBooking(BusinessObjectFactory factory, string mode, string paymentTerms, OrgHeader client, OrgHeader consignor, OrgHeader consignee, OrgHeader carrier, string origin, string destination, decimal weight, decimal volume, QuotedBookingState quotedBookingState)
		{
			var quotePK = quotedBookingState == QuotedBookingState.QuoteOnly || quotedBookingState == QuotedBookingState.AcceptedBookingWithQuote
				? QuotedBooking.CreateNewQuote(factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK
				: Guid.Empty;

			var bookingPK = quotedBookingState == QuotedBookingState.BookingOnly || quotedBookingState == QuotedBookingState.AcceptedBookingWithQuote
				? QuotedBooking.CreateNewBooking(factory).PK
				: Guid.Empty;

			var quotedBooking = QuotedBooking.New(quotePK, bookingPK, factory);
			if (quotedBooking.ClientDocAddress != null && client?.MainAddress != null)
			{
				quotedBooking.ClientDocAddress.E2_OA_Address = client.MainAddress.PK;
			}

			if (consignor != null)
			{
				if (!string.IsNullOrEmpty(origin))
				{
					consignor.OH_RL_NKClosestPort = origin;
				}

				if (quotedBooking.Booking != null)
				{
					quotedBooking.Booking.ConsignorPickupAddress.E2_OA_Address = consignor.MainAddress.PK;
				}

				quotedBooking.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			}

			if (consignee != null)
			{
				if (!string.IsNullOrEmpty(destination))
				{
					consignee.OH_RL_NKClosestPort = destination;
				}

				if (quotedBooking.Booking != null)
				{
					quotedBooking.Booking.ConsigneeDeliveryAddress.E2_OA_Address = consignee.MainAddress.PK;
				}

				quotedBooking.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			}

			if (carrier != null)
			{
				quotedBooking.OH_Carrier = carrier.PK;
			}

			quotedBooking.Mode = mode;
			quotedBooking.PaymentTerms = paymentTerms;
			quotedBooking.Origin = origin;
			quotedBooking.Destination = destination;
			quotedBooking.Weight = weight;
			quotedBooking.Volume = volume;
			factory.Save();
			return quotedBooking;
		}

		#region Implementation

		void DisposeJobs(QuotedBooking quotedBooking)
		{
			if (quotedBooking != null && quotedBooking.Job != null)
			{
				quotedBooking.Job.Dispose();
			}
		}

		protected virtual bool IsLoadDischargeDefaultedFromOriginDestination => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			currentTestQuotedBooking = CreateNewQuotedBooking(quote.PK, booking.PK);
			return currentTestQuotedBooking;
		}

		protected virtual QuotedBooking CreateNewQuotedBooking(ZGuid quotePK, ZGuid bookingPK) => QuotedBooking.New(quotePK, bookingPK, Factory);

		protected override void TearDown()
		{
			DisposeCurrentTestQuotedBookingJobHeaderMutexes();
			base.TearDown();
		}

		void DisposeCurrentTestQuotedBookingJobHeaderMutexes()
		{
			DisposeJobs(currentTestQuotedBooking);
		}

		QuotedBooking currentTestQuotedBooking;

		#endregion

		#region Helpers

		QuotedBooking CreateApprovedOneOffQuote()
		{
			var oneOffQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			Factory.Save();

			AssertEquals("Precondition: QuoteStatus", Quote.QuoteStatusOptions.Approved, oneOffQuote.Quote.QuoteStatus);

			return oneOffQuote;
		}

		QuotedBooking CreateActiveOneOffQuote()
		{
			bool oldValue = DataRegistryRating.Instance.QuoteRequireInternalApproval.Value;
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var oneOffQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			var quote = oneOffQuote.Quote;
			quote.ShowApprovalDialog += new EventHandler<Quote.ApprovalDialogEventArgs>(delegate(object sender, Quote.ApprovalDialogEventArgs e)
			{ e.Cancel = true; });

			Factory.Save();
			DataRegistryRating.Instance.QuoteRequireInternalApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldValue);

			AssertEquals("Precondition: QuoteStatus", Quote.QuoteStatusOptions.Active, oneOffQuote.Quote.QuoteStatus);

			return oneOffQuote;
		}

		QuotedBooking CreateAcceptedOneOffQuote()
		{
			var oneOffQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			oneOffQuote.Quote.TH_Accepted = ZDateTime.Today;
			Factory.Save();

			AssertEquals("Precondition: QuoteStatus", Quote.QuoteStatusOptions.Accepted, oneOffQuote.Quote.QuoteStatus);

			return oneOffQuote;
		}

		QuotedBooking CreateClientAcceptedOneOffQuote(ZDateTime clientAcceptedDateTime)
		{
			var oneOffQuote = QuotedBooking.New(Integration.QuoteBookingType.SpotQuote, Factory);
			oneOffQuote.Quote.TH_ClientAccepted = clientAcceptedDateTime;
			Factory.Save();

			AssertEquals("Precondition: QuoteStatus", Quote.QuoteStatusOptions.ClientAccepted, oneOffQuote.Quote.QuoteStatus);

			return oneOffQuote;
		}

		QuotedBooking CreateFinalPrintedOneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.TH_IsLocked = true;
			var oneOffQuote = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			oneOffQuote.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			AssertEquals("Precondition: QuoteStatus", Quote.QuoteStatusOptions.Accepted, oneOffQuote.Quote.QuoteStatus);

			return oneOffQuote;
		}

		QuotedBooking CreateBookingWithQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var oneOffQuote = QuotedBooking.New(quote.PK, booking.PK, Factory);
			oneOffQuote.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();

			AssertEquals("Precondition: QuoteStatus", Quote.QuoteStatusOptions.Accepted, oneOffQuote.Quote.QuoteStatus);

			return oneOffQuote;
		}

		QuotedBooking CreateConsumedOneOffQuote()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			quote.TH_IsOneOffQuoteConsumed = true;
			var oneOffQuote = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			oneOffQuote.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			return oneOffQuote;
		}

		#endregion
	}
}
