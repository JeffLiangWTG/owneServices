using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class JobDeclarationValidationFormalEntry : JobDeclarationValidation
	{
		public JobDeclarationValidationFormalEntry(JobDeclaration declaration)
			: base(declaration)
		{
		}

		#region Overrides where validation is not wanted in NZ
		protected override void CheckJE_TotalNoOfPacksPackType()
		{
			// This field is not used in the NZ Implementation. Validation Overridden.
		}
		#endregion

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();

			if (Declaration.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_OH_ImporterInfo);
				CheckJE_OH_ImporterIsValidZGuid();
				CheckCustomsClientCode(Declaration.JE_OH_ImporterInfo, Declaration.Importer);
				if (Declaration.IsTSWDeclaration)
				{
					if (Declaration.TSWSimplifiedMiscEntry && Declaration.IsAttachedToShipment)
					{
						if (!HasValidImporterContact)
						{
							Parent.JE_OH_ImporterInfo.AddMessageError(Res.GetString("20288710-8D45-44D6-8E15-1A5C9A0E7005", "Importer contact with phone, fax or email is mandatory for NZ Customs.\r\nPlease override the 'Consignee Documentary Address' values in the 'Addresses' tab for this Importer and enter the required contact name and contact details."));
						}
					}
					else if (!Declaration.IsECIWriteoff)
					{
						CheckHasAllocatedContact(Declaration.JE_OH_ImporterInfo, Declaration.Importer);
						CheckContactEmailMaxLength(Declaration.JE_OH_ImporterInfo, Declaration.Importer);
					}
				}
			}
			else if (Declaration.JE_OH_Importer.IsEmpty)
			{
				Declaration.JE_OH_ImporterInfo.AddWarning("You have not entered an importer. If you do here, this will be defaulted to Importer for a new invoice.");
			}
		}

		bool HasValidImporterContact
		{
			get
			{
				var hasValidContact = false;
				var importerDocAddress = Declaration.Shipment.DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress);
				if (importerDocAddress != null)
				{
					bool overriddenContactIsValid = importerDocAddress.E2_AddressOverride && !importerDocAddress.E2_Contact.IsEmpty;
					bool orgContactMethodProvided = importerDocAddress.E2_AddressOverride && !(importerDocAddress.E2_Email.IsEmpty && importerDocAddress.E2_Mobile.IsEmpty && importerDocAddress.E2_Phone.IsEmpty && importerDocAddress.E2_Fax.IsEmpty);
					hasValidContact = overriddenContactIsValid && orgContactMethodProvided;
				}

				return hasValidContact;
			}
		}

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();
			if (Declaration.IsExport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_OH_SupplierInfo);
				CheckJE_OH_SupplierIsValidZGuid();
				CheckCustomsClientCode(Declaration.JE_OH_SupplierInfo, Declaration.Supplier);
			}
			else if (Declaration.IsImport)
			{
				if (Declaration.JE_OH_Supplier.IsEmpty)
				{
					if (Declaration.IsTSWDeclaration)
					{
						Declaration.JE_OH_SupplierInfo.AddMessageError(TSWMessageError.SupplierIsRequired);
					}
					else
					{
						Declaration.JE_OH_SupplierInfo.AddWarning("You have not entered a supplier. If you do here, this will be defaulted to Supplier for a new invoice.");
					}
				}
				else
				{
					if (!Declaration.IsPrimaryIndustriesImportDeclaration)
					{
						CheckCustomsSupplierCode(Declaration.JE_OH_SupplierInfo);
					}

					if (!Declaration.IsECIWriteoff)
					{
						CheckContactEmailMaxLength(Declaration.JE_OH_SupplierInfo, Declaration.Supplier);
					}
				}
			}
		}

		protected override void CheckJE_MasterBill()
		{
			base.CheckJE_MasterBill();
			if (!Declaration.JE_MasterBill.IsEmpty && Declaration.JE_MasterBill.StartsWith(" ", StringComparison.Ordinal))
			{
				Declaration.JE_MasterBillInfo.AddMessageError(BillCannotHaveLeadingSpaces);
			}
		}

		protected override void CheckJE_HouseBill()
		{
			base.CheckJE_HouseBill();
			if (!Declaration.JE_HouseBill.IsEmpty && Declaration.JE_HouseBill.StartsWith(" ", StringComparison.Ordinal))
			{
				Declaration.JE_HouseBillInfo.AddMessageError(BillCannotHaveLeadingSpaces);
			}
		}
		public const string BillCannotHaveLeadingSpaces = "Bill number cannot have leading spaces.";

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			base.CheckJE_RL_NKPortOfLoading();
			MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_RL_NKPortOfLoadingInfo);
		}

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			if (Declaration.JE_RL_NKPortOfArrival != "VARIO" || !Declaration.IsPeriodicDrawback)
			{
				base.CheckJE_RL_NKPortOfArrival();
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_RL_NKPortOfArrivalInfo);
			}
		}

		protected override void CheckJE_MessageType()
		{
			base.CheckJE_MessageType();
			ValidateJE_EDITransmitDate();
			ValidateJE_SendMCDContainerQuarantineDeclaration();

			if (Declaration.JE_MessageType == JobMessageTypeList.Codes.Excise)
			{
				Declaration.JE_MessageTypeInfo.AddMessageError(MessageErorWhenExciseIsSelected);
			}
		}

		public const string MessageErorWhenExciseIsSelected = "Entry type \"EXC\" (Excise) is currently not supported. There has been no valid business case provided by our clients for the use of this message, so we do not support it. If you have a valid business requirement for submitting an Excise Entry, and have confirmed this with Customs, please contact CargoWise Support. We can only support this if Customs verify the requirement.";

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();
			ValidateJE_EDITransmitDate();
			ValidateJE_SendMCDContainerQuarantineDeclaration();
		}

		protected override void CheckJE_DateOfArrival()
		{
			base.CheckJE_DateOfArrival();
			if (Declaration.IsImport)
			{
				ValidateJE_EDITransmitDate();
			}
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			if (Declaration.IsExport)
			{
				ValidateJE_EDITransmitDate();
			}
		}

		protected override void CheckJE_EDITransmitDate()
		{
			base.CheckJE_EDITransmitDate();
			if (!Declaration.JE_EDITransmitDateFinalised)
			{
				if (Declaration.GetAppropriateCusEntryHeaderIfExists() == null || !Declaration.CusEntryHeader.CH_IsRestored)
				{
					EDITransmitDateManager.ValidationResult result = Declaration.TransmitDateManager.CheckEDITransmitDate();
					if (result != null)
					{
						if (result.IsError)
						{
							Declaration.JE_EDITransmitDateInfo.AddMessageError(result.MessageText);
						}
						else
						{
							Declaration.JE_EDITransmitDateInfo.AddWarning(result.MessageText);
						}
					}
				}
			}
		}

		protected override void CheckJE_TotalWeight()
		{
			base.CheckJE_TotalWeight();
			if (!Declaration.IsPost && Declaration.JE_TotalWeight <= 0m)
			{
				Declaration.JE_TotalWeightInfo.AddMessageError("Weight should be greater than zero.");
			}
		}

		protected override void CheckJE_TotalWeightUnit()
		{
			base.CheckJE_TotalWeightUnit();
			MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_TotalWeightUnitInfo);
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_TotalWeightUnitInfo, Declaration.Lookups.WeightUnitList);
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			base.CheckJE_RL_NKOrigin();
			if (Declaration.IsTSWDeclaration && Declaration.Origin == null)
			{
				Declaration.JE_RL_NKOriginInfo.AddMessageError(CountryOfExportRequired);
			}
		}
		public const string CountryOfExportRequired = "Must be transmitted to state the Country/Region of export of the shipment.";

		protected override void CheckJE_RL_NKFinalDestination()
		{
			if (Declaration.JE_RL_NKFinalDestination != "VARIO" || !Declaration.IsPeriodicDrawback)
			{
				base.CheckJE_RL_NKFinalDestination();
				if (Declaration.IsExport)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_RL_NKFinalDestinationInfo);
				}
			}
		}

		protected override void CheckJE_EntryAuthorisationDate()
		{
			base.CheckJE_EntryAuthorisationDate();
			if (Declaration.IsPeriodic || Declaration.IsExcise)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_EntryAuthorisationDateInfo);
			}
		}

		protected override void CheckJE_PaymentMethod()
		{
			base.CheckJE_PaymentMethod();

			if (Declaration.IsTSWDeclaration && Declaration.IsExport)
			{
				if (Declaration.IsDrawback && (Declaration.JE_PaymentMethod != PaymentMethodList.Codes.CashPaidByBroker && Declaration.JE_PaymentMethod != PaymentMethodList.Codes.CashPaidByClient && Declaration.JE_PaymentMethod != PaymentMethodList.Codes.ClientDeferred))
				{
					Declaration.JE_PaymentMethodInfo.AddMessageError(TSWMessageError.PaymentMethodInvalid);
				}
			}
			else if (Declaration.JE_PaymentMethod.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_PaymentMethodInfo, "Payment Method");
			}

			if (Declaration.IsTSWDeclaration && Declaration.IsTSWCustomsCleared)
			{
				if (Declaration.JE_PaymentMethodInfo.HasChanges)
				{
					Declaration.JE_PaymentMethodInfo.AddError(ErrorIfPaymentMethodChanged);
				}
			}
		}
		public const string ErrorIfPaymentMethodChanged = "Once cleared the payment method cannot be amended.";

		protected override void CheckJE_OriginalEntryNumber()
		{
			base.CheckJE_OriginalEntryNumber();
			if (Declaration.IsCompletion)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_OriginalEntryNumberInfo, "Original Entry Number");
			}
		}

		protected override void CheckJE_OriginalEntryType()
		{
			base.CheckJE_OriginalEntryType();
			if (Declaration.IsCompletion)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_OriginalEntryTypeInfo, "Original Entry Type");
				ListValidation.MessageErrorIfInvalidCode(Declaration.JE_OriginalEntryTypeInfo, Declaration.Lookups.OriginalEntryTypeList);
			}
		}

		void CheckCustomsSupplierCode(ZPropertyInfo guidInfo)
		{
			OrgHeader party = (OrgHeader)Declaration.Factory.Load(typeof(OrgHeader), (ZGuid)guidInfo.Value);
			if (party != null)
			{
				ZString localCustomsSupplierCode = party.LocalCustomsSupplierCode;
				if (localCustomsSupplierCode.IsEmpty)
				{
					if (!Declaration.IsSimplified)
					{
						guidInfo.AddMessageError(OrganisationCSCCustomsCodeMissing);
					}
				}
				else
				{
					NZCustomsCodeValidator cCDChecker = new NZCustomsCodeValidator(OrgCusCode.CodeTypes.SupplierCode);
					ZString supplierCodeErrors = cCDChecker.GetCasperCodeValidationErrors(localCustomsSupplierCode);
					if (!supplierCodeErrors.IsEmpty)
					{
						guidInfo.AddMessageError(OrganisationCSCCustomsCodeMissing);
						guidInfo.AddMessageError(supplierCodeErrors);
					}
				}
			}
		}

		void CheckCustomsClientCode(ZPropertyInfo guidInfo, OrgHeader party)
		{
			if (party != null)
			{
				ZString localCustomsClientCode = party.LocalCustomsClientCode;
				if (localCustomsClientCode.IsEmpty)
				{
					if (!Declaration.IsSimplified && !Declaration.IsTSW_IPI_Declaration)
					{
						guidInfo.AddMessageError(OrganisationCCDCustomsCodeMissing);
					}
				}
				else
				{
					NZCustomsCodeValidator cCDChecker = new NZCustomsCodeValidator(OrgCusCode.CodeTypes.CustomsClientCode);
					ZString clientCodeErrors = cCDChecker.GetCasperCodeValidationErrors(localCustomsClientCode);
					if (!clientCodeErrors.IsEmpty)
					{
						guidInfo.AddMessageError(OrganisationCCDCustomsCodeMissing);
						guidInfo.AddMessageError(clientCodeErrors);
					}
				}
			}
		}

		public const string OrganisationCCDCustomsCodeMissing = "This Importer must have a valid Customs Client Code 'CCD' configured.\r\nPlease edit the Importer Organization and add a 'CCD' code for NZ on the Details > Config > Registration Numbers / Codes tab.";
		public const string OrganisationCSCCustomsCodeMissing = "This Supplier must have a valid Customs Supplier Code 'CSC' configured.\r\nPlease edit the Supplier Organization and add a 'CSC' code for NZ on the Details > Config > Registration Numbers / Codes tab.";

		public static class TSWMessageError
		{
			public const string SupplierIsRequired = "Supplier is mandatory for TSW Import message.";
			public const string SupplierContactRequired = "At least one means of communication must be transmitted for the Supplier. Edit the organisation details to add telephone, mobile, fax or email details for this Supplier.";
			public const string PaymentMethodInvalid = "Payment method for Drawback entry in TSW can only be Cash or Client Deferred.";
		}

		void CheckContactEmailMaxLength(ZPropertyInfo propertyInfo, OrgHeader party)
		{
			var emailMaxLength = 50;
			OrgContact allocatedContact = null;

			if (party != null && !Declaration.TSWSimplifiedMiscEntry)
			{
				foreach (OrgContact contact in party.Contacts)
				{
					foreach (OrgContactAttribute contactAllocation in contact.Allocations)
					{
						if (contactAllocation.PC_Type == OrgConstants.ContactAllocationType.NZCustoms)
						{
							allocatedContact = contact;
							break;
						}
					}
				}

				var errmsg = Res.GetString("86908166-ffdb-48ab-80b1-31eb67b74c73", "Contact email address exceeds {0} characters.", emailMaxLength);

				if ((allocatedContact == null) || (allocatedContact.OC_Email.IsEmpty && allocatedContact.OC_Mobile.IsEmpty && allocatedContact.OC_Phone.IsEmpty && allocatedContact.OC_Fax.IsEmpty))
				{
					if (party.MainAddress.OA_Email.Length > emailMaxLength)
					{
						propertyInfo.AddMessageError(errmsg);
					}
				}
				else if (allocatedContact.Email.Length > emailMaxLength)
				{
					propertyInfo.AddMessageError(errmsg);
				}
			}
		}
	}
}
