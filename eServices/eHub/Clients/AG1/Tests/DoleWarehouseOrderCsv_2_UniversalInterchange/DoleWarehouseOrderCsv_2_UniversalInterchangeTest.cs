using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.AG1.Transforms.DoleWarehouseOrderCsv_2_UniversalInterchange;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Clients.AG1.Tests
{
	[TestClass]
	public class DoleWarehouseOrderCsv_2_UniversalInterchangeTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestDoleWarehouseOrderCsv_2_UniversalInterchange()
		{
			var mockCodeMapper = GetMockCodeMapper();

			var extensionObjects = new Dictionary<string, object>() { 
			     { "http://schemas.microsoft.com/BizTalk/2003/ScriptNS1", mockCodeMapper }
			};
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

			string sourceFile = "DoleWarehouseOrderCsv_2_UniversalInterchange.TestFiles.DoleWarehouseOrderCsv_2_UniversalInterchange_input.xml";
			string expectedFile = "DoleWarehouseOrderCsv_2_UniversalInterchange.TestFiles.DoleWarehouseOrderCsv_2_UniversalInterchange_output.xml";
			mapTester.Execute<DoleWarehouseOrderCsv_2_UniversalInterchange>(sourceFile, expectedFile);

			mockCodeMapper.VerifyAllExpectations();
		}

		private static CodeMapper GetMockCodeMapper()
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("AG1PQCSIN_DWO", "AG1PQCSIN", "Dole WH Order csv-file: Receive Warehouse Orders", "Defaults", "Data Provider")).Return("DOLE");
			mockCodeMapper.Expect(x => x.GetRecipientCode("AG1PQCSIN_DWO", "AG1PQCSIN", "Dole WH Order csv-file: Receive Warehouse Orders", "Ship Mode", "Output Code", "OCEAN")).Return("SEA").Repeat.Twice();
			mockCodeMapper.Expect(x => x.GetRecipientCode("AG1PQCSIN_DWO", "AG1PQCSIN", "Dole WH Order csv-file: Receive Warehouse Orders", "Ship Mode", "Output Code", "AIR")).Return("AIR").Repeat.Twice();
			mockCodeMapper.Expect(x => x.GetRecipientCode("AG1PQCSIN_DWO", "AG1PQCSIN", "Dole WH Order csv-file: Receive Warehouse Orders", "Unit of Measurement", "Output Code", "PIECE")).Return("PCE").Repeat.Any();
			Func<string, string, string, string, string, string, string> returnInputField1 = (sc, rc, ts, cs, cr, inputField1) => inputField1;
			mockCodeMapper.Expect(x => x.GetRecipientCode(
				Arg<string>.Is.Equal("AG1PQCSIN_DWO"),
				Arg<string>.Is.Equal("AG1PQCSIN"),
				Arg<string>.Is.Equal("Dole WH Order csv-file: Receive Warehouse Orders"),
				Arg<string>.Is.Equal("Unit of Measurement"),
				Arg<string>.Is.Equal("Output Code"),
				Arg<string>.Is.Anything)).Do(returnInputField1).Repeat.Any();

			return mockCodeMapper;
		}
	}
}
