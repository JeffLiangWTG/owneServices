using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.TW.Business
{
	public class TWMessageSendingValidation : MessageSendingValidation
	{
		protected TWMessageSendingValidation(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors, SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint, bool refreshValidation = true)
			: base(topLevelBusinessObjectForValidation, messageErrors, sendMessageWithErrorsSecurityCheckpoint, refreshValidation)
		{
		}

		public new static TWMessageSendingValidation New(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors, bool refreshValidation = true)
		{
			return new TWMessageSendingValidation(topLevelBusinessObjectForValidation, messageErrors, Env.Security.CustomsDeclarationSendWithMessageErrors, refreshValidation);
		}

		protected override MessageSendingNotificationCollection CheckBusinessObjectLevelValidationCore()
		{
			return CheckBusinessObjectLevelValidationCore(ErrorExistHeaderText, MessageErrorsExistHeaderText, string.Empty);
		}

		protected override MessageSendingNotificationCollection CheckBusinessObjectLevelValidationCore(string errorHeaderText, string messageErrorHeaderText, string confirmationQuestionText)
		{
			var result = base.CheckBusinessObjectLevelValidationCore(errorHeaderText, messageErrorHeaderText, confirmationQuestionText);

			var notifications = new List<Notification>();
			if (TopLevelBusinessObjectForValidation is JobDeclarationMessageSendingObjectParent parent && parent.ParentDeclaration?.EntryHeader is CusEntryHeader entryHeader)
			{
				var isImport = entryHeader.IsImport;
				if (isImport)
				{
					AddNotificationIfValueIsNegative(notifications, entryHeader.CH_TotalIMPFOBAmountInInvoiceCurrency, entryHeader.CH_TotalIMPFOBAmountInInvoiceCurrencyInfo);
				}
				else
				{
					AddNotificationIfValueIsNegative(notifications, entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency, entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrencyInfo);
				}
				AddNotificationIfValueIsNegative(notifications, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency, ResourceStringHelper.GetTotalInternationalFreightAmountInInvoiceCurrencyResString(isImport).Caption);
				AddNotificationIfValueIsNegative(notifications, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency, ResourceStringHelper.GetTotalInternationalInsuranceAmountInInvoiceCurrencyCaption(isImport).Caption);
				AddNotificationIfValueIsNegative(notifications, entryHeader.CH_TotalAdditionsInInvoiceCurrency, ResourceStringHelper.GetTotalAdditionsInInvoiceCurrencyCaption(isImport).Caption);
				AddNotificationIfValueIsNegative(notifications, entryHeader.CH_TotalDeductionsInInvoiceCurrency, ResourceStringHelper.GetTotalDeductionsInInvoiceCurrencyCaption(isImport).Caption);
				AddNotificationIfValueIsNegative(notifications, entryHeader.CH_TotalCustomsValueInInvoiceCurrency, ResourceStringHelper.GetTotalCustomsValueInInvoiceCurrencyCaption(isImport).Caption);
				AddNotificationIfValueIsNegative(notifications, entryHeader.CH_TotalCustomsValueInLocalCurrency, ResourceStringHelper.GetTotalCustomsValueInLocalCurrencyCaption(isImport).Caption);
				AddNotificationIfValueIsNegative(notifications, entryHeader.BusinessTaxBaseAmount, entryHeader.BusinessTaxBaseAmountInfo);
				AddNotificationIfValueIsNegative(notifications, entryHeader.TotalCashTaxAmount, entryHeader.TotalCashTaxAmountInfo);
				AddNotificationIfValueIsNegative(notifications, entryHeader.TotalNonCashTaxAmount, entryHeader.TotalNonCashTaxAmountInfo);
			}

			if (notifications.Any())
			{
				result.AddWarning(notifications.ToUniqueMessageListString());
			}
			return result;
		}

		void AddNotificationIfValueIsNegative(List<Notification> notifications, ZDecimal value, ZPropertyInfo info)
		{
			AddNotificationIfValueIsNegative(notifications, value, info.HumanReadableName);
		}

		void AddNotificationIfValueIsNegative(List<Notification> notifications, ZDecimal value, ZString caption)
		{
			if (value < 0)
			{
				notifications.Add(new Notification(CargoWise.EntityFramework.NotificationType.MessageError, MandatoryValidation.ValueCannotBeNegativeMessage(caption)));
			}
		}
	}
}
