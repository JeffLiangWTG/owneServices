using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Tracking.Business.Testing
{
	public class QuoteDocumentServiceTest : TestCaseWithFactory
	{
		public void TestPrint_NoContact()
		{
			var quotedBooking = GetQuotedBooking();
			Factory.Save();

			var result = service.Print(Guid.NewGuid(), quotedBooking.PK.ToGuid());

			AssertNull(result);
		}

		public void TestPrint_NoQuote()
		{
			var contact = GetContact();
			Factory.Save();

			var result = service.Print(contact.PK.ToGuid(), Guid.NewGuid());

			AssertNull(result);
		}

		public void TestPrint_QuoteNotRelatedToContact()
		{
			using (WebDataRegistry.Instance.SaveQuotesWithoutRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var contact = GetContact();
				var quotedBooking = GetQuotedBooking();
				Assert("Precondition", !quotedBooking.Quote.ValidOneOffQuoteChargesExist);
				Factory.Save();

				var result = service.Print(contact.PK.ToGuid(), quotedBooking.PK.ToGuid());

				AssertNull(result);
			}
		}

		public void TestPrint_QuoteNoCompany()
		{
			using (WebDataRegistry.Instance.SaveQuotesWithoutRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var contact = GetContact();
				var quotedBooking = GetQuotedBooking(contact.ParentOrg);
				quotedBooking.Quote.TH_FollowUpDate = ZDateTime.Empty;
				quotedBooking.Quote.TH_GC = ZGuid.Empty;
				Factory.Save();

				var result = service.Print(contact.PK.ToGuid(), quotedBooking.PK.ToGuid());

				AssertNull(result);
			}
		}

		public void TestPrint_NoRates()
		{
			using (WebDataRegistry.Instance.SaveQuotesWithoutRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var contact = GetContact();
				var quotedBooking = GetQuotedBooking(contact.ParentOrg);
				Assert("Precondition", !quotedBooking.Quote.ValidOneOffQuoteChargesExist);
				Factory.Save();

				var result = service.Print(contact.PK.ToGuid(), quotedBooking.PK.ToGuid());

				AssertEquals("No matching rates were found. Please contact your sales representative to obtain a Spot Quote.", result.ErrorMessage);
			}
		}

		public void TestPrint_NoSellAmt()
		{
			using (WebDataRegistry.Instance.SaveQuotesWithoutRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var contact = GetContact();
				var quotedBooking = GetQuotedBooking(contact.ParentOrg);
				var job = new JobHeader.Loader(quotedBooking.Quote).TryCreate();
				var charge = Factory.NewWithValidTestData<JobCharge>();
				charge.JR_JH = job.PK;
				charge.JR_AC = Env.Registry.FreightChargeCode;
				Factory.Save();

				var result = service.Print(contact.PK.ToGuid(), quotedBooking.PK.ToGuid());

				AssertEquals("No matching rates were found. Please contact your sales representative to obtain a Spot Quote.", result.ErrorMessage);
			}
		}

		public void TestPrint()
		{
			using (WebDataRegistry.Instance.SaveQuotesWithoutRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var contact = GetContact();
				var quotedBooking = GetQuotedBooking(contact.ParentOrg);
				Assert("Precondition", !quotedBooking.Quote.ValidOneOffQuoteChargesExist);
				Factory.Save();

				var result = service.Print(contact.PK.ToGuid(), quotedBooking.PK.ToGuid());

				CombineAssertions(() =>
				{
					AssertEquals($"Quote {quotedBooking.Quote.TH_QuoteNumber}.pdf", result.FileName);
					Assert(result.FileContents.Length > 0);
				});
			}
		}

		public void TestPrint_ContactIsRelatedParty()
		{
			using (WebDataRegistry.Instance.SaveQuotesWithoutRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				string[] partTypesToTest =
				[
					RelatedPartyTypeList.Codes.ShipperBroker,
					RelatedPartyTypeList.Codes.ControllingCustomer,
					RelatedPartyTypeList.Codes.ManagementGrouping
				];

				var contact = GetContact();

				foreach (var partyType in partTypesToTest)
				{
					var relatedOrg = GetRelatedParty(contact.ParentOrg, partyType);
					var quotedBooking = GetQuotedBooking(relatedOrg);
					Assert("Precondition", !quotedBooking.Quote.ValidOneOffQuoteChargesExist);
					Factory.Save();

					var result = service.Print(contact.PK.ToGuid(), quotedBooking.PK.ToGuid());

					CombineAssertions(() =>
					{
						AssertNotNull($"Quote printed by {partyType}", result);
						AssertEquals($"Quote {quotedBooking.Quote.TH_QuoteNumber}.pdf", result.FileName);
						Assert(result.FileContents.Length > 0);
					});
				}
			}
		}

		public void TestPrint_ValidSellAmt_UsesQuoteBranch()
		{
			using (WebDataRegistry.Instance.SaveQuotesWithoutRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var contact = GetContact();
				var quotedBooking = GetQuotedBooking(contact.ParentOrg);
				var job = new JobHeader.Loader(quotedBooking.Quote).TryCreate();
				var charge = Factory.NewWithValidTestData<JobCharge>();
				charge.JR_JH = job.PK;
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_OSSellAmt = 100m;
				Factory.Save();

				var demBranchPK = new Guid("2FDBA7FB-60BA-4A03-8336-0DEFAC4F9673");
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, demBranchPK, Env.CurrentDepartmentPK))
				{
					var result = service.Print(contact.PK.ToGuid(), quotedBooking.PK.ToGuid());

					CombineAssertions(() =>
					{
						AssertEquals($"Quote {quotedBooking.Quote.TH_QuoteNumber}.pdf", result.FileName);
						Assert(result.FileContents.Length > 0);
					});
				}
			}
		}

		OrgContact GetContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = org.OH_Code;

			return org.Contacts.AddNew();
		}

		OrgHeader GetRelatedParty(OrgHeader relatedOrg, string relatedPartyType)
		{
			var relatedPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = Factory.NewWithValidTestData<OrgRelatedParty>();
			relatedParty.PR_PartyType = relatedPartyType;
			relatedParty.PR_OH_RelatedParty = relatedOrg.PK;
			relatedParty.PR_OH_Parent = relatedPartyOrg.PK;
			relatedParty.PR_FreightDirection = ZString.Empty;
			relatedParty.PR_Location = ZString.Empty;

			return relatedPartyOrg;
		}

		QuotedBooking GetQuotedBooking(OrgHeader org = null)
		{
			var addressPK = org?.MainAddress.PK ?? Factory.NewWithValidTestData<OrgAddress>().PK;
			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OneTimeQuote = true;
			quote.TH_QuoteNumber = $"{quoteNumber++}";
			quote.QuotationClientAddress.E2_OA_Address = addressPK;
			var viewQuotedBooking = Factory.NewWithValidTestData<ViewQuotedBooking>();
			viewQuotedBooking.VB_TH = quote.PK;

			return QuotedBooking.New(viewQuotedBooking, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			service = new QuoteDocumentsService();
		}
		QuoteDocumentsService service;
		static int quoteNumber = 1;
	}
}
