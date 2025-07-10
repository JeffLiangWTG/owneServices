using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Rating;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgOpportunityDocumentSupporter))]
	sealed class OrgOpportunityDocumentSupporterTest : DocumentSupporterTest
	{
		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<OrgOpportunity>();
		}

		public void TestNoContacts()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";

			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.AssignedOrgPK = org.PK;
			var docSupporter = new OrgOpportunityDocumentSupporter(opportunity);

			AssertNoExceptionThrown(delegate
			{
				docSupporter.GetContactOrganisation(ZString.Empty, ContactType.NoContactType, DocumentDirection.ANY);
			});
		}

		public void TestGetContactOrganisation_Client()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";
			var contact = org.Contacts.AddNew();

			var org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "Some Enquiry Org 2";
			org2.MainAddress.OA_Address1 = "2 Street";
			org2.OH_RL_NKClosestPort = "AUSYD";

			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.P8_OC = contact.PK;
			opportunity.AssignedOrgPK = org2.PK;
			var docSupporter = new OrgOpportunityDocumentSupporter(opportunity);

			AssertEquals(org, docSupporter.GetContactOrganisation(ZString.Empty, ContactType.NoContactType, DocumentDirection.ANY).OrgHeader);
		}

		public void TestGetContactOrganisation_Assigned()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_FullName = "Some Enquiry Org 2";
			org2.MainAddress.OA_Address1 = "2 Street";
			org2.OH_RL_NKClosestPort = "AUSYD";

			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.AssignedOrgPK = org2.PK;
			var docSupporter = new OrgOpportunityDocumentSupporter(opportunity);

			AssertEquals(org2, docSupporter.GetContactOrganisation(ZString.Empty, ContactType.ControllingAgent, DocumentDirection.ANY).OrgHeader);
		}

		public new void TestRunningDocumentsShouldNotCauseException()
		{
			Assert(true);
		}

		public void TestSequenceForQuotation()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";

			var opportunity = org.SalesOpportunities.AddNew();
			var docSupporter = new OrgOpportunityDocumentSupporter(opportunity);
			var pivots = opportunity.RelatedChildActivityPivotCollection;

			var oneOffQuote5 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			(oneOffQuote5 as ICancellable).IsCancelled = true;
			var rating5 = oneOffQuote5.Quote as IRatingHeader;
			rating5.TH_QuoteNumber = "005";
			rating5.TH_OH = org.PK;
			var pivot5 = pivots.AddNew();
			pivot5.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot5.RAP_ChildActivityTableCode = ViewQuotedBookingSchema.Constants.Prefix;
			pivot5.RAP_ChildActivityID = oneOffQuote5.ViewPK;

			var rating4 = Factory.New<IRatingHeader>();
			rating4.TH_QuoteNumber = "004";
			rating4.TH_RateType = "QTE";
			rating4.TH_OH = org.PK;
			(rating4 as ICancellable).IsCancelled = true;
			var pivot4 = pivots.AddNew();
			pivot4.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot4.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot4.RAP_ChildActivityID = rating4.PK;

			var rating3 = Factory.New<IRatingHeader>();
			rating3.TH_QuoteNumber = "003";
			rating3.TH_RateType = "QTE";
			rating3.TH_OH = org.PK;
			var pivot3 = pivots.AddNew();
			pivot3.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot3.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot3.RAP_ChildActivityID = rating3.PK;

			var oneOffQuote2 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			var rating2 = oneOffQuote2.Quote as IRatingHeader;
			rating2.TH_QuoteNumber = "002";
			rating2.TH_OH = org.PK;
			var pivot2 = pivots.AddNew();
			pivot2.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot2.RAP_ChildActivityTableCode = ViewQuotedBookingSchema.Constants.Prefix;
			pivot2.RAP_ChildActivityID = oneOffQuote2.ViewPK;

			var rating1 = Factory.New<IRatingHeader>();
			rating1.TH_QuoteNumber = "001";
			rating1.TH_RateType = "QTE";
			rating1.TH_OH = org.PK;
			var pivot1 = pivots.AddNew();
			pivot1.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot1.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot1.RAP_ChildActivityID = rating1.PK;

			var resultList = docSupporter.GetChildCollection(null, BusinessContext.Quotation, null);

			AssertEquals("Only 3 nodes should be found", 3, resultList.Length);
			AssertEquals("1st node should be 001", "001", (resultList[0] as IRatingHeader).TH_QuoteNumber);
			AssertEquals("2nd node should be 002", "002", ((resultList[1] as IQuotedBooking).Quote as IRatingHeader).TH_QuoteNumber);
			AssertEquals("3rd node should be 003", "003", (resultList[2] as IRatingHeader).TH_QuoteNumber);
		}

		public void TestExcludeRatingsFromOtherCompanies()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Some Enquiry Org";
			org.MainAddress.OA_Address1 = "1 Street";
			org.OH_RL_NKClosestPort = "AUSYD";
			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "AAA";
			var opportunity = org.SalesOpportunities.AddNew();
			var pivots = opportunity.RelatedChildActivityPivotCollection;

			var oneOffQuote4 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			var rating4 = oneOffQuote4.Quote as IRatingHeader;
			rating4.TH_QuoteNumber = "004";
			rating4.TH_OH = org.PK;
			rating4.TH_GC = Env.CurrentCompanyPK;
			var pivot4 = pivots.AddNew();
			pivot4.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot4.RAP_ChildActivityTableCode = ViewQuotedBookingSchema.Constants.Prefix;
			pivot4.RAP_ChildActivityID = oneOffQuote4.ViewPK;

			var rating3 = Factory.New<IRatingHeader>();
			rating3.TH_QuoteNumber = "003";
			rating3.TH_OH = org.PK;
			rating3.TH_GC = Env.CurrentCompanyPK;
			rating3.TH_RateType = "QTE";
			var pivot3 = pivots.AddNew();
			pivot3.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot3.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot3.RAP_ChildActivityID = rating3.PK;

			var oneOffQuote2 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			var rating2 = oneOffQuote2.Quote as IRatingHeader;
			rating2.TH_QuoteNumber = "002";
			rating2.TH_OH = org.PK;
			rating2.TH_GC = otherCompany.PK;
			var pivot2 = pivots.AddNew();
			pivot2.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot2.RAP_ChildActivityTableCode = ViewQuotedBookingSchema.Constants.Prefix;
			pivot2.RAP_ChildActivityID = oneOffQuote2.ViewPK;

			var rating1 = Factory.New<IRatingHeader>();
			rating1.TH_QuoteNumber = "001";
			rating1.TH_OH = org.PK;
			rating1.TH_GC = otherCompany.PK;
			rating1.TH_RateType = "QTE";
			var pivot1 = pivots.AddNew();
			pivot1.RAP_ParentActivityTableCode = OrgOpportunitySchema.Constants.Prefix;
			pivot1.RAP_ChildActivityTableCode = RatingHeaderSchema.Constants.Prefix;
			pivot1.RAP_ChildActivityID = rating1.PK;

			var doc = new OrgOpportunityDocumentSupporter(opportunity);
			var resultList = doc.GetChildCollection(null, BusinessContext.Quotation, null);

			AssertEquals("Only 2 nodes should be found", 2, resultList.Length);
			AssertEquals("1st node should be 003", "003", (resultList[0] as IRatingHeader).TH_QuoteNumber);
			AssertEquals("2nd node should be 004", "004", ((resultList[1] as IQuotedBooking).Quote as IRatingHeader).TH_QuoteNumber);
		}

		#endregion
	}
}
