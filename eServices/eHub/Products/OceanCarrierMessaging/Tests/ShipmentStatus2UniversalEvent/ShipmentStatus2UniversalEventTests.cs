using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.ShipmentStatus2UniversalEvent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
	[TestClass]
	public class ShipmentStatus2UniversalEventTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestShipmentStatus2UniversalEvent()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Event Type", "AE", "Y")).Return("FLO");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Reference", "AE", "Y")).Return("Loaded on Rail, {ContainerStatusDescription}");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Facility?", "AE", "Y")).Return("Y");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Location?", "AE", "Y")).Return("Y");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Type", "AE", "Y")).Return("Loaded");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Department", "AE", "Y")).Return("Terminal");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Estimate?", "AE", "Y")).Return("");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Container Status Desc", "Description", "L")).Return("Full Container");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Location", "Facility", "D")).Return("CTO");
			mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10");
			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper },
			};
			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			const string sourceFile = "ShipmentStatus2UniversalEvent.TestFiles.ShipmentStatus2UniversalEvent_input.xml";
			const string expectedFile = "ShipmentStatus2UniversalEvent.TestFiles.ShipmentStatus2UniversalEvent_output.xml";

			mapTester.Execute<ShipmentStatus2UniversalEvent>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestShipmentStatus2UniversalEvent_ContainerEmpty()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Event Type", "X1", "Y")).Return("DLV");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Reference", "X1", "Y")).Return("");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Facility?", "X1", "Y")).Return("Y");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Location?", "X1", "Y")).Return("Y");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Type", "X1", "Y")).Return("");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Department", "X1", "Y")).Return("");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Type", "Estimate?", "X1", "Y")).Return("Y");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Container Status Desc", "Description", "E")).Return("Empty Container");
			mockCodeMapper.Expect(x => x.GetRecipientCode("GTNEXUS", "GTNEXUS", "Shipment Status from GTNEXUS", "Event Location", "Facility", "D")).Return("CTO");
			mockDateMapper.Expect(x => x.CurrentDateTime(Arg.Is("s"))).Return("2014-09-09T09:30:10");
			var extensionObjects = new Dictionary<string, object>() { 
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper },
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			string sourceFile = "ShipmentStatus2UniversalEvent.TestFiles.ShipmentStatus2UniversalEvent_ContainerEmpty_input.xml";
			string expectedFile = "ShipmentStatus2UniversalEvent.TestFiles.ShipmentStatus2UniversalEvent_ContainerEmpty_output.xml";
			
			mapTester.Execute<ShipmentStatus2UniversalEvent>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
			mockDateMapper.VerifyAllExpectations();
		}
	}
}
