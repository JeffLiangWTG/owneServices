using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgInvTypeDeferredCharges))]
	sealed class OrgInvTypeDeferredChargesTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPO_Description()
		{
			AssertEquals(ZString.Empty, defferedCharge.PO_Description);
			defferedCharge.PO_ChargeGroup = "FRT";
			AssertEquals("Freight", defferedCharge.PO_Description);
			defferedCharge.PO_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT")).PK;
			AssertEquals("International Freight", defferedCharge.PO_Description);
		}

		public void TestSettingPO_ACCallsValidation()
		{
			OrgInvoiceType invoiceType = Factory.NewWithValidTestData<OrgInvoiceType>();
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.INC;
			OrgInvTypeDeferredCharges defcharge = Factory.NewWithValidTestData<OrgInvTypeDeferredCharges>();
			defcharge.PO_PI = invoiceType.PK;
			OrgInvTypeDeferredCharges defcharge2 = Factory.NewWithValidTestData<OrgInvTypeDeferredCharges>();
			defcharge2.PO_PI = invoiceType.PK;
			OrgInvTypeDeferredChargesCollection collection = new OrgInvTypeDeferredChargesCollection(Factory);
			collection.Add(defcharge);
			collection.Add(defcharge2);

			AccChargeCode chargeCodeFRT = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			defcharge.PO_AC = chargeCodeFRT.PK;
			defcharge2.PO_ChargeGroup = "CDS";
			Assert(!defcharge.PO_ACInfo.HasErrors());
			Assert(!defcharge2.PO_ACInfo.HasErrors());
			Assert(!defcharge2.PO_ChargeGroupInfo.HasErrors());

			defcharge2.PO_AC = chargeCodeFRT.PK;
			Assert(defcharge.PO_ACInfo.HasErrors());
			Assert(defcharge2.PO_ACInfo.HasErrors());
			Assert(defcharge2.PO_ChargeGroupInfo.HasErrors());
		}

		public void TestSettingPO_ChargeGroupCallsValidation()
		{
			OrgInvoiceType invoiceType = Factory.NewWithValidTestData<OrgInvoiceType>();
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.INC;
			OrgInvTypeDeferredCharges defcharge = Factory.NewWithValidTestData<OrgInvTypeDeferredCharges>();
			defcharge.PO_PI = invoiceType.PK;
			OrgInvTypeDeferredCharges defcharge2 = Factory.NewWithValidTestData<OrgInvTypeDeferredCharges>();
			defcharge2.PO_PI = invoiceType.PK;
			OrgInvTypeDeferredChargesCollection collection = new OrgInvTypeDeferredChargesCollection(Factory);
			collection.Add(defcharge);
			collection.Add(defcharge2);

			AccChargeCode chargeCodeCAF = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CAF"));
			AccChargeCode chargeCodeFRT = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			defcharge.PO_AC = chargeCodeCAF.PK;
			defcharge2.PO_ChargeGroup = "CDS";
			Assert(!defcharge.PO_ACInfo.HasErrors());
			Assert(!defcharge.PO_ChargeGroupInfo.HasErrors());
			Assert(!defcharge2.PO_ChargeGroupInfo.HasErrors());

			defcharge.PO_ChargeGroup = "CDS";
			Assert(defcharge.PO_ACInfo.HasErrors());
			Assert(defcharge.PO_ChargeGroupInfo.HasErrors());
			Assert(defcharge2.PO_ChargeGroupInfo.HasErrors());

			defcharge.PO_ChargeGroup = "";
			defcharge2.PO_ChargeGroup = "";
			Assert(!defcharge.PO_ACInfo.HasErrors());
			defcharge2.PO_ChargeGroup = "FRT";
			Assert(defcharge.PO_ACInfo.HasErrors());
		}

		#region Implementation
		OrgInvTypeDeferredCharges defferedCharge;

		protected override void SetUp()
		{
			base.SetUp();
			defferedCharge = Factory.New<OrgInvTypeDeferredCharges>();
		}
		#endregion
	}
}
