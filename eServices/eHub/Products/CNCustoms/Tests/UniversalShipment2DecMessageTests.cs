using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Core.Transforms.Helper.Testing;
using CargoWise.eHub.Products.CNCustoms.Transforms.UniversalShipment2DecMessage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Tests
{
	[TestClass]
	public class UniversalShipment2DecMessageTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2DecMessage_Test01_IMPCUS()
		{
			TestMap("UniversalShipment2DecMessage_input.UniversalShipment2DecMessage_input01_IMPCUS.xml",
						"UniversalShipment2DecMessage_output.UniversalShipment2DecMessage_output01_IMPCUS.xml",
						"CUS201811300000050", "WTLDCNJNSBJS_CSW");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2DecMessage_Test02_IMPREC()
		{
			TestMap("UniversalShipment2DecMessage_input.UniversalShipment2DecMessage_input02_IMPREC.xml",
						"UniversalShipment2DecMessage_output.UniversalShipment2DecMessage_output02_IMPREC.xml",
						"REC201811300000005", "WTLDCNJNS_CSW");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2DecMessage_Test03_EXPCUS()
		{
			TestMap("UniversalShipment2DecMessage_input.UniversalShipment2DecMessage_input03_EXPCUS.xml",
						"UniversalShipment2DecMessage_output.UniversalShipment2DecMessage_output03_EXPCUS.xml",
						"CUS201812030000006");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2DecMessage_Test04_EXPREC()
		{
			TestMap("UniversalShipment2DecMessage_input.UniversalShipment2DecMessage_input04_EXPREC.xml",
						"UniversalShipment2DecMessage_output.UniversalShipment2DecMessage_output04_EXPREC.xml",
						"REC201812030000006");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2DecMessage_Test05_IMPBTHCUS()
		{
			TestMap("UniversalShipment2DecMessage_input.UniversalShipment2DecMessage_input05_IMPBTHCUS.xml",
						"UniversalShipment2DecMessage_output.UniversalShipment2DecMessage_output05_IMPBTHCUS.xml",
						"CUS201811300000005");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2DecMessage_Test06_IMPBTHREC()
		{
			TestMap("UniversalShipment2DecMessage_input.UniversalShipment2DecMessage_input06_IMPBTHREC.xml",
						"UniversalShipment2DecMessage_output.UniversalShipment2DecMessage_output06_IMPBTHREC.xml",
						"REC201811300000005");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2DecMessage_Test07_EXPBTHCUS()
		{
			TestMap("UniversalShipment2DecMessage_input.UniversalShipment2DecMessage_input07_EXPBTHCUS.xml",
						"UniversalShipment2DecMessage_output.UniversalShipment2DecMessage_output07_EXPBTHCUS.xml",
						"CUS201812030000006");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2DecMessage_Test08_EXPBTHREC()
		{
			TestMap("UniversalShipment2DecMessage_input.UniversalShipment2DecMessage_input08_EXPBTHREC.xml",
						"UniversalShipment2DecMessage_output.UniversalShipment2DecMessage_output08_EXPBTHREC.xml",
						"REC201812030000006");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2DecMessage_Test09_IMPCUSCIQ()
		{
			TestMap("UniversalShipment2DecMessage_input.UniversalShipment2DecMessage_input09_IMPCUSCIQ.xml",
						"UniversalShipment2DecMessage_output.UniversalShipment2DecMessage_output09_IMPCUSCIQ.xml",
						"CUS201811300000050");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2DecMessage_Test10_EXPCUSCIQ()
		{
			TestMap("UniversalShipment2DecMessage_input.UniversalShipment2DecMessage_input10_EXPCUSCIQ.xml",
						"UniversalShipment2DecMessage_output.UniversalShipment2DecMessage_output10_EXPCUSCIQ.xml",
						"CUS201812030000006");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2DecMessage_Test11_SecondQtyIssue()
		{
			TestMap("UniversalShipment2DecMessage_input.UniversalShipment2DecMessage_input11_SecondQty.xml",
						"UniversalShipment2DecMessage_output.UniversalShipment2DecMessage_output11_SecondQty.xml",
						"CUS202312250000120");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void UniversalShipment2DecMessage_Test12_MultiEntries()
		{
			TestMap("UniversalShipment2DecMessage_input.UniversalShipment2DecMessage_input12_MultiEntries.xml",
						"UniversalShipment2DecMessage_output.UniversalShipment2DecMessage_output12_MultiEntries.xml",
						"CUS202411060000129");
		}

		void TestMap(string sourceFile, string outputFile, string lrnNumber, string expectedDestinationParty = "Test11223_CSW")
		{
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetColumnValue", "@result", "@tableName", "RefCountry", "@inputColumn", "RN_Code", "@outputColumn", "RN_IsoAlpha3Code", "@inputValue", "")).Return("");
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetColumnValue", "@result", "@tableName", "RefCountry", "@inputColumn", "RN_Code", "@outputColumn", "RN_IsoAlpha3Code", "@inputValue", "CN")).Return("CHN");
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetColumnValue", "@result", "@tableName", "RefCountry", "@inputColumn", "RN_Code", "@outputColumn", "RN_IsoAlpha3Code", "@inputValue", "US")).Return("USA");
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetColumnValue", "@result", "@tableName", "RefCountry", "@inputColumn", "RN_Code", "@outputColumn", "RN_IsoAlpha3Code", "@inputValue", "AD")).Return("AND");
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetColumnValue", "@result", "@tableName", "RefCountry", "@inputColumn", "RN_Code", "@outputColumn", "RN_IsoAlpha3Code", "@inputValue", "AU")).Return("AUS");
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetColumnValue", "@result", "@tableName", "RefCountry", "@inputColumn", "RN_Code", "@outputColumn", "RN_IsoAlpha3Code", "@inputValue", "NZ")).Return("NZL");
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("GetColumnValue", "@result", "@tableName", "RefCountry", "@inputColumn", "RN_Code", "@outputColumn", "RN_IsoAlpha3Code", "@inputValue", "TW")).Return("TWN");
			mockCodeMapper.Stub(x => x.CallActionProcedureHelper("SelectClientExists", "", "@ID", expectedDestinationParty)).Return("True");

			var ctx = InitialiseTestingMessageContext();

			var extensionObjects = new Dictionary<string, object>() {
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockCodeMapper }
			};

			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), ComparerToExcludeDateTimeNodes, extensionObjects);

			mapTester.Execute<UniversalShipment2DecMessage>(sourceFile, outputFile);

			var bizTlkPropertiesNs = "http://schemas.microsoft.com/BizTalk/2003/system-properties";
			Assert.IsTrue(Regex.Match(ctx.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06").ToString(), lrnNumber + "_" + DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHH") + @"\d{7}").Success);
			Assert.AreEqual(ctx.Read("DestinationParty", bizTlkPropertiesNs).ToString(), expectedDestinationParty);
			mockCodeMapper.VerifyAllExpectations();
		}

		static TestingMessageContext InitialiseTestingMessageContext()
		{
			var bizTlkPropertiesNs = "http://schemas.microsoft.com/BizTalk/2003/system-properties";
			var ctx = new TestingMessageContext();
			ctx.Write("SourceParty", bizTlkPropertiesNs, "Test11223");
			ctx.Write("DestinationParty", bizTlkPropertiesNs, "DEST123");
			ctx.Write("DestinationPartyQualifier", bizTlkPropertiesNs, "");
			ctx.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "");
			var ca = new ContextAccessor();
			ca.SetTestingMessageContext(ctx);

			return ctx;
		}

		ICompare ComparerToExcludeDateTimeNodes
		{
			get
			{
				if (comparer == null)
				{
					var exclusionXpaths = new List<string>();
					exclusionXpaths.Add("//*[local-name()='DecMessage']/*[local-name()='DecHead']/*[local-name()='PDate']");
					comparer = new ExcludingComparer(exclusionXpaths);
				}

				return comparer;
			}
		}

		ICompare comparer;
	}
}
