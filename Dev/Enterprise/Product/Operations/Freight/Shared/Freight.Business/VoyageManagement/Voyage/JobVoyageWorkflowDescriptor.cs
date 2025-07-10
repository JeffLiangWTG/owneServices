using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class JobVoyageWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code
		{
			get { return WorkflowDescriptors.SailingScheduleWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("Freight|JobVoyageWorkflowDescriptor|Description", "Sailing Schedule"); }
		}

		#endregion

		#region Templates Differentiation

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				var list = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				list.Add(new ProcessTemplateSubType(Res.GetString("b3af7f1a-d193-47ba-8c92-0a2e71fa57de", "Transport Mode"), GetTransportModes()));
				return list.ToArray();
			}
		}

		CodeDescriptionPairList GetTransportModes()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(string.Empty, Res.GetString("d43468c9-30fe-4aee-9c6e-d6f3271797b8", "All"));
			result.AddPair(Core.Constants.TransportModes.Air, ResString.GetMultilingualString("3e6d5253-c2d5-46cf-bd5e-84895a8e7e54", "Air"));
			result.AddPair(Core.Constants.TransportModes.Sea, ResString.GetMultilingualString("72f787d0-00d1-464d-be02-d48abcd5cf3c", "Sea"));
			result.AddPair(Core.Constants.TransportModes.Road, ResString.GetMultilingualString("504484bf-342c-4393-bcc1-bb9506e26e62", "Road"));
			result.AddPair(Core.Constants.TransportModes.Rail, ResString.GetMultilingualString("40a0c29d-3985-47ee-944e-dcc98b700fd2", "Rail"));
			return result;
		}

		#endregion

		public override ZString ClientName
		{
			get { return ResString.GetMultilingualString("9a177627-4262-4d96-832c-29263397a01b", "Carrier"); }
		}

		public override Func<ProcessTaskTemplate, OrgHeaderCollection> ClientListProvider
		{
			get { return template => BaseJobVoyageLookups.GetCarrierLookup(template.Factory, template.P0_SubType1); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(JobVoyage); }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.INVALID }; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool AreTasksCompanySpecific
		{
			get { return false; }
		}

		public override bool SupportsBufferManagement
		{
			get { return true; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.ArrivalCTO |
				MessageRecipientPartyType.DepartureCTO |
				MessageRecipientPartyType.Carrier |
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[]
			{
				JobVoyOriginSchema.JA_E_DEP,
				JobVoyOriginSchema.JA_CutOff,
				JobVoyOriginSchema.JA_ReceivalCommences,
				JobVoyOriginSchema.JA_DGCutOff,
				JobVoyOriginSchema.JA_DGReceivalCommences,
				JobVoyDestinationSchema.JB_E_ARV
			};
		}

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var voyage = (JobVoyage)source.Job;
			StmChangeLog[] triggeringChangeLogs = queuedLog?.ChangeLogs.Cast<StmChangeLog>().ToArray();

			var eventContext = new MessageReceipientEventContext();
			if (triggeringChangeLogs != null && triggeringChangeLogs.Length > 0)
			{
				foreach (var depCTO in GetOriginsFromChangeLogs(voyage, triggeringChangeLogs))
				{
					eventContext.Origins.Add(depCTO);
					eventContext.HasLocationReference = true;
				}

				foreach (var arvCTO in GetDestinationsFromChangeLogs(voyage, triggeringChangeLogs))
				{
					eventContext.Destinations.Add(arvCTO);
					eventContext.HasLocationReference = true;
				}
			}

			StmALog triggeringLog;
			if (queuedLog != null && TryGetTriggeringLogFromQueuedLog(queuedLog, source.Trigger, source.Job, out triggeringLog) && !triggeringLog.SL_Reference.IsEmpty)
			{
				var depCTO = GetOriginFromLogReference(voyage, triggeringLog);

				if (depCTO != null)
				{
					eventContext.Origins.Add(depCTO);
				}

				var arvCTO = GetDestinationFromLogReference(voyage, triggeringLog);

				if (arvCTO != null)
				{
					eventContext.Destinations.Add(arvCTO);
				}

				eventContext.HasLocationReference = true;
			}

			voyage.Factory.SetValue(() => eventContext);
			return base.GetWorkflowTriggerActionCore(source, queuedLog);
		}

		protected override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
		{
			var voyage = (JobVoyage)bizObj;

			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.Carrier:
					messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(voyage.Line, ZString.Empty));
					break;

				case MessageRecipientPartyTypeList.Codes.DepartureCTO:
					{
						var eventContext = voyage.Factory.GetValue<MessageReceipientEventContext>();

						IEnumerable<VoyageOrigin> origins = eventContext == null || !eventContext.HasLocationReference
							? voyage.Origins.Cast<VoyageOrigin>()
							: eventContext.Origins;

						foreach (var depCTO in origins.Select(GetDepartureCTO))
						{
							messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(depCTO));
						}
					}
					break;

				case MessageRecipientPartyTypeList.Codes.ArrivalCTO:
					{
						var eventContext = voyage.Factory.GetValue<MessageReceipientEventContext>();

						IEnumerable<VoyageDestination> destinations = eventContext == null || !eventContext.HasLocationReference
							? voyage.Destinations.Cast<VoyageDestination>()
							: eventContext.Destinations;

						foreach (var arvCTO in destinations.Select(GetArrivalCTO))
						{
							messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(arvCTO));
						}
					}
					break;
			}
		}

		#region DepartureCTO

		IEnumerable<VoyageOrigin> GetOriginsFromChangeLogs(JobVoyage voyage, StmChangeLog[] triggeringChangeLogs)
		{
			IEnumerable<VoyageOrigin> result = null;

			if (triggeringChangeLogs != null && triggeringChangeLogs.Length > 0)
			{
				result = triggeringChangeLogs
					.Where(log => log.SY_ParentTableCode == JobVoyOriginSchema.Constants.Prefix)
					.Select(log => voyage.Origins.Cast<VoyageOrigin>().FirstOrDefault(origin => origin.PK == log.SY_ParentID));
			}

			return result ?? Enumerable.Empty<VoyageOrigin>();
		}

		VoyageOrigin GetOriginFromLogReference(JobVoyage voyage, StmALog trigerringLog)
		{
			VoyageOrigin result = null;
			string locationUnloco;

			if (trigerringLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, out locationUnloco))
			{
				result = voyage.Origins
					.Cast<VoyageOrigin>()
					.FirstOrDefault(origin => origin.JA_RL_NKPortOfLoading == locationUnloco);
			}

			return result;
		}

		OrgAddress GetDepartureCTO(VoyageOrigin origin)
		{
			return origin != null && origin.DepartureCTOAddress != null
				? origin.DepartureCTOAddress : null;
		}

		#endregion

		#region ArrivalCTO

		IEnumerable<VoyageDestination> GetDestinationsFromChangeLogs(JobVoyage voyage, StmChangeLog[] triggeringChangeLogs)
		{
			IEnumerable<VoyageDestination> result = null;

			if (triggeringChangeLogs != null && triggeringChangeLogs.Length > 0)
			{
				result = triggeringChangeLogs
					.Where(log => log.SY_ParentTableCode == JobVoyDestinationSchema.Constants.Prefix)
					.Select(log => voyage.Destinations.Cast<VoyageDestination>().FirstOrDefault(destination => destination.PK == log.SY_ParentID));
			}

			return result ?? Enumerable.Empty<VoyageDestination>();
		}

		VoyageDestination GetDestinationFromLogReference(JobVoyage voyage, StmALog trigerringLog)
		{
			VoyageDestination result = null;
			string locationUnloco;

			if (trigerringLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, out locationUnloco))
			{
				result = voyage.Destinations
					.Cast<VoyageDestination>()
					.FirstOrDefault(origin => origin.JB_RL_NKPortOfDischarge == locationUnloco);
			}

			return result;
		}

		OrgAddress GetArrivalCTO(VoyageDestination destination)
		{
			return destination != null && destination.ArrivalCTOAddress != null
				? destination.ArrivalCTOAddress : null;
		}

		#endregion

		#region MessageReceipientEventContext

		class MessageReceipientEventContext
		{
			public HashSet<VoyageOrigin> Origins
			{
				get { return origins ?? (origins = new HashSet<VoyageOrigin>()); }
			}
			HashSet<VoyageOrigin> origins;

			public HashSet<VoyageDestination> Destinations
			{
				get { return destinations ?? (destinations = new HashSet<VoyageDestination>()); }
			}
			HashSet<VoyageDestination> destinations;

			public bool HasLocationReference { get; set; }
		}

		#endregion

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.JobSeaVoyage; }
		}
	}
}
