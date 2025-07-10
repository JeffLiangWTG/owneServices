using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.CreditControl;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.Forwarding;
using ResString = Enterprise.MasterFiles.Business.ResString;

namespace Enterprise.MasterFiles.CreditControl.Business
{
	class DocumentDeliveryCreditControlHelper
	{
		internal DocumentDeliveryCreditControlHelper(DocumentDeliveryExtraRestrictions extraRestrictions, string documentDescription, BusinessObject businessObject, ZGuid menuItemPK)
		{
			ExtraRestrictions = extraRestrictions;
			DocumentDescription = documentDescription;
			BusinessObject = businessObject;
			MenuItemPK = menuItemPK;

			OrganisationsDescription = "";
			IsCreditOnHold = false;
			IsOverGlobalCreditLimit = false;
			AuthorizationLevelRequiredDueToCreditLimitExceeding = 0;
			HasUnpaidPIAInvoices = ZBool.False;
			DocumentLoginMessageBoxCallback = null;
			OrgsForCreditChecks = Array.Empty<OrgHeader>();
			OrgsForCreditOnHoldChecks = Array.Empty<OrgHeader>();
		}

		internal DocumentDeliveryCreditControlHelper(DocumentDeliveryExtraRestrictions extraRestrictions, bool hasUnpaidPIAInvoices, bool hasOutstandingCashAdvanceRequests,
			string documentDescription, string organisationsDescription, CustomMessageBoxCallback documentLoginMessageBoxCallback, OrgHeader[] organizationsForCreditChecks,
			BusinessObject businessObject, ZGuid menuItemPK, CreditCheckTracer traceCollector = null)
		{
			ExtraRestrictions = extraRestrictions;
			HasUnpaidPIAInvoices = hasUnpaidPIAInvoices;
			HasOutstandingCashAdvanceRequests = hasOutstandingCashAdvanceRequests;
			DocumentDescription = documentDescription;
			OrganisationsDescription = organisationsDescription;
			DocumentLoginMessageBoxCallback = documentLoginMessageBoxCallback;
			OrgsForCreditChecks = organizationsForCreditChecks == null ? Array.Empty<OrgHeader>() : organizationsForCreditChecks.Where(x => x != null).Distinct().ToArray();
			if (!IsExternalAccountingSystemUsed)
			{
				OrgsForCreditOnHoldChecks = OrgsForCreditChecks.Where(x => x.CreditChecker.IsCreditOnHold()).ToArray();
				IsCreditOnHold = OrgsForCreditOnHoldChecks.Any();
				IsOverGlobalCreditLimit = OrgsForCreditChecks.Any(x => x.CreditChecker.IsOverGlobalCreditLimit());
				AuthorizationLevelRequiredDueToCreditLimitExceeding = GetAuthorizationLevelRequiredDueToCreditLimitExceeding();
			}
			else
			{
				OrgsForCreditOnHoldChecks = Array.Empty<OrgHeader>();
			}
			BusinessObject = businessObject;
			MenuItemPK = menuItemPK;

			PrintTraceInfo(traceCollector);
		}

		void PrintTraceInfo(CreditCheckTracer traceCollector)
		{
			#region SuppressResourceStringsCheckRegion for logging

			if (traceCollector != null && traceCollector.IsEnabled())
			{
				var creditChecksOrgs = OrgsForCreditChecks.Any() ? string.Join(", ", OrgsForCreditChecks.Select(o => o.OH_Code).ToArray()) : string.Empty;
				var creditOnHoldChecksOrgs = OrgsForCreditOnHoldChecks.Any() ? string.Join(", ", OrgsForCreditOnHoldChecks.Select(o => o.OH_Code).ToArray()) : string.Empty;

				var diagnosticMessage = FormattableString.Invariant($"""
										{CriticalValidationInfoCollectorServiceKeyType.DocumentDeliveryCreditControlHelper_IfCollectionActivated}:
										.{nameof(ExtraRestrictions.IsDPSFreightMovementRestricted)}: {ExtraRestrictions.IsDPSFreightMovementRestricted}
										.{nameof(ExtraRestrictions.IsAviationSecurityFreightMovementRestricted)}: {ExtraRestrictions.IsAviationSecurityFreightMovementRestricted}
										.{nameof(HasUnpaidPIAInvoices)}: {HasUnpaidPIAInvoices}
										.{nameof(OrgsForCreditChecks)}: {creditChecksOrgs}
										.{nameof(IsExternalAccountingSystemUsed)}: {IsExternalAccountingSystemUsed}
										.{nameof(IsCreditOnHold)}: {IsCreditOnHold}
										.{nameof(OrgsForCreditOnHoldChecks)}: {creditOnHoldChecksOrgs}
										.{nameof(IsOverGlobalCreditLimit)}: {IsOverGlobalCreditLimit}
										.{nameof(AuthorizationLevelRequiredDueToCreditLimitExceeding)}: {AuthorizationLevelRequiredDueToCreditLimitExceeding}
										.{nameof(AuthorizationLevelRequiredDueToCreditCheck)}: {AuthorizationLevelRequiredDueToCreditCheck}
										.{nameof(HasCreditControlledDocumentsApproval)}: {HasCreditControlledDocumentsApproval}
										""");

				traceCollector.TraceInformation(() => diagnosticMessage);
			}

			#endregion
		}

