using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using IConsolidateLVXDeclarationSupporter = Enterprise.Integration.Customs.CA.IConsolidateLVXDeclarationSupporter;
using IScheduleB3MessageSupporter = Enterprise.Integration.Customs.CA.IScheduleB3MessageSupporter;
using ISubmitAVSQuerySupporter = Enterprise.Integration.Customs.CA.ISubmitAVSQuerySupporter;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;
using Res = Enterprise.Customs.DataTransfer.Res;

namespace Enterprise.Customs.Business
{
#if DEBUG
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
#endif
	public class JobDeclarationWorkflowDescriptor : WorkflowDescriptor
		, IWorkflowParentWithLines
	{
		#region ID / Description

		public override string Code
		{
			get { return JobInvoicingConsumerTypes.Brokerage.Code; }
		}

		public override IMultilingualString Description
		{
			get { return JobInvoicingConsumerTypes.Brokerage.MultilingualDescription; }
		}

		public override ZString MilestoneTemplateHintCaption
		{
			get { return ""; }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				var list = new List<ProcessTemplateSubType>(base.SubTypeInformation)
				{
					new ProcessTemplateSubType(Res.GetString("d950b7b1-2691-4ee2-bb26-849b0d96bbd7", "Transport Mode"), GetTransportModeList(this)),
					new ProcessTemplateSubType(Res.GetString("6650b7b1-2691-4ee2-bb26-849b0d96bbd7", "Job Type"), GetJobMessageTypeList(this))
				};
				return list.ToArray();
			}
		}

		#region JobMessageTypeList

