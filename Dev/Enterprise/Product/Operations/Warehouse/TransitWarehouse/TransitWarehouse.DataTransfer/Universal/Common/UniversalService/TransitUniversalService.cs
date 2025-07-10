using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.Messaging.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class TransitUniversalService : ITransitUniversalService
	{
		public (string ErrorType, string Message) PublishUniversal(ZGuid[] jobPKs, string jobType)
		{
			var result = (ErrorType: string.Empty, Message: string.Empty);

			switch (jobType)
			{
				case WhsItemReceiveConsignmentSchema.Constants.TableName:
					var rcnQuery = new ZQuery(WhsItemReceiveConsignmentSchema.PK, jobPKs);
					var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(rcnQuery);
					result = PublishOutturn(receiveConsignments.ToDictionary(r => r.WRC_ConsignmentID, r => r));
					break;
				case WhsItemDispatchConsignmentSchema.Constants.TableName:
					var dcnQuery = new ZQuery(WhsItemDispatchConsignmentSchema.PK, jobPKs);
					var dispatchConsignments = Factory.Load<WhsItemDispatchConsignment>(dcnQuery);
					result = PublishOutturn(dispatchConsignments.ToDictionary(d => d.WDC_ConsignmentID, d => d));
					break;
				default:
					result.ErrorType = MessageTypes.Error;
					result.Message = Res.GetString("e71bdaaf-de3c-4990-9fc2-a7d27e617518", "The job type {0} is not yet supported.", jobType);
					break;
			}

			return result;
		}

		(string ErrorType, string Message) PublishOutturn<T>(Dictionary<ZString, T> bizODictionary) where T : IBusiness, IWorkflowProvider
		{
			var results = new List<(string ErrorType, string Message)>();

			foreach (var bizOEntry in bizODictionary.OrderBy(b => b.Key))
			{
				var ownerRecipientTypeMap = TransitUniversalJobLinkHelper.GetOwnerRecipientTypeSourceTypeMapByStmUniversalJobLink(bizOEntry.Value);

				if (ownerRecipientTypeMap.Count == 0)
				{
					results.Add((MessageTypes.Warning, bizOEntry.Key + ": " + Res.GetString("34c9293c-f40b-49fa-8160-d2d84693eb4a", "Outturn/Packs cannot update Forwarding Shipment or Customs Declaration as no match could be found.")));
					continue;
				}

				foreach (var (ownerPK, recipientType, recipientDescription) in ownerRecipientTypeMap)
				{
					try
					{
						using (bizOEntry.Value.Factory.AddDisposableService())
						using (var notifier = new TransitErrorMessageNotifier())
						{
							var eventReference = $"Send Outturn @ {ZDateTime.UtcNow} UTC"; // Event reference to find correct DEX event
							using (var dataExport = new ManualDataExport(bizOEntry.Value.Factory, bizOEntry.Value, UniversalDataType.UniversalShipment))
							{
								dataExport.EventReference = eventReference;
								dataExport.RecipientType = recipientType;
								dataExport.RecipientPK = ownerPK;

								var xmlEvents = dataExport.SendData(notifier);
								if (xmlEvents.Any())
								{
									var statusCode = xmlEvents.Select(e => e.Context.ProcessingStatusCode).FirstOrDefault(status => !status.IsEmpty);
									var sendResult = ManualDataExportHelper.GetMessage(
										statusCode,
										Res.GetString("ed70265e-45e3-46d9-a4ee-508115ec071d", "Outturn to {0}", recipientDescription),
										Res.GetString("cd1e058f-362d-44e4-806a-e1a62a4bd568", "job"));
									results.Add((sendResult.ErrorType, bizOEntry.Key + ": " + sendResult.Message));
								}
								else
								{
									results.Add((MessageTypes.Error, bizOEntry.Key + ": " + Res.GetString("e1066367-e23b-4d90-acdd-4e4ac5fca630", "Outturn to {0} failed to send. Check Booking Party EDI Communications.", recipientDescription)));
								}
							}

							bizOEntry.Value.Factory.Save();

							if (notifier.ErrorMessage.Count != 0)
							{
								results.Add((MessageTypes.Error, bizOEntry.Key + ": " + BuildErrorMessage(notifier.ErrorMessage, recipientDescription)));
							}
						}
					}
					catch (ZSaveConcurrencyException ex)
					{
						ZExceptionReporting.HandleSaveException(ex, NotificationHandler.Instance);
					}
				}
			}

			return GetFinalResult(results);
		}

		(string ErrorType, string Message) GetFinalResult(List<(string ErrorType, string Message)> results)
		{
			(string ErrorType, string Message) result;
			if (results.Any(r => r.ErrorType == MessageTypes.Error))
			{
				result.ErrorType = MessageTypes.Error;
			}
			else if (results.Any(r => r.ErrorType == MessageTypes.Warning))
			{
				result.ErrorType = MessageTypes.Warning;
			}
			else
			{
				result.ErrorType = MessageTypes.Success;
			}

			result.Message = string.Join(System.Environment.NewLine, results.Select(r => r.Message).ToList());

			return result;
		}

		public INotification GetNotificationForInstruction(string status, string failureReason)
		{
			INotification notification = null;
			var instructionMessage = ManualDataExportHelper.GetMessage(
				status,
				Res.GetString("bf82b65c-e9c6-4ada-810d-fa615e458de8", "Instruction"),
				Res.GetString("958a35c5-ac55-4260-b5d3-6e44e57365c9", "job"));
			switch (instructionMessage.ErrorType)
			{
				case MessageTypes.Success:
					notification = new InfoNotification(instructionMessage.Message);
					break;
				case MessageTypes.Error:
					var disPlayReason = "";

					if (failureReason.Length > 0)
					{
						var msg = InstructionMapper.GetMessageFromLog(failureReason);

						if (msg.Length > 0)
						{
							disPlayReason = Res.GetString("052f5a91-9b62-4f76-84f4-89686909c0ac", "{0}\r\nThe reason is: {1}", "", msg);
						}
					}

					notification = new ErrorNotification(ErrorType.Error, instructionMessage.Message + disPlayReason);
					break;
				case MessageTypes.Warning:
					notification = new WarningNotification(instructionMessage.Message);
					break;
			}

			return notification;
		}

		public INotification GetNotificationForStopLoadInstructionEvent(string status, bool isCancel)
		{
			INotification notification;
			if (IsSucessfullyProcessed(status))
			{
				notification = isCancel ?
					new InfoNotification(Res.GetString("3ac051ab-fe6e-4762-9c18-2f91aed182a9", "Successfully canceled Stop Load.")) :
					new InfoNotification(Res.GetString("1ee7a330-7d68-4575-a377-ec1623dca765", "Successfully stopped Load."));
			}
			else if (IsErrorStatus(status))
			{
				notification = isCancel ?
					new ErrorNotification(ErrorType.Error, Res.GetString("fe904b15-d96a-4e8f-ad7f-90e7188a2c62", "Could not cancel Stop Load. Check DEX logs for details.")) :
					new ErrorNotification(ErrorType.Error, Res.GetString("d7bb187b-af21-4d1e-bc9d-1fbf4c04c17a", "Could not stop Load. Check DEX logs for details."));
			}
			else
			{
				notification = new WarningNotification(Res.GetString("65626bda-4479-4a63-8125-f9c704b7b390", "Instruction has been queued to send. Check DEX logs for details."));
			}

			return notification;
		}

		bool IsErrorStatus(string status)
		{
			return status == EDIMessageStatusList.Codes.Error ||
				status == EDIMessageStatusList.Codes.Discarded ||
				status == EDIMessageStatusList.Codes.Cancelled ||
				status == EDIMessageStatusList.Codes.Withdrawn ||
				status == EDIMessageStatusList.Codes.Failed ||
				status == EDIMessageStatusList.Codes.Rejected;
		}

		bool IsSucessfullyProcessed(string status)
		{
			return status == EDIMessageStatusList.Codes.ProcessedOK ||
				status == EDIMessageStatusList.Codes.Warning;
		}

		string BuildErrorMessage(IEnumerable<string> errorMessages, ZString sourceType)
		{
			var message = new ZStringBuilder();
			foreach (var error in errorMessages)
			{
				message.AppendLine(error);
			}

			return $"{sourceType}: {message}";
		}

		public WhsTransitLogInstructionMapper InstructionMapper => instructionMapper = instructionMapper ?? new WhsTransitLogInstructionMapper();

		WhsTransitLogInstructionMapper instructionMapper;

		BusinessObjectFactory Factory
		{
			get
			{
				using (Db.DisposableActionForDbConnection())
				{
					return factory = factory ?? new BusinessObjectFactory();
				}
			}
		}

		BusinessObjectFactory factory;
	}
}
