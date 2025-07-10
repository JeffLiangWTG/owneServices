using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExpireCommissionAgreementAction))]
	sealed class ExpireCommissionAgreementActionTest : NonPersistentBusinessObjectTestCase
	{
		#region Default Values

		[TestDate(2000, 1, 1)]
		public void TestDefaultValues()
		{
			var action = new ExpireCommissionAgreementAction(Enumerable.Empty<OrgCommissionAgreement>());
			AssertEquals(new ZDateTime(2000, 1, 1), action.Date);
		}

		#endregion

		#region Execute

		public void TestExecute()
		{
			var agreement1 = Factory.New<OrgCommissionAgreement>();
			var agreement2 = Factory.New<OrgCommissionAgreement>();
			var action = new ExpireCommissionAgreementAction(new[] { agreement1, agreement2 });
			action.Date = new ZDateTime(2000, 1, 1);

			action.Execute();
			AssertEquals(new ZDateTime(2000, 1, 1), agreement1.CA0_ExpiredDate);
			AssertEquals(new ZDateTime(2000, 1, 1), agreement2.CA0_ExpiredDate);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var agreement1 = Factory.New<OrgCommissionAgreement>();
			var agreement2 = Factory.New<OrgCommissionAgreement>();
			return new ExpireCommissionAgreementAction(new[] { agreement1, agreement2 });
		}

		#endregion
	}
}
