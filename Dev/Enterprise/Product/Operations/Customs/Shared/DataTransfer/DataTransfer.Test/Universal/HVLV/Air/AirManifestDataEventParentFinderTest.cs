using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.DataTransfer.Universal.AirManifest.Testing
{
	sealed class AirManifestDataEventParentFinderTest : TestCaseWithFactory
	{
		public void TestGetMAWBs()
		{
			var xmlObject = ObjectFactory.Get<IXmlEventDeserializer>().Parse(AirManifestXMLEvent);

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = ZString.Empty;

			var mawbBO = AirManifestDataEventParentFinder.GetMAWBs(Factory, xmlObject);
			AssertEquals("No MAWB should be found because MAWBNumer is empty", 0, mawbBO.Length);

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "AAA";
			var branch1 = company1.Branches.AddNew();

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Code = "BBB";
			var branch2 = company2.Branches.AddNew();

			var mawb1 = Factory.New<CusMAWB>();
			mawb1.CM_MAWB = "MB1";
			mawb1.CM_GB = branch1.PK;
			mawb1.CM_MasterHouseBill = "MH1";

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.CM_MAWB = "MB2";
			mawb2.CM_GB = branch2.PK;
			mawb2.CM_MasterHouseBill = "MH2";

			var mawb3 = Factory.New<CusMAWB>();
			mawb3.CM_MAWB = "MB2";
			mawb3.CM_GB = branch1.PK;
			mawb3.CM_MasterHouseBill = "MH1";

			mawbBO = AirManifestDataEventParentFinder.GetMAWBs(Factory, xmlObject);
			AssertEquals("MAWB should be found", 1, mawbBO.Length);
			AssertEquals("CM_MAWB", "MB1", mawbBO[0].CM_MAWB);
			AssertEquals("CM_MasterHouseBill", "MH1", mawbBO[0].CM_MasterHouseBill);
		}

		public void TestEmptyMAWBNumberShouldNotTargetThisModule()
		{
			var xmlEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	  <Event>
		<DataContext>
		  <Company>
			<Code>MEL</Code>
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
			mawb.CM_MAWB = ZString.Empty;
			var finder = new AirManifestDataEventParentFinder(Factory, new AirManifestDataContextManager(), new DummyLogger());
			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var parents = finder.GetLogParentsForEvent(deserializer.Parse(xmlEvent));
			AssertNull("no parents should be found because MAWBNumer is empty", parents);
		}

		const string AirManifestXMLEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	  <Event>
		<DataContext>
		  <Company>
			<Code>AAA</Code>
			<Country>
			  <Code>AU</Code>
			</Country>
		  </Company>
		</DataContext>
		<EventTime>2014-12-10T10:28:50</EventTime>
		<EventType>CES</EventType>
		<ContextCollection>
			<Context>
			  <Type>MAWBNumber</Type>
			  <Value>MB1</Value>
			</Context>
			<Context>
			  <Type>MasterHouseBill</Type>
			  <Value>MH1</Value>
			</Context>
		</ContextCollection>
	  </Event>
	</UniversalEvent>
";
	}
}
