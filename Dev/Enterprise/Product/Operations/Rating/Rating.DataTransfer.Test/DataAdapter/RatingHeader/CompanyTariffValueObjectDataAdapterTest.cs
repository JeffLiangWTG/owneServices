using System;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Testing
{
	[TestedType(typeof(CompanyTariffValueObjectDataAdapter))]
	sealed class CompanyTariffValueObjectDataAdapterTest : RatingValueObjectDataAdapterTest<CompanyTariff>
	{
		#region Export

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			var companyTariff = Factory.New<CompanyTariff>();

			companyTariff.TH_GC = GlbCompany.CurrentCompany.PK;
			companyTariff.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;

			companyTariff.TH_GlobalRateDescription = "test level";
			companyTariff.TH_GlobalRateLevel = 1;

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.CompanyTariffPopulatedWithEmptyFields.xml");
			return new BusinessObjectAndExpectedOutputFileName(companyTariff, expectedOutputFilename, ValidationKind.None, "Company Tariff With Empty Fields");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var companyTariff = Factory.New<CompanyTariff>();

			companyTariff.TH_GC = GlbCompany.CurrentCompany.PK;
			companyTariff.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;

			companyTariff.TH_GlobalRateDescription = "test level";
			companyTariff.TH_GlobalRateLevel = 2;

			var rateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "ADALV", "CAAAL");

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

			CombinedCalculator calculator = (CombinedCalculator)rateLine.Calculator;

			calculator.RateLineItems.RemoveAndDeleteAll();

			calculator.Minimum = 1.0000m;
			calculator.IsAccumulated = false;
			calculator.UseHigherChargeableLowerRateRule = false;
			calculator.UseInclusiveBreaks = false;

			var rateItem = calculator.RateLineItems.AddNew();
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

			Factory.Save();

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.CompanyTariffFullyPopulated.xml");
			return new BusinessObjectAndExpectedOutputFileName(companyTariff, expectedOutputFilename, ValidationKind.None, "Company Tariff");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var companyTariff = Factory.New<CompanyTariff>();

			companyTariff.TH_GC = GlbCompany.CurrentCompany.PK;
			companyTariff.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;

			companyTariff.TH_GlobalRateDescription = "test level";
			companyTariff.TH_GlobalRateLevel = 3;

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.CompanyTariffEmptySample.xml");
			return new BusinessObjectAndExpectedOutputFileName(companyTariff, expectedOutputFilename, ValidationKind.None, "Empty Company Tariff");
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"OrganisationDetails",
					"Owner/OrganisationDetails",
					"CartageDeliveryAddress/Organisation",
					"RateEntries/ServiceProvider",
					"RateEntries/RateLines/TACT",
					"RateEntries/CartagePickupAddress/Organisation",
					"RateEntries/CartageDeliveryAddress/Organisation",
					"RateEntries/Consignee",
					"RateEntries/Consignor",
					"RateEntries/Units",
					"Quote",
					"CFX",
					"Owner",
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
			Xsd.Rate rateXSD = GetRateXSD(1, false);
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);

			int numOfGlobalRate = Factory.GetDatabaseCount(typeof(CompanyTariff));

			AssertEquals("no. of global rate", 0, numOfGlobalRate);

			RatingHeader header = DataAdapter.CreateOrUpdateFromValueObject(rateXSD, context);

			Factory.Save();
			numOfGlobalRate = Factory.GetDatabaseCount(typeof(CompanyTariff));

			AssertEquals("Notificaiton has no error", true, !buffer.HasErrors);
			AssertNotNull("header is not null", header);

			AssertEquals("header 's company tariff level", (ZByte)1, header.TH_GlobalRateLevel);
			AssertEquals("header 's TH_GC", Env.CurrentCompany.PK, header.TH_GC);
			AssertEquals("header 's tariff description", "test level", header.TH_GlobalRateDescription);
			AssertEquals("no. of global rate", 1, numOfGlobalRate);
		}

		public void TestImportWithError()
		{
			Xsd.Rate rateXSD = GetRateXSD(2, false);
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);

			int numOfGlobalRate = Factory.GetDatabaseCount(typeof(CompanyTariff));

			AssertEquals("no. of global rate", 0, numOfGlobalRate);

			CompanyTariff header = DataAdapter.CreateOrUpdateFromValueObject(rateXSD, context);

			numOfGlobalRate = Factory.GetDatabaseCount(typeof(CompanyTariff));
			AssertEquals("buffer has error", true, buffer.HasErrors);

			ZString expectedMesg = "Error: Only Base Company Tariff(Level One) Import is Allowed.";
			AssertEquals("buffer should have this error message", true, TestHelper.AssertContainsErrorMesg(expectedMesg, buffer));
			AssertEquals("no. of global rate", 0, numOfGlobalRate);

			rateXSD = GetRateXSD(1, true);

			header = DataAdapter.CreateOrUpdateFromValueObject(rateXSD, context);

			numOfGlobalRate = Factory.GetDatabaseCount(typeof(CompanyTariff));
			AssertEquals("buffer has error", true, buffer.HasErrors);
			expectedMesg = "Error: Company Tariff Level is not Provided.";
			AssertEquals("buffer should have this error message", true, TestHelper.AssertContainsErrorMesg(expectedMesg, buffer));
			AssertEquals("no. of global rate", 0, numOfGlobalRate);
		}

		public void TestExportToValueObject_WithSelectedFilterCategory()
		{
			const string origin = "AUSYD";
			const string destination = "USLAX";
			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			tariff.SelectedFilterCategory = RatingConstants.RateCategory.AIR;
			tariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, origin, destination);
			var adapter = GetNewBizObjXmlDataAdapter();
			var exportedValueObject = (Xsd.Rate)Activator.CreateInstance(adapter.ValueObjectType);
			adapter.ExportToValueObject(tariff, exportedValueObject, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(exportedValueObject.RateEntries[0].Origin, origin);
			AssertEquals(exportedValueObject.RateEntries[0].Destination, destination);
			AssertEquals(exportedValueObject.RateEntries[0].Category, RatingConstants.RateCategory.AIR);
			AssertEquals(exportedValueObject.RateEntries[0].Mode, Core.Constants.RateMode.LSE);
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
			get { return RatingConstants.RatingHeaderTypes.Tariff; }
		}

		protected override ZString ExpectedInvalidModuleMesg
		{
			get { return "Error: Cannot Import Client Rate in Company Tariff Module"; }
		}

		protected override bool RateShouldCheckOwnerIsSpecified
		{
			get { return false; }
		}

		protected override RatingValueObjectDataAdapter<CompanyTariff> GetDataAdapter
		{
			get { return new CompanyTariffValueObjectDataAdapter(); }
		}

		protected override ZString OtherRateTypeForErrorTesitng
		{
			get { return RatingConstants.RatingHeaderTypes.ClientRate; }
		}

		protected override Type GetDataAdapterType()
		{
			{ return typeof(CompanyTariffValueObjectDataAdapter); }
		}

		Xsd.Rate GetRateXSD(int tariffLvl, bool skipCompanyTariff)
		{
			Xsd.Rate rate = new Xsd.Rate();
			rate.RateType = RatingConstants.RatingHeaderTypes.Tariff;

			if (!skipCompanyTariff)
			{
				rate.CompanyTariff.Description = "test level";
				rate.CompanyTariff.Level = tariffLvl;
			}
			return rate;
		}

		class CompanyTariffValueObjectDataAdapterForTest : CompanyTariffValueObjectDataAdapter
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
		/// Exporting a RateLine with a note generates a NoteCreatedDateTime element 
		/// if the RateLine has been saved, but not if it is unsaved.
		/// This removes that element to avoid there being any difference.
		/// </summary>
		protected override string TransformVolatilePartsBeforeComparing(string originalOutput)
		{
			return System.Text.RegularExpressions.Regex.Replace(originalOutput, " *<NoteCreatedDateTime>.*</NoteCreatedDateTime>\r\n", string.Empty);
		}

		/// <summary>
		/// Override the base class to reuse the sample BizObj as the BizObj to import to.
		/// If they are different then they cannot be saved due to unique constraint violations.
		/// The BizObj has to be saved since RateEntryValueObjectDataAdapter uses DB only queries.
		/// </summary>
		protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			if (IsImportFromValueObjectSupported && IsExportToValueObjectSupported)
			{
				IValueObjectDataAdapter adapter = GetNewBizObjXmlDataAdapter();
				Xsd.Rate exportedValueObject;
				if (sample.ConstructedValueObject != null)
				{
					exportedValueObject = sample.ConstructedValueObject;
				}
				else
				{
					exportedValueObject = (Xsd.Rate)Activator.CreateInstance(adapter.ValueObjectType);
				}

				using (StmALogValueObjectDataAdapter.SuspendExportingLogs())
				{
					adapter.ExportToValueObject(sample.BizObj, exportedValueObject, new ValueObjectExportContext(new NotificationBuffer()));
					var exportedValueObjectXml = WriteBusinessObjectToXml(sample.BizObj, sample.ConstructedValueObject, sample.Description, (sample.ValidationKind & ValidationKind.Xsd) != 0);

					var bizObjToImportTo = sample.BizObj;
					AssertImportFromThenExportToProducesSameXml(sample, bizObjToImportTo, exportedValueObject, exportedValueObjectXml, "From a new business object");
					AssertImportFromThenExportToProducesSameXml(sample, bizObjToImportTo, exportedValueObject, exportedValueObjectXml, "Updating an already populated business object to ensure that updating updates existing collection items and doesn't add them if they already exist");
					bizObjToImportTo.Delete();
				}
			}
			else
			{
				Assert(true);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			DataAdapter = new CompanyTariffValueObjectDataAdapterForTest();
		}

		CompanyTariffValueObjectDataAdapterForTest DataAdapter;
	}
}
