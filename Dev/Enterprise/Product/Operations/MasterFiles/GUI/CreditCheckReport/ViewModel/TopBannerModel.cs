using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public class TopBannerModel : ModelBase<TopBannerModel>
	{
		bool statusImageAndDetailEventInfoVisible;
		string statusHeader;
		string statusDetails;
		string statusDetailsEventsInfo;
		readonly string organizationName;

		public TopBannerModel(CreditReportStatusType statusType, int eventsCount, string organizationName)
		{
			this.organizationName = organizationName;
			UpdateStatusTypeAndEventsCount(statusType, eventsCount);
		}

		public void UpdateStatusTypeAndEventsCount(CreditReportStatusType statusType, int eventsCount)
		{
			EventsCount = eventsCount;
			StatusType = statusType;

			switch (StatusType)
			{
				case CreditReportStatusType.NoEvent:
					StatusImage = null;
					StatusHeader = ResourceStringHelper.StatusHeaderNoEvent;
					StatusDetails = ResourceStringHelper.StatusDetailsNoEvent;
					break;

				case CreditReportStatusType.UpToDate:
					StatusImage = Properties.Resources.TickGreen;
					StatusHeader = ResourceStringHelper.StatusHeaderUpToDate;
					StatusDetails = ResourceStringHelper.StatusDetailsUpToDate(organizationName);
					break;

				case CreditReportStatusType.Warning:
					StatusImage = Properties.Resources.WarningRed;
					StatusHeader = ResourceStringHelper.StatusHeaderWarning;
					StatusDetails = ResourceStringHelper.StatusDetailsWarning(organizationName);
					break;
			}

			StatusDetailsEventsInfo = StatusType == CreditReportStatusType.NoEvent ? string.Empty : ResourceStringHelper.GetEventsInfo(EventsCount);
			StatusDetailsEventsInfoColor = StatusColorHelper.GetStatusColor(StatusType);
			StatusImageAndDetailEventInfoVisible = StatusType != CreditReportStatusType.NoEvent;
		}

		public void UpdateStatusTypeAndEventsCount(IEnumerable<CreditEvent> events)
		{
			if (events != null && events.Any())
			{
				UpdateStatusTypeAndEventsCount(CreditReportStatusType.Warning, events.Count());
			}
			else
			{
				UpdateStatusTypeAndEventsCount(CreditReportStatusType.UpToDate, 0);
			}
		}

		public string StatusHeader
		{
			get => statusHeader;
			private set
			{
				statusHeader = value;
				NotifyPropertyChanged();
			}
		}

		public string StatusDetails
		{
			get => statusDetails;
			private set
			{
				statusDetails = value;
				NotifyPropertyChanged();
			}
		}

		public string StatusDetailsEventsInfo
		{
			get => statusDetailsEventsInfo;
			set
			{
				statusDetailsEventsInfo = value;
				NotifyPropertyChanged();
			}
		}

		public Color StatusDetailsEventsInfoColor { get; private set; }

		public Image StatusImage { get; private set; }

		public bool StatusImageAndDetailEventInfoVisible
		{
			get => statusImageAndDetailEventInfoVisible;
			set
			{
				statusImageAndDetailEventInfoVisible = value;
				NotifyPropertyChanged();
			}
		}

		public CreditReportStatusType StatusType { get; private set; }

		int EventsCount { get; set; }
	}
}