		internal DocumentDeliveryCreditControlHelper(bool hasUnpaidPIAInvoices, bool hasOutstandingCashAdvanceRequests, OrgHeader organizationForCreditCheck)
		{
			HasUnpaidPIAInvoices = hasUnpaidPIAInvoices;
			HasOutstandingCashAdvanceRequests = hasOutstandingCashAdvanceRequests;
			OrgsForCreditChecks = organizationForCreditCheck == null ? Array.Empty<OrgHeader>() : new[] { organizationForCreditCheck };
			if (!IsExternalAccountingSystemUsed)
			{
				OrgsForCreditOnHoldChecks = OrgsForCreditChecks.Where(x => x.CreditChecker.IsCreditOnHold()).ToArray();
				IsCreditOnHold = OrgsForCreditOnHoldChecks.Any();
				AuthorizationLevelRequiredDueToCreditLimitExceeding = GetAuthorizationLevelRequiredDueToCreditLimitExceeding();
			}
			else
			{
				OrgsForCreditOnHoldChecks = Array.Empty<OrgHeader>();
			}
		}

		int GetAuthorizationLevelRequiredDueToCreditLimitExceeding()
		{
			var creditControllerOverrideThreshold = AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.Value;
			return creditControllerOverrideThreshold != null && creditControllerOverrideThreshold.Count != 0 ? AccountingMasterFilesUtils.GetMaxRequiredAuthorizationDueToExceedingCreditLimit(OrgsForCreditChecks) : 0;
		}

		#region GetSecurityLoginEventArgs

		public SecurityLoginEventArgsForDocumentApproval GetSecurityLoginEventArgs(string defaultApprovalRequestReasonDescription = null)
		{
			var securityCheckpoints = GetSecurityCheckPoints(out int[] authorizationRequired);
			var isAccountingRestricted = IsAccountingRestricted();

			var args = new SecurityLoginEventArgsForDocumentApproval(
				LoginPromptMessage,
				MessageToShowWhenNotAllowed,
				null,
				HasUnpaidPIAInvoices ? DocumentLoginMessageBoxCallback : null,
				BusinessObject,
				MenuItemPK,
				authorizationRequired,
				defaultApprovalRequestReasonDescription,
				ExtraRestrictions.IsDPSFreightMovementRestricted,
				ExtraRestrictions.IsAviationSecurityFreightMovementRestricted,
				isAccountingRestricted && IsExternalAccountingSystemUsed,
				isAccountingRestricted);

			if (ExtraRestrictions.IsAviationSecurityFreightMovementRestricted)
			{
				args.HideApprovalRequestButton = true;
			}

			args.SecurityCheckPoints.AddRange(securityCheckpoints);

			return args;
		}

