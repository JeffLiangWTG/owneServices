using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Testing
{
	[TestedType(typeof(QuotationValueObjectDataAdapter))]
	sealed class QuotationValueObjectDataAdapterTest : RatingValueObjectDataAdapterTest<Quote>
	{
		#region Export

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			var quote = Factory.New<Quote>();

			quote.TH_GC = GlbCompany.CurrentCompany.PK;
			quote.TH_RateType = RatingConstants.RatingHeaderTypes.Quote;

			quote.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "JAYSCH")).PK;
			quote.TH_Accepted = new ZDateTime(2007, 1, 1);
			quote.TH_QuoteNumber = "Q00009001";
			quote.TH_QuoteDate = new ZDate("2007-07-25");
			quote.TH_QuoteEndDate = new ZDate("2007-08-25");

			quote.TH_AirCFX = 5m;
			quote.TH_ExportAirCFX = 6m;
			quote.TH_SeaCFX = 7m;
			quote.TH_ExportSeaCFX = 8m;

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.QuotePopulatedWithEmptyFields.xml");
			return new BusinessObjectAndExpectedOutputFileName(quote, expectedOutputFilename, ValidationKind.None, "Client Rate With Empty Fields");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var quote = Factory.New<Quote>();

			quote.TH_GC = GlbCompany.CurrentCompany.PK;
			quote.TH_RateType = RatingConstants.RatingHeaderTypes.Quote;

			quote.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "JAYSCH")).PK;
			quote.TH_Accepted = new ZDateTime(2007, 1, 1);
			quote.TH_QuoteNumber = "Q00009001";
			quote.TH_QuoteDate = new ZDate("2007-07-25");
			quote.TH_QuoteEndDate = new ZDate("2007-08-25");

			quote.TH_AirCFX = 5m;
			quote.TH_ExportAirCFX = 6m;
			quote.TH_SeaCFX = 7m;
			quote.TH_ExportSeaCFX = 8m;

			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "ADALV", "CAAAL");

			rateEntry.TI_RateCategory = "AIR";
			rateEntry.TI_RateStartDate = new ZDate("2007-07-17");
			rateEntry.TI_RateEndDate = new ZDate("2008-01-17");
			rateEntry.TI_RX_NKCurrency = "EUR";
			rateEntry.TI_IsCrossTrade = false;
			rateEntry.TI_MatchContainerRateClass = false;
			rateEntry.TI_CartagePickupAddressPostCode = "2127";
			rateEntry.TI_CartageDeliveryAddressPostCode = "2721";
			rateEntry.TI_RH_NKCommodityCode = "GEN";
			rateEntry.TI_RC = Containers["20GP"].PK;
			rateEntry.TI_TransitTime = "100";
			rateEntry.TI_Frequency = 2;
			rateEntry.TI_FrequencyUnit = "Fornight";
			rateEntry.TI_ContractNumber = "ABC123";
			rateEntry.TI_RS_NKServiceLevel_NI = "D2D";
			rateEntry.TI_ViaLRC = "AUBNE";
			rateEntry.TI_PL_NKCarrierServiceLevel = "ART";

			rateEntry.RateLines.RemoveAndDeleteAll();
			RateLine rateLine = rateEntry.RateLines.AddNew();

			try
			{
				rateLine.LockCalculator = true;
				rateLine.TL_AC = Env.Registry.FreightChargeCode;
			}
			finally
			{
				rateLine.LockCalculator = false;
			}
			rateLine.TL_ContainerOwnership = "SHP";
			rateLine.TL_RX_NKCurrency = "EUR";
			rateLine.UseOnlyActualWeightMeasure = false;
			rateLine.TL_Rounding = "NOR";
			rateLine.TL_WeightVolumeMultiple = 2.5m;
			rateLine.TL_RateCalculator = CombinedCalculator.Code;
			rateLine.TL_WeightVolume = "CN";
			rateLine.TL_Condition = RateLineConditions.UserDefined;
			rateLine.TL_ConditionalExpression = "MOD=FSA";
			rateLine.ChargeInformationNoteText = "Public Note";

			CombinedCalculator calculator = (CombinedCalculator)rateLine.Calculator;

			calculator.RateLineItems.RemoveAndDeleteAll();

			calculator.Minimum = 1.0000m;
			calculator.IsAccumulated = false;
			calculator.UseHigherChargeableLowerRateRule = false;
			calculator.UseInclusiveBreaks = false;

			RateLineItem rateItem;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("-");
			rateItem.TM_Break = 45.000m;
			rateItem.TM_Value = 2.0000m;
			rateItem.TM_FlatAmount = 0.0000m;
			rateItem.TM_BreakMinimum = 0.000m;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_Break = 45.000m;
			rateItem.TM_Value = 3.0000m;
			rateItem.TM_FlatAmount = 0.0000m;
			rateItem.TM_BreakMinimum = 0.000m;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_Break = 100.000m;
			rateItem.TM_Value = 4.0000m;
			rateItem.TM_FlatAmount = 0.0000m;
			rateItem.TM_BreakMinimum = 0.000m;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_Break = 250.000m;
			rateItem.TM_Value = 5.0000m;
			rateItem.TM_FlatAmount = 0.0000m;
			rateItem.TM_BreakMinimum = 0.000m;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_Break = 500.000m;
			rateItem.TM_Value = 6.0000m;
			rateItem.TM_FlatAmount = 0.0000m;
			rateItem.TM_BreakMinimum = 0.000m;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_Break = 1000.000m;
			rateItem.TM_Value = 7.0000m;
			rateItem.TM_FlatAmount = 0.0000m;
			rateItem.TM_BreakMinimum = 0.000m;

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.QuoteFullyPopulated.xml");
			return new BusinessObjectAndExpectedOutputFileName(quote, expectedOutputFilename, ValidationKind.None, "Client Rate");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			var quote = Factory.New<Quote>();

			quote.TH_GC = GlbCompany.CurrentCompany.PK;
			quote.TH_RateType = RatingConstants.RatingHeaderTypes.Quote;

			quote.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "JAYSCH")).PK;
			quote.TH_Accepted = new ZDateTime(2007, 1, 1);
			quote.TH_QuoteNumber = "Q00009001";
			quote.TH_QuoteDate = new ZDate("2007-07-25");
			quote.TH_QuoteEndDate = new ZDate("2007-08-25");

			quote.TH_AirCFX = 5m;
			quote.TH_ExportAirCFX = 6m;
			quote.TH_SeaCFX = 7m;
			quote.TH_ExportSeaCFX = 8m;

			var rateEntry = quote.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "ADALV", "CAAAL");

			rateEntry.TI_RateCategory = "AIR";
			rateEntry.TI_RX_NKCurrency = "EUR";
			rateEntry.TI_IsCrossTrade = false;
			rateEntry.TI_MatchContainerRateClass = false;
			rateEntry.TI_CartagePickupAddressPostCode = "2127";
			rateEntry.TI_CartageDeliveryAddressPostCode = "2721";
			rateEntry.TI_RH_NKCommodityCode = "GEN";
			rateEntry.TI_RC = Containers["20GP"].PK;
			rateEntry.TI_TransitTime = "100";
			rateEntry.TI_Frequency = 2;
			rateEntry.TI_FrequencyUnit = "Fornight";
			rateEntry.TI_ContractNumber = "ABC123";
			rateEntry.TI_RS_NKServiceLevel_NI = "D2D";
			rateEntry.TI_ViaLRC = "AUBNE";
			rateEntry.TI_PL_NKCarrierServiceLevel = "ART";

			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.RateLines.AddNew();

			try
			{
				rateLine.LockCalculator = true;
				rateLine.TL_AC = Env.Registry.FreightChargeCode;
			}
			finally
			{
				rateLine.LockCalculator = false;
			}
			rateLine.TL_RX_NKCurrency = "EUR";
			rateLine.UseOnlyActualWeightMeasure = false;
			rateLine.TL_Rounding = "NOR";
			rateLine.TL_WeightVolumeMultiple = 2.5m;
			rateLine.TL_RateCalculator = CombinedCalculator.Code;
			rateLine.TL_WeightVolume = "KG";

			var calculator = (CombinedCalculator)rateLine.Calculator;

			calculator.RateLineItems.RemoveAndDeleteAll();

			calculator.Minimum = 1.0000m;
			calculator.IsAccumulated = false;
			calculator.UseHigherChargeableLowerRateRule = false;
			calculator.UseInclusiveBreaks = false;

			RateLineItem rateItem;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("-");
			rateItem.TM_Break = 45.000m;
			rateItem.TM_Value = 2.0000m;
			rateItem.TM_FlatAmount = 0.0000m;
			rateItem.TM_BreakMinimum = 0.000m;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_Break = 45.000m;
			rateItem.TM_Value = 3.0000m;
			rateItem.TM_FlatAmount = 0.0000m;
			rateItem.TM_BreakMinimum = 0.000m;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_Break = 100.000m;
			rateItem.TM_Value = 4.0000m;
			rateItem.TM_FlatAmount = 0.0000m;
			rateItem.TM_BreakMinimum = 0.000m;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_Break = 250.000m;
			rateItem.TM_Value = 5.0000m;
			rateItem.TM_FlatAmount = 0.0000m;
			rateItem.TM_BreakMinimum = 0.000m;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_Break = 500.000m;
			rateItem.TM_Value = 6.0000m;
			rateItem.TM_FlatAmount = 0.0000m;
			rateItem.TM_BreakMinimum = 0.000m;

			rateItem = calculator.RateLineItems.AddNew();
			rateItem.TM_Type = new ZString("+");
			rateItem.TM_Break = 1000.000m;
			rateItem.TM_Value = 7.0000m;
			rateItem.TM_FlatAmount = 0.0000m;
			rateItem.TM_BreakMinimum = 0.000m;

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.QuoteFullyPopulatedWithoutRateEntryDates.xml");
			return new BusinessObjectAndExpectedOutputFileName[] { new BusinessObjectAndExpectedOutputFileName(quote, expectedOutputFilename, ValidationKind.None, "Quote Without Entry Date") };
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var quote = Factory.New<Quote>();

			quote.TH_GC = GlbCompany.CurrentCompany.PK;
			quote.TH_RateType = RatingConstants.RatingHeaderTypes.Quote;

			quote.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "JAYSCH")).PK;
			quote.TH_Accepted = new ZDateTime(2007, 1, 1);
			quote.TH_QuoteNumber = "Q00009001";
			quote.TH_QuoteDate = new ZDate("2007-07-25");
			quote.TH_QuoteEndDate = new ZDate("2007-08-25");

			quote.TH_AirCFX = 5m;
			quote.TH_ExportAirCFX = 6m;
			quote.TH_SeaCFX = 7m;
			quote.TH_ExportSeaCFX = 8m;

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.QuotePopulatedWithEmptyFields.xml");
			return new BusinessObjectAndExpectedOutputFileName(quote, expectedOutputFilename, ValidationKind.None, "Empty Client Rate");
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"OrganisationDetails",
					"Owner/OrganisationDetails",
					"Owner/Notes/CustomNoteTypeName",
					"Owner/Notes/NoteData",
					"Owner/Notes/NoteCreatedDateTime",
					"CartageDeliveryAddress/Organisation",
					"RateEntries/ServiceProvider",
					"RateEntries/RateLines/TACT",
					"RateEntries/CartagePickupAddress/Organisation",
					"RateEntries/CartageDeliveryAddress/Organisation",
					"RateEntries/Consignee",
					"RateEntries/Consignor",
					"RateEntries/Units",
					"CompanyTariff",
					"RateEntries/RateLines/CompanyTariffLevel",
					"RateEntries/RateLines/Notes/CustomNoteTypeName",
					"RateEntries/RateLines/Notes/NoteCreatedDateTime",
					"RateEntries/TransportProvider",
					"OwnerPK",
				};
			}
		}

		#endregion

		protected override void TestImportCore()
		{
			Xsd.Rate rateXSD = GetRateXSD(true, true);
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);

			int numOfQuotation = Factory.GetDatabaseCount(typeof(Quote));

			AssertEquals("no. of costing", 0, numOfQuotation);

			Quote header = DataAdapter.CreateOrUpdateFromValueObject(rateXSD, context);

			Factory.Save();
			numOfQuotation = Factory.GetDatabaseCount(typeof(Quote));

			AssertEquals("Notificaiton has no error", true, !buffer.HasErrors);
			AssertNotNull("header is not null", header);

			AssertEquals("header 's TH_GC", Env.CurrentCompany.PK, header.TH_GC);
			AssertEquals("header 's owner", header.TH_OH, TestHelper.Consignee.PK);
			AssertEquals("no. of Quotation", 1, numOfQuotation);
			AssertEquals("header 's CFX air Export", 2m, header.TH_ExportAirCFX);
			AssertEquals("header 's CFX sea Export", 3m, header.TH_ExportSeaCFX);
			AssertEquals("header 's CFX air Import", 4m, header.TH_AirCFX);
			AssertEquals("header 's CFX sea Import", 5m, header.TH_SeaCFX);
			AssertEquals("Start Date", new ZDateTime(2007, 1, 1), header.TH_QuoteDate);
			AssertEquals("End Date", new ZDateTime(2007, 1, 30), header.TH_QuoteEndDate);
		}

		public void TestImportWithError()
		{
			Xsd.Rate rateXSD = GetRateXSD(false, true);
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);

			int numOfQuotation = Factory.GetDatabaseCount(typeof(Quote));

			AssertEquals("no. of Quotation", 0, numOfQuotation);

			Quote header = DataAdapter.CreateOrUpdateFromValueObject(rateXSD, context);

			numOfQuotation = Factory.GetDatabaseCount(typeof(Quote));
			AssertEquals("buffer has error", true, buffer.HasErrors);

			ZString expectedMesg = "Error: Error Importing Quote as Client Info is not provided.";
			AssertEquals("buffer should have this error message", true, TestHelper.AssertContainsErrorMesg(expectedMesg, buffer));
			AssertEquals("no. of Quotation", 0, numOfQuotation);

			rateXSD = GetRateXSD(true, false);
			header = DataAdapter.CreateOrUpdateFromValueObject(rateXSD, context);

			numOfQuotation = Factory.GetDatabaseCount(typeof(Quote));
			AssertEquals("buffer has error", true, buffer.HasErrors);

			expectedMesg = "Error: Quotation Start Date is not Provided";
			AssertEquals("buffer should have this error message", true, TestHelper.AssertContainsErrorMesg(expectedMesg, buffer));
			AssertEquals("no. of Quotation", 0, numOfQuotation);
		}

		protected override void TestExpectedRateTypeCore()
		{
			AssertEquals("Rate Type", ExpectedRateType, DataAdapter.ExpectedRateType);
		}

		protected override void TestShouldCheckOwnerIsSpecifiedCore()
		{
			AssertEquals("Should Check Owner is Specified", RateShouldCheckOwnerIsSpecified, DataAdapter.ShouldCheckifOwnerIsSpecified);
		}

		protected override ZString ExpectedRateType
		{
			get { return RatingConstants.RatingHeaderTypes.Quote; }
		}

		protected override ZString ExpectedInvalidModuleMesg
		{
			get { return "Error: Cannot Import Costing in Quotation Module"; }
		}

		protected override bool RateShouldCheckOwnerIsSpecified
		{
			get { return true; }
		}

		protected override RatingValueObjectDataAdapter<Quote> GetDataAdapter
		{
			get { return new QuotationValueObjectDataAdapter(); }
		}

		protected override Type GetDataAdapterType()
		{
			{ return typeof(QuotationValueObjectDataAdapter); }
		}

		protected override ZString OtherRateTypeForErrorTesitng
		{
			get { return RatingConstants.RatingHeaderTypes.Costing; }
		}

		Xsd.Rate GetRateXSD(bool specifiedOwner, bool specifiedQuote)
		{
			Xsd.Rate rate = new Xsd.Rate();
			rate.RateType = RatingConstants.RatingHeaderTypes.Quote;
			if (specifiedOwner)
			{
				rate.Owner = TestHelper.ConsigneeXSD;
			}

			if (specifiedQuote)
			{
				rate.Quote.StartDate = new ZDate(2007, 1, 1);
				rate.Quote.EndDate = new ZDate(2007, 1, 30);
			}
			rate.CFX.ExportAir = 2m;
			rate.CFX.ExportSea = 3m;
			rate.CFX.ImportAir = 4m;
			rate.CFX.ImportSea = 5m;

			return rate;
		}

		class QuotationValueObjectDataAdapterForTest : QuotationValueObjectDataAdapter
		{
			public new ZString ExpectedRateType
			{
				get { return base.ExpectedRateType; }
			}

			public new bool ShouldCheckifOwnerIsSpecified
			{
				get { return base.ShouldCheckifOwnerIsSpecified; }
			}
		}

		/// <summary>
		/// Quote are always imported as new objects.
		/// So this test is not needed.
		/// Note, its not obvious from the code that you always get new Quotes, but TH_OH is always null for quotes
		/// while the query for existing BizObjs filters on the Org so it never returns any results.
		/// </summary>
		protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			Assert(true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DataAdapter = new QuotationValueObjectDataAdapterForTest();
		}

		QuotationValueObjectDataAdapterForTest DataAdapter;
	}
}
