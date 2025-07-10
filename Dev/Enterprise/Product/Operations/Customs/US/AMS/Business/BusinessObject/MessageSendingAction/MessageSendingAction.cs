using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class MessageSendingAction : AutoMessageSendingAction
	{
		public MessageSendingAction(CusInBondHeader header, ActionCode actionCode, bool canSendMessageErrorsAsWarnings = true)
			: base(header.Factory)
		{
			this.Header = Argument.NotNull(header, "header");
			this.ActionCode = actionCode;
			this.CanSendMessageErrorsAsWarnings = canSendMessageErrorsAsWarnings;
			UpdateValidationModes();
			SetValidationModes(actionCode);
		}

		#region New Properties

		public MessageSendingMovementCollection Movements
		{
			get
			{
				if (movements == null)
				{
					movements = new MessageSendingMovementCollection(Factory);
					foreach (var moveHeader in GetMoveHeaders())
					{
						if (ActionCodeTool.IsVesselArrivalEventRelevent(ActionCode))
						{
							foreach (var movementDetail in moveHeader.MovementDetails)
							{
								if (movementDetail.Bill != null && !movements.Any(movement => ((MessageSendingMovement)movement).FirstSendingObject.MB_PortOfUnlading == movementDetail.Bill.B0_InBondPortOfDestDCode))
								{
									if (ActionCode == ActionCode.VesselArrival && movementDetail.Bill.B0_A_ARV.IsEmpty || ActionCode == ActionCode.ChangeEstDateOfArrival)
									{
										var movement = new MessageSendingMovement(movementDetail, ActionCode);
										movement.MM_SendInfo.ValueChanged += new EventHandler(MM_SendInfo_ValueChanged);
										movement.HasChanges = false;
										movements.Add(movement);
									}
								}
							}
						}
						else
						{
							var movement = new MessageSendingMovement(moveHeader, ActionCode);
							movement.MM_SendInfo.ValueChanged += new EventHandler(MM_SendInfo_ValueChanged);
							movement.HasChanges = false;
							movements.Add(movement);
						}
					}
					RegisterEditableChildObject(movements);
				}
				return movements;
			}
		}
		MessageSendingMovementCollection movements;

		void MM_SendInfo_ValueChanged(object sender, EventArgs e)
		{
			ReBuildMessageSendingObjects(sender as MessageSendingMovement);
		}

		IEnumerable<CusInBondMoveHeader> GetMoveHeaders()
		{
			if (ActionCodeTool.IsInBondType(ActionCode))
			{
				foreach (var moveHeader in Header.InBondMovementHeaders.OfType<CusInBondMoveHeader>())
				{
					yield return moveHeader;
				}
			}
			else if (ActionCodeTool.IsPermitToTransferAction(ActionCode))
			{
				foreach (var pttMovementHeader in Header.PTTMovements.OfType<CusInBondMoveHeader>())
				{
					yield return pttMovementHeader;
				}
			}
			else
			{
				yield return Header.MovementHeader;
			}
		}

		public MessageSendingObjectCollection MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					messageSendingObjects = new MessageSendingObjectCollection(Factory);

					if (ActionCodeTool.IsVesselArrivalEventRelevent(ActionCode))
					{
						foreach (var movement in Movements.Cast<MessageSendingMovement>().Where(x => x.MM_Send))
						{
							var bill = movement.MovementDetail?.Bill;
							var shouldSendMovement = (ActionCode == ActionCode.VesselArrival && bill != null && bill.B0_A_ARV.IsEmpty) || ActionCode == ActionCode.ChangeEstDateOfArrival;
							if (shouldSendMovement)
							{
								var sendingObj = movement.FirstSendingObject;
								if (sendingObj != null)
								{
									sendingObj.UpdateAction(ActionCode);
									sendingObj.ShouldValidateDate = () => movement.MM_Send;
									messageSendingObjects.Add(sendingObj);
								}
							}
						}
					}
					else
					{
						foreach (MessageSendingMovement movement in Movements)
						{
							if (movement.MM_Send)
							{
								if (ActionCodeTool.IsVesselEvent(ActionCode))
								{
									var firstSendingObj = movement.FirstSendingObject;
									if (firstSendingObj != null)
									{
										firstSendingObj.UpdateAction(ActionCode);
										firstSendingObj.ShouldValidateDate = () => movement.MM_Send;
										messageSendingObjects.Add(firstSendingObj);
									}
									break; //for vessel events one movement is enought, because data captured from header level
								}
								else
								{
									foreach (var moveDetail in movement.MovementDetails)
									{
										var obj = Factory.Load<MessageSendingObject>(moveDetail.PK);
										if (obj == null)
										{
											obj = new MessageSendingObject(moveDetail, ActionCode);
										}
										else
										{
											obj.UpdateAction(ActionCode);
										}
										messageSendingObjects.Add(obj);
									}
								}
							}
						}
					}

					RegisterEditableChildObject(messageSendingObjects);
				}
				return messageSendingObjects;
			}
		}
		MessageSendingObjectCollection messageSendingObjects;

		internal AMSMessageHelper AMSMessageHelper
		{
			get
			{
				if (fAMSMessageHelper == null)
				{
					fAMSMessageHelper = new AMSMessageHelper();
				}
				return fAMSMessageHelper;
			}
			set
			{
				fAMSMessageHelper = value;
			}
		}
		AMSMessageHelper fAMSMessageHelper;

		#endregion

		public int CreateAMSMessages()
		{
			var result = 0;
			AMSEDIMessage message = null;
			var beginTime = ZDateTime.UtcNow;

			foreach (var obj in ObjectsToSend)
			{
				if (obj.IsAmendingDelete)
				{
					message = new ACEAMSMessageBuilder(new DeleteMessageSendingObject(obj), obj.GetActualActionCode()).PopulateMessage();
					if (obj.MB_BillActionCode == AMSBillSendingActionCodeList.Codes.ReplaceEntireBillDeleteAndAdd)
					{
						obj.GeneratePendingOriginalAdd();
					}
				}
				else
				{
					message = new ACEAMSMessageBuilder(obj, obj.GetActualActionCode()).PopulateMessage();
					if (message != null && CanSendMessageErrorsAsWarnings)
					{
						message.EM_SendWithMessageErrors = obj.HasMessageErrors;
					}
				}

				if (result >= AMSMessageHelper.BatchSize && message != null)
				{
					message.EM_HeldUntilDate = beginTime.AddMinutes(result / AMSMessageHelper.BatchSize * AMSMessageHelper.Delay);
				}

				obj.PurgeActionCodeAndAmendmentCode();
				result++;
			}
			return result;
		}

		public IReadOnlyList<MessageSendingObject> ObjectsToSend
		{
			get
			{
				if (objectsToSendCached == null)
				{
					objectsToSendCached = new CachedProperty<IReadOnlyList<MessageSendingObject>>(Factory, delegate
					{
						var list = new List<MessageSendingObject>();
						foreach (MessageSendingObject msgData in MessageSendingObjects)
						{
							if (msgData.MB_Send)
							{
								list.Add(msgData);
							}
						}
						return list;
					});
				}
				return objectsToSendCached.Value;
			}
		}
		CachedProperty<IReadOnlyList<MessageSendingObject>> objectsToSendCached;

		public ZString GetWarningForEntitiesToSendThatAreWaitingForResponse()
		{
			var builder = new ZStringBuilder();
			foreach (var obj in ObjectsToSend)
			{
				if (AMSBillMessageStatusList.IsMessagingInProgressType(obj.MB_MessageStatus))
				{
					var details = new ZStringBuilder();
					details.AppendIfNotEmpty(obj.MB_IssuerCode);
					details.AppendIfNotEmpty(obj.MB_BillOfLadingSequenceNumber);
					builder.Append(details.ToStringWithDelimiterBetweenAppends(" "));
				}
			}
			return builder.IsEmpty ? "" : EntitiesToSendThatAreWaitingForResponseMessage(builder.ToStringWithNewLineBetweenAppends());
		}

		public static string EntitiesToSendThatAreWaitingForResponseMessage(string entities)
		{
			return Res.GetString("MessageSendingAction|94AFEA45-04D8-44FC-9020-769BE180E409", "The following entities have pending responses:\r\n\r\n{0}\r\n\r\nDo you still want to send messages to Customs?", entities);
		}

		public void ResetValidationModesAndUnRegisterBillAsEditableChildObject()
		{
			foreach (var validationModesSupporter in validationModesSupportersCached)
			{
				validationModesSupporter.Key.ValidationModes = validationModesSupporter.Value;
			}

			foreach (MessageSendingObject obj in MessageSendingObjects)
			{
				obj.UnRegisterBillAsEditableChildObject();
			}

			Header.ValidationModes = headerValidationMode;
		}

		public readonly CusInBondHeader Header;
		public readonly ActionCode ActionCode;
		public readonly bool CanSendMessageErrorsAsWarnings;
		readonly Dictionary<IValidationModesSupporter, ValidationModes> validationModesSupportersCached = new Dictionary<IValidationModesSupporter, ValidationModes>();
		void UpdateValidationModes()
		{
			headerValidationMode = Header.ValidationModes;
			foreach (var supporter in ValidationModesSupporters)
			{
				validationModesSupportersCached.Add(supporter, supporter.ValidationModes);
				supporter.ValidationModes = ValidationModes.None;
			}
		}
		ValidationModes headerValidationMode;

		IEnumerable<IValidationModesSupporter> ValidationModesSupporters
		{
			get
			{
				foreach (IValidationModesSupporter bill in Header.Bills)
				{
					yield return bill;
				}
				yield return Header.MovementHeader;
				foreach (IValidationModesSupporter pttMovement in Header.PTTMovements)
				{
					yield return pttMovement;
				}
				foreach (IValidationModesSupporter inBondMovement in Header.InBondMovementHeaders)
				{
					yield return inBondMovement;
				}
			}
		}

		void SetValidationModes(ActionCode actionCode)
		{
			var modes = ValidationModes.InventoryRecord;
			switch (actionCode)
			{
				case Messaging.Business.ActionCode.SubsequentInBondAmendment:
				case Messaging.Business.ActionCode.SubsequentInBondDelete:
				case Messaging.Business.ActionCode.SubsequentInBondOriginal:
					modes = ValidationModes.SubsequentInBond;
					break;
				case Messaging.Business.ActionCode.PermitToTransfer:
					modes = ValidationModes.PermitToTransfer;
					break;
				case Messaging.Business.ActionCode.InBondArrival:
					modes = ValidationModes.InBondArrival;
					break;
				case Messaging.Business.ActionCode.InBondExportation:
					modes = ValidationModes.InBondExportation;
					break;
				case Messaging.Business.ActionCode.InBondTransferOfLiability:
					modes = ValidationModes.InBondTOL;
					break;
				case ActionCode.CancelPermitToTransfer:
				case ActionCode.ChangeEstDateOfArrival:
					modes = ValidationModes.ChangeEstDateOfArrival;
					break;
				case ActionCode.VesselArrival:
					modes = ValidationModes.VesselArrival;
					break;
				case ActionCode.VesselDeparture:
					modes = ValidationModes.VesselDeparture;
					break;
				case ActionCode.AmendingAdd:
				case ActionCode.AmendingDelete:
				case ActionCode.AmendingUpdate:
					modes = ValidationModes.InventoryRecordAmendment;
					break;
			}
			Header.ValidationModes = modes;
		}

		void ReBuildMessageSendingObjects(MessageSendingMovement movement)
		{
			if (movement != null)
			{
				if (ActionCodeTool.IsVesselEvent(ActionCode))
				{
					var firstSendingObj = movement.FirstSendingObject;
					if (firstSendingObj != null)
					{
						firstSendingObj.UpdateAction(ActionCode);
						firstSendingObj.ShouldValidateDate = () => movement.MM_Send;

						if (movement.MM_Send)
						{
							if (MessageSendingObjects.FindByPK(firstSendingObj.PK) == null)
							{
								MessageSendingObjects.Add(firstSendingObj);
							}
						}
						else
						{
							if (MessageSendingObjects.FindByPK(firstSendingObj.PK) != null)
							{
								MessageSendingObjects.Remove(firstSendingObj);
							}
						}
					}
				}
				else
				{
					foreach (var moveDetail in movement.MovementDetails)
					{
						var messageSendingBill = Factory.Load<MessageSendingObject>(moveDetail.PK);
						if (movement.MM_Send)
						{
							if (messageSendingBill == null)
							{
								messageSendingBill = new MessageSendingObject(moveDetail, ActionCode);
							}
							else
							{
								messageSendingBill.UpdateAction(ActionCode);
							}
							if (MessageSendingObjects.FindByPK(messageSendingBill.PK) == null)
							{
								MessageSendingObjects.Add(messageSendingBill);
							}
						}
						else
						{
							if (messageSendingBill != null)
							{
								messageSendingBill.MB_Send = false;
								MessageSendingObjects.Remove(messageSendingBill);
							}
						}
					}
				}
				MessageSendingObjects.RefreshBindingIncludingChildren();
			}
		}
	}
}
