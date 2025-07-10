using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class CommonConsolEventParentFinderHelperTest : TestCase
	{
		public void TestConstructor()
		{
			var helper = new Mock<IUniversalFreightHelper>();
			var eventValueObject = new Mock<IXmlEventValueObject>();
			var context = new Mock<IXmlEventValueObjectContextValueList>();

			eventValueObject.Setup(m => m.Context).Returns(context.Object);
			context.Setup(m => m.MAWBNumber).Returns("MBO1234");
			context.Setup(m => m.CarriersBookingReference).Returns("BOKK");

			var consolEventParentFinder = new CommonConsolEventParentFinderHelper(eventValueObject.Object, helper.Object);
			AssertEquals("MBO1234", consolEventParentFinder.Masterbill);
			AssertEquals("BOKK", consolEventParentFinder.BookingReference);
		}

		public void TestMatchMainCarrierReferencesToCoLoader()
		{
			var helper = new Mock<IUniversalFreightHelper>();
			var logger = new TestErrorLogger();
			var universalEvent = new Event();

			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(XMLSourceWithWTGTracking)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(universalEvent, stream, logger);
				AssertNotNull(universalEvent);

				var consolReferences = new CommonConsolEventParentFinderHelper(universalEvent, helper.Object).GetConsolReferences();
				AssertNotNull(consolReferences);
				AssertEquals(true, consolReferences.MatchMainCarrierReferencesToCoLoader);
			}

			universalEvent = new Event();
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(XMLSourceWithoutWTGTracking)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(universalEvent, stream, logger);
				AssertNotNull(universalEvent);

				var consolReferences = new CommonConsolEventParentFinderHelper(universalEvent, helper.Object).GetConsolReferences();
				AssertNotNull(consolReferences);
				AssertEquals(false, consolReferences.MatchMainCarrierReferencesToCoLoader);
			}
		}

		const string XMLSourceWithoutWTGTracking = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
         <Event>
            <EventTime>2020-04-07T09:00</EventTime>
            <EventType>GOU</EventType>
            <EventParameters>
               <Facility>CTO</Facility>
               <Location>SGSIN</Location>
            </EventParameters>
            <EventReference />
            <IsEstimate>true</IsEstimate>
            <ContextCollection>
               <Context>
                  <Type>MBOLNumber</Type>
                  <Value>ONEYLIVA13099300</Value>
               </Context>
               <Context>
                  <Type>CarriersBookingReference</Type>
                  <Value>LIVA13099300</Value>
               </Context>
               <Context>
                  <Type>CarrierCode</Type>
                  <Value>ONEY</Value>
               </Context>
               <Context>
                  <Type>ContainerNumber</Type>
                  <Value>BMOU9774735</Value>
               </Context>
               <Context>
                  <Type>ContainerISOCode</Type>
                  <Value>45R1</Value>
               </Context>
               <Context>
                  <Type>MBOLOriginUNLOCO</Type>
                  <Value>GBSOU</Value>
               </Context>
               <Context>
                  <Type>MBOLDestinationUNLOCO</Type>
                  <Value>SGSIN</Value>
               </Context>
            </ContextCollection>
         </Event>
      </UniversalEvent>";

		const string XMLSourceWithWTGTracking = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
         <Event>
            <EventTime>2020-04-07T09:00</EventTime>
            <EventType>GOU</EventType>
            <EventParameters>
               <Facility>CTO</Facility>
               <Location>SGSIN</Location>
            </EventParameters>
            <EventReference />
            <IsEstimate>true</IsEstimate>
            <ContextCollection>
               <Context>
                  <Type>MBOLNumber</Type>
                  <Value>ONEYLIVA13099300</Value>
               </Context>
               <Context>
                  <Type>CarriersBookingReference</Type>
                  <Value>LIVA13099300</Value>
               </Context>
               <Context>
                  <Type>CarrierCode</Type>
                  <Value>ONEY</Value>
               </Context>
               <Context>
                  <Type>ContainerNumber</Type>
                  <Value>BMOU9774735</Value>
               </Context>
               <Context>
                  <Type>ContainerISOCode</Type>
                  <Value>45R1</Value>
               </Context>
               <Context>
                  <Type>MBOLOriginUNLOCO</Type>
                  <Value>GBSOU</Value>
               </Context>
               <Context>
                  <Type>MBOLDestinationUNLOCO</Type>
                  <Value>SGSIN</Value>
               </Context>
            </ContextCollection>
            <DataContext>
               <DataSource>
                  <DataProvider>WTG Tracking &amp; Automation</DataProvider>
               </DataSource>
            </DataContext>
         </Event>
      </UniversalEvent>";
	}
}
