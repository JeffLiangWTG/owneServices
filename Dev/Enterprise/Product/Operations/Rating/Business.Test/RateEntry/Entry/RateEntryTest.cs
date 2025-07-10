using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;
using Mode = Enterprise.Core.Constants.RateMode;

namespace Enterprise.Rating.Business.Testing
{
	sealed class RateEntryTest : RatingTestCase
	{
		#region QuotePageIncoterm

		public void TestQuotePageIncoTerm_ReadOnly_ClientRate() => TestQuotePageIncoTerm_ReadOnly(ratingHeader: Helper.NewClientRate(Helper.NewOrgHeader()), expectedReadOnlyCategories: Category.RateCategories);

		public void TestQuotePageIncoTerm_ReadOnly_Costing() => TestQuotePageIncoTerm_ReadOnly(ratingHeader: Helper.NewCosting(Helper.NewOrgHeader()), expectedReadOnlyCategories: Category.RateCategories);

		public void TestQuotePageIncoTerm_ReadOnly_Tariff() => TestQuotePageIncoTerm_ReadOnly(ratingHeader: Helper.NewCompanyTariff(), expectedReadOnlyCategories: Category.RateCategories);

		public void TestQuotePageIncoTerm_ReadOnly_Quote()
			=> TestQuotePageIncoTerm_ReadOnly
			(
				ratingHeader: Helper.NewQuote(Helper.NewOrgHeader()),
				expectedReadOnlyCategories: Category.RateCategories.Except(new[] { Category.AIR, Category.FCL, Category.LCL, Category.SCO, Category.SNC })
			);

		void TestQuotePageIncoTerm_ReadOnly(RatingHeader ratingHeader, IEnumerable<string> expectedReadOnlyCategories)
		{
			CombineAssertions("Incoterm is only editable in Quotation AIR/FCL/LCL/SCO/SNC", () =>
			{
				foreach (var rateCategory in Category.RateCategories)
				{
					var rateEntry = ratingHeader.AddRateEntry(rateCategory);

					var expectedReadOnlyCategory = expectedReadOnlyCategories.Contains(rateCategory);
					AssertEquals
					(
						message: rateCategory,
						expected: expectedReadOnlyCategory,
						actual: rateEntry.TI_QuotePageIncoTermInfo.ReadOnly
					);
				}
			});
		}

		#endregion

		public void TestRateCreationSourceSetWhenRequired()
		{
			var sourceStack = ObjectFactory.Get<IBusinessObjectCreationSourceStack>();
			var source = new Mock<IBusinessObjectCreationSource>();
			source.Setup(x => x.CreationSourceCode).Returns(RateEntryCreator.Sources.FromADAW);

			AssertNull("sourceStack.CurrentSource", sourceStack.CurrentSource);

			var rateEntry = Helper
				.NewClientRate(Helper.NewOrgHeader())
				.AddRateEntry(RatingConstants.RateCategory.AIR);
			AssertEquals("rateEntry.TI_CreationSource should be an empty string initially", string.Empty, rateEntry.TI_CreationSource);

			sourceStack.PushCreationSource(source.Object);
			AssertSame("sourceStack.CurrentSource should match the pushed source", source.Object, sourceStack.CurrentSource);

			rateEntry = Helper
				.NewClientRate(Helper.NewOrgHeader())
				.AddRateEntry(RatingConstants.RateCategory.AIR);
			AssertEquals("rateEntry.TI_CreationSource should match the source's creation source code", RateEntryCreator.Sources.FromADAW, rateEntry.TI_CreationSource);
		}

		public void TestRateCategoryMode()
		{
			CombineAssertions(() =>
			{
				var rateEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.AIR);
				rateEntry.TI_Mode = "BCN";
				AssertNoExceptionThrown(() => { Factory.Save(); });

				rateEntry.TI_RateCategory = RatingConstants.RateCategory.FCL;
				rateEntry.TI_Mode = "BCN";
				AssertNoExceptionThrown("AIR - BCN", () => { Factory.Save(); });

				rateEntry.TI_RateCategory = RatingConstants.RateCategory.LCL;
				rateEntry.TI_Mode = "BCN";
				AssertNoExceptionThrown("LCL - BCN", () => { Factory.Save(); });

				rateEntry.TI_Mode = "BBK";
				AssertNoExceptionThrown("LCL - BBK", () => { Factory.Save(); });

				rateEntry.TI_Mode = "BLK";
				AssertNoExceptionThrown("LCL - BLK", () => { Factory.Save(); });

				rateEntry.TI_Mode = "ROR";
				AssertNoExceptionThrown("LCL - ROR", () => { Factory.Save(); });

				rateEntry.TI_RateCategory = RatingConstants.RateCategory.ORG;
				rateEntry.TI_Mode = "BCN";
				AssertNoExceptionThrown("ORG - BCN", () => { Factory.Save(); });

				rateEntry.TI_Mode = "BBK";
				AssertNoExceptionThrown("ORG - BBK", () => { Factory.Save(); });

				rateEntry.TI_Mode = "BLK";
				AssertNoExceptionThrown("ORG - BLK", () => { Factory.Save(); });

				rateEntry.TI_Mode = "ROR";
				AssertNoExceptionThrown("ORG - ROR", () => { Factory.Save(); });

				rateEntry.TI_RateCategory = RatingConstants.RateCategory.DST;
				rateEntry.TI_Mode = "BCN";
				AssertNoExceptionThrown("DST - BCN", () => { Factory.Save(); });

				rateEntry.TI_Mode = "BBK";
				AssertNoExceptionThrown("DST - BBK", () => { Factory.Save(); });

				rateEntry.TI_Mode = "BLK";
				AssertNoExceptionThrown("DST - BLK", () => { Factory.Save(); });

				rateEntry.TI_Mode = "ROR";
				AssertNoExceptionThrown("DST - ROR", () => { Factory.Save(); });
			});
		}

		[TestDate(2010, 12, 2)]
		public void TestGetIncotermLookupType1()
		{
			AssertGetIncotermLookupType(Constants.IncoTerms.Incoterms2000);
		}

		[TestDate(2011, 12, 2)]
		public void TestGetIncotermLookupType2()
		{
			AssertGetIncotermLookupType(Constants.IncoTerms.Incoterms2010);
		}

		void AssertGetIncotermLookupType(IEnumerable<string> expectedCodes)
		{
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SOR);

			entry.TI_RateStartDate = ZDate.Empty;
			entry.TI_RateEndDate = ZDate.Empty;

			AssertIncotermLookupCodes(entry, expectedCodes);

			entry.TI_RateStartDate = new ZDate(2011, 1, 2);
			entry.TI_RateEndDate = ZDate.Empty;

			AssertIncotermLookupCodes(entry, expectedCodes);

			entry.TI_RateStartDate = ZDate.Empty;
			entry.TI_RateEndDate = new ZDate(2011, 12, 2);

			AssertIncotermLookupCodes(entry, expectedCodes);

			entry.TI_RateStartDate = new ZDate(2001, 12, 2);
			entry.TI_RateEndDate = new ZDate(2011, 12, 2);

