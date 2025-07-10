using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.Tracking.Business
{
	public class TrackingMilestone : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string SearchGridCaption = "SearchGridCaption";
			public const string Description = "Description";
			public const string ActualDate = "ActualDate";
			public const string EstimatedDate = "EstimatedDate";
			public const string DisplayDate = "DisplayDate";
			public const string Status = "Status";
			public const string Sequence = "Sequence";
			public const string ActualDateWithSuppression = "ActualDateWithSuppression";
			public const string EstimatedDateWithSuppression = "EstimatedDateWithSuppression";
			public const string ParentCode = "ParentCode";
		}

		#endregion

		public TrackingMilestone(ZString description, ZDateTimeOffset actualDate, ZDateTimeOffset estimatedDate, ZInt sequence)
		{
			this.description = description;
			this.actualDate = actualDate;
			this.estimatedDate = estimatedDate;
			this.sequence = sequence;
			this.eventCode = string.Empty;
		}

		public TrackingMilestone(ProcessTask task)
			: this(task, null)
		{
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public TrackingMilestone(ProcessTask task, IFlightDetailsSuppression suppressionDetails)
			: this(task.P9_Description, task.TriggerProperties.ActualDate, task.TriggerProperties.ScheduledDate, task.P9_Sequence)
		{
			this.task = task;
			this.suppressionDetails = suppressionDetails;
			this.eventCode = task.P9_SE_NKMilestoneEvent;
			this.parentCode = task.ParentCode;

			RegisterEditableChildObject(task);
		}

		public MultilingualString GenerateDetailsForEmailReporting()
		{
			return MultilingualString.Join(System.Environment.NewLine,
				ResString.GetMultilingualString("c02acae6-83f1-4fa0-aaeb-a9aae77b8edb", "Description: {0}", Description),
				ResString.GetMultilingualString("d7f6ac5a-e693-4f2f-a0bd-84ac2e5f7c50", "Estimated Date: {0}", WebDateTimeFormatter.GetFormattedDate(EstimatedDate, ZDateTimePickerFormat.Long)),
				ResString.GetMultilingualString("ebd71921-8068-453d-a248-81884a7ec451", "Actual Date: {0}", WebDateTimeFormatter.GetFormattedDate(ActualDate, ZDateTimePickerFormat.Long)),
				ResString.GetMultilingualString("b130dc3c-ab6b-4e86-aca3-21308a10cc49", "Status: {0}", Status));
		}

		#region IsSuppressionCheckRequired

		protected bool IsSuppressionCheckRequired
		{
			get { return SuppressionDetails != null && (IsArrival || IsDeparture); }
		}

		#endregion

		#region SuppressEstimagedDateType

		protected SuppressFields SuppressEstimagedDateType
		{
			get { return IsArrival ? SuppressFields.ETA : SuppressFields.ETD; }
		}

		#endregion

		#region SuppressActualDateType

		protected SuppressFields SuppressActualDateType
		{
			get { return IsArrival ? SuppressFields.ATA : SuppressFields.ATD; }
		}

		#endregion

		#region IsDeparture

		protected bool IsDeparture
		{
			get { return this.eventCode == AutoEvents.DepartureCode; }
		}

		#endregion

		#region IsArrival

		protected bool IsArrival
		{
			get { return this.eventCode == AutoEvents.ArrivalCode; }
		}

		#endregion

		#region Description

		public ZString EventCode
		{
			get
			{
				var result = eventCode;
				if (Task != null)
				{
					if (Task.MilestoneEvent != null)
					{
						result = Task.MilestoneEvent.SE_Code;
					}
				}
				return result;
			}
		}

		readonly ZString eventCode;

		public ZString Description
		{
			get { return description; }
		}
		readonly ZString description;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#region ParentCode

		public ZString ParentCode
		{
			get { return parentCode; }
		}
		readonly ZString parentCode;

		public ZPropertyInfo ParentCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ParentCode); }
		}
		#endregion

		#region ActualDate

		public ZDateTimeOffset ActualDate
		{
			get { return actualDate; }
			set
			{
				if (Task != null)
				{
					actualDate = value;
					// This usage of this proxy setter that will not result in defects,
					// But updates to ProcessTask  will not update the TrackingMilestone
					ObjectFactory.Get<IActualDateWorkAround>().SetActualDateAndIKnowIShouldNotBeCallingThis(Task, value);
					ActualDateInfo.RefreshBinding();
				}
			}
		}
		ZDateTimeOffset actualDate;

		public ZPropertyInfo ActualDateInfo
		{
			get { return GetZPropertyInfo(Schema.ActualDate); }
		}

		#endregion

		#region ActualDateWithSuppression

		public ZDateTimeOffset ActualDateWithSuppression
		{
			get { return IsSuppressionCheckRequired ? Suppression.GetWebValue(ActualDate, SuppressionDetails, SuppressActualDateType) : ActualDate; }
		}

		public ZPropertyInfo ActualDateWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.ActualDateWithSuppression); }
		}

		#endregion

		#region EstimatedDateWithSuppression

		public ZDateTimeOffset EstimatedDateWithSuppression
		{
			get { return IsSuppressionCheckRequired ? Suppression.GetWebValue(EstimatedDate, SuppressionDetails, SuppressEstimagedDateType) : EstimatedDate; }
		}

		public ZPropertyInfo EstimatedDateWithSuppressionInfo
		{
			get { return GetZPropertyInfo(Schema.EstimatedDateWithSuppression); }
		}

		#endregion

		#region EstimatedDate

		public ZDateTimeOffset EstimatedDate
		{
			get { return estimatedDate; }
			set
			{
				if (Task != null)
				{
					estimatedDate = value;
					Task.SetScheduledDate(value);
					EstimatedDateInfo.RefreshBinding();
				}
			}
		}
		ZDateTimeOffset estimatedDate;

		public ZPropertyInfo EstimatedDateInfo
		{
			get { return GetZPropertyInfo(Schema.EstimatedDate); }
		}

		#endregion

		#region DisplayDate

		public ZDateTimeOffset DisplayDate
		{
			get { return !ActualDate.IsEmpty ? ActualDateWithSuppression : EstimatedDateWithSuppression; }
		}

		public ZPropertyInfo DisplayDateInfo
		{
			get { return GetZPropertyInfo(Schema.DisplayDate); }
		}

		#endregion

		#region Status

		public ZString ActualStatus => task?.Status ?? TimelineHelper.GetProcessTaskWebStatus(ActualDate, EstimatedDate, false);

		public ZString Status => TimelineHelper.GetStatusAccordingToRegistry(ActualStatus);

		public ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(Schema.Status); }
		}

		#endregion

		#region Sequence

		public ZInt Sequence
		{
			get { return sequence; }
		}
		readonly ZInt sequence;

		public ZPropertyInfo SequenceInfo
		{
			get { return GetZPropertyInfo(Schema.Sequence); }
		}

		#endregion

		#region SuppressionDetails

		protected IFlightDetailsSuppression SuppressionDetails
		{
			get { return suppressionDetails; }
		}

		readonly IFlightDetailsSuppression suppressionDetails;

		#endregion

		#region Implementation

		public ProcessTask Task
		{
			get { return task; }
		}

		readonly ProcessTask task;

		#endregion
	}
}
