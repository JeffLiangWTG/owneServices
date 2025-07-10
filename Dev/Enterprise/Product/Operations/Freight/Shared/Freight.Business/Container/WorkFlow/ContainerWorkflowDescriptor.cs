using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ContainerWorkflowDescriptor : WorkflowDescriptor
	{
		#region ReplacementConstants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded constant")]
		public static class ReplacementConstants
		{
			public const string ContainerAvailabilityReportUrl = "(*ContainerAvailabilityReportUrl*)";
			public const string VesselName = "(*VesselName*)";
			public const string Voyage = "(*Voyage*)";
			public const string ContainerNumber = "(*ContainerNumber*)";
			public const string JobsAffected = "(*JobsAffected*)";
		}

		#endregion

		#region ID / Description

		public override string Code
		{
			get { return WorkflowDescriptors.ContainerWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("Freight|ContainerWorkflowDescriptor|Description", "Container"); }
		}

		#endregion

		#region  Conditions

		public override bool RequiresClient
		{
			get { return true; }
		}

		public override bool RequiresPort1
		{
			get { return true; }
		}

		public override bool RequiresPort2
		{
			get { return true; }
		}

		public override bool RequiresBranch
		{
			get { return true; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		#endregion

		#region Carrier

		public override ZString ClientName
		{
			get { return Res.GetString("037cc1d5-a9df-41e5-a0e1-92b56934f25b", "Carrier"); }
		}

		public override Func<ProcessTaskTemplate, OrgHeaderCollection> ClientListProvider
		{
			get { return template => GetCarrierLookup(template.Factory, template.P0_SubType1); }
		}

		#endregion

		#region Port Names

		public override ZString Port1Name
		{
			get { return Res.GetString("093f989b-a03c-4547-a009-415c50a874c9", "Origin"); }
		}

		public override ZString Port2Name
		{
			get { return Res.GetString("21b83d0d-e87d-4d58-99c7-0b48ace90677", "Destination"); }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				return new[]
				{
					new ProcessTemplateSubType(Res.GetString("eb5863e1-ec43-461d-9724-ca41de64eff5", "Transport Mode"), TransportModesList),
					new ProcessTemplateSubType(Res.GetString("ffc623c9-d8d8-496f-a407-913d87bc524d", "Container Mode"), ContainerModesList),
				};
			}
		}

		#endregion

		#region Lists

		public static OrgHeaderCollection GetCarrierLookup(BusinessObjectFactory factory, string transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					return new AirShippingProviderCollection(factory);

				case Core.Constants.TransportModes.Sea:
					return new SeaShippingProviderCollection(factory);

				case Core.Constants.TransportModes.Rail:
					return new RailShippingProviderCollection(factory);

				case Core.Constants.TransportModes.Road:
					return new LineHaulShippingProviderCollection(factory);

				default:
					return new ShippingProviderCollection(factory);
			}
		}

		public CodeDescriptionPairList TransportModesList
		{
			get
			{
				if (transportModesList == null)
				{
					transportModesList = new ConsolTransportModeCodeDescriptionPairList();
				}

				return transportModesList;
			}
		}
		ConsolTransportModeCodeDescriptionPairList transportModesList;

		public CodeDescriptionPairList ContainerModesList
		{
			get { return LastProcessTaskTemplate != null ? CommonContainerLookups.GetContainerModesList(LastProcessTaskTemplate.P0_SubType1) : ContainerModesList_Empty; }
		}

		CodeDescriptionPairList ContainerModesList_Empty
		{
			get { return containerModesList_Empty ?? (containerModesList_Empty = new CodeDescriptionPairList()); }
		}
		CodeDescriptionPairList containerModesList_Empty;

		#endregion

		#region Workflow Triggers

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.ExportBroker |
				MessageRecipientPartyType.ImportBroker |
				MessageRecipientPartyType.Consignor |
				MessageRecipientPartyType.Consignee |
				MessageRecipientPartyType.Carrier |
				MessageRecipientPartyType.PickupCartage |
				MessageRecipientPartyType.DeliveryCartage |
				MessageRecipientPartyType.DepartureContainerYard |
				MessageRecipientPartyType.ArrivalContainerYard |
				MessageRecipientPartyType.DepartureCTO |
				MessageRecipientPartyType.ArrivalCTO |
				MessageRecipientPartyType.SendingAgent |
				MessageRecipientPartyType.ReceivingAgent |
				MessageRecipientPartyType.DepartureCFS |
				MessageRecipientPartyType.ArrivalCFS |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			CommonContainer container = (CommonContainer)bizObj;
			IContainerParent parent = container.ContainerParent;

			if (parent != null)
			{
				switch (partyType)
				{
					case MessageRecipientPartyTypeList.Codes.Carrier:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(parent.ShippingLineAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.PickupCartage:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(parent.DeparturePackCFSTransportAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.DeliveryCartage:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(parent.ArrivalUnpackCFSTransportAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.DepartureCTO:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(parent.DepartureCTOAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.ArrivalCTO:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(parent.ArrivalCTOAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.SendingAgent:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(parent.SendingForwarderAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.ReceivingAgent:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(parent.ReceivingForwarderAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.DepartureContainerYard:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(parent.ContainerYardEmptyPickupAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.ArrivalContainerYard:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(parent.ContainerYardEmptyReturnAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.DepartureCFS:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(parent.DepartureCFSAddress));
						break;
					case MessageRecipientPartyTypeList.Codes.ArrivalCFS:
						messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(parent.ArrivalCFSAddress));
						break;
				}
			}

			if (container != null)
			{
				var declaration = container.Declaration as Enterprise.Integration.Customs.IBaseJobDeclaration;

				switch (partyType)
				{
					case MessageRecipientPartyTypeList.Codes.ExportBroker:
						if (declaration != null && declaration.IsExport)
						{
							OrgHeader exportBroker = container.Factory.Load<OrgHeader>(declaration.CompanyPK);
							messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(exportBroker, ZString.Empty));
						}
						else
						{
							foreach (var shipment in container.GetParentShipments())
							{
								messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ExportBroker, ZString.Empty));
							}
						}

						break;

					case MessageRecipientPartyTypeList.Codes.ImportBroker:
						if (declaration == null || declaration.IsExport)
						{
							foreach (var shipment in container.GetParentShipments())
							{
								messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.ImportBroker, ZString.Empty));
							}
						}

						break;

					case MessageRecipientPartyTypeList.Codes.Consignor:
						if (declaration != null)
						{
							OrgHeader supplier = container.Factory.Load<OrgHeader>(declaration.JE_OH_Supplier);
							if (supplier != null)
							{
								messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(supplier, ZString.Empty));
							}
						}
						else
						{
							foreach (var shipment in container.GetParentShipments())
							{
								messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.Consignor, ZString.Empty));
							}
						}

						break;

					case MessageRecipientPartyTypeList.Codes.Consignee:
						if (declaration != null)
						{
							OrgHeader importer = container.Factory.Load<OrgHeader>(declaration.JE_OH_Importer);
							if (importer != null)
							{
								messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(importer, ZString.Empty));
							}
						}
						{
							foreach (var shipment in container.GetParentShipments())
							{
								messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(shipment.Consignee, ZString.Empty));
							}
						}

						break;
				}
			}
		}

		protected override BusinessObjectEventDataModel GetEventDataModelCore(BusinessObject businessObject)
		{
			return new ContainerEventDataModel((CommonContainer)businessObject);
		}

		protected override WorkflowTriggerNotification GetWorkflowTriggerForNotificationEmail(Lazy<MessageProcessorCommunicationModesResult> modes, ProcessTaskNotification action, BusinessObject parent, Lazy<IStmALog> logProvider)
		{
			var triggerNotification = base.GetWorkflowTriggerForNotificationEmail(modes, action, parent, logProvider);

			triggerNotification.ExtraDataSubstitution = (ProcessTaskNotification processTaskNotification, BusinessObject bizo, ZString message) =>
			{
				var container = (CommonContainer)bizo;

				var result = message.ReplaceIgnoringCase(ReplacementConstants.ContainerAvailabilityReportUrl, CreateContainerAvailabilityReportUrl());
				result = result.ReplaceIgnoringCase(ReplacementConstants.VesselName, GetPropertyValueFromRelatedJob(container, CommonConsol.Schema.JK_JX_JV_NKVessel, JobDeclarationSchema.JE_VesselName.Name));
				result = result.ReplaceIgnoringCase(ReplacementConstants.Voyage, GetPropertyValueFromRelatedJob(container, CommonConsol.Schema.JK_JX_JV_VoyageFlight, JobDeclarationSchema.JE_VoyageFlightNo.Name));
				result = result.ReplaceIgnoringCase(ReplacementConstants.ContainerNumber, GetContainerNumber(container));
				result = result.ReplaceIgnoringCase(ReplacementConstants.JobsAffected, BuildJobsAffected(container));

				return result;
			};

			return triggerNotification;
		}

		string CreateContainerAvailabilityReportUrl()
		{
			var factory = new BusinessObjectFactory();
			var query = new DocumentZQuery("RepBookingsReports", (NoResString)"FCL Container Availability");
			var reportCommand = factory.LoadTop1<ReportCommand>(query);

			if (reportCommand == null)
			{
				SendReportForFCLContainerAvailability(factory);
			}

			return ObjectFactory.Get<IReportUrlHandler>().Create(reportCommand);
		}

		#region SuppressResourceStringsCheckRegion

		void SendReportForFCLContainerAvailability(BusinessObjectFactory factory)
		{
			var menuItemQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, "FCL Container Availability");
			var menuItem = factory.LoadTop1<StmMenuItem>(menuItemQuery);

			string menuItemInfo;
			if (menuItem != null)
			{
				menuItemInfo = FormattableString.Invariant($@"FCL Container Availability menu item is found:
{menuItem.SU_MenuType},
{menuItem.SU_BusinessContext},
{menuItem.SU_IsSystemDefined}");
			}
			else
			{
				menuItemInfo = "Cannot find FCL Container Availability menu item.";
			}

			ErrorReporter.ReportOnce("ContainerWorkflowDescriptor_CreateContainerAvailabilityReportUrl_ReportCommandIsNull",
				FormattableString.Invariant($@"ContainerWorkflowDescriptor.CreateContainerAvailabilityReportUrl:
reportCommand is null.
See WI00468184.
{menuItemInfo}"));
		}

		#endregion

		ZString GetPropertyValueFromRelatedJob(CommonContainer container, string consolPropertyName, string declarationPropertyName)
		{
			if (container.Consol != null)
			{
				return (ZString)container.Consol[consolPropertyName];
			}
			else if (container.Declaration != null)
			{
				return (ZString)container.Declaration[declarationPropertyName];
			}

			return ZString.Empty;
		}

		ZString BuildJobsAffected(CommonContainer container)
		{
			var resultBuilder = new ZStringBuilder();
			var jobLine = @"<a href=""{0}"">{1}</a>";

			if (container.Consol != null)
			{
				resultBuilder.Append(string.Format(CultureInfo.InvariantCulture, jobLine, GetBoURL(container.Consol, ControllerIDs.JobConsol), Res.GetString("88313e26-a8cf-4965-9dd9-dc258d62c9f3", "Consol {0}", container.Consol.JK_UniqueConsignRef)));
			}

			if (container.Declaration != null)
			{
				resultBuilder.Append("&nbsp;");
				resultBuilder.AppendLine("<br />");
				resultBuilder.Append("&nbsp;");
				resultBuilder.Append(string.Format(CultureInfo.InvariantCulture, jobLine, GetBoURL(container.Declaration, ControllerIDs.Customs.JobDeclaration), Res.GetString("d9e06a6d-3f0e-4a44-b6d1-407b6ccffef9", "Declaration {0}", container.Declaration[JobDeclarationSchema.JE_DeclarationReference])));
			}

			return resultBuilder.ToString();
		}

		ZString GetBoURL(BusinessObject businessObject, ControllerID controllerID)
		{
			return ObjectFactory.Get<IShowEditFormUrlCreator>().Create(controllerID, businessObject.PK.ToGuid());
		}

		ZString GetContainerNumber(CommonContainer container)
		{
			var result = container.JC_ContainerNum;

			if (result.IsEmpty)
			{
				var query = new ZQuery(CusContainerSchema.CO_JC, container.PK);
				var customsContainer = (BusinessObject)container.Factory.LoadTop1<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(query);

				if (customsContainer != null)
				{
					result = (ZString)customsContainer[CusContainerSchema.CO_ContainerNumber];
				}
			}

			return result;
		}

		#endregion

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(CommonContainer); }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Containers; }
		}

		#region CondtionList1

		public override CodeDescriptionPairList GetConditionList1(ITemplateConditionalWorkflowItem workflowItem)
		{
			return new CodeDescriptionPairList(new JobContainerWorkflowCondition1CodeList());
		}

		#endregion

		protected override ICodeDescriptionPairList GetAdditionalWorkflowTriggerActionTypeList(IBaseTrigger trigger, IBusiness parent)
		{
			var result = new CodeDescriptionPairList();

			var currentCountry = (RefCountry)Env.CurrentCompany.Country;
			if (currentCountry.IsFranceOrTerritory || currentCountry.IsPartOfEuropeanUnion || currentCountry.IsInEFTA)
			{
				result.AddPair(
					WorkflowTriggerActionTypeConstants.Codes.SendExportDemandDeTracing,
					 WorkflowTriggerActionTypeConstants.Descriptions.SendExportDemandDeTracing);

				result.AddPair(
					WorkflowTriggerActionTypeConstants.Codes.SendImportDemandDeTracing,
					WorkflowTriggerActionTypeConstants.Descriptions.SendImportDemandDeTracing);
			}

			return result;
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var result = base.GetWorkflowTriggerActionCore(source, queuedLog);

			if (result != null)
			{
				return result;
			}

			var action = source.Action;
			var container = (CommonContainer)source.Job;

			switch (action.PQ_TriggerType)
			{
				case WorkflowTriggerActionTypeConstants.Codes.SendImportDemandDeTracing:
					result = ObjectFactory.New<IDemandeDeTracingMessageProcessorProvider>().GetProcessor(container, true);
					break;

				case WorkflowTriggerActionTypeConstants.Codes.SendExportDemandDeTracing:
					result = ObjectFactory.New<IDemandeDeTracingMessageProcessorProvider>().GetProcessor(container, false);
					break;
			}

			return result;
		}

		#region Defaulting Trigger Conditions

		protected override void SetDefaultTriggerConditionsCore(IMilestoneDateDefaultable defaultable, BusinessObject parent)
		{
			if (defaultable.TriggerCondition.IsEmpty
				&& (IsDepartureRelatedProcessTask(defaultable) || IsArrivalRelatedProcessTask(defaultable))
				&& parent is CommonContainer container)
			{
				var leg = GetLegIdentifier(defaultable, container);
				if (leg != null)
				{
					defaultable.AddTriggerCondition(IsDepartureRelatedProcessTask(defaultable), leg, true);
				}
			}
		}

		static string GetLegIdentifier(IMilestoneDateDefaultable processTask, CommonContainer container)
		{
			var transports = GetTransportLegsFromLoadToDischarge(container);
			return processTask.GetLegIdentifier(transports.Length,
				IsDepartureRelatedProcessTask(processTask),
				IsArrivalRelatedProcessTask(processTask),
				Condition1Codes,
				legNames);
		}

		static bool IsDepartureRelatedProcessTask(IMilestoneDateDefaultable processTask)
		{
			return processTask.TriggerEventCode == Events.Departure.Code
				|| processTask.TriggerEventCode == Events.GateIn.Code
				|| processTask.TriggerEventCode == Events.FreightLoaded.Code;
		}

		static bool IsArrivalRelatedProcessTask(IMilestoneDateDefaultable processTask)
		{
			return processTask.TriggerEventCode == Events.Arrival.Code
				|| processTask.TriggerEventCode == Events.GateOut.Code
				|| processTask.TriggerEventCode == Events.FreightUnloaded.Code;
		}

		public static ImmutableArray<string> Condition1Codes { get; } = new[]
		{
			JobContainerWorkflowCondition1CodeList.Codes.Has2ndIntermediateLeg,
			JobContainerWorkflowCondition1CodeList.Codes.Has3rdIntermediateLeg,
			JobContainerWorkflowCondition1CodeList.Codes.Has4thIntermediateLeg
		}.ToImmutableArray();

		readonly static ImmutableArray<string> legNames = new[]
		{
			nameof(ContainerEventDataModel.FirstLeg),
			nameof(ContainerEventDataModel.SecondLeg),
			nameof(ContainerEventDataModel.ThirdLeg),
			nameof(ContainerEventDataModel.FourthLeg)
		}.ToImmutableArray();

		internal static Transport[] GetTransportLegsFromLoadToDischarge(CommonContainer container)
		{
			var containerParent = container.ContainerParent;
			var transports = ((IRoutingSupport)container).Transports;
			if (containerParent != null && transports != null)
			{
				var sortedTransports = transports.Cast<Transport>().ToArray();
				MovementLegComparer.SortMovementLegsByPorts(sortedTransports);
				return RoutineSupportHelper.GetTransportLegsFromLoadToDischarge(sortedTransports,
					(containerParent.LoadPort?.RL_Code).GetValueOrDefault(),
					(containerParent.DischargePort?.RL_Code).GetValueOrDefault());
			}

			return Array.Empty<Transport>();
		}

		#endregion

	}
}
