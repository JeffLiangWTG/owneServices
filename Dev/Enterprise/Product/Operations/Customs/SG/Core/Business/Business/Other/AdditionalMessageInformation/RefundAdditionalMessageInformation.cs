using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.SG.V4.Business
{
	public class RefundAdditionalMessageInformation : AdditionalMessageInformation
	{
		#region Schema

		public new class Schema : AdditionalMessageInformation.Schema
		{
			public const string AM_RefundCode = "AM_RefundCode";
			public const string AM_ReasonForRefund = "AM_ReasonForRefund";
			public const string AM_UpdateIndicator = "AM_UpdateIndicator";

			public const int AM_RefundCodeMaxLength = 4;
			public const int AM_ReasonForRefundMaxLength = 280;
			public const int AM_UpdateIndicatornMaxLength = 3;
		}

		#endregion

		public RefundAdditionalMessageInformation(CUSDECEDIMessage lastMessage, JobDeclaration declaration, IStorageDocsBaseCollection eDocs, BusinessObjectFactory factory, string applicationCode)
			: base(lastMessage, eDocs, BoundFormTypes.Refund, factory, applicationCode)
		{
			this.Declaration = declaration;
		}

		public RefundAdditionalMessageInformation(JobDeclaration declaration, BusinessObjectFactory factory, string applicationCode = "SG4")
			: base(BoundFormTypes.Refund, factory, applicationCode)
		{
			this.Declaration = declaration;
		}

		public JobDeclaration Declaration
		{
			get { return declaration; }
			set
			{
				declaration = value;
				declaration.FilteredInvoiceLines.NoAddingOrRemoving = true;
			}
		}
		JobDeclaration declaration;

		#region Properties

		bool FullRefundRequest => AM_UpdateIndicator == UpdateIndicatorCodeList.Codes.FRF;
		bool PartialRefundRequest => AM_UpdateIndicator == UpdateIndicatorCodeList.Codes.PRS || AM_UpdateIndicator == UpdateIndicatorCodeList.Codes.PRG;

		#region AM_UpdateIndicator

		[MaxLength(Schema.AM_UpdateIndicatornMaxLength)]
		public ZString AM_UpdateIndicator
		{
			get { return fAM_UpdateIndicator; }
			set
			{
				CheckMaximumLength(AM_UpdateIndicatorInfo, value);
				SetNonPersistentPropertyValue(AM_UpdateIndicatorInfo, ref fAM_UpdateIndicator, value);

				if (FullRefundRequest)
				{
					AM_GSTRefund = Declaration.TotalGSTPayable;
					AM_DutyRefund = Declaration.TotalDutyPayable;
					AM_ExciseRefund = Declaration.TotalExcisePayable;
				}
				else
				{
					AM_GSTRefund = 0;
					AM_DutyRefund = 0;
					AM_ExciseRefund = 0;
				}

				Declaration.SetReadOnlyIncludingChildren(AM_UpdateIndicator != UpdateIndicatorCodeList.Codes.PRS);

				if (!IsValidationSuspended)
				{
					ValidateAM_UpdateIndicator();
					ValidateAM_GSTRefund();
					ValidateAM_DutyRefund();
					ValidateAM_ExciseRefund();
				}
			}
		}
		ZString fAM_UpdateIndicator;

		public ZPropertyInfo AM_UpdateIndicatorInfo
		{
			get { return GetZPropertyInfo(Schema.AM_UpdateIndicator); }
		}

		protected void ValidateAM_UpdateIndicator()
		{
			AM_UpdateIndicatorInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AM_UpdateIndicatorInfo, "Refund Type");
			ListValidation.ErrorIfInvalidCode(AM_UpdateIndicatorInfo, Lookups.UpdateIndicatorList);
		}

		#endregion

		#region AM_RefundCode

		[MaxLength(Schema.AM_RefundCodeMaxLength)]
		public ZString AM_RefundCode
		{
			get { return fAM_RefundCode; }
			set
			{
				CheckMaximumLength(AM_RefundCodeInfo, value);

				SetNonPersistentPropertyValue(AM_RefundCodeInfo, ref fAM_RefundCode, value);

				if (!RequiresRefundReason)
				{
					AM_ReasonForRefund = string.Empty;
				}

				if (!IsValidationSuspended)
				{
					ValidateAM_RefundCode();
				}
			}
		}
		ZString fAM_RefundCode;

		bool RequiresRefundReason => AM_RefundCode == ReasonForRefundCodeList.Codes.RF35 || AM_RefundCode == ReasonForRefundCodeList.Codes.RF37;

		public ZPropertyInfo AM_RefundCodeInfo
		{
			get { return GetZPropertyInfo(Schema.AM_RefundCode); }
		}

		protected void ValidateAM_RefundCode()
		{
			AM_RefundCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(AM_RefundCodeInfo, Lookups.RefundCodeList);
			MandatoryValidation.CheckEntered(AM_RefundCodeInfo, "Refund Code");
		}

		#endregion

		#region AM_ReasonForRefund

		[MaxLength(Schema.AM_ReasonForRefundMaxLength)]
		public ZString AM_ReasonForRefund
		{
			get { return fAM_ReasonForRefund; }
			set
			{
				CheckMaximumLength(AM_ReasonForRefundInfo, value);
				SetNonPersistentPropertyValue(AM_ReasonForRefundInfo, ref fAM_ReasonForRefund, value);
				if (!IsValidationSuspended)
				{
					ValidateAM_ReasonForRefund();
				}
			}
		}
		ZString fAM_ReasonForRefund;

		public ZPropertyInfo AM_ReasonForRefundInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.AM_ReasonForRefund);
			}
		}

		public bool AM_ReasonForRefund_ReadOnly
		{
			get { return !RequiresRefundReason; }
		}

		protected void ValidateAM_ReasonForRefund()
		{
			AM_ReasonForRefundInfo.ClearAllNotifications();
			if (AM_RefundCode == ReasonForRefundCodeList.Codes.RF35)
			{
				MandatoryValidation.CheckEntered(AM_ReasonForRefundInfo);
			}
			else if (AM_RefundCode == ReasonForRefundCodeList.Codes.RF37 && AM_ReasonForRefund.IsEmpty)
			{
				AM_ReasonForRefundInfo.AddError(OVRRefundReason);
			}
		}
		const string OVRRefundReason = "An OVR Refund Request should contain the OVR Vendor GST Number and the reason for submitting the OVR Refund application.";

		#endregion

		#region AM_GSTRefund

		public ZDecimal AM_GSTRefund
		{
			get { return fAM_GSTRefund; }
			set
			{
				SetNonPersistentPropertyValue(AM_GSTRefundInfo, ref fAM_GSTRefund, value);
				if (!IsValidationSuspended)
				{
					ValidateAM_GSTRefund();
				}
			}
		}
		ZDecimal fAM_GSTRefund;

		public ZPropertyInfo AM_GSTRefundInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AM_GSTRefund), "GST Refund Amount");
			}
		}

		public bool AM_GSTRefund_ReadOnly
		{
			get { return AM_UpdateIndicator == UpdateIndicatorCodeList.Codes.PRS; }
		}

		protected void ValidateAM_GSTRefund()
		{
			AM_GSTRefundInfo.ClearAllNotifications();

			if (FullRefundRequest && AM_GSTRefund.Round(2) != Declaration.TotalGSTPayable.Round(2))
			{
				AM_GSTRefundInfo.AddError("GST Refund should be equal to Total GST Payable.");
			}
			else if (AM_GSTRefund < 0)
			{
				AM_GSTRefundInfo.AddError("GST Refund should be greater than 0.");
			}
			else if (AM_GSTRefund.Round(2) > Declaration.TotalGSTPayable.Round(2))
			{
				AM_GSTRefundInfo.AddError("GST Refund should be less or equal to Total GST Payable.");
			}
		}

		#endregion

		#region AM_DutyRefund

		public ZDecimal AM_DutyRefund
		{
			get { return fAM_DutyRefund; }
			set
			{
				SetNonPersistentPropertyValue(AM_DutyRefundInfo, ref fAM_DutyRefund, value);
				if (!IsValidationSuspended)
				{
					ValidateAM_DutyRefund();
				}
			}
		}
		ZDecimal fAM_DutyRefund;

		public ZPropertyInfo AM_DutyRefundInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AM_DutyRefund), "Duty Refund Amount");
			}
		}

		public bool AM_DutyRefund_ReadOnly => PartialRefundRequest;

		protected void ValidateAM_DutyRefund()
		{
			AM_DutyRefundInfo.ClearAllNotifications();

			if (FullRefundRequest && AM_DutyRefund != Declaration.TotalDutyPayable)
			{
				CompareValidation.CheckEqual(AM_DutyRefundInfo, Declaration.TotalDutyPayableInfo);
			}
		}

		#endregion

		#region AM_ExciseRefund

		public ZDecimal AM_ExciseRefund
		{
			get { return fAM_ExciseRefund; }
			set
			{
				SetNonPersistentPropertyValue(AM_ExciseRefundInfo, ref fAM_ExciseRefund, value);
				if (!IsValidationSuspended)
				{
					ValidateAM_ExciseRefund();
				}
			}
		}
		ZDecimal fAM_ExciseRefund;

		public ZPropertyInfo AM_ExciseRefundInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AM_ExciseRefund), "Excise Refund Amount");
			}
		}

		public bool AM_ExciseRefund_ReadOnly => PartialRefundRequest;

		protected void ValidateAM_ExciseRefund()
		{
			AM_ExciseRefundInfo.ClearAllNotifications();

			if (FullRefundRequest && AM_ExciseRefund != Declaration.TotalExcisePayable)
			{
				CompareValidation.CheckEqual(AM_ExciseRefundInfo, Declaration.TotalExcisePayableInfo);
			}
		}

		#endregion

		protected void ValidateRefundAmountEntered()
		{
			AM_GSTRefundInfo.ClearAllNotifications();
			if (AM_UpdateIndicator == UpdateIndicatorCodeList.Codes.PRG)
			{
				var refundRequested = AM_GSTRefund + AM_DutyRefund + AM_ExciseRefund;
				if (refundRequested == 0)
				{
					AM_GSTRefundInfo.AddError(PartialRefundRequiresValue);
				}
			}
			else if (AM_UpdateIndicator == UpdateIndicatorCodeList.Codes.PRS)
			{
				var refundRequested = ZDecimal.Zero;
				foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					refundRequested += (invoiceLine.SG_RefundForItemGSTAmount + invoiceLine.SG_RefundForItemCustomsDutyAmount + invoiceLine.SG_RefundForItemExciseAmount);
				}

				if (refundRequested == 0)
				{
					AM_UpdateIndicatorInfo.AddError(PartialRefundRequiresValue);
				}
			}
		}

		public const string PartialRefundRequiresValue = "A Partial Refund Request requires the requested refund amount to be entered into the relevant GST, Duty or Exise values";

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAM_UpdateIndicator();
			ValidateAM_RefundCode();
			ValidateAM_ReasonForRefund();
			if (PartialRefundRequest)
			{
				ValidateRefundAmountEntered();
			}
		}

		#region IAdditionalMessageInformation

		public override ZDecimal GSTRefundAmount
		{
			get { return AM_GSTRefund; }
		}

		public override ZDecimal DutyRefundAmount
		{
			get { return AM_DutyRefund; }
		}

		public override ZDecimal ExciseRefundAmount
		{
			get { return AM_ExciseRefund; }
		}

		protected override ZString UpdateIndicator
		{
			get { return AM_UpdateIndicator; }
		}

		protected override ZString RefundCode
		{
			get { return AM_RefundCode; }
		}

		protected override ZString ReasonForRefund
		{
			get { return AM_ReasonForRefund; }
		}

		#endregion
	}
}
