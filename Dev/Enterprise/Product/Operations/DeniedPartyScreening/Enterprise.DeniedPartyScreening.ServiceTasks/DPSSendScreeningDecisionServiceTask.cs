using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.DeniedPartyScreening.ServiceTasks.DPSSendScreeningDecisionServiceTask.Code,
	"Denied Party Screening – Send Screening Decisions To Server",
	"DPS",
	typeof(Enterprise.DeniedPartyScreening.ServiceTasks.DPSSendScreeningDecisionServiceTask),
	IsMandatory = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "1hour",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1day")]
[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.DeniedPartyScreening.ServiceTasks.DPSSendScreeningDecisionServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.DPSRequestMessage,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIInterchange.Direction.Transmit,
		EDIMessageSchema.Constants.EM_MessageType + "=" + EDIMessageTypeList.Codes.JDC,
		EDIMessageSchema.Constants.EM_MessageSubType + "=" + EDIMessageSubTypeList.Codes.ScreeningRequest,
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued
	},
	"Denied Party Screening Send Screening Decisions Service")]

namespace Enterprise.DeniedPartyScreening.ServiceTasks
{
	public class DPSSendScreeningDecisionServiceTask : ServiceProviderImpl
	{
		public const string Code = "DPM";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging")]
		public override void RunTask(CancellationToken token)
		{
			var branch = GlbBranch.GetFirstActiveBranch();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var serviceTaskHelper = new DpsServiceTaskHelper(Code);
				var factoryProvider = new BusinessObjectFactoryProvider();
				var nextBatchQuery = new ZQuery();
				nextBatchQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.DPSRequestMessage);
				nextBatchQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				nextBatchQuery.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.JDC);
				nextBatchQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.ScreeningRequest);
				nextBatchQuery.AddToFilter(EDIMessageSchema.EM_IsActive, true);
				nextBatchQuery.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Queued);
				nextBatchQuery.MaximumRows = OrganisationsDataRegistry.Instance.DPSSendScreeningDecisionServiceTaskBatchSize.Value;
				nextBatchQuery.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + ", " + EDIMessageSchema.EM_MessageNum.Name;
				nextBatchQuery.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum));

				try
				{
					DpsEDIMessage[] dpsEDIMessages;

					while ((dpsEDIMessages = factoryProvider.Current.Load<DpsEDIMessage>(nextBatchQuery)).Length > 0)
					{
						token.ThrowIfCancellationRequested();
						ServiceLogger?.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Retrieved {0} Screening Decision(s).", dpsEDIMessages.Length));
						SendScreeningDecisionsToServer(dpsEDIMessages, token);
						ServiceLogger?.Log(LogType.Information, $"Sent {dpsEDIMessages.Length} Screening Decision(s).");
						dpsEDIMessages.ForEach(dpsEDIMessage => dpsEDIMessage.EM_Status = EDIMessageStatusList.Codes.ProcessedOK);
						factoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
						ServiceLogger?.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Updated Screening Decision(s) As Processed."));
					}

					serviceTaskHelper.UpdateLastSuccessfulRuntime();
				}
				catch (WebException ex)
				{
					HttpDpsWebHelper.HandleWebException(ex, ServiceLogger, serviceTaskHelper);
				}
				catch (Exception ex) when (!ex.IsCriticalException() && !(ex is OperationCanceledException))
				{
					ServiceLogger?.Log(LogType.Error, ex.Message);

					if (!(ex is ZCannotSaveException || ex is ZSaveException || ex is AuthCertNotFoundException))
					{
						ErrorReporter.ReportOnce("Exception occurred while running DPM service task.", ex);
					}
				}
			}
		}

		void SendScreeningDecisionsToServer(IEnumerable<DpsEDIMessage> dpsEDIMessages, CancellationToken token)
		{
			var messageDtos = new List<DpsEDIMessageCandidatesSendObject>();

			foreach (var ediMessage in dpsEDIMessages)
			{
				token.ThrowIfCancellationRequested();
				var messageData = JsonConvert.DeserializeObject<DpsEDIMessageCandidatesContent>(ediMessage.EM_MessageData.ToUTF8());
				if (messageData != null)
				{
					if (messageData.DpsNameCandidates != null ||
						messageData.DpsAddressCandidates != null ||
						messageData.DpsCountryCandidates != null ||
						messageData.DpsRegistrationCodeCandidates != null)
					{
						messageDtos.Add(new DpsEDIMessageCandidatesSendObject { MessageContent = messageData, MessageNum = ediMessage.EM_MessageNum, MessageCreateUtc = ediMessage.EM_SystemCreateTimeUtc.ToDateTime() });
						ServiceLogger?.Log(LogType.Debug, $"Screening decision with Message PK {ediMessage.PK} and CSI {messageData.ClientSpecifiedIdentifier} is added to the web request");
					}
					else if (IsDpsRequestHeader(ediMessage, out var messageDataOldFormat))
					{
						var dpsEDIMessageCandidatesSendObject = new DpsEDIMessageCandidatesContent
						{
							ClientLicence = messageDataOldFormat.ClientLicence,
							DatabaseType = messageDataOldFormat.DatabaseType,
							ClientSpecifiedIdentifier = messageDataOldFormat.ClientSpecifiedIdentifier,
							EntityType = messageDataOldFormat.EntityType,
							PersistentStatus = messageDataOldFormat.PersistentStatus,
							ScreenTime = messageDataOldFormat.ScreenTime,
							DpsNameCandidates = messageDataOldFormat.DpsRequestHeader.DpsNameCandidates,
							DpsAddressCandidates = messageDataOldFormat.DpsRequestHeader.DpsAddressCandidates,
							DpsCountryCandidates = messageDataOldFormat.DpsRequestHeader.DpsCountryCandidates,
							DpsRegistrationCodeCandidates = messageDataOldFormat.DpsRequestHeader.DpsRegistrationCodeCandidates
						};
						messageDtos.Add(new DpsEDIMessageCandidatesSendObject { MessageContent = dpsEDIMessageCandidatesSendObject, MessageNum = ediMessage.EM_MessageNum, MessageCreateUtc = ediMessage.EM_SystemCreateTimeUtc.ToDateTime() });
						ServiceLogger?.Log(LogType.Debug, $"Screening decision with Message PK {ediMessage.PK} and CSI {messageData.ClientSpecifiedIdentifier} is added to the web request");
					}
					else
					{
						ErrorReporter.ReportOnce("DPM service task got invalid DpsRequestHeader", $"Screening decision with Message PK {ediMessage.PK} and CSI {messageData.ClientSpecifiedIdentifier} is not added to the web request as the Request Header is Null");
					}
				}
				else
				{
					ErrorReporter.ReportOnce("DPM service task got invalid DpsEDIMessageContent", $"Screening decision with Message PK {ediMessage.PK} is not added to the web request as Message Data is Null");
				}
			}

			if (messageDtos.Count > 0)
			{
				HttpDpsWebHelper.PostMatchDecisions(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageDtos)));
			}
		}

		bool IsDpsRequestHeader(DpsEDIMessage ediMessage, out DpsEDIMessageContent messageDataOldFormat)
		{
			messageDataOldFormat = null;
			var messageData = JsonConvert.DeserializeObject<DpsEDIMessageContent>(ediMessage.EM_MessageData.ToUTF8());
			if (messageData.DpsRequestHeader != null)
			{
				messageDataOldFormat = messageData;
				return true;
			}

			return false;
		}
	}
}
