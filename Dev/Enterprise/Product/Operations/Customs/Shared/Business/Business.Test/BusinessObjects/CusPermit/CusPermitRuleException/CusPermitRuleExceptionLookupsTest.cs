using System.Collections;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPermitRuleExceptionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCPE_ValueFromList()
		{
			Assert(typeof(ICollection).IsAssignableFrom(PermitRuleException.Lookups.CPE_ValueFromList.GetType()));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var permitHeader = Factory.New<BaseCusPermitHeader>();
			PermitRule = permitHeader.CusPermitRules.AddNew();
			PermitRuleException = PermitRule.CusPermitRuleExceptions.AddNew();
		}

		public BaseCusPermitRule PermitRule;
		public BaseCusPermitRuleException PermitRuleException;

		#endregion
	}
}
