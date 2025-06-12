using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
	//ToDo. To use new tools/test framework that works on .Net 4.5.
	[TestClass]
	public class GLSHKResponse2GLSHKResponseFFTest
	{
		const string filePath = "GLSHKResponse2GLSHKResponseFF.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestGLSHKResponse2GLSHKResponseFF_HKCustoms()
		{
			AssertMapping1("GLSHKResponse_HKC.xml", "GLSHKResponseFF_HKC.xml");
		}

		void AssertMapping1(string inputFile, string expectedOutputFile)
		{
			//var input = filePath + inputFile;
			//var expectedOutput = filePath + expectedOutputFile;

			//var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();

			//mockContextAccessor.Expect(x => x.SetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "GLSHK")).Repeat.Once();
			//mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "GLSHK_HKC")).Repeat.Once();
			//mockContextAccessor.Expect(x => x.SetContextProperty("ReceivedFileName", "http://schemas.microsoft.com/BizTalk/2003/file-properties", "ReadyForPolling2HKC.txt")).Repeat.Once();

			//var extensionObjects = new Dictionary<string, object>() { 
			//		{ "http://schemas.microsoft.com/BizTalk/2003/ScriptContextAccessor", mockContextAccessor },
			//	};

			//var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			//mapTester.Execute<GLSHKResponse2GLSHKResponseFF>(input, expectedOutput);

			//mockContextAccessor.VerifyAllExpectations();
		}
	}
}
