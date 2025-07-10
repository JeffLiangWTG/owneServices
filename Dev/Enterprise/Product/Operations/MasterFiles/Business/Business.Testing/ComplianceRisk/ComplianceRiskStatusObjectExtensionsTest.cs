using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ComplianceRiskStatusObjectExtensionsTest : TestCase
	{
		public void TestGetRiskDescription()
		{
			var risk = new ComplianceRiskStatusObject(
				ComplianceRiskStatusCodeList.Codes.OverrideClear,
				ComplianceRiskStatusCodeList.Codes.Blocked,
				ComplianceRiskStatusCodeList.Codes.Clear,
				ComplianceRiskStatusCodeList.Codes.Unknown);

			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.OverrideClear, risk.GetOverallRiskDescription());
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.Blocked, risk.GetPartyRiskDescription());
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.Clear, risk.GetLocationRiskDescription());
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.Unknown, risk.GetCommodityRiskDescription());

			var allComplianceCodes = new ComplianceRiskStatusCodeList();
			foreach (CodeDescriptionPair codeDescriptionPair in allComplianceCodes)
			{
				risk = new ComplianceRiskStatusObject(
				codeDescriptionPair.Code,
				ComplianceRiskStatusCodeList.Codes.Blocked,
				ComplianceRiskStatusCodeList.Codes.Clear,
				ComplianceRiskStatusCodeList.Codes.Unknown);

				AssertEquals(codeDescriptionPair.Description, risk.GetOverallRiskDescription());
			}
		}

		public void TestGetRiskDescription_UnmatchCode()
		{
			var risk = new ComplianceRiskStatusObject("1", "2", "3", "4");

			AssertEquals("1", risk.GetOverallRiskDescription());
			AssertEquals("2", risk.GetPartyRiskDescription());
			AssertEquals("3", risk.GetLocationRiskDescription());
			AssertEquals("4", risk.GetCommodityRiskDescription());
		}
	}
}