		(int MaxAuthorization, bool IsCreditOnHoldCausingMaximumAuthorization) MaxAuthorizationRequired
		{
			get
			{
				if (maxAuthorizationRequired == null)
				{
					int maxAuthorization = 0;
					bool isCreditOnHoldCausingMaximumAuthorization = false;

					if (IsDocumentDeliveryRestricted)
					{
						if (IsExternalAccountingSystemUsed)
						{
							maxAuthorization = 3;
						}
						else
						{
							int authorizationForCreditCheck = AuthorizationLevelRequiredDueToCreditCheck;

							maxAuthorization = Math.Max(AuthorizationLevelRequiredDueToCreditLimitExceeding, authorizationForCreditCheck);

							int unPaidPIAInvoiceAuthorizationLevel = 0;
							if (HasUnpaidPIAInvoices)
							{
								unPaidPIAInvoiceAuthorizationLevel = 3; //For now there is only one level of authorization for PIA invoice. Later on there might be more than one.
							}

							maxAuthorization = Math.Max(maxAuthorization, unPaidPIAInvoiceAuthorizationLevel);

							int creditOnHoldAuthorizationLevel = 0;
							if (IsCreditOnHold)
							{
								creditOnHoldAuthorizationLevel = (OrgsForCreditOnHoldChecks == null || OrgsForCreditOnHoldChecks.Length == 0) ? 0 :
									ObjectFactory.Get<IAccounting>().UseWebServiceForCreditLimit ? 3 : OrgsForCreditOnHoldChecks.Max(org => org.CompanyData.GetCreditOnHoldAuthorizationLevel());
								if (creditOnHoldAuthorizationLevel >= maxAuthorization)
								{
									maxAuthorization = creditOnHoldAuthorizationLevel;
									isCreditOnHoldCausingMaximumAuthorization = true;
								}
							}
						}
					}

					maxAuthorizationRequired = (maxAuthorization, isCreditOnHoldCausingMaximumAuthorization);
				}
				return maxAuthorizationRequired.Value;
			}
		}
		(int, bool)? maxAuthorizationRequired;

		// It should be noted that if the check points are modified here, the CheckpointLookupKeys in ComplianceRiskApprovalRequestHandler also needs to be modified accordingly.
		void AddDpsOrComplianceSecurityCheckPoints(List<Func<SecurityCore, SecurityCheckpoint>> security)
		{
			if (ComplianceRiskHelper.IsFreightEnabledComplianceWise && ExtraRestrictions.IsRestrictionForConsol)
			{
				security.Add(s => s.ConsolidationsComplianceAllowOverrideFreightMovementRestrictions);
			}
			else if (ComplianceRiskHelper.IsFreightEnabledComplianceWise && ExtraRestrictions.IsRestrictionForShipment)
			{
				security.Add(s => s.ShipmentsComplianceAllowOverrideFreightMovementRestrictions);
			}
			else if (ComplianceRiskHelper.IsFreightEnabledComplianceWise && ExtraRestrictions.IsRestrictionForQuotedBooking)
			{
				security.Add(s => s.BookingsComplianceAllowOverrideFreightMovementRestrictions);
			}
			else if (ComplianceRiskHelper.IsCustomsEnabledComplianceWise && ExtraRestrictions.IsRestrictedForDeclaration)
			{
				security.Add(s => s.CustomsComplianceAllowOverrideFreightMovementRestrictions);
			}
			else
			{
				security.Add(s => s.OrgDeniedPartyScreeningOverrideFreightMvmtRestr);
			}
		}

		List<Func<SecurityCore, SecurityCheckpoint>> GetSecurityCheckPoints(out int[] authorizationRequired)
		{
			var result = new List<Func<SecurityCore, SecurityCheckpoint>>();
			authorizationRequired = Array.Empty<int>();

			if (IsAccountingRestricted())
			{
				var maxAuthorizationRequiredResult = MaxAuthorizationRequired;
				var maxAuthorization = maxAuthorizationRequiredResult.MaxAuthorization;
				var tempResult = new List<Func<SecurityCore, SecurityCheckpoint>>();

				var authorizationList = new List<int>();
				if (maxAuthorization > 0)
				{
					switch (maxAuthorization)
					{
						case 1:
							tempResult.Add(s => s.OnCreditHoldControllerFirstLevel);
							authorizationList.Add(1);

							if (maxAuthorizationRequiredResult.IsCreditOnHoldCausingMaximumAuthorization)
							{
								tempResult.Add(s => s.OnCreditHoldControllerSecondLevel);
								authorizationList.Add(2);

								tempResult.Add(s => s.OnCreditHoldControllerThirdLevel);
								authorizationList.Add(3);
							}
							break;
						case 2:
							tempResult.Add(s => s.OnCreditHoldControllerSecondLevel);
							authorizationList.Add(2);

							if (maxAuthorizationRequiredResult.IsCreditOnHoldCausingMaximumAuthorization)
							{
								tempResult.Add(s => s.OnCreditHoldControllerThirdLevel);
								authorizationList.Add(3);
							}
							break;
						case 3:
							tempResult.Add(s => s.OnCreditHoldControllerThirdLevel);
							authorizationList.Add(3);
							break;
					}
				}

				if (IsExternalAccountingSystemUsed)
				{
					tempResult.Clear(); //this is not requered for external system as we then propose user to enter credentials to override these securities, but this is not allowed for external system. Only security allowed to override by user when we use external sysstem is DPS security when it is applicable.
				}

				if (!tempResult.IsNullOrEmpty())
				{
					result.AddRange(tempResult);
				}

				authorizationRequired = authorizationList.ToArray();
			}

			if (ExtraRestrictions.IsDPSFreightMovementRestricted)
			{
				AddDpsOrComplianceSecurityCheckPoints(result);
			}

			if (ExtraRestrictions.IsAviationSecurityFreightMovementRestricted)
			{
				result.Add(s => s.OverrideRestrictionOfAviationSecurityFreightMovementRestricted);
			}

			return result;
		}