			AssertIncotermLookupCodes(entry, expectedCodes);
		}

		void AssertIncotermLookupCodes(RateEntry entry, IEnumerable<string> expectedCodes)
		{
			var actualCodes = (from code in entry.Lookups.IncoTerms.Cast<CodeDescriptionPair>() select code.Code).ToArray();
			AssertContainsExactElementsInAnyOrder(expectedCodes, actualCodes);
		}

		#region IsDuplicateForPricingPageGrouping

		public void TestIsDuplicateForPricingPageGrouping()
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			var frtEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Constants.RateMode.SEA, "AU", "NL", "FRT", 100);
			var orgEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.ALL, "AU", "NL", "ODOC", 20);
			var dstEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.LCL, "AU", "NL", "DDOC", 20);

			CombineAssertions("Comparing a rate against itself should always return true for duplicates", () =>
			{
				AssertEquals(true, frtEntry.IsDuplicateForPricingPageGrouping(frtEntry, true));
				AssertEquals(true, frtEntry.IsDuplicateForPricingPageGrouping(frtEntry, true, true));
				AssertEquals(true, frtEntry.IsDuplicateForPricingPageGrouping(frtEntry, false));
				AssertEquals(true, frtEntry.IsDuplicateForPricingPageGrouping(frtEntry, false, true));

				AssertEquals(true, orgEntry.IsDuplicateForPricingPageGrouping(orgEntry, true));
				AssertEquals(true, orgEntry.IsDuplicateForPricingPageGrouping(orgEntry, true, true));
				AssertEquals(true, orgEntry.IsDuplicateForPricingPageGrouping(orgEntry, false));
				AssertEquals(true, orgEntry.IsDuplicateForPricingPageGrouping(orgEntry, false, true));

				AssertEquals(true, dstEntry.IsDuplicateForPricingPageGrouping(dstEntry, true));
				AssertEquals(true, dstEntry.IsDuplicateForPricingPageGrouping(dstEntry, true, true));
				AssertEquals(true, dstEntry.IsDuplicateForPricingPageGrouping(dstEntry, false));
				AssertEquals(true, dstEntry.IsDuplicateForPricingPageGrouping(dstEntry, false, true));
			});

			CombineAssertions("Due to Mode and Category these rates are not duplicates", () =>
			{
				AssertEquals(false, frtEntry.IsDuplicateForPricingPageGrouping(orgEntry, false));
				AssertEquals(false, frtEntry.IsDuplicateForPricingPageGrouping(orgEntry, false, true));

				AssertEquals(false, frtEntry.IsDuplicateForPricingPageGrouping(dstEntry, false));
				AssertEquals(false, frtEntry.IsDuplicateForPricingPageGrouping(dstEntry, false, true));

				AssertEquals(false, orgEntry.IsDuplicateForPricingPageGrouping(dstEntry, false));
				AssertEquals(false, orgEntry.IsDuplicateForPricingPageGrouping(dstEntry, false, true));
			});

			CombineAssertions("Ignoring Mode and Category these rates are duplicates", () =>
			{
				AssertEquals(true, frtEntry.IsDuplicateForPricingPageGrouping(orgEntry, true));
				AssertEquals(true, frtEntry.IsDuplicateForPricingPageGrouping(orgEntry, true, true));

				AssertEquals(true, frtEntry.IsDuplicateForPricingPageGrouping(dstEntry, true));
				AssertEquals(true, frtEntry.IsDuplicateForPricingPageGrouping(dstEntry, true, true));

				AssertEquals(true, orgEntry.IsDuplicateForPricingPageGrouping(dstEntry, true));
				AssertEquals(true, orgEntry.IsDuplicateForPricingPageGrouping(dstEntry, true, true));
			});
		}

		public void TestIsDuplicateForPricingPageGrouping_CheckContainerClass()
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			var entry1 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AU", "NL", "FRT", 100);
			var entry2 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AU", "NL", "FRT", 110);
			var entry3 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AU", "NL", "FRT", 120);

			var container1 = Helper.Containers["20GP"];
			var container2 = Helper.Containers["20RE"];
			var container3 = Helper.Containers["40GP"];

			container1.RC_FreightRateClass = "20GN";
			container2.RC_FreightRateClass = "20GN";
			container3.RC_FreightRateClass = "40GN";

			entry1.TI_RC = container1.PK;
			entry2.TI_RC = container2.PK;
			entry3.TI_RC = container3.PK;

			CombineAssertions("Pricing Page Groupings always ignore Containers and Container Class", () =>
			{
				Assert("Pre-condition", !entry1.TI_MatchContainerRateClass);
				Assert("Pre-condition", !entry2.TI_MatchContainerRateClass);
				Assert("Pre-condition", !entry3.TI_MatchContainerRateClass);

				AssertEquals(true, entry1.IsDuplicateForPricingPageGrouping(entry2, false));
				AssertEquals(true, entry1.IsDuplicateForPricingPageGrouping(entry2, false, true));

				AssertEquals(true, entry1.IsDuplicateForPricingPageGrouping(entry3, false));
				AssertEquals(true, entry1.IsDuplicateForPricingPageGrouping(entry3, false, true));

				AssertEquals(true, entry2.IsDuplicateForPricingPageGrouping(entry3, false));
				AssertEquals(true, entry2.IsDuplicateForPricingPageGrouping(entry3, false, true));
			});

			entry1.TI_MatchContainerRateClass = true;
			entry2.TI_MatchContainerRateClass = true;
			entry3.TI_MatchContainerRateClass = true;

			CombineAssertions("All the same still", () =>
			{
				AssertEquals(true, entry1.IsDuplicateForPricingPageGrouping(entry2, false));
				AssertEquals(true, entry1.IsDuplicateForPricingPageGrouping(entry2, false, true));

				AssertEquals(true, entry1.IsDuplicateForPricingPageGrouping(entry3, false));
				AssertEquals(true, entry1.IsDuplicateForPricingPageGrouping(entry3, false, true));

				AssertEquals(true, entry2.IsDuplicateForPricingPageGrouping(entry3, false));
				AssertEquals(true, entry2.IsDuplicateForPricingPageGrouping(entry3, false, true));
			});

			var entry4 = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AU", "NL", "FRT", 100);
			entry4.TI_RC = container1.PK;

			CombineAssertions("Now we have a mix between container class and non container class rates we expect some non-duplicates", () =>
			{
				AssertEquals(true, entry4.IsDuplicateForPricingPageGrouping(entry1, false));
				AssertEquals(false, entry4.IsDuplicateForPricingPageGrouping(entry1, false, true));

				AssertEquals(true, entry4.IsDuplicateForPricingPageGrouping(entry2, false));
				AssertEquals(false, entry4.IsDuplicateForPricingPageGrouping(entry2, false, true));

				AssertEquals(true, entry4.IsDuplicateForPricingPageGrouping(entry3, false));
				AssertEquals(false, entry4.IsDuplicateForPricingPageGrouping(entry3, false, true));
			});
		}

		public void TestIsDuplicateForPricingPageGrouping_ByAddress()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());

			var entry1 = quote.AddRateEntry(RatingConstants.RateCategory.LCL);
			entry1.TI_CartageDeliveryAddressPostCode = "4";
			entry1.TI_OriginLRC = Constants.CountryCodes.Australia;
			entry1.TI_DestinationLRC = Constants.CountryCodes.UnitedKingdom;

			var entry2 = quote.AddRateEntry(RatingConstants.RateCategory.LCL);
			entry2.TI_CartageDeliveryAddressPostCode = "5";
			entry2.TI_OriginLRC = Constants.CountryCodes.Australia;
			entry2.TI_DestinationLRC = Constants.CountryCodes.UnitedKingdom;

			AssertEquals("Different PostCodes", false, entry1.IsDuplicateForPricingPageGrouping(entry2, false));

			entry2.TI_CartageDeliveryAddressPostCode = "4";
			AssertEquals("Same PostCodes", true, entry1.IsDuplicateForPricingPageGrouping(entry2, false));

			entry1.TI_CartagePickupAddressPostCode = "1000";
			entry2.TI_CartagePickupAddressPostCode = "2000";
			AssertEquals("Different PostCodes", false, entry1.IsDuplicateForPricingPageGrouping(entry2, false));

			entry2.TI_CartagePickupAddressPostCode = "1000";
			AssertEquals("Same PostCodes", true, entry1.IsDuplicateForPricingPageGrouping(entry2, false));

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			var address2 = Factory.NewWithValidTestData<OrgAddress>();

			entry1.TI_OA_CartagePickupAddressOverride = address1.PK;
			entry2.TI_OA_CartagePickupAddressOverride = address2.PK;
			AssertEquals("Different Pickup Address", false, entry1.IsDuplicateForPricingPageGrouping(entry2, false));

			entry1.TI_OA_CartagePickupAddressOverride = address2.PK;
			AssertEquals("Same Pickup Address", true, entry1.IsDuplicateForPricingPageGrouping(entry2, false));

			entry1.TI_OA_CartageDeliveryAddressOverride = address1.PK;
			entry2.TI_OA_CartageDeliveryAddressOverride = address2.PK;
			AssertEquals("Different Delivery Address", false, entry1.IsDuplicateForPricingPageGrouping(entry2, false));

			entry2.TI_OA_CartageDeliveryAddressOverride = address1.PK;
			AssertEquals("Same Delivery Address", true, entry1.IsDuplicateForPricingPageGrouping(entry2, false));
		}

		public void TestPrintOriginChargesAndDestinationChargesWithSameOriginAndDestination()
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			var entry1 = quote.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.SEA, "AU", "NL");
			var line = entry1.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.CN);
			line.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var entry2 = quote.AddRateEntry(RatingConstants.RateCategory.DST, Constants.RateMode.LCL, "AU", "NL");
			var line2 = entry2.AddRateLine("DDOC", MinimumOrPerUnitCalculator.Code, QuantityUnit.M3);
			line2.GetCalculator<MinimumOrPerUnitCalculator>().Minimum = 100m;
			line2.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 50m;

			AssertEquals("Different TI_RateCategorys", false, entry1.IsDuplicateForPricingPageGrouping(entry2, false));
		}

		public void TestMergingChargesInForwardingLandscapePricingPage()
		{
			var quote = Factory.NewWithValidTestData<Quote>();

			var entry1 = quote.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AU", "NL");
			entry1.TI_RC = GP20.PK;
			var line = entry1.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			line.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var entry2 = quote.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "AU", "NL");
			var line2 = entry2.AddRateLine("FRT", MinimumOrPerUnitCalculator.Code, QuantityUnit.M3);
			line2.GetCalculator<MinimumOrPerUnitCalculator>().Minimum = 100m;
			line2.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 50m;

			AssertEquals("Two rate lines should be merged into a single line when displaying in forwarding landscape pricing page",
				true, entry1.IsDuplicateForPricingPageGrouping(entry2, true));

			entry2.TI_PL_NKCarrierServiceLevel = "AAA";
			entry1.TI_PL_NKCarrierServiceLevel = "XXX";

			AssertEquals("Two rate lines should NOT be merged when Carrier Service Level is different even though we don't show it on a Document",
				false, entry1.IsDuplicateForPricingPageGrouping(entry2, true));
		}

		#endregion

		public void TestSupportsNotes()
		{
			var entry1 = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SOR);
			var entry2 = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SOR);
			Assert(!entry1.SupportsNotes);
			Assert(!entry2.SupportsNotes);
		}

		public void TestDefaultFromPortCurrency_Shipping()
		{
			var shippingOrigin = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SOR);
			var shippingDestination = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SDE);
			var shippingImportDetention = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SID);
			var shippingExportDetention = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.SED);

			var entries = new RateEntry[]
			{
				shippingOrigin,
				shippingExportDetention,

				shippingDestination,
				shippingImportDetention,
			};

			foreach (var entry in entries)
			{
				entry.TI_OriginLRC = Constants.CountryCodes.NewZealand;
				entry.TI_DestinationLRC = Constants.CountryCodes.Netherlands;
			}

			CombineAssertions(delegate
			{
				AssertEquals("Shipping Origin", Constants.CurrencyCodes.NewZealand, shippingOrigin.TI_RX_NKCurrency);
				AssertEquals("Shipping Export Detention", Constants.CurrencyCodes.NewZealand, shippingExportDetention.TI_RX_NKCurrency);

				AssertEquals("Shipping Destination", Constants.CurrencyCodes.Netherlands, shippingDestination.TI_RX_NKCurrency);
				AssertEquals("Shipping Import Detention", Constants.CurrencyCodes.Netherlands, shippingImportDetention.TI_RX_NKCurrency);
			});
		}

		public void TestIsFCL()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.CFC);
			AssertEquals("IsFCL", true, entry.IsFCL());
		}

		public void TestShippingFlags()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.SCO);

			AssertEquals("IsShipping", true, entry.IsShipping());
			AssertEquals("IsShippingContainerisedFreight", true, entry.TI_RateCategory == RatingConstants.RateCategory.SCO);
			AssertEquals("IsShippingNonContainerisedFreight", false, entry.TI_RateCategory == RatingConstants.RateCategory.SNC);
			AssertEquals("IsFCL", true, entry.IsFCL());

			entry.TI_RateCategory = RatingConstants.RateCategory.SNC;
			AssertEquals("IsShipping", true, entry.IsShipping());
			AssertEquals("IsShippingContainerisedFreight", false, entry.TI_RateCategory == RatingConstants.RateCategory.SCO);
			AssertEquals("IsShippingNonContainerisedFreight", true, entry.TI_RateCategory == RatingConstants.RateCategory.SNC);
			AssertEquals("IsFCL", false, entry.IsFCL());

			entry.TI_RateCategory = RatingConstants.RateCategory.FCL;
			AssertEquals("IsShipping", false, entry.IsShipping());
			AssertEquals("IsShippingContainerisedFreight", false, entry.TI_RateCategory == RatingConstants.RateCategory.SCO);
			AssertEquals("IsShippingNonContainerisedFreight", false, entry.TI_RateCategory == RatingConstants.RateCategory.SNC);
			AssertEquals("IsFCL", true, entry.IsFCL());

			entry.TI_RateCategory = RatingConstants.RateCategory.CFC;
			AssertEquals("IsShipping", false, entry.IsShipping());
			AssertEquals("IsShippingContainerisedFreight", false, entry.TI_RateCategory == RatingConstants.RateCategory.SCO);
			AssertEquals("IsShippingNonContainerisedFreight", false, entry.TI_RateCategory == RatingConstants.RateCategory.SNC);
			AssertEquals("IsFCL", true, entry.IsFCL());
		}

		public void TestShippingDetentionFlags()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.SID);

			AssertEquals("IsShippingExportDetention", false, entry.IsShippingExportDetention());
			AssertEquals("IsShippingImportDetention", true, entry.IsShippingImportDetention());
			AssertEquals("IsFCL", true, entry.IsFCL());

			entry.TI_RateCategory = RatingConstants.RateCategory.SED;
			AssertEquals("IsShippingExportDetention", true, entry.IsShippingExportDetention());
			AssertEquals("IsShippingImportDetention", false, entry.IsShippingImportDetention());
			AssertEquals("IsFCL", true, entry.IsFCL());
		}

		public void TestCarrierServiceLevel()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrier1SvcLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			carrier1SvcLevel.PL_Code = "XYZ";

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2SvcLevel = carrier2.MiscServ.CarrierServiceLevels.AddNew();
			carrier2SvcLevel.PL_Code = "XYZ";

			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var cost = Factory.New<Costing>();
			cost.TH_OH = carrier.PK;
			var costEntry = cost.AddRateEntry("AIR");
			costEntry.TI_PL_NKCarrierServiceLevel = "XYZ";
			AssertEquals(carrier1SvcLevel, costEntry.CarrierServiceLevel);

			costEntry.TI_OH_TransportProvider = carrier2.PK;
			AssertEquals(carrier2SvcLevel, costEntry.CarrierServiceLevel);

			costEntry.TI_OH_TransportProvider = carrier3.PK;
			costEntry.TI_PL_NKCarrierServiceLevel = "STD";
			AssertEquals("STD", costEntry.CarrierServiceLevel.PL_Code);
		}

		public void TestQuotationIndexDescription()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			var airEntry = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			airEntry.TI_ViaLRC = "DEHAM";

			AssertEquals("Sydney to Los Angeles, US via Hamburg", airEntry.QuotationIndexDescription);
		}

		public void TestCrossTrade()
		{
			var rate = Factory.New<ClientRate>();
			var rateEntry = rate.AddRateEntry("AIR");

			AssertEquals("Cross trade off by default", false, rateEntry.TI_IsCrossTrade);
			AssertEquals("Origin / Dest editable", false, rateEntry.TI_OriginLRCInfo.ReadOnly);
			AssertEquals("Origin / Dest editable", false, rateEntry.TI_DestinationLRCInfo.ReadOnly);

			rateEntry.TI_OriginLRC = "AUSYD";
			rateEntry.TI_DestinationLRC = "USLAX";
			rateEntry.TI_IsCrossTrade = true;

			AssertEquals("Origin / Dest readonly", true, rateEntry.TI_OriginLRCInfo.ReadOnly);
			AssertEquals("Origin / Dest readonly", true, rateEntry.TI_DestinationLRCInfo.ReadOnly);
			Assert("Origin / Dest blank", rateEntry.TI_OriginLRC.IsEmpty);
			Assert("Origin / Dest blank", rateEntry.TI_DestinationLRC.IsEmpty);

			rateEntry.Delete();
			AssertEquals("Origin / Dest not readonly as deleted", false, rateEntry.TI_OriginLRCInfo.ReadOnly);
			AssertEquals("Origin / Dest not readonly as deleted", false, rateEntry.TI_DestinationLRCInfo.ReadOnly);
		}

		#region TestCopyRateLine

		public void TestInsertRelatedRateLine_CostingToClientRate_ContractNumber_ShouldNotPopulate()
		{
			var cost = Helper.NewCosting(Helper.NewOrgHeader());
			var costEntry = cost.AddRateEntry("FCL", "SEA", "AUSYD", "");
			costEntry.TI_ContractNumber = "12345";

			var costLine = costEntry.AddRateLine("ODOC");
			var relatedLine = Factory.Load<RelatedRateLine>(costLine.PK);

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "");
			rateEntry.InsertRelatedRateLine(relatedLine, false);

			AssertEquals("We can't populate carrier contract number as client contract number", ZString.Empty, rateEntry.TI_ContractNumber);
		}

		public void TestInsertRelatedRateLine_CostingToCostingRate_ContractNumber_ShouldPopulate()
		{
			var cost = Helper.NewCosting(Helper.NewOrgHeader());
			var costEntry = cost.AddRateEntry("FCL", "SEA", "AUSYD", "");
			costEntry.TI_ContractNumber = "12345";

			var costLine = costEntry.AddRateLine("ODOC");
			var relatedLine = Factory.Load<RelatedRateLine>(costLine.PK);

			var rate = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "");
			rateEntry.InsertRelatedRateLine(relatedLine, false);

			AssertEquals("12345", rateEntry.TI_ContractNumber);
		}

		public void TestInsertRelatedRateLine_ClientRateToClientRate_ContractNumber_ShouldPopulate()
		{
			var cost = Helper.NewClientRate(Helper.NewOrgHeader());
			var costEntry = cost.AddRateEntry("FCL", "SEA", "AUSYD", "");
			costEntry.TI_ContractNumber = "12345";

			var costLine = costEntry.AddRateLine("ODOC");
			var relatedLine = Factory.Load<RelatedRateLine>(costLine.PK);

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "");
			rateEntry.InsertRelatedRateLine(relatedLine, false);

			AssertEquals("12345", rateEntry.TI_ContractNumber);
		}

		public void TestCopyRateLineFromClientRate()
		{
			var testChargeCode = Factory.New<AccChargeCode>();
			testChargeCode.AC_Code = "ZUB";

			var rate = Factory.New<ClientRate>();
			var rateEntry = rate.AddRateEntry("AIR");
			var rateLine = rateEntry.RateLines[0];
			rateLine.TL_AC = testChargeCode.PK;

			var costingLine = Factory.Load<RelatedRateLine>(rateLine.PK);

			var testQuote = Factory.New<Quote>();
			var quoteEntry = testQuote.AddRateEntry("AIR");
			quoteEntry.RateLines.RemoveAndDeleteAll();

			AssertEquals("Pre-condition - No Rate Lines on Quote", 0, quoteEntry.RateLines.Count);
			quoteEntry.InsertRelatedRateLine(costingLine, true);
			AssertEquals("1 rate line added from client rate", 1, rateEntry.RateLines.Count);
			AssertEquals("Same Charge Code", rateEntry.RateLines[0].ChargeCode.PK, costingLine.ChargeCode.PK);
			AssertEquals("Same Calculator", rateEntry.RateLines[0].TL_RateCalculator, costingLine.TL_RateCalculator);
		}

		public void TestCopyRateLineFromClientRateWithPlannedLoadAndDischarge()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry("AIR");
			rateEntry.TI_PlannedLoadLRC = "AUSYD";
			rateEntry.TI_PlannedDischargeLRC = "USLAX";

			var rateLine = rateEntry.RateLines[0];
			var tariffLine = Factory.Load<RelatedRateLine>(rateLine.PK);

			var tariff = Helper.NewCompanyTariff();
			var tariffEntry = tariff.AddRateEntry("AIR");
			tariffEntry.InsertRelatedRateLine(tariffLine, true);

			AssertEquals("Same Planned Load", rateEntry.TI_PlannedLoadLRC, tariffEntry.TI_PlannedLoadLRC);
			AssertEquals("Same Planned Discharge", rateEntry.TI_PlannedDischargeLRC, tariffEntry.TI_PlannedDischargeLRC);
		}

		public void TestCopyRateLineCalculatorFromClientRate()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry("AIR");

			var line = rateEntry.RelatedRateLines.AddNew();
			line.TL_TI = rateEntry.PK;
			line.TL_AC = Helper.ChargeCodes["FRT"].PK;
			line.TL_RateCalculator = "CMB";
			line.Calculator.AddRateLineItem("-", 45m, 0m, 10m);
			line.Calculator.AddRateLineItem("+", 45m, 0m, 15m);
			line.Calculator.AddRateLineItem("+", 100m, 0m, 17m);
			line.Calculator.AddRateLineItem("+", 300m, 0.5m, 20m);

			var testQuote = Factory.New<Quote>();
			var quoteEntry = testQuote.AddRateEntry("AIR");
			quoteEntry.RateLines.RemoveAndDeleteAll();
			quoteEntry.InsertRelatedRateLine(line, true);

			AssertEquals("Calculators have same number of RateLineItems",
				line.Calculator.ApplyToRateLineItems.Count, quoteEntry.RateLines[0].Calculator.ApplyToRateLineItems.Count);
			for (var i = 0; i < line.Calculator.ApplyToRateLineItems.Count; i++)
			{
				AssertEquals(line.Calculator.ApplyToRateLineItems[0].TM_Type, quoteEntry.RateLines[0].Calculator.ApplyToRateLineItems[0].TM_Type);
				AssertEquals(line.Calculator.ApplyToRateLineItems[0].TM_Value, quoteEntry.RateLines[0].Calculator.ApplyToRateLineItems[0].TM_Value);
			}

			line.TL_RateCalculator = "UNT";
			line.RateLineItems[0].TM_Value = 300;
			quoteEntry.RateLines.RemoveAndDeleteAll();
			quoteEntry.InsertRelatedRateLine(line, true);

			AssertEquals("Calculators have same number of RateLineItems",
				line.Calculator.ApplyToRateLineItems.Count, quoteEntry.RateLines[0].Calculator.ApplyToRateLineItems.Count);
			for (var i = 0; i < line.Calculator.ApplyToRateLineItems.Count; i++)
			{
				AssertEquals(line.Calculator.ApplyToRateLineItems[0].TM_Type, quoteEntry.RateLines[0].Calculator.ApplyToRateLineItems[0].TM_Type);
				AssertEquals(line.Calculator.ApplyToRateLineItems[0].TM_Value, quoteEntry.RateLines[0].Calculator.ApplyToRateLineItems[0].TM_Value);
			}
		}

		public void TestCopyRateLineIncludeBothServiceLevelAndCarrierServiceLevel()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry("FCL");
			rateEntry.TI_PL_NKCarrierServiceLevel = "STD";
			rateEntry.TI_RS_NKServiceLevel_NI = "DIR";

			var line = rateEntry.RelatedRateLines.AddNew();
			line.TL_TI = rateEntry.PK;
			line.TL_AC = Helper.ChargeCodes["FRT"].PK;

			var testQuote = Factory.New<Quote>();
			var quoteEntry = testQuote.AddRateEntry("FCL");
			quoteEntry.RateLines.RemoveAndDeleteAll();
			quoteEntry.InsertRelatedRateLine(line, true);

			AssertEquals("STD", quoteEntry.TI_PL_NKCarrierServiceLevel);
			AssertEquals("DIR", quoteEntry.TI_RS_NKServiceLevel_NI);
		}

		public void TestCopyRateLine_FromCostingOrCompanyTariff()
		{
			var testChargeCode = Factory.New<AccChargeCode>();
			testChargeCode.AC_Code = "ZUB";

			var cost = Factory.New<Costing>();
			var costEntry = cost.AddRateEntry("AIR");

			var fakeCostingLine = costEntry.RateLines.AddNew();
			fakeCostingLine.TL_AC = testChargeCode.PK;

			var costingLine = Factory.Load<RelatedRateLine>(fakeCostingLine.PK);

			var rate = Factory.New<ClientRate>();
			var rateEntry = rate.AddRateEntry("AIR");
			rateEntry.RateLines.RemoveAndDeleteAll();

			// Don't replace
			var rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = testChargeCode.PK;

			AssertEquals("Pre-condition - 1 Rate Line on Rate", 1, rateEntry.RateLines.Count);
			rateEntry.InsertRelatedRateLine(costingLine, false);
			AssertEquals("2 rate lines exist", 2, rateEntry.RateLines.Count);
			AssertEquals("Both have same charge code", rateEntry.RateLines[0].ChargeCode.PK, rateEntry.RateLines[1].ChargeCode.PK);

			// Replace existing
			rateEntry.RateLines.RemoveAndDeleteAll();

			rateLine = rateEntry.RateLines.AddNew();
			rateLine.TL_AC = testChargeCode.PK;

			AssertEquals("Pre-condition - 1 Rate Line on Rate", 1, rateEntry.RateLines.Count);
			rateEntry.InsertRelatedRateLine(costingLine, true);
			AssertEquals("1 rate line exists - existing removed", 1, rateEntry.RateLines.Count);
			AssertEquals("Same Charge Code", costingLine.ChargeCode.PK, rateEntry.RateLines[0].ChargeCode.PK);
		}

		public void TestCopyRateLine_FromCostingOrCompanyTariffWithEquipment()
		{
			var testChargeCode = Factory.New<AccChargeCode>();
			testChargeCode.AC_Code = "ZUB";
			testChargeCode.AC_RateCalculator = CartageCalculator.Code;

			var cost = Factory.New<Costing>();
			var costEntry = cost.AddRateEntry("ORG", origin: "AUSYD");

			var fakeCostingLine = costEntry.RateLines.AddNew();
			fakeCostingLine.TL_AC = testChargeCode.PK;

			Factory.Save();

			var costingLine = Factory.Load<RelatedRateLine>(fakeCostingLine.PK);

			costingLine.Calculator.EquipmentType = "###";

			var rate = Factory.New<ClientRate>();
			var rateEntry = rate.AddRateEntry("ORG", origin: "AUSYD");
			rateEntry.InsertRelatedRateLine(costingLine, false);
			Assert(rateEntry.RateLines[0].Calculator.ShowEquipmentType);
			AssertEquals("###", rateEntry.RateLines[0].Calculator.EquipmentType);
		}

		public void TestCopyRateLine_FromCostingOrCompanyTariffCopiesRateEntryValuesFromNonCarrierSupplier()
		{
			var cost = Helper.NewCosting(Helper.NewOrgHeader());
			var costEntry = cost.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "D2D", "20GP");
			costEntry.TI_RH_NKCommodityCode = "HAZ";
			costEntry.TI_ViaLRC = "HKHKG";
			costEntry.TI_TransitTime = "SMD";
			costEntry.TI_Frequency = 5;
			costEntry.TI_FrequencyUnit = "DAY";

			var fakeCostingLine = costEntry.AddRateLine("BAF");
			var costingLine = Factory.Load<RelatedRateLine>(fakeCostingLine.PK);

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = cost.AddRateEntry("FCL", "SEA", "AU", "", "", "");
			rateEntry.TI_RH_NKCommodityCode = "HAZ";
			rateEntry.InsertRelatedRateLine(costingLine, false);

			AssertEquals("Values copied from cost entry", costEntry.TI_DestinationLRC, rateEntry.TI_DestinationLRC);
			AssertEquals("Origin NOT updated", "AU", rateEntry.TI_OriginLRC);
			AssertEquals("Values copied from cost entry", costEntry.TI_ViaLRC, rateEntry.TI_ViaLRC);
			AssertEquals("Values copied from cost entry", costEntry.TI_RH_NKCommodityCode, rateEntry.TI_RH_NKCommodityCode);
			AssertEquals("Values copied from cost entry", costEntry.TI_RS_NKServiceLevel_NI, rateEntry.TI_RS_NKServiceLevel_NI);
			AssertEquals("Values copied from cost entry", cost.TH_OH, rateEntry.TI_OH_Supplier);
			AssertEquals("Carrier field is blank as the cost supplier is not a carrier", ZGuid.Empty, rateEntry.TI_OH_TransportProvider);
			AssertEquals("Values copied from cost entry", costEntry.TI_RC, rateEntry.TI_RC);
			AssertEquals("Values copied from cost entry", costEntry.TI_TransitTime, rateEntry.TI_TransitTime);
			AssertEquals("Values copied from cost entry", costEntry.TI_Frequency, rateEntry.TI_Frequency);
			AssertEquals("Values copied from cost entry", costEntry.TI_FrequencyUnit, rateEntry.TI_FrequencyUnit);

			cost.Header.OH_IsShippingProvider = true;
			cost.Header.OH_IsAirLine = true;

			var rateEntry2 = cost.AddRateEntry("FCL", "SEA", "AU", "", "", "");
			rateEntry2.InsertRelatedRateLine(costingLine, false);

			AssertEquals("Values copied from cost entry", cost.TH_OH, rateEntry2.TI_OH_Supplier);
			AssertEquals("Carrier field copied as this costing provider IS a carrier", cost.TH_OH, rateEntry2.TI_OH_TransportProvider);
		}

		public void TestCopyRateLine_FromCostingOrCompanyTariffCopiesRateEntryValuesFromCarrier()
		{
			var cost = Helper.NewCosting(Helper.NewOrgHeader());
			cost.Header.OH_IsShippingProvider = true;
			cost.Header.OH_IsAirLine = true;

			var costEntry = cost.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "D2D", "20GP");
			costEntry.TI_RH_NKCommodityCode = "HAZ";
			costEntry.TI_ViaLRC = "HKHKG";
			costEntry.TI_TransitTime = "SMD";
			costEntry.TI_Frequency = 5;
			costEntry.TI_FrequencyUnit = "DAY";

			var fakeCostingLine = costEntry.AddRateLine("BAF");
			var costingLine = Factory.Load<RelatedRateLine>(fakeCostingLine.PK);

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = cost.AddRateEntry("FCL", "SEA", "AU", "", "", "");
			rateEntry.TI_RH_NKCommodityCode = "HAZ";
			rateEntry.InsertRelatedRateLine(costingLine, false);

			AssertEquals("Values copied from cost entry", costEntry.TI_DestinationLRC, rateEntry.TI_DestinationLRC);
			AssertEquals("Origin NOT updated", "AU", rateEntry.TI_OriginLRC);
			AssertEquals("Values copied from cost entry", costEntry.TI_ViaLRC, rateEntry.TI_ViaLRC);
			AssertEquals("Values copied from cost entry", costEntry.TI_RH_NKCommodityCode, rateEntry.TI_RH_NKCommodityCode);
			AssertEquals("Values copied from cost entry", costEntry.TI_RS_NKServiceLevel_NI, rateEntry.TI_RS_NKServiceLevel_NI);
			AssertEquals("Values copied from cost entry", cost.TH_OH, rateEntry.TI_OH_Supplier);
			AssertEquals("Carrier field copied as this costing provider IS a carrier", cost.TH_OH, rateEntry.TI_OH_TransportProvider);
			AssertEquals("Values copied from cost entry", costEntry.TI_RC, rateEntry.TI_RC);
			AssertEquals("Values copied from cost entry", costEntry.TI_TransitTime, rateEntry.TI_TransitTime);
			AssertEquals("Values copied from cost entry", costEntry.TI_Frequency, rateEntry.TI_Frequency);
			AssertEquals("Values copied from cost entry", costEntry.TI_FrequencyUnit, rateEntry.TI_FrequencyUnit);
		}

		public void TestCopyRateLine_FromCostingOrCompanyTariffWithMessageType()
		{
			var testChargeCode = Factory.New<AccChargeCode>();
			testChargeCode.AC_Code = "ZUB";
			testChargeCode.AC_RateCalculator = AgencyCalculator.Code;

			var cost = Factory.New<Costing>();
			var costEntry = cost.AddRateEntry("ORG");

			var fakeCostingLine = costEntry.RateLines.AddNew();
			fakeCostingLine.TL_AC = testChargeCode.PK;

			var costingLine = Factory.Load<RelatedRateLine>(fakeCostingLine.PK);

			costingLine.Calculator.MessageType = SharedJobMessageTypeList.Codes.Import;
			costingLine.Calculator.MessageSubType = "FRM";

			var rate = Factory.New<ClientRate>();
			var rateEntry = rate.AddRateEntry("ORG");
			rateEntry.InsertRelatedRateLine(costingLine, false);
			Assert(rateEntry.RateLines[0].Calculator.ShowMessageTypeSubType);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, rateEntry.RateLines[0].Calculator.MessageType);
			AssertEquals("FRM", rateEntry.RateLines[0].Calculator.MessageSubType);
		}

		public void TestCopyRateLine_FromCostingOrCompanyTariffWithConsignee()
		{
			var cost = Helper.NewCosting(Helper.NewOrgHeader());
			var costEntry = cost.AddRateEntry("ORG", "AIR", "AUSYD", "");
			costEntry.TI_OH_Consignee = Helper.NewOrgHeader().PK;
			costEntry.TI_OA_CartageDeliveryAddressOverride = costEntry.Consignee.MainAddress.PK;
			var costLine = costEntry.AddRateLine("ODOC");
			var relatedLine = Factory.Load<RelatedRateLine>(costLine.PK);

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry("ORG", "AIR", "AUSYD", "");
			rateEntry.InsertRelatedRateLine(relatedLine, false);
			AssertNotNull(rateEntry.Consignee);
			AssertNotNull(rateEntry.CartageDeliveryAddressOverride);
		}

		public void TestCopyRateLine_FromCostingOrCompanyTariffWithConsignor()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var tariffEntry = tariff1.AddRateEntry("DST", "LCL", "", "USLAX");
			tariffEntry.TI_OH_Consignor = Helper.NewOrgHeader().PK;
			tariffEntry.TI_OA_CartagePickupAddressOverride = tariffEntry.Consignor.MainAddress.PK;
			var tariffLine = tariffEntry.AddRateLine("ODOC");
			var relatedLine = Factory.Load<RelatedRateLine>(tariffLine.PK);
			tariff1.Factory.Save();

			var rate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var rateEntry = rate.AddRateEntry("DST", "LCL", "", "USLAX");
			rateEntry.InsertRelatedRateLine(relatedLine, false);
			AssertNotNull(rateEntry.Consignor);
			AssertNotNull(rateEntry.CartagePickupAddressOverride);
		}

		[TestDate(2008, 1, 10)]
		public void TestCopyRateLine_FromCostingOrCompanyTariffWithQuoteDates()
		{
			var tariff1 = Helper.NewCompanyTariff();

			var tariffEntry = tariff1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			tariffEntry.TI_RateStartDate = new ZDate(2008, 1, 1);
			tariffEntry.TI_RateEndDate = new ZDate(2008, 1, 31);
			var tariffLine = tariffEntry.RateLines[0];
			tariffLine.TL_RateCalculator = UnitCalculator.Code;
			((UnitCalculator)tariffLine.Calculator).PerUnit = 100m;

			var tariffEntry2 = tariff1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			tariffEntry2.TI_RateStartDate = new ZDate(2008, 2, 1);
			tariffEntry2.TI_RateEndDate = new ZDate(2008, 2, 20);
			var tariffLine2 = tariffEntry2.RateLines[0];
			tariffLine2.TL_RateCalculator = UnitCalculator.Code;
			((UnitCalculator)tariffLine2.Calculator).PerUnit = 200m;

			tariff1.Factory.Save();

			Factory.Save();

			var quote = Factory.New<Quote>();
			quote.TH_OH = Helper.NewOrgHeader(1).PK;

			var quoteEntry = quote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			quoteEntry.RateLines[0].TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;

			quote.TH_QuoteDate = new ZDate(2008, 1, 20);
			var resultsLine = new CompanyTariffOrCostLineCloneHelper(quoteEntry.RateLines[0]).GetClone();

			AssertEquals(100m, ((UnitCalculator)resultsLine.Calculator).PerUnit);

			quote.TH_QuoteDate = new ZDate(2008, 2, 20);
			resultsLine = new CompanyTariffOrCostLineCloneHelper(quoteEntry.RateLines[0]).GetClone();

			AssertEquals(200m, ((UnitCalculator)resultsLine.Calculator).PerUnit);
		}

		public void TestCopyRateLine_ToClientRateOrTariffWithFMCTariffID()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var tariffEntry = tariff1.AddRateEntry("DST", "LCL", "", "USLAX");
			tariffEntry.TI_FMCTariffID = "123";
			var tariffLine = tariffEntry.AddRateLine("ODOC");
			var relatedLine = Factory.Load<RelatedRateLine>(tariffLine.PK);
			tariff1.Factory.Save();

			var rate = Helper.NewClientRate(Helper.NewOrgHeader(1));
			var rateEntry = rate.AddRateEntry("DST", "LCL", "", "USLAX");
			rateEntry.InsertRelatedRateLine(relatedLine, false);
			AssertEquals("123", rateEntry.TI_FMCTariffID);
		}

		public void TestCopyRateLine_ToCostingWithFMCTariffID()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var tariffEntry = tariff1.AddRateEntry("DST", "LCL", "", "USLAX");
			tariffEntry.TI_FMCTariffID = "123";
			var tariffLine = tariffEntry.AddRateLine("ODOC");
			var relatedLine = Factory.Load<RelatedRateLine>(tariffLine.PK);
			tariff1.Factory.Save();

			var rate = Helper.NewCosting(Helper.NewOrgHeader(1));
			var rateEntry = rate.AddRateEntry("DST", "LCL", "", "USLAX");
			rateEntry.InsertRelatedRateLine(relatedLine, false);
			Assert(rateEntry.TI_FMCTariffID.IsEmpty);
		}

		public void TestCopyRateLine_ToClientRate_IsNonOperatedReefer()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var tariffEntry = tariff1.AddRateEntry("ORG", "FCL", "", "USLAX", container: "20FR");
			tariffEntry.TI_IsNonOperatedReefer = "Y";
			var tariffLine = tariffEntry.AddRateLine("ODOC");
			var relatedLine = Factory.Load<RelatedRateLine>(tariffLine.PK);
			tariff1.Factory.Save();

			var rate = Helper.NewCosting(Helper.NewOrgHeader(1));
			var rateEntry = rate.AddRateEntry("ORG", "FCL", "", "USLAX");
			rateEntry.InsertRelatedRateLine(relatedLine, false);
			AssertEquals("Y", rateEntry.TI_IsNonOperatedReefer);
		}

		public void TestCopyRateLine_ToClientRate_IsNonOperatedReefer_ReadOnlyView()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var tariffEntry = tariff1.AddRateEntry("ORG", "ALL", "", "USLAX", container: "20FR");
			tariffEntry.TI_IsNonOperatedReefer = "Y";
			var tariffLine = tariffEntry.AddRateLine("ODOC");
			var relatedLine = Factory.Load<RelatedRateLine>(tariffLine.PK);
			tariff1.Factory.Save();

			var rate = Helper.NewCosting(Helper.NewOrgHeader(1));
			var rateEntry = rate.AddRateEntry("ORG", "ALL", "", "USLAX");
			rateEntry.InsertRelatedRateLine(relatedLine, false);
			Assert(rateEntry.TI_IsNonOperatedReefer.IsEmpty);
		}

		#endregion

		public void TestCartageAddressReadonlyState()
		{
			var cost = Helper.NewCosting(Helper.NewOrgHeader());
			var costEntry = cost.AddRateEntry("ORG", "AIR", "AUSYD", "");

			costEntry.TI_OH_Consignee = Helper.NewOrgHeader().PK;
			AssertEquals(false, costEntry.TI_OA_CartageDeliveryAddressOverrideInfo.ReadOnly);

			costEntry.TI_OH_Consignee = ZGuid.Empty;
			AssertEquals(true, costEntry.TI_OA_CartageDeliveryAddressOverrideInfo.ReadOnly);

			costEntry.TI_OH_Consignor = Helper.NewOrgHeader().PK;
			AssertEquals(false, costEntry.TI_OA_CartagePickupAddressOverrideInfo.ReadOnly);

			costEntry.TI_OH_Consignor = ZGuid.Empty;
			AssertEquals(true, costEntry.TI_OA_CartagePickupAddressOverrideInfo.ReadOnly);
		}

		public void TestDeleteLevel1TariffEntryRemovesOveriddenLines()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var tariff1 = Factory.New<CompanyTariff>();
			var tariff1Entry = tariff1.AddRateEntry("ORG");
			var tariff1Line = tariff1Entry.RateLines.AddNew();
			tariff1Line.TL_AC = Helper.ChargeCodes["FRT"].PK;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var tariff2 = factory2.New<CompanyTariff>();
			var tariff2Entry = tariff2.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG)[0];

			AssertEquals("Line is inherited", true, tariff2Entry.RateLines[0].IsTariffLineInherited);
			tariff2Entry.RateLines.OverrideTariffLines(new[] { tariff2Entry.RateLines[0] });
			AssertEquals("Precondition - Line is no longer inherited", false, tariff2Entry.RateLines[0].IsTariffLineInherited);

			factory2.Save();

			tariff1Entry.Delete();
			Factory.Save();
		}

		public void TestInheritedTariffLinesShouldNotHaveCalculatorsInitalisedOnDeletion()
		{
			var level1Tariff = Helper.NewCompanyTariff();
			var level1Entry = level1Tariff.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LSE, "AUSYD", "");
			var rateLine1 = level1Entry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<CartageZoneDistanceCalculator>().EquipmentType = Constants.LCLAIREquipmentNeeded.HandHaulier;
			var rateLine2 = level1Entry.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 50m;
			var level1Factory = level1Tariff.Factory;
			level1Factory.Save();

			var level2Tariff = Helper.NewCompanyTariff();
			level2Tariff.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 10m);
			var level2Collection = level2Tariff.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection;
			level2Collection.LoadAndSortForGUI();
			var level2Factory = level2Tariff.Factory;
			level2Factory.Save();

			var level3Tariff = Helper.NewCompanyTariff();
			level3Tariff.Discounts.SetDiscount(RatingConstants.RateCategory.ORG, 20m);
			level3Tariff.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection.LoadAndSortForGUI();
			var level3Collection = level3Tariff.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection;
			level3Collection.LoadAndSortForGUI();
			var level3Factory = level3Tariff.Factory;
			level3Factory.Save();

			AssertEquals(new ZByte(1), level1Tariff.TH_GlobalRateLevel);
			AssertEquals(new ZByte(2), level2Tariff.TH_GlobalRateLevel);
			AssertEquals(new ZByte(3), level3Tariff.TH_GlobalRateLevel);

			AssertEquals("Should have loaded base tariff's rate entry", 1, level2Collection.Count);

			var level2TariffEntry = level2Collection[0];
			AssertEquals(level1Entry.PK, level2TariffEntry.PK);
			AssertEquals("Thinks it belongs to level 2 tariff (this is expected)", level2Tariff.PK, level2TariffEntry.Parent.PK);
			AssertEquals("Actually belongs to level 1 tariff", level1Tariff.PK, level2TariffEntry.TI_TH);

			var level3TariffEntry = level3Collection[0];
			AssertEquals(level1Entry.PK, level3TariffEntry.PK);
			AssertEquals("Thinks it belongs to level 3 tariff (this is expected)", level3Tariff.PK, level3TariffEntry.Parent.PK);
			AssertEquals("Actually belongs to level 1 tariff", level1Tariff.PK, level3TariffEntry.TI_TH);

			level3TariffEntry.RateLines[0].RateCalculatorChanged = true;
			level3TariffEntry.RateLines[1].RateCalculatorChanged = true;
			AssertEquals("Pre-condition: we reload the factory to ensure the calculator has not been initialized", false, level3TariffEntry.RateLines[0].IsCalculatorInitialized);
			AssertEquals("Pre-condition: we reload the factory to ensure the calculator has not been initialized", false, level3TariffEntry.RateLines[1].IsCalculatorInitialized);

			level3Tariff.Delete();
			AssertNoExceptionThrown("Should not throw any exception deleting the tariff", () => level3Factory.Save());

			AssertEquals("Should not have deleted the actual rate lines as they belong to the level 1 tariff that has not been deleted", false, IsRateLineDeleted(rateLine1.PK));
			AssertEquals(false, IsRateLineDeleted(rateLine2.PK));

			level2TariffEntry.RateLines[0].RateCalculatorChanged = true;
			AssertEquals(false, level2TariffEntry.RateLines[0].IsCalculatorInitialized);

			level2Tariff.Delete();
			AssertNoExceptionThrown("Should not throw any exception deleting the tariff", () => level2Factory.Save());

			AssertEquals(false, IsRateLineDeleted(rateLine1.PK));
			AssertEquals(false, IsRateLineDeleted(rateLine2.PK));

			rateLine1.RateCalculatorChanged = true;
			AssertEquals(false, rateLine1.IsCalculatorInitialized);

			level1Tariff.Delete();
			AssertNoExceptionThrown("Should not throw any exception deleting the tariff", () => level1Factory.Save());

			AssertEquals("Should have been deleted now the actual RatingHeader has been deleted", true, IsRateLineDeleted(rateLine1.PK));
			AssertEquals(true, IsRateLineDeleted(rateLine2.PK));

			bool IsRateLineDeleted(ZGuid rateLinePK)
			{
				Factory.ClearQueryCache();

				var zQuery = new ZDBOnlyQuery(typeof(RateLine));
				zQuery.AddToFilter(RateLinesSchema.PK, rateLinePK);
				return Factory.LoadTop1<RateLine>(zQuery) == null;
			}
		}

		public void TestRelatedEntitiesAreDeletedWithoutLoadingWhenDeletingEntry()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD");
			var line1 = entry.RateLines[0];
			line1.TL_RateCalculator = "FLT";
			line1.GetCalculator<FlatCalculator>().BaseRate = 11;
			line1.TL_Condition = RateLineConditions.UserDefined;
			line1.TL_ConditionalExpression = "MOD=FSA";
			var line2 = entry.AddRateLine("FRT", FlatCalculator.Code);
			line2.GetCalculator<FlatCalculator>().BaseRate = 12;

			var linePKs = new[] { line1.PK, line2.PK };

			var lineItemPKs = Factory.Load<RateLineItem>(new ZQuery(RateLineItemsSchema.TM_TL, linePKs))
				.Select(x => x.PK)
				.ToList();

			var stmNotePKs = Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, linePKs).AddToFilter(StmNoteSchema.ST_Table, line1.TableName))
				.Select(x => x.PK)
				.ToList();

			Assert(lineItemPKs.Count > 0);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var entryInNewFactory = newFactory.Load<RateEntry>(entry.PK);

			entryInNewFactory.Delete();
			newFactory.Save();

			AssertNoRelatedEntityIsLoaded(newFactory);
			AssertRelatedEntitiesAreDeleted(newFactory, linePKs, lineItemPKs, stmNotePKs);
		}

		public void TestContainsLineWithChargeCode()
		{
			var aBC = Factory.New<AccChargeCode>();
			aBC.AC_Code = "ABC";

			var rate = Factory.New<ClientRate>();
			var entry = rate.AddRateEntry("AIR");

			Assert(!entry.ContainsLineWithChargeCode("ABC"));

			var line1 = entry.RateLines.AddNew();
			line1.TL_AC = aBC.PK;

			Assert(entry.ContainsLineWithChargeCode("ABC"));
			Assert(!entry.ContainsLineWithChargeCode("ZUB"));
		}

		public void TestCarrierTwoCharacterCode()
		{
			var iATAAirLine = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, SQLComparisonOperator.NotEqual, ZString.Empty));

			var testAirline = Factory.New<OrgHeader>();
			testAirline.OH_Code = "AIR123";
			testAirline.OH_IsShippingProvider = true;
			testAirline.OH_IsAirLine = true;
			testAirline.MiscServ.OM_RM_Airline = iATAAirLine.PK;

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "USLAX");

			AssertEquals("Blank 2 char code", "", rateEntry.TransportProviderCarrierCode);
			rateEntry.TI_OH_TransportProvider = testAirline.PK;
			AssertEquals("Correct 2 char code", iATAAirLine.RM_TwoCharacterCode, rateEntry.TransportProviderCarrierCode);
		}

		public void TestOriginDestinationTypesSet()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "AUSYD", "USLAX");
			AssertEquals("OriginType", true, rateEntry.Origin() is RefUNLOCO);
			AssertEquals("DestinationType", true, rateEntry.Destination() is RefUNLOCO);

			rateEntry.TI_OriginLRC = "AUEC";
			rateEntry.TI_DestinationLRC = "USCA";
			AssertEquals("OriginType", true, rateEntry.Origin() is RefZoneHeader);
			AssertEquals("DestinationType", true, rateEntry.Destination() is RefZoneHeader);

			rateEntry.TI_OriginLRC = "AU";
			rateEntry.TI_DestinationLRC = "US";
			AssertEquals("OriginType", true, rateEntry.Origin() is RefCountry);
			AssertEquals("DestinationType", true, rateEntry.Destination() is RefCountry);
		}

		public void TestSaleCurrencySetWhenOriginIsSet()
		{
			var testEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.AIR);

			testEntry.TI_OriginLRC = "AUSYD";
			AssertEquals("Sale Currency Set", "AUD", testEntry.TI_RX_NKCurrency);

			testEntry.TI_OriginLRC = "USLAX";
			AssertEquals("Sale Currency Set", "USD", testEntry.TI_RX_NKCurrency);

			testEntry.TI_OriginLRC = "CA";
			AssertEquals("Sale Currency Set", "CAD", testEntry.TI_RX_NKCurrency);

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.CAI);

				testEntry.TI_OriginLRC = "AUSYD";
				AssertEquals("Sale Currency Set", "AUD", testEntry.TI_RX_NKCurrency);

				testEntry.TI_OriginLRC = "USLAX";
				AssertEquals("Sale Currency Set", "USD", testEntry.TI_RX_NKCurrency);

				testEntry.TI_OriginLRC = "CA";
				AssertEquals("Sale Currency Set", "CAD", testEntry.TI_RX_NKCurrency);
			}
		}

		public void TestContainerSetToNullIfModeNotContainerised()
		{
			var testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("ORG", "FCL", "", "", "", "20GP");

			testEntry.TI_Mode = Core.Constants.RateMode.LCL;
			AssertEquals("Container Empty", ZGuid.Empty, testEntry.TI_RC);

			testEntry.TI_Mode = Core.Constants.RateMode.ULD;
			testEntry.TI_RC = Helper.Containers["20GP"].PK;

			testEntry.TI_Mode = Core.Constants.RateMode.FTL;
			AssertNotEquals("Container Not Empty", ZGuid.Empty, testEntry.TI_RC);

			testEntry.TI_Mode = Core.Constants.RateMode.LSE;
			AssertEquals("Container Empty", ZGuid.Empty, testEntry.TI_RC);

			testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.WHS);
			testEntry.TI_RC = Helper.Containers["20GP"].PK;
			testEntry.TI_Mode = Core.Constants.RateMode.ALL;
			AssertNotEquals("Container Not Empty", ZGuid.Empty, testEntry.TI_RC);

			testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.TRW);
			testEntry.TI_RC = Helper.Containers["20GP"].PK;
			testEntry.TI_Mode = Core.Constants.RateMode.ALL;
			AssertNotEquals("Container Not Empty", ZGuid.Empty, testEntry.TI_RC);

			testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.TWU);
			testEntry.TI_RC = Helper.Containers["20GP"].PK;
			testEntry.TI_Mode = Core.Constants.RateMode.ALL;
			AssertNotEquals("Container Not Empty", ZGuid.Empty, testEntry.TI_RC);
			testEntry.TI_Mode = Core.Constants.RateMode.AIR;
			AssertNotEquals("Container Not Empty", ZGuid.Empty, testEntry.TI_RC);
			testEntry.TI_Mode = Core.Constants.RateMode.SEA;
			AssertNotEquals("Container Not Empty", ZGuid.Empty, testEntry.TI_RC);
			testEntry.TI_Mode = Core.Constants.RateMode.ROA;
			AssertNotEquals("Container Not Empty", ZGuid.Empty, testEntry.TI_RC);
		}

		public void TestValidateTI_RCWhenTI_ModeChange()
		{
			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR");

			entry.TI_Mode = Core.Constants.RateMode.LSE;
			AssertEquals("Read Only", true, entry.TI_RCInfo.ReadOnly);
			AssertEquals("Container Empty", ZGuid.Empty, entry.TI_RC);
			AssertNoErrors(entry.TI_RCInfo);

			entry.TI_Mode = Core.Constants.RateMode.ULD;
			AssertEquals("Read Only", false, entry.TI_RCInfo.ReadOnly);
			AssertEquals("Container Empty", ZGuid.Empty, entry.TI_RC);
			AssertHasError(entry.TI_RCInfo, "Please enter a " + entry.TI_RCInfo.HumanReadableName + ".");
		}

		public void TestWeightVolumeChangedForAirULD()
		{
			var testEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR");

			AssertEquals("LSE", testEntry.TI_Mode);
			AssertEquals("KG", testEntry.Unit);

			testEntry.TI_Mode = "ULD";
			AssertEquals("CN", testEntry.Unit);

			testEntry.TI_Mode = "LSE";
			AssertEquals("KG", testEntry.Unit);
		}

		public void TestUnit()
		{
			AssertModeAndUnit(Category.AIR, mode: Mode.LSE, expectedUnit: QuantityUnit.KG);
			AssertModeAndUnit(Category.AIR, mode: Mode.ULD, expectedUnit: QuantityUnit.CN);
			AssertModeAndUnit(Category.AIR, mode: Mode.LSE, expectedUnit: QuantityUnit.KG);

			AssertModeAndUnit(Category.CAI, mode: Mode.LSE, expectedUnit: QuantityUnit.KG);
			AssertModeAndUnit(Category.CAI, mode: Mode.ULD, expectedUnit: QuantityUnit.CN);

			AssertModeAndUnit(Category.SCO, mode: Mode.SEA, expectedUnit: QuantityUnit.CN);
			AssertModeAndUnit(Category.FCL, mode: Mode.SEA, expectedUnit: QuantityUnit.CN);
			AssertModeAndUnit(Category.CFC, mode: Mode.SEA, expectedUnit: QuantityUnit.CN);

			AssertModeAndUnit(Category.SNC, mode: Mode.LCL, expectedUnit: QuantityUnit.M3);
			AssertModeAndUnit(Category.LCL, mode: Mode.LCL, expectedUnit: QuantityUnit.M3);
			AssertModeAndUnit(Category.CLC, mode: Mode.LCL, expectedUnit: QuantityUnit.M3);

			AssertModeAndUnit(Category.SID, mode: Mode.SEA, expectedUnit: QuantityUnit.DY);
			AssertModeAndUnit(Category.SED, mode: Mode.SEA, expectedUnit: QuantityUnit.DY);

			void AssertModeAndUnit(string category, string mode, string expectedUnit)
			{
				var testEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(category, mode);
				AssertEquals($"{category}-{mode}: Unit", expectedUnit, testEntry.Unit);
			}
		}

		public void TestSaleCurrency_CustomsRateEntry()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var caiRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CAI, Constants.RateMode.ULD, "HKHKG", "NZAKL", "FRT", 10m);
			var cfcRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CFC, Constants.RateMode.SEA, "HKHKG", "NZAKL", "FRT", 20m);
			var clcRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CLC, Constants.RateMode.LCL, "HKHKG", "NZAKL", "FRT", 30m);
			var corRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.COR, Constants.RateMode.AIR, "HKHKG", "NZAKL", "FRT", 40m);
			var cdsRateEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.CDS, Constants.RateMode.BLK, "HKHKG", "NZAKL", "FRT", 50m);

			CombineAssertions(() =>
			{
				AssertEquals("CAI RateEntry currency should be set to Origin currency", "HKD", caiRateEntry.RateLines[0].TL_RX_NKCurrency);
				AssertEquals("CFC RateEntry currency should be set to USD", "USD", cfcRateEntry.RateLines[0].TL_RX_NKCurrency);
				AssertEquals("CLC RateEntry currency should be set to USD", "USD", clcRateEntry.RateLines[0].TL_RX_NKCurrency);
				AssertEquals("COR RateEntry currency should be set to Origin currency", "HKD", corRateEntry.RateLines[0].TL_RX_NKCurrency);
				AssertEquals("CDS RateEntry currency should be set to Destination currency", "NZD", cdsRateEntry.RateLines[0].TL_RX_NKCurrency);
			});
		}

		public void TestSaleCurrencySetInLines()
		{
			var testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.ORG);
			testEntry.RateLines.AddNew();
			testEntry.RateLines.AddNew();
			testEntry.TI_RX_NKCurrency = "EUR";

			AssertEquals("Currency in Line0 Set", "EUR", testEntry.RateLines[0].TL_RX_NKCurrency);
			AssertEquals("Currency in Line1 Set", "EUR", testEntry.RateLines[1].TL_RX_NKCurrency);

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testEntry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.COR);
				testEntry.RateLines.AddNew();
				testEntry.RateLines.AddNew();
				testEntry.TI_RX_NKCurrency = "EUR";

				AssertEquals("Currency in Line0 Set", "EUR", testEntry.RateLines[0].TL_RX_NKCurrency);
				AssertEquals("Currency in Line1 Set", "EUR", testEntry.RateLines[1].TL_RX_NKCurrency);
			}
		}

		public void TestWeightVolumeSetInLines()
		{
			var testEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR");
			testEntry.RateLines.AddNew();

			AssertEquals("Currency in Line0 Set", "KG", testEntry.RateLines[0].TL_WeightVolume);
			AssertEquals("Currency in Line1 Set", "KG", testEntry.RateLines[1].TL_WeightVolume);
		}

		public void TestCurrencySetToUSDOnLCLFCL()
		{
			var testEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.LCL);
			testEntry.TI_RX_NKCurrency = "";
			testEntry.TI_OriginLRC = "CATOR";
			AssertEquals("Sale Currency", "USD", testEntry.TI_RX_NKCurrency);
			testEntry.TI_RX_NKCurrency = "";
			testEntry.TI_OriginLRC = "GB";
			AssertEquals("Sale Currency", "USD", testEntry.TI_RX_NKCurrency);
			testEntry.TI_RX_NKCurrency = "";
			testEntry.TI_OriginLRC = "NZNI";
			AssertEquals("Sale Currency", "USD", testEntry.TI_RX_NKCurrency);

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				testEntry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry(RatingConstants.RateCategory.CLC);
				testEntry.TI_RX_NKCurrency = "";
				testEntry.TI_OriginLRC = "CATOR";
				AssertEquals("Sale Currency", "USD", testEntry.TI_RX_NKCurrency);
				testEntry.TI_RX_NKCurrency = "";
				testEntry.TI_OriginLRC = "GB";
				AssertEquals("Sale Currency", "USD", testEntry.TI_RX_NKCurrency);
				testEntry.TI_RX_NKCurrency = "";
				testEntry.TI_OriginLRC = "NZNI";
				AssertEquals("Sale Currency", "USD", testEntry.TI_RX_NKCurrency);
			}
		}

		public void TestCurrencySetWithDifferentGlbCompany()
		{
			ClientRate rate;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "NEW";
			var branch = company.Branches.AddNew();
			company.GC_RN_NKCountryCode = "UA";
			company.GC_RX_NKLocalCurrency = "UAH";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				rate = Helper.NewClientRate(Helper.NewOrgHeader());
			}

			var regionWithoutCountry = Helper.NewInternationalZone("TSTR", null, "INBOM", "SGSIN");
			AssertNull(((ILocation)regionWithoutCountry).Country);

			var entry = rate.AddRateEntry("ORG", "AIR", "USLAX", "");
			AssertEquals("USD", entry.Currency.RX_Code);

			entry.TI_OriginLRC = "TSTR";
			AssertEquals("UAH", entry.Currency.RX_Code);
		}

		public void TestHasRateLines()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG);
			AssertEquals("Has Rate Lines", false, rateEntry.HasRateLines);
			rateEntry.RateLines.AddNew();
			AssertEquals("Has Rate Lines", true, rateEntry.HasRateLines);
			rateEntry.RateLines.RemoveAll();
			AssertEquals("Has Rate Lines", false, rateEntry.HasRateLines);
		}

		public void TestContainerReadOnlyORGAndDST()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var testEntry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.AIR, "", "");
			AssertEquals("Read Only", true, testEntry.TI_RCInfo.ReadOnly);
			AssertEquals("Read Only", true, testEntry.TI_MatchContainerRateClassInfo.ReadOnly);

			testEntry.TI_Mode = Core.Constants.RateMode.ULD;
			AssertEquals("Read Only", false, testEntry.TI_RCInfo.ReadOnly);
			AssertEquals("Read Only", false, testEntry.TI_MatchContainerRateClassInfo.ReadOnly);

			testEntry.TI_Mode = Core.Constants.RateMode.LCL;
			AssertEquals("Read Only", true, testEntry.TI_RCInfo.ReadOnly);
			AssertEquals("Read Only", true, testEntry.TI_MatchContainerRateClassInfo.ReadOnly);

			testEntry.TI_Mode = Core.Constants.RateMode.FCL;
			AssertEquals("Read Only", false, testEntry.TI_RCInfo.ReadOnly);
			AssertEquals("Read Only", false, testEntry.TI_MatchContainerRateClassInfo.ReadOnly);

			testEntry.TI_RateCategory = RatingConstants.RateCategory.DST;
			testEntry.TI_Mode = Core.Constants.RateMode.AIR;
			AssertEquals("Read Only", true, testEntry.TI_RCInfo.ReadOnly);
			AssertEquals("Read Only", true, testEntry.TI_MatchContainerRateClassInfo.ReadOnly);

			testEntry.TI_Mode = Core.Constants.RateMode.LSE;
			AssertEquals("Read Only", true, testEntry.TI_RCInfo.ReadOnly);
			AssertEquals("Read Only", true, testEntry.TI_MatchContainerRateClassInfo.ReadOnly);

			testEntry.TI_Mode = Core.Constants.RateMode.ULD;
			AssertEquals("Read Only", false, testEntry.TI_RCInfo.ReadOnly);
			AssertEquals("Read Only", false, testEntry.TI_MatchContainerRateClassInfo.ReadOnly);

			testEntry.TI_Mode = Core.Constants.RateMode.LCL;
			AssertEquals("Read Only", true, testEntry.TI_RCInfo.ReadOnly);
			AssertEquals("Read Only", true, testEntry.TI_MatchContainerRateClassInfo.ReadOnly);

			testEntry.TI_Mode = Core.Constants.RateMode.FCL;
			AssertEquals("Read Only", false, testEntry.TI_RCInfo.ReadOnly);
			AssertEquals("Read Only", false, testEntry.TI_MatchContainerRateClassInfo.ReadOnly);

			testEntry.TI_Mode = Core.Constants.RateMode.FTL;
			AssertEquals("Read Only", false, testEntry.TI_RCInfo.ReadOnly);
			AssertEquals("Read Only", false, testEntry.TI_MatchContainerRateClassInfo.ReadOnly);
		}

		public void TestHBLDeliveryMode_ReadOnly()
		{
			var clientRate = Helper.NewClientRate(Factory.NewWithValidTestData<OrgHeader>());

			var testEntry = clientRate.AddRateEntry(Category.AIR, Mode.LSE, "", "");
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry.TI_Mode = Mode.ULD;
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry.TI_Mode = Mode.BCN;
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry.TI_Mode = Mode.FCL;
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry.TI_Mode = Mode.LCL;
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry.TI_Mode = Mode.BBK;
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry.TI_Mode = Mode.BLK;
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry.TI_Mode = Mode.ROR;
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry.TI_Mode = Mode.LSE;
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry.TI_HBLDeliveryMode = Constants.HBLDeliveryModes.Codes.DOOR_DOOR;

			testEntry.TI_Mode = Mode.SCN;
			AssertEquals("Read Only", true, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);
			AssertEquals("We should clear HBL Delivery Mode", string.Empty, testEntry.TI_HBLDeliveryMode);

			testEntry.TI_Mode = Mode.ALL;
			AssertEquals("Read Only", true, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry = clientRate.AddRateEntry(Category.FCL, Mode.SEA, "", "");
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry.TI_Mode = Mode.ROA;
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry.TI_Mode = Mode.RAI;
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry.TI_Mode = Mode.FRA;
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);

			testEntry.TI_Mode = Mode.FRO;
			AssertEquals("Not Read Only", false, testEntry.TI_HBLDeliveryModeInfo.ReadOnly);
		}

		public void TestIsNonOperatedReefer_ReadOnly()
		{
			var rate = Helper.NewClientRate(Factory.NewWithValidTestData<OrgHeader>());
			var entry = rate.AddRateEntry(Category.ORG, Mode.FCL);
			AssertEquals("Not Read Only", false, entry.TI_IsNonOperatedReeferInfo.ReadOnly);

			entry.TI_Mode = Mode.FRA;
			AssertEquals("Not Read Only", false, entry.TI_IsNonOperatedReeferInfo.ReadOnly);

			entry.TI_Mode = Mode.FRO;
			AssertEquals("Not Read Only", false, entry.TI_IsNonOperatedReeferInfo.ReadOnly);

			entry.TI_Mode = Mode.SCN;
			AssertEquals("Not Read Only", false, entry.TI_IsNonOperatedReeferInfo.ReadOnly);

			entry.TI_Mode = Mode.BCN;
			AssertEquals("Not Read Only", false, entry.TI_IsNonOperatedReeferInfo.ReadOnly);

			entry.TI_Mode = Mode.ULD;
			AssertEquals("Read Only", true, entry.TI_IsNonOperatedReeferInfo.ReadOnly);
		}

		public void TestIsNonOperatingReefer_ValueClearedWhenReadonly()
		{
			var quote = Helper.NewQuote(Factory.NewWithValidTestData<OrgHeader>());
			var testEntry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, Mode.AIR);
			testEntry.TI_IsNonOperatedReefer = "Y";

			testEntry.TI_Mode = Mode.FCL;
			AssertEquals("Value Stays when switching invalid mode to valid mode", "Y", testEntry.TI_IsNonOperatedReefer);

			testEntry.TI_Mode = Mode.BCN;
			AssertEquals("Value Stays when switching between 2 valid modes", "Y", testEntry.TI_IsNonOperatedReefer);

			testEntry.TI_Mode = Mode.ALL;
			AssertEquals("Value gets removed when switching to an invalid mode", "", testEntry.TI_IsNonOperatedReefer);
		}

		public void TestTransitTimeLists()
		{
			var testEntry = Factory.New<QuoteEntry>();
			testEntry.TI_RateCategory = RatingConstants.RateCategory.AIR;
			AssertEquals("Days and Sameday and Overnight available transit times for AIR", true, testEntry.Lookups.AirTransitTimes.ContainsCode(RatingConstants.TransitTimes.SameDay));
			AssertEquals("Days and Sameday and Overnight available transit times for AIR", true, testEntry.Lookups.AirTransitTimes.ContainsCode(RatingConstants.TransitTimes.Overnight));
			AssertEquals("Days and Sameday and Overnight available transit times for AIR", 122, testEntry.Lookups.AirTransitTimes.Count);

			testEntry = Factory.New<QuoteEntry>();
			testEntry.TI_RateCategory = RatingConstants.RateCategory.FCL;
			AssertEquals("Sameday and Overnight not available transit times for AIR", false, testEntry.Lookups.SeaTransitTimes.ContainsCode(RatingConstants.TransitTimes.SameDay));
			AssertEquals("Only Sameday and Overnight available transit times for AIR", false, testEntry.Lookups.SeaTransitTimes.ContainsCode(RatingConstants.TransitTimes.Overnight));
			AssertEquals("Only Sameday and Overnight available transit times for AIR", 120, testEntry.Lookups.SeaTransitTimes.Count);
		}

		public void TestIsDuplicate()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var airRateEntry1 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON", "STD", "");
			airRateEntry1.TI_ViaLRC = "SGSIN";
			airRateEntry1.TI_PL_NKCarrierServiceLevel = "XYZ";

			airRateEntry1.TI_OH_TransportProvider = TransportProvider1.PK;
			var airRateEntry2 = testRate.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON", "STD", "");
			airRateEntry2.TI_ViaLRC = "SGSIN";
			airRateEntry2.TI_PL_NKCarrierServiceLevel = "DEF";
			airRateEntry2.TI_OH_TransportProvider = TransportProvider1.PK;

			AssertEquals("Are Equal", false, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry2.TI_PL_NKCarrierServiceLevel = "XYZ";
			AssertEquals("Are Equal", true, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry2.TI_IsCrossTrade = true;
			AssertEquals("Are Equal", false, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry1.TI_IsCrossTrade = true;
			AssertEquals("Are Equal", true, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry2.TI_OriginLRC = "AUMEL";
			AssertEquals("Are Equal", false, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry1.TI_OriginLRC = "AUSYD";
			airRateEntry2.TI_OriginLRC = "AUSYD";
			AssertEquals("Are Equal", true, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry2.TI_WW_Warehouse = ZGuid.NewZGuid();
			AssertEquals("Are Equal", false, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry1.TI_WW_Warehouse = ZGuid.NewZGuid();
			AssertEquals("Are Equal", false, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry1.TI_WW_Warehouse = airRateEntry2.TI_WW_Warehouse;
			AssertEquals("Are Equal", true, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry1.TI_CYC_WW_Facility = ZGuid.NewZGuid();
			AssertEquals("Are Equal", false, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry1.TI_CYC_WW_Facility = ZGuid.NewZGuid();
			AssertEquals("Are Equal", false, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry1.TI_CYC_WW_Facility = airRateEntry2.TI_CYC_WW_Facility;
			AssertEquals("Are Equal", true, airRateEntry1.IsDuplicate(airRateEntry2));

			var hash1 = airRateEntry1.GetKeyForDuplicateSearch();
			airRateEntry1.TI_TZ_OriginZone = ZGuid.NewZGuid();
			var hash2 = airRateEntry1.GetKeyForDuplicateSearch();

			AssertNotEquals(hash1, hash2);

			AssertEquals("Are Equal", false, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry2.TI_TZ_OriginZone = airRateEntry1.TI_TZ_OriginZone;
			AssertEquals("Are Equal", true, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry1.TI_TZ_DestinationZone = ZGuid.NewZGuid();
			AssertEquals("Are Equal", false, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry2.TI_TZ_DestinationZone = ZGuid.NewZGuid();
			AssertEquals("Are Equal", false, airRateEntry1.IsDuplicate(airRateEntry2));

			airRateEntry1.TI_TZ_DestinationZone = airRateEntry2.TI_TZ_DestinationZone;
			AssertEquals("Are Equal", true, airRateEntry1.IsDuplicate(airRateEntry2));
			AssertEquals(airRateEntry1.GetKeyForDuplicateSearch(), airRateEntry2.GetKeyForDuplicateSearch());
		}

		#region FCL Container Class

		public void TestGetKeyForDuplicateSearch_MatchContainerRateClassIsTrue_ReturnKeyWithRateContainerClass()
		{
			var container = Helper.Containers["20GP"];
			container.RC_FreightRateClass = "20FN";
			container.RC_HandlingRateClass = "20HN";

			var rate = Factory.NewWithValidTestData<ClientRate>();
			var fclEntry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			fclEntry.TI_RC = container.PK;
			var hash = fclEntry.GetKeyForDuplicateSearch();
			AssertContains("Hash has TI_RC", container.PK.ToString(), hash);

			fclEntry.TI_MatchContainerRateClass = true;
			hash = fclEntry.GetKeyForDuplicateSearch();
			AssertNotContains("Hash has NO TI_RC", container.PK.ToString(), hash);
			AssertContains("Hash has container class instead", container.RC_FreightRateClass, hash);

			var dstEntry = rate.AddRateEntry("DST", "SEA", "AUSYD", "USLAX");
			dstEntry.TI_RC = container.PK;
			hash = dstEntry.GetKeyForDuplicateSearch();
			AssertContains("Hash has TI_RC", container.PK.ToString(), hash);

			dstEntry.TI_MatchContainerRateClass = true;
			hash = dstEntry.GetKeyForDuplicateSearch();
			AssertNotContains("Hash has NO TI_RC", container.PK.ToString(), hash);
			AssertContains("Hash has container handling class instead", container.RC_HandlingRateClass, hash);
		}

		public void TestIsDuplicate_RatesHaveSameContainerClassAndMatchContainerRateClassIsTrue_ReturnTrue()
		{
			var container1 = Factory.New<RefContainer>();
			container1.RC_Code = "Z1";
			container1.RC_FreightRateClass = "ZUB";

			var container2 = Factory.New<RefContainer>();
			container2.RC_Code = "Z2";
			container2.RC_FreightRateClass = "ZUB";

			var rate = Factory.New<ClientRate>();
			var fclEntry1 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "Z1");
			fclEntry1.TI_RateStartDate = ZDate.Today;
			fclEntry1.TI_RateEndDate = ZDate.Today.AddDays(2);
			fclEntry1.TI_MatchContainerRateClass = false;

			var fclEntry2 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "Z2");
			fclEntry2.TI_RateStartDate = ZDate.Today.AddDays(1);
			fclEntry2.TI_RateEndDate = ZDate.Today.AddDays(6);
			fclEntry2.TI_MatchContainerRateClass = false;

			AssertEquals("Entries have no overlap since RC_Code are different", false, fclEntry1.IsDuplicate(fclEntry2));

			fclEntry1.TI_MatchContainerRateClass = true;
			fclEntry2.TI_MatchContainerRateClass = true;
			AssertEquals("Entries have overlap since RC_FreightRateClass are the same", true, fclEntry1.IsDuplicate(fclEntry2));
		}

		public void TestValidateOnly1MatchPerContainerClass()
		{
			var container1 = Factory.New<RefContainer>();
			container1.RC_FreightRateClass = "ZUB";
			container1.RC_Code = "Z1";

			var container2 = Factory.New<RefContainer>();
			container2.RC_FreightRateClass = "ZUB";
			container2.RC_Code = "Z2";

			var rate = Factory.New<ClientRate>();
			var fCLEntry1 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "Z1");
			fCLEntry1.TI_RateStartDate = ZDate.Today;
			fCLEntry1.TI_RateEndDate = ZDate.Today.AddDays(2);
			var fCLEntry2 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "Z2");
			fCLEntry2.TI_RateStartDate = ZDate.Today.AddDays(3);
			fCLEntry2.TI_RateEndDate = ZDate.Today.AddDays(6);

			rate.EntryCollectionValidator.Validate();
			AssertEquals("No Errors on either", false, fCLEntry1.HasErrors);
			AssertEquals("No Errors on either", false, fCLEntry2.HasErrors);

			fCLEntry1.TI_MatchContainerRateClass = true;
			rate.EntryCollectionValidator.Validate();
			AssertEquals("No Errors on either", false, fCLEntry1.HasErrors);
			AssertEquals("No Errors on either", false, fCLEntry2.HasErrors);

			fCLEntry2.TI_MatchContainerRateClass = true;
			rate.EntryCollectionValidator.Validate();
			AssertEquals("No Errors on entry 2", false, fCLEntry2.HasErrors);
			AssertEquals("No Error on entry 1 as dates don't overlap", false, fCLEntry1.HasErrors);

			fCLEntry2.TI_MatchContainerRateClass = false;

			fCLEntry2.TI_RateStartDate = ZDate.Today;
			fCLEntry2.TI_RateEndDate = ZDate.Today.AddDays(2);

			fCLEntry2.TI_MatchContainerRateClass = true;

			rate.EntryCollectionValidator.Validate();
			AssertEquals("Error on entry 2 as rates overlap", true, fCLEntry2.HasErrors);
		}

		public void TestValidateMatchClassNoContainer()
		{
			var container1 = Factory.New<RefContainer>();
			container1.RC_FreightRateClass = "ZUB";
			container1.RC_Code = "Z1";

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var fCLEntry1 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");

			fCLEntry1.TI_MatchContainerRateClass = true;
			AssertEquals("Error on entry 1 regarding no container specified", true, fCLEntry1.HasErrors);

			fCLEntry1.TI_RC = container1.PK;
			fCLEntry1.RunPreSaveValidation();
			AssertEquals("No Errors on entry", false, fCLEntry1.HasErrors);
		}

		public void TestValidateMatchClassNoClassOnContainer()
		{
			var container1 = Factory.New<RefContainer>();
			container1.RC_Code = "Z1";

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var fCLEntry1 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "Z1");

			AssertEquals("No Errors on entry", false, fCLEntry1.HasErrors);

			fCLEntry1.TI_MatchContainerRateClass = true;

			AssertEquals("Error on entry 1", true, fCLEntry1.HasErrors);
		}

		#endregion

		public void TestDefaultEndDate()
		{
			var testRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = testRate.AddRateEntry("AIR");

			ZDateTime endDate = entry.DefaultEndDate(-6);
			AssertEquals("End Day+1 Day", 1, endDate.AddDays(1).Day);

			endDate = entry.DefaultEndDate(6);
			AssertEquals("End Day", ZDateTime.Today.AddMonths(6), endDate);

			endDate = entry.DefaultEndDate(0);
			AssertEquals("Entry should have infinite validity", ZDateTime.Empty, endDate);
		}

		public void TestPostCodesAreTakenFromAppropriateAddressWhenAvailable()
		{
			var fromOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = fromOrganisation.Addresses.AddNew();
			orgAddress1.OA_Address1 = "Sydney Address";
			orgAddress1.OA_PostCode = "2000";

			var client = Factory.NewWithValidTestData<OrgHeader>();
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.TRN, "FRO", "AU", "");

			rateEntry.TI_OH_Consignor = fromOrganisation.PK;
			rateEntry.TI_CartagePickupAddressPostCode = "1000";

			Assert(!rateEntry.TI_CartagePickupAddressPostCodeInfo.ReadOnly);

			rateEntry.TI_OA_CartagePickupAddressOverride = orgAddress1.PK;

			Assert(rateEntry.TI_CartagePickupAddressPostCodeInfo.ReadOnly);
			AssertEquals(orgAddress1.OA_PostCode, rateEntry.TI_CartagePickupAddressPostCode);

			orgAddress1.OA_PostCode = "3000";

			AssertEquals(orgAddress1.OA_PostCode, rateEntry.TI_CartagePickupAddressPostCode);

			rateEntry.TI_OA_CartagePickupAddressOverride = ZGuid.Empty;

			Assert(rateEntry.TI_CartagePickupAddressPostCode.IsEmpty);

			rateEntry.TI_CartagePickupAddressPostCode = "1000";

			AssertEquals("1000", rateEntry.TI_CartagePickupAddressPostCode);

			var toOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress2 = fromOrganisation.Addresses.AddNew();
			orgAddress2.OA_Address1 = "Melbourne Address";
			orgAddress2.OA_PostCode = "3000";

			rateEntry.TI_OH_Consignee = toOrganisation.PK;
			rateEntry.TI_CartageDeliveryAddressPostCode = "1500";

			Assert(!rateEntry.TI_CartageDeliveryAddressPostCodeInfo.ReadOnly);

			rateEntry.TI_OA_CartageDeliveryAddressOverride = orgAddress2.PK;

			Assert(rateEntry.TI_CartageDeliveryAddressPostCodeInfo.ReadOnly);
			AssertEquals(orgAddress2.OA_PostCode, rateEntry.TI_CartageDeliveryAddressPostCode);

			orgAddress1.OA_PostCode = "4000";

			AssertEquals(orgAddress2.OA_PostCode, rateEntry.TI_CartageDeliveryAddressPostCode);

			rateEntry.TI_OA_CartageDeliveryAddressOverride = ZGuid.Empty;

			Assert(rateEntry.TI_CartageDeliveryAddressPostCode.IsEmpty);

			rateEntry.TI_CartageDeliveryAddressPostCode = "1500";

			AssertEquals("1500", rateEntry.TI_CartageDeliveryAddressPostCode);
		}

		public void TestOrganisation()
		{
			var client = Helper.CreateCreditor("SOMEORG");
			var clientRate = Helper.NewClientRate(client);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AU", "");

			AssertEquals("SOMEORG", rateEntry.Organisation);

			var companyTariff = Factory.New<CompanyTariff>();
			rateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.ALL, "AU", "");

			AssertEquals("Organisation should be empty", ZString.Empty, rateEntry.Organisation);
		}

		#region TestTI_Mode

		public void TestTI_Mode_ChangingModeChangesRateLineWeightVolume()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR);
			var line1 = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);
			var line2 = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);

			AssertEquals("Precondition: Line Weight Volume should be set to Kilograms.", QuantityUnit.KG, line1.TL_WeightVolume);
			AssertEquals("Precondition: Line Weight Volume should be set to Kilograms.", QuantityUnit.KG, line2.TL_WeightVolume);

			rateEntry.TI_Mode = Core.Constants.RateMode.ULD;
			AssertEquals("Line Weight Volume should be set to Container after changing Mode to ULD.", QuantityUnit.CN, line1.TL_WeightVolume);
			AssertEquals("Line Weight Volume should be set to Container after changing Mode to ULD.", QuantityUnit.CN, line2.TL_WeightVolume);

			rateEntry.TI_Mode = Core.Constants.RateMode.LSE;
			AssertEquals("Line Weight Volume should be set to Kilograms after changing Mode to LSE.", QuantityUnit.KG, line1.TL_WeightVolume);
			AssertEquals("Line Weight Volume should be set to Kilograms after changing Mode to LSE.", QuantityUnit.KG, line2.TL_WeightVolume);

			line1.TL_WeightVolume = "";
			line2.TL_WeightVolume = "";
			AssertEquals("Line Weight Volume should be empty.", ZString.Empty, line1.TL_WeightVolume);
			AssertEquals("Line Weight Volume should be empty.", ZString.Empty, line2.TL_WeightVolume);

			rateEntry.TI_Mode = Core.Constants.RateMode.ALL;
			AssertEquals("Line Weight Volume should be unchanged when changing to a mode that is neither LSE or ULD.", ZString.Empty, line1.TL_WeightVolume);
			AssertEquals("Line Weight Volume should be unchanged by changing to a mode that is neither LSE or ULD.", ZString.Empty, line2.TL_WeightVolume);
		}

		public void TestTI_Mode_ChangingModeDoesNotChangeRateLinesWithReadOnlyWeightVolume()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR);
			var line1 = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);
			var line2 = rateEntry.AddRateLine("FRT", PercentageCalculator.Code);
			var line3 = rateEntry.AddRateLine("FRT", AgencyCalculator.Code);

			AssertEquals("Precondition: Line Weight Volume should be set to Kilograms.", QuantityUnit.KG, line1.TL_WeightVolume);
			AssertEquals("Precondition: Line Weight Volume should not be a read only field.", false, line1.TL_WeightVolumeInfo.ReadOnly);
			AssertEquals("Precondition: Line Weight Volume should be empty.", ZString.Empty, line2.TL_WeightVolume);
			AssertEquals("Precondition: Line Weight Volume should be a Read Only field.", true, line2.TL_WeightVolumeInfo.ReadOnly);
			AssertEquals("Precondition: Line Weight Volume should be empty.", ZString.Empty, line3.TL_WeightVolume);
			AssertEquals("Precondition: Line Weight Volume should be a Read Only field.", true, line3.TL_WeightVolumeInfo.ReadOnly);

			rateEntry.TI_Mode = Core.Constants.RateMode.ULD;
			AssertEquals("Line Weight Volume should be set to Container after changing Mode to ULD.", QuantityUnit.CN, line1.TL_WeightVolume);
			AssertEquals("Lines with Read Only Weight Volume should not have a value for field.", ZString.Empty, line2.TL_WeightVolume);
			AssertEquals("Lines with Read Only Weight Volume should not have a value for field.", ZString.Empty, line3.TL_WeightVolume);

			rateEntry.TI_Mode = Core.Constants.RateMode.LSE;
			AssertEquals("Line Weight Volume should be  set to Kilograms after changing Mode to ULD.", QuantityUnit.KG, line1.TL_WeightVolume);
			AssertEquals("Lines with Read Only Weight Volume should not have a value for field.", ZString.Empty, line2.TL_WeightVolume);
			AssertEquals("Lines with Read Only Weight Volume should not have a value for field.", ZString.Empty, line3.TL_WeightVolume);

			rateEntry.TI_Mode = Core.Constants.RateMode.ALL;
			AssertEquals("Line Weight Volume should be unchanged when changing to a mode that is neither LSE or ULD.", QuantityUnit.KG, line1.TL_WeightVolume);
			AssertEquals("Lines with Read Only Weight Volume should not have a value for field.", ZString.Empty, line2.TL_WeightVolume);
			AssertEquals("Lines with Read Only Weight Volume should not have a value for field.", ZString.Empty, line3.TL_WeightVolume);
		}

		#endregion

		#region ContainerPayloadWeight

		public void TestContainerPayloadWeight_ReturnContainerNetWeight()
		{
			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(Category.AIR);

			entry.TI_RC = ZGuid.Empty;
			AssertEquals("Initial payload weight should be zero when TI_RC is empty.", 0m, entry.ContainerPayloadWeight);

			var container = Helper.Containers["20GP"];
			container.RC_GrossWeight = 500;
			container.RC_TareWeight = 50;

			entry.TI_RC = container.PK;
			AssertEquals("Payload weight should be calculated as gross weight minus tare weight.", 450m, entry.ContainerPayloadWeight);
		}

		#endregion

		#region ContainerPayloadVolume

		public void TestContainerPayloadVolume_ReturnContainerCapacity()
		{
			var rate = Helper.NewCosting(null);
			var entry = rate.AddRateEntry(Category.AIR);

			entry.TI_RC = ZGuid.Empty;
			AssertEquals("Initial payload weight should be zero.", 0m, entry.ContainerPayloadWeight);

			var container = Helper.Containers["20GP"];
			container.RC_CubicCapacity = 1.5m;

			entry.TI_RC = container.PK;
			AssertEquals("The payload volume should match the container's cubic capacity.", 1.5m, entry.ContainerPayloadVolume);
		}

		#endregion

		#region TestUnits

		public void TestUnits_ChangingUnitChangesRateLineWeightVolume()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR);
			var line1 = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);
			var line2 = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);

			AssertEquals("Precondition: Line Weight Volume should be set to Kilograms.", QuantityUnit.KG, line1.TL_WeightVolume);
			AssertEquals("Precondition: Line Weight Volume should be set to Kilograms.", QuantityUnit.KG, line2.TL_WeightVolume);

			rateEntry.Unit = QuantityUnit.CN;
			AssertEquals("Line Weight Volume should match unit value.", QuantityUnit.CN, line1.TL_WeightVolume);
			AssertEquals("Line Weight Volume should match unit value.", QuantityUnit.CN, line2.TL_WeightVolume);

			rateEntry.Unit = Constants.PkgUnit.Package;
			AssertEquals("Line Weight Volume should match unit value.", Constants.PkgUnit.Package, line1.TL_WeightVolume);
			AssertEquals("Line Weight Volume should match unit value.", Constants.PkgUnit.Package, line2.TL_WeightVolume);

			rateEntry.Unit = QuantityUnit.M3;
			AssertEquals("Line Weight Volume should match unit value.", QuantityUnit.M3, line1.TL_WeightVolume);
			AssertEquals("Line Weight Volume should match unit value.", QuantityUnit.M3, line2.TL_WeightVolume);
		}

		public void TestUnits_ChangingUnitDoesNotChangeRateLinesWithReadOnlyWeightVolume()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR);
			var line1 = rateEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG);
			var line2 = rateEntry.AddRateLine("FRT", PercentageCalculator.Code);
			var line3 = rateEntry.AddRateLine("FRT", AgencyCalculator.Code);

			AssertEquals("Precondition: Line Weight Volume should be empty.", QuantityUnit.KG, line1.TL_WeightVolume);
			AssertEquals("Precondition: Line Weight Volume should not be a read only field.", false, line1.TL_WeightVolumeInfo.ReadOnly);
			AssertEquals("Precondition: Line Weight Volume should be empty.", ZString.Empty, line2.TL_WeightVolume);
			AssertEquals("Precondition: Line Weight Volume should be a Read Only field.", true, line2.TL_WeightVolumeInfo.ReadOnly);
			AssertEquals("Precondition: Line Weight Volume should be empty.", ZString.Empty, line3.TL_WeightVolume);
			AssertEquals("Precondition: Line Weight Volume should be a Read Only field.", true, line3.TL_WeightVolumeInfo.ReadOnly);

			rateEntry.Unit = QuantityUnit.CN;
			AssertEquals("Line Weight Volume should match unit value.", QuantityUnit.CN, line1.TL_WeightVolume);
			AssertEquals("Lines with Read Only Weight Volume should not have a value for field.", ZString.Empty, line2.TL_WeightVolume);
			AssertEquals("Lines with Read Only Weight Volume should not have a value for field.", ZString.Empty, line3.TL_WeightVolume);

			rateEntry.Unit = Constants.PkgUnit.Package;
			AssertEquals("Line Weight Volume should match unit value.", Constants.PkgUnit.Package, line1.TL_WeightVolume);
			AssertEquals("Lines with Read Only Weight Volume should not have a value for field.", ZString.Empty, line2.TL_WeightVolume);
			AssertEquals("Lines with Read Only Weight Volume should not have a value for field.", ZString.Empty, line3.TL_WeightVolume);

			rateEntry.Unit = QuantityUnit.M3;
			AssertEquals("Line Weight Volume should match unit value.", QuantityUnit.M3, line1.TL_WeightVolume);
			AssertEquals("Lines with Read Only Weight Volume should not have a value for field.", ZString.Empty, line2.TL_WeightVolume);
			AssertEquals("Lines with Read Only Weight Volume should not have a value for field.", ZString.Empty, line3.TL_WeightVolume);
		}

		#endregion

		#region Cartage Addresses

		public void TestCorrectCartageAddressesLoadedForOrigin()
		{
			var testRate = Factory.NewWithValidTestData<ClientRate>();
			testRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var testEntry = testRate.AddRateEntry("ORG");

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "Zubin Supplier Inc";

			var pAD1 = supplier.Addresses.AddNew();
			pAD1.OA_Address1 = "PAD1";
			pAD1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);

			var pickup1 = supplier.Addresses.AddNew();
			pickup1.OA_Address1 = "Pickup1";
			pickup1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			var pickup2 = supplier.Addresses.AddNew();
			pickup2.OA_Address1 = "Pickup2";
			pickup2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			var delivery1 = supplier.Addresses.AddNew();
			delivery1.OA_Address1 = "Delivery1";
			delivery1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			var office = supplier.Addresses.AddNew();
			office.OA_Address1 = "Office";
			office.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);

			var aRM = supplier.Addresses.AddNew();
			aRM.OA_Address1 = "ARM";
			aRM.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);

			testEntry.TI_OH_Consignor = supplier.PK;

			Factory.Save();

			AssertEquals("Only PICKUP, PAD and ALL visible on Origin Entries", true, testEntry.Lookups.CartagePickupAddressOverrides.Contains(pickup1));
			AssertEquals("Only PICKUP, PAD and ALL visible on Origin Entries", true, testEntry.Lookups.CartagePickupAddressOverrides.Contains(pickup2));
			AssertEquals("Only PICKUP, PAD and ALL visible on Origin Entries", false, testEntry.Lookups.CartagePickupAddressOverrides.Contains(delivery1));
			AssertEquals("Only PICKUP, PAD and ALL visible on Origin Entries", true, testEntry.Lookups.CartagePickupAddressOverrides.Contains(office));
			AssertEquals("Only PICKUP, PAD and ALL visible on Origin Entries", false, testEntry.Lookups.CartagePickupAddressOverrides.Contains(aRM));
			AssertEquals("Only PICKUP, PAD and ALL visible on Origin Entries", true, testEntry.Lookups.CartagePickupAddressOverrides.Contains(pAD1));
		}

		public void TestALLCartageAddressLoadedForOrigin()
		{
			var testRate = Factory.NewWithValidTestData<ClientRate>();
			testRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var testEntry = testRate.AddRateEntry("ORG");

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "Zubin Supplier Inc";

			var all = supplier.Addresses[0];
			testEntry.TI_OH_Consignor = supplier.PK;
			Factory.Save();
			AssertEquals("Only ALL visible on Origin Entries", true, testEntry.Lookups.CartagePickupAddressOverrides.Contains(all));
		}

		public void TestCorrectCartageAddressesLoadedForDestination()
		{
			var testRate = Factory.New<ClientRate>();
			testRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var testEntry = testRate.AddRateEntry("DST");

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "Zubin Supplier Inc";

			var pAD1 = supplier.Addresses.AddNew();
			pAD1.OA_Address1 = "PAD1";
			pAD1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);

			var pickup1 = supplier.Addresses.AddNew();
			pickup1.OA_Address1 = "Piclup1";
			pickup1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			var pickup2 = supplier.Addresses.AddNew();
			pickup2.OA_Address1 = "Pickup2";
			pickup2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);

			var delivery1 = supplier.Addresses.AddNew();
			delivery1.OA_Address1 = "Delivery1";
			delivery1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);

			var office = supplier.Addresses.AddNew();
			office.OA_Address1 = "Office";
			office.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Office);

			var aRM = supplier.Addresses.AddNew();
			aRM.OA_Address1 = "ARM";
			aRM.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);

			testEntry.TI_OH_Consignee = supplier.PK;
			Factory.Save();
			AssertEquals("Only DELIVERY, PAD and OFC visible on Destination Entries", false, testEntry.Lookups.CartageDeliveryAddressOverrides.Contains(pickup1));
			AssertEquals("Only DELIVERY, PAD and OFC visible on Destination Entries", false, testEntry.Lookups.CartageDeliveryAddressOverrides.Contains(pickup2));
			AssertEquals("Only DELIVERY, PAD and OFC visible on Destination Entries", true, testEntry.Lookups.CartageDeliveryAddressOverrides.Contains(delivery1));
			AssertEquals("Only DELIVERY, PAD and OFC visible on Destination Entries", true, testEntry.Lookups.CartageDeliveryAddressOverrides.Contains(office));
			AssertEquals("Only DELIVERY, PAD and OFC visible on Destination Entries", false, testEntry.Lookups.CartageDeliveryAddressOverrides.Contains(aRM));
			AssertEquals("Only DELIVERY, PAD and OFC visible on Destination Entries", true, testEntry.Lookups.CartageDeliveryAddressOverrides.Contains(pAD1));
		}

		public void TestALLCartageAddressLoadedForDestination()
		{
			var testRate = Factory.New<ClientRate>();
			testRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var testEntry = testRate.AddRateEntry("DST");

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "Zubin Supplier Inc";

			var all = supplier.Addresses[0];

			testEntry.TI_OH_Consignee = supplier.PK;
			Factory.Save();
			AssertEquals("Only ALL visible on Destination Entries", true, testEntry.Lookups.CartageDeliveryAddressOverrides.Contains(all));
		}

		public void TestCartagePickupAddressOverride()
		{
			var testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = testSupplier.Addresses.AddNew();
			address1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			address1.OA_Address1 = "Address1";
			address1.OA_Code = "Test Address 1";
			address1.OA_PostCode = "2125";

			var address2 = testSupplier.Addresses.AddNew();
			address2.OA_Address1 = "Address2";
			address2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			address2.OA_Code = "Test Address 2";

			var testEntry = Helper.NewCosting(null).AddRateEntry(RatingConstants.RateCategory.AIR);
			testEntry.TI_OH_Consignor = testSupplier.PK;
			Factory.Save();
			testEntry.TI_OA_CartagePickupAddressOverride = address1.PK;

			AssertEquals("Cartage Address PK set", address1.PK, testEntry.TI_OA_CartagePickupAddressOverride);
			AssertEquals("Postcode set", address1.OA_PostCode, testEntry.TI_CartagePickupAddressPostCode);
			Assert("Postcode readonly", testEntry.TI_CartagePickupAddressPostCodeInfo.ReadOnly);

			testEntry.TI_OA_CartagePickupAddressOverride = ZGuid.Empty;
			AssertEquals("Cartage Address PK cleared", ZGuid.Empty, testEntry.TI_OA_CartagePickupAddressOverride);
			AssertEquals("Postcode cleared", ZString.Empty, testEntry.TI_CartagePickupAddressPostCode);
			Assert("Postcode not readonly", !testEntry.TI_CartagePickupAddressPostCodeInfo.ReadOnly);

			testEntry.TI_OA_CartagePickupAddressOverride = address2.PK;
			AssertEquals("Cartage Address PK set", address2.PK, testEntry.TI_OA_CartagePickupAddressOverride);
			AssertEquals("Postcode set", ZString.Empty, testEntry.TI_CartagePickupAddressPostCode);
			Assert("Postcode readonly", testEntry.TI_CartagePickupAddressPostCodeInfo.ReadOnly);
		}

		public void TestCartageDeliveryAddressOverride()
		{
			var testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = testSupplier.Addresses.AddNew();
			address1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			address1.OA_Address1 = "Address1";
			address1.OA_Code = "Test Address 1";
			address1.OA_PostCode = "2125";

			var address2 = testSupplier.Addresses.AddNew();
			address2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
			address2.OA_Address1 = "Address2";
			address2.OA_Code = "Test Address 2";

			var testEntry = Helper.NewCosting(null).AddRateEntry(RatingConstants.RateCategory.AIR);
			testEntry.TI_OH_Consignee = testSupplier.PK;

			Factory.Save();
			testEntry.TI_OA_CartageDeliveryAddressOverride = address1.PK;
			AssertEquals("Cartage Address PK set", address1.PK, testEntry.TI_OA_CartageDeliveryAddressOverride);
			AssertEquals("Postcode set", address1.OA_PostCode, testEntry.TI_CartageDeliveryAddressPostCode);
			Assert("Postcode readonly", testEntry.TI_CartageDeliveryAddressPostCodeInfo.ReadOnly);

			testEntry.TI_OA_CartageDeliveryAddressOverride = ZGuid.Empty;
			AssertEquals("Cartage Address PK cleared", ZGuid.Empty, testEntry.TI_OA_CartageDeliveryAddressOverride);
			AssertEquals("Postcode cleared", ZString.Empty, testEntry.TI_CartageDeliveryAddressPostCode);
			Assert("Postcode not readonly", !testEntry.TI_CartageDeliveryAddressPostCodeInfo.ReadOnly);

			testEntry.TI_OA_CartageDeliveryAddressOverride = address2.PK;
			AssertEquals("Cartage Address PK set", address2.PK, testEntry.TI_OA_CartageDeliveryAddressOverride);
			AssertEquals("Postcode set", ZString.Empty, testEntry.TI_CartageDeliveryAddressPostCode);
			Assert("Postcode readonly", testEntry.TI_CartageDeliveryAddressPostCodeInfo.ReadOnly);

			testEntry.TI_OH_Consignee = testSupplier.PK;
			testEntry.TI_OA_CartageDeliveryAddressOverride = address1.PK;
			AssertEquals("Postcode set", address1.OA_PostCode, testEntry.TI_CartageDeliveryAddressPostCode);
			testEntry.TI_OH_Consignee = ZGuid.Empty;
			AssertEquals("Postcode cleared", ZString.Empty, testEntry.TI_CartageDeliveryAddressPostCode);
		}

		#endregion

		#region Copy / Clone

		public void TestClone()
		{
			var testRate = Factory.NewWithValidTestData<ClientRate>();
			var testEntry = testRate.AddRateEntry("DST");

			var line = testEntry.RateLines.AddNew();
			line.TL_RateCalculator = "ABC";

			var clonedEntry = testEntry.Clone(testRate.GetRateEntryCollectionForCategory("DST"));

			Assert("Different objects", testEntry != clonedEntry);
			AssertEquals("Rate Lines NOT copied", 0, clonedEntry.RateLines.Count);
		}

		public void TestDataCheckedPropertyIsNotCloned()
		{
			var testRate = Factory.New<ClientRate>();
			var testEntry = testRate.AddRateEntry("DST");
			testEntry.TI_DataChecked = true;

			var clonedEntry = testEntry.Clone(testRate.GetRateEntryCollectionForCategory("DST"));
			Assert(!clonedEntry.TI_DataChecked);
		}

		public void TestContractNumberPropertyIsClonedWhenRatingHeaderTypesEqual()
		{
			const string contractNumber = "123456";
			var testRate = Factory.New<ClientRate>();
			var testEntry = testRate.AddRateEntry("DST");
			testEntry.TI_ContractNumber = contractNumber;

			var anotherClientRate = Factory.New<ClientRate>();
			var clonedEntry = testEntry.Clone(anotherClientRate.GetRateEntryCollectionForCategory("DST"));
			AssertEquals(contractNumber, clonedEntry.TI_ContractNumber.ToString());
		}

		public void TestContractNumberPropertyIsNotClonedWhenRatingHeaderTypesDiffer()
		{
			const string contractNumber = "123456";
			var testRate = Factory.New<ClientRate>();
			var testEntry = testRate.AddRateEntry("DST");
			testEntry.TI_ContractNumber = contractNumber;

			var testCosting = Factory.New<Costing>();
			var clonedEntry = testEntry.Clone(testCosting.GetRateEntryCollectionForCategory("DST"));
			Assert(clonedEntry.TI_ContractNumber.IsEmpty);
			AssertNotEquals(contractNumber, clonedEntry.TI_ContractNumber.ToString());
		}

		public void TestContractNumberPropertyIsClonedWhenAcceptQuotation()
		{
			const string contractNumber = "123456";
			var testQuote = Helper.NewQuote(NewClient);
			var testEntry = testQuote.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.ALL, "AUBNE", "NZAKL", "STD", "20GP", "GEN");
			testEntry.TI_ContractNumber = contractNumber;

			testQuote.TryAcceptQuote(out var clientRate, false);

			var acceptedEntries = clientRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.FCL);

			AssertSameRateEntryKeys((RateEntry)acceptedEntries.Single(), testEntry, true, "Should copy all properties");
			Assert(true);
		}

		public void TestDuplicateEntriesWhenAcceptQuotation()
		{
			const string contractNumber = "123456";
			var testQuote = Helper.NewQuote(NewClient);
			var testEntry = testQuote.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.ALL, "AUBNE", "NZAKL", "STD", "20GP", "GEN");
			testEntry.TI_ContractNumber = contractNumber;

			var existingClientRate = Helper.NewClientRate(NewClient);
			var existingEntry1 = existingClientRate.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.ALL, "AUBNE", "NZAKL", "STD", "20GP", "GEN");
			var existingEntry2 = existingClientRate.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.ALL, "AUBNE", "NZAKL", "STD", "20GP", "GEN");
			existingEntry2.TI_ContractNumber = contractNumber;

			testQuote.TryAcceptQuote(out var clientRate, false);

			var allEntries = clientRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.FCL);
			var newEntries = allEntries.Except(new[] { existingEntry1, existingEntry2 });

			AssertEquals(
				"Should add new entry to existing client rate",
				3,
				allEntries.Count
			);

			AssertSameRateEntryKeys(
				(RateEntry)newEntries.Single(),
				existingEntry2,
				false,
				"Should be duplicated to existing entry having same values for key columns."
			);
		}

		public void TestAllPropertiesAreClonedWhenRatingHeaderTypesEqual()
		{
			var testClientRate = Helper.NewClientRate(NewClient);
			var testEntry = testClientRate.AddRateEntry(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AUSYD", "USLAX", "STD", "20GP", "GEN");
			testEntry.TI_ContractNumber = "123456";

			var anotherClientRate = Helper.NewClientRate(NewClient2);
			var clonedEntry = testEntry.Clone(anotherClientRate.GetRateEntryCollectionForCategory("DST"));

			AssertSameRateEntryKeys(clonedEntry, testEntry, true);
			AssertEquals("Cloned rate entry's start date should match the original entry's start date.", testEntry.TI_RateStartDate, clonedEntry.TI_RateStartDate);
			AssertEquals("Cloned rate entry's end date should match the original entry's end date.", testEntry.TI_RateEndDate, clonedEntry.TI_RateEndDate);
		}

		void AssertSameRateEntryKeys(RateEntry entry, RateEntry otherEntry, bool excludeRatingHeader = false, string reason = null)
		{
			var key = entry.GetKeyForDuplicateSearch();
			var otherKey = otherEntry.GetKeyForDuplicateSearch();

			if (excludeRatingHeader)
			{
				var valuesExceptRatingHeader = key
					.Split(new[] { "|" }, StringSplitOptions.None)
					.Skip(1);

				var otherValuesExceptRatingHeader = otherKey
					.Split(new[] { "|" }, StringSplitOptions.None)
					.Skip(1);

				AssertContainsExactElementsInAnyOrder(reason ?? "The keys without the rating header do not match.", valuesExceptRatingHeader, otherValuesExceptRatingHeader);
			}
			else
			{
				AssertEquals(reason ?? "The keys do not match.", key, otherKey);
			}
		}

		public void TestIsTactShouldExcludeFromCloning()
		{
			var testRate = Factory.New<ClientRate>();
			var testEntry = testRate.AddRateEntry("DST");
			testEntry.TI_IsTact = true;

			Assert(testEntry.TI_IsTact);

			var clonedEntry = testEntry.Clone(testRate.GetRateEntryCollectionForCategory("DST"));
			Assert(!clonedEntry.TI_IsTact);
		}

		public void TestDeepClone()
		{
			var testRate = Factory.NewWithValidTestData<ClientRate>();
			var testEntry = testRate.AddRateEntry("ORG");
			var line = testEntry.RateLines.AddNew();
			line.TL_RateCalculator = CombinedCalculator.Code;
			line.Calculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0m, 200m, 0m);

			AssertEquals(1, testRate.EntryCollections["ORG"].LazyLoadingCollection.Count);
			AssertEquals(1, testEntry.RateLines.Count);
			AssertEquals(5, testEntry.RateLines[0].RateLineItems.Count);

			var clonedEntry = testEntry.DeepClone(testRate.EntryCollections["ORG"]);

			AssertType<RateEntry>(clonedEntry);
			AssertEquals(2, testRate.EntryCollections["ORG"].LazyLoadingCollection.Count);
			Assert(testEntry != clonedEntry);
			AssertEquals(1, clonedEntry.RateLines.Count);
			AssertEquals(5, clonedEntry.RateLines[0].RateLineItems.Count);

			var minRateLineItem = clonedEntry.RateLines[0].RateLineItems.Cast<RateLineItem>().First(x => x.TM_Type == "MIN");
			AssertEquals((ZDecimal)200m, minRateLineItem.TM_Value);
			AssertNull(clonedEntry.RateLines[0].RateLineItems.Cast<RateLineItem>().FirstOrDefault(x => x.TM_Type == "TAC"));
		}

		public void TestDeepClone_NotOverrideChargeDescription()
		{
			var testRate = Factory.NewWithValidTestData<ClientRate>();
			var testEntry = testRate.AddRateEntry("ORG");
			var line = testEntry.AddRateLine("FRT", FlatCalculator.Code);

			AssertEquals("Precondition: OverrideChargeDescription", false, line.OverrideChargeDescription);

			var clonedEntry = testEntry.DeepClone(testRate.EntryCollections["ORG"]);
			var clonedRateLine = clonedEntry.RateLines[0];

			AssertEquals("WHEN duplicate THEN OverrideChargeDescription should be copied", false, clonedRateLine.OverrideChargeDescription);
		}

		public void TestDeepClone_OverrideChargeDescription()
		{
			var testRate = Factory.NewWithValidTestData<ClientRate>();
			var testEntry = testRate.AddRateEntry("ORG");
			var line = testEntry.RateLines.AddNew();
			line.TL_RateCalculator = FlatCalculator.Code;
			line.OverrideChargeDescription = true;
			line.TL_RateDesc = "Rate Description Override";
			line.TL_RateDescLocal = "Rate Description Local Override";

			var clonedEntry = testEntry.DeepClone(testRate.EntryCollections["ORG"]);
			var clonedRateLine = clonedEntry.RateLines[0];

			CombineAssertions
			(
				"WHEN duplicate THEN OverrideChargeDescription, RateDesc and RateDescLocal should be copied",
				() =>
				{
					AssertEquals("OverrideChargeDescription", true, clonedRateLine.OverrideChargeDescription);
					AssertEquals("RateDesc", "Rate Description Override", clonedRateLine.TL_RateDesc);
					AssertEquals("RateDescLocal", "Rate Description Local Override", clonedRateLine.TL_RateDescLocal);
				}
			);
		}

		public void TestDeepClone_OverrideChargeDescription_NotChangingRateDescLocal()
		{
			var testRate = Factory.NewWithValidTestData<ClientRate>();
			var testEntry = testRate.AddRateEntry("ORG");
			var line = testEntry.AddRateLine("FRT", FlatCalculator.Code);
			line.OverrideChargeDescription = true;

			var clonedEntry = testEntry.DeepClone(testRate.EntryCollections["ORG"]);
			var clonedRateLine = clonedEntry.RateLines[0];
			Assert("WHEN duplicate THEN OverrideChargeDescription should be copied", clonedRateLine.OverrideChargeDescription);
		}

		public void TestCloningWhileNotAllowed()
		{
			var securitySetup = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("DST");
			var entryCollection = rate.GetRateEntryCollectionForCategory("DST");

			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", FlatCalculator.Code);
			line.TL_RateCalculator = "ABC";

			var clonedEntry = entry.DeepClone(entryCollection);
			AssertNotNull(clonedEntry);

			entry.TI_OH_Consignee = securitySetup.DeniedOrg.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(securitySetup.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				// Reloading entry in new Factory because of security settings' caching.
				var entryReloaded = Helper.LoadInNewFactory(entry);

				clonedEntry = entryReloaded.Clone(entryCollection);
				AssertNull(clonedEntry);

				clonedEntry = entryReloaded.DeepClone(entryCollection);
				AssertNull(clonedEntry);
			}
		}

		public void TestCloneFromQuote()
		{
			var header = Factory.NewWithValidTestData<Costing>();
			var rateEntry = header.AddRateEntry(RatingConstants.RateCategory.DST);
			rateEntry.AddRateLine("FRT", FlatCalculator.Code);

			var clonedEntry = rateEntry.Clone(header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.DST));

			Assert("Different objects", rateEntry != clonedEntry);
			AssertNotEquals("Different objects", rateEntry.PK, clonedEntry.PK);
			AssertEquals("No Rate Lines copied", 0, clonedEntry.RateLines.Count);
		}

		public void TestLocalRateEntryDeepCloneToGlobalRateEntry()
		{
			AccChargeCode localChargeCode, localChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out localChargeCode, out localChargeCodeLinked, out globalChargeCode);

			var client = Helper.NewOrgHeader();
			var localClientRate = Helper.NewClientRate(client);
			var localRateEntry = localClientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			localRateEntry.RateLines.RemoveAndDeleteAll();
			localRateEntry.AddRateLine(localChargeCodeLinked, FlatCalculator.Code);
			localRateEntry.AddRateLine(localChargeCode, FlatCalculator.Code);

			var globalClientRate = Helper.NewGlobalClientRate(client);
			var globalRateEntry = localRateEntry.DeepClone(globalClientRate.LCLRateEntriesForBinding);

			globalRateEntry.RunPreSaveValidation();

			CombineAssertions(() =>
			{
				AssertEquals("Cloned rate entry should be added to the correct collection", 1, globalClientRate.LCLRateEntriesForBinding.Count);
				AssertEquals(RatingConstants.RateCategory.LCL, globalRateEntry.TI_RateCategory);
				AssertEquals(Core.Constants.RateMode.LCL, globalRateEntry.TI_Mode);
				AssertEquals("AU", globalRateEntry.TI_OriginLRC);
				AssertEquals("", globalRateEntry.TI_DestinationLRC);
				AssertEquals(2, globalRateEntry.RateLines.Count);

				AssertEquals(1, globalRateEntry.RateLines.Cast<RateLine>().Count(x => x.TL_AC == localChargeCode.PK));
				AssertEquals("localChargeCodeLinked should be replaced with globalChargeCode when can be linked", 1, globalRateEntry.RateLines.Cast<RateLine>().Count(x => x.TL_AC == globalChargeCode.PK));
			});
		}

		public void TestGlobalRateEntryDeepCloneToLocalRateEntry()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBCHRG");
			globalChargeCode.Factory.Save();

			var unmappedGlobalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			unmappedGlobalChargeCode.AC_GC = ZGuid.Empty;
			unmappedGlobalChargeCode.AC_RateCalculator = FlatCalculator.Code;

			var client = Helper.NewOrgHeader();
			var globalClientRate = Helper.NewGlobalClientRate(client);
			var globalRateEntry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AU", "");
			globalRateEntry.RateLines.RemoveAndDeleteAll();
			globalRateEntry.AddRateLine(globalChargeCode, FlatCalculator.Code);
			globalRateEntry.AddRateLine(unmappedGlobalChargeCode, FlatCalculator.Code);

			var localClientRate = Helper.NewClientRate(client);
			var localRateEntry = globalRateEntry.DeepClone(localClientRate.LCLRateEntriesForBinding);
			localRateEntry.RunPreSaveValidation();

			CombineAssertions(() =>
			{
				AssertEquals("Cloned rate entry should be added to the correct RatingHeader", localClientRate.PK, localRateEntry.TI_TH);
				AssertEquals(RatingConstants.RateCategory.LCL, localRateEntry.TI_RateCategory);
				AssertEquals(Core.Constants.RateMode.LCL, localRateEntry.TI_Mode);
				AssertEquals("AU", localRateEntry.TI_OriginLRC);
				AssertEquals("", localRateEntry.TI_DestinationLRC);
				AssertEquals(2, localRateEntry.RateLines.Count);

				AssertEquals(1, globalRateEntry.RateLines.Cast<RateLine>().Count(x => x.TL_AC == unmappedGlobalChargeCode.PK));

				var message = "globalChargeCode should be replaced with local charge code for current company";
				var expectedLocallyLinkedChargeCode = globalChargeCode.ChildChargeCodes.Single(x => x.AC_GC == Env.CurrentCompanyPK);
				AssertEquals(message, 1, localRateEntry.RateLines.Cast<RateLine>().Count(x => x.TL_AC == expectedLocallyLinkedChargeCode.PK));
			});
		}

		public void TestCorrectChargeCodeIsUsedWhenDuplicatingGlobalRateWithinALocalRate()
		{
			AccChargeCode localChargeCode, localChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out localChargeCode, out localChargeCodeLinked, out globalChargeCode);
			Factory.Save();

			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			var clientEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "CN");
			clientEntry.RateLines.RemoveAndDeleteAll();
			clientEntry.AddRateLine(localChargeCodeLinked, FlatCalculator.Code);

			clientEntry.IsPublished = true;

			AssertNotNull("Publishing a Rate should create a Global Rating Header", clientRate.GlobalRatingHeader);
			AssertEquals("Should move the Published Rate Entry to the newly Created Global Rating Header", clientEntry.TI_TH, clientRate.GlobalRatingHeader.PK);
			AssertEquals("Global Charge code should be used when line is published", 1, clientEntry.RateLines.Cast<RateLine>().Count(x => x.TL_AC == globalChargeCode.PK));

			var globalPublishedEntry = clientRate.GlobalRatingHeader.AIRRateEntriesForBinding.Cast<RateEntry>().FirstOrDefault();

			var copiedRateEntry = globalPublishedEntry.DeepClone(clientRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR));
			AssertEquals("Local Charge code should be used when published line is cloned to Local Rate", 1, copiedRateEntry.RateLines.Cast<RateLine>().Count(x => x.TL_AC == localChargeCodeLinked.PK));
		}

		#endregion

		#region Set Default Values

		public void TestDefaultsSet()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			AssertEquals("Start Date", ZDateTime.Today, entry.TI_RateStartDate);
			AssertEquals("End Date", entry.DefaultRateEndDate, entry.TI_RateEndDate);
		}

		#endregion

		#region Local/Overseas Port

		public void TestLocalOverseasPort()
		{
			GlbCompany.CurrentCompany.SetCountry("US");

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			rateEntry.TI_OriginLRC = "USLAX";
			rateEntry.TI_DestinationLRC = "GBLON";
			AssertEquals("Local Port", "USLAX", rateEntry.LocalPort.Code);
			AssertEquals("Overseas Port", "GBLON", rateEntry.OverseasPort.Code);

			rateEntry.TI_OriginLRC = "GBLON";
			rateEntry.TI_DestinationLRC = "USLAX";
			AssertEquals("Local Port", "USLAX", rateEntry.LocalPort.Code);
			AssertEquals("Overseas Port", "GBLON", rateEntry.OverseasPort.Code);

			rateEntry.TI_OriginLRC = "US";
			rateEntry.TI_DestinationLRC = "";
			AssertEquals("Local Port", "US", rateEntry.LocalPort.Code);
			AssertNull("Overseas Port", rateEntry.OverseasPort);

			rateEntry.TI_OriginLRC = "";
			rateEntry.TI_DestinationLRC = "USCA";
			AssertEquals("Local Port", "USCA", rateEntry.LocalPort.Code);
			AssertNull("Overseas Port", rateEntry.OverseasPort);

			rateEntry.TI_OriginLRC = "GB";
			rateEntry.TI_DestinationLRC = "";
			AssertNull("Local Port", rateEntry.LocalPort);
			AssertEquals("Overseas Port", "GB", rateEntry.OverseasPort.Code);

			rateEntry.TI_OriginLRC = "";
			rateEntry.TI_DestinationLRC = "AU";
			AssertNull("Local Port", rateEntry.LocalPort);
			AssertEquals("Overseas Port", "AU", rateEntry.OverseasPort.Code);

			rateEntry.TI_OriginLRC = "AUSYD";
			rateEntry.TI_DestinationLRC = "GBLON";
			AssertNull("Local Port", rateEntry.LocalPort);
			AssertNull("Overseas Port", rateEntry.OverseasPort);

			rateEntry.TI_OriginLRC = "USLAX";
			rateEntry.TI_DestinationLRC = "USSFO";
			AssertNull("Local Port", rateEntry.LocalPort);
			AssertNull("Overseas Port", rateEntry.OverseasPort);
		}

		#endregion

		#region Warehouse

		public void TestWarehouseProperties_ProductWarehouse()
		{
			AssertWarehouseProperties(RatingConstants.RateCategory.WHS);
		}

		public void TestWarehouseProperties_TransitWarehouse()
		{
			AssertWarehouseProperties(RatingConstants.RateCategory.TRW);
		}

		public void TestWarehouseProperties_TransitWarehouseTransportationUnit()
		{
			AssertWarehouseProperties(RatingConstants.RateCategory.TWU);
		}

		void AssertWarehouseProperties(string rateCategory)
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry(rateCategory, "ALL", "", "");

			Assert(!entry.AllWarehouses);
			Assert(!entry.TI_WW_WarehouseInfo.ReadOnly);

			entry.RunPreSaveValidation();
			Assert(entry.TI_WW_WarehouseInfo.HasErrors());
			AssertEquals("Please enter a " + entry.TI_WW_WarehouseInfo.Description + ".", entry.TI_WW_WarehouseInfo.GetErrors().GetFirstMessage());

			entry.TI_WW_Warehouse = ZGuid.NewZGuid();

			entry.AllWarehouses = true;
			Assert(entry.TI_WW_WarehouseInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, entry.TI_WW_Warehouse);

			entry.RunPreSaveValidation();
			Assert(!entry.TI_WW_WarehouseInfo.HasErrors());

			Factory.Save();

			new BusinessObjectFactory().Load<RateEntry>(entry.PK);
			Assert(entry.AllWarehouses);
			Assert(entry.TI_WW_WarehouseInfo.ReadOnly);
		}

		public void TestWarehouse_ProductWarehouse()
		{
			AssertWarehouse(RatingConstants.RateCategory.WHS);
		}

		public void TestWarehouse_TransitWarehouse()
		{
			AssertWarehouse(RatingConstants.RateCategory.TRW);
		}

		public void TestWarehouse_TransitWarehouseTransportationUnit()
		{
			AssertWarehouse(RatingConstants.RateCategory.TWU);
		}

		void AssertWarehouse(string rateCategory)
		{
			var warehouse1 = Helper.NewWarehouse();
			var warehouse2 = Helper.NewWarehouse();
			warehouse1[WhsWarehouseSchema.WW_WarehouseType] = rateCategory == "WHS" ? "PRW" : "TRW";
			warehouse2[WhsWarehouseSchema.WW_WarehouseType] = rateCategory == "WHS" ? "PRW" : "TRW";

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry(rateCategory, "ALL", "", "");

			AssertNull(entry.Warehouse());
			entry.TI_WW_Warehouse = warehouse1.PK;
			AssertEquals(warehouse1, entry.Warehouse());
			entry.TI_WW_Warehouse = ZGuid.NewZGuid();
			AssertNull(entry.Warehouse());
			entry.TI_WW_Warehouse = warehouse2.PK;
			AssertEquals(warehouse2, entry.Warehouse());
			entry.TI_WW_Warehouse = ZGuid.Empty;
			AssertNull(entry.Warehouse());
		}

		#endregion

		#region Container Yard

		public void TestContainerYardProperties()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.CYD, "ALL", "", "");

			entry.RunPreSaveValidation();
			Assert(!entry.TI_CYC_WW_FacilityInfo.ReadOnly);
			Assert(!entry.TI_CYC_WW_FacilityInfo.HasErrors());
		}

		public void TestContainerYardObjProperty()
		{
			var yard1 = Helper.NewWarehouse();
			var yard2 = Helper.NewWarehouse();

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.CYD, "ALL", "", "");

			AssertNull(entry.Yard());
			entry.TI_CYC_WW_Facility = yard1.PK;
			AssertEquals(yard1, entry.Yard());
			entry.TI_CYC_WW_Facility = ZGuid.NewZGuid();
			AssertNull(entry.Yard());
			entry.TI_CYC_WW_Facility = yard2.PK;
			AssertEquals(yard2, entry.Yard());
			entry.TI_CYC_WW_Facility = ZGuid.Empty;
			AssertNull(entry.Yard());
		}

		public void TestContainerYard_TypeSizeShouldBeReadOnly_WhenUnitTypeIsEmpty()
		{
			var yard = Helper.NewWarehouse();
			var container = Helper.Containers["20GP"];

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var yardEntry = rate.AddRateEntry(RatingConstants.RateCategory.CYD, "ALL", "", "");

			AssertEquals(ZString.Empty, yardEntry.TI_YardUnitType);
			AssertEquals(ZGuid.Empty, yardEntry.TI_RC);

			yardEntry.TI_YardUnitType = "CNT";
			yardEntry.TI_RC = container.PK;

			AssertEquals(yardEntry.TI_RC, container.PK);
			Assert(!yardEntry.TI_RCInfo.ReadOnly);

			yardEntry.TI_YardUnitType = ZString.Empty;

			AssertEquals(ZGuid.Empty, yardEntry.TI_RC);
			Assert(yardEntry.TI_RCInfo.ReadOnly);

			var nonYardEntry = rate.AddRateEntry(RatingConstants.RateCategory.TWU, "ALL", "", "");

			nonYardEntry.TI_YardUnitType = "CNT";
			nonYardEntry.TI_RC = container.PK;

			AssertEquals(nonYardEntry.TI_RC, container.PK);

			nonYardEntry.TI_YardUnitType = ZString.Empty;

			AssertEquals(ZGuid.Empty, yardEntry.TI_RC);
			Assert(!nonYardEntry.TI_RCInfo.ReadOnly);
		}

		#endregion

		#region Event Logging

		public void TestRateEntryEventLogging_GivenDeletedRatingHeader_ThenShouldNotThrowNullReferenceException()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntryWithFlatRateLine(Category.FCL, Mode.SEA, "AUSYD", "USLAX", "BAF", 100m);

			costing.Delete();

			AssertNoExceptionThrown("GIVEN deleted RatingHeader, WHEN log rateEntry THEN should not throw NullReferenceException", () =>
			{
				rateEntry.AddLog();
			});
		}

		public void TestRateEntryIsNotLoggedDuringTACTImport()
		{
			var rate = Factory.New<ClientRate>();
			rate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry = rate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)50m;
			Factory.Save();

			var rateLogs = new StmALogCollectionView(rate);

			foreach (StmALog log in rateLogs)
			{
				AssertNotEquals(RateEntrySchema.Constants.TableName, log.SL_Table);
			}
		}

		public void TestRateEntryIsUpdatedIfItsChildWasChanged()
		{
			var rate = Factory.New<ClientRate>();
			rate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry = rate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)50m;

			Factory.Save();

			Assert(!entry.HasChanges);
			var lastEditTime = entry.TI_SystemLastEditTimeUtc;

			entry.RateLines[0].Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)50m;
			Assert(entry.HasChanges);

			Factory.Save();

			Assert(!entry.HasChanges);
			Assert(entry.TI_SystemLastEditTimeUtc > lastEditTime);
		}

		[TestDateIncremental(0, 0, 2)]
		public void TestRateEntryEventLogging()
		{
			var rate = Factory.New<ClientRate>();
			rate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var entry = rate.AddRateEntry("LCL", "LCL", "AUSYD", "USLAX");
			entry.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)50m;

			Factory.Save();

			var logs = rate.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).ToList();
			var log = logs[0];
			CombineAssertions(() =>
			{
				AssertEquals("SL_Table", RatingHeaderSchema.Constants.TableName, log.SL_Table);
				AssertEquals("SL_SE_NKEvent", Events.AddedARecordToTheSystem.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "1/0/0 entries added/edited/deleted", log.SL_Reference);
				AssertEquals("1 new log", 1, logs.Count);
			});

			entry.TI_DestinationLRC = "GBLON";
			entry.TI_Mode = Core.Constants.RateMode.LRO;

			Factory.Save();

			logs = rate.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).ToList();
			log = logs[0];
			CombineAssertions(() =>
			{
				AssertEquals("SL_Table", RatingHeaderSchema.Constants.TableName, log.SL_Table);
				AssertEquals("SL_SE_NKEvent", Events.EditedARecord.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "0/1/0 entries added/edited/deleted", log.SL_Reference);
				AssertEquals("1 new log", 2, logs.Count);
			});

			rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL).RemoveAndDelete(entry);

			Factory.Save();

			logs = rate.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).ToList();
			log = logs[0];
			CombineAssertions(() =>
			{
				AssertEquals("SL_Table", RatingHeaderSchema.Constants.TableName, log.SL_Table);
				AssertEquals("SL_SE_NKEvent", Events.EditedARecord.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "0/0/1 entries added/edited/deleted", log.SL_Reference);
				AssertEquals("1 new log", 3, logs.Count);
			});

			var entry2 = rate.AddRateEntry("LCL", "LCL", "AUBNE", "USLAX");
			var entry3 = rate.AddRateEntry("LCL", "LCL", "AUMEL", "USLAX");
			var entry4 = rate.AddRateEntry("LCL", "LCL", "AUPER", "USLAX");
			Factory.Save();
			logs = rate.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).ToList();
			log = logs[0];
			CombineAssertions(() =>
			{
				AssertEquals("SL_Table", RatingHeaderSchema.Constants.TableName, log.SL_Table);
				AssertEquals("SL_SE_NKEvent", Events.EditedARecord.Code, log.SL_SE_NKEvent);
				AssertEquals("SL_Reference", "3/0/0 entries added/edited/deleted", log.SL_Reference);
				AssertEquals("1 new log", 4, logs.Count);
			});

			entry2.Delete();
			entry3.TI_DestinationLRC = "GBLON";
			entry4.TI_DestinationLRC = "GBLON";
			var entry5 = rate.AddRateEntry("LCL", "LCL", "AUADL", "USLAX");
			Factory.Save();
			logs = rate.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(x => x.SL_PostedTimeUtc).ToList();
			log = logs[0];
			CombineAssertions(() =>
			{
				AssertEquals("SL_Reference", "1/2/1 entries added/edited/deleted", log.SL_Reference);
				AssertEquals("1 new log", 5, logs.Count);
			});
		}

		#endregion

		#region Utility Properties

		public void TestUtilityProperties()
		{
			var testSet = new object[,]
			{
				{ "IsFreightEntry", new string[] { "AIR", "BCN", "AIR", "LSE", "AIR", "ULD", "FCL", "SEA", "FCL", "ROA", "FCL", "RAI",  "LCL", "LCL", "LCL", "LRO", "LCL", "LRA", "LCL", "FTL", "LCL", "FWL" } },
				{ "IsAirFreight", new string[] { "AIR", "BCN", "AIR", "LSE", "AIR", "ULD" } },
				{ "IsLCLFreight", new string[] { "LCL", "LCL", "LCL", "LRO", "LCL", "LRA", "LCL", "FTL", "LCL", "FWL" } },
				{ "IsULDFreight", new string[] { "AIR", "ULD" } },
				{ "IsLooseFreight", new string[] { "AIR", "LSE" } },
				{ "IsSeaFreight", new string[] { "FCL", "SEA", "LCL", "LCL" } },
				{ "IsRoadFreight", new string[] { "FCL", "ROA", "LCL", "LRO", "LCL", "FTL" } },
				{ "IsRailFreight", new string[] { "FCL", "RAI", "LCL", "LRA", "LCL", "FWL" } },
				{ "IsSupplementaryEntry", new string[] { "ORG", "BCN", "ORG", "LSE", "ORG", "ULD", "ORG", "AIR", "ORG", "FCL", "ORG", "FRO", "ORG", "FRA", "ORG", "LCL", "ORG", "LRO", "ORG", "LRA", "ORG", "FTL", "ORG", "FWL", "ORG", "SEA", "ORG", "ROA", "ORG", "RAI", "ORG", "MAI", "ORG", "ALL", "DST", "BCN", "DST", "LSE", "DST", "ULD", "DST", "AIR", "DST", "FCL", "DST", "FRO", "DST", "FRA", "DST", "LCL", "DST", "LRO", "DST", "LRA", "DST", "FTL", "DST", "FWL", "DST", "SEA", "DST", "ROA", "DST", "RAI", "DST", "MAI", "DST", "ALL", "CST", "AIR", "CST", "SEA", "CST", "ROA", "CST", "RAI", "CST", "ALL" } },
				{ "IsOriginEntry", new string[] { "ORG", "BCN", "ORG", "LSE", "ORG", "ULD", "ORG", "AIR", "ORG", "FCL", "ORG", "FRO", "ORG", "FRA", "ORG", "LCL", "ORG", "LRO", "ORG", "LRA", "ORG", "FTL", "ORG", "FWL", "ORG", "SEA", "ORG", "ROA", "ORG", "RAI", "ORG", "MAI", "ORG", "ALL" } },
				{ "IsDestinationEntry", new string[] { "DST", "BCN", "DST", "LSE", "DST", "ULD", "DST", "AIR", "DST", "FCL", "DST", "FRO", "DST", "FRA", "DST", "LCL", "DST", "LRO", "DST", "LRA", "DST", "FTL", "DST", "FWL", "DST", "SEA", "DST", "ROA", "DST", "RAI", "DST", "MAI", "DST", "ALL" } },
				{ "IsAir", new string[] { "AIR", "LSE", "AIR", "ULD", "ORG", "LSE", "ORG", "ULD", "ORG", "AIR", "DST", "LSE", "DST", "ULD", "DST", "AIR", "CST", "AIR" } },
				{ "IsSea", new string[] { "FCL", "SEA", "LCL", "LCL", "ORG", "FCL", "ORG", "LCL", "ORG", "SEA", "DST", "FCL", "DST", "LCL", "DST", "SEA", "CST", "SEA" } },
				{ "IsRoad", new string[] { "FCL", "ROA", "LCL", "LRO", "LCL", "FTL", "ORG", "FRO", "ORG", "LRO", "ORG", "FTL", "ORG", "ROA", "DST", "FRO", "DST", "LRO", "DST", "FTL", "DST", "ROA", "CST", "ROA" } },
				{ "IsRail", new string[] { "FCL", "RAI", "LCL", "LRA", "LCL", "FWL", "ORG", "FRA", "ORG", "LRA", "ORG", "FWL", "ORG", "RAI", "DST", "FRA", "DST", "LRA", "DST", "FWL", "DST", "RAI", "CST", "RAI" } },
				{ "IsMail", new string[] { "ORG", "MAI", "DST", "MAI" } },
				{ "IsFCL", new string[] { "FCL", "SEA", "FCL", "ROA", "FCL", "RAI", "ORG", "FCL", "ORG", "FRO", "ORG", "FRA", "DST", "FCL", "DST", "FRO", "DST", "FRA", "CST", "SEA", "CST", "ROA", "CST", "RAI" } },
				{ "IsLCL", new string[] { "LCL", "LCL", "LCL", "LRO", "LCL", "LRA", "LCL", "FTL", "LCL", "FWL", "ORG", "LCL", "ORG", "LRO", "ORG", "LRA", "ORG", "FTL", "ORG", "FWL", "DST", "LCL", "DST", "LRO", "DST", "LRA", "DST", "FTL", "DST", "FWL" } },
				{ "IsULD", new string[] { "AIR", "ULD", "ORG", "ULD", "DST", "ULD", "CST", "AIR" } },
				{ "IsBCN", new string[] { "ORG", "BCN", "DST", "BCN", "AIR", "BCN" } }
			};

			var uniqueEntryTypeMode = new Dictionary<string, string[]>();
			var propertyEntryTypeMode = new Dictionary<string, bool>();
			for (var i = 0; i < testSet.GetLength(0); i++)
			{
				var entryTypes = (string[])testSet[i, 1];
				for (var j = 0; j < entryTypes.Length; j += 2)
				{
					if (!uniqueEntryTypeMode.ContainsKey(entryTypes[j] + entryTypes[j + 1]))
					{
						uniqueEntryTypeMode.Add(entryTypes[j] + entryTypes[j + 1], new string[] { entryTypes[j], entryTypes[j + 1] });
					}
					propertyEntryTypeMode.Add((string)testSet[i, 0] + entryTypes[j] + entryTypes[j + 1], true);
				}
			}
			AssertEquals(50, uniqueEntryTypeMode.Count);

			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			for (var i = 0; i < testSet.GetLength(0); i++)
			{
				foreach (var kv in uniqueEntryTypeMode)
				{
					entry.TI_RateCategory = kv.Value[0];
					entry.TI_Mode = kv.Value[1];

					var expectedValue = propertyEntryTypeMode.ContainsKey((string)testSet[i, 0] + kv.Key);
					var actualValue = GetValue(entry, (string)testSet[i, 0]);

					AssertEquals($"{testSet[i, 0]} ({kv.Value[0]}, {kv.Value[1]})", expectedValue, actualValue);
				}
			}
		}

		static bool GetValue(RateEntry entry, string propertyName)
		{
			var method = typeof(RateEntryExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == propertyName);

			if (method.Any())
			{
				return (bool)method.Single().Invoke(entry, new object[] { entry });
			}

			return (bool)entry[propertyName];
		}

		#endregion

		#region CompanyTariffLevel

		public void TestGetCompanyTariffLevels()
		{
			var org = Helper.NewOrgHeader(1);
			var entry = Helper.NewClientRate(org).AddRateEntry("ORG", "LSE", "AUSYD", "");

			AssertEquals(1, entry.GetCompanyTariffLevels().FirstOrDefault());

			var tariff1 = Helper.NewCompanyTariff();
			var tariff2 = Helper.NewCompanyTariff();
			var tariff3 = Helper.NewCompanyTariff();
			var tariff4 = Helper.NewCompanyTariff();

			tariff1.Factory.Save();

			org.CompanyData.RateTariffLevels.SetLevel("ORG", nameof(OrgRateTariffLevel.Directions.EXP), "LSE", 2);
			org.CompanyData.RateTariffLevels.SetLevel("ORG", nameof(OrgRateTariffLevel.Directions.IMP), "LSE", 3);
			org.CompanyData.RateTariffLevels.SetLevel("FRT", nameof(OrgRateTariffLevel.Directions.EXP), "LSE", 4);

			AssertContainsExactElementsInAnyOrder(new[] { 2, 3 }, entry.GetCompanyTariffLevels());
			AssertContainsExactElementsInAnyOrder(new[] { 3 }, entry.GetCompanyTariffLevels(OrgRateTariffLevel.Directions.IMP));
			AssertContainsExactElementsInAnyOrder(new[] { 2 }, entry.GetCompanyTariffLevels(OrgRateTariffLevel.Directions.EXP));
		}

		public void TestGetCompanyTariffLevels_NullParent()
		{
			var org = Helper.NewOrgHeader(1);
			var ratingHeader = Helper.NewClientRate(org);
			var rateEntry = ratingHeader.AddRateEntry("ORG", "LSE", "AUSYD", "");

			ratingHeader.Delete(); //This test may be redundant after changes to RateEntry.Parent getter

			var tariffLevels = rateEntry.GetCompanyTariffLevels();
			AssertEquals(1, tariffLevels.Count());
			AssertEquals(0, tariffLevels.First());
		}

		public void TestGetCompanyTariffDiscountForLevelWithDifferentGlbCompany()
		{
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "NEW";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company2.Branches.Add(branch1);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var tariff1 = Helper.NewCompanyTariff();
				tariff1.Factory.Save();
				var tariff2 = Helper.NewCompanyTariff();
				tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 5m);
				tariff2.Factory.Save();
			}

			var tariff11 = Helper.NewCompanyTariff();
			tariff11.Factory.Save();
			var tariff22 = Helper.NewCompanyTariff();
			tariff22.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 10m);
			tariff22.Factory.Save();

			var entry = Helper.NewClientRate(Helper.NewOrgHeader()).AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			AssertEquals(10m, entry.GetCompanyTariffDiscountForLevel(2));

			entry.Parent.TH_GC = company2.PK;
			AssertEquals(5m, entry.GetCompanyTariffDiscountForLevel(2));
		}

		public void TestCompanyTariffDiscountForGlobalRates()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", FlatCalculator.Code);
			globalChargeCode.Factory.Save();

			var global1 = Helper.NewGlobalTariff();
			var globalEntry1 = global1.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "AUMEL");
			globalEntry1.RateLines.RemoveAndDeleteAll();
			globalEntry1.AddRateLine(globalChargeCode, FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 800m;
			global1.Factory.Save();

			var global2 = Helper.NewGlobalTariff();
			global2.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 10m);
			global2.Factory.Save();

			var tariff1 = Helper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "AUMEL", "FRT", 500m);
			tariff1.Factory.Save();

			var tariff2 = Helper.NewCompanyTariff();
			tariff2.Discounts.SetDiscount(RatingConstants.RateCategory.AIR, 5m);
			tariff2.Factory.Save();

			var entry = Helper.NewQuote(Helper.NewOrgHeader(2)).AddRateEntry("AIR", "LSE", "AUSYD", "AUMEL");
			var relatedLines = entry.RelatedRateLines;

			var globalTariffRateLine = relatedLines.Where(x => x.RateType == "Global Tariff").Single();
			var companyTariffRateLine = relatedLines.Where(x => x.RateType == "Company Tariff").Single();

			AssertEquals(10m, globalTariffRateLine.CompanyTariffDiscount);
			AssertEquals("Apply the discount and display the rate correctly", 720m, globalTariffRateLine.GetCalculator<FlatCalculator>().BaseRate);

			AssertEquals(5m, companyTariffRateLine.CompanyTariffDiscount);
			AssertEquals("Apply the discount and display the rate correctly", 475m, companyTariffRateLine.GetCalculator<FlatCalculator>().BaseRate);
		}

		#endregion

		#region Properties

		public void TestEmptyProperties()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			AssertNull("Via is null", entry.Via);
		}

		public void TestPopulatedProperties()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			entry.TI_ViaLRC = "AUSYD";
			AssertEquals("Via", "Sydney", entry.Via.Description);
		}

		#endregion

		#region TestSelectedLineChangeCodeCallsParentOnSelectedRateLineChanged

		public void TestSelectedLineChangeCodeLoadsRelatedCollectionWhenNeeded()
		{
			RatingHeader header = Helper.NewQuote(Helper.NewOrgHeader());
			var entry = header.AddRateEntry(RatingConstants.RateCategory.LCL);

			AssertNull("Precondition", entry.fRelatedRateLinesForTest);

			entry.SelectedLineChargeCode = ZGuid.NewZGuid();

			AssertNull("Not loaded no handlers on header", entry.fRelatedRateLinesForTest);

			header.SelectedRateLineChanged += (sender, e) => { };

			entry.SelectedLineChargeCode = ZGuid.NewZGuid();

			AssertNotNull("Loaded to be processed by handlers on header", entry.fRelatedRateLinesForTest);
		}

		#endregion

		#region TestCheckDuplicateColumn

		public void TestCheckDuplicateColumn()
		{
			var allRateEntryColumns = RateEntrySchema.All.ToList();

			var columnsToIgnore = new ReadOnlyCollection<SchemaColumn>(new SchemaColumn[]
			{
				RateEntrySchema.PK,
				RateEntrySchema.TI_IsValid,
				RateEntrySchema.TI_GC_Publisher
			});

			var columnsNotAffectingDuplicateCheck = new ReadOnlyCollection<SchemaColumn>(new SchemaColumn[]
			{
				RateEntrySchema.TI_ParentTableCode,
				RateEntrySchema.TI_CreationSource,
				RateEntrySchema.TI_BuyersConsolRateMode,
				RateEntrySchema.TI_DataChecked,
				RateEntrySchema.TI_LineOrder,
				RateEntrySchema.TI_MatchContainerRateClass,
				RateEntrySchema.TI_OH_AgentOverride,
				RateEntrySchema.TI_PageClosingText,
				RateEntrySchema.TI_PageHeading,
				RateEntrySchema.TI_PageOpeningText,
				RateEntrySchema.TI_QuotePageIncoTerm,
				RateEntrySchema.TI_RateEndDate,
				RateEntrySchema.TI_RateStartDate,
				RateEntrySchema.TI_RX_NKCurrency,
				RateEntrySchema.TI_SystemCreateTimeUtc,
				RateEntrySchema.TI_SystemCreateUser,
				RateEntrySchema.TI_SystemLastEditTimeUtc,
				RateEntrySchema.TI_SystemLastEditUser,
				RateEntrySchema.TI_IsExcludedFromAutoRating,
				RateEntrySchema.TI_ContractNumberLinked,
				RateEntrySchema.TI_ProviderReferenceID,
			});

			var columnsForDuplicateCheck = allRateEntryColumns.Except(columnsNotAffectingDuplicateCheck).Except(columnsToIgnore);

			var header = Factory.New<RatingHeader>();

			foreach (var column in columnsForDuplicateCheck)
			{
				var entry1 = header.AddRateEntry(RatingConstants.RateCategory.DST);
				var entry2 = header.AddRateEntry(RatingConstants.RateCategory.DST);

				var value1 = GetRandomTestValue(null, column.ColumnType);
				var value2 = GetRandomTestValue(value1, column.ColumnType);

				entry1[column] = value1;
				entry2[column] = value2;

				Assert("Precondition", !entry1[column].Equals(entry2[column]));

				if (entry1.IsDuplicate(entry2))
				{
					var message = string.Format("Add {0} column to RateEntry.duplicateSearchColumns if it belongs to the candidate key, otherwise to the columnsNotAffectingDuplicateCheck in this test to indicate this column is not considered during duplicate search", column.Name);
					Assert(message, false);
				}
			}

			var entry3 = header.AddRateEntry(RatingConstants.RateCategory.DST);
			var entry4 = header.AddRateEntry(RatingConstants.RateCategory.DST);

			foreach (var noDuplicateColumn in columnsNotAffectingDuplicateCheck)
			{
				var value1 = GetRandomTestValue(null, noDuplicateColumn.ColumnType);
				var value2 = GetRandomTestValue(value1, noDuplicateColumn.ColumnType);

				entry3[noDuplicateColumn] = value1;
				entry4[noDuplicateColumn] = value2;

				Assert("Precondition", !entry3[noDuplicateColumn].Equals(entry4[noDuplicateColumn]));
			}

			Assert(entry3.IsDuplicate(entry4));
		}

		object GetRandomTestValue(object column, SchemaColumnType columnType)
		{
			object columnValue = null;

			switch (columnType)
			{
				case SchemaColumnType.Bool:
					columnValue = (column == null);
					break;
				case SchemaColumnType.DateTime:
					columnValue = (column == null) ? ZDateTime.Now : ZDateTime.Now.AddMonths(1);
					break;
				case SchemaColumnType.Date:
					columnValue = (column == null) ? ZDate.Today : ZDate.Today.AddMonths(1);
					break;
				case SchemaColumnType.Byte:
				case SchemaColumnType.Decimal:
				case SchemaColumnType.Int:
					columnValue = (column == null) ? 1 : 0;
					break;
				case SchemaColumnType.Short:
					columnValue = (column == null) ? (ZShort)1 : (ZShort)0;
					break;
				case SchemaColumnType.String:
					columnValue = (column == null) ? "A" : "B";
					break;
				case SchemaColumnType.Guid:
					columnValue = ZGuid.NewZGuid();
					break;
				default:
					break;
			}

			return columnValue;
		}

		#endregion

		#region ReadOnly

		public void TestEntryIsReadOnlyForUnauthorizedUsers()
		{
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry("AIR");
			entry1.TI_OH_Consignee = setupResult.DeniedOrg.PK;

			var entry2 = rate.AddRateEntry("ORG");
			entry2.TI_OH_Consignee = setupResult.AllowedOrg.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.CachingEnabled = false;

				var securityABC = Env.Security.FindCheckPoint(Env.Security.RatesSecurity.Code + "ABC");
				Assert("Just to check security is found and working fine", !securityABC.IsAllowed);

				var securityXYZ = Env.Security.FindCheckPoint(Env.Security.RatesSecurity.Code + "XYZ");
				Assert("Just to check security is found and working fine", securityXYZ.IsAllowed);

				Assert(entry1.ReadOnly);
				Assert(!entry2.ReadOnly);

				entry1.TI_OH_Consignee = setupResult.AllowedOrg.PK;
				entry2.TI_OH_Consignee = setupResult.DeniedOrg.PK;

				Factory.Save();

				Assert(!entry1.ReadOnly);
				Assert(entry2.ReadOnly);
			}
		}

		public void TestIsReadOnlyDueToGlobalPublisher_ClientRate()
		{
			var client = Helper.NewOrgHeader();
			var globalClientRate = Helper.NewGlobalClientRate(client);
			var rateEntry1 = globalClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AU", "");

			var localClientRate = Helper.NewClientRate(client);
			var localRateEntry1 = localClientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AU", "");

			TestIsReadOnlyDueToGlobalPublisher(rateEntry1, localRateEntry1, Env.Security.GlobalClientRatesEditFromAnyCompany);
		}

		public void TestIsReadOnlyDueToGlobalPublisher_Costing()
		{
			var serviceProvider = Helper.NewOrgHeader();
			var globalCosting = Helper.NewGlobalCosting(serviceProvider);
			var costEntry1 = globalCosting.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "CN", "");
			costEntry1.RateLines.RemoveAndDeleteAll();

			var localCosting = Helper.NewCosting(serviceProvider);
			var localEntry1 = localCosting.AddRateEntry(RatingConstants.RateCategory.LCL, Constants.RateMode.LCL, "UA", "");
			localEntry1.RateLines.RemoveAndDeleteAll();

			TestIsReadOnlyDueToGlobalPublisher(costEntry1, localEntry1, Env.Security.GlobalCostingRatesEditFromAnyCompany);
		}

		public void TestIsReadOnlyDueToGlobalPublisher_Tariff()
		{
			var globalTariff = Factory.New<GlobalTariff>();
			var tariffEntry1 = globalTariff.AddRateEntry(RatingConstants.RateCategory.DST, Constants.RateMode.SEA, "AUSYD", "");

			var companyTariff = Factory.New<CompanyTariff>();
			var localEntry1 = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AU", "");

			TestIsReadOnlyDueToGlobalPublisher(tariffEntry1, localEntry1, Env.Security.GlobalTariffRatesEditFromAnyCompany);
		}

		void TestIsReadOnlyDueToGlobalPublisher(RateEntry rateEntry1, RateEntry localRateEntry1, SecurityCheckpoint securityCheckPoint)
		{
			var isAllowed = securityCheckPoint.IsAllowed;

			try
			{
				var company1 = Factory.NewWithValidTestData<GlbCompany>();
				company1.GC_Code = "NEW";
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				company1.Branches.Add(branch1);

				var company2 = Factory.NewWithValidTestData<GlbCompany>();
				company2.GC_Code = "NUP";
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();
				company2.Branches.Add(branch2);

				securityCheckPoint.IsAllowed = false;
				Assert("Pre-condition", !securityCheckPoint.IsAllowed);
				Factory.ClearCachedValue<bool>($"CanEditFromAnyCompany.{rateEntry1.TI_TH}");

				Factory.Save();

				RateEntry rateEntry2;
				RateEntry localRateEntry2;

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					rateEntry2 = rateEntry1.Parent.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.SEA, "", "AU");
					localRateEntry2 = localRateEntry1.Parent.AddRateEntry(RatingConstants.RateCategory.WHS, Constants.RateMode.ALL, "", "");
					Factory.Save();

					AssertEquals(rateEntry1.IsReadOnlyDueToGlobalPublisher, true);
					AssertEquals(rateEntry2.IsReadOnlyDueToGlobalPublisher, false);
					AssertEquals(localRateEntry1.IsReadOnlyDueToGlobalPublisher, false);
					AssertEquals(localRateEntry2.IsReadOnlyDueToGlobalPublisher, false);
				}

				AssertEquals(rateEntry1.IsReadOnlyDueToGlobalPublisher, false);
				AssertEquals(rateEntry2.IsReadOnlyDueToGlobalPublisher, true);
				AssertEquals(localRateEntry1.IsReadOnlyDueToGlobalPublisher, false);
				AssertEquals(localRateEntry2.IsReadOnlyDueToGlobalPublisher, false);

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					AssertEquals(rateEntry1.IsReadOnlyDueToGlobalPublisher, true);
					AssertEquals(rateEntry2.IsReadOnlyDueToGlobalPublisher, true);
					AssertEquals(localRateEntry1.IsReadOnlyDueToGlobalPublisher, false);
					AssertEquals(localRateEntry2.IsReadOnlyDueToGlobalPublisher, false);

					securityCheckPoint.IsAllowed = true;
					Assert("Pre-condition", securityCheckPoint.IsAllowed);

					var newFactory = new BusinessObjectFactory();
					var reloadedRateEntry1 = newFactory.Load<RateEntry>(rateEntry1.PK);
					var reloadedRateEntry2 = newFactory.Load<RateEntry>(rateEntry2.PK);

					AssertEquals(reloadedRateEntry1.IsReadOnlyDueToGlobalPublisher, false);
					AssertEquals(reloadedRateEntry2.IsReadOnlyDueToGlobalPublisher, false);
				}
			}
			finally
			{
				securityCheckPoint.IsAllowed = isAllowed;
			}
		}

		public void TestCanEditFromAnyCompany_OnlyFalseWhenSecurityIsDisallowed()
		{
			var message = "CanEditFromAnyCompany should only set RateEntries as read only if they are disallowed by security. " +
				"It should be false by default as that sets RateEntries to be read only without any way for the user to fix it.";

			var canEditFromAnyCompanyProperty = typeof(RateEntry).GetProperty("CanEditFromAnyCompany", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNotNull("Pre-condition: expected property to exist", canEditFromAnyCompanyProperty);

			var entry1 = Helper.NewClientRate(NewClient).AddRateEntry(RatingConstants.RateCategory.WHS);
			var result = canEditFromAnyCompanyProperty.GetValue(entry1);

			AssertEquals(message, true, (bool)result);

			Env.Security.GlobalClientRatesEditFromAnyCompany.IsAllowed = false;
			var entry2 = Helper.NewGlobalClientRate(NewClient).AddRateEntry(RatingConstants.RateCategory.WHS);
			result = canEditFromAnyCompanyProperty.GetValue(entry2);

			AssertEquals(message, false, (bool)result);

			var entry3 = Helper.NewIntercompanyTariff().AddRateEntry(RatingConstants.RateCategory.WHS);
			result = canEditFromAnyCompanyProperty.GetValue(entry3);

			AssertEquals(message, true, (bool)result);
		}

		#endregion

		#region IsPublished

		public void TestIsPublished_EntryHasNoHeader_ReturnFalse()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry("AIR", "ULD", "UAIEV", "AUSYD");

			entry.Parent = null;
			AssertEquals(false, entry.IsPublished);
		}

		public void TestIsPublished_CompanyTariff_CreatesGlobalTariff()
		{
			var companyTariff = Helper.NewCompanyTariff();
			AssertCanPublishAndUnpublishRates(companyTariff);
			AssertSecurityCheckPointsMakeIsPublishedReadonly(companyTariff, Env.Security.GlobalTariffRatesPublishAndUnpublish);
		}

		public void TestIsPublished_CompanyTariff_LoadsExisting()
		{
			var globalTariff = Helper.NewGlobalTariff();
			globalTariff.Factory.Save();

			var companyTariff = Helper.NewCompanyTariff();
			AssertCanPublishAndUnpublishRates(companyTariff, globalTariff);
			AssertSecurityCheckPointsMakeIsPublishedReadonly(companyTariff, Env.Security.GlobalTariffRatesPublishAndUnpublish);
		}

		public void TestIsPublished_ClientRate_CreatesNew()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			AssertCanPublishAndUnpublishRates(clientRate);
			AssertSecurityCheckPointsMakeIsPublishedReadonly(clientRate, Env.Security.GlobalClientRatesPublishAndUnpublish);
		}

		public void TestIsPublished_ClientRate_LoadsExisting()
		{
			var client = Helper.NewOrgHeader();
			var globalClientRate = Helper.NewGlobalClientRate(client);
			Factory.Save();

			var localClientRate = Helper.NewClientRate(client);
			AssertCanPublishAndUnpublishRates(localClientRate, globalClientRate);
			AssertSecurityCheckPointsMakeIsPublishedReadonly(localClientRate, Env.Security.GlobalClientRatesPublishAndUnpublish);
		}

		public void TestIsPublished_Costing_CreatesNew()
		{
			var costing = Helper.NewCosting(Helper.CreateCreditor());
			AssertCanPublishAndUnpublishRates(costing);
			AssertSecurityCheckPointsMakeIsPublishedReadonly(costing, Env.Security.GlobalCostingRatesPublishAndUnpublish);
		}

		public void TestIsPublished_Costing_LoadsExisting()
		{
			var serviceProvider = Helper.CreateCreditor();
			var globalCosting = Helper.NewGlobalCosting(serviceProvider);
			Factory.Save();

			var localCosting = Helper.NewCosting(serviceProvider);
			AssertCanPublishAndUnpublishRates(localCosting, globalCosting);
			AssertSecurityCheckPointsMakeIsPublishedReadonly(localCosting, Env.Security.GlobalCostingRatesPublishAndUnpublish);
		}

		void AssertCanPublishAndUnpublishRates(RatingHeader localRatingHeader, RatingHeader globalRatingHeader = null)
		{
			var entry1 = localRatingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "CN");
			var entry2 = localRatingHeader.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "CN", "");

			CombineAssertions(() =>
			{
				Assert("Pre-condition: localRatingHeader should be able to Publish Rates", localRatingHeader.SupportsRateEntryPublish());

				if (globalRatingHeader == null)
				{
					AssertNull("No Global Rating Header currently exists", localRatingHeader.GlobalRatingHeader);
				}
				else
				{
					AssertEquals("Pre-condition: GlobalRatingHeader should be set up to match", globalRatingHeader.PK, localRatingHeader.GlobalRatingHeader.PK);
				}

				entry1.IsPublished = true;

				AssertNotNull("Publishing a Rate should create a Global Rating Header", localRatingHeader.GlobalRatingHeader);
				AssertEquals("Should move the Published Rate Entry to the newly Created Global Rating Header", entry1.TI_TH, localRatingHeader.GlobalRatingHeader.PK);
				AssertEquals("Should not affect unpublished rate", entry2.TI_TH, localRatingHeader.PK);

				entry1.IsPublished = false;
				AssertEquals("Should be able to Unpublish Rates", entry1.TI_TH, localRatingHeader.PK);
			});
		}

		void AssertSecurityCheckPointsMakeIsPublishedReadonly(RatingHeader localRatingHeader, SecurityCheckpoint securityCheckpoint)
		{
			var isAllowed = securityCheckpoint.IsAllowed;
			try
			{
				securityCheckpoint.IsAllowed = false;

				var newFactory = new BusinessObjectFactory();
				var reloadedRatingHeader = newFactory.Load<RatingHeader>(localRatingHeader.PK);
				var rateEntry = localRatingHeader.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "AU", "CN");
				localRatingHeader.Factory.Save();

				Assert("localRatingHeader shouldn't be able to Publish Rates", rateEntry.IsPublishedInfo.ReadOnly);

				rateEntry.Delete();

				securityCheckpoint.IsAllowed = true;

				newFactory = new BusinessObjectFactory();
				reloadedRatingHeader = newFactory.Load<RatingHeader>(localRatingHeader.PK);
				rateEntry = reloadedRatingHeader.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.ALL, "GB", "AU");

				Assert("localRatingHeader should be able to Publish Rates", !rateEntry.IsPublishedInfo.ReadOnly);
			}
			finally
			{
				securityCheckpoint.IsAllowed = isAllowed;
			}
		}

		public void TestIsPublished_CreatesNewGlobalRateWhenDeleted()
		{
			var globalCosting = Helper.NewGlobalCosting(null);
			var originalGlobalCostingPK = globalCosting.PK;

			var localCosting = Helper.NewCosting(null);
			var localCostEntry1 = localCosting.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "CN");

			AssertEquals("Pre-condition", originalGlobalCostingPK, localCosting.GlobalRatingHeader.PK);

			globalCosting.Delete();
			Assert(localCosting.GlobalRatingHeader.IsDeleted);

			localCostEntry1.IsPublished = true;

			AssertNotEquals("Should have created a new global costing instead of using the delete one", originalGlobalCostingPK, localCosting.GlobalRatingHeader.PK);
			Assert(!localCosting.GlobalRatingHeader.IsDeleted);
			AssertEquals(localCostEntry1.TI_TH, localCosting.GlobalRatingHeader.PK);
		}

		public void TestIsPublished_ConvertsChargeCodesWithoutClearingCalculatorValues()
		{
			var globalChargeCode1 = Helper.ChargeCodes.CreateGlobalCharge("GLBCHRG1", FlatCalculator.Code);
			var globalChargeCode2 = Helper.ChargeCodes.CreateGlobalCharge("GLBCHRG2", PercentageCalculator.Code);
			Factory.Save();

			var localChargeCode1 = globalChargeCode1.ChildChargeCodes.Single(cc => cc.AC_GC == Env.CurrentCompanyPK);
			var localChargeCode2 = globalChargeCode2.ChildChargeCodes.Single(cc => cc.AC_GC == Env.CurrentCompanyPK);
			localChargeCode2.AC_RateCalculator = FlatCalculator.Code;
			AssertNotNull("Should have created a single local charge code for the current company upon saving", localChargeCode1);
			AssertNotNull("Should have created a single local charge code for the current company upon saving", localChargeCode2);

			var freightChargeCode = Helper.ChargeCodes["FRT"];

			var client = Helper.NewOrgHeader();
			var localClientRate = Helper.NewClientRate(client);
			var rateEntry = localClientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine(localChargeCode1, UnitCalculator.Code, QuantityUnit.HB);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 10m;

			var rateLine2 = rateEntry.AddRateLine(freightChargeCode, FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 150m;

			rateEntry.IsPublished = true;
			rateLine1.Validation.ValidateAll();
			rateLine2.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertEquals("Should have converted charge code", rateLine1.TL_AC, globalChargeCode1.PK);
				AssertEquals("Should remain a unit calculator", UnitCalculator.Code, rateLine1.TL_RateCalculator);
				AssertEquals(QuantityUnit.HB, rateLine1.TL_WeightVolume);
				AssertEquals("Rate should remain the same", 10m, rateLine1.GetCalculator<UnitCalculator>().PerUnit);

				AssertEquals("Has no global charge code to default, expected not to change", rateLine2.TL_AC, freightChargeCode.PK);
				AssertEquals(FlatCalculator.Code, rateLine2.TL_RateCalculator);
				AssertHasError(rateLine2.TL_ACInfo, "Enter a valid Charge Code.");
				AssertEquals("Rate should remain the same", 150m, rateLine2.GetCalculator<FlatCalculator>().BaseRate);
			});

			var rateLine3 = rateEntry.AddRateLine(globalChargeCode2, PercentageCalculator.Code);
			var percentageCalculator = rateLine3.GetCalculator<PercentageCalculator>();
			percentageCalculator.Percent = 7.5m;
			percentageCalculator.BaseRate = 100m;
			percentageCalculator.AddApplyToItem(CalculatorConstants.Text.ChargeCode).TM_AC = globalChargeCode1.PK;

			rateEntry.IsPublished = false;
			rateLine1.Validation.ValidateAll();
			rateLine2.Validation.ValidateAll();
			rateLine3.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertEquals("Should convert back to local charge code", rateLine1.TL_AC, localChargeCode1.PK);
				AssertNoErrors(rateLine1.TL_ACInfo);
				AssertEquals(UnitCalculator.Code, rateLine1.TL_RateCalculator);
				AssertEquals(QuantityUnit.HB, rateLine1.TL_WeightVolume);
				AssertEquals(10m, rateLine1.GetCalculator<UnitCalculator>().PerUnit);

				AssertEquals("Should remain unchanged", rateLine2.TL_AC, freightChargeCode.PK);
				AssertNoErrors(rateLine2.TL_ACInfo);
				AssertEquals(FlatCalculator.Code, rateLine2.TL_RateCalculator);
				AssertEquals(150m, rateLine2.GetCalculator<FlatCalculator>().BaseRate);

				AssertEquals("Should be converted to local charge code", rateLine3.TL_AC, localChargeCode2.PK);
				AssertNoErrors(rateLine3.TL_ACInfo);
				AssertEquals(PercentageCalculator.Code, rateLine3.TL_RateCalculator);
				AssertEquals(7.5m, rateLine3.GetCalculator<PercentageCalculator>().Percent);
				AssertEquals(100m, rateLine3.GetCalculator<PercentageCalculator>().BaseRate);

				var applyTo = rateLine3.RateLineItems.Cast<RateLineItem>().FirstOrDefault(x => x.RateOperatorIsApplyTo());
				AssertNotNull(applyTo);
				AssertEquals("Percentage of should also covert to local charge code", localChargeCode1.PK, applyTo.TM_AC);
			});
		}

		public void TestIsPublished_CannotPublish_DuplicateRates()
		{
			var creditor = Helper.CreateCreditor();
			var globalCosting = Helper.NewGlobalCosting(creditor);
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var costEntry1PK = newFactory.Load<Costing>(globalCosting.PK)
				.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "CN", "").PK;
			//Has to be in another factory so the first ParentCollection found is that of the local Costing
			newFactory.Save();

			var localCosting = Helper.NewCosting(creditor);

			//Create a User Filter with no ZQuery so we don't filter the Published Rate
			var emptyUserFilter = new RateEntryFilterStripBusinessObjectForTest(new ZQuery());
			localCosting.EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection.SetUserFilter(emptyUserFilter);

			var costEntry1 = localCosting.LCLRateEntriesForBinding.Cast<RateEntry>().Single(x => x.PK == costEntry1PK);
			var costEntry2 = localCosting.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "CN", "");
			localCosting.RunPreSaveValidation();

			AssertEquals("Pre-condition", true, costEntry1.IsPublished);
			AssertEquals("Pre-condition", false, costEntry2.IsPublished);
			AssertNoRowErrors("Different TI_TH on otherwise identical rates so there is no error expected", costEntry1);
			AssertNoRowErrors("Different TI_TH on otherwise identical rates so there is no error expected", costEntry2);

			costEntry2.IsPublished = true;
			localCosting.RunPreSaveValidation();

			AssertHasRowError("Changed IsPublished changes the parent", costEntry1, ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError("Changed IsPublished changes the parent", costEntry2, ErrorMessages.OverlappingDatesOnRateEntry);

			costEntry2.IsPublished = false;
			localCosting.RunPreSaveValidation();

			AssertNoRowErrors(costEntry1);
			AssertNoRowErrors(costEntry2);
		}

		public void TestIsPublished_CannotUnpublish_DuplicateRates()
		{
			var creditor = Helper.CreateCreditor();
			var globalCosting = Helper.NewGlobalCosting(creditor);
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var costEntry1PK = newFactory.Load<Costing>(globalCosting.PK)
				.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "CN", "").PK;
			//Has to be in another factory so the first ParentCollection found is that of the local Costing
			newFactory.Save();

			var localCosting = Helper.NewCosting(creditor);

			//Create a User Filter with no ZQuery so we don't filter the Published Rate
			var emptyUserFilter = new RateEntryFilterStripBusinessObjectForTest(new ZQuery());
			localCosting.EntryCollections[RatingConstants.RateCategory.LCL].LazyLoadingCollection.SetUserFilter(emptyUserFilter);

			var costEntry1 = localCosting.LCLRateEntriesForBinding.Cast<RateEntry>().Single(x => x.PK == costEntry1PK);
			var costEntry2 = localCosting.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "CN", "");
			localCosting.RunPreSaveValidation();

			AssertEquals(localCosting.PK, costEntry1.Parent.PK);
			AssertEquals(true, costEntry1.IsPublished);
			AssertEquals(false, costEntry2.IsPublished);
			AssertNoRowErrors("Different TI_TH on otherwise identical rates so there is no error expected", costEntry1);
			AssertNoRowErrors("Different TI_TH on otherwise identical rates so there is no error expected", costEntry2);

			costEntry1.IsPublished = false;
			localCosting.RunPreSaveValidation();

			AssertHasRowError("Changed IsPublished changes the parent", costEntry1, ErrorMessages.OverlappingDatesOnRateEntry);
			AssertHasRowError("Changed IsPublished changes the parent", costEntry2, ErrorMessages.OverlappingDatesOnRateEntry);

			costEntry1.IsPublished = true;
			localCosting.RunPreSaveValidation();

			AssertNoRowErrors(costEntry1);
			AssertNoRowErrors(costEntry2);
		}

		#endregion

		#region TestImportWizardWithErrorData
		[ExpectNoExceptions]
		public void TestImportWizardWithErrorData()
		{
			var clientRate = Factory.New<ClientRate>();
			var collection = clientRate.AllEntriesCollection;
			var impl = new ImportCollectionInfoImpl(collection)
			{
				new ImportPropertyInfoImpl<RateEntry>(RateEntrySchema.Constants.TI_RateStartDate) { HeaderText = "Start Date" },
				new ImportPropertyInfoImpl<RateEntry>(RateEntrySchema.Constants.TI_RateEndDate) { HeaderText = "End Date" }
			};

			var wizard = new Mock<ImportWizard>(impl, null, new FileMapperForTest()) { CallBase = true };
			wizard.Setup(m => m.LoadFile(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(new List<string[]>() { new[] { "2009-4-24", "2016-4-31" } });

			wizard.Object.Mapping[0].AddFileColumnIndex(0);
			wizard.Object.Mapping[1].AddFileColumnIndex(1);
			wizard.Object.ImportIntoCollection(collection, -1);
		}
		#endregion

		#region ContractNumberLinked

		public void TestContractNumberLinked_RegistryDisabled_IsReadOnly()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var org = Helper.NewOrgHeader();
				var costing = Helper.NewCosting(org);
				Helper.NewRatingContract(org, "555", RatingContractTypes.Provider);

				var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
				Assert("ContractNumberLinked should always be readonly when the registry is disabled", entry.TI_ContractNumberLinkedInfo.ReadOnly);
				entry.TI_ContractNumber = "555";
				Assert("ContractNumberLinked should always be readonly when the registry is disabled", entry.TI_ContractNumberLinkedInfo.ReadOnly);
			}
		}

		public void TestContractNumberLinked_RegistryEnabled_DeletionWarningText()
		{
			var expectedMessage = "You are about to delete rates that have been linked to Carrier Contract & Allocation record(s).";

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Helper.NewOrgHeader();
				var costing = Helper.NewCosting(org);
				Helper.NewRatingContract(org, "555", RatingContractTypes.Provider);
				var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
				entry.TI_ContractNumber = "555";
				entry.TI_ContractNumberLinked = true;

				AssertEquals(
					"Warning is present whether the RateEntry is saved or not",
					expectedMessage,
					entry.GetWarningBeforeBeingDeleted()
				);

				Factory.Save();

				AssertEquals(
					"Warning is present whether the RateEntry is saved or not",
					expectedMessage,
					entry.GetWarningBeforeBeingDeleted()
				);
			}
		}

		public void TestContractNumberLinked_RegistryEnabled_WhenCheckedAndCloned_CloneIsNotChecked()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = Helper.NewOrgHeader();
				var costing = Helper.NewCosting(org);
				Helper.NewRatingContract(org, "555", RatingContractTypes.Provider);

				var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
				entry.TI_ContractNumber = "555";
				entry.TI_ContractNumberLinked = true;
				AssertEquals("Once the contract linked is ticked, you cannot change the contract number", true, entry.TI_ContractNumberInfo.ReadOnly);

				Factory.Save();
				AssertEquals("After saving, you can't undo the check", true, entry.TI_ContractNumberLinkedInfo.ReadOnly);

				var cloned = entry.Clone(costing.FCLRateEntriesForBinding);
				AssertEquals("It should not be linked", false, cloned.TI_ContractNumberInfo.ReadOnly);
				AssertEquals("It should not be linked", false, cloned.TI_ContractNumberLinkedInfo.ReadOnly);
				AssertEquals("It should not be linked", false, cloned.TI_ContractNumberLinked);
				AssertEquals("Contract number is copied", "555", cloned.TI_ContractNumber);
			}
		}

		public void TestContractNumberLinked_RegistryEnabled_WhenChecked_ContractNumberIsReadonly()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var costing = Helper.NewCosting(Helper.NewOrgHeader());
				var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
				entry.TI_ContractNumber = "555";
				entry.TI_ContractNumberLinked = false;
				AssertEquals(
					"The contract number can be changed when it is not linked",
					false,
					entry.TI_ContractNumberInfo.ReadOnly
				);
				AssertEquals(
					"An invalid carrier-contract-allocation contract number is detected by validation not by readonly-changing",
					false,
					entry.TI_ContractNumberLinkedInfo.ReadOnly
				);

				entry.TI_ContractNumberLinked = true;
				AssertEquals(
					"Once the contract linked is ticked, you cannot change the contract number",
					true,
					entry.TI_ContractNumberInfo.ReadOnly
				);
			}
		}

		public void TestContractNumberLinked_RegistryEnabled_WhenSavedThenChecked_ContractNumberLinkedIsReadonly()
		{
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var costing = Helper.NewCosting(Helper.NewOrgHeader());
				var entry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "BAF", 100);
				entry.TI_ContractNumber = "555";
				entry.TI_ContractNumberLinked = true;

				AssertEquals(
					"I can still untick a ContractNumberLinked if it has not been saved yet.",
					false,
					entry.TI_ContractNumberLinkedInfo.ReadOnly
				);

				Factory.Save();

				AssertEquals(
					"Once saved, I can no longer change the ContractNumberLinked",
					true,
					entry.TI_ContractNumberLinkedInfo.ReadOnly
				);
			}
		}

		#endregion

		public void TestImportedFCLRateEntryNoDefaultRateLines()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entryWhenNotImporting = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL);
			AssertEquals("When not importing, there is a default rate line made for FCL RateEntry", 1, entryWhenNotImporting.RateLines.Count);

			using (SupportDataImportingHelper.DataImporting(clientRate))
			{
				var entryWhenImporting = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL);
				AssertEquals("When importing, there are no RateLines for an FCL RateEntry. This is to prevent unexpected RateLines being added to what the user imports", 0, entryWhenImporting.RateLines.Count);
			}

			using (DataImportIndicatorService.StartDataImport(Factory))
			{
				var entryWhenImporting = clientRate.AddRateEntry(RatingConstants.RateCategory.FCL);
				AssertEquals("When importing, there are no RateLines for an FCL RateEntry. This is to prevent unexpected RateLines being added to what the user imports", 0, entryWhenImporting.RateLines.Count);
			}
		}

		public void TestValidation()
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL);

			Assert(entry.Validation is ClientAndCostRateEntryValidation);
		}

		public void TestDeniedSecurityCheckPoint()
		{
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var quote = Helper.NewQuote(setupResult.AllowedOrg);
				var clientRate = Helper.NewClientRate(setupResult.AllowedOrg);
				var quoteEntry = quote.AddRateEntry("AIR", "LSE", "AU", "ZA");
				var rateEntry = clientRate.AddRateEntry("AIR", "LSE", "AU", "ZA");
				Factory.Save();

				AssertNull(quoteEntry.DeniedSecurityCheckPoint);
				AssertNull(rateEntry.DeniedSecurityCheckPoint);

				quote.TH_OH = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity, quoteEntry.DeniedSecurityCheckPoint);

				clientRate.TH_OH = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity, rateEntry.DeniedSecurityCheckPoint);

				quote.TH_OH = setupResult.AllowedOrg.PK;
				clientRate.TH_OH = setupResult.AllowedOrg.PK;

				quoteEntry.TI_OH_Consignee = setupResult.AllowedOrg.PK;
				quoteEntry.TI_OH_Consignor = setupResult.AllowedOrg.PK;
				quoteEntry.TI_OH_Supplier = setupResult.AllowedOrg.PK;
				rateEntry.TI_OH_Consignee = setupResult.AllowedOrg.PK;
				rateEntry.TI_OH_Consignor = setupResult.AllowedOrg.PK;
				rateEntry.TI_OH_Supplier = setupResult.AllowedOrg.PK;

				Factory.Save();
				AssertNull(quoteEntry.DeniedSecurityCheckPoint);
				AssertNull(rateEntry.DeniedSecurityCheckPoint);

				quoteEntry.TI_OH_Consignee = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity, quoteEntry.DeniedSecurityCheckPoint);

				quoteEntry.TI_OH_Consignee = setupResult.AllowedOrg.PK;
				quoteEntry.TI_OH_Consignor = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity, quoteEntry.DeniedSecurityCheckPoint);

				quoteEntry.TI_OH_Consignor = setupResult.AllowedOrg.PK;
				quoteEntry.TI_OH_Supplier = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity, quoteEntry.DeniedSecurityCheckPoint);

				quoteEntry.TI_OH_Supplier = setupResult.AllowedOrg.PK;
				rateEntry.TI_OH_Consignee = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity, rateEntry.DeniedSecurityCheckPoint);

				rateEntry.TI_OH_Consignee = setupResult.AllowedOrg.PK;
				rateEntry.TI_OH_Consignor = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity, rateEntry.DeniedSecurityCheckPoint);

				rateEntry.TI_OH_Consignor = setupResult.AllowedOrg.PK;
				rateEntry.TI_OH_Supplier = setupResult.DeniedOrg.PK;
				Factory.Save();
				AssertEquals(setupResult.DeniedSecurity, rateEntry.DeniedSecurityCheckPoint);
			}
		}

		public void TestDBHit_RateEntryDelete()
		{
			var clientRate = Factory.NewWithValidTestData<ClientRate>();
			var rateEntry = clientRate.AddRateEntryWithFlatRateLine("AIR", "LSE", "AU", "FR", "BAF", 100);

			var newPivot = Factory.NewWithValidTestData<OrgSalesValueAssociationPivot>();
			newPivot.SVP_ActivityTableCode = RateEntrySchema.Constants.Prefix;
			newPivot.SVP_ActivityId = rateEntry.PK;
			newPivot.SVP_TradeTableCode = "OW";
			Factory.Save();

			var pivotForRateEntryTable = new ZQuery(OrgSalesValueAssociationPivotSchema.SVP_ActivityTableCode, RateEntrySchema.Constants.Prefix);
			var pivotForRateEntryGuid = new ZQuery(OrgSalesValueAssociationPivotSchema.SVP_ActivityId, rateEntry.PK);
			var pivotQuery = new ZQuery(pivotForRateEntryGuid, pivotForRateEntryTable);

			var hits =
				new Dictionary<string, int>
				{
					{ OrgSalesValueAssociationPivotSchema.Constants.TableName, 0 },
				};
			Factory.ResetDatabaseLoadCount();
			using (AssertDbHitsWithUsefulQueryInformation(hits, Factory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				rateEntry.Delete();
				Factory.Save();
			}

			var otherFactory = new BusinessObjectFactory();
			var pivotObjects = otherFactory.Load<OrgSalesValueAssociationPivot>(pivotQuery);
			AssertEquals("Sales Pivot for RateEntry does not exist", 0, pivotObjects.Length);
		}

		public void TestHasRateLinesDoesntHitDB()
		{
			var factory = new BusinessObjectFactory();

			var rate = Factory.NewWithValidTestData<ClientRate>();
			rate.TH_OH = NewClient.PK;

			var rateEntry1 = rate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "NZAKL", "FRT", 40m);

			TestCaseHelper.ClearTable(RateLinesSchema.Constants.TableName);
			Factory.ResetDatabaseLoadCount();

			var hasRateLines = rateEntry1.HasRateLines;

			var tableHitDictionary = new Dictionary<string, int>
			{
				{ RateLinesSchema.Constants.TableName, 0 },
			};

			AssertDbHits(tableHitDictionary, Factory);
		}

		public void TestFilteredRateLinesForBinding()
		{
			var header = Helper.NewClientRate(NewClient);
			var entry = header.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.LSE, "AU", "NZ", "FRT", 100);
			var line1 = entry.RateLines[0];
			var filteredLines = entry.FilteredRateLinesForBinding;
			AssertEquals("list can't be sorted by user since the system sorts them by TL_Order", false, ((IBindingList)filteredLines).SupportsSorting);

			var line2 = ((IRateLinesWithParentEntry)filteredLines).InsertNew(0);
			var line3 = ((IRateLinesWithParentEntry)filteredLines).InsertNew(0);
			var line4 = ((IRateLinesWithParentEntry)filteredLines).InsertNew(1);
			AssertEquals("inserted at correct position", 0, (int)line3.TL_LineOrder);
			AssertEquals("inserted at correct position", 1, (int)line4.TL_LineOrder);
			AssertEquals("inserted at correct position", 2, (int)line2.TL_LineOrder);
			AssertEquals(3, (int)line1.TL_LineOrder);
		}

		public void TestFilteredRateLinesForBinding_Filter()
		{
			var header = Helper.NewClientRate(NewClient);
			var entry1 = header.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.LSE, "AUSYD", "NZ", "FRT", 100);
			var line1 = entry1.RateLines[0];
			var entry2 = header.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.LSE, "AUMEL", "NZ", "FRT", 100);
			line1.TL_Condition = RateLineConditions.OwnBrokerage;
			Factory.Save();

			entry1.SetLineFilter(new ZQuery(RateLinesSchema.TL_Condition, RateLineConditions.HandOver));
			var filteredLines = entry1.FilteredRateLinesForBinding;
			AssertEquals("line is removed as filter not match", 0, filteredLines.Count);

			entry1.SetLineFilter(new ZQuery(RateLinesSchema.TL_Condition, RateLineConditions.OwnBrokerage));
			AssertEquals("line is present as filter match", 1, filteredLines.Count);

			line1.TL_Condition = RateLineConditions.HandOver;
			AssertEquals("line is still present due to HasChanges", 1, filteredLines.Count);

			AssertEquals("PRE", false, entry2.IsFilteredRateLinesForBindingLoaded_ForTest);
			AssertEquals("can load unfiltered collection", 1, entry2.FilteredRateLinesForBinding.Count);
			entry2.SetLineFilter(new ZQuery(RateLinesSchema.TL_Condition, RateLineConditions.HandOver));
			AssertEquals("can set filter for first time after collection is loaded", 0, entry2.FilteredRateLinesForBinding.Count);
		}

		public static void AssertNoRelatedEntityIsLoaded(BusinessObjectFactory factory)
		{
			var inMemoryLinesCount = factory.Load<RateLine>(new ZQuery() { FetchOnlyFromLocalCache = true }).Length;
			AssertEquals(0, inMemoryLinesCount);

			RateLinesTest.AssertNoRelatedEntityIsLoaded(factory);
		}

		public static void AssertRelatedEntitiesAreDeleted(BusinessObjectFactory factory, IEnumerable<ZGuid> linePKs, IEnumerable<ZGuid> lineItemPKs, IEnumerable<ZGuid> stmNotePKs)
		{
			var linesCount = factory.Load<RateLine>(new ZQuery(RateLinesSchema.PK, linePKs)).Length;
			AssertEquals(0, linesCount);

			RateLinesTest.AssertRelatedEntitiesAreDeleted(factory, lineItemPKs, stmNotePKs);
		}
	}

	[TestedType(typeof(RateEntry))]
	internal sealed class RateEntryBizObjTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			//Assert("This should be implemented if a client has an issue with deleting", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<ClientRate>().AddRateEntry(Constants.TransportModes.Air);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var rate = factory.NewWithValidTestData<Quote>();
			return rate.AddRateEntry(Constants.TransportModes.Air);
		}
	}

	[TestedType(typeof(RateEntry))]
	internal sealed class RateEntrySalesValueAssociatedEntityTest : SalesValueAssociatedEntityTestCase
	{
		protected override ISalesValueAssociatedEntity GetNewEntity()
		{
			return Factory.NewWithValidTestData<ClientRate>().AddRateEntry(Constants.TransportModes.Air);
		}
	}
}
