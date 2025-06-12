using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Products.USCustoms.eBond.Transforms.DeliveryNotification2UEvent;

namespace CargoWise.eHub.Products.USCustoms.Tests
{
	[TestClass]
	public class DeliveryNotification2UEventTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestDeliveryNotification2UEvent()
		{
			var sourceFile = "DeliveryNotification2UEvent.DeliveryNotification.xml";
			var outputFile = "DeliveryNotification2UEvent.Output1.xml";

			AssertUShipment2BrokerToSurety(sourceFile, outputFile);
		}


		void AssertUShipment2BrokerToSurety(string sourceFile, string outputFile)
		{

			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();

			mockDateMapper.Expect(x => x.CurrentDateTime("s")).Return("2019-05-01T10:30:38");

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS0", mockDateMapper }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<DeliveryNotification2UEvent>(sourceFile, outputFile);

			mockDateMapper.VerifyAllExpectations();
		}
	}
}