using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Clients.EDI.Transforms.NativeEventInternal2NativeEventHeader;

namespace Cargowise.eHub.Clients.EDI.Tests.NativeEventInternalTests
{
	[TestClass]
	public class NativeEventInternal2NativeEventHeaderTests
	{

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestNativeEventInternal2NativeEventHeader()
		{
			MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());

			string sourceFile = "NativeEventInternalTests.TestFiles.FSU12_OneCarrierAndAllStatusRecords_NativeEventInternal.xml";
			string expectedFile = "NativeEventInternalTests.TestFiles.NativeEventHeader.xml";
			mapTester.Execute<NativeEventInternal2NativeEventHeader>(sourceFile, expectedFile);
		}
	}
}
