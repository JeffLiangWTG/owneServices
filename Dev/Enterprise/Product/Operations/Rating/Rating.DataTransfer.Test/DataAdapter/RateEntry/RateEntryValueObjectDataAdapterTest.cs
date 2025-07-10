using System;
using System.IO;
using System.Xml;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Testing
{
	[TestedType(typeof(RateEntryValueObjectDataAdapter))]
	sealed class RateEntryValueObjectDataAdapterTest : ValueObjectDataAdapterTest<RateEntry, Xsd.RateEntry>
	{
		protected override IValueObject PopulateValueObject(Type valueType, int fieldPopulateDepth)
		{
			var rateEntryXSD = (Xsd.RateEntry)base.PopulateValueObject(valueType, fieldPopulateDepth);
			rateEntryXSD.Category = "AIR";
			rateEntryXSD.CarrierServiceLevel = "ART";
			return rateEntryXSD;
		}

		#region Export

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR);
			rateEntry.TI_RateStartDate = new ZDate("2007-07-25");
			rateEntry.TI_RateEndDate = new ZDate("2008-01-25");
			rateEntry.TI_PL_NKCarrierServiceLevel = "ART";
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RateEntry.Testing.RateEntryPopulatedWithEmptyFields.xml");
			return new BusinessObjectAndExpectedOutputFileName(rateEntry, expectedOutputFilename, ValidationKind.None, "Rate Entry With Empty Fields");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "ADALV", "CAAAL");

			rateEntry.TI_RateCategory = "AIR";
			rateEntry.TI_RateStartDate = new ZDate("2007-07-17");
			rateEntry.TI_RateEndDate = new ZDate("2008-01-17");
			rateEntry.TI_RX_NKCurrency = "EUR";
			rateEntry.TI_IsCrossTrade = false;
			rateEntry.TI_MatchContainerRateClass = false;
			rateEntry.TI_CartagePickupAddressPostCode = "2127";
			rateEntry.TI_CartageDeliveryAddressPostCode = "2721";
			rateEntry.TI_RH_NKCommodityCode = "GEN";
			rateEntry.TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
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

			var calculator = (CombinedCalculator)rateLine.Calculator;

			calculator.RateLineItems.RemoveAndDeleteAll();

			calculator.Minimum = 1.0000m;
			calculator.IsAccumulated = false;
			calculator.UseHigherChargeableLowerRateRule = false;
			calculator.UseInclusiveBreaks = false;

			RateLineItem rateItem = calculator.RateLineItems.AddNew();
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

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RateEntry.Testing.RateEntryFullyPopulated.xml");
			return new BusinessObjectAndExpectedOutputFileName(rateEntry, expectedOutputFilename, ValidationKind.None, "Rate Entry");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var clientRate = Factory.New<ClientRate>();
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR);
			rateEntry.TI_RateStartDate = new ZDate("2007-07-25");
			rateEntry.TI_RateEndDate = new ZDate("2008-01-25");
			rateEntry.TI_PL_NKCarrierServiceLevel = "ART";

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RateEntry.Testing.RateEntryPopulatedWithEmptyFields.xml");
			return new BusinessObjectAndExpectedOutputFileName(rateEntry, expectedOutputFilename, ValidationKind.None, "Empty Rate Entry");
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"CartageDeliveryAddress/Organisation",
					"ServiceProvider/OwnerCode",
					"ServiceProvider/OrganisationDetails",
					"ServiceProvider/EDICode",
					"ServiceProvider/Notes/CustomNoteTypeName",
					"ServiceProvider/Notes/NoteData",
					"ServiceProvider/Notes/NoteCreatedDateTime",
					"RateLines/TACT",
					"RateLines/CompanyTariffLevel",
					"CartagePickupAddress/Organisation",
					"Consignee",
					"Consignor",
					"Units",
					"TransportProvider",
					"RateLines/Notes/CustomNoteTypeName",
					"RateLines/Notes/NoteCreatedDateTime",
				};
			}
		}

		#endregion

		#region Import

		public void TestImportFromXmlFile_NoError()
		{
			TestHelper.Consignee.Factory.Save();
			var notification = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notification);

			var header = Factory.New<CompanyTariff>();
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RateEntry.Testing.FullyPopulatedRateEntry_NoError.xml");
			Xsd.RateEntry xmlRateEntry = GetValueObjectFromXmlFile(expectedOutputFilename);
			AssertNoRateEntryForRatingHeader(header);

			var dataAdapter = new RateEntryValueObjectDataAdapter(header);
			RateEntry rateEntry = dataAdapter.CreateOrUpdateFromValueObject(xmlRateEntry, context);

			Assert("Notification Buffer has no error", !notification.HasErrors);
			AssertNotNull("rateEntry should not be null", rateEntry);

			AssertEquals("No of Air Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).Count);
			AssertEquals("No of FCL Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.FCL).Count);
			AssertEquals("No of LCL Rate Entry", 1, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL).Count);
			AssertEquals("No of ORG Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Count);
			AssertEquals("No of DST Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.DST).Count);
			AssertEquals("No of PAC Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.PAC).Count);
			AssertEquals("No of TRN Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.TRN).Count);
			AssertEquals("No of TBC Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.TBC).Count);
			AssertEquals("No of UNP Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.UNP).Count);
			AssertEquals("No of WHS Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.WHS).Count);

			AssertRateEntryDetails(rateEntry, header);

			Factory.Save();

			RateEntry anotherRateEntry = dataAdapter.CreateOrUpdateFromValueObject(xmlRateEntry, context);
			Assert("Notification Buffer has no error", !notification.HasErrors);
			AssertNotNull("rateEntry should not be null", anotherRateEntry);
			AssertEquals("Existing Rate Entry Found, no new rate entry is created", rateEntry.PK, anotherRateEntry.PK);
		}

		void AssertRateEntryDetails(RateEntry rateEntry, RatingHeader header)
		{
			AssertEquals("rateEntry Parent", rateEntry.TI_TH, header.PK);
			AssertEquals("Category", "LCL", rateEntry.TI_RateCategory);
			AssertEquals("Mode", "LCL", rateEntry.TI_Mode);
			AssertEquals("Start Date", new ZDate(2007, 10, 20), rateEntry.TI_RateStartDate);
			AssertEquals("Currency", "NZD", rateEntry.TI_RX_NKCurrency);
			AssertEquals("Transit Time", "100", rateEntry.TI_TransitTime);
			AssertEquals("Frequency", 2, rateEntry.TI_Frequency);
			AssertEquals("Frequency Unit", "Fornight", rateEntry.TI_FrequencyUnit);
			AssertEquals("Consignor", Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "CALALB").PK, rateEntry.TI_OH_Consignor);
			AssertEquals("Consignee", TestHelper.Consignee.PK, rateEntry.TI_OH_Consignee);
			AssertEquals("Origin", "AUSYD", rateEntry.TI_OriginLRC);
			AssertEquals("Destination", "HKHKG", rateEntry.TI_DestinationLRC);
			AssertEquals("Cross Trade", false, rateEntry.TI_IsCrossTrade);
			AssertEquals("Commodity Code", "HAZ", rateEntry.TI_RH_NKCommodityCode);
		}

		public void TestImportFromXmlFile_WithError()
		{
			var notification = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notification);
			var header = Factory.New<Costing>();
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RateEntry.Testing.FullyPopulatedRateEntry_WithError.xml");
			Xsd.RateEntry xmlRateEntry = GetValueObjectFromXmlFile(expectedOutputFilename);

			var dataAdapter = new RateEntryValueObjectDataAdapter(header);
			dataAdapter.CreateOrUpdateFromValueObject(xmlRateEntry, context);
			Assert("Notification Buffer has errors", notification.HasErrors);

			ZString expectedErrorMesg = "Error:  (Pickup Address Does not Belong To Consignor)";
			AssertEquals("expected error message 1:", true, TestHelper.AssertContainsErrorMesg(expectedErrorMesg, notification));

			expectedErrorMesg = "Error: Please Fix the Address Error(s) before continuing";
			AssertEquals("expected error message 2:", true, TestHelper.AssertContainsErrorMesg(expectedErrorMesg, notification));
		}

		public void TestSetCurrencyWhenImportingFromXmlFile()
		{
			var notification = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notification);

			var header = Factory.New<Costing>();
			var xmlRateEntry = GetValueObjectFromXmlFile(RateEntryForTestingCurrencyPath);
			AssertNoRateEntryForRatingHeader(header);

			var dataAdapter = new RateEntryValueObjectDataAdapter(header);
			var rateEntry = dataAdapter.CreateOrUpdateFromValueObject(xmlRateEntry, context);

			AssertEquals("Category", "AIR", rateEntry.TI_RateCategory);
			AssertEquals("Origin", "ADALV", rateEntry.TI_OriginLRC);
			AssertEquals("Destination", "HKHKG", rateEntry.TI_DestinationLRC);
			AssertEquals("Currency", "NZD", rateEntry.TI_RX_NKCurrency);
		}

		public void TestMappingsAppliedCorrectly()
		{
			var proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			RateTestHelper.MapPort("AUSYD", "SYPOR", proxy, Factory);
			RateTestHelper.MapPort("NZAKL", "AKPOR", proxy, Factory);
			RateTestHelper.MapPort("USLAX", "LAPOR", proxy, Factory);
			RateTestHelper.MapCurrency("NZD", "NZP", proxy, Factory);
			RateTestHelper.MapServiceLevel("TSP", "TREAT", proxy, Factory);

			Factory.Save();

			var header = Factory.New<Costing>();
			var xmlRateEntry = GetValueObjectFromXmlFile(RateEntryForTestingCurrencyPath);
			AssertNoRateEntryForRatingHeader(header);

			xmlRateEntry.Origin = "SYPOR";
			xmlRateEntry.Destination = "AKPOR";
			xmlRateEntry.Via = "LAPOR";
			xmlRateEntry.Currency = "NZP";
			xmlRateEntry.ServiceLevel = "TREAT";

			var dataAdapter = new RateEntryValueObjectDataAdapter(header);
			var rateEntry = dataAdapter.CreateOrUpdateFromValueObject(xmlRateEntry, RateTestHelper.GetContext(Factory));

			AssertEquals("AUSYD", rateEntry.TI_OriginLRC);
			AssertEquals("NZAKL", rateEntry.TI_DestinationLRC);
			AssertEquals("USLAX", rateEntry.TI_ViaLRC);
			AssertEquals("NZD", rateEntry.TI_RX_NKCurrency);
			AssertEquals("TSP", rateEntry.TI_RS_NKServiceLevel_NI);
		}

		#endregion

		#region Implementation

		void AssertNoRateEntryForRatingHeader(RatingHeader header)
		{
			AssertEquals("No of Air Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.AIR).Count);
			AssertEquals("No of FCL Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.FCL).Count);
			AssertEquals("No of LCL Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.LCL).Count);
			AssertEquals("No of ORG Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Count);
			AssertEquals("No of DST Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.DST).Count);
			AssertEquals("No of PAC Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.PAC).Count);
			AssertEquals("No of TRN Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.TRN).Count);
			AssertEquals("No of TBC Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.TBC).Count);
			AssertEquals("No of UNP Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.UNP).Count);
			AssertEquals("No of WHS Rate Entry", 0, header.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.WHS).Count);
		}

		public void TestFindBusinessObject()
		{
			TestHelper.Consignee.Factory.Save();

			var header = Factory.New<Costing>();
			var entry1 = GetNewEntry(header, RatingConstants.RateCategory.LCL, Core.Constants.RateMode.FTL, Core.Constants.Weight.Pounds, "",
				TestHelper.Consignee.PK, "AUSYD", "HKHKG", new ZDate(2007, 01, 30), ZDate.Empty, 2, RatingConstants.FrequencyUnits.Daily);

			Factory.Save();

			Xsd.RateEntry entryXSD = GetEntryXSD("LCL", "FTL", "LB", "", TestHelper.ConsigneeXSD, "AUSYD",
				"HKHKG", new ZDate(2007, 02, 27), ZDate.Empty, 2, "Daily");

			var buffer = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, buffer);
			var dataAdapter = new RateEntryValueObjectDataAdapterForTest(header);

			var entry = dataAdapter.FindBusinessObject(entryXSD, context);
			AssertNotNull("entry is found", entry);
			AssertEquals("Entry from findbusinessobject", entry1.PK, entry.PK);

			entryXSD = GetEntryXSD("LCL", "FTL", "LB", "", TestHelper.ConsigneeXSD, "AUSYD",
				"HKHKG", new ZDate(2007, 02, 27), ZDate.Empty, 2, "Fornight");

			entry = dataAdapter.FindBusinessObject(entryXSD, context);
			AssertNull("entry is not found ", entry);

			entryXSD = GetEntryXSD("LCL", "FTL", "LB", "", TestHelper.ConsigneeXSD, "AUSYD",
				"NZAKL", new ZDate(2007, 02, 27), ZDate.Empty, 2, "Daily");

			entry = dataAdapter.FindBusinessObject(entryXSD, context);
			AssertNull("entry is not found ", entry);
		}

		RateEntry GetNewEntry(RatingHeader header, ZString category, ZString mode, ZString weightVol, ZString commodityCode,
			ZGuid consigneePK, ZString origin, ZString destination, ZDate startDate, ZDate endDate, int freq, ZString freqUnit)
		{
			var entry = header.AddRateEntry(category, mode, origin, destination);
			entry.Unit = weightVol;
			entry.TI_OH_Consignee = consigneePK;
			entry.TI_RateEndDate = endDate;
			entry.TI_RateStartDate = startDate;
			entry.TI_Frequency = freq;
			entry.TI_FrequencyUnit = freqUnit;
			entry.TI_RH_NKCommodityCode = commodityCode;

			return entry;
		}

		Xsd.RateEntry GetEntryXSD(ZString category, ZString mode, ZString weightVol, ZString commodityCode,
			Xsd.Organisation consignee, ZString origin, ZString destination, ZDate startDate, ZDate endDate, int freq, ZString freqUnit)
		{
			var entry = new Xsd.RateEntry();
			entry.Category = category;
			entry.Mode = mode;
			entry.Units = weightVol;
			entry.CommodityCode = commodityCode;
			if (consignee != null)
			{
				entry.Consignee = consignee;
			}
			entry.Origin = origin;
			entry.Destination = destination;
			entry.StartDate = startDate;
			entry.EndDate = endDate;
			entry.Frequency = freq;
			entry.FrequencyUnit = freqUnit;

			return entry;
		}

		#region Override

		protected override string ExpectedRootCollectionElementName
		{
			get { return "RateEntries"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "RateEntry"; }
		}

		protected override bool IsExportToCollectionSupported
		{
			get { return true; }
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return true; }
		}

		protected override Type GetDataAdapterType()
		{
			return typeof(RateEntryValueObjectDataAdapter);
		}

		protected override ValueObjectDataAdapter<RateEntry, Xsd.RateEntry> GetNewBizObjXmlDataAdapter()
		{
			return new RateEntryValueObjectDataAdapter(Factory.New<CompanyTariff>());
		}

		#endregion

		Xsd.RateEntry GetValueObjectFromXmlFile(ZString fileName)
		{
			var serializer = new XmlValueObjectSerializer(typeof(Xsd.RateEntry));

			var exampleDoc = new XmlDocument();

			using (var reader = new StreamReader(fileName))
			{
				exampleDoc.Load(reader);
			}

			var reader1 = new XmlNodeReader(exampleDoc);
			var xmlRateEntry = serializer.Deserialize(reader1) as Xsd.RateEntry;

			AssertNotNull("PreCondition: Value Object is not null", xmlRateEntry);

			return xmlRateEntry;
		}

		protected override RateEntry NewBusinessObject()
		{
			return Factory.New<CompanyTariff>().AddRateEntry("FCL");
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		string rateEntryForTestingCurrencyPath;
		string RateEntryForTestingCurrencyPath
		{
			get
			{
				if (string.IsNullOrEmpty(rateEntryForTestingCurrencyPath))
				{
					rateEntryForTestingCurrencyPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RateEntry.Testing.RateEntryForTestingCurrency.xml");
				}
				return rateEntryForTestingCurrencyPath;
			}
		}

		RatingXSDTestHelper testHelper;
		RatingXSDTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new RatingXSDTestHelper()); }
		}

		class RateEntryValueObjectDataAdapterForTest : RateEntryValueObjectDataAdapter
		{
			public RateEntryValueObjectDataAdapterForTest(RatingHeader ratingHeader)
				: base(ratingHeader)
			{
			}

			public new RateEntry FindBusinessObject(Xsd.RateEntry rateEntryXSD, IValueObjectImportContext context)
			{
				return base.FindBusinessObject(rateEntryXSD, context);
			}
		}

		#endregion
	}
}
