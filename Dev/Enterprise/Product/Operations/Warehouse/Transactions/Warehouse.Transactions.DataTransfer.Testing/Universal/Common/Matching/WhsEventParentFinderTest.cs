using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	abstract class WhsEventParentFinderTest<TEventParentFinder, TDocket> : WhsTestCaseWithFactory
		where TEventParentFinder : EventParentFinder
		where TDocket : WhsDocket
	{
		protected BusinessObject ProcessEventXML(string eventXmlText)
		{
			Factory.Save();
			var logParents = GetLogParents(eventXmlText);
			return logParents[0];
		}

		protected static void AssertLogParentIsCorrect(TDocket matchingOrder, BusinessObject logParent, string assertionMessage = "Should get best matching Docket")
		{
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(TDocket), logParent.GetType());
			AssertEquals(assertionMessage, GetHumanReadableID(matchingOrder), GetHumanReadableID(logParent));
		}

		protected void AssertLogParentIsNull(string eventXmlText, string assertionMessage = "")
		{
			Factory.Save();
			var logParents = GetLogParents(eventXmlText);
			AssertNull(assertionMessage, logParents);
		}

		BusinessObject[] GetLogParents(string eventXmlText)
		{
			var logger = new DummyLogger();
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			if (xmlEvent.DataContext == null)
			{
				xmlEvent.DataContext = DataContextFactory.New();
			}
			logger.TopLevelDataObject = xmlEvent;
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			return logParents;
		}

		protected static string GetHumanReadableID(BusinessObject businessObject) => $"{businessObject.HumanReadableName} - PK: {businessObject.PK}";

		protected abstract TEventParentFinder GetNewEventParentFinder(IXmlImportLogger logger);
	}
}
