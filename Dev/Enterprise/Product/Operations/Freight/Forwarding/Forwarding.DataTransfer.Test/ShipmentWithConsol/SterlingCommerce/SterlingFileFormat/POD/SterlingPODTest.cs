using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingPOD))]
	public class SterlingPODTest : SterlingRecordTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SterlingPOD();
		}

		public void TestSterlingPODRecord()
		{
			AssertEquals("Parsed record is different from expected", ExpectedSterlingPODRecord1, SterlingForTest.PODInfo[0].Record);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingPODRecord2, SterlingForTest.PODInfo[1].Record);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingPODRecord3, SterlingForTest.PODInfo[2].Record);
		}
		const string ExpectedSterlingPODRecord1 = "POD|PODName1|AEX>\r\n";
		const string ExpectedSterlingPODRecord2 = "POD|PODName2|AIM>\r\n";
		const string ExpectedSterlingPODRecord3 = "POD|PODName3|DCD>\r\n";
	}
}