		#endregion

		#region GetMessage

		internal MultilingualString LoginPromptMessage
		{
			get { return loginPromptMessage ?? (loginPromptMessage = GetLoginPromptMessage()); }
		}
		MultilingualString loginPromptMessage;

		internal MultilingualString MessageToShowWhenNotAllowed
		{
			get { return messageToShowWhenNotAllowed ?? (messageToShowWhenNotAllowed = GetMessageToShowWhenNotAllowed()); }
		}
		MultilingualString messageToShowWhenNotAllowed;

		MultilingualString GetLoginPromptMessage()
		{
			if (!MessageToShowWhenNotAllowed.IsEmpty)
			{
				return MultilingualString.Join(string.Empty, MessageToShowWhenNotAllowed, (NoResString)System.Environment.NewLine, (NoResString)System.Environment.NewLine, GetContactMessage(), (NoResString)System.Environment.NewLine);
			}

			return (NoResString)string.Empty;
		}

		MultilingualString GetMessageToShowWhenNotAllowed()
		{
			var restrictedReasons = GetRestrictedReasons();

			if (restrictedReasons.Length > 0)
			{
				MultilingualString result = ResString.GetMultilingualString("12d85969-12ab-4b2b-8d43-7f28c87e8572", "Delivery of this {0} is restricted because:", DocumentDescription);
				if (BusinessObject is ICreditControlledNotificationTextProvider provider)
				{
					var headerText = provider.NotificationHeaderText;
					if (headerText != null && headerText.IsValid)
					{
						result = headerText;
					}
				}
				if (restrictedReasons.Length == 1)
				{
					result = MultilingualString.Join(string.Empty, result, (NoResString)System.Environment.NewLine, (NoResString)@"       ", restrictedReasons[0]);
				}
				else
				{
					var restrictionCount = 1;
					foreach (var item in restrictedReasons)
					{
						result = AppendLineWithDelimiter(result, item, restrictionCount++);
					}
				}

				return result;
			}

			return (NoResString)string.Empty;
		}

