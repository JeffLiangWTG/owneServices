using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.BizTalk.UnitTestFX;
using System.Reflection;
using System.IO;
using CargoWise.eHub.Products.OneStop.Transforms.OneStopVesselSchedule2UniversalSchedule;

namespace CargoWise.eHub.Products.OneStop.Tests
{
	[TestClass]
	public class OneStopVesselSchedule2UniversalScheduleTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUniversalShipment2EventMessages()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "OneStopVesselSchedule2UniversalSchedule.TestFiles.Test1_input.xml";
			string expectedFile = "OneStopVesselSchedule2UniversalSchedule.TestFiles.Test1_output1.xml";
			mapTester.Execute<OneStopVesselSchedule2UniversalSchedule>(sourceFile, expectedFile);

			sourceFile = "OneStopVesselSchedule2UniversalSchedule.TestFiles.Test1_output1.xml";
			expectedFile = "OneStopVesselSchedule2UniversalSchedule.TestFiles.Test1_output2.xml";
			mapTester.Execute<UniversalSchedule2UniversalInterchange>(sourceFile, expectedFile);
		}
	}
}

