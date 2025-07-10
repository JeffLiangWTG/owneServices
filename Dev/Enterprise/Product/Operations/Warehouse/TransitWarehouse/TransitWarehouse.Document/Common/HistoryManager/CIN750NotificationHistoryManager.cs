using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Warehouse.Transit.Document
{
	public abstract class CIN750NotificationHistoryManager<TBusinessObject> where TBusinessObject : EnterpriseBusinessObject
	{
		public TBusinessObject Parent;

		public List<NotificationHistoryInfo> HistoryInfo;

		public List<NotificationHistoryInfo> InHistory => GetHistoryByLogType(CIN750NotificationMessageTypes.CIN750InNotification);
		public List<NotificationHistoryInfo> CorHistory => GetHistoryByLogType(CIN750NotificationMessageTypes.CIN750CorNotification);
		public List<NotificationHistoryInfo> DeconsHistory => GetHistoryByLogType(CIN750NotificationMessageTypes.CIN750DeconsNotification);
		public List<NotificationHistoryInfo> ConsHistory => GetHistoryByLogType(CIN750NotificationMessageTypes.CIN750ConsNotification);
		public List<NotificationHistoryInfo> OutHistory => GetHistoryByLogType(CIN750NotificationMessageTypes.CIN750OutNotification);

		public string GetNotificationName(CIN750NotificationMessageTypes? type)
		{
			switch (type)
			{
				case CIN750NotificationMessageTypes.CIN750InNotification:
					return Res.GetString("19a7d14a-64fb-4635-8ea0-f1a8e780d669", "CIN 750 In Notification");
				case CIN750NotificationMessageTypes.CIN750CorNotification:
					return Res.GetString("0ab2dfd4-b65a-4c5f-9055-88a8e22da141", "CIN 750 Cor Notification");
				case CIN750NotificationMessageTypes.CIN750DeconsNotification:
					return Res.GetString("1f26cac7-1196-462e-8cba-c15633eb5a1d", "CIN 750 Deconsolidation Notification");
				case CIN750NotificationMessageTypes.CIN750ConsNotification:
					return Res.GetString("9b746d8c-f676-466b-aa8c-e31c1a152f67", "CIN 750 Consolidation Notification");
				case CIN750NotificationMessageTypes.CIN750OutNotification:
					return Res.GetString("778298b7-a660-4619-b971-07440e5a23c7", "CIN 750 Out Notification");
				default:
					return string.Empty;
			}
		}

		protected List<NotificationHistoryInfo> GetHistoryByLogType(CIN750NotificationMessageTypes messageType)
		{
			return HistoryInfo.FindAll(h => h.LogType.StartsWith(messageType.ToString()));
		}

		public string GetCheckSendingNotificationMessage(CIN750NotificationMessageTypes messageType) => GetNextMessageTypeCore(messageType).ErrorMessage;

		public CIN750NotificationMessageTypes? GetNextMessageType() => GetNextMessageTypeCore().Type;

		public abstract (CIN750NotificationMessageTypes? Type, string ErrorMessage) GetNextMessageTypeCore(CIN750NotificationMessageTypes? messageType = null);

		protected string GetErrorMessage(CIN750NotificationMessageTypes? notificationWanted, CIN750NotificationMessageTypes? notificationSuggested, string reason)
		{
			if (!notificationWanted.HasValue || notificationWanted == notificationSuggested)
			{
				if (!notificationWanted.HasValue)
				{
					return Res.GetString("6f6e611f-176e-411d-96a3-024b5185c544", "Cannot send CIN 750 Notification because {0}.", reason);
				}
				else
				{
					return string.Empty;
				}
			}
			else if (notificationSuggested.HasValue)
			{
				return Res.GetString("e2f76393-4ec6-4340-8a8f-62eaa02aa186", "Cannot send {0} because {1}. Please send {2}.", GetNotificationName(notificationWanted), reason, GetNotificationName(notificationSuggested));
			}
			else
			{
				return Res.GetString("c87feeaf-5220-43aa-9137-a2c11f0959a1", "Cannot send {0} because {1}.", GetNotificationName(notificationWanted), reason);
			}
		}

		protected List<NotificationHistoryInfo> GetCIN750NotificationHistory(BusinessObject bo)
		{
			var result = new List<NotificationHistoryInfo>();
			var logs = GetMessageSentLogs(bo);
			foreach (var log in logs)
			{
				var parameters = StmALog.GetParametersFromReference(log.SL_Reference);
				if (parameters.TryGetValue(EventReferenceParameters.Codes.MessageType, out var messageType))
				{
					if (parameters.TryGetValue(EventReferenceParameters.Codes.ParentType, out var refType)
						&& parameters.TryGetValue(EventReferenceParameters.Codes.ReferenceNumber, out var reference)
						&& parameters.TryGetValue(EventReferenceParameters.Codes.JobNumber, out var jobID)
						&& parameters.TryGetValue(EventReferenceParameters.Codes.MasterBill, out var masterbill)
						&& parameters.TryGetValue(EventReferenceParameters.Codes.HouseBill, out var housebill)
						&& parameters.TryGetValue(EventReferenceParameters.Codes.Weight, out var weightStr) && ZDecimal.TryParse(weightStr, out var weight)
						&& parameters.TryGetValue(EventReferenceParameters.Codes.OuterPackQuantity, out var quantityStr) && ZInt.TryParse(quantityStr, out var quantity))
					{
						var history = new NotificationHistoryInfo()
						{
							LogType = messageType,
							LogCreatedTime = log.SL_EventTime,
							RefType = refType,
							Reference = reference,
							Quantity = quantity,
							Weight = weight,
							MasterBill = masterbill,
							HouseBill = housebill,
							JobID = jobID,
						};
						result.Add(history);
					}
				}
			}
			return result;
		}

		List<StmALog> GetMessageSentLogs(BusinessObject bo)
		{
			var historyLogsQuery = new ZQuery(StmALogSchema.SL_Parent, bo.PK);
			historyLogsQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.MessageSentCode);
			historyLogsQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			return bo.Factory.Load<StmALog>(historyLogsQuery).OrderByDescending(log => log.SL_EventTime).ToList();
		}
	}

	public class NotificationHistoryInfo
	{
		public static string FromHistorySuffix => "_From";
		public static string ToHistorySuffix => "_To";
		public ZString LogType { get; set; }
		public ZDateTime LogCreatedTime { get; set; }
		public ZString RefType { get; set; }
		public ZString Reference { get; set; }
		public ZString JobID { get; set; }
		public ZString MasterBill { get; set; }
		public ZString MasterBillWithoutHyphen => MasterBill.Replace("-", ZString.Empty);
		public ZString HouseBill { get; set; }
		public ZInt Quantity { get; set; }
		public ZDecimal Weight { get; set; }
		public ZString RefCode => RefType == RefTypeStrings.ReferenceTypeStr ? Reference : RefType == RefTypeStrings.MasterBillTypeStr ? MasterBill : HouseBill;
		public bool IsFromRCN => JobID.StartsWith("RC") && LogType.EndsWith(FromHistorySuffix);
		public bool IsFromDCN => JobID.StartsWith("DC") && LogType.EndsWith(FromHistorySuffix);
		public bool IsTo => LogType.EndsWith(ToHistorySuffix);
	}

	public class HistoryPackageGroup
	{
		public int Quantity { get; set; }
		public decimal Weight
		{
			get => weight;
			set
			{
				weight = value.RoundTo3Digits();
			}
		}
		decimal weight;

		public string RefType { get; set; }
		public string RefCode { get; set; }
		public WhsItemReceiveConsignment ParentRCN { get; set; }
	}

	public class OutNotificationAdditionalData
	{
		public int PackageQuantityReadyToOut { get; set; }
		public decimal PackageWeightReadyToOut
		{
			get => weight;
			set
			{
				weight = value.RoundTo3Digits();
			}
		}
		decimal weight;
	}

	public class ConsNotificationAdditionalData
	{
		public List<HistoryPackageGroup> RCNHistoryPackageGroups { get; set; }
		public HistoryPackageGroup DCNPackageGroupForHWB { get; set; }
		public HistoryPackageGroup DCNPackageGroupForAWB { get; set; }
		public int PackageQuantityToCons { get; set; }
		public decimal PackageWeightToCons
		{
			get => weight;
			set
			{
				weight = value.RoundTo3Digits();
			}
		}
		decimal weight;
	}

	public static class RefTypeStrings
	{
		public const string ReferenceTypeStr = "REF";
		public const string MasterBillTypeStr = "AWB";
		public const string HouseBillTypeStr = "HWB";
	}

	public static class CIN750WeightExtension
	{
		public static decimal RoundTo3Digits(this decimal value)
		{
			return ZArchitecture.Core.Utilities.Round(value, 3);
		}

		public static ZDecimal RoundTo3Digits(this ZDecimal value)
		{
			return ZArchitecture.Core.Utilities.Round(value, 3);
		}
	}
}
