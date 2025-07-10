using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderInvoicingSupporter))]
	public class WhsOrderInvoicingSupporterTest : WhsPickableDocketInvoicingSupporterTest
	{
		#region TestAuditSecurity

		public void TestAuditSecurity()
		{
			var securityInstance = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			var security = new SecurityCheckpoint("", (NoResString)"", null, securityInstance);

			var supporter = GetNewSupporter(Docket);
			((WhsOrderInvoicingSupporter)supporter).SetAuditBillingSecurity(security);
			AssertEquals("AuditSecurity", security, supporter.AuditSecurity);
		}

		#endregion

		#region TestIJobInvoicingPlugIn_ActualChargeableUnit

		public override void TestIJobInvoicingPlugIn_ActualChargeableUnit()
		{
			Docket.WD_TotalWeight = 100;
			Docket.WD_TotalWeightUnit = "KG";

			var supporter = (WhsOrderInvoicingSupporter)GetNewSupporter(Docket);
			AssertEquals("Actual Chargeable:", (ZDecimal)100, supporter.ActualChargeable);
			AssertEquals("Actual Chargeable Unit:", "KG", supporter.ActualChargeableUnit);
		}

		#endregion

		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Factory.NewWithValidTestData<WhsOrder>();
		}

		protected override WhsDocketInvoicingSupporter GetNewSupporter(WhsDocket parent)
		{
			return new WhsOrderInvoicingSupporter((WhsOrder)parent);
		}

		#endregion
	}
}
