using System.Threading.Tasks;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CreditTermsAcknowledgementCheckerTest : TestCase
	{
		public void TestCreditCheckTermType()
		{
			var term = new CreditReportTermsAndConditionForTest();
			var result = term.Type;
			AssertEquals("CCS", result);
		}

		#region Implementation

		public class CreditReportTermsAndConditionForTest : CreditReportTermsAndCondition
		{
			public bool IsLocalDisplayConditionSatisfied_Exposed() => base.IsLocalDisplayConditionSatisfied();
			public override async Task<bool> TryLoadTerm() => await Task.FromResult(true);
		}

		#endregion
	}
}
