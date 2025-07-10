using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Testing
{
	sealed class JobDeclarationEventParentFinderTest : TestCaseWithFactory
	{
		public void TestUpdateEntryNumberWhenEmpty()
		{
			var testHelper = new Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = "B00001100";
			declaration.JE_HouseBill = "TESTHOUSE";
			Factory.Save();
			var testEventXml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<EventTime>2015-04-16T08:12:21.637</EventTime>
		<EventType>{0}</EventType>
		<EventReference>1</EventReference>

		<ContextCollection>
			<Context>
				<Type>EntryNumber</Type>
				<Value>123456</Value>
			</Context>
			<Context>
				<Type>DeclarationReference</Type>
				<Value>B00001100</Value>
			</Context>
			<Context>
				<Type>EntryNumberType</Type>
				<Value>IMP</Value>
			</Context>
			<Context>
				<Type>EntryNumberCountryOfIssue</Type>
				<Value>ZA</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinderWithLogger(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(CultureInfo.InvariantCulture, testEventXml, Events.WarehouseJobCanNowBeFinalised.Code));
			AssertNull(subscriber.GetLogParentsForEvent(xmlEvent));
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			Factory.Save();
			logger = new TestErrorLogger();
			subscriber = GetNewEventParentFinderWithLogger(logger);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			var entry = (CusEntryHeader)logParents.First();
			AssertEquals("123456", entry.EntryNumber);
		}

		JobDeclarationEventParentFinder GetNewEventParentFinderWithLogger(IXmlImportLogger logger)
		{
			return new JobDeclarationEventParentFinder(Factory, new JobDeclarationDataContextManager(), logger);
		}
	}
}
