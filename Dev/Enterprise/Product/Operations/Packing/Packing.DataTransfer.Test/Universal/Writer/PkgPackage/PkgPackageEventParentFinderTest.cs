using CargoWise.EntityFramework.Testing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Packing.DataTransfer.Testing
{
	public class PkgPackageEventParentFinderTest : TestCaseWithFactory
	{
		#region TestGetLogParents

		public void TestGetLogParents()
		{
			Data.CreatePackingData();
			var package = Helper.CreatePackage(Data.PackageJob, "P0001", 1, "BOX");
			Factory.Save();

			var xmlEvent = eventDeserializer.Parse(STUEvent);
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			var relatedObj = logParents[0] as PkgPackage;
			AssertNotNull("Not Null", relatedObj);
			AssertEquals("Package ID", package.PK, relatedObj.PK);
		}

		#endregion

		#region TestGetLogParents_NoPackageId

		public void TestGetLogParents_NoPackageId()
		{
			Data.CreatePackingData();
			var package = Helper.CreatePackage(Data.PackageJob, "P0001", 1, "BOX");
			Factory.Save();

			var xmlEvent = eventDeserializer.Parse(STUEventNoPackageId);
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertNull(logParents);
		}

		#endregion

		#region TestGetLogParents_WrongPackageId

		public void TestGetLogParents_WrongPackageId()
		{
			Data.CreatePackingData();
			var package = Helper.CreatePackage(Data.PackageJob, "P0002", 1, "BOX");
			Factory.Save();

			var xmlEvent = eventDeserializer.Parse(STUEvent);
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertNull(logParents);
		}

		#endregion

		#region Implementation

		#region STUEvent

		const string STUEvent = @"<UniversalEvent>
	<Event>
		<EventTime>2018-04-01T01:02:03.001</EventTime>
		<EventType>STU</EventType>
		<EventReference>TYP=CBN|RFN=132132132|RES=Carrier Label was generated</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>PkgPackage</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>ClientReference</Type>
				<Value>O1</Value>
			</Context>
			<Context>
				<Type>TransportBookingPackageID</Type>
				<Value>P0001</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		#region STUEventNoPackageId

		const string STUEventNoPackageId = @"<UniversalEvent>
	<Event>
		<EventTime>2018-04-01T01:02:03.001</EventTime>
		<EventType>STU</EventType>
		<EventReference>TYP=CBN|RFN=132132132|RES=Carrier Label was generated</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>PkgPackage</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>ClientReference</Type>
				<Value>O1</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			eventDeserializer = new XmlEventDeserializer();
			finder = new PkgPackageEventParentFinder(Factory, new PkgPackageDataContextManager(), new XmlSessionTracker(new ServiceTaskLogForTesting()));
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		PackingTestHelper Helper => helper ?? (helper = new PackingTestHelper(Factory));
		PackingTestHelper helper;

		TestDataForPacking Data => data ?? (data = new TestDataForPacking(Factory));
		TestDataForPacking data;

		PkgPackageEventParentFinder finder;
		XmlEventDeserializer eventDeserializer;

		#endregion
	}
}
