using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USDOTAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_DOTCommercialDesc()
		{
			Validation.ValidateUS_DOTCommercialDesc();
			AssertEquals(true, DOTAddInfo.US_DOTCommercialDescInfo.HasNotifications());

			DOTAddInfo.US_DOTCommercialDesc = "TEST";
			AssertEquals(false, DOTAddInfo.US_DOTCommercialDescInfo.HasNotifications());
		}

		public void TestCheckUS_DOTBoxNo()
		{
			Validation.ValidateUS_DOTBoxNo();
			AssertEquals(true, DOTAddInfo.US_DOTBoxNoInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = "XX";
			AssertEquals(true, DOTAddInfo.US_DOTBoxNoInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._04;
			AssertEquals(false, DOTAddInfo.US_DOTBoxNoInfo.HasNotifications());

			DOT.DOTVINs.AddNew();
			AssertEquals("Pre-condition", false, DOTAddInfo.Parent.DOTVINs.HasNotifications());
			AssertEquals("Pre-condition", false, DOTAddInfo.US_DOTCountryOfOriginInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;

			AssertEquals(true, DOTAddInfo.US_DOTCountryOfOriginInfo.HasNotifications());
			AssertEquals(true, DOTAddInfo.Parent.DOTVINs.HasNotifications());
		}

		public void TestCheckUS_DOTPassport()
		{
			Validation.ValidateUS_DOTPassport();
			AssertEquals(false, DOTAddInfo.US_DOTPassportInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			Validation.ValidateUS_DOTPassport();
			AssertEquals(true, DOTAddInfo.US_DOTPassportInfo.HasNotifications());

			DOTAddInfo.US_DOTPassport = "TEST";
			AssertEquals(false, DOTAddInfo.US_DOTPassportInfo.HasNotifications());
		}

		public void TestCheckUS_DOTCountryOfOrigin()
		{
			Validation.ValidateUS_DOTCountryOfOrigin();
			AssertEquals(false, DOTAddInfo.US_DOTCountryOfOriginInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			Validation.ValidateUS_DOTCountryOfOrigin();
			AssertEquals(true, DOTAddInfo.US_DOTCountryOfOriginInfo.HasNotifications());

			DOTAddInfo.US_DOTCountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals(false, DOTAddInfo.US_DOTCountryOfOriginInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._11;
			AssertEquals(true, DOTAddInfo.US_DOTCountryOfOriginInfo.HasNotifications());

			DOTAddInfo.US_DOTCountryOfOrigin = "";
			AssertEquals(false, DOTAddInfo.US_DOTCountryOfOriginInfo.HasNotifications());
		}

		public void TestCheckUS_DOTBondSuretyCode()
		{
			Validation.ValidateUS_DOTBondSuretyCode();
			AssertEquals(false, DOTAddInfo.US_DOTBondSuretyCodeInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._03;
			Validation.ValidateUS_DOTBondSuretyCode();
			AssertEquals(true, DOTAddInfo.US_DOTBondSuretyCodeInfo.HasNotifications());

			DOTAddInfo.US_DOTBondSuretyCode = "444";
			AssertEquals(false, DOTAddInfo.US_DOTBondSuretyCodeInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._04;
			Validation.ValidateUS_DOTBondSuretyCode();
			AssertEquals(true, DOTAddInfo.US_DOTBondSuretyCodeInfo.HasNotifications());

			DOTAddInfo.US_DOTBondSuretyCode = ZString.Empty;
			AssertEquals(false, DOTAddInfo.US_DOTBondSuretyCodeInfo.HasNotifications());

			DOTAddInfo.US_DOTBondSuretyCode = "8Z1";
			AssertHasMessageError(DOTAddInfo.US_DOTBondSuretyCodeInfo, "Surety Code is not required when Box Number is other than 03.");
			AssertNoMessageError(DOTAddInfo.US_DOTBondSuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._03;
			AssertNoMessageError(DOTAddInfo.US_DOTBondSuretyCodeInfo, "Surety Code is not required when Box Number is other than 03.");
			AssertHasMessageError(DOTAddInfo.US_DOTBondSuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);

			DOTAddInfo.US_DOTBondSuretyCode = "891";
			AssertNoMessageError(DOTAddInfo.US_DOTBondSuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
		}

		public void TestCheckUS_DOTPriorApproval()
		{
			DOTAddInfo.US_DOTPriorApproval = true;
			AssertEquals(true, DOTAddInfo.US_DOTPriorApprovalInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._2B;
			Validation.ValidateUS_DOTPriorApproval();
			AssertEquals(false, DOTAddInfo.US_DOTPriorApprovalInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._06;
			Validation.ValidateUS_DOTPriorApproval();
			AssertEquals(false, DOTAddInfo.US_DOTPriorApprovalInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._07;
			Validation.ValidateUS_DOTPriorApproval();
			AssertEquals(false, DOTAddInfo.US_DOTPriorApprovalInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._12;
			Validation.ValidateUS_DOTPriorApproval();
			AssertEquals(false, DOTAddInfo.US_DOTPriorApprovalInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._08;
			Validation.ValidateUS_DOTPriorApproval();

			AssertEquals(true, DOTAddInfo.US_DOTPriorApprovalInfo.HasNotifications());
			DOTAddInfo.US_DOTPriorApproval = false;
			AssertEquals(false, DOTAddInfo.US_DOTPriorApprovalInfo.HasNotifications());
		}

		public void TestCheckUS_DOTImpSubstStatement()
		{
			DOTAddInfo.US_DOTImpSubstStatement = true;
			AssertEquals(true, DOTAddInfo.US_DOTImpSubstStatementInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._2B;
			Validation.ValidateUS_DOTImpSubstStatement();
			AssertEquals(false, DOTAddInfo.US_DOTImpSubstStatementInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._03;
			Validation.ValidateUS_DOTImpSubstStatement();
			AssertEquals(false, DOTAddInfo.US_DOTImpSubstStatementInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._07;
			Validation.ValidateUS_DOTImpSubstStatement();
			AssertEquals(false, DOTAddInfo.US_DOTImpSubstStatementInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._08;
			Validation.ValidateUS_DOTImpSubstStatement();
			AssertEquals(false, DOTAddInfo.US_DOTImpSubstStatementInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			Validation.ValidateUS_DOTImpSubstStatement();

			AssertEquals(true, DOTAddInfo.US_DOTImpSubstStatementInfo.HasNotifications());
			DOTAddInfo.US_DOTImpSubstStatement = false;
			AssertEquals(false, DOTAddInfo.US_DOTImpSubstStatementInfo.HasNotifications());
		}

		public void TestCheckUS_DOTClarCode()
		{
			Validation.ValidateUS_DOTClarCode();
			AssertEquals(false, DOTAddInfo.US_DOTClarCodeInfo.HasNotifications());

			DOTAddInfo.US_DOTClarCode = "X";
			AssertEquals(true, DOTAddInfo.US_DOTClarCodeInfo.HasNotifications());

			DOTAddInfo.US_DOTClarCode = ClarificationCodeList.Codes.Equipment;
			AssertEquals(false, DOTAddInfo.US_DOTClarCodeInfo.HasNotifications());

			DOTAddInfo.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._12;
			Validation.ValidateUS_DOTClarCode();
			AssertEquals(true, DOTAddInfo.US_DOTClarCodeInfo.HasNotifications());

			DOTAddInfo.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			AssertEquals(false, DOTAddInfo.US_DOTClarCodeInfo.HasNotifications());
		}

		public void TestCheckUS_DOTTireID()
		{
			Validation.ValidateUS_DOTTireID();
			AssertNoWarnings(DOTAddInfo.US_DOTTireIDInfo);

			DOTAddInfo.US_DOTClarCode = ClarificationCodeList.Codes.Tire;
			Validation.ValidateUS_DOTTireID();
			AssertHasWarnings(DOTAddInfo.US_DOTTireIDInfo);

			DOTAddInfo.US_DOTTireID = "0A5";
			Validation.ValidateUS_DOTTireID();
			AssertNoWarnings(DOTAddInfo.US_DOTTireIDInfo);

			DOTAddInfo.US_DOTTireID = "~~~";
			AssertHasMessageErrors(DOTAddInfo.US_DOTTireIDInfo);

			DOTAddInfo.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			Validation.ValidateUS_DOTTireID();
			AssertHasWarnings(DOTAddInfo.US_DOTTireIDInfo);

			DOTAddInfo.US_DOTTireID = "";
			AssertNoNotifications(DOTAddInfo.US_DOTTireIDInfo);
		}

		public void TestCheckUS_DOTTireBrandName()
		{
			Validation.ValidateUS_DOTTireBrandName();
			AssertNoWarnings(DOTAddInfo.US_DOTTireBrandNameInfo);

			DOTAddInfo.US_DOTTireID = "000";
			Validation.ValidateUS_DOTTireBrandName();
			AssertHasWarnings(DOTAddInfo.US_DOTTireBrandNameInfo);

			DOTAddInfo.US_DOTTireBrandName = "BRAND NAME";
			Validation.ValidateUS_DOTTireBrandName();
			AssertNoWarnings(DOTAddInfo.US_DOTTireBrandNameInfo);

			DOTAddInfo.US_DOTTireBrandName = "~~~";
			AssertHasMessageErrors(DOTAddInfo.US_DOTTireBrandNameInfo);

			DOTAddInfo.US_DOTTireID = "";
			Validation.ValidateUS_DOTTireBrandName();
			AssertHasWarnings(DOTAddInfo.US_DOTTireBrandNameInfo);

			DOTAddInfo.US_DOTTireBrandName = "";
			AssertNoNotifications(DOTAddInfo.US_DOTTireBrandNameInfo);
		}

		#region Implementation

		USDOTAddInfoValidation Validation
		{
			get { return DOTAddInfo.Validation; }
		}

		DOTAddInfo DOTAddInfo
		{
			get
			{
				if (dotAddInfo == null)
				{
					dotAddInfo = new DOTAddInfo(DOT.B7_AddInfoDataInfo);
				}

				return dotAddInfo;
			}
		}
		DOTAddInfo dotAddInfo;

		DOT DOT
		{
			get { return dot ?? (dot = InvoiceLine.DOTs.AddNew()); }
		}
		DOT dot;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>()); }
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