		MultilingualString[] GetRestrictedReasons()
		{
			var restrictedReasons = new List<MultilingualString>();

			if (IsAccountingRestricted())
			{
				if (IsExternalAccountingSystemUsed)
				{
					restrictedReasons.Add(ResString.GetMultilingualString("7A068358-4F80-48CB-AB98-845F16278B23", "External system request is required to evaluate credit status."));
				}
				else
				{
					if (IsCreditOnHold || AuthorizationLevelRequiredDueToCreditLimitExceeding > 0 || AuthorizationLevelRequiredDueToCreditCheck > 0 || HasOutstandingCashAdvanceRequests)
					{
						if ((AuthorizationLevelRequiredDueToCreditLimitExceeding > 0 && IsOverGlobalCreditLimit) ||
							(AuthorizationLevelRequiredDueToCreditCheck > 0 && AuthorizationLevelRequiredDueToGlobalCreditCheck))
						{
							if (ObjectFactory.Get<IAccounting>().IsIncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationEnabled(GlbCompany.CurrentCompany.PK.ToGuid()))
							{
								restrictedReasons.Add(ResString.GetMultilingualString("78fdc376-5697-4c7e-a929-b749aa987f70", @"The {0}
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Global Credit Limit, OR
	  c) Has been put on Credit Hold, OR
	  d) Has an outstanding Advance Payment Request.", OrganisationsDescription));
							}
							else  
							{
								restrictedReasons.Add(ResString.GetMultilingualString("17d316cd-f231-4f6e-9925-3fdc106d7bf8", @"The {0}
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Global Credit Limit, OR
	  c) Has been put on Credit Hold.", OrganisationsDescription));
							}
						}
						else
						{
							if (ObjectFactory.Get<IAccounting>().IsIncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationEnabled(GlbCompany.CurrentCompany.PK.ToGuid()))
							{
								restrictedReasons.Add(ResString.GetMultilingualString("b4ce8957-d65f-4dcf-b42d-5032d5376a16", @"The {0}
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold, OR
	  d) Has an outstanding Advance Payment Request.", OrganisationsDescription));
							}
							else
							{
								restrictedReasons.Add(ResString.GetMultilingualString("8c8bb111-9b14-4307-83da-4db868f4b387", @"The {0}
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.", OrganisationsDescription));
							}
						}
					}

					if (HasUnpaidPIAInvoices)
					{
						restrictedReasons.Add(ResString.GetMultilingualString("2be9fbd6-5fa7-445c-a110-96c5b55ff1a0", @"The {0}
	  a) Has unpaid PIA (Payment In Advance) invoices posted on this shipment, OR
	  b) Has no invoices posted on this shipment and has credit terms PIA (Payment In Advance).", OrganisationsDescription));
					}
				}
			}

			if (ExtraRestrictions.IsDPSFreightMovementRestricted)
			{
				restrictedReasons.Add(GetDpsOrComplianceFreightMovementRestrictedMessage());
			}

			if (ExtraRestrictions.IsAviationSecurityFreightMovementRestricted)
			{
				restrictedReasons.Add(SupplyChainSecurityConfiguration.AviationSecurityFreightMovementRestrictedErrorMessage);
			}

			return restrictedReasons.ToArray();
		}

		internal ResourceString GetDpsOrComplianceFreightMovementRestrictedMessage()
		{
			ResourceString restrictedMessage;
			if (IsComplianceFreightRestrictionModulesValid)
			{
				restrictedMessage = ResString.GetMultilingualString("4C841FDB-C41E-4787-99B0-F1A2887ED3FA", @"The Job Compliance Status is not Clear. Please see the Compliance Risk tab for more details.");
			}
			else if (IsComplianceFreightParentJobRestrictionModulesValid)
			{
				restrictedMessage = ResString.GetMultilingualString("A88B0007-C61C-440A-8CCC-853954D91C77", "The Job Compliance Status of a related job is not Clear.\r\n       Please see the Compliance Risk tab of the related job for more details.");
			}
			else
			{
				restrictedMessage = ResString.GetMultilingualString("79596428-06b8-4600-a339-5e6f82bffbc8", @"Screening Status is not Clear.");
			}
			return restrictedMessage;
		}

		bool IsComplianceFreightRestrictionModulesValid =>
			(ComplianceRiskHelper.IsFreightEnabledComplianceWise && (ExtraRestrictions.IsRestrictionForConsol || ExtraRestrictions.IsRestrictionForShipment || ExtraRestrictions.IsRestrictionForQuotedBooking))
			|| (ComplianceRiskHelper.IsCustomsEnabledComplianceWise && ExtraRestrictions.IsRestrictedForDeclaration);

		bool IsComplianceFreightParentJobRestrictionModulesValid =>
			ComplianceRiskHelper.IsFreightEnabledComplianceWise
			&& ExtraRestrictions.IsRestrictionForDtbBooking
			&& IsComplianceWiseEnabledForParentJob();

		bool IsComplianceWiseEnabledForParentJob()
		{
			return BusinessObject is not IJobHeaderParentCore { TableName: JobDeclarationSchema.Constants.TableName };
		}

		#region SupplyChainSecurityConfiguration

		public ISupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get { return supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>().GetConfiguration()); }
		}
		ISupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		#endregion

		MultilingualString AppendLineWithDelimiter(MultilingualString source, MultilingualString toAppend, int count)
		{
			MultilingualString result = source;
			if (count > 1)
			{
				result = MultilingualString.Join(string.Empty, result, (NoResString)System.Environment.NewLine, ResString.GetMultilingualString("9591783F-197D-441C-A5DB-09E6DB1A9CBD", "AND"));
			}
			result = MultilingualString.Join(string.Empty, result, (NoResString)System.Environment.NewLine, (NoResString)string.Format(CultureInfo.InvariantCulture, @"       {0} ) ", count), toAppend);
			return result;
		}

		MultilingualString GetContactMessage()
		{
			MultilingualString result = ResString.GetMultilingualString("415eb1ad-2fb4-4b34-b7df-f3aefb9ab6ac", "Do you wish to override it and deliver this {0}?", DocumentDescription);
			if (BusinessObject is ICreditControlledNotificationTextProvider provider)
			{
				var confirmationText = provider.NotificationConfirmationText;
				if (confirmationText != null && confirmationText.IsValid)
				{
					result = confirmationText;
				}
			}
			return result;
		}

		#endregion

		internal string[] GetEventReferences()
		{
			var result = new List<string>();

			var documentName = BusinessObject?.Factory.Load<StmMenuItem>(MenuItemPK)?.SU_MenuName;

			if (IsDocumentDeliveryRestricted)
			{
				if (IsExternalAccountingSystemUsed)
				{
					result.Add(Res.GetString("47F3813E-BE2C-47EA-ADC2-A4482C2C9F72", "External System Credit Status Overridden"));
				}
				else
				{
					if (HasUnpaidPIAInvoices)
					{
						result.Add(Res.GetString("58c167ec-a987-45bf-8cba-4b97220accd7", "Unpaid Payment in Advance Transactions"));
					}
					else if (IsCreditOnHold)
					{
						result.Add(Res.GetString("e3d5df4e-a60e-4b7f-b1fd-efd0439d724e", "Credit Hold Status Overridden"));
					}
					else if (AuthorizationLevelRequiredDueToCreditLimitExceeding > 0)
					{
						if (IsOverGlobalCreditLimit)
						{
							result.Add(Res.GetString("f068ae62-85fe-4001-a0c8-8f78ff05f223", "At or Over Global Credit Limit"));
						}
						else
						{
							result.Add(Res.GetString("f8230271-9b2a-478b-a3e7-7b0b1bf5f902", "At or Over Credit Limit"));
						}
					}
					else if (HasOutstandingCashAdvanceRequests)
					{
						result.Add(Res.GetString("a8b6e12e-4a6a-44db-8e19-fb5011e98b21", "Unpaid Advance Payment Request"));
					}
				}
			}

			if (ExtraRestrictions.IsDPSFreightMovementRestricted)
			{
				if (IsComplianceFreightRestrictionModulesValid)
				{
					result.Add(ZString.Format((NoResString)"|MST=Compliance Risk|Document Hold Status Overridden|{0}", documentName));
				}
				else
				{
					result.Add(ZString.Format((NoResString)"Denied Party Hold Status Overridden|{0}", documentName));
				}
			}

			if (ExtraRestrictions.IsAviationSecurityFreightMovementRestricted)
			{
				result.Add(Res.GetString("b53cbf6c-fc4f-4fa8-b417-48c01cc54943", "Aviation Security Hold Status Overridden"));
			}

			return result.ToArray();
		}

		internal bool IsRestricted()
		{
			return ExtraRestrictions.IsRestricted || IsAccountingRestricted();
		}

		bool IsAccountingRestricted()
		{
			return IsDocumentDeliveryRestricted && !HasCreditControlledDocumentsApproval;
		}

		bool IsDocumentDeliveryRestricted
		{
			get { return IsAccountingCheckRequires && (IsExternalAccountingSystemUsed || IsCreditOnHold || AuthorizationLevelRequiredDueToCreditLimitExceeding > 0 || HasUnpaidPIAInvoices || AuthorizationLevelRequiredDueToCreditCheck > 0 || HasOutstandingCashAdvanceRequests); }
		}

		bool IsExternalAccountingSystemUsed => AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.Value == AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveInExternalSystem.Code;

		bool IsAccountingCheckRequires => OrgsForCreditChecks.Any();

		internal ZString GetBreachReasonsForOrganisation()
		{
			var reasonBuilder = new ZStringBuilder();
			if (IsDocumentDeliveryRestricted)
			{
				if (IsExternalAccountingSystemUsed)
				{
					reasonBuilder.Append(Res.GetString("20330F88-734B-4447-BA78-652401B2B8B2", "External system request is required to evaluate credit status"));
				}
				else
				{
					if (IsCreditOnHold)
					{
						reasonBuilder.Append(Res.GetString("46c5287b-9323-4ffe-88d6-81a9d67d20a3", "Credit On Hold"));
					}
					if (AuthorizationLevelRequiredDueToCreditLimitExceeding > 0)
					{
						reasonBuilder.Append(Res.GetString("62a23059-0c51-49d3-a44f-a2d71a8feb41", "Over Credit Limit"));
					}
					if (AuthorizationLevelRequiredDueToCreditCheck > 0)
					{
						reasonBuilder.Append(Res.GetString("c2ecc891-5be0-400f-99a8-39481ceba8ed", "Overdue Transactions"));
					}
					if (HasUnpaidPIAInvoices)
					{
						reasonBuilder.Append(Res.GetString("776999a0-1b66-4921-8385-a9fe2a19bd62", "Credit Term PIA"));
					}
					if (HasOutstandingCashAdvanceRequests)
					{
						reasonBuilder.Append(Res.GetString("51300396-58d0-4650-86fe-5b516f97c65a", "Unpaid Advance Payment Request"));
					}
				}
			}

			return reasonBuilder.ToStringWithDelimiterBetweenAppends(",");
		}

		bool HasCreditControlledDocumentsApproval
		{
			get
			{
				if (hasCreditControlledDocumentsApproval == null)
				{
					var privilegeRequired = MaxAuthorizationRequired.MaxAuthorization;
					var approval = new BusinessObjectFactory().New<ICreditControlledDocumentsApproval>();
					approval.Initialize(BusinessObject, MenuItemPK, privilegeRequired != 0 ? new int[] { privilegeRequired } : Array.Empty<int>());

					hasCreditControlledDocumentsApproval = approval.RequestAlreadyApproved;
				}

				return hasCreditControlledDocumentsApproval.Value;
			}
		}
		bool? hasCreditControlledDocumentsApproval;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
		int AuthorizationLevelRequiredDueToCreditCheck
		{
			get
			{
				if (authorizationLevelRequiredDueToCreditCheck == null)
				{
					authorizationLevelRequiredDueToCreditCheck = 0;
					foreach (var org in OrgsForCreditChecks)
					{
						var orgAuthLevelRequired = org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToCreditCheck();
						if (orgAuthLevelRequired == AccountingMasterFilesUtils.GetAuthorisationRequirementWeight(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.MissingExRate))
						{
							if (!MissingExRateSentEmailHeaders.Contains(org.OH_Code))
							{
								new GlobalCreditControlledDocumentsApprovalMissingExRateEmail(org, BusinessObject, MenuItemPK).Send();
								MissingExRateSentEmailHeaders.TryAdd(org.OH_Code);
							}
							authorizationLevelRequiredDueToCreditCheck = AccountingMasterFilesUtils.GetAuthorisationRequirementWeight(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly);
							break;
						}
						if (orgAuthLevelRequired > authorizationLevelRequiredDueToCreditCheck)
						{
							authorizationLevelRequiredDueToCreditCheck = orgAuthLevelRequired;
						}
					}
				}

				return authorizationLevelRequiredDueToCreditCheck.Value;
			}
		}
		int? authorizationLevelRequiredDueToCreditCheck;

		static ConcurrentHashSet<string> MissingExRateSentEmailHeaders => missingExRateSentEmailHeaders ?? (missingExRateSentEmailHeaders = new ConcurrentHashSet<string>());
		static ConcurrentHashSet<string> missingExRateSentEmailHeaders;

		bool AuthorizationLevelRequiredDueToGlobalCreditCheck
		{
			get
			{
				if (authorizationLevelRequiredDueToGlobalCreditCheck == null)
				{
					authorizationLevelRequiredDueToGlobalCreditCheck = OrgsForCreditChecks.Any(org => org.CompanyData.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToGlobalCreditCheck);
				}

				return authorizationLevelRequiredDueToGlobalCreditCheck.Value;
			}
		}
		bool? authorizationLevelRequiredDueToGlobalCreditCheck;

		readonly bool IsCreditOnHold;
		readonly bool IsOverGlobalCreditLimit;
		readonly int AuthorizationLevelRequiredDueToCreditLimitExceeding;
		readonly bool HasUnpaidPIAInvoices;
		readonly bool HasOutstandingCashAdvanceRequests;
		readonly DocumentDeliveryExtraRestrictions ExtraRestrictions;
		readonly string DocumentDescription;
		readonly string OrganisationsDescription;
		readonly CustomMessageBoxCallback DocumentLoginMessageBoxCallback;
		readonly OrgHeader[] OrgsForCreditChecks;
		readonly OrgHeader[] OrgsForCreditOnHoldChecks;
		readonly BusinessObject BusinessObject;
		readonly ZGuid MenuItemPK;
	}
}
