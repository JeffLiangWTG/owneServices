using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashAdvanceDefaultingChargeCode))]
	sealed class CashAdvanceDefaultingChargeCodeTest : CashAdvanceDefaultingJobConfigPivotTest
	{
		public void TestHumanReadableNameCore()
		{
			var pivot = Factory.New<CashAdvanceDefaultingChargeCode>();
			AssertEquals("Advance Payment Defaulting Charge Code", pivot.HumanReadableName);
		}

		public void TestIsDuplicate()
		{
			var chargeCodePk = ZGuid.BrettsGuid;
			var bizo = Factory.New<CashAdvanceDefaultingChargeCode>();
			bizo.JCT_ParentId = chargeCodePk;
			var duplicate = Factory.New<CashAdvanceDefaultingChargeCode>();
			duplicate.JCT_ParentId = chargeCodePk;

			AssertNotEquals("Precondition: two different bizos", bizo.PK, duplicate.PK);
			Assert("Bizo should not be a duplicate of itself", !duplicate.IsDuplicateOf(duplicate));
			Assert("Duplicate should consider JCT_ParentId", duplicate.IsDuplicateOf(bizo));

			duplicate.JCT_ParentId = ZGuid.NewZGuid();
			Assert("Duplicate should consider JCT_ParentId", !duplicate.IsDuplicateOf(bizo));
		}

		public void TestChargeCodeDescription()
		{
			var pivot = Factory.New<CashAdvanceDefaultingChargeCode>();
			AssertEquals(ZGuid.Empty, pivot.JCT_ParentId);
			AssertEquals(ZString.Empty, pivot.ChargeCodeDescription);

			var chargeCodeA = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeA.AC_Desc = "Charge Code A";
			pivot.JCT_ParentId = chargeCodeA.PK;
			AssertEquals("Charge Code A", pivot.ChargeCodeDescription);
		}

		public void TestSetDefaultValues()
		{
			var pivot = Factory.New<CashAdvanceDefaultingChargeCode>();
			AssertEquals(AccChargeCodeSchema.Constants.Prefix, pivot.JCT_ParentTableCode);
		}

		public void TestChargeCodeCollection()
		{
			var companyA = Factory.NewWithValidTestData<GlbCompany>();
			var branchA = Factory.NewWithValidTestData<GlbBranch>();
			branchA.GB_GC = companyA.PK;
			var chargeCodeA = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeA.AC_GC = companyA.PK;

			var companyB = Factory.NewWithValidTestData<GlbCompany>();
			var branchB = Factory.NewWithValidTestData<GlbBranch>();
			branchB.GB_GC = companyB.PK;
			var chargeCodeB = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeB.AC_GC = companyB.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchA.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var pivotA = Factory.New<CashAdvanceDefaultingChargeCode>();
				pivotA.ChargeCodes.Load();
				AssertContainsExactElementsInAnyOrder(new[] { chargeCodeA }, pivotA.ChargeCodes);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchB.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var pivotB = Factory.New<CashAdvanceDefaultingChargeCode>();
				pivotB.ChargeCodes.Load();
				AssertContainsExactElementsInAnyOrder(new[] { chargeCodeB }, pivotB.ChargeCodes);
			}
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TESTCODE";

			var configuration = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			configuration.CAC_ConfigType = JobConfiguration.TypeCodes.CashAdvanceDefaulting;
			configuration.CAC_GC = Env.CurrentCompanyPK;
			configuration.CAC_Ledger = LedgerTypes.AccountsReceivable;
			configuration.CAC_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			configuration.CAC_ServiceDirection = Core.Constants.FreightShipmentDirection.Code.Import;
			configuration.CAC_TransportMode = Core.Constants.TransportModes.Sea;
			configuration.CAC_DefaultingOption = CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;

			var pivot = Factory.New<CashAdvanceDefaultingChargeCode>();
			pivot.JCT_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
			pivot.JCT_ParentId = chargeCode.PK;
			pivot.JCT_JCF_JobConfig = configuration.PK;

			return pivot;
		}

		#endregion
	}
}
