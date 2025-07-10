using System.Collections.Generic;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class CMDNotificationBuffer : NotificationBuffer
	{
		public CMDNotificationBuffer(BusinessObject parent)
		{
			this.Parent = parent;
		}

		public ZString GetErrorMessages()
		{
			StringBuilder result = new StringBuilder();
			Dictionary<BusinessObject, List<ZString>> errorMessageDictionary = GetMessagesGroupedByBizO(ErrorType.Error);

			// ParentBizO notifications shown first
			AppendLineIfNotEmpty(result, GetNotificationsForBizO(Parent, errorMessageDictionary));

			foreach (BusinessObject bizO in errorMessageDictionary.Keys)
			{
				if (bizO == Parent)
				{
					continue;
				}

				AppendLineIfNotEmpty(result, GetNotificationsForBizO(bizO, errorMessageDictionary));
			}

			return result.ToString().Trim();
		}

		public ZString GetInfoMessages()
		{
			StringBuilder result = new StringBuilder();
			Dictionary<BusinessObject, List<ZString>> infoMessageDictionary = GetMessagesGroupedByBizO(NotificationSubscriberType.Info);

			// ParentBizO notifications shown first
			AppendLineIfNotEmpty(result, GetNotificationsForBizO(Parent, infoMessageDictionary));

			foreach (BusinessObject bizO in infoMessageDictionary.Keys)
			{
				if (bizO == Parent)
				{
					continue;
				}

				AppendLineIfNotEmpty(result, GetNotificationsForBizO(bizO, infoMessageDictionary));
			}

			return result.ToString().Trim();
		}

		#region Implementation

		void AppendLineIfNotEmpty(StringBuilder builder, ZString message)
		{
			if (!message.IsEmpty)
			{
				builder.AppendLine(message);
			}
		}

		ZString GetNotificationsForBizO(BusinessObject bizO, Dictionary<BusinessObject, List<ZString>> messageDictionary)
		{
			StringBuilder result = new StringBuilder();

			if (messageDictionary.ContainsKey(bizO))
			{
				result.AppendLine(bizO.HumanReadableName);
				foreach (ZString message in messageDictionary[bizO])
				{
					result.AppendLine(message);
				}
			}

			return result.ToString();
		}

		Dictionary<BusinessObject, List<ZString>> GetMessagesGroupedByBizO(NotificationSubscriberType notificationType)
		{
			Dictionary<BusinessObject, List<ZString>> result = new Dictionary<BusinessObject, List<ZString>>();

			foreach (INotification notification in GetEventsByType(notificationType))
			{
				CMDNotification cMDNotification = notification as CMDNotification;
				BusinessObject bizO = (cMDNotification != null && cMDNotification.BizO != Parent) ? cMDNotification.BizO : Parent;
				List<ZString> messageList;
				if (!result.TryGetValue(bizO, out messageList))
				{
					messageList = new List<ZString>();
					result.Add(bizO, messageList);
				}
				INotificationSubscriberNotification subscriberNotification = notification as INotificationSubscriberNotification;
				if (subscriberNotification != null)
				{
					messageList.Add(subscriberNotification.AdditionalInfo);
				}
			}

			return result;
		}

		#endregion

		public readonly BusinessObject Parent;
	}
}
