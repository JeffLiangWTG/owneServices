using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public partial class WorkflowTriggerActionTypeConstants
	{
		public static bool IsStandardXml(string code)
		{
			return code == Codes.SendXML
				|| code == Codes.SendXMLSimplified
				|| code == Codes.SendNativeXML
				|| code == Codes.SendUniversalShipmentXML
				|| code == Codes.SendUniversalEventXML
				|| code == Codes.SendUniversalEventCollectionXML
				|| code == Codes.SendUniversalEventXMLWithEDoc
				|| code == Codes.SendUniversalTransactionXML
				|| code == Codes.SendUniversalScheduleXML
				|| code == Codes.SendUniversalActivityXML
				|| IsSendXMLWithAWB(code);
		}

		public static bool IsNotificationEmail(string code)
		{
			return code == Codes.NotificationEmail
				|| code == Codes.NotificationBodyEmail;
		}

		public static bool IsDebtorBalanceXml(string code)
		{
			return code == Codes.SendXMLDebtorBalance;
		}

		public static bool IsXmlWithJobFallback(string code)
		{
			return code == Codes.SendXMLWithJobFallback || code == Codes.SendXMLWithJobFallbackSimplified;
		}

		public static bool IsSimplifiedXml(string code)
		{
			return code == Codes.SendXMLSimplified || code == Codes.SendXMLWithJobFallbackSimplified;
		}

		public static bool IsNativeXml(string code)
		{
			return code == Codes.SendNativeXML;
		}

		public static bool IsXmlUniversalShipment(string code)
		{
			return code == Codes.SendUniversalShipmentXML;
		}

		public static bool IsXmlUniversalEvent(string code)
		{
			return code == Codes.SendUniversalEventXML;
		}

		public static bool IsXmlUniversalEventWithEdoc(string code)
		{
			return code == Codes.SendUniversalEventXMLWithEDoc;
		}

		public static bool IsUniversalXmlWithDeliveryContextSelector(string code)
		{
			return code == Codes.SendUniversalEventXML
				|| code == Codes.SendUniversalEventXMLWithEDoc;
		}

		public static bool IsXmlUniversalEventCollection(string code)
		{
			return code == Codes.SendUniversalEventCollectionXML;
		}

		public static bool IsSendXMLWithAWB(string code)
		{
			return code == Codes.SendXMLWithAWB;
		}

		public static bool IsSendEDocXml(string code)
		{
			return code == Codes.SendEDocXml;
		}

		public static bool IsEmailSendAction(string actionType)
		{
			return IsNotificationEmail(actionType) ||
				actionType == Codes.SendDescartesXml ||
				actionType == Codes.SendDocument ||
				actionType == Codes.SendXML ||
				actionType == Codes.SendXMLWithJobFallback ||
				actionType == Codes.SendXMLSimplified ||
				actionType == Codes.SendXMLWithJobFallbackSimplified ||
				actionType == Codes.SendXMLDebtorBalance ||
				actionType == Codes.SendEDocXml ||
				actionType == Codes.SendNativeXML ||
				ValidateForCustomsMessagingCodes.Contains(actionType) ||
				actionType == Codes.SendXMLWithAWB ||
				actionType == Codes.AssignStaffandEmail;
		}

		public static bool IsSendDocument(string actionType)
		{
			return actionType == Codes.SendDocument;
		}

		public static bool IsImportAPInvoicesFromOtherCompanies(string actionType)
		{
			return actionType == Codes.ImportAPInvoicesFromOtherCompanies;
		}

		public static bool IsPostOverseasAgentCharges(string actionType)
		{
			return actionType == Codes.PostOverseasAgentCharges;
		}

		public static bool IsRecognizeRevenue(string actionType)
		{
			return actionType == Codes.RecognizeRevenue;
		}

		public static bool IsPostConsolCostOnly(string actionType)
		{
			return actionType == Codes.PostConsolCostOnly;
		}

		public static bool IsPostAllRevenue(string actionType)
		{
			return actionType == Codes.PostAllRevenue;
		}

		public static bool IsPostAllSisterCompanyCharges(string actionType)
		{
			return actionType == Codes.PostAllSisterCompanyCharges;
		}

		public static bool IsPostLocalSisterCompanyChargesOnly(string actionType)
		{
			return actionType == Codes.PostLocalSisterCompanyChargesOnly;
		}

		public static bool IsPostAllCosts(string actionType)
		{
			return actionType == Codes.PostAllCosts;
		}

		public static bool IsCreateJobInvoiceHeader(string actionType)
		{
			return actionType == Codes.CreateJobInvoiceHeader;
		}

		public static bool IsSetField(string code)
		{
			return code == Codes.SetField || code == Codes.ImmediateFieldChange;
		}

		public static bool IsScheduleEvent(string code)
		{
			return code == Codes.ScheduleDelayedEvent;
		}

		public static bool IsAutoFinalisation(string code)
		{
			return code == Codes.AutoFinalisation;
		}

		public static IEnumerable<string> ValidateForCustomsMessagingCodes
		{
			get
			{
				yield return Codes.ValidateForCustomsMessaging;
				yield return Codes.ValidateForAUCargoMessaging;
			}
		}

		public static bool IsAutoPack(string actionType)
		{
			return actionType == Codes.AutoPack;
		}

		public static bool IsCreateTransportBooking(string actionType)
		{
			return actionType == Codes.CreateTransportBooking;
		}

		public static bool IsCreateTransportBookingContainer(string actionType)
		{
			return actionType == Codes.CreateTransportBookingContainer;
		}

		public static bool IsCreateTransportJob(string actionType)
		{
			return actionType == Codes.CreateTransportJob;
		}

		public static bool IsValidActionType(string actionType)
		{
			return new WorkflowTriggerActionTypeConstants().ToArray().Any(c => c.Code == actionType);
		}
	}
}
