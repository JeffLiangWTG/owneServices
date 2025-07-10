using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.JobDeclarationExtensions;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.US;
using Enterprise.UniversalDataBuss.Management;
using IInBondWarehouseIntegrationSupporter = Enterprise.Customs.US.InBond.Business.WarehouseExtensions.IInBondWarehouseIntegrationSupporter;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class InBondMessageSendingHeaderObject : NonPersistentBusinessObject
	{
		public InBondMessageSendingHeaderObject(ZGuid headerPK, InBondMessageType messageType, Customs.Business.ISendsMessagesToCustoms messageInitiator)
			: base(new BusinessObjectFactory())
		{
			Header = Argument.NotNull(Factory.Load<CusInBondHeader>(headerPK), "headerPK doesn't exists");
			Header.MessageInitiator = messageInitiator;
			Header.RecalculateValidationModesOnHeader(messageType);
			this.MessageType = messageType;
		}

		public InBondMessageSendingHeaderObject(CusInBondHeader header, InBondMessageType messageType, Customs.Business.ISendsMessagesToCustoms messageInitiator)
			: base(header.Factory)
		{
			Header = Argument.NotNull(header, nameof(header));
			header.MessageInitiator = messageInitiator;
			Header.RecalculateValidationModesOnHeader(messageType);
			this.MessageType = messageType;
		}

		public readonly CusInBondHeader Header;
		public readonly InBondMessageType MessageType;

		public bool IsWarehouseType
		{
			get
			{
				return MessageType == InBondMessageType.BondedWarehouseCancel ||
					MessageType == InBondMessageType.BondedWarehouseUpdate;
			}
		}

		[ChildEditable]
		public InBondMessageSendingObjectCollection SendingObjects
		{
			get
			{
				if (sendingObjects == null)
				{
					sendingObjects = new InBondMessageSendingObjectCollection(this);
					RegisterEditableChildObject(sendingObjects);
				}
				return sendingObjects;
			}
		}
		InBondMessageSendingObjectCollection sendingObjects;

		public bool SendData()
		{
			var result = false;
			if (IsWarehouseType)
			{
				result = SendWhsTransaction();
			}
			else
			{
				result = SendCustomsMessage();
			}
			return result;
		}

		bool SendWhsTransaction()
		{
			var result = true;
			if (MessageType == InBondMessageType.BondedWarehouseCancel)
			{
				result = Header.MessageInitiator.YesNoQuery("Are you sure you want to cancel the Inventory stock release for the In-Bond Movement? \r\nIf you click 'Yes', all stock of the In-Bond Movement will be uncommitted.", "Are you sure?");
			}
			if (result)
			{
				result = ContinueWithNotifications(this) != SendType.DoNotSend;

				if (result)
				{
					foreach (var obj in GetObjectsMarkedForSending())
					{
						obj.SendWhsTransaction();
					}
				}
			}
			return result;
		}

		InBondMessageSendingObject[] GetObjectsMarkedForSending()
		{
			return SendingObjects.OfType<InBondMessageSendingObject>().Where(x => x.US_ShouldSend).OrderBy(y => y.US_InBondNumber).ToArray();
		}

		public bool HasNotificationsOnObjectsMarkedForSending()
		{
			var result = false;
			foreach (var sendingObject in GetObjectsMarkedForSending())
			{
				sendingObject.RunPreSaveValidation();
				if (sendingObject.HasNotifications())
				{
					result = true;
					break;
				}
			}
			return result;
		}

		bool SendCustomsMessage()
		{
			var result = false;
			SendType sendType = ContinueWithNotifications(Header);

			if (sendType != SendType.DoNotSend)
			{
				var restoreToPreMessagingStateList = new List<Action>();
				try
				{
					var hasWarehouseIntegrationFailed = false;
					var sendingObjs = GetObjectsMarkedForSending();
					foreach (var sendingObj in sendingObjs)
					{
						Func<PublishToUniversalResult> preMessagingAction = null;
						Action restoreToPreMessagingState = null;
						var warehouseIntegrationSupporter = sendingObj.moveHeader as IInBondWarehouseIntegrationSupporter;
						bool isBondedWarehouse = SetupBondedWarehousePreMessagingActionForOutward(warehouseIntegrationSupporter, ref preMessagingAction, ref restoreToPreMessagingState);
						if (isBondedWarehouse)
						{
							if (preMessagingAction != null)
							{
								if (MessageType == InBondMessageType.DepartureAdd)
								{
									sendingObj.moveHeader.AllocateInBondNumberIfNeeded();
								}
								if (warehouseIntegrationSupporter.MessageInitiator.IsPublishToUniversalTransactionOK(preMessagingAction()))
								{
									restoreToPreMessagingStateList.Add(restoreToPreMessagingState);
								}
								else
								{
									hasWarehouseIntegrationFailed = true;
									break;
								}
							}
						}
					}

					var count = 0;
					if (!hasWarehouseIntegrationFailed)
					{
						foreach (var sendingObj in sendingObjs)
						{
							var message = sendingObj.Send();
							if (message != null)
							{
								message.EM_SendWithMessageErrors = sendType == SendType.SendWithError;
								count++;
							}
						}
					}
					if (count > 0)
					{
						if (IsAmendMessage())
						{
							count += CreatePendingAmendmentMessagesIfRequired();
						}

						Factory.Save();
						Header.MessageInitiator.NotifyUserOfASuccessfulSend(count > 1 ? string.Format("{0} messages were created.", count) : string.Format("{0} message was created.", count));
						restoreToPreMessagingStateList.Clear();
					}
					result = true;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
				finally
				{
					if (restoreToPreMessagingStateList.Count > 0)
					{
						restoreToPreMessagingStateList.ForEach(restoreAction => restoreAction());
					}
				}
			}
			return result;
		}

		int CreatePendingAmendmentMessagesIfRequired()
		{
			int result = 0;
			foreach (CusInBondMoveHeader moveHeader in Header.MovementHeaders)
			{
				if (moveHeader.BM_CustomsStatus == ImportMessageStatusList.Codes.AwaitingDepartureWithdraw)
				{
					moveHeader.GeneratePendingOriginalForAmendment(MessageType);
					moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureAmendment;
					result++;
				}
			}

			return result;
		}

		enum SendType { Send, SendWithError, DoNotSend }

		SendType ContinueWithNotifications(BusinessObject bizObj)
		{
			var result = SendType.DoNotSend;
			var notifications = Enterprise.Customs.Business.MessageSendingValidation.New(this, null).CheckBusinessObjectLevelValidation();

			if (notifications.ContainsError())
			{
				Header.MessageInitiator.NotifyUserOfAnInvalidOperation(string.Format("{0}\n{1}", Header.HumanReadableName, notifications.NotificationsAsString()));
			}
			else if (notifications.ContainsWarning())
			{
				if (Header.MessageInitiator.YesNoQuery(notifications.NotificationsAsString(), ValidationConstants.Header.ContinueMessage))
				{
					result = SendType.SendWithError;
				}
			}
			else
			{
				notifications = Enterprise.Customs.Business.MessageSendingValidation.New(bizObj, null).CheckBusinessObjectLevelValidation();
				if (notifications.ContainsError())
				{
					Header.MessageInitiator.NotifyUserOfAnInvalidOperation(string.Format("{0}\n{1}", Header.HumanReadableName, notifications.NotificationsAsString()));
				}
				else if (notifications.ContainsWarning())
				{
					if (Header.MessageInitiator.YesNoQuery(notifications.NotificationsAsString(), ValidationConstants.Header.ContinueMessage))
					{
						result = SendType.SendWithError;
					}
				}
				else
				{
					result = SendType.Send;
				}
			}
			return result;
		}

		bool SetupBondedWarehousePreMessagingActionForOutward(IInBondWarehouseIntegrationSupporter supporter, ref Func<PublishToUniversalResult> preMessagingAction, ref Action restoreToPreMessagingState)
		{
			var isBondedWarehouse = supporter != null &&
				supporter.IsActive && !supporter.IsBondedWarehousingDisabled &&
				(supporter.HasWHSTransaction() || supporter.IsExBondAutomationEnabled);
			if (isBondedWarehouse)
			{
				switch (MessageType)
				{
					case InBondMessageType.DepartureDelete:
						if (supporter.HasWHSTransaction())
						{
							preMessagingAction = () => supporter.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
							restoreToPreMessagingState = supporter.PublishAcceptEventForWHSOutwardInADifferentFactory;
						}
						break;
					case InBondMessageType.DepartureAdd:
					case InBondMessageType.DepartureAmend:
						if (supporter.HasWHSTransaction())
						{
							preMessagingAction = supporter.PublishShipmentForWHSOutwardWithPreAmendmentData;
							restoreToPreMessagingState = supporter.RestoreLatestClearedBondedWarehouseOutwardInADifferentFactory;
						}
						else
						{
							preMessagingAction = () => supporter.PublishShipmentForWHSOutward(true);
							restoreToPreMessagingState = supporter.PublishCancelEventForWHSOutwardInADifferentFactory;
						}
						break;
					default:
						isBondedWarehouse = false;
						break;
				}
			}
			return isBondedWarehouse;
		}

		bool IsAmendMessage()
		{
			return MessageType == InBondMessageType.DepartureAmend || MessageType == InBondMessageType.AirInBondAmend;
		}

		public virtual IEnumerable<CusInBondMoveHeader> GetMovementHeadersForSending()
		{
			return Header.MovementHeaders.OfType<CusInBondMoveHeader>();
		}
	}
}
