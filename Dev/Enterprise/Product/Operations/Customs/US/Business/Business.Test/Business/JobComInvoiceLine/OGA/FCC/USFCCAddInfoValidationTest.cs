using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USFCCAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_FCCImpCondNo()
		{
			FCCAddInfo.US_FCCImpCondNo = "X";
			AssertHasMessageError("Code not in List", FCCAddInfo.US_FCCImpCondNoInfo, ListValidation.InvalidCodeMessageError);

			FCCAddInfo.US_FCCImpCondNo = FCCImportConditionNumberList.Codes._02;
			AssertNoMessageError("Code not in List", FCCAddInfo.US_FCCImpCondNoInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError("No message errors 'Import Condition Number Required'", FCCAddInfo.US_FCCImpCondNoInfo, USFCCAddInfoValidation.ImportConditionNumRequired);

			FCCAddInfo.US_FCCImpCondNo = "";
			AssertHasMessageError("Notification should be added - Import Condition Number Required", FCCAddInfo.US_FCCImpCondNoInfo, USFCCAddInfoValidation.ImportConditionNumRequired);
		}

		public void TestCheckUS_FCCImpCondNoQtyAppr()
		{
			Validation.ValidateUS_FCCImpCondNoQtyAppr();
			AssertEquals("No validation notification at all", false, FCCAddInfo.US_FCCImpCondNoQtyApprInfo.HasNotifications());

			FCCAddInfo.US_FCCImpCondNo = FCCImportConditionNumberList.Codes._03;
			Validation.ValidateUS_FCCImpCondNoQtyAppr();
			AssertNoMessageError("Qty less than 200 and Qty not approved, should not be 'Approval Not Required For Quantity' message error",
								FCCAddInfo.US_FCCImpCondNoQtyApprInfo,
								USFCCAddInfoValidation.ApprovalNotRequiredForQuantity);

			FCCAddInfo.US_FCCQty = 250;
			Validation.ValidateUS_FCCImpCondNoQtyAppr();
			AssertHasMessageError("Qty more than 200, should be 'ApprovalRequiredForQuantity' message error",
					FCCAddInfo.US_FCCImpCondNoQtyApprInfo,
					USFCCAddInfoValidation.ApprovalRequiredForQuantity);

			FCCAddInfo.US_FCCImpCondNoQtyAppr = true;
			AssertNoMessageError("Qty Approved, no 'ApprovalRequiredForQuantity' message error",
									FCCAddInfo.US_FCCImpCondNoQtyApprInfo,
									USFCCAddInfoValidation.ApprovalRequiredForQuantity);

			FCCAddInfo.US_FCCQty = 200;
			Validation.ValidateUS_FCCImpCondNoQtyAppr();
			AssertHasMessageError("Qty less than 200 and Qty approved (which is not required in this case), should be 'Approval Not Required For Quantity' message error",
					FCCAddInfo.US_FCCImpCondNoQtyApprInfo,
					USFCCAddInfoValidation.ApprovalNotRequiredForQuantity);

			FCCAddInfo.US_FCCImpCondNo = FCCImportConditionNumberList.Codes._01;
			Validation.ValidateUS_FCCImpCondNoQtyAppr();
			AssertHasMessageError("Prior approval is not required for Import Condition Number '01', should be message error",
									FCCAddInfo.US_FCCImpCondNoQtyApprInfo,
									USFCCAddInfoValidation.ApprovalNotRequiredMessage);

			FCCAddInfo.US_FCCImpCondNoQtyAppr = false;
			Validation.ValidateUS_FCCImpCondNoQtyAppr();
			AssertNoMessageError("Qty not approved and prior approval is not required for Import Condition Number '01', should not have message error",
						FCCAddInfo.US_FCCImpCondNoQtyApprInfo,
						USFCCAddInfoValidation.ApprovalNotRequiredMessage);
		}

		public void TestCheckUS_FCCID()
		{
			FCCAddInfo.US_FCCImpCondNo = FCCImportConditionNumberList.Codes._03;
			Validation.ValidateUS_FCCID();
			AssertNoMessageError("No validation notification, because Import Condition Number not equals '01'",
								FCCAddInfo.US_FCCIDInfo, USFCCAddInfoValidation.IDRequired);

			FCCAddInfo.US_FCCImpCondNo = FCCImportConditionNumberList.Codes._01;
			Validation.ValidateUS_FCCID();
			AssertHasMessageError("Notification exists, because Import Condition Number equals '01' and ID is empty",
								FCCAddInfo.US_FCCIDInfo, USFCCAddInfoValidation.IDRequired);

			FCCAddInfo.US_FCCID = "12345";
			AssertNoMessageError("No notifications, because Import Condition Number equals '01' and ID is not empty",
								FCCAddInfo.US_FCCIDInfo, USFCCAddInfoValidation.IDRequired);

			FCCAddInfo.US_FCCID = "123";
			AssertHasMessageError("Length notification exists, because length of ID < 4",
								FCCAddInfo.US_FCCIDInfo, USFCCAddInfoValidation.IDLengthIsIncorrect);

			FCCAddInfo.US_FCCImpCondNo = FCCImportConditionNumberList.Codes._02;
			Validation.ValidateUS_FCCID();
			AssertHasMessageError("Length notification exists, because length of ID < 4",
								FCCAddInfo.US_FCCIDInfo, USFCCAddInfoValidation.IDLengthIsIncorrect);

			FCCAddInfo.US_FCCID = "12345";
			AssertNoMessageError("No length notifications exists, because length of ID equals 4 chars",
					FCCAddInfo.US_FCCIDInfo, USFCCAddInfoValidation.IDLengthIsIncorrect);
		}

		public void TestCheckUS_FCCTradeName()
		{
			FCCAddInfo.US_FCCTradeName = "TRADENAME";
			AssertNoMessageError("No validation for Trade Name, because Trade Name entered", FCCAddInfo.US_FCCTradeNameInfo, USFCCAddInfoValidation.TradeNameRequired);

			FCCAddInfo.US_FCCTradeName = "";
			AssertHasMessageError("Validation notification for Trade Name exists, because Trade Name not entered", FCCAddInfo.US_FCCTradeNameInfo, USFCCAddInfoValidation.TradeNameRequired);
		}

		public void TestCheckUS_FCCModel()
		{
			FCCAddInfo.US_FCCModel = "MODEL";
			AssertNoMessageError("No validation for Model, because Model entered", FCCAddInfo.US_FCCModelInfo, USFCCAddInfoValidation.ModelRequired);

			FCCAddInfo.US_FCCModel = "";
			AssertHasMessageError("Validation notification exists for Model, because Model not entered", FCCAddInfo.US_FCCModelInfo, USFCCAddInfoValidation.ModelRequired);
		}

		public void TestCheckUS_FCCQtyForInvoiceLine()
		{
			FCCAddInfo.US_FCCQty = 10;
			AssertNoMessageError("No validation for Quantity, because Qty entered", FCCAddInfo.US_FCCQtyInfo, USFCCAddInfoValidation.QuantityRequired);

			FCCAddInfo.US_FCCQty = 0;
			AssertHasMessageError("notification exists for Quantity, because FDA belongs to Invoice Line and Qty not entered", FCCAddInfo.US_FCCQtyInfo, USFCCAddInfoValidation.QuantityRequired);

			FCCAddInfo.US_FCCQty = -5;
			AssertHasMessageError("Negative Amount Not Allowed", FCCAddInfo.US_FCCQtyInfo, USFCCAddInfoValidation.QuantityRequired);
		}

		public void TestCheckUS_FCCCommercialDesc()
		{
			Validation.ValidateUS_FCCCommercialDesc();
			AssertEquals(true, FCCAddInfo.US_FCCCommercialDescInfo.HasNotifications());

			FCCAddInfo.US_FCCCommercialDesc = "";
			AssertEquals(true, FCCAddInfo.US_FCCCommercialDescInfo.HasNotifications());

			FCCAddInfo.US_FCCCommercialDesc = "TEST";
			AssertEquals(false, FCCAddInfo.US_FCCCommercialDescInfo.HasNotifications());
		}

		#region Implementation

		USFCCAddInfoValidation Validation
		{
			get { return FCCAddInfo.Validation; }
		}

		FCCAddInfo FCCAddInfo
		{
			get
			{
				if (fccAddInfo == null)
				{
					fccAddInfo = new FCCAddInfo(FCC.B7_AddInfoDataInfo);
				}

				return fccAddInfo;
			}
		}
		FCCAddInfo fccAddInfo;

		FCC FCC
		{
			get
			{
				if (fcc == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					fcc = invoiceLine.FCCs.AddNew();
				}
				return fcc;
			}
		}
		FCC fcc;

		#endregion
	}
}
