using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.UniversalData
{
	public static class WorkflowListHelper
	{
		public static CodeDescriptionPairList GetCachedEventCodeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("EventCodeList", GetNewEventCodeList);
		}

		static CodeDescriptionPairList GetNewEventCodeList()
		{
			var result = new CodeDescriptionPairList();
			foreach (Event type in Events.All)
			{
				result.AddPair(type.Code, type.Description);
			}
			result.SortByDescription();
			return result;
		}

		public static CodeDescriptionPairList GetCachedPurposeCodeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ProcessTaskTriggerPurposeList", ProcessTaskNotificationLookups.GetNewProcessTaskTriggerPurposeList);
		}

		public static CodeDescriptionPairList GetRecipientTypeList(WorkflowDescriptor workflowDescriptor, string actionType = null)
		{
			var supportedPartyTypes = MessageRecipientPartyType.None;
			if (workflowDescriptor != null)
			{
				supportedPartyTypes = workflowDescriptor.SupportedMessageRecipientParties(null, null);
				if (!string.IsNullOrEmpty(actionType))
				{
					var specificSupportedPartyTypes = workflowDescriptor.SupportedMessageRecipientPartiesForSpecificAction(actionType);
					if (specificSupportedPartyTypes != MessageRecipientPartyType.None)
					{
						supportedPartyTypes |= specificSupportedPartyTypes;
					}
				}
			}
			var result = new CodeDescriptionPairList();
			foreach (PartyTypeDescriptionPair parentPair in new MessageRecipientPartyTypeList(supportedPartyTypes))
			{
				if (parentPair.Code != MessageRecipientPartyTypeList.Codes.Email
					&& parentPair.Code != MessageRecipientPartyTypeList.Codes.Print)
				{
					result.AddPair(parentPair.Code, parentPair.Description);
				}
			}

			result.AddPair(MessageRecipientPartyTypeList.SpecialCodes.Other, ResString.GetMultilingualString("4dd72365-85f7-45a9-8e9e-e2c12992468b", "Other"));
			return result;
		}

		#region TriggerPartyServiceList

		public static CodeDescriptionPairList GetCachedTriggerPartyServiceList(BusinessObjectFactory factory, WorkflowDescriptor descriptor, ZString triggerParty)
		{
			Argument.NotNull(factory, nameof(factory));
			CodeDescriptionPairList result = null;

			if (descriptor != null && !triggerParty.IsEmpty)
			{
				var key = FormattableString.Invariant($"TriggerPartyServices|{descriptor.Code}|{triggerParty}");
				result = factory.GetCachedValue(key, () =>
				{
					var list = new CodeDescriptionPairList();
					var serviceCodeList = new ServiceCodesList();
					var triggerPartyCodes = descriptor.SupportedTriggerPartyServices(triggerParty);
					Array.ForEach(triggerPartyCodes, c => list.AddPair(c, serviceCodeList.GetDescriptionFromCode(c)));

					return list;
				});
			}

			return result ?? new CodeDescriptionPairList();
		}

		#endregion
	}
}
