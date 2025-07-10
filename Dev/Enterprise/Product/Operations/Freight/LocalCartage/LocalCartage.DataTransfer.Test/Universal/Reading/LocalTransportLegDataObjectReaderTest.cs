using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Testing.Core;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal.Testing
{
	public class LocalTransportLegDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestSetCustomValue()
		{
			var year = ZDateTime.Now.Year;
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "LTL";
			processTaskTemplate.P0_IsActive = true;
			AddCustomColumn(processTaskTemplate, "Textual context", "STR");
			AddCustomColumn(processTaskTemplate, "First Date", "DAT");
			AddCustomColumn(processTaskTemplate, "Deci Deca", "DEC");
			AddCustomColumn(processTaskTemplate, "Flagger", "BOO");
			AddCustomColumn(processTaskTemplate, "Flag This!", "BOO");
			AddCustomColumn(processTaskTemplate, "Integer Mate", "INT");
			AddCustomColumn(processTaskTemplate, "Integraler", "INT");
			Factory.SaveForTesting();
			var legDataObject = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			legDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			legDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Textual context", new ZString("HELLO")));
			legDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Customs are customary", new ZString("GOODBYE")));
			legDataObject.CustomizedFieldCollection.Add(CustomizedField.New("First Date", new ZDateTime(year, 1, 1)));
			legDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Last Date", new ZDateTime(year, 1, 2)));
			legDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Deci Deca", new ZDecimal(0.3)));
			legDataObject.CustomizedFieldCollection.Add(CustomizedField.New("+ 1 point zero", new ZDecimal(1.3)));
			legDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Flagger", ZBool.True));
			var incorrectCustomField = new CustomizedField { Key = "Flag This!", Value = new ZString("I am NOT a BOOLEAN!"), DataType = DataType.Boolean };
			legDataObject.CustomizedFieldCollection.Add(incorrectCustomField);
			legDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integer Mate", new ZInt(42)));
			legDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Integraler", ZInt.Zero));
			legDataObject.CustomizedFieldCollection.Add(CustomizedField.New("Bogus Custom Field", new ZString("I am BOGUS")));
			var localTransport = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLUnpackLooseToCNE, 1);
			var container = localTransport.Containers.First();
			var leg = localTransport.GetBookedMoves(localTransport.Containers.First())[0].CartageLegs[0];
			var reader = new LocalTransportLegDataObjectReader(legDataObject, Logger, Factory, leg);
			var legBO = reader.ReadIntoBusinessObject();
			CombineAssertions(delegate
			{
				var customFields = legBO.GetUserDefinedValues();
				var customFieldsString = customFields.Select(f => f.PropertyName + " - " + f.Value).ToList();
				Assert("Custom Field 1 not found", customFieldsString.Contains("Deci Deca - 0.3"));
				Assert("Custom Field 2 not found", customFieldsString.Contains("First Date - " + new ZDateTime(year, 1, 1).ToString()));
				Assert("Custom Field 3 not found", customFieldsString.Contains("Flagger - Y"));
				Assert("Custom Field 4 not found", customFieldsString.Contains("Integer Mate - 42"));
				Assert("Custom Field 5 not found", customFieldsString.Contains("Textual context - HELLO"));
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching CommonCartageLeg.
Information - Populating CommonCartageLeg...
Warning - Custom Fields - Invalid value [I am NOT a BOOLEAN!]. Value must be a valid Boolean (true or false).
			".Trim(), logger.Logs);
			});
		}

		public void TestSetCustomValueSafely_WhenValueExceedTheMaximumLength()
		{
			const string testColumnName = "Textual context";
			const string testColumnValue = "HELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLOHELLO";
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "LTL";
			processTaskTemplate.P0_IsActive = true;
			AddCustomColumn(processTaskTemplate, testColumnName, "STR");
			Factory.SaveForTesting();
			var legDataObject = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			legDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField> { CustomizedField.New(testColumnName, new ZString(testColumnValue)), });
			var localTransport = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLUnpackLooseToCNE, 1);
			var leg = localTransport.GetBookedMoves(localTransport.Containers.First())[0].CartageLegs[0];
			var reader = new LocalTransportLegDataObjectReader(legDataObject, Logger, Factory, leg);
			reader.ReadIntoBusinessObject();
			var waring = ZString.Format("Attempted to insert {0} characters into Field [{1}] which has a maximum length of {2} characters. Field was truncated.", testColumnValue.Length, testColumnName, AutoGenCustomAddOnValue.Schema.XV_DataMaxLength);
			Assert(Logger.GetWarnings().Contains(waring));
		}

		void AddCustomColumn(ProcessTaskTemplate processTaskTemplate, string name, string type)
		{
			var customField = Factory.New<GenCustomColumnDefinition>();
			customField.XC_Name = name;
			customField.XC_Type = type;
			processTaskTemplate.GenCustomColumnDefinitions.Add(customField);
		}

		TestErrorLogger Logger
		{
			get
			{
				return logger ?? (logger = new TestErrorLogger());
			}
		}

		TestErrorLogger logger;

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory.BOFactory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
