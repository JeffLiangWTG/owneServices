using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	public class TestExportAWBOtherChargesValidation : BusinessObjectValidationTestCase
	{
		public virtual void TestCheckEO_ChargeCode()
		{
			Validation.ValidateEO_ChargeCode();
			AssertHasNotifications(Validation.Parent.EO_ChargeCodeInfo);

			OtherCharges.EO_ChargeCode = "XX";
			Validation.ValidateEO_ChargeCode();
			AssertHasNotifications(Validation.Parent.EO_ChargeCodeInfo);

			OtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.MA;
			Validation.ValidateEO_ChargeCode();
			AssertNoNotifications(Validation.Parent.EO_ChargeCodeInfo);
		}

		public void TestCheckEO_EntitlementCode()
		{
			OtherCharges.EO_EntitlementCode = "";
			Validation.ValidateEO_EntitlementCode();
			AssertHasNotifications(Validation.Parent.EO_EntitlementCodeInfo);

			OtherCharges.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			Validation.ValidateEO_EntitlementCode();
			AssertNoNotifications(Validation.Parent.EO_EntitlementCodeInfo);

			OtherCharges.EO_EntitlementCode = "#";
			Validation.ValidateEO_EntitlementCode();
			AssertHasNotifications(Validation.Parent.EO_EntitlementCodeInfo);

			OtherCharges.EO_EH = ZGuid.Empty;
			Validation.ValidateEO_EntitlementCode();
			AssertHasNotifications(Validation.Parent.EO_EntitlementCodeInfo);
		}

		public void TestCheckEO_Amount()
		{
			Validation.ValidateEO_Amount();
			Assert(Validation.Parent.EO_AmountInfo.HasWarnings());

			OtherCharges.EO_Amount = 10;
			Validation.ValidateEO_Amount();
			Assert(!Validation.Parent.EO_AmountInfo.HasWarnings());
		}

		#region Implementation

		protected override void SetUp()
		{
			OtherCharges = Factory.New<ExportAWBOtherCharges>();
			OtherCharges.EO_EH = Factory.New<ExportAWBHeader>().PK;
			Validation = new ExportAWBOtherChargesValidation(OtherCharges);
		}

		ExportAWBOtherCharges OtherCharges;
		ExportAWBOtherChargesValidation Validation;

		#endregion
	}
}
