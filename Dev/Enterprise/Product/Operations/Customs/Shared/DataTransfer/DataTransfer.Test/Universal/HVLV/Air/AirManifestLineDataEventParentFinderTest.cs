using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest.Testing
{
	sealed class AirManifestLineDataEventParentFinderTest : TestCaseWithFactory
	{
		public void TestEmptyHAWBNumerShouldNotBeTargetToThisModule()
		{
			var xmlEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	  <Event>
		<DataContext>
		  <Company>
			<Code>" + GlbCompany.CurrentCompany.GC_Code + @"</Code>
			<Country>
			  <Code>AU</Code>
			</Country>
		  </Company>
		</DataContext>
		<EventTime>2014-12-10T10:28:50</EventTime>
		<EventType>CAD</EventType>
		<ContextCollection>
			<Context>
			  <Type>MBOLNumber</Type>
			  <Value>Ocean Bill</Value>
			</Context>
			<Context>
			  <Type>LloydsNumber</Type>
			  <Value>1357642</Value>
			</Context>
			<Context>
			  <Type>VoyageNumber</Type>
			  <Value>Voyage</Value>
			</Context>
		</ContextCollection>
	  </Event>
	</UniversalEvent>
";

			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = ZString.Empty;
			hawb.MAWB.CM_MAWB = ZString.Empty;
			hawb.MAWB.CM_ArrivalDate = ZDateTime.Today;
			var finder = new AirManifestLineDataEventParentFinder(Factory, new AirManifestLineDataContextManager(), new DummyLogger());
			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var parents = finder.GetLogParentsForEvent(deserializer.Parse(xmlEvent));
			AssertNull("no parents should be found because MAWBNumer is empty", parents);
		}
	}
}
