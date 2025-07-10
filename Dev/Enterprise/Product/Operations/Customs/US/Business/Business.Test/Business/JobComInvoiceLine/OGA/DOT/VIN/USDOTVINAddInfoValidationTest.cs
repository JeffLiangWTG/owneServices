using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USDOTVINAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_DOTMake()
		{
			DOT.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			DOTVINAddInfo.Validation.ValidateUS_DOTMake();
			AssertEquals(true, DOTVINAddInfo.US_DOTMakeInfo.HasNotifications());

			DOTVINAddInfo.US_DOTMake = "TEST";
			AssertEquals(false, DOTVINAddInfo.US_DOTMakeInfo.HasNotifications());
		}

		public void TestCheckUS_DOTModel()
		{
			DOT.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			DOTVINAddInfo.Validation.ValidateUS_DOTModel();
			AssertEquals(true, DOTVINAddInfo.US_DOTModelInfo.HasNotifications());

			DOTVINAddInfo.US_DOTModel = "TEST";
			AssertEquals(false, DOTVINAddInfo.US_DOTModelInfo.HasNotifications());
		}

		public void TestCheckUS_DOTVIN()
		{
			DOT.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			DOTVINAddInfo.Validation.ValidateUS_DOTVIN();
			AssertEquals(true, DOTVINAddInfo.US_DOTVINInfo.HasNotifications());

			DOTVINAddInfo.US_DOTVIN = "TEST";
			AssertEquals(false, DOTVINAddInfo.US_DOTVINInfo.HasNotifications());
		}

		public void TestDOTVINDuplicates()
		{
			DOTVIN dotvin = DOT.DOTVINs.AddNew();
			dotvin.US_DOTVIN = "TEST";

			dotvin = DOT.DOTVINs.AddNew();
			dotvin.US_DOTVIN = "TEST";
			AssertHasMessageError(dotvin.US_DOTVINInfo, ValidationConstants.DOT.DuplicateVIN);

			dotvin.US_DOTVIN = "TEST1";
			AssertNoMessageError(dotvin.US_DOTVINInfo, ValidationConstants.DOT.DuplicateVIN);
		}

		public void TestCheckUS_DOTYear()
		{
			DOT.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			DOTVINAddInfo.Validation.ValidateUS_DOTYear();
			AssertEquals(true, DOTVINAddInfo.US_DOTYearInfo.HasNotifications());

			DOTVINAddInfo.US_DOTYear = 2000;
			AssertEquals(false, DOTVINAddInfo.US_DOTYearInfo.HasNotifications());
		}

		public void TestCheckUS_DOTRINo()
		{
			DOT.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._03;
			DOTVINAddInfo.Validation.ValidateUS_DOTRINo();
			AssertEquals(true, DOTVINAddInfo.US_DOTRINoInfo.HasNotifications());

			DOTVINAddInfo.US_DOTRINo = "R-08-ABC";
			AssertEquals(true, DOTVINAddInfo.US_DOTRINoInfo.HasNotifications());

			DOTVINAddInfo.US_DOTRINo = "R-0A-123";
			AssertEquals(true, DOTVINAddInfo.US_DOTRINoInfo.HasNotifications());

			DOTVINAddInfo.US_DOTRINo = "R08123";
			AssertEquals(true, DOTVINAddInfo.US_DOTRINoInfo.HasNotifications());

			DOTVINAddInfo.US_DOTRINo = "R-0A-12";
			AssertEquals(true, DOTVINAddInfo.US_DOTRINoInfo.HasNotifications());

			DOTVINAddInfo.US_DOTRINo = "R-08-123";
			AssertEquals(false, DOTVINAddInfo.US_DOTRINoInfo.HasNotifications());

			DOT.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._08;
			DOTVINAddInfo.Validation.ValidateUS_DOTRINo();
			AssertEquals(true, DOTVINAddInfo.US_DOTRINoInfo.HasNotifications());
		}

		//protected override void CheckUS_DOTVEN()
		//{
		//  base.CheckUS_DOTVEN();
		//  CheckRIVEN("Vehicle Eligibility Number", Parent.US_DOTVENInfo);

		//  if (!Parent.US_DOTVEN.IsEmpty)
		//  {
		//    if (!Regex.IsMatch(Parent.US_DOTVEN, @"^((VSA|VSP|VCP)\d{3})|PET$", RegexOptions.IgnoreCase))
		//    {
		//      Parent.US_DOTVENInfo.AddMessageError("The VEN Number must be in the following format; 'VSA' or 'VSP' or 'VCP' followed by 3 numerics or 'PET'.");
		//    }

		public void TestCheckUS_DOTVEN()
		{
			DOT.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._03;
			DOTVINAddInfo.Validation.ValidateUS_DOTVEN();
			AssertEquals(true, DOTVINAddInfo.US_DOTVENInfo.HasNotifications());

			DOTVINAddInfo.US_DOTVEN = "ASA123";
			AssertEquals(true, DOTVINAddInfo.US_DOTVENInfo.HasNotifications());

			DOTVINAddInfo.US_DOTVEN = "VSA1";
			AssertEquals(true, DOTVINAddInfo.US_DOTVENInfo.HasNotifications());

			DOTVINAddInfo.US_DOTVEN = "PET123";
			AssertEquals(true, DOTVINAddInfo.US_DOTVENInfo.HasNotifications());

			DOTVINAddInfo.US_DOTVEN = "VSA123";
			AssertEquals(false, DOTVINAddInfo.US_DOTVENInfo.HasNotifications());

			DOTVINAddInfo.US_DOTVEN = "VSP123";
			AssertEquals(false, DOTVINAddInfo.US_DOTVENInfo.HasNotifications());

			DOTVINAddInfo.US_DOTVEN = "VCP123";
			AssertEquals(false, DOTVINAddInfo.US_DOTVENInfo.HasNotifications());

			DOTVINAddInfo.US_DOTVEN = "PET";
			AssertEquals(false, DOTVINAddInfo.US_DOTVENInfo.HasNotifications());

			DOT.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._08;
			DOTVINAddInfo.Validation.ValidateUS_DOTVEN();
			AssertEquals(true, DOTVINAddInfo.US_DOTVENInfo.HasNotifications());
		}

		public void TestVehicleDetailsRequired()
		{
			DOT.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			AssertNoRowMessageError(DOTVIN, "Vehicle Details are only required when Clarification Code is 'V(ehicle)' or Box Number is 05");

			DOT.US_DOTClarCode = ClarificationCodeList.Codes.Equipment;
			DOT.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			AssertNoRowMessageError(DOTVIN, "Vehicle Details are only required when Clarification Code is 'V(ehicle)' or Box Number is 05");
		}

		#region Implementation

		DOTVINAddInfo DOTVINAddInfo
		{
			get
			{
				if (dotvinAddInfo == null)
				{
					dotvinAddInfo = new DOTVINAddInfo(DOTVIN.B7_AddInfoDataInfo);
				}

				return dotvinAddInfo;
			}
		}
		DOTVINAddInfo dotvinAddInfo;

		DOTVIN DOTVIN
		{
			get { return dotvin ?? (dotvin = DOT.DOTVINs.AddNew()); }
		}
		DOTVIN dotvin;

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
