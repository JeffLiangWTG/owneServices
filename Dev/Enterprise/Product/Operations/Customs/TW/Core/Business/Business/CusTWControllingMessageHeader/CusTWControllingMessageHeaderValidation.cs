using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class CusTWControllingMessageHeaderValidation : AutoCusTWControllingMessageHeaderValidation
	{
		public CusTWControllingMessageHeaderValidation(AutoCusTWControllingMessageHeader parent) : base(parent)
		{
		}

		protected new CusTWControllingMessageHeader Parent => (CusTWControllingMessageHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateBulkApplicationID();
				ValidateBulkPaymentID();
				ValidateCustomsMessageIdentifier();
			}
			ValidatePermitNumber();
		}

		public void ValidatePermitNumber()
		{
			ValidateCalculatedProperty(Parent.PermitNumberInfo);
		}

		protected virtual void CheckPermitNumber()
		{
			var parent = Parent;
			var permitNumber = parent.PermitNumber;
			if (parent.IsNX603)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.PermitNumberInfo);
			}
			if (!permitNumber.IsEmpty && permitNumber.Length != 14)
			{
				parent.PermitNumberInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.LengthForPermitNo);
			}
		}

		protected override void CheckTW1_RL_NKPortOfLoading()
		{
			base.CheckTW1_RL_NKPortOfLoading();
			var parent = Parent;
			if (parent.IsNX101)
			{
				if (!parent.IsZ99PortOfLoading)
				{
					ListValidation.MessageErrorIfInvalidCode(parent.TW1_RL_NKPortOfLoadingInfo);
				}

				if (parent.IsPortRequiredCertificateTypes)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.TW1_RL_NKPortOfLoadingInfo);
				}
			}
		}

		protected override void CheckTW1_RL_NKPortOfUnloading()
		{
			base.CheckTW1_RL_NKPortOfUnloading();

			var parent = Parent;
			if (parent.IsNX101)
			{
				if (!parent.IsZ99PortOfUnloading)
				{
					ListValidation.MessageErrorIfInvalidCode(parent.TW1_RL_NKPortOfUnloadingInfo);
				}

				if (parent.IsPortRequiredCertificateTypes)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.TW1_RL_NKPortOfUnloadingInfo);
				}
			}
		}

		protected override void CheckTW1_PortOfLoadingName()
		{
			base.CheckTW1_PortOfLoadingName();

			var parent = Parent;
			if (parent.IsPortNameRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.TW1_PortOfLoadingNameInfo);
			}
		}

		protected override void CheckTW1_PortOfUnloadingName()
		{
			base.CheckTW1_PortOfUnloadingName();

			var parent = Parent;
			if (parent.IsPortNameRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.TW1_PortOfUnloadingNameInfo);
			}
		}

		protected override void CheckTW1_BusinessType()
		{
			base.CheckTW1_BusinessType();
			var parent = Parent;
			if (parent.IsNX301_DN || parent.IsNX401)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.TW1_BusinessTypeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(parent.TW1_BusinessTypeInfo, parent.Lookups.BusinessTypeList);
		}

		protected override void CheckTW1_ProcessingUnit()
		{
			base.CheckTW1_ProcessingUnit();
			var parent = Parent;
			var targetInfo = parent.TW1_ProcessingUnitInfo;
			var processingUnit = parent.TW1_ProcessingUnit;
			if (parent.IsNX101 || parent.IsNX301 || parent.IsNX401 || parent.IsNX601 || parent.IsNX603)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}

			if (!processingUnit.IsEmpty)
			{
				var processingUnitBo = parent.ProcessingUnit;
				var controllingAgency = parent.TW1_ControllingAgency;
				if (parent.IsNX101)
				{
					ListValidation.MessageErrorIfInvalidCode(targetInfo);
				}
				else if (processingUnitBo == null)
				{
					targetInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
				}
				else if (!processingUnitBo.Attributes.HasAttribute(RefCusCodeListAttributeTypes.Codes.ControlAgency, controllingAgency))
				{
					targetInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.ProcessingUnitDoesNotBelongToControllingAgency(processingUnit, controllingAgency));
				}
			}
		}

		protected override void CheckTW1_PaymentMethod()
		{
			base.CheckTW1_PaymentMethod();
			var parent = Parent;
			if (parent.IsNX301 || parent.IsNX301_DN || parent.IsNX401 || parent.IsNX601 || parent.IsNX603)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.TW1_PaymentMethodInfo, parent.Lookups.CPT_116_PaymentMethodList);
			}
		}

		protected override void CheckTW1_AppointmentPeriod()
		{
			base.CheckTW1_AppointmentPeriod();
			var parent = Parent;
			var targetInfo = parent.TW1_AppointmentPeriodInfo;
			if (parent.TW1_AppointmentDate.IsValid)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
			if (parent.IsNX301_DN && parent.TW1_BusinessType == CPT_111_BusinessTypeList.Codes.ExemptionFromInspection && !parent.TW1_AppointmentPeriod.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("23223455-03F7-42AB-B2C2-E28A36CE7E75", "Appointment Period should be empty when Business Type is 'C'."));
			}
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
		}

		protected override void CheckTW1_AppointmentDate()
		{
			base.CheckTW1_AppointmentDate();
			var parent = Parent;
			if (parent.IsNX301_DN && parent.TW1_BusinessType == CPT_111_BusinessTypeList.Codes.ExemptionFromInspection && !parent.TW1_AppointmentDate.IsEmpty)
			{
				parent.TW1_AppointmentDateInfo.AddMessageError(Res.GetString("2DEB71BF-4EAF-412E-83F7-C9F5F993560D", "Appointment Date should be empty when Business Type is 'C'."));
			}
		}

		protected override void CheckTW1_PrePermitNumber()
		{
			base.CheckTW1_PrePermitNumber();
			var parent = Parent;
			var value = parent.TW1_PrePermitNumber;
			if (!value.IsEmpty && value.Length != CusTWControllingMessageHeader.Schema.TW1_PrePermitNumberMaxLength)
			{
				parent.TW1_PrePermitNumberInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.PreviousPermitNumberLengthMustbe14);
			}

			if (parent.IsNX101 && value.IsEmpty)
			{
				var certificateType = parent.TW1_CertificateType;
				if (certificateType == CertificateTypeList.Codes.Code17 || certificateType == CertificateTypeList.Codes.Code18)
				{
					parent.TW1_PrePermitNumberInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("DF97262D-D8E3-4C49-BD4E-FFF255827CE8", "Previous Permit No")));
				}
			}
		}

		protected override void CheckTW1_PreWineInspectionStatus()
		{
			base.CheckTW1_PreWineInspectionStatus();
			var parent = Parent;
			if (parent.IsNX301_DN)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.TW1_PreWineInspectionStatusInfo, parent.Lookups.PreWineInspectionStatusList);
			}
		}

		protected override void CheckTW1_Purpose()
		{
			base.CheckTW1_Purpose();
			var parent = Parent;
			if (parent.IsNX301_DN)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.TW1_PurposeInfo, parent.Lookups.PurposeList);
			}
		}

		protected override void CheckTW1_ControllingMessageType()
		{
			base.CheckTW1_ControllingMessageType();
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCode(parent.TW1_ControllingMessageTypeInfo, parent.Lookups.ControllingMessageTypeList);
		}

		protected override void CheckTW1_CertificateType()
		{
			base.CheckTW1_CertificateType();
			var parent = Parent;
			var targetInfo = parent.TW1_CertificateTypeInfo;
			if (parent.IsNX101OrX101)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo, parent.Lookups.CertificateTypeList);
			}
			if (parent.IsNX101)
			{
				var certificateType = parent.TW1_CertificateType;
				switch (certificateType)
				{
					case CertificateTypeList.Codes.Code8:
						ValidatePreviousDocumentNumberAndCertificateOfOriginNumber(targetInfo, parent);
						break;
					case CertificateTypeList.Codes.Code16:
						ValidatePreviousDocumentNumberAndCertificateOfOriginNumber(targetInfo, parent);
						break;
					case CertificateTypeList.Codes.Code17:
						ValidateCertificateOfOriginNumber(targetInfo, parent);
						ValidatePreviousDocumentNumber(targetInfo, parent);
						break;
				}
			}
			ValidateInvoiceHeader();
			ValidateInvoiceLine();
		}

		void ValidateInvoiceHeader()
		{
			var messageHeader = Parent;
			if (messageHeader.Declaration is JobDeclaration declaration)
			{
				foreach (var invoiceHeader in declaration.Invoices)
				{
					invoiceHeader.Validation.ValidateJZ_InvoiceNumber();
					invoiceHeader.Validation.ValidateJZ_InvoiceDate();
				}
			}
		}

		void ValidateInvoiceLine()
		{
			var messageHeader = Parent;
			if (messageHeader.Declaration is JobDeclaration declaration)
			{
				foreach (var invoiceLine in declaration.FilteredInvoiceLines)
				{
					invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				}
			}
		}

		void ValidatePreviousDocumentNumberAndCertificateOfOriginNumber(ZPropertyInfo propertyInfo, CusTWControllingMessageHeader header)
		{
			if (header.CertificateOfOrigins.Cast<CMCertificateOfOriginCusSupporting>().All(x => x.CSI_ReferenceNumber.IsEmpty)
				&& header.PreviousDocumentNumbers.Cast<PreviousDocumentNumberCusSupporting>().All(x => x.CSI_ReferenceNumber.IsEmpty))
			{
				propertyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("630DF82F-FEFC-4129-8CD1-B913C409A51F", "Previous Document Number")));
				propertyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("AB84CB34-7FDC-45C2-A917-D2A20A36635A", "Certificate of Origin Number")));
			}
		}

		void ValidateCertificateOfOriginNumber(ZPropertyInfo propertyInfo, CusTWControllingMessageHeader header)
		{
			if (header.CertificateOfOrigins.Cast<CMCertificateOfOriginCusSupporting>().All(x => x.CSI_ReferenceNumber.IsEmpty))
			{
				propertyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("AB84CB34-7FDC-45C2-A917-D2A20A36635A", "Certificate of Origin Number")));
			}
		}

		void ValidatePreviousDocumentNumber(ZPropertyInfo propertyInfo, CusTWControllingMessageHeader header)
		{
			if (header.PreviousDocumentNumbers.Cast<PreviousDocumentNumberCusSupporting>().All(x => x.CSI_ReferenceNumber.IsEmpty))
			{
				propertyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Res.GetString("630DF82F-FEFC-4129-8CD1-B913C409A51F", "Previous Document Number")));
			}
		}

		protected override void CheckTW1_IsSpecialApplication()
		{
			base.CheckTW1_IsSpecialApplication();
			var parent = Parent;
			if (!parent.TW1_IsSpecialApplication)
			{
				var info = parent.TW1_IsSpecialApplicationInfo;
				if (parent.TW1_OriginalQuantity > 5)
				{
					info.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.SpecialApplicationMustBeTickedForOriginalQuantity);
				}
				else if (parent.TW1_CopyQuantity > 10)
				{
					info.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.SpecialApplicationMustBeTickedForCopyQuantity);
				}
			}
		}

		protected override void CheckTW1_OriginalQuantity()
		{
			base.CheckTW1_OriginalQuantity();
			var parent = Parent;
			if (parent.IsCertificate15 && parent.TW1_OriginalQuantity > 1)
			{
				parent.TW1_OriginalQuantityInfo.AddMessageError(ValidationConstants.CusTWControllingMessageHeader.OriginalQuantityCannotMoreThanOne);
			}
		}

		protected override void CheckTW1_EUSteelProductNo()
		{
			base.CheckTW1_EUSteelProductNo();
			ListValidation.MessageErrorIfInvalidCode(Parent.TW1_EUSteelProductNoInfo);
		}

		protected override void CheckTW1_EUSteelProductPhase()
		{
			base.CheckTW1_EUSteelProductPhase();
			ListValidation.MessageErrorIfInvalidCode(Parent.TW1_EUSteelProductPhaseInfo);
		}

		protected override void CheckTW1_ManufacturerPrintingCode()
		{
			base.CheckTW1_ManufacturerPrintingCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.TW1_ManufacturerPrintingCodeInfo);
		}

		protected override void CheckTW1_PrintingCode()
		{
			base.CheckTW1_PrintingCode();
			var parent = Parent;
			var targetInfo = parent.TW1_PrintingCodeInfo;
			if (parent.IsNX101)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
		}

		protected override void CheckTW1_OH_Applicant()
		{
			base.CheckTW1_OH_Applicant();
			var parent = Parent;

			if (parent.IsNX101
				&& parent.DocAddresses.FindByDocAddressType(DocAddressType.Applicant) is TWJobDocAddress applicantAddress
				&& !applicantAddress.E2_AddressOverride
				&& applicantAddress.CompanyChineseName.IsEmpty)
			{
				parent.TW1_OH_ApplicantInfo.AddMessageError(Res.GetString("3dff624b-b23c-4cdd-90ad-a39b5ba6e1b5", "You have not entered an Applicant Local Company Name."));
			}
		}

		protected override void CheckTW1_OH_Supplier()
		{
			base.CheckTW1_OH_Supplier();
			var parent = Parent;

			if (parent.SupplierDocumentaryAddress.IsEmpty)
			{
				var notification = MandatoryValidation.YouHaveNotEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(parent.TW1_OH_SupplierInfo));
				parent.TW1_OH_SupplierInfo.AddMessageError(notification);
			}
		}

		protected override void CheckTW1_OH_Importer()
		{
			base.CheckTW1_OH_Importer();
			var parent = Parent;

			if (parent.ImporterDocumentaryAddress.IsEmpty)
			{
				var notification = MandatoryValidation.YouHaveNotEnteredMessage(MandatoryValidation.GetErrorFieldFromProperyInfo(parent.TW1_OH_ImporterInfo));
				parent.TW1_OH_ImporterInfo.AddMessageError(notification);
			}
		}

		protected override void CheckTW1_BeforeClearanceApplicationReason()
		{
			base.CheckTW1_BeforeClearanceApplicationReason();
			var parent = Parent;
			var targetInfo = parent.TW1_BeforeClearanceApplicationReasonInfo;

			if (parent.IsNX101 && !parent.IsCertificate15 && parent.TW1_BeforeClearanceApplicationReason.IsEmpty && parent.Declaration is JobDeclaration declaration && declaration.ClearanceStatus.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("F155EFC5-693C-4B7E-B004-009F72DCB3C6", "You have not selected a Goods Release Reason."));
			}

			ListValidation.MessageErrorIfInvalidCode(targetInfo);
		}

		protected override void CheckTW1_PortOfBulkCommodity()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.TW1_PortOfBulkCommodityInfo);
		}

		#region BulkApplicationID

		public void ValidateBulkApplicationID()
		{
			ValidateCalculatedProperty(Parent.BulkApplicationIDInfo);
		}

		protected void CheckBulkApplicationID()
		{
			var parent = Parent;
			var bulkApplicationID = parent.BulkApplicationID;
			if (parent.IsNX301_AX && !bulkApplicationID.IsEmpty && bulkApplicationID.Length != 14)
			{
				parent.BulkApplicationIDInfo.AddMessageError(Res.GetString("7A9C7332-F13B-4448-83CD-8CBDCA5021BF", "Bulk Application ID must consist 14 alphanumeric characters."));
			}
		}

		#endregion

		#region BulkPaymentID

		public void ValidateBulkPaymentID()
		{
			ValidateCalculatedProperty(Parent.BulkPaymentIDInfo);
		}

		protected void CheckBulkPaymentID()
		{
			var parent = Parent;
			var bulkPaymentID = parent.BulkPaymentID;
			if (parent.IsNX301_AX && !bulkPaymentID.IsEmpty && bulkPaymentID.Length != 14)
			{
				parent.BulkPaymentIDInfo.AddMessageError(Res.GetString("65F0A7E5-E45C-423A-AA72-87F0C2CC5628", "Bulk Payment ID must consist 14 alphanumeric characters."));
			}
		}

		#endregion

		#region CustomsMessageIdentifier

		public void ValidateCustomsMessageIdentifier()
		{
			ValidateCalculatedProperty(Parent.CustomsMessageIdentifierInfo);
		}

		protected void CheckCustomsMessageIdentifier()
		{
			var customsMessageIdentifier = Parent.CustomsMessageIdentifier;
			if (!customsMessageIdentifier.IsEmpty && customsMessageIdentifier.Length != 19)
			{
				Parent.CustomsMessageIdentifierInfo.AddMessageError(Res.GetString("1FF616BD-D892-45B5-9FA0-3254A56A6F86", "Customs Message Identifier must consist 19 alphanumeric characters"));
			}
		}

		#endregion
	}
}