		public static ICodeDescriptionPairList GetJobMessageTypeList(WorkflowDescriptor workflowDescriptor)
		{
			ICodeDescriptionPairList list;
			var template = workflowDescriptor.LastProcessTaskTemplate;
			if (template != null)
			{
				if (template.GlobalTemplate && template.P0_GB.IsEmpty)
				{
					list = (ICodeDescriptionPairList)AllAvailableJobMessageTypeList.Clone();
				}
				else
				{
					var company = GetCompany(template);
					list = GetJobMessageTypeListForCountry(company.GC_RN_NKCountryCode);
				}
			}
			else
			{
				list = GetMessageTypeForWorkflow(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}
			return list;
		}

		static CodeDescriptionPairList GetMessageTypeForWorkflow(string countryCode)
		{
			var result = new JobMessageTypeList();
			if (countryCode == Core.Constants.CountryCodes.UnitedStates)
			{
				result.RemoveCode(JobMessageTypeList.Codes.Drawback);
			}
			return result;
		}

		static CodeDescriptionPairList GetJobMessageTypeListForCountry(ZString countryCode)
		{
			var result = new CodeDescriptionPairList();
			foreach (ICodeDescription codeDesc in GetJobMessageTypeListFor(countryCode))
			{
				if (!result.ContainsCode(codeDesc.Code))
				{
					result.AddPair(codeDesc.Code, GetDescriptionWithCountryCode(codeDesc.Description, countryCode));
				}
			}
			return result;
		}

		static CodeDescriptionPairList AllAvailableJobMessageTypeList
		{
			get
			{
				if (allAvailableJobMessageTypeList == null)
				{
					allAvailableJobMessageTypeList = GetMessageTypeForWorkflow(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					foreach (var pair in GetCodeTypeAndCountriesDictionary(allAvailableJobMessageTypeList))
					{
						allAvailableJobMessageTypeList.AddPair(pair.Key, GetDescriptionWithCountryDetails(pair.Value));
					}
				}
				return allAvailableJobMessageTypeList;
			}
		}
		[ThreadStatic]
		static CodeDescriptionPairList allAvailableJobMessageTypeList;

		static ZString GetDescriptionWithCountryDetails(Dictionary<ZString, List<string>> dictionary)
		{
			var builder = new ZStringBuilder();
			foreach (var pair in dictionary)
			{
				builder.Append(pair.Key + " (" + new ZStringBuilder(pair.Value).ToStringWithDelimiterBetweenAppends(", ") + ")");
			}
			return builder.ToStringWithDelimiterBetweenAppends(", ");
		}

		static SortedDictionary<ZString, Dictionary<ZString, List<string>>> GetCodeTypeAndCountriesDictionary(CodeDescriptionPairList list)
		{
			var codeTypeAndCountriesDictionary = new SortedDictionary<ZString, Dictionary<ZString, List<string>>>();
			foreach (var countryCode in GetSupportedCountryCodes())
			{
				foreach (ICodeDescription codeDesc in GetJobMessageTypeListFor(countryCode))
				{
					if (!list.ContainsCode(codeDesc.Code))
					{
						if (!codeTypeAndCountriesDictionary.TryGetValue(codeDesc.Code, out var descriptionAndCountriesDictionary))
						{
							descriptionAndCountriesDictionary = new Dictionary<ZString, List<string>>();
							codeTypeAndCountriesDictionary.Add(codeDesc.Code, descriptionAndCountriesDictionary);
						}

						if (!descriptionAndCountriesDictionary.TryGetValue(codeDesc.Description, out var countryList))
						{
							countryList = new List<string>();
							descriptionAndCountriesDictionary.Add(codeDesc.Description, countryList);
						}
						countryList.Add(countryCode);
					}
				}
			}
			return codeTypeAndCountriesDictionary;
		}

		static IEnumerable<ZString> GetSupportedCountryCodes()
		{
			var result = new List<ZString>();
			var query = new ZQuery();
			query.OrderBy = GlbCompanySchema.GC_RN_NKCountryCode.Name;
			foreach (var company in new BusinessObjectFactory().Load<GlbCompany>(query))
			{
				if (!result.Contains(company.GC_RN_NKCountryCode))
				{
					result.Add(company.GC_RN_NKCountryCode);
				}
			}
			return result;
		}

		static ICodeDescriptionPairList GetJobMessageTypeListFor(ZString countryCode)
		{
			if (!CacheOfJobMessageTypeListPerCountry.ContainsKey(countryCode))
			{
				var list = GetMessageTypeListForCountry(countryCode);
				CacheOfJobMessageTypeListPerCountry.Add(countryCode, list);
			}
			return CacheOfJobMessageTypeListPerCountry[countryCode];
		}

		static Dictionary<string, ICodeDescriptionPairList> CacheOfJobMessageTypeListPerCountry
		{
			get { return cacheOfJobMessageTypeListPerCountry ?? (cacheOfJobMessageTypeListPerCountry = new Dictionary<string, ICodeDescriptionPairList>()); }
		}
		[ThreadStatic]
		static Dictionary<string, ICodeDescriptionPairList> cacheOfJobMessageTypeListPerCountry;

		internal static CodeDescriptionPairList GetMessageTypeListForCountry(ZString countryCode)
		{
			var list = JobMessageTypeList.GetNewListFor(countryCode);
			switch (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode))
			{
				case Core.Constants.CountryCodes.Canada:
					list.AddPair(ImportOnlyForCanadaCode, ImportOnlyForCanadaDescription);
					break;
				case Core.Constants.CountryCodes.UnitedStates:
					list.RemoveCode(Common.US.USJobMessageTypeList.Codes.Drawback);
					list.RemoveCode(Common.US.USJobMessageTypeList.Codes.Recon);
					break;
			}
			return list;
		}

		#endregion

		#region TransportModeList

		static GlbCompany GetCompany(ProcessTaskTemplate template)
		{
			return template?.Branch?.Company ?? template?.Company ?? GlbCompany.CurrentCompany;
		}

		static ICodeDescriptionPairList GetTransportModeList(WorkflowDescriptor workflowDescriptor)
		{
			ICodeDescriptionPairList list;
			var template = workflowDescriptor.LastProcessTaskTemplate;
			if (template != null)
			{
				if (template.GlobalTemplate && template.P0_GB.IsEmpty)
				{
					list = (ICodeDescriptionPairList)AllAvailableTransportModeList.Clone();
				}
				else
				{
					var company = GetCompany(template);
					list = GetTransportModeListForTemplate(company.GC_RN_NKCountryCode);
				}
			}
			else
			{
				list = GetTransportModeListForCountry(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}

			return list;
		}

		static CodeDescriptionPairList AllAvailableTransportModeList
		{
			get
			{
				if (allAvailableTransportModeList == null)
				{
					allAvailableTransportModeList = new CodeDescriptionPairList();

					allAvailableTransportModeList.AddPair("", Res.GetString("45419BF1-8496-41C9-9EB0-E3473AE01977", "All"));
					allAvailableTransportModeList.AddPairsIfNotExist(GetAllAvailableTransportModeListCore(GetSupportedCountryCodes()));

					allAvailableTransportModeList.Sort();
				}
				return allAvailableTransportModeList;
			}
		}
		[ThreadStatic]
		static CodeDescriptionPairList allAvailableTransportModeList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		internal static IEnumerable<ICodeDescription> GetAllAvailableTransportModeListCore(IEnumerable<ZString> supportedCountryCodes)
		{
			var defaultTranportModeList = new TransportTypeList();

			var details = from countryCode in supportedCountryCodes
					.Select(countryCode => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode))
					.Distinct()
						  let codeDescriptions = GetTransportModeListForCountry(countryCode).Cast<ICodeDescription>()
						  from codeDescription in codeDescriptions
						  orderby countryCode
						  select new
						  {
							  CountryCode = (ZString)countryCode,
							  TransportMode = (ZString)codeDescription.Code,
							  TransportDescription = (ZString)codeDescription.Description
						  };

			return details.GroupBy(
				x => x.TransportMode,
				x => new
				{
					x.TransportDescription,
					x.CountryCode
				},
				(transportMode, descriptions) =>
				{
					var defaultDescription = (ZString)defaultTranportModeList.GetDescriptionFromCode(transportMode);

					ZString[] formattedDescriptions;
					if (defaultDescription.IsEmpty)
					{
						formattedDescriptions = descriptions.GroupBy(
							x => x.TransportDescription,
							x => x.CountryCode,
							GetDescriptionWithCountryCodes).ToArray();
					}
					else
					{
						formattedDescriptions = new[] { defaultDescription }.Union(descriptions
							.Where(x => x.TransportDescription != defaultDescription).GroupBy(
								x => x.TransportDescription,
								x => x.CountryCode,
								GetDescriptionWithCountryCodes)).ToArray();
					}

					return new CodeDescriptionPair((NoResString)transportMode, ZString.Join(", ", formattedDescriptions));
				});
		}

		static ZString GetDescriptionWithCountryCodes(ZString descriptoin, IEnumerable<ZString> countryCodes)
		{
			var countryCodeJoin = ZString.Join(", ", countryCodes.ToArray());
			return GetDescriptionWithCountryCode(descriptoin, countryCodeJoin);
		}

		static ZString GetDescriptionWithCountryCode(ZString descriptoin, ZString countryCode)
		{
			return ZString.Format("{0} ({1})", descriptoin, countryCode);
		}

		internal static CodeDescriptionPairList GetTransportModeListForCountry(ZString countryCode)
		{
			CodeDescriptionPairList result;
			if (TransportModeListForCountryCache.TryGetValue(countryCode, out var transportModeListForCountry))
			{
				result = transportModeListForCountry;
			}
			else
			{
				result = GetTransportModeListForCountryCore(countryCode);
				TransportModeListForCountryCache.Add(countryCode, result);
			}

			return result;
		}

		static CodeDescriptionPairList GetTransportModeListForTemplate(ZString countryCode)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("", Res.GetString("48f5f3f2-0f57-4b47-b6b9-d49e952db145", "All"));
			foreach (ICodeDescription codeDesc in GetTransportModeListForCountry(countryCode))
			{
				result.AddPairIfNotExist(codeDesc.Code, GetDescriptionWithCountryCode(codeDesc.Description, countryCode));
			}
			return result;
		}

