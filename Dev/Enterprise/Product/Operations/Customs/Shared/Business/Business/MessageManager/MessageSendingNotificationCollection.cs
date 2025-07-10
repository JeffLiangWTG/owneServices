using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class MessageSendingNotificationCollection : IEnumerable<MessageSendingNotification>, IEnumerable
	{
		public MessageSendingNotificationCollection()
		{
			elements = new List<MessageSendingNotification>();
		}

		public void Add(MessageSendingNotification notification)
		{
			elements.Add(notification);
		}

		public void Clear()
		{
			elements.Clear();
		}

		public void AddError(string message)
		{
			Add(new MessageSendingError(message));
		}

		public void AddWarning(string message)
		{
			Add(new MessageSendingWarning(message));
		}

		public void AddInformation(string message)
		{
			Add(new MessageSendingInformation(message));
		}

		public MessageSendingNotification this[int index]
		{
			get { return elements[index]; }
		}

		public int Count
		{
			get { return elements.Count; }
		}

		public int WarningCount
		{
			get
			{
				int result = 0;
				foreach (MessageSendingNotification notification in elements)
				{
					if (notification.IsWarning)
					{
						result++;
					}
				}
				return result;
			}
		}

		public int ErrorCount
		{
			get
			{
				int result = 0;
				foreach (MessageSendingNotification notification in elements)
				{
					if (notification.IsError)
					{
						result++;
					}
				}
				return result;
			}
		}

		public int InformationCount
		{
			get
			{
				int result = 0;
				foreach (MessageSendingNotification notification in elements)
				{
					if (!notification.IsError && !notification.IsWarning)
					{
						result++;
					}
				}
				return result;
			}
		}

		public void AddRange(MessageSendingNotificationCollection collection)
		{
			elements.AddRange(collection);
		}

#if DEBUG
		#region Methods for Unit Tests
		public bool ContainsError(string errorText)
		{
			bool result = false;
			foreach (MessageSendingNotification notification in elements)
			{
				if (notification.IsError)
				{
					result |= notification.Message == errorText;
				}
			}
			return result;
		}

		public bool ContainsWarning(string errorText)
		{
			bool result = false;
			foreach (MessageSendingNotification notification in elements)
			{
				if (notification.IsWarning)
				{
					result |= notification.Message == errorText;
				}
			}
			return result;
		}

		public bool ContainsInformation(string errorText)
		{
			bool result = false;
			foreach (MessageSendingNotification notification in elements)
			{
				if (notification.IsInformation)
				{
					result |= notification.Message == errorText;
				}
			}
			return result;
		}
		#endregion
#endif
		public bool ContainsError()
		{
			foreach (MessageSendingNotification notification in elements)
			{
				if (notification.IsError)
				{
					return true;
				}
			}
			return false;
		}

		public bool ContainsWarning()
		{
			foreach (MessageSendingNotification notification in elements)
			{
				if (notification.IsWarning)
				{
					return true;
				}
			}
			return false;
		}

		public bool ContainsInformation()
		{
			foreach (MessageSendingNotification notification in elements)
			{
				if (notification.IsInformation)
				{
					return true;
				}
			}
			return false;
		}

		public ZString NotificationsAsString(int maxNotificationsToReport = 0)
		{
			ZStringBuilder result = new ZStringBuilder();
			int count = 0;
			foreach (MessageSendingNotification notification in elements)
			{
				count++;
				if (maxNotificationsToReport > 0 && count > maxNotificationsToReport)
				{
					result.Append(Res.GetString("83162151-2ec6-46fd-9c04-ca50aa3c04a6", "More notifications (not listed)...") + "\r\n");
					break;
				}
				result.Append(notification.Message + "\r\n");
			}
			return result.ToString();
		}

		public ZString ErrorNotificationsAsString()
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (MessageSendingNotification notification in elements)
			{
				if (notification.IsError)
				{
					result.Append(notification.Message);
				}
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		public ZString WarningNotificationsAsString()
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (MessageSendingNotification notification in elements)
			{
				if (notification.IsWarning)
				{
					result.Append(notification.Message);
				}
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		public ZString InformationNotificationsAsString()
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (MessageSendingNotification notification in elements)
			{
				if (notification.IsInformation)
				{
					result.Append(notification.Message);
				}
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		public MessageSendingNotification CreateSummaryNotification(bool includeErrorsAlone = false)
		{
			MessageSendingNotification summary = null;

			if (elements.Any())
			{
				var anyErrors = elements.Any(x => x.IsError);
				var anyWarnings = elements.Any(x => x.IsWarning);
				var anyInfo = elements.Any(x => x.IsInformation);

				var summaryMessage = ZString.Empty;

				if (elements.Count == 1)
				{
					summaryMessage = elements[0].Message;
				}
				else
				{
					var msg = new ZStringBuilder();
					if (anyErrors)
					{
						if ((anyWarnings || anyInfo) && !includeErrorsAlone)
						{
							msg.Append(Res.GetString("B8EA56EF-679F-4F5C-BAD0-A28E99354853", "Errors:"));
						}
						elements.Where(x => x.IsError).ToList().ForEach(x => msg.Append(x.Message));
					}

					if (anyWarnings && (!includeErrorsAlone || !anyErrors))
					{
						if (anyErrors || anyInfo)
						{
							msg.Append(Res.GetString("3937825D-5C65-4438-A81D-3AA9AAD8BD46", "Warnings:"));
						}
						elements.Where(x => x.IsWarning).ToList().ForEach(x => msg.Append(x.Message));
					}

					if (anyInfo && (!includeErrorsAlone || !anyErrors))
					{
						if (anyErrors || anyWarnings)
						{
							msg.Append(Res.GetString("7C0BF173-B6B6-4657-AC95-AB68F3C4E7AE", "Information:"));
						}
						elements.Where(x => x.IsInformation).ToList().ForEach(x => msg.Append(x.Message));
					}

					summaryMessage = msg.ToStringWithNewLineBetweenAppends();
				}

				if (anyErrors)
				{
					summary = new MessageSendingError(summaryMessage);
				}
				else if (anyWarnings)
				{
					summary = new MessageSendingWarning(summaryMessage);
				}
				else
				{
					summary = new MessageSendingInformation(summaryMessage);
				}
			}

			return summary;
		}

		#region Implementation

		readonly List<MessageSendingNotification> elements;

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		#endregion

		#region IEnumerable<MessageSendingNotification> Members

		IEnumerator<MessageSendingNotification> IEnumerable<MessageSendingNotification>.GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		#endregion
	}
}
