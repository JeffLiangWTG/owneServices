using System;
using CargoWise.ComponentModel;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Testing
{
	[TestedType(typeof(RateLineValueObjectDataAdapter))]
	sealed class RateLineValueObjectDataAdapterTest : ValueObjectDataAdapterTest<RateLine, Xsd.RateLine>
	{
		#region Export

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			var rateLine = rateEntry.RateLines.AddNew();

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RateLine.Testing.RateLinePopulatedWithEmptyFields.xml");
			return new BusinessObjectAndExpectedOutputFileName(rateLine, expectedOutputFilename, ValidationKind.None, "Rate Line With Empty Fields");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
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
			rateLine.TL_ContainerOwnership = Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned;
			rateLine.TL_RX_NKCurrency = "EUR";
			rateLine.UseOnlyActualWeightMeasure = false;
			rateLine.TL_ActualPercentage = 50;
			rateLine.TL_Rounding = "NOR";
			rateLine.TL_WeightVolumeMultiple = 2.5m;
			rateLine.TL_Condition = RateLineConditions.UserDefined;
			rateLine.TL_ConditionalExpression = "MOD=FSA";
			rateLine.TL_RateCalculator = CombinedCalculator.Code;
			rateLine.TL_WeightVolume = "CN";

			CombinedCalculator calculator = (CombinedCalculator)rateLine.Calculator;

			calculator.RateLineItems.RemoveAndDeleteAll();

			calculator.Minimum = 1.0000m;
			calculator.IsAccumulated = false;
			calculator.UseHigherChargeableLowerRateRule = false;
			calculator.UseInclusiveBreaks = false;

			rateLine.ChargeInformationNoteText = "Information note";
			rateLine.ChargeInternalNoteText = "Internal note";

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

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RateLine.Testing.RateLineFullyPopulated.xml");
			return new BusinessObjectAndExpectedOutputFileName(rateLine, expectedOutputFilename, ValidationKind.None, "Rate Line");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var rateLine = rateEntry.RateLines.AddNew();

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.Rating.DataTransfer.Test.DataAdapter.RateLine.Testing.RateLinePopulatedWithEmptyFields.xml");
			return new BusinessObjectAndExpectedOutputFileName(rateLine, expectedOutputFilename, ValidationKind.None, "Empty Rate Line");
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"TACT",
					"CompanyTariffLevel",
					"Notes/CustomNoteTypeName",
					"Notes/NoteCreatedDateTime",
				};
			}
		}

		#endregion

		#region Import

		public void TestImportFromValueObjectCore()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);

			Xsd.RateLine rateLineXSD = GetRateLineXSD(CartageChargeCode.AC_Code);
			RateLineValueObjectDataAdapter dataAdapter = new RateLineValueObjectDataAdapter(rateEntry);
			rateEntry.RateLines.RemoveAndDeleteAll();

			AssertEquals("rateEntry has no ratelines", 0, rateEntry.RateLines.Count);

			RateLine rateLine = dataAdapter.CreateOrUpdateFromValueObject(rateLineXSD, context);
			AssertNotNull("Rate line shouldn't be null", rateLine);
			AssertEquals("rateEntry has 1 rateline", 1, rateEntry.RateLines.Count);
			AssertEquals("notification has no errors", true, !buffer.HasErrors);
			AssertRateLineDetails(rateLine, rateLineXSD);

			RateLine anotherRateLine = dataAdapter.CreateOrUpdateFromValueObject(rateLineXSD, context);
			AssertNotNull("Rate line shouldn't be null", rateLine);
			AssertEquals("rateEntry has 1 rateline", 1, rateEntry.RateLines.Count);
			AssertEquals("notification has no errors", true, !buffer.HasErrors);

			AssertEquals("Existing Rate Line found", rateLine.PK, anotherRateLine.PK);

			rateEntry.AddRateLine(CartageChargeCode.AC_Code);

			AssertEquals("rateEntry has 2 rateline", 2, rateEntry.RateLines.Count);

			anotherRateLine = dataAdapter.CreateOrUpdateFromValueObject(rateLineXSD, context);
			AssertEquals("a new rate line is created as there are 2 existing rateline with same charge code", true, anotherRateLine.PK != rateLine.PK);
		}

		public void TestImportFromValueObjectCoreWithError()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);

			Xsd.RateLine rateLineXSD = GetRateLineXSD("ABC");
			RateLineValueObjectDataAdapter dataAdapter = new RateLineValueObjectDataAdapter(rateEntry);

			rateEntry.RateLines.RemoveAndDeleteAll();
			AssertEquals("rateEntry has no ratelines", 0, rateEntry.RateLines.Count);

			RateLine rateLine = dataAdapter.CreateOrUpdateFromValueObject(rateLineXSD, context);

			AssertEquals("notification has errors", true, buffer.HasErrors);

			ZString expectedErrorMesg = "Error: Invalid Charge Code (ABC)";
			Assert("expected error message ", buffer.Events.ContainsNotificationContaining(expectedErrorMesg));
		}

		public void TestImportWithCalculatorNotRequireWeightVolume()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);

			Xsd.FLTCalculator calculator = new Xsd.FLTCalculator();
			calculator.BasePrice = 123m;
			Xsd.RateLine rateLineXSD = GetRateLineXSD(CartageChargeCode.AC_Code);
			rateLineXSD.RateCalculator.Item = calculator;
			RateLineValueObjectDataAdapter dataAdapter = new RateLineValueObjectDataAdapter(rateEntry);

			rateEntry.RateLines.RemoveAndDeleteAll();
			AssertEquals("rateEntry has no ratelines", 0, rateEntry.RateLines.Count);

			RateLine rateLine = dataAdapter.CreateOrUpdateFromValueObject(rateLineXSD, context);
			AssertEquals("Weight Volume not require for Flat Rate Calculator", "", rateLine.TL_WeightVolume);
			AssertEquals("Weight Volume Multiple not require for Flat Rate Calculator", 0m, rateLine.TL_WeightVolumeMultiple);
		}

		public void TestImportWithCalculatorNotRequireWeightVolumeButDefaultWeightVolume()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);

			Xsd.WPKCalculator calculator = new Xsd.WPKCalculator();
			Xsd.RateItemWithDescAmtOrRate item = calculator.RateItems.AddNew();
			item.Code = "BOX";
			item.Rate = 14m;
			Xsd.RateLine rateLineXSD = GetRateLineXSD(CartageChargeCode.AC_Code);
			rateLineXSD.RateCalculator.Item = calculator;
			rateLineXSD.Units = "UNT";
			RateLineValueObjectDataAdapter dataAdapter = new RateLineValueObjectDataAdapter(rateEntry);

			rateEntry.RateLines.RemoveAndDeleteAll();
			AssertEquals("rateEntry has no ratelines", 0, rateEntry.RateLines.Count);

			RateLine rateLine = dataAdapter.CreateOrUpdateFromValueObject(rateLineXSD, context);
			AssertEquals("Weight Volume should be imported for Warehouse Package Calculator", "UNT", rateLine.TL_WeightVolume);
		}

		void AssertRateLineDetails(RateLine rateLine, Xsd.RateLine rateLineXSD)
		{
			AssertEquals("rate line 's parent", rateLine.TL_TI, rateEntry.PK);
			AssertEquals("rate line 's charge", CartageChargeCode.PK, rateLine.TL_AC);
			AssertEquals("rate line 's unit", "CN", rateLine.TL_WeightVolume);
			AssertEquals("rate line 's container ownership", Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned, rateLine.TL_ContainerOwnership);
			AssertEquals("rate line 's currency", rateLineXSD.Currency, rateLine.TL_RX_NKCurrency);
			AssertEquals("rate line 's unitmultiple", 2.5m, rateLine.TL_WeightVolumeMultiple);
			AssertEquals("rate line 's calculator", "UNT", rateLine.TL_RateCalculator);
			AssertEquals("rate line 's ChargeInformationNoteText", "Information note", rateLine.ChargeInformationNoteText);
			AssertEquals(RateLineConditions.UserDefined, rateLine.TL_Condition);
			AssertEquals("MOD=FSA", rateLine.TL_ConditionalExpression);
		}

		public void TestMappingsAppliedCorrectly()
		{
			var proxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			Testing.RateTestHelper.MapChargeCode(Env.Registry.FreightChargeCode, "YYY", null, Factory);
			Testing.RateTestHelper.MapCurrency("NZD", "NZP", proxy, Factory);

			var calculator = new Xsd.FLTCalculator { BasePrice = 123m };
			Xsd.RateLine rateLineXSD = GetRateLineXSD("YYY");
			rateLineXSD.Currency = "NZP";
			rateLineXSD.RateCalculator.Item = calculator;
			RateLineValueObjectDataAdapter dataAdapter = new RateLineValueObjectDataAdapter(rateEntry);

			rateEntry.RateLines.RemoveAndDeleteAll();
			AssertEquals("rateEntry must have no rateline", 0, rateEntry.RateLines.Count);

			RateLine rateLine = dataAdapter.CreateOrUpdateFromValueObject(rateLineXSD, Testing.RateTestHelper.GetContext(Factory));
			AssertEquals(Env.Registry.FreightChargeCode, rateLine.TL_AC);
			AssertEquals("NZD", rateLine.TL_RX_NKCurrency);
		}

		#endregion

		#region Override

		protected override string ExpectedRootCollectionElementName
		{
			get { return "RateLines"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "RateLine"; }
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
			return typeof(RateLineValueObjectDataAdapter);
		}

		protected override ValueObjectDataAdapter<RateLine, Xsd.RateLine> GetNewBizObjXmlDataAdapter()
		{
			return new RateLineValueObjectDataAdapter(rateEntry);
		}

		protected override RateLine NewBusinessObjectFromIValueObject(IValueObject value)
		{
			var rateEntry = Factory.NewWithValidTestData<RateEntry>();
			return rateEntry.RateLines.AddNew();
		}

		#endregion

		#region Implementation

		Xsd.RateLine GetRateLineXSD(ZString chargeCode)
		{
			Xsd.RateLine line = new Xsd.RateLine();
			line.ChargeCode = chargeCode;
			line.Currency = Core.Constants.CurrencyCodes.Australia;
			line.Units = "CN";
			line.ContainerOwnership = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			line.UnitsMultiple = 2.5m;
			line.Condition.Type = RateLineConditions.UserDefined;
			line.Condition.Value = "MOD=FSA";
			Xsd.UNTCalculator calculator = new Xsd.UNTCalculator();
			calculator.PerUnitPrice = 123m;

			line.RateCalculator.Item = calculator;

			var notes = line.Notes.AddNew();
			notes.NoteType = Xsd.NotesNoteNoteType.TradeLaneChargeInformation;
			notes.NoteData = "Information note";

			return line;
		}

		protected override void SetUp()
		{
			base.SetUp();

			rateEntry = Factory.New<Costing>().AddRateEntry("FCL");
			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-6);
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

		AccChargeCode cartageChargeCode;
		AccChargeCode CartageChargeCode
		{
			get { return cartageChargeCode ?? (cartageChargeCode = new TestHelper(Factory).ChargeCodes.New("CTG", "Cartage", "")); }
		}

		RateEntry rateEntry;

		#endregion
	}
}
