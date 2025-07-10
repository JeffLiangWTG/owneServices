using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingOrderReference))]
	public class SterlingOrderReferenceTest : SterlingRecordTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SterlingOrderReference();
		}

		public void TestOrderReferenceRecord()
		{
			AssertEquals("Parsed record is different from expected", ExpectedSterlingOrderReferenceRecord1, SterlingForTest.OrderReferenceInfo[0].Record);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingOrderReferenceRecord2, SterlingForTest.OrderReferenceInfo[1].Record);
			AssertEquals("Parsed record is different from expected", ExpectedSterlingOrderReferenceRecord3, SterlingForTest.OrderReferenceInfo[2].Record);
		}
		const string ExpectedSterlingOrderReferenceRecord1 = "ORF|Reference1>\r\n";
		const string ExpectedSterlingOrderReferenceRecord2 = "ORF|Reference2>\r\n";
		const string ExpectedSterlingOrderReferenceRecord3 = "ORF|Reference3>\r\n";
	}
}