		static CodeDescriptionPairList GetTransportModeListForCountryCore(ZString countryCode)
		{
			CodeDescriptionPairList list = null;

			var customsCountryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
			if (!customsCountryCode.IsNullOrEmpty())
			{
				var providers = ObjectFactory.Get<Hashtable>("TransportTypeListProvider");
				var objectHandle = (ObjectHandle)providers[customsCountryCode];
				list = (CodeDescriptionPairList)objectHandle?.GetObject() ?? new TransportTypeList();

				if (customsCountryCode == Core.Constants.CountryCodes.Australia)
				{
					list.AddPairIfNotExist(Core.Constants.TransportModes.Other, Core.Constants.TransportModeDescriptions.Other);
				}
			}

			return list ?? new TransportTypeList();
		}

		static Dictionary<string, CodeDescriptionPairList> TransportModeListForCountryCache => transportModeListForCountryCache ?? (transportModeListForCountryCache = new Dictionary<string, CodeDescriptionPairList>());

		[ThreadStatic]
		static Dictionary<string, CodeDescriptionPairList> transportModeListForCountryCache;

		#endregion

		#endregion

		#region Port Names

		public override ZString Port1Name
		{
			get { return Res.GetString("2d20c7f0-3f1b-42a5-b943-1d333b79cf27", "Origin"); }
		}

