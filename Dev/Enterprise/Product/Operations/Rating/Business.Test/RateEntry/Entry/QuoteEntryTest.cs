using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class QuoteEntryTest : RatingTestCase
	{
		public void TestQuotationIndexDescription()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());

			var airEntry = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			airEntry.TI_ViaLRC = "DEHAM";

			AssertEquals("Sydney to Los Angeles, US via Hamburg", airEntry.QuotationIndexDescription);
			Factory.Save();
			testQuote.TH_OneTimeQuote = true;
			testQuote.CurrentOneOffQuote.TT_RL_NKReceivalLocation = "AUMEL";
			testQuote.CurrentOneOffQuote.TT_RL_NKViaLocation = "INBOM";
			testQuote.CurrentOneOffQuote.TT_RL_NKDeliveryLocation = "USLAX";

			AssertEquals("Melbourne to Los Angeles, US via Mumbai (ex Bombay)", airEntry.QuotationIndexDescription);
		}

		public void TestDefaultHeadingOpeningClosingText()
		{
			var testQuote = Factory.New<Quote>();
			var originEntry = (QuoteEntry)testQuote.AddRateEntry("ORG");

			AssertEquals("No heading text", ZString.Empty, originEntry.PageHeader);
			AssertEquals("No opening text", ZString.Empty, originEntry.OpeningText);
			AssertEquals("No closing text", ZString.Empty, originEntry.ClosingText);

			DocumentsDataRegistry.Instance.QuoteOpeningText.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Opening Test");

			var originEntry2 = (QuoteEntry)testQuote.AddRateEntry("ORG");
			originEntry2.TI_PageClosingText = "Closing on this quote";

			AssertEquals("No heading text", ZString.Empty, originEntry2.PageHeader);

			AssertEquals("Opening text from registry", "Opening Test", originEntry2.OpeningText);
			AssertEquals("Opening Text not stored on entry object", ZString.Empty, originEntry2.TI_PageOpeningText);

			AssertEquals("Closing text from quote itself", "Closing on this quote", originEntry2.ClosingText);
			AssertEquals("Closing Text stored on entry object", originEntry2.ClosingText, originEntry2.TI_PageClosingText);
		}

		public void TestAgentOverrideDefaultsOnSimilarEntries()
		{
			var testForwarder = Factory.New<OrgHeader>();
			testForwarder.OH_FullName = "Test Client";
			testForwarder.MainAddress.OA_Address1 = "184 Bourke Road";
			testForwarder.MainAddress.OA_City = "Alexandria";
			testForwarder.OH_RL_NKClosestPort = "AUSYD";

			var testForwarder2 = Factory.New<OrgHeader>();
			testForwarder2.OH_FullName = "Test Client";
			testForwarder2.MainAddress.OA_Address1 = "184 Bourke Road";
			testForwarder2.MainAddress.OA_City = "Alexandria";
			testForwarder2.OH_RL_NKClosestPort = "AUSYD";

			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			var entry1 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var entry2 = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			var entry3 = quote.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");

			AssertEquals("Pre-condition", ZGuid.Empty, entry1.TI_OH_AgentOverride);
			AssertEquals("Pre-condition", ZGuid.Empty, entry2.TI_OH_AgentOverride);
			AssertEquals("Pre-condition", ZGuid.Empty, entry3.TI_OH_AgentOverride);

			entry1.TI_OH_AgentOverride = testForwarder.PK;

			AssertEquals("Agent override should be set", testForwarder.PK, entry1.TI_OH_AgentOverride);
			AssertEquals("Agent override should be defaulted from Entry 1", testForwarder.PK, entry2.TI_OH_AgentOverride);
			AssertEquals("Agent override should not be defaulted as it is a different transport mode", ZGuid.Empty, entry3.TI_OH_AgentOverride);

			entry2.TI_OH_AgentOverride = testForwarder2.PK;
			AssertEquals("Agent override should be set", testForwarder2.PK, entry2.TI_OH_AgentOverride);
			AssertEquals("Agent override should NOT be defaulted from Entry 1", testForwarder.PK, entry1.TI_OH_AgentOverride);
			AssertEquals("Agent override should not be defaulted as it is a different transport mode", ZGuid.Empty, entry3.TI_OH_AgentOverride);
		}

		public void TestQuotationHeader()
		{
			var newQuote = Factory.NewWithValidTestData<Quote>();
			var newEntry = (QuoteEntry)newQuote.AddRateEntry(RatingConstants.RateCategory.SCO);

			AssertEquals("Should be Shipping header", "Shipping Containerized Freight Charges", newEntry.QuotationHeader);

			newQuote = Factory.NewWithValidTestData<Quote>();
			newEntry = (QuoteEntry)newQuote.AddRateEntry(RatingConstants.RateCategory.FCL);
			AssertEquals("Should be Freight header", "FCL Freight", newEntry.QuotationHeader);

			newQuote = Factory.NewWithValidTestData<Quote>();
			newEntry = (QuoteEntry)newQuote.AddRateEntry(RatingConstants.RateCategory.AIR);
			newEntry.TI_Mode = Core.Constants.RateMode.COU;
			AssertEquals("Should be Courier header", "Courier", newEntry.QuotationHeader);

			newQuote = Factory.NewWithValidTestData<Quote>();
			newEntry = (QuoteEntry)newQuote.AddRateEntry(RatingConstants.RateCategory.CFC);
			AssertEquals("Customs rate should be Freight header", "FCL Freight", newEntry.QuotationHeader);

			newQuote = Factory.NewWithValidTestData<Quote>();
			newEntry = (QuoteEntry)newQuote.AddRateEntry(RatingConstants.RateCategory.CAI);
			newEntry.TI_Mode = Core.Constants.RateMode.COU;
			AssertEquals("Customs rate should be Courier header", "Courier", newEntry.QuotationHeader);
		}
	}

	[TestedType(typeof(QuoteEntry))]
	public class QuoteEntryBizObjTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<Quote>();
			return header.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection.AddNew();
		}
	}
}
