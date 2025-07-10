using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.MasterFiles.CreditControl.Business
{
	public class DocumentDeliveryCreditControlManager : IDocumentDeliveryCreditControlManager
	{
		public bool IsCustomsSubmission { get; set; }

		public ZString GetDocumentDeliveryStatusForCreditManagement(BusinessObject businessObject, string documentDescription, ZGuid menuItemPK, bool creditCheckEnabled = true, string documentDirection = "")
		{
			var result = ZString.Empty;
			var documentDelivery = businessObject as ICreditControlledDocumentDelivery;
			if (documentDelivery != null)
			{
				if (documentDelivery.IsDPSFreightMovementRestricted)
				{
					var complianceDocumentDeliveryStrategy = GetComplianceDocumentDeliveryStrategyResult(documentDelivery);

					if (complianceDocumentDeliveryStrategy.DocumentDeliveryResponse is DocumentDeliveryResultForComplianceWorkflow.StopDocumentDelivery)
					{
						return MultilingualString.Join(" ", ResString.GetMultilingualString("587DA14A-C642-40E3-8CFB-6FA686139774", "Document Delivery"), SecurityLogin.CancelledText);
					}
				}

				var extraRestrictions = new DocumentDeliveryExtraRestrictions(
					documentDelivery.IsDPSFreightMovementRestricted,
					documentDelivery.IsAviationSecurityFreightMovementRestricted && documentDirection != nameof(DocumentDirection.ARV),
					businessObject.GetType());

				var traceBuilder = new CreditCheckTracer();
				var hasOutstandingCashAdvanceRequests = CheckOutstandingCashAdvanceRequests(documentDelivery);

				if (creditCheckEnabled)
				{
					documentDeliveryCreditControlHelper = new DocumentDeliveryCreditControlHelper(
						extraRestrictions,
						new UnpaidPaymentInAdvanceTransactionFilter(documentDelivery).HasUnPaidPIAInvoices,
						hasOutstandingCashAdvanceRequests,
						documentDescription,
						documentDelivery.DescriptionOfOrganisationBeingCheckedForCredit,
						documentDelivery.DocumentLoginMessageBoxCallback,
						documentDelivery.OrganisationsForCreditChecks,
						businessObject,
						menuItemPK,
						traceBuilder);
				}
				else
				{
					documentDeliveryCreditControlHelper = new DocumentDeliveryCreditControlHelper(extraRestrictions, documentDescription, businessObject, menuItemPK);
				}

				var isRestricted = documentDeliveryCreditControlHelper.IsRestricted();
				var isAllowedToProceed = false;
				if (isRestricted)
				{
					var args = documentDeliveryCreditControlHelper.GetSecurityLoginEventArgs();
					args.IsCustomsSubmission = IsCustomsSubmission;
					documentDelivery.RaiseOnGetDocumentLogin(args);
					isAllowedToProceed = args.IsAllowedToProceed;
					if (isAllowedToProceed)
					{
						Array.ForEach(documentDeliveryCreditControlHelper.GetEventReferences(), eventReference => LogEvent(documentDelivery, Events.HoldStatusOverride, eventReference, args));
					}
					else
					{
						result = args.Message;
					}
				}

				PrintTraceInfo(traceBuilder, extraRestrictions, isRestricted, isAllowedToProceed);
			}

			return result;

			void PrintTraceInfo(CreditCheckTracer traceBuilder, DocumentDeliveryExtraRestrictions extraRestrictions, bool isRestricted, bool isAllowedToProceed)
			{
				#region SuppressResourceStringsCheckRegion for logging

				if (traceBuilder != null && traceBuilder.IsEnabled())
				{
					var diagnosticMessage = FormattableString.Invariant($"""
																		CreditCheckDiagnosticInfo_DocumentDelivery_2
																		Collected against BusinessObject: {businessObject.GetType().ToString()} PK: {businessObject.PK} ->

																		{CriticalValidationInfoCollectorServiceKeyType.DocumentDeliveryCreditControlManager_IfCollectionActivated}:
																		.{nameof(businessObject)} type: {businessObject.GetType().ToString()}
																		.{nameof(extraRestrictions.IsDPSFreightMovementRestricted)}: {extraRestrictions.IsDPSFreightMovementRestricted}
																		.{nameof(extraRestrictions.IsAviationSecurityFreightMovementRestricted)}: {extraRestrictions.IsAviationSecurityFreightMovementRestricted}
																		.{nameof(creditCheckEnabled)}: {creditCheckEnabled}
																		.{nameof(documentDeliveryCreditControlHelper.IsRestricted)}: {isRestricted}
																		.args.IsAllowedToProceed: {isAllowedToProceed}
																		""");

					traceBuilder.TraceInformation(() => diagnosticMessage);
				}

				#endregion
			}
		}

		ComplianceDocumentDeliveryStrategy GetComplianceDocumentDeliveryStrategyResult(ICreditControlledDocumentDelivery creditControlledDocumentDelivery)
		{
			var complianceDocumentDeliveryStrategy = new ComplianceDocumentDeliveryStrategy();

			if (creditControlledDocumentDelivery is IComplianceItemRiskStatusProvider complianceRiskStatusProvider
				&& complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded != null)
			{
				complianceDocumentDeliveryStrategy.DocumentDeliveryResponse = complianceRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded.Invoke();
			}

			return complianceDocumentDeliveryStrategy;
		}

		public bool ShouldStopDelivery(BusinessObject businessObject) => GetComplianceDocumentDeliveryStrategyResult(businessObject as ICreditControlledDocumentDelivery).DocumentDeliveryResponse == DocumentDeliveryResultForComplianceWorkflow.StopDocumentDelivery;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Reference string")]
		void AddComplianceDocumentHoldStatusEventLogIfNeeded(BusinessObject bizO, ZString eventUser, ZString eventReference)
		{
			const string reference = "|MST=Compliance Risk|Document Hold Status Overridden|";
			if (eventReference.StartsWith(reference) && bizO is IComplianceItemRiskStatusProvider provider)
			{
				var documentName = eventReference.ReplaceIgnoringCase(reference, string.Empty);
				ObjectFactory.Get<IComplianceRiskStatusSupporter>().AddComplianceDocumentHoldStatusEventLog(provider, eventUser, documentName);
			}
		}

		bool CheckOutstandingCashAdvanceRequests(ICreditControlledDocumentDelivery creditControlledDocumentDelivery)
		{
			return ObjectFactory.Get<IAccounting>().IsIncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationEnabled(GlbCompany.CurrentCompany.PK.ToGuid()) &&
				OutstandingCashAdvanceFilter.HasOutstandingARCashAdvances(creditControlledDocumentDelivery.JobNumber);
		}

		void LogEvent(ICreditControlledDocumentDelivery bizO, Event evt, ZString evtRef, SecurityLoginEventArgs args)
		{
			StmALog log = null;
			BusinessObjectFactory factory = null;
			var biz = bizO as BusinessObject;
			if (!biz.IsInDatabase)
			{
				log = bizO.Logs.AddNew(evt, evtRef);
				factory = biz.Factory;
			}
			else
			{
				factory = new BusinessObjectFactory();
				biz = factory.Load(biz.GetType(), biz.PK);
				log = biz.GetLogs().AddNew(evt, evtRef);
			}

			var authorisingUser = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, args.AuthorisingStaffLogin));
			if (authorisingUser != null)
			{
				log.SL_GS_NKUser = authorisingUser.GS_Code;
			}

			AddComplianceDocumentHoldStatusEventLogIfNeeded(biz, log.SL_GS_NKUser, evtRef);

			ZExceptionReporting.ProcessWithSaveExceptionHandling(() => factory.Save(), null, true);
		}

		public SecurityLoginEventArgsForDocumentApproval GetSecurityLoginEventArgs(string defaultApprovalRequestReasonDescription = null)
		{
			return documentDeliveryCreditControlHelper.GetSecurityLoginEventArgs(defaultApprovalRequestReasonDescription);
		}

		public Dictionary<OrgHeader, ZString> GetOrganisationsInBreachAndTheirBreachReasons(BusinessObject businessObject)
		{
			var result = new Dictionary<OrgHeader, ZString>();
			var documentDelivery = businessObject as ICreditControlledDocumentDelivery;
			if (documentDelivery != null)
			{
				var organisationsForCreditCheck = documentDelivery.OrganisationsForCreditChecks;
				if (organisationsForCreditCheck != null)
				{
					foreach (var organisation in organisationsForCreditCheck.Where(x => x != null).Distinct())
					{
						documentDeliveryCreditControlHelper = new DocumentDeliveryCreditControlHelper(
						new UnpaidPaymentInAdvanceTransactionFilter(documentDelivery).HasUnPaidPIAInvoicesForOrg(organisation.PK),
						OutstandingCashAdvanceFilter.HasOutstandingARCashAdvancesForOrg(documentDelivery.JobNumber, organisation.PK),
						organisation);
						var reasons = documentDeliveryCreditControlHelper.GetBreachReasonsForOrganisation();
						if (!reasons.IsEmpty)
						{
							result.Add(organisation, reasons);
						}
					}
				}
			}
			return result;
		}

		public ZString ErrorMessageText
		{
			get { return documentDeliveryCreditControlHelper.MessageToShowWhenNotAllowed; }
		}

		public ZString LoginMessageText
		{
			get { return documentDeliveryCreditControlHelper.LoginPromptMessage; }
		}

		DocumentDeliveryCreditControlHelper documentDeliveryCreditControlHelper;
	}
}
