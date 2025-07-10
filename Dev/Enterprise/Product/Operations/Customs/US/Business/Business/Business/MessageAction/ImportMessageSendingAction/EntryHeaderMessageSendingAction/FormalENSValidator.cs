using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public static class FormalENSValidator
	{
		public static ZString[] GetEntrySummaryMessageErrors(CusEntryHeader entry, ImportMessageSendingMessageType messageType, bool pSCReasonRequired = false)
		{
			var result = new List<ZString>();
			var declaration = entry.Declaration;
			if (declaration.RelatedStatement != null && !declaration.US_PSC)
			{
				result.Add(ValidationConstants.EntrySummary.AlreadyOnStatement("entry summary"));
			}

			if (messageType == ImportMessageSendingMessageType.Original && entry != null)
			{
				if (entry.HasBeenLodgedAtCustoms)
				{
					var lastAcceptedBrokerReference = GetLastAcceptedBrokerReference(entry);

					if (!lastAcceptedBrokerReference.IsEmpty && !declaration.US_BRDRefNo.IsEmpty && lastAcceptedBrokerReference != ((ICusEntryHeader)entry).BrokerReferenceNumber)
					{
						result.Add(ValidationConstants.EntrySummary.BrokerReferenceNumberDifferentAndReplacementShouldBeSent(lastAcceptedBrokerReference));
					}
				}
				else if (declaration.IsPSCFilingOfEntriesByOtherFiler && declaration.US_BRDRefNo.IsEmpty)
				{
					result.Add(ValidationConstants.EntrySummary.PSCFilingOfEntriesFiledByOtherBroker);
				}
			}

			if (declaration.IsACE)
			{
				if (entry.HasBeenCancelled)
				{
					result.Add(ValidationConstants.EntrySummary.HasBeenCancelled);
				}

				if (pSCReasonRequired)
				{
					result.Add(ValidationConstants.PSC.NoReasonCodeEntered);
				}
			}

			return result.ToArray();
		}

		static ZString GetLastAcceptedBrokerReference(CusEntryHeader entry)
		{
			var query = new ZQuery(EDIMessageSchema.EM_MessageType, new string[] { ACEApplicationIdentifierCodeList.Codes.EntrySummary, ApplicationIdentifierCodeList.Codes.EntrySummary });
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + " desc";

			MQEDIMessage acceptedMessage = null;
			foreach (MQEDIMessage message in entry.Messages.Find(query))
			{
				var response = message.ResponseMessage;
				if (response != null && response.IsENSCleared)
				{
					acceptedMessage = message;
					break;
				}
			}

			var result = ZString.Empty;
			if (acceptedMessage != null)
			{
				if (acceptedMessage.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.EntrySummary)
				{
					var ens10 = acceptedMessage.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
					result = ens10 != null ? ens10.BrokerReferenceNumber : ZString.Empty;
				}
				else
				{
					var ens20 = acceptedMessage.MessageBlock.MessageBlocks.OfType<ENS20>().FirstOrDefault();
					result = ens20 != null ? ens20.BrokerReferenceNumber : ZString.Empty;
				}
			}

			return result;
		}

		public static ZString CheckSendRLFSecurity(CusEntryHeader entry, Guid registryCompanyPK, string messageSendingType)
		{
			var result = ZString.Empty;

			var branchPortsRelations = USCustomsDataRegistry.Instance.BranchDistrictPortRelationship.GetFallBackValueAtAllLevels(registryCompanyPK, Guid.Empty, Guid.Empty);
			if (branchPortsRelations.Count > 0 && !GlbStaff.CurrentUser.IsSupportUser)
			{
				var userBranch = GlbStaff.CurrentUser.HomeBranch;
				if (userBranch == null)
				{
					result = string.Format(CultureInfo.CurrentCulture, ValidationConstants.Declaration.BranchIsNotConfiguredUnableToSendMsg, messageSendingType);
				}
				else
				{
					var mapping = branchPortsRelations.Cast<BranchDistrictPort>().FirstOrDefault(x => x.BranchPK == userBranch.PK &&
										entry.DistrictPortOfEntry.StartsWith(x.PortCode, StringComparison.OrdinalIgnoreCase));

					if (mapping == null && !entry.IsRemoteLocationFiling)
					{
						result = string.Format(CultureInfo.CurrentCulture, ValidationConstants.Declaration.NotRLFJob, messageSendingType);
					}
					else if (mapping != null && entry.IsRemoteLocationFiling)
					{
						result = string.Format(CultureInfo.CurrentCulture, ValidationConstants.Declaration.RLFJob, messageSendingType);
					}
				}
			}
			return result;
		}
	}
}
