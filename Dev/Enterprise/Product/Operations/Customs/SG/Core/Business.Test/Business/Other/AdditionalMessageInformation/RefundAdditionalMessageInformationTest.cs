using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(AdditionalMessageInformation))]
	public class RefundAdditionalMessageInformationTest : AdditionalMessageInformationTest
	{
		public void TestDeclaration()
		{
			AssertEquals(false, AdditionalMessageInformation.Declaration.FilteredInvoiceLines.AllowNew);
			AssertEquals(false, AdditionalMessageInformation.Declaration.FilteredInvoiceLines.AllowRemove);
		}

		public void TestUpdateIndicator()
		{
			AdditionalMessageInformation.AM_UpdateIndicator = "";
			AssertEquals(true, AdditionalMessageInformation.AM_UpdateIndicatorInfo.HasErrors());
			AdditionalMessageInformation.AM_UpdateIndicator = "XXX";
			AssertEquals(true, AdditionalMessageInformation.AM_UpdateIndicatorInfo.HasErrors());
			MergeDeclaration();
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Duty, 10m);
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Excise, 20m);
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.GST, 30m);
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.FRF;
			AssertEquals(false, AdditionalMessageInformation.AM_UpdateIndicatorInfo.HasErrors());
			AssertEquals(10m, AdditionalMessageInformation.AM_DutyRefund);
			AssertEquals(20m, AdditionalMessageInformation.AM_ExciseRefund);
			AssertEquals(30m, AdditionalMessageInformation.AM_GSTRefund);
			AssertEquals(true, Declaration.ReadOnly);
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.PRG;
			AssertEquals(false, AdditionalMessageInformation.AM_UpdateIndicatorInfo.HasErrors());
			AssertEquals(0m, AdditionalMessageInformation.AM_DutyRefund);
			AssertEquals(0m, AdditionalMessageInformation.AM_ExciseRefund);
			AssertEquals(0m, AdditionalMessageInformation.AM_GSTRefund);
			AssertEquals(true, Declaration.ReadOnly);
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.PRS;
			AssertEquals(false, AdditionalMessageInformation.AM_UpdateIndicatorInfo.HasErrors());
			AssertEquals(0m, AdditionalMessageInformation.AM_DutyRefund);
			AssertEquals(0m, AdditionalMessageInformation.AM_ExciseRefund);
			AssertEquals(0m, AdditionalMessageInformation.AM_GSTRefund);
			AssertEquals(false, Declaration.ReadOnly);
		}

		void MergeDeclaration()
		{
			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00001111";
			invoiceLine.SG_LastSellingPrice = 100m;
			Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Declaration.DoMerge();
		}

		public void TestRefundCode()
		{
			AdditionalMessageInformation.AM_RefundCode = "";
			AssertEquals(true, AdditionalMessageInformation.AM_RefundCodeInfo.HasErrors());
			AdditionalMessageInformation.AM_RefundCode = "XXX";
			AssertEquals(true, AdditionalMessageInformation.AM_RefundCodeInfo.HasErrors());
			AdditionalMessageInformation.AM_RefundCode = ReasonForRefundCodeList.Codes.RF03;
			AssertEquals(false, AdditionalMessageInformation.AM_RefundCodeInfo.HasErrors());
			AdditionalMessageInformation.AM_ReasonForRefund = "TEST";
			AdditionalMessageInformation.AM_RefundCode = ReasonForRefundCodeList.Codes.RF35;
			AssertEquals("TEST", AdditionalMessageInformation.AM_ReasonForRefund);
			AdditionalMessageInformation.AM_RefundCode = ReasonForRefundCodeList.Codes.RF34;
			AssertEquals("", AdditionalMessageInformation.AM_ReasonForRefund);
			AdditionalMessageInformation.AM_RefundCode = ReasonForRefundCodeList.Codes.RF37;
			AdditionalMessageInformation.AM_ReasonForRefund = "OVR - RF37";
			AssertEquals("Reason for Refund OVR should also allow for reason text", "OVR - RF37", AdditionalMessageInformation.AM_ReasonForRefund);
		}

		public void TestReasonForRefund()
		{
			AdditionalMessageInformation.AM_ReasonForRefund = "TEST";
			AdditionalMessageInformation.AM_RefundCode = ReasonForRefundCodeList.Codes.RF05;
			AssertEquals(true, AdditionalMessageInformation.AM_ReasonForRefundInfo.ReadOnly);
			AssertEquals("", AdditionalMessageInformation.AM_ReasonForRefund);
			AssertEquals(false, AdditionalMessageInformation.AM_ReasonForRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_RefundCode = ReasonForRefundCodeList.Codes.RF35;
			AdditionalMessageInformation.AM_ReasonForRefund = "";
			AssertEquals(false, AdditionalMessageInformation.AM_ReasonForRefundInfo.ReadOnly);
			AssertEquals(true, AdditionalMessageInformation.AM_ReasonForRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_ReasonForRefund = "TEST";
			AssertEquals(false, AdditionalMessageInformation.AM_ReasonForRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_RefundCode = ReasonForRefundCodeList.Codes.RF37;
			AdditionalMessageInformation.AM_ReasonForRefund = "";
			AssertEquals(false, AdditionalMessageInformation.AM_ReasonForRefundInfo.ReadOnly);
			AssertEquals(true, AdditionalMessageInformation.AM_ReasonForRefundInfo.HasError("An OVR Refund Request should contain the OVR Vendor GST Number and the reason for submitting the OVR Refund application."));
		}

		public void TestGSTRefund()
		{
			MergeDeclaration();
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.GST, 30m);
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.FRF;
			AssertEquals(30m, AdditionalMessageInformation.AM_GSTRefund);
			AdditionalMessageInformation.AM_GSTRefund = -3;
			AssertEquals(true, AdditionalMessageInformation.AM_GSTRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_GSTRefund = 35;
			AssertEquals(true, AdditionalMessageInformation.AM_GSTRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.PRS;
			AssertEquals(true, AdditionalMessageInformation.AM_GSTRefundInfo.ReadOnly);
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.PRG;
			AssertEquals(false, AdditionalMessageInformation.AM_GSTRefundInfo.ReadOnly);
			AdditionalMessageInformation.AM_GSTRefund = 3;
			AssertEquals(false, AdditionalMessageInformation.AM_GSTRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_GSTRefund = 30;
			AssertEquals(false, AdditionalMessageInformation.AM_GSTRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.FRF;
			AssertEquals(false, AdditionalMessageInformation.AM_GSTRefundInfo.ReadOnly);
			AdditionalMessageInformation.AM_GSTRefund = 3;
			AssertEquals(true, AdditionalMessageInformation.AM_GSTRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_GSTRefund = 30;
			AssertEquals(false, AdditionalMessageInformation.AM_GSTRefundInfo.HasErrors());
		}

		public void TestDutyRefund()
		{
			MergeDeclaration();
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Duty, 30m);
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.FRF;
			AssertEquals(30m, AdditionalMessageInformation.AM_DutyRefund);
			AdditionalMessageInformation.AM_DutyRefund = -3;
			AssertEquals(true, AdditionalMessageInformation.AM_DutyRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_DutyRefund = 35;
			AssertEquals(true, AdditionalMessageInformation.AM_DutyRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.PRS;
			AssertEquals(true, AdditionalMessageInformation.AM_DutyRefundInfo.ReadOnly);
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.PRG;
			AssertEquals(true, AdditionalMessageInformation.AM_DutyRefundInfo.ReadOnly);
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.FRF;
			AssertEquals(false, AdditionalMessageInformation.AM_DutyRefundInfo.ReadOnly);
			AdditionalMessageInformation.AM_DutyRefund = 3;
			AssertEquals(true, AdditionalMessageInformation.AM_DutyRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_DutyRefund = 30;
			AssertEquals(false, AdditionalMessageInformation.AM_DutyRefundInfo.HasErrors());
		}

		public void TestExciseRefund()
		{
			MergeDeclaration();
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.Excise, 30m);
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.FRF;
			AssertEquals(30m, AdditionalMessageInformation.AM_ExciseRefund);
			AdditionalMessageInformation.AM_ExciseRefund = -3;
			AssertEquals(true, AdditionalMessageInformation.AM_ExciseRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_ExciseRefund = 35;
			AssertEquals(true, AdditionalMessageInformation.AM_ExciseRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.PRS;
			AssertEquals(true, AdditionalMessageInformation.AM_ExciseRefundInfo.ReadOnly);
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.PRG;
			AssertEquals(true, AdditionalMessageInformation.AM_ExciseRefundInfo.ReadOnly);
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.FRF;
			AssertEquals(false, AdditionalMessageInformation.AM_ExciseRefundInfo.ReadOnly);
			AdditionalMessageInformation.AM_ExciseRefund = 3;
			AssertEquals(true, AdditionalMessageInformation.AM_ExciseRefundInfo.HasErrors());
			AdditionalMessageInformation.AM_ExciseRefund = 30;
			AssertEquals(false, AdditionalMessageInformation.AM_ExciseRefundInfo.HasErrors());
		}

		public void TestPartialRefundRequestHasRefundAmount()
		{
			MergeDeclaration();
			Declaration.ActiveEntryHeaders[0].MergedLines[0].Fees.AddOrUpdate(Registry.EntryChargeTypeList.Codes.GST, 115m);
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.FRF;
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertEquals(false, AdditionalMessageInformation.AM_GSTRefundInfo.HasError(RefundAdditionalMessageInformation.PartialRefundRequiresValue));
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.PRG;
			AssertEquals("Default values for partial refund request", 0m, AdditionalMessageInformation.AM_GSTRefund);
			AssertEquals("Default values for partial refund request", 0m, AdditionalMessageInformation.AM_DutyRefund);
			AssertEquals("Default values for partial refund request", 0m, AdditionalMessageInformation.AM_ExciseRefund);
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertEquals("Partial refund requires refund amount to be entered", true, AdditionalMessageInformation.AM_GSTRefundInfo.HasError(RefundAdditionalMessageInformation.PartialRefundRequiresValue));
			AdditionalMessageInformation.AM_GSTRefund = 55m;
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertEquals(false, AdditionalMessageInformation.AM_GSTRefundInfo.HasError(RefundAdditionalMessageInformation.PartialRefundRequiresValue));
			AdditionalMessageInformation.AM_GSTRefund = 0m;
			AdditionalMessageInformation.AM_DutyRefund = 30.75m;
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertEquals(false, AdditionalMessageInformation.AM_GSTRefundInfo.HasError(RefundAdditionalMessageInformation.PartialRefundRequiresValue));
			AdditionalMessageInformation.AM_DutyRefund = 0m;
			AdditionalMessageInformation.AM_ExciseRefund = 30.75m;
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertEquals(false, AdditionalMessageInformation.AM_GSTRefundInfo.HasError(RefundAdditionalMessageInformation.PartialRefundRequiresValue));
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.PRS;
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertEquals("Partial refund requires refund amount to be entered", true, AdditionalMessageInformation.AM_UpdateIndicatorInfo.HasError(RefundAdditionalMessageInformation.PartialRefundRequiresValue));
			var invLine1 = Declaration.InvoiceLines.AddNew();
			invLine1.SG_RefundForItemGSTAmount = 55m;
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertEquals(false, AdditionalMessageInformation.AM_UpdateIndicatorInfo.HasError(RefundAdditionalMessageInformation.PartialRefundRequiresValue));
			invLine1.SG_RefundForItemGSTAmount = 0m;
			invLine1.SG_RefundForItemCustomsDutyAmount = 30.75m;
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertEquals(false, AdditionalMessageInformation.AM_UpdateIndicatorInfo.HasError(RefundAdditionalMessageInformation.PartialRefundRequiresValue));
			invLine1.SG_RefundForItemCustomsDutyAmount = 0m;
			invLine1.SG_RefundForItemExciseAmount = 30.75m;
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertEquals(false, AdditionalMessageInformation.AM_UpdateIndicatorInfo.HasError(RefundAdditionalMessageInformation.PartialRefundRequiresValue));
		}

		public override void TestPreSaveValidation()
		{
			AdditionalMessageInformation.AM_UpdateIndicator = "";
			AdditionalMessageInformation.AM_RefundCode = ReasonForRefundCodeList.Codes.RF35;
			AdditionalMessageInformation.AM_ExtendingTemporaryImportPeriod = true;
			AdditionalMessageInformation.RunPreSaveValidation();
			AssertEquals(false, AdditionalMessageInformation.AM_CancellationCodeInfo.HasErrors());
			AssertEquals(false, AdditionalMessageInformation.AM_ExtendingTemporaryImportPeriodInfo.HasErrors());
			AssertEquals(false, AdditionalMessageInformation.AM_ReasonForAmendingInfo.HasErrors());
			AssertEquals(false, AdditionalMessageInformation.AM_ReasonForExtendingTemporaryImportPeriodInfo.HasErrors());
			AssertEquals(true, AdditionalMessageInformation.AM_ReasonForRefundInfo.HasErrors());
			AssertEquals(false, AdditionalMessageInformation.AM_RefundCodeInfo.HasErrors());
			AssertEquals(true, AdditionalMessageInformation.AM_UpdateIndicatorInfo.HasErrors());
			AssertEquals(true, AdditionalMessageInformation.AM_BrokerInfo.HasErrors());
		}

		#region IAdditionalMessageInformation
		public void TestUpdateIndicator_()
		{
			AdditionalMessageInformation.AM_UpdateIndicator = UpdateIndicatorCodeList.Codes.FRF;
			AssertEquals(UpdateIndicatorCodeList.Codes.FRF, IAdditionalMessageInformation.UpdateIndicator);
		}

		public void TestRefundCode_()
		{
			AdditionalMessageInformation.AM_RefundCode = ReasonForRefundCodeList.Codes.RF01;
			AssertEquals(ReasonForRefundCodeList.Codes.RF01, IAdditionalMessageInformation.RefundCode);
		}

		public void TestReasonForRefund_()
		{
			AdditionalMessageInformation.AM_ReasonForRefund = "Good Reason for Refund";
			AssertEquals("Good Reason for Refund", IAdditionalMessageInformation.ReasonForRefund);
		}

		public void TestGSTRefundAmount_()
		{
			AdditionalMessageInformation.AM_GSTRefund = 10m;
			AssertEquals(10m, IAdditionalMessageInformation.GSTRefundAmount);
		}

		public void TestExciseRefundAmount_()
		{
			AdditionalMessageInformation.AM_ExciseRefund = 10m;
			AssertEquals(10m, IAdditionalMessageInformation.ExciseRefundAmount);
		}

		public void TestDutyRefundAmount_()
		{
			AdditionalMessageInformation.AM_DutyRefund = 10m;
			AssertEquals(10m, IAdditionalMessageInformation.DutyRefundAmount);
		}

		#endregion
		#region Implementation
		IAdditionalMessageInformation IAdditionalMessageInformation
		{
			get
			{
				return AdditionalMessageInformation;
			}
		}

		RefundAdditionalMessageInformation AdditionalMessageInformation
		{
			get
			{
				if (additionalMessageInformation == null)
				{
					var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
					staff.GS_Code = "IGG";
					var wrapper = SGGlbStaffWrapper.Get(staff);
					wrapper.Tradenetv4Password.GP_UserID = "MAIL123";
					wrapper.Tradenetv4Password.GP_CurrentPassword = "0KKwb6ydAlmfdLUP+jgqwUGsR6GLqfWCNrg9I0zCnfI=";
					wrapper.Tradenetv4Password.CurrentDecryptedPassword = "Password";
					additionalMessageInformation = new RefundAdditionalMessageInformation(Declaration, Factory);
				}

				return additionalMessageInformation;
			}
		}

		RefundAdditionalMessageInformation additionalMessageInformation;
		JobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<JobDeclaration>());
			}
		}

		JobDeclaration declaration;
		#endregion
	}
}
