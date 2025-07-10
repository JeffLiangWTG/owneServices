using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public abstract class NX201_01MessageSendingObjectParentValidation : LicensingMessageSendingObjectParentValidation
	{
		public NX201_01MessageSendingObjectParentValidation(LicensingMessageSendingObjectParent parent, JobDeclaration declaration, IEnumerable<INotification> messageErrors, SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint, bool refreshValidation = true)
			: base(parent, declaration, messageErrors, sendMessageWithErrorsSecurityCheckpoint, refreshValidation)
		{
		}

		protected override MessageSendingNotificationCollection CheckBusinessObjectLevelValidationCore()
		{
			var result = new MessageSendingNotificationCollection();
			foreach (LicensingMessageSendingObject sendingObject in Parent.SendingObjectsCollection)
			{
				if (sendingObject.ShouldSend)
				{
					var header = sendingObject.Header;

					result.AddRange(GetDeclarationgNotificationCollection(header, (NoResString)"Declaration"));
					var controllingMessageHeaderLinkInvoiceLines = header.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().Where(c => c.Link);
					var invoiceHeaders = controllingMessageHeaderLinkInvoiceLines.Select(c => c.Invoiceline.InvoiceHeader).Distinct();
					foreach (var invoiceHeader in invoiceHeaders)
					{
						var prefix = $"Invoice Header {invoiceHeader.JZ_InvoiceNumber}";
						result.AddRange(GetInvoiceHeaderNotificationCollection(invoiceHeader, prefix));
					}

					foreach (var controllingMessageHeaderLinkInvoiceLine in controllingMessageHeaderLinkInvoiceLines)
					{
						var invoiceLine = controllingMessageHeaderLinkInvoiceLine.Invoiceline;
						var prefix = $"Invoice Line {invoiceLine.JI_Calc_Invoice} {invoiceLine.JI_LineNo}";
						result.AddRange(GetInvoiceLineNotificationCollection(invoiceLine, prefix));
					}

					var sendingObjectPrefix = $"Licensing {header.TW1_FunctionalReferenceId}";
					result.AddRange(GetLicensingMessageSendingObjectNotificationCollection(sendingObject, header, sendingObjectPrefix));
				}
			}

			return result;
		}

		protected virtual MessageSendingNotificationCollection GetDeclarationgNotificationCollection(CusTWControllingMessageHeader header, string prefix)
		{
			var result = new MessageSendingNotificationCollection();
			var declaration = header.Declaration;
			if (header.TW1_BusinessType == NX902_TypeOfApplicationCodeList.Codes._2 && (declaration?.JE_RL_NKOrigin.IsEmpty ?? false))
			{
				AddErrorIfEmpty(result, Declaration.Validation, Declaration.JE_RL_NKOriginInfo, prefix, columnName: (NoResString)"Port of Origin");
			}

			var declarantAddress = declaration.DeclarantAddress;
			if (declarantAddress != null)
			{
				if (declarantAddress.Language != Core.SharedConstants.Languages.ChineseTraditional)
				{
					if (!declarantAddress.HasCompanyNameOfLanguage(Core.SharedConstants.Languages.ChineseTraditional))
					{
						result.AddError($"{prefix}: {MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.Declaration.Declarant.LocalCompanyNameResString)}");
					}
					if (!declarantAddress.HasAddressOfLanguage(Core.SharedConstants.Languages.ChineseTraditional))
					{
						result.AddError($"{prefix}: {MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.Declaration.Declarant.LocalAddressResString)}");
					}
				}
				if (declarantAddress.OA_Phone.IsEmpty)
				{
					result.AddError($"{prefix}: {MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.Declaration.Declarant.PhoneResString)}");
				}
				if (!declarantAddress.Header.ContactsActive.Any())
				{
					result.AddError($"{prefix}: {MandatoryValidation.YouHaveNotEnteredMessage(ValidationConstants.Declaration.Declarant.ContactNameResString)}");
				}
			}

			if (declarantAddress?.Header is OrgHeader orgHeader && !OrgHeaderHelper.CheckHasCusCode(orgHeader, Core.Constants.CountryCodes.Taiwan, new string[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID }))
			{
				result.AddError($"{prefix}: {ValidationConstants.Declaration.Declarant.AValidIDIsReruired}");
			}

			return result;
		}

		protected virtual MessageSendingNotificationCollection GetInvoiceHeaderNotificationCollection(JobComInvoiceHeader invoiceHeader, string prefix)
		{
			var result = new MessageSendingNotificationCollection();
			AddErrorIfEmptyOrNotInList(result, invoiceHeader.Validation, invoiceHeader.JZ_IncoTermInfo, invoiceHeader.Lookups.JZ_IncoTerm_List, prefix, errorMessageForEmpty: (NoResString)"Please enter an Incoterm.");
			AddErrorIfEmptyOrNotInList(result, invoiceHeader.Validation, invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, invoiceHeader.Lookups.CurrencyList.Select(c => c.RX_Code), prefix, errorMessageForEmpty: (NoResString)"Please enter a Currency.", errorMessageForNotInList: (NoResString)"Enter a valid Curr..");
			return result;
		}

		protected virtual MessageSendingNotificationCollection GetInvoiceLineNotificationCollection(JobComInvoiceLine invoiceLine, string prefix)
		{
			var result = new MessageSendingNotificationCollection();
			AddErrorIfEmpty(result, invoiceLine.Validation, invoiceLine.JI_DescriptionInfo, prefix, columnName: (NoResString)"English Name");
			if (!AddErrorIfEmpty(result, invoiceLine.Validation, invoiceLine.JI_TariffInfo, prefix, errorMessage: ValidationConstants.InvoiceLine.TariffCodeIsEmpty))
			{
				var errorMessageForNotInList = ValidationConstants.InvoiceLine.TariffCodeIsNotValid;
				if (invoiceLine.UniversalTariff == null)
				{
					AddErrorOnProperty(invoiceLine.Validation, invoiceLine.JI_TariffInfo, errorMessageForNotInList);
					result.AddError($"{prefix}: {errorMessageForNotInList}");
				}
				else
				{
					var tariffList = new List<ZString>();
					foreach (TariffView item in invoiceLine.UniversalTariff)
					{
						tariffList.Add(item.ZZ1_TariffCode);
					}
					AddErrorIfEmptyOrNotInList(result, invoiceLine.Validation, invoiceLine.JI_TariffInfo, tariffList, prefix, errorMessageForEmpty: ValidationConstants.InvoiceLine.TariffCodeIsEmpty, errorMessageForNotInList: errorMessageForNotInList);
				}
			}
			AddErrorIfNotGreaterThan0(result, invoiceLine.Validation, invoiceLine.JI_InvoiceQuantityInfo, prefix);
			AddErrorIfEmptyOrNotInList(result, invoiceLine.Validation, invoiceLine.JI_InvoiceUQInfo, invoiceLine.Lookups.InvoiceUQList, prefix);
			AddErrorIfNotGreaterThan0(result, invoiceLine.Validation, invoiceLine.JI_LinePriceInfo, prefix);
			return result;
		}

		protected virtual MessageSendingNotificationCollection GetLicensingMessageSendingObjectNotificationCollection(LicensingMessageSendingObject sendingObject, CusTWControllingMessageHeader header, string prefix)
		{
			var result = new MessageSendingNotificationCollection();
			AddErrorIfEmptyOrNotInList(result, header.Validation, header.TW1_ProcessingUnitInfo, header.Lookups.ProcessingUnitList, prefix);
			AddErrorIfEmptyOrNotInList(result, header.Validation, header.TW1_BusinessTypeInfo, header.Lookups.BusinessTypeList, prefix);

			var applicantDocumentaryAddress = header.ApplicantDocumentaryAddress;
			AddErrorIfEmpty(result, applicantDocumentaryAddress.Validation, applicantDocumentaryAddress.IDCodeInfo, prefix, columnName: (NoResString)"Applicant ID");
			AddErrorIfEmpty(result, applicantDocumentaryAddress.Validation, applicantDocumentaryAddress.E2_ContactInfo, prefix, columnName: (NoResString)"Applicant Contact Name");
			AddErrorIfEmpty(result, applicantDocumentaryAddress.Validation, applicantDocumentaryAddress.E2_EmailInfo, prefix, columnName: (NoResString)"Applicant Email Address");
			AddErrorIfEmpty(result, applicantDocumentaryAddress.Validation, applicantDocumentaryAddress.E2_PhoneInfo, prefix, columnName: (NoResString)"Applicant Telephone Number");
			if (applicantDocumentaryAddress.E2_AddressOverride)
			{
				var localAddress = applicantDocumentaryAddress.LocalAddress;
				if (localAddress == null)
				{
					var errorMessage = MandatoryValidation.YouHaveNotEnteredMessage((NoResString)"Applicant Local Address Information");
					AddErrorOnProperty(applicantDocumentaryAddress.Validation, applicantDocumentaryAddress.E2_AddressOverrideInfo, errorMessage);
					result.AddError($"{prefix}: {errorMessage}");
				}
				else
				{
					AddErrorIfEmpty(result, localAddress.Validation, localAddress.E2_CompanyNameInfo, prefix, columnName: (NoResString)"Applicant Local Name");
					AddErrorIfEmpty(result, localAddress.Validation, localAddress.E2_Address1Info, prefix, columnName: (NoResString)"Applicant Local Address");
				}
			}
			AddErrorIfEmptyOrNotInList(result, sendingObject.Validation, sendingObject.ActionInfo, sendingObject.Lookups.ActionList, prefix);
			return result;
		}
	}
}
