using System;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	public class CYDTransportationUnitEventParentFinderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region Test CNC Events

		public void TestGetLogParentsForEvent_CNCEvent_SuccessCase()
		{
			var factory = new UniversalObjectFactory();
			var tpu = GetTransportationUnit(factory, true);
			UniversalTestHelper.CreateStmUniversalJobLink(factory, tpu.PK, "TPU123", nameof(DataContextType.GateVehicleMovement));

			var xmlEvent = GetXmlEvent(UniversalTestHelper.BuildEventXMLWithEventReference(AutoEvents.CancelledCode, nameof(DataContextType.GateVehicleMovement), "TPU123", "|EVT=GIN|OFF=011:00|RES=Test 1|RFN=RFN001|STF=AAA|TYP=Gate In"));
			var finder = GetFinder(xmlEvent);

			var businessObjects = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, businessObjects.Length);
			AssertEquals(tpu.PK, businessObjects[0].PK);
		}

		public void TestGetLogParentsForEvent_CNCEvent_NoMatchingTPU_ThrowException()
		{
			var factory = new UniversalObjectFactory();
			UniversalTestHelper.CreateStmUniversalJobLink(new UniversalObjectFactory(), ZGuid.NewZGuid(), "TPU123", nameof(DataContextType.GateVehicleMovement));

			var xmlEvent = GetXmlEvent(UniversalTestHelper.BuildEventXMLWithEventReference(AutoEvents.CancelledCode, nameof(DataContextType.GateVehicleMovement), "TPU123", "|EVT=GIN|OFF=011:00|RES=Test 1|RFN=RFN001|STF=AAA|TYP=Gate In"));
			var finder = GetFinder(xmlEvent);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Matching TPU not found", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGetLogParentsForEvent_CNCEvent_NonVehicleMovement_ThrowException()
		{
			var factory = new UniversalObjectFactory();
			var tpu = GetTransportationUnit(factory, true);
			UniversalTestHelper.CreateStmUniversalJobLink(new UniversalObjectFactory(), tpu.PK, "TPU123", nameof(DataContextType.GateMovement));

			var xmlEvent = GetXmlEvent(UniversalTestHelper.BuildEventXMLWithEventReference(AutoEvents.CancelledCode, nameof(DataContextType.GateMovement), "TPU123", "|EVT=GIN|OFF=011:00|RES=Test 1|RFN=RFN001|STF=AAA|TYP=Gate In"));
			var finder = GetFinder(xmlEvent);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Matching data source not found", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGetLogParentsForEvent_CNCEvent_NoMatchingRecipientRole_ReturnNull()
		{
			var factory = new UniversalObjectFactory();
			UniversalTestHelper.CreateStmUniversalJobLink(new UniversalObjectFactory(), ZGuid.NewZGuid(), "TPU123", nameof(DataContextType.GateVehicleMovement));

			var xmlEvent = GetXmlEvent(UniversalTestHelper.BuildEventXMLWithEventReference(AutoEvents.CancelledCode, nameof(DataContextType.GateVehicleMovement), "TPU123", "|EVT=GIN|OFF=011:00|RES=Test 1|RFN=RFN001|STF=AAA|TYP=Gate In", recipientRole: "ATW"));
			var finder = GetFinder(xmlEvent);

			AssertNoExceptionThrown("No error is thrown", () => finder.GetLogParentsForEvent(xmlEvent));
			AssertNull(finder.GetLogParentsForEvent(xmlEvent));
		}

		#endregion

		#region helpers

		protected CYDTransportationUnit GetTransportationUnit(UniversalObjectFactory factory, bool isGatedIn)
		{
			var yard = factory.NewWithValidTestData<WhsWarehouse>();
			var tpu = UniversalTestHelper.CreateTransportationUnit(factory, yard, "RFN001");
			if (isGatedIn)
			{
				var area = factory.NewWithValidTestData<WhsArea>();
				var waitingBayLocation = factory.NewWithValidTestData<WhsLocation>();
				waitingBayLocation.WLV_WA_PutawayArea = area.PK;
				tpu.YTU_GateInTime = UniversalTestHelper.TrimSeconds(DateTimeOffset.Now);
				tpu.YTU_WL_WaitingBayLocation = waitingBayLocation.PK;
			}
			return tpu;
		}

		protected IXmlEventValueObject GetXmlEvent(string xml)
		{
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(xml);
			return xmlEvent;
		}

		protected EventParentFinder GetFinder(IXmlEventValueObject xmlEvent)
		{
			var logger = new TestErrorLogger { TopLevelDataObject = xmlEvent };
			return new CYDTransportationUnitEventParentFinder(Factory.BOFactory, new CYDTransportationUnitDataContextManager(), logger);
		}

		#endregion
	}
}
