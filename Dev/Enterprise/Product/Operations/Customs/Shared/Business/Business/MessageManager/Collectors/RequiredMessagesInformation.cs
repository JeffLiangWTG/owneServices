using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// It contains informations about messages required to be sent
	/// </summary>
	public class RequiredMessagesInformation
	{
		public RequiredMessagesInformation(IMessageManageableBizObj bizObj)
		{
			bridges = new List<ManagerCollectorSingleMessageManagerBridge>();

			IBackDoorSavingSupportableBizObj backDoorSuppertableBizObj = bizObj as IBackDoorSavingSupportableBizObj;
			if (backDoorSuppertableBizObj != null)
			{
				AmendmentWithdrawalReason = backDoorSuppertableBizObj.GetAmendmentWithdrawalReason();
				SupportBackDoorForSavingWhenAmendmentDetected = backDoorSuppertableBizObj.SupportBackDoorForSavingWhenAmendmentDetected;
			}
		}

		public bool IsInProcessOfDetectingRequiredMessages
		{
			get { return fIsInProcessOfDetectingRequiredMessages; }
			set
			{
				fIsInProcessOfDetectingRequiredMessages = value;
				if (value)
				{
					ClearBridgesAndReInitialise();
				}
			}
		}
		bool fIsInProcessOfDetectingRequiredMessages;

		public void ClearBridgesAndReInitialise()
		{
			bridges.Clear();
			Initialise();
		}

		readonly List<ManagerCollectorSingleMessageManagerBridge> bridges;

		public readonly AmendmentWithdrawalReason AmendmentWithdrawalReason;
		public readonly bool SupportBackDoorForSavingWhenAmendmentDetected;

		public bool HasMessagesToSend
		{
			get
			{
				CheckIsInProcessOfDetectingRequiredMessages("HasMessagesToSend");
				return bridges.Count > 0;
			}
		}

		public bool HasMessageManagersWaitingForResponses
		{
			get
			{
				CheckIsInProcessOfDetectingRequiredMessages("HasMessageManagersWaitingForResponses");
				foreach (SingleMessageManager messageManager in ManagersToSendMessagesFor)
				{
					if (messageManager.IsWaitingForResponse)
					{
						return true;
					}
				}
				return false;
			}
		}

		public string FullDescriptionsForMessagesToSend
		{
			get
			{
				CheckIsInProcessOfDetectingRequiredMessages("FullDescriptionsForMessagesToSend");
				ZStringBuilder result = new ZStringBuilder();

				foreach (SingleMessageManager singleManager in ManagersToSendMessagesFor)
				{
					result.Append("\r\n\t" + singleManager.MessageFriendlyName);
				}

				return result.ToString();
			}
		}

		public string MessageTypesToSend
		{
			get
			{
				CheckIsInProcessOfDetectingRequiredMessages("MessageTypesToSend");
				if (HasMessagesToSend)
				{
					if (IsThereOriginal)
					{
						if (IsThereAmendmentOrWithdrawal)
						{
							return Res.GetString("2205717a-bb16-4f15-acb8-c742a361e64e", "original(s) and amendment(s)");
						}
						else
						{
							return Res.GetString("1796a1a6-f22f-4938-bf10-bb822ae0fc15", "original(s)");
						}
					}
					else
					{
						return Res.GetString("2ee6ec07-ddbf-4422-ba88-9dec5a11cb28", "amendment(s)");
					}
				}
				else
				{
					return "";
				}
			}
		}

		public IEnumerable<SingleMessageManager> ManagersToSendMessagesFor
		{
			get
			{
				CheckIsInProcessOfDetectingRequiredMessages("ManagersToSendMessagesFor");
				foreach (ManagerCollectorSingleMessageManagerBridge bridge in bridges)
				{
					foreach (SingleMessageManager manager in bridge.Managers)
					{
						yield return manager;
					}
				}
			}
		}

		public IEnumerable<BusinessObject> BizObjsToSendMessageForForAmendmentOrWithdrawal
		{
			get
			{
				CheckIsInProcessOfDetectingRequiredMessages("BizObjsToSendMessageForForAmendmentOrWithdrawal");
				foreach (ManagerCollectorSingleMessageManagerBridge bridge in bridges)
				{
					if (bridge.ManagerCollector.IsAmendment || bridge.ManagerCollector.IsWithdrawal)
					{
						foreach (SingleMessageManager manager in bridge.Managers)
						{
							yield return manager.BusinessObject;
						}
					}
				}
			}
		}

		public IEnumerable<ManagerCollector> ManagerCollectors
		{
			get
			{
				CheckIsInProcessOfDetectingRequiredMessages("ManagerCollectors");
				foreach (ManagerCollectorSingleMessageManagerBridge bridge in bridges)
				{
					yield return bridge.ManagerCollector;
				}
			}
		}

		public MessageSendingNotificationCollection AllNotifications
		{
			get
			{
				CheckIsInProcessOfDetectingRequiredMessages("AllNotifications");

				if (fAllNotifications == null)
				{
					fAllNotifications = new MessageSendingNotificationCollection();
					foreach (ManagerCollector collector in ManagerCollectors)
					{
						fAllNotifications.AddRange(collector.Notifications);
					}
				}
				return fAllNotifications;
			}
		}
		MessageSendingNotificationCollection fAllNotifications;

		public bool IsThereAmendmentOrWithdrawal
		{
			get
			{
				CheckIsInProcessOfDetectingRequiredMessages("IsThereAmendmentOrWithdrawal");
				if (!isThereAmendmentOrWithdrawalCalculated)
				{
					isThereAmendmentOrWithdrawalCalculated = true;

					foreach (ManagerCollectorSingleMessageManagerBridge bridge in bridges)
					{
						if (bridge.ManagerCollector.IsAmendment || bridge.ManagerCollector.IsWithdrawal)
						{
							fIsThereAmendmentOrWithdrawal = true;
							break;
						}
					}
				}
				return fIsThereAmendmentOrWithdrawal;
			}
		}
		bool isThereAmendmentOrWithdrawalCalculated;
		bool fIsThereAmendmentOrWithdrawal;

		public bool IsThereOriginal
		{
			get
			{
				CheckIsInProcessOfDetectingRequiredMessages("IsThereOriginal");
				if (!isThereOriginalCalculated)
				{
					isThereOriginalCalculated = true;

					foreach (ManagerCollectorSingleMessageManagerBridge bridge in bridges)
					{
						if (bridge.ManagerCollector.IsOriginal)
						{
							fIsThereOriginal = true;
							break;
						}
					}
				}
				return fIsThereOriginal;
			}
		}
		bool isThereOriginalCalculated;
		bool fIsThereOriginal;

		#region Implementation

		internal void AddBridge(ManagerCollectorSingleMessageManagerBridge bridge)
		{
			bridges.Add(bridge);
			Initialise();
		}

		void CheckIsInProcessOfDetectingRequiredMessages(string nameOfMethod)
		{
			if (IsInProcessOfDetectingRequiredMessages)
			{
				throw new InvalidOperationException(string.Format("System is in the process of detecting required messages and accessing {0} at this point gives you incorrect information", nameOfMethod));
			}
		}

		void Initialise()
		{
			isThereOriginalCalculated = false;
			fIsThereOriginal = false;
			isThereAmendmentOrWithdrawalCalculated = false;
			fIsThereAmendmentOrWithdrawal = false;
			fAllNotifications = null;
		}

		#endregion

		#region For Testing
#if DEBUG

		public void AddAmendmentBridgeForTesting(SingleMessageManager[] managers)
		{
			AddBridge(new ManagerCollectorSingleMessageManagerBridge(new AmendmentManagerCollector(managers, false), managers));
		}
#endif
		#endregion
	}
}
