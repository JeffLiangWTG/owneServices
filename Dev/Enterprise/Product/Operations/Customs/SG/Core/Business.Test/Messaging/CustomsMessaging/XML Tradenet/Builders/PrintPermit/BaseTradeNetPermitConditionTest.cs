using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	public class BaseTradeNetPermitConditionTest : TestCaseWithFactory
	{
		public void TestGetPermitConditions()
		{
			var approvalConditions = new IApprovalCondition[] { new PermitCAApprovalCondition()
			{ ConditionCode = "CA1", ConditionDescription = new string('A', 200) }, new PermitSCApprovalCondition()
			{ ConditionCode = "SC2020", ConditionDescription = new string('B', 60) } };
			var expectedOutput = @"<b><ExpandToFit>CA1 </b> - AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
<b><ExpandToFit>SC20</b> - BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
			var actualOutput = string.Join(System.Environment.NewLine, BaseTradeNetPermitCondition.GetPermitConditions(approvalConditions).Select(c => c.Condition));
			AssertEquals("Should split these text in a proper range.", expectedOutput, actualOutput);
		}
	}
}
