using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(ExportAWBSecurityStatusLine))]
	sealed class ExportAWBSecurityStatusLineTest : EnterpriseBusinessObjectTestCaseWithListChecking<ExportAWBSecurityStatusLine>
	{
		public void TestIsEmpty()
		{
			var line = Factory.New<ExportAWBSecurityStatusLine>();
			AssertEquals("IsEmpty", true, line.IsEmpty);

			line.EAS_RN_NKCountryCode = "GB";
			AssertEquals("IsEmpty", false, line.IsEmpty);

			line.EAS_RN_NKCountryCode = ZString.Empty;
			line.EAS_ApprovalCategory = "XX";
			AssertEquals("IsEmpty", false, line.IsEmpty);

			line.EAS_ApprovalCategory = ZString.Empty;
			line.EAS_ApprovalNumber = "1234";
			AssertEquals("IsEmpty", false, line.IsEmpty);

			line.EAS_ApprovalNumber = ZString.Empty;
			line.EAS_ApprovalExpiryDate = ZDateTime.Today;
			AssertEquals("IsEmpty", false, line.IsEmpty);

			line.EAS_ApprovalExpiryDate = ZDateTime.Empty;
			line.EAS_ScreeningMethod = "XXX";
			AssertEquals("IsEmpty", false, line.IsEmpty);

			line.EAS_ScreeningMethod = ZString.Empty;
			line.EAS_ExemptionGround = "XXX";
			AssertEquals("IsEmpty", false, line.IsEmpty);

			line.EAS_ExemptionGround = ZString.Empty;
			AssertEquals("IsEmpty", true, line.IsEmpty);
		}

		public void TestApprovalCategoryList()
		{
			var line = Factory.New<ExportAWBSecurityStatusLine>();
			AssertContainsExactElementsInAnyOrder("ApprovalCategoryList",
				new[] { "AA", "AC", "AH", "CA", "CH", "KC", "RA", "RC", "RE" },
				line.ApprovalCategoryList.Cast<ICodeDescription>().Select(elem => elem.Code));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<ExportAWBHeader>();
			var result = factory.NewWithValidTestData<ExportAWBSecurityStatusLine>();
			result.EAS_EH = header.PK;

			return result;
		}
	}
}
