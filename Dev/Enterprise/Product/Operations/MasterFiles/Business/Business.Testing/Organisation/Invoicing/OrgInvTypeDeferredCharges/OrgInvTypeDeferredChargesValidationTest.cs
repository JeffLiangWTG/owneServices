using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgInvTypeDeferredChargesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPO_AC()
		{
			OrgInvoiceType invoiceType = Factory.NewWithValidTestData<OrgInvoiceType>();
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.INC;
			OrgInvTypeDeferredCharges defcharge = Factory.NewWithValidTestData<OrgInvTypeDeferredCharges>();
			defcharge.PO_PI = invoiceType.PK;
			OrgInvTypeDeferredCharges defcharge2 = Factory.NewWithValidTestData<OrgInvTypeDeferredCharges>();
			defcharge2.PO_PI = invoiceType.PK;
			OrgInvTypeDeferredChargesValidation validation = new OrgInvTypeDeferredChargesValidation(defcharge);
			OrgInvTypeDeferredChargesCollection collection = new OrgInvTypeDeferredChargesCollection(Factory);
			collection.Add(defcharge);
			collection.Add(defcharge2);
			validation.ValidatePO_AC();
			Assert(defcharge.PO_ACInfo.HasError("Please enter a " + defcharge.PO_ACInfo.Description + "."));
			AccChargeCode chargeCodeFRT = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			defcharge.PO_AC = chargeCodeFRT.PK;
			validation.ValidatePO_AC();
			Assert(!defcharge.PO_ACInfo.HasError("Please enter a " + defcharge.PO_ACInfo.Description + "."));
			defcharge.PO_ChargeGroup = "FRT";
			validation.ValidatePO_AC();
			Assert(defcharge.PO_ACInfo.HasError("Please do not enter a " + defcharge.PO_ACInfo.Description + "."));
			defcharge.PO_AC = ZGuid.Empty;
			validation.ValidatePO_AC();
			Assert(!defcharge.PO_ACInfo.HasError("Please do not enter a " + defcharge.PO_ACInfo.Description + "."));
			defcharge.PO_ChargeGroup = "";

			defcharge.PO_AC = chargeCodeFRT.PK;
			defcharge2.PO_AC = chargeCodeFRT.PK;
			AssertHasError(defcharge.PO_ACInfo, "You cannot add row with the same value!");
			AssertHasError(defcharge2.PO_ACInfo, "You cannot add row with the same value!");
			AccChargeCode chargeCodeCAF = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CAF"));
			defcharge.PO_AC = chargeCodeCAF.PK;
			AssertNoError(defcharge.PO_ACInfo, "You cannot add row with the same value!");
			AssertNoError(defcharge2.PO_ACInfo, "You cannot add row with the same value!");

			defcharge.PO_AC = ZGuid.Empty;
			defcharge.PO_ChargeGroup = "FRT";
			Assert(defcharge2.PO_ACInfo.HasError("This charge code consist in charge group which already chosen!"));
			defcharge.PO_ChargeGroup = "CDS";
			Assert(!defcharge2.PO_ACInfo.HasError("This charge code consist in charge group which already chosen!"));
		}

		public void TestCheckPO_ChargeGroup()
		{
			OrgInvoiceType invoiceType = Factory.NewWithValidTestData<OrgInvoiceType>();
			invoiceType.PI_Calc_IsInclude = InvoiceTypeChargeInclusionTypeList.Codes.INC;
			OrgInvTypeDeferredCharges defcharge = Factory.NewWithValidTestData<OrgInvTypeDeferredCharges>();
			defcharge.PO_PI = invoiceType.PK;
			OrgInvTypeDeferredCharges defcharge2 = Factory.NewWithValidTestData<OrgInvTypeDeferredCharges>();
			defcharge2.PO_PI = invoiceType.PK;
			OrgInvTypeDeferredChargesValidation validation = new OrgInvTypeDeferredChargesValidation(defcharge);
			OrgInvTypeDeferredChargesCollection collection = new OrgInvTypeDeferredChargesCollection(Factory);
			collection.Add(defcharge);
			collection.Add(defcharge2);
			validation.ValidatePO_ChargeGroup();
			Assert(defcharge.PO_ChargeGroupInfo.HasError("Please enter a " + defcharge.PO_ChargeGroupInfo.Description + "."));
			defcharge.PO_ChargeGroup = "FRT";
			validation.ValidatePO_ChargeGroup();
			Assert(!defcharge.PO_ChargeGroupInfo.HasError("Please enter a value."));
			defcharge.PO_AC = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT")).PK;
			validation.ValidatePO_ChargeGroup();
			Assert(defcharge.PO_ChargeGroupInfo.HasError("Please do not enter a " + defcharge.PO_ChargeGroupInfo.Description + "."));
			defcharge.PO_ChargeGroup = ZString.Empty;
			validation.ValidatePO_ChargeGroup();
			Assert(!defcharge.PO_ChargeGroupInfo.HasError("Please do not enter a " + defcharge.PO_ChargeGroupInfo.Description + "."));
			defcharge.PO_AC = ZGuid.Empty;

			defcharge.PO_ChargeGroup = "FRT";
			defcharge2.PO_ChargeGroup = "FRT";
			Assert(defcharge.PO_ChargeGroupInfo.HasError("You can't add row with the same value!"));
			Assert(defcharge2.PO_ChargeGroupInfo.HasError("You can't add row with the same value!"));
			defcharge.PO_ChargeGroup = "CDS";
			Assert(!defcharge.PO_ChargeGroupInfo.HasError("You can't add row with the same value!"));
			Assert(!defcharge2.PO_ChargeGroupInfo.HasError("You can't add row with the same value!"));
		}
	}
}
