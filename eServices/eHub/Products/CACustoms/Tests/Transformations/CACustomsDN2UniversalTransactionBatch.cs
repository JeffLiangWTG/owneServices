using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.CACustoms.Transformations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.CACustoms.Tests.Transformations
{
	[TestClass]
	public class CACustomsDN2UniversalTransactionBatchTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.CACustomsDN2UniversalTransactionBatch_Input.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.CACustomsDN2UniversalTransactionBatch_Output.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test01()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_01.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_01.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test02()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_02.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_02.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test03()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_03.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_03.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test04()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_04.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_04.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test05()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_05.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_05.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test06()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_06.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_06.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test07()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_07.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_07.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test08()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_08.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_08.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test09()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_09.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_09.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test10()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_10.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_10.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test11()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_11.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_11.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test12()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_12.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_12.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test13()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_13.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_13.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test14()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_14.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_14.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test15()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_15.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_15.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsDN2UniversalTransactionBatch_Test16()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("DN");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Input.input_16.xml";
			string outputFile = "Transformations.TestFiles.CACustomsDN2UniversalTransactionBatch_Output.output_16.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsSoA2UniversalTransactionBatch_Test01()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("3");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("SOA");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Input.input_01.xml";
			string outputFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Output.output_01.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsSoA2UniversalTransactionBatch_Test02()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("SOA");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Input.input_02.xml";
			string outputFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Output.output_02.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsSoA2UniversalTransactionBatch_Test03()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("SOA");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Input.input_03.xml";
			string outputFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Output.output_03.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsSoA2UniversalTransactionBatch_Test04()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("SOA");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Input.input_04.xml";
			string outputFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Output.output_04.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsSoA2UniversalTransactionBatch_Test05()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("SOA");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Input.input_05.xml";
			string outputFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Output.output_05.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsSoA2UniversalTransactionBatch_Test06()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("SOA");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Input.input_06.xml";
			string outputFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Output.output_06.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsSoA2UniversalTransactionBatch_Test07()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("SOA");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Input.input_07.xml";
			string outputFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Output.output_07.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsSoA2UniversalTransactionBatch_Test08()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("SOA");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Input.input_08.xml";
			string outputFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Output.output_08.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsSoA2UniversalTransactionBatch_Test09()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("2");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("SOA");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Input.input_09.xml";
			string outputFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Output.output_09.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void CACustomsSoA2UniversalTransactionBatch_Test10()
		{
			var mockContextAccessor = MockRepository.GenerateStrictMock<CargoWise.eHub.Core.Transforms.Helper.ContextAccessor>();
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB1_2", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("3");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("INETCECPT");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("VICIIDTST");
			mockContextAccessor.Expect(x => x.GetContextProperty("UNG2_1", "http://schemas.microsoft.com/Edi/PropertySchema")).Return("SOA");
			var extensionObjects = new System.Collections.Generic.Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ScriptNS2", mockContextAccessor }
			};
			string sourceFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Input.input_10.xml";
			string outputFile = "Transformations.TestFiles.CACustomsSoA2UniversalTransactionBatch_Output.output_10.xml";
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<CACustoms.Transformations.CUSDEC2UniversalTransactionBatch.CACustomsDN2UniversalTransactionBatch>(sourceFile, outputFile);
			mockContextAccessor.VerifyAllExpectations();
		}
	}
}

