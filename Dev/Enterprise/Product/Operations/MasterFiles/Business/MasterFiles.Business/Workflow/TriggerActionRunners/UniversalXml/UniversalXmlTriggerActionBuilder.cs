using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public abstract class UniversalXmlTriggerActionBuilder
	{
		protected UniversalXmlTriggerActionBuilder(WorkflowDescriptor descriptor, ActionWrapper actionInfo, EventInfoProvider eventInfo)
		{
			Descriptor = descriptor;
			ActionInfo = actionInfo;
			EventInfo = eventInfo;
		}
		protected WorkflowDescriptor Descriptor { get; }
		protected ActionWrapper ActionInfo { get; }
		protected EventInfoProvider EventInfo { get; }
		protected abstract string FileFormat { get; }
		protected abstract ITopLevelDataObjectWriter GetTopLevelDataObjectWriter(IDataWritingManager outboundSessionManager);
		protected abstract BusinessObject GetParent();

		#region GetUniversalWorkflowProcessor

		public IUniversalXmlWorkflowProcessor GetUniversalWorkflowProcessor()
		{
			Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = GetTopLevelDataObjectWriter;
			var schemaOverride = Descriptor != null && Descriptor.UniversalDataContextManager != null ? Descriptor.UniversalDataContextManager.SchemaOverride : null;

			return UniversalXmlWorkflowProcessorBuilder.New(ActionInfo
				, GetInternalOrExternalCommunicationModes()
				, dataWriterGetter
				, GetParent()
				, EventInfo
				, null
				, schemaOverride);
		}

		IMessageProcessorCommunicationModesResult GetInternalOrExternalCommunicationModes()
		{
			EDICommunicationModeQuery GetModeQuery(string recipientRole)
			{
				return new EDICommunicationModeQuery(
					parent: ActionInfo.ParentBO,
					descriptor: Descriptor,
					fileFormat: FileFormat,
					purpose: ActionInfo.PurposeCode,
					recipientRole: recipientRole,
					eventCode: ActionInfo.TriggerEventCode,
					eventReference: ActionInfo.TriggerEventReference);
			}

			return new UniversalXmlCommunicationModeProvider(() =>
			{
				var communicationModes = new List<IEDICommunicationsMode>();
				var failureReasons = new List<MultilingualString>();

				var recipientRoleDetails = ActionInfo.RecipientRoleDetails;
				if (recipientRoleDetails != null)
				{
					var recipientOrganisation = ActionInfo.RecipientOrganization as OrgHeader;
					if (recipientOrganisation != null)
					{
						var recipient = new MessageRecipientParty(recipientOrganisation, ZString.Empty);
						foreach (var recipientRoleDetail in recipientRoleDetails)
						{
							var modeQuery = GetModeQuery(recipientRoleDetail.Type.ToString());
							var result = Descriptor.GetCommunicationModesForRecipient(recipient.Party, modeQuery);
							communicationModes.AddRange(result.communicationModes);
							failureReasons.Add(result.failureReason);
						}
					}
					else
					{
						foreach (var recipientRoleDetail in recipientRoleDetails)
						{
							var recipientRoleType = recipientRoleDetail.Type.ToString();
							var (messageRecipientParty, failureReason) = Descriptor.GetMessageRecipientPartyWithFailureReason(ActionInfo.ParentBO, recipientRoleType);

							if (failureReason != null)
							{
								failureReasons.Add(failureReason);
							}
							else if (messageRecipientParty == null || !messageRecipientParty.Any())
							{
								failureReasons.Add(ResString.GetMultilingualString("07916f7e-226f-4b4e-9ceb-d5c13154bad1", "Recipient Organization not found."));
							}
							else
							{
								var modeQuery = GetModeQuery(recipientRoleType);
								var currentCompany = GlbCompany.CurrentCompany;
								foreach (var recipient in messageRecipientParty)
								{
									var result = Descriptor.GetCommunicationModesForRecipient(recipient.Party, modeQuery);
									if (result.communicationModes.Length > 0)
									{
										communicationModes.AddRange(result.communicationModes);
										failureReasons.Add(ResString.GetMultilingualString("f161d240-8134-46ca-bb57-dbd9e6bc5318", "Organization [{0}] for Company [{1}].", recipient.Party?.OH_Code ?? ZString.Empty, currentCompany.GC_Code));
									}
									else
									{
										if (recipient.Party == null)
										{
											failureReasons.Add(ResString.GetMultilingualString("c266cf93-0343-4d03-8771-73c9d9dff414", "Company [{0}] has no Organization for [{1}].", currentCompany.GC_Code, recipientRoleType));
										}
										else
										{
											failureReasons.Add(ResString.GetMultilingualString("34cdbe84-345f-4a67-962d-adf74f54c0b3", "Organization [{0}] for Company [{1}] has no matching Communication Modes.", recipient.Party?.OH_Code, currentCompany.GC_Code));
										}
									}
								}
							}
						}
					}

					return (communicationModes.ToArray(), ResourceString.Join(System.Environment.NewLine, failureReasons.ToArray()));
				}
				else
				{
					return (Array.Empty<IEDICommunicationsMode>(), ResString.GetMultilingualString("e1af3a6b-51c4-425c-afb7-20a0f310400f", "There are no recipient role details."));
				}
			});
		}

		#endregion
	}
}
