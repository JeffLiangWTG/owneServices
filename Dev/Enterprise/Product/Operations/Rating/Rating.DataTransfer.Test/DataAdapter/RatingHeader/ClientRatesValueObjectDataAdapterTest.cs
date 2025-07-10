using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Testing
{
	[TestedType(typeof(ClientRatesValueObjectDataAdapter))]
	sealed class ClientRatesValueObjectDataAdapterTest : RatingValueObjectDataAdapterTest<ClientRate>
	{
		#region Export

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			var clientRate = Factory.New<ClientRate>();

			clientRate.TH_GC = GlbCompany.CurrentCompany.PK;
			clientRate.TH_RateType = RatingConstants.RatingHeaderTypes.ClientRate;

			clientRate.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "JAYSCH")).PK;

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.ClientRatePopulatedWithEmptyFields.xml");
			return new BusinessObjectAndExpectedOutputFileName(clientRate, expectedOutputFilename, ValidationKind.None, "Client Rate With Empty Fields");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var clientRate = Factory.New<ClientRate>();

			clientRate.TH_GC = GlbCompany.CurrentCompany.PK;
			clientRate.TH_RateType = RatingConstants.RatingHeaderTypes.ClientRate;

			clientRate.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AUSJOH")).PK;

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

			Factory.Save();

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.ClientRateFullyPopulated.xml");
			return new BusinessObjectAndExpectedOutputFileName(clientRate, expectedOutputFilename, ValidationKind.None, "Client Rate");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var clientRate = Factory.New<ClientRate>();

			clientRate.TH_GC = GlbCompany.CurrentCompany.PK;
			clientRate.TH_RateType = RatingConstants.RatingHeaderTypes.ClientRate;

			clientRate.TH_OH = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "NATMUT")).PK;

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RatingHeader.Testing.ClientRateEmptySample.xml");
			return new BusinessObjectAndExpectedOutputFileName(clientRate, expectedOutputFilename, ValidationKind.None, "Empty Client Rate");
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
					"Quote",
					"CFX",
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
			Xsd.Rate rateXSD = GetRateXSD(true);
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);

			int numOfClientRate = Factory.GetDatabaseCount(typeof(ClientRate));

			AssertEquals("no. of client rate", 0, numOfClientRate);

			ClientRate header = DataAdapter.CreateOrUpdateFromValueObject(rateXSD, context);

			Factory.Save();
			numOfClientRate = Factory.GetDatabaseCount(typeof(ClientRate));

			AssertEquals("Notificaiton has no error", true, !buffer.HasErrors);
			AssertNotNull("header is not null", header);

			AssertEquals("header 's TH_GC", Env.CurrentCompany.PK, header.TH_GC);
			AssertEquals("header 's owner", header.TH_OH, TestHelper.Consignee.PK);
			AssertEquals("no. of client rate", 1, numOfClientRate);

			ClientRate anotherRate = DataAdapter.CreateOrUpdateFromValueObject(rateXSD, context);
			AssertEquals("no. of client rate", 1, numOfClientRate);
			AssertEquals("Notificaiton has no error", true, !buffer.HasErrors);
			AssertNotNull("header is not null", header);
			AssertEquals("rate is updated", anotherRate.PK, header.PK);
		}

		public void TestImportWithError()
		{
			Xsd.Rate rateXSD = GetRateXSD(false);
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);

			int numOfClientRate = Factory.GetDatabaseCount(typeof(ClientRate));

			AssertEquals("no. of client rate", 0, numOfClientRate);

			ClientRate header = DataAdapter.CreateOrUpdateFromValueObject(rateXSD, context);

			numOfClientRate = Factory.GetDatabaseCount(typeof(ClientRate));
			AssertEquals("buffer has error", true, buffer.HasErrors);

			ZString expectedMesg = "Error: Error Importing ClientRate as Client Info is not provided.";
			AssertEquals("buffer should have this error message", true, TestHelper.AssertContainsErrorMesg(expectedMesg, buffer));
			AssertEquals("no. of global rate", 0, numOfClientRate);
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
			get { return RatingConstants.RatingHeaderTypes.ClientRate; }
		}

		protected override ZString ExpectedInvalidModuleMesg
		{
			get { return "Error: Cannot Import Quotation in Client Rate Module"; }
		}

		protected override bool RateShouldCheckOwnerIsSpecified
		{
			get { return true; }
		}

		protected override RatingValueObjectDataAdapter<ClientRate> GetDataAdapter
		{
			get { return new ClientRatesValueObjectDataAdapter(); }
		}

		protected override Type GetDataAdapterType()
		{
			{ return typeof(ClientRatesValueObjectDataAdapter); }
		}

		protected override ZString OtherRateTypeForErrorTesitng
		{
			get { return RatingConstants.RatingHeaderTypes.Quote; }
		}

		Xsd.Rate GetRateXSD(bool specifiedOwner)
		{
			Xsd.Rate rate = new Xsd.Rate();
			rate.RateType = RatingConstants.RatingHeaderTypes.ClientRate;
			if (specifiedOwner)
			{
				rate.Owner = TestHelper.ConsigneeXSD;
			}

			return rate;
		}

		class ClientRatesValueObjectDataAdapterForTest : ClientRatesValueObjectDataAdapter
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
			DataAdapter = new ClientRatesValueObjectDataAdapterForTest();
		}

		ClientRatesValueObjectDataAdapterForTest DataAdapter;
	}
}