		public override ZString Port2Name
		{
			get { return Res.GetString("dffbfb5a-fbed-49b0-b640-7a88fff18a25", "Destination"); }
		}

		#endregion

		#region Conditions

		public override CodeDescriptionPairList GetConditionList1(ITemplateConditionalWorkflowItem workflowItem)
		{
			var result = new JobDeclarationWorkflowCondition1CodeList();
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada)
			{
				result.AddPair(ImportOnlyForCanadaCode, ImportOnlyForCanadaDescription);
			}
			return result;
		}

		public override CodeDescriptionPairList GetConditionList2(ITemplateConditionalWorkflowItem workflowItem)
		{
			var result = new JobDeclarationWorkflowCondition2CodeList();
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada)
			{
				result.AddPair(ImportOnlyForCanadaCode, ImportOnlyForCanadaDescription);
			}
			return result;
		}

		public static ZString ImportOnlyForCanadaCode
		{
			get { return Res.GetString("20032C67-1659-4D4C-9A81-CF5CB83CC2BB", "IMO"); }
		}
		internal static ZString ImportOnlyForCanadaDescription
		{
			get { return Res.GetString("049A574B-EC40-408D-8D0D-22D3C0F2E0DB", "Import, LVS excluded"); }
		}

		#endregion

		#region Criteria Requirements

		public override bool RequiresBranch { get { return true; } }
		public override bool RequiresDepartment { get { return true; } }
		public override bool RequiresPort1 { get { return true; } }
		public override bool RequiresPort2 { get { return true; } }
		public override bool SupportsEventTracking { get { return true; } }

		#endregion

		#region Workflow Trigger

		public override bool SupportsWorkflowTriggerActionXML
		{
			get { return true; }
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[]
			{
				JobDeclarationSchema.JE_HouseBill,
				JobDeclarationSchema.JE_VesselName,
				JobDeclarationSchema.JE_VoyageFlightNo,
				JobDeclarationSchema.JE_DateAtOrigin,
				JobDeclarationSchema.JE_DateAtFinalDestination,
				JobDeclarationSchema.JE_ExportDate,
				JobDeclarationSchema.JE_DateOfArrival,
				JobDeclarationSchema.JE_MasterBill,
				JobDeclarationSchema.JE_EntryAuthorisationDate,
				JobDeclarationSchema.JE_EntrySubmittedDate,
				JobDeclarationSchema.JE_WarehouseReleaseDate,
				JobDeclarationSchema.JE_DateOfFirstArrival,
				JobDeclarationSchema.JE_EntryDate,
				JobDeclarationSchema.JE_RL_NKPortOfLoading,
				JobDeclarationSchema.JE_RL_NKPortOfFirstArrival,
				JobDeclarationSchema.JE_RL_NKPortOfArrival,
				JobDeclarationSchema.JE_RL_NKFinalDestination,
				JobDeclarationSchema.JE_LandedPieces,
				JobDeclarationSchema.JE_TotalNoOfPacks,
				JobDocsAndCartageSchema.JP_EstimatedDelivery,
				JobDocsAndCartageSchema.JP_EstimatedPickup,
				CusEntryHeaderSchema.CH_BondAcquittedDate,
				CusEntryHeaderSchema.CH_BondValidToDate
			};
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.BillToParty |
				MessageRecipientPartyType.PickupCartage |
				MessageRecipientPartyType.DeliveryCartage |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.Forwarder |
				MessageRecipientPartyType.ExternalBroker |
				MessageRecipientPartyType.ControllingCustomer;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var result = base.GetWorkflowTriggerActionCore(source, queuedLog);
			if (result != null)
			{
				return result;
			}
			var action = source.Action;

			var declaration = (BaseJobDeclaration)source.Job;
			var declarationTriggeredByEvents = !queuedLog.ChangeLogs.Any() ? new EventsWithSourceType(EventsWithSourceType.SourceType.Declaration, action, declaration) : EventsWithSourceType.Empty;

			if (WorkflowTriggerActionTypeConstants.IsStandardXml(action.PQ_TriggerType))
			{
				var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XML);
				return new XmlMessageDeliver(xmlModes, declaration, () =>
				{
					return new DeclarationWithConsolShipmentDetailExporter().ExportDeclarationWithRelatedConsolShipmentDetails(declaration, declarationTriggeredByEvents, action);
				}, action);
			}
			else if (WorkflowTriggerActionTypeConstants.IsDebtorBalanceXml(action.PQ_TriggerType))
			{
				var job = declaration.Job;
				if (job != null)
				{
					var factory = declaration.Factory;
					var debtorAddress = factory.Load<OrgAddress>(job.JH_OA_LocalChargesAddr);
					var debtor = debtorAddress != null ? factory.Load<OrgHeader>(debtorAddress.OA_OH) : null;

					if (debtor != null)
					{
						var xmlModes = GetMessageRecipientEdiCommunicationsModes(source, EDICommunicationsModeFileFormatList.Codes.XMB);
						return new XmlMessageDeliver(xmlModes, new DebtorBalanceRecordForExport(debtor, factory), declaration, new DebtorBalanceValueObjectDataAdapter(), action);
					}
					else
					{
						return new LogAction((NoResString)"No debtor found for job");
					}
				}
				else
				{
					return new LogAction((NoResString)"No job attached to declaration.");
				}
			}
			else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SynchronizeWithBondedWarehouse)
			{
				if (declaration != null)
				{
					return new SynchronizeWithBondedWarehouseProcessor(declaration);
				}
			}
			else if (action.Parent.IsForCountry(declaration, Core.Constants.CountryCodes.Canada))
			{
				if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message)
				{
					var supporter = GetScheduleB3MessageSupporter(declaration as CA.IJobDeclaration);
					if (supporter != null && supporter.SupportScheduleB3Message)
					{
						return supporter.CreateStmProcessQueueProcessor(null, action.PQ_TriggerType);
					}
					return new LogAction($"Trigger Action Type {WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message} is only allowed for IMP/LVS declarations. Declaration: {declaration?.HumanReadableName ?? ZString.Empty}.");
				}
				else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery)
				{
					var supporter = GetSubmitAVSQuerySupporter(declaration as CA.IJobDeclaration);
					if (supporter != null && supporter.SupportSubmitAVSQuery)
					{
						return supporter.CreateSubmitAVSQueryProcessor();
					}
					return new LogAction($"Trigger Action Type {WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery} is only allowed for IMP declarations. Declaration: {declaration?.HumanReadableName ?? ZString.Empty}.");
				}
				else if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.ConsolidateLVXDeclaration)
				{
					var supporter = GetConsolidateLVXDeclarationSupporter(declaration as CA.IJobDeclaration);
					if (supporter != null && supporter.SupportConsolidateLVXDeclaration)
					{
						return supporter.CreateConsolidateLVXDeclarationProcessor();
					}
					return new LogAction($"Trigger Action Type {WorkflowTriggerActionTypeConstants.Codes.ConsolidateLVXDeclaration} is only allowed for LVX declarations. Declaration: {declaration?.HumanReadableName ?? ZString.Empty}.");
				}
			}

			if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage || action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage)
			{
				if (declaration is IJobDeclarationAutoSendingMessageSupporter supporter)
				{
					return supporter.CreateStmProcessQueueProcessor(null, action.PQ_TriggerType);
				}
				return new LogAction($"Trigger Action Type {WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage} is only allowed for declarations which support automated message sending. Declaration: {declaration?.HumanReadableName ?? ZString.Empty}.");
			}

			// An unexpected error condition has occured
			return null;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageRecipientParties, BusinessObject bizObj, ZString partyType)
		{
			BaseJobDeclaration declaration = (BaseJobDeclaration)bizObj;

			if (partyType == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(declaration.Consignee, ZString.Empty));
			}

			if (partyType == MessageRecipientPartyTypeList.Codes.Consignor)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(declaration.Consignor, ZString.Empty));
			}

			if (partyType == MessageRecipientPartyTypeList.Codes.PickupCartage
				&& declaration.IsExport
				&& declaration.DocsAndCartage != null)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(declaration.DocsAndCartage.PickupCartageCoAddr));
			}

			if (partyType == MessageRecipientPartyTypeList.Codes.DeliveryCartage
				&& declaration.IsImport
				&& declaration.DocsAndCartage != null)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(declaration.DocsAndCartage.DeliveryCartageCoAddr));
			}

			if (partyType == MessageRecipientPartyTypeList.Codes.Forwarder)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(declaration.Forwarder, ZString.Empty));
			}

			if (partyType == MessageRecipientPartyTypeList.Codes.ExternalBroker)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(declaration.ExternalBroker, ZString.Empty));
			}

			if (partyType == MessageRecipientPartyTypeList.Codes.ControllingCustomer)
			{
				messageRecipientParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(declaration.ControllingCustomer, ZString.Empty));
			}
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new BusinessContext[] { BusinessContext.Customs }; }
		}

		#endregion

		#region Default From List

		public override CodeDescriptionPairList EstimateDefaultedFromList
		{
			get { return new DeclarationEstimateDefaultedFromList(); }
		}

		#endregion

		#region Default Date

		protected override (ZDateTime, RefUNLOCO) GetScheduleDateTimeAndLocationForTimezone(IDefaultedFromDateProvider defaultedFromDateProvider, ZString dateTimeSourceType)
		{
			if (!dateTimeSourceType.IsEmpty && defaultedFromDateProvider.Parent is BaseJobDeclaration concreteParent)
			{
				if (dateTimeSourceType == DeclarationEstimateDefaultedFromList.Codes.ETA)
				{
					return (concreteParent.JE_DateAtFinalDestination, concreteParent.FinalDestination);
				}
			}
			return (ZDateTime.Empty, null);
		}

		#endregion

		public override Type WorkflowProviderType
		{
			get { return typeof(BaseJobDeclaration); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.JobDeclaration; }
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new JobDeclarationFormCustomisationSettingsProvider();
		}

		public override bool SupportsCreateTransportBooking
		{
			get { return true; }
		}

		protected override BusinessObjectEventDataModel GetEventDataModelCore(BusinessObject businessObject)
		{
			return businessObject is BaseJobDeclaration declaration
				? new JobDeclarationEventDataModel(declaration)
				: base.GetEventDataModelCore(businessObject);
		}

		#region Validation Rules

		protected override ValidationToolSettings GetValidationToolSettings() => new JobDeclarationValidationToolSettings(this);

		#endregion

		protected override IEnumerable<string> GetSupportsValidateForCustomsMessagingTriggerActionsCore(IBaseTrigger trigger, IBusiness parent)
		{
			var countrySupportedVCMInWorkflowTemplate = countriesSupportedAutoSendingMessage.Any(x => trigger.IsForCountry((BusinessObject)parent, x));
			var customsMessagingSupporter = parent as IValidateForCustomsMessagingSupporter;
			if (customsMessagingSupporter?.SupportValidateCustomsMessaging ?? countrySupportedVCMInWorkflowTemplate)
			{
				yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			}
		}

		IScheduleB3MessageSupporter GetScheduleB3MessageSupporter(CA.IJobDeclaration declaration)
		{
			return declaration == null ? null : ObjectFactory.New<IScheduleB3MessageSupporter>(declaration);
		}

		ISubmitAVSQuerySupporter GetSubmitAVSQuerySupporter(CA.IJobDeclaration declaration)
		{
			return declaration == null ? null : ObjectFactory.New<ISubmitAVSQuerySupporter>(declaration);
		}

		IConsolidateLVXDeclarationSupporter GetConsolidateLVXDeclarationSupporter(CA.IJobDeclaration declaration)
		{
			return declaration == null ? null : ObjectFactory.New<IConsolidateLVXDeclarationSupporter>(declaration);
		}

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = (CodeDescriptionPairList)base.GetAdditionalWorkflowTriggerActionTypeList(trigger, parent);
			var bizo = (BusinessObject)parent;
			if (trigger.IsForCountry(bizo, Core.Constants.CountryCodes.Canada))
			{
				var b3MessageSupport = GetScheduleB3MessageSupporter(bizo as CA.IJobDeclaration);
				if (b3MessageSupport == null || b3MessageSupport.SupportScheduleB3Message)
				{
					result.AddPair(WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message, WorkflowTriggerActionTypeConstants.Descriptions.ScheduleB3Message);
				}

				var avsSupport = GetSubmitAVSQuerySupporter(bizo as CA.IJobDeclaration);
				if (avsSupport == null || avsSupport.SupportSubmitAVSQuery)
				{
					result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery, WorkflowTriggerActionTypeConstants.Descriptions.SubmitAVSQuery);
				}

				var lvxSupport = GetConsolidateLVXDeclarationSupporter(bizo as CA.IJobDeclaration);
				if (lvxSupport == null || lvxSupport.SupportConsolidateLVXDeclaration)
				{
					result.AddPair(WorkflowTriggerActionTypeConstants.Codes.ConsolidateLVXDeclaration, WorkflowTriggerActionTypeConstants.Descriptions.ConsolidateLVXDeclaration);
				}
			}
			if (ShouldAddSynchronizeWithBondedWarehouse(parent))
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SynchronizeWithBondedWarehouse, WorkflowTriggerActionTypeConstants.Descriptions.SynchronizeWithBondedWarehouse);
			}

			var declarationMessageSupporter = bizo as IJobDeclarationAutoSendingMessageSupporter;
			var countrySupportedAutoSendingMessageInWorkflowTemplate = countriesSupportedAutoSendingMessage.Any(x => trigger.IsForCountry(bizo, x));
			var supportEntryDeclarationMessage = declarationMessageSupporter?.SupportEntryDeclarationMessage ?? countrySupportedAutoSendingMessageInWorkflowTemplate;
			var supportReleaseMessage = declarationMessageSupporter?.SupportReleaseMessage ?? countrySupportedAutoSendingMessageInWorkflowTemplate;
			if (supportEntryDeclarationMessage)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendEntryDeclarationMessage);
			}
			if (supportReleaseMessage)
			{
				result.AddPair(WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendReleaseMessage);
			}

			return result;
		}

		readonly ZString[] countriesSupportedAutoSendingMessage = new ZString[]
		{
			Core.Constants.CountryCodes.Canada,
			Core.Constants.CountryCodes.SouthAfrica,
			Core.Constants.CountryCodes.UnitedStates,
			Core.Constants.CountryCodes.UnitedKingdom,
			Core.Constants.CountryCodes.France,
			Core.Constants.CountryCodes.FrenchGuyana,
			Core.Constants.CountryCodes.Guadeloupe,
			Core.Constants.CountryCodes.Martinique,
			Core.Constants.CountryCodes.Mayotte,
			Core.Constants.CountryCodes.Reunion,
			Core.Constants.CountryCodes.SaintBarthelemy,
			Core.Constants.CountryCodes.SaintMartin,
			Core.Constants.CountryCodes.Spain,
			Core.Constants.CountryCodes.Ireland,
			Core.Constants.CountryCodes.Norway,
		};

		protected override Dictionary<ZString, SecurityCheckpoint> GetWorkflowTriggerActionTypeSecurityCheckPoints()
		{
			var result = base.GetWorkflowTriggerActionTypeSecurityCheckPoints();
			result.Add(WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message, Env.Security.CAB3MsgSend);
			return result;
		}

		bool ShouldAddSynchronizeWithBondedWarehouse(IBusiness parent)
		{
			var result = parent is ProcessTaskTemplate;
			if (!result)
			{
				var declaration = parent as BaseJobDeclaration;
				result = declaration != null && declaration.IsWHSUniversalXMLActive && declaration.ShouldUpdateOutwardLinesWithInventoryDetails;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void CheckWorkflowTriggerActionTypeCore(IBaseTrigger trigger, IBusiness ibizo, ZPropertyInfo actionTypeInfo)
		{
			base.CheckWorkflowTriggerActionTypeCore(trigger, ibizo, actionTypeInfo);
			var bizo = (BusinessObject)ibizo;
			var declaration = bizo as BaseJobDeclaration;
			switch (actionTypeInfo.Value.ToString())
			{
				case WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message:
					if (!trigger.IsForCountry(bizo, Core.Constants.CountryCodes.Canada))
					{
						actionTypeInfo.AddError(Res.GetString("e801ec87-694f-4b05-8a30-df54e38cb375", "Trigger action Schedule CAD Message is valid only for CA."));
					}
					else
					{
						var supporter = GetScheduleB3MessageSupporter(declaration as CA.IJobDeclaration);
						if (supporter != null && !supporter.SupportScheduleB3Message)
						{
							actionTypeInfo.AddError(supporter.NotSupportScheduleB3MessageReason);
						}
					}
					break;
				case WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery:
					if (!trigger.IsForCountry(bizo, Core.Constants.CountryCodes.Canada))
					{
						actionTypeInfo.AddError(Res.GetString("507e944f-0d69-4086-ab76-d4551f7a592a", "Trigger action Submit AVS Query is valid only for CA."));
					}
					else
					{
						var supporter = GetSubmitAVSQuerySupporter(declaration as CA.IJobDeclaration);
						if (supporter != null && !supporter.SupportSubmitAVSQuery)
						{
							actionTypeInfo.AddError(supporter.NotSupportSubmitAVSQueryReason);
						}
					}
					break;
				case WorkflowTriggerActionTypeConstants.Codes.ConsolidateLVXDeclaration:
					if (!trigger.IsForCountry(bizo, Core.Constants.CountryCodes.Canada))
					{
						actionTypeInfo.AddError(Res.GetString("f257edc3-8603-4ceb-a864-88b6b3138d23", "Trigger action Consolidate LVX Declaration is valid only for CA."));
					}
					else
					{
						var supporter = GetConsolidateLVXDeclarationSupporter(declaration as CA.IJobDeclaration);
						if (supporter != null && !supporter.SupportConsolidateLVXDeclaration)
						{
							actionTypeInfo.AddError(supporter.NotSupportConsolidateLVXDeclarationReason);
						}
					}
					break;
				case WorkflowTriggerActionTypeConstants.Codes.SynchronizeWithBondedWarehouse:
					if (!(bizo is ProcessTaskTemplate))
					{
						if (declaration == null || !declaration.IsWHSUniversalXMLActive || !declaration.ShouldUpdateOutwardLinesWithInventoryDetails)
						{
							actionTypeInfo.AddError(Res.GetString("{C433B899-12B2-4409-A767-BD7D5ED179E3}", "Trigger action Synchronize with Inventory is valid only for warehouse jobs with Inventory Management Integration enabled."));
						}
					}
					break;
				case WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage:
					if (declaration is IJobDeclarationAutoSendingMessageSupporter entryDeclarationMessageSupporter && !entryDeclarationMessageSupporter.SupportEntryDeclarationMessage)
					{
						actionTypeInfo.AddError(entryDeclarationMessageSupporter.GetReasonForNotSupportEntryDeclarationMessage);
					}
					break;
				case WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage:
					if (declaration is IJobDeclarationAutoSendingMessageSupporter releaseMessageSupporter && !releaseMessageSupporter.SupportReleaseMessage)
					{
						actionTypeInfo.AddError(releaseMessageSupporter.GetReasonForNotSupportReleaseMessage);
					}
					break;
			}
		}

		class SynchronizeWithBondedWarehouseProcessor : IProcessor
		{
			public SynchronizeWithBondedWarehouseProcessor(BaseJobDeclaration declaration)
			{
				this.declaration = declaration;
			}
			readonly BaseJobDeclaration declaration;

			public void Process(INotifications notifications, CancellationToken token
#if DEBUG
				= new CancellationToken()
#endif
			)
			{
				var error = declaration.UpdateOutwardLinesWithInventoryDetails();
				if (!error.IsEmpty)
				{
					notifications.AddError(error);
				}
			}
		}

		public IEnumerable<string> SupportedTriggerLineTypes
		{
			get
			{
				yield return TriggerLineTypes.Codes.CusEntryHeader;
				yield return TriggerLineTypes.Codes.CusExitReport;
				yield return TriggerLineTypes.Codes.CusNOEmmaMessageGenerator;
			}
		}
	}
}
