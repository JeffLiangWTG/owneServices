using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.GPS.Business;

namespace Enterprise.Freight.LocalCartage.Business.GPS
{
	[DebuggerDisplay("Event: {EventTypeCode} Time: {EventTime} Seq.: {EventLegSequence} Info:{EventShortInfo}")]
	public class GPSEvent : NonPersistentBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Scope = "type")]
		internal class Schema
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
			public static readonly string EventTime = "EventTime";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
			public static readonly string EventTypeCode = "EventTypeCode";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
			public static readonly string EventLegSequence = "EventLegSequence";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
			public static readonly string EventShortInfo = "EventShortInfo";
		}

		public CommonCartageLeg EventLeg
		{ get; set; }

		public ZGuid EventPK
		{ get; set; }

		[MaxLength(AutoLocalCartageVehicleActivity.Schema.EN_ActivityInformationMaxLength)]
		[ResourceStringData("GPSEvent|EventShortInfo", Caption = "Event Information", ShortCaption = "Event Info.")]
		public ZString EventShortInfo
		{
			get { return fEventShortInfo; }
			set { SetNonPersistentPropertyValue(EventShortInfoInfo, ref fEventShortInfo, value); }
		}
		ZString fEventShortInfo;

		public ZPropertyInfo EventShortInfoInfo
		{
			get { return GetZPropertyInfo(Schema.EventShortInfo); }
		}

		public bool EventShortInfo_ReadOnly
		{
			get { return true; }
		}

		[ResourceStringData("GPSEvent|EventTime", Caption = "Event Time", ShortCaption = "Time")]
		public ZDateTime EventTime
		{
			get { return fEventTime; }
			set { SetNonPersistentPropertyValue(EventTimeInfo, ref fEventTime, value); }
		}
		ZDateTime fEventTime;

		public ZPropertyInfo EventTimeInfo
		{
			get { return GetZPropertyInfo(Schema.EventTime); }
		}

		public bool EventTime_ReadOnly
		{
			get { return true; }
		}

		[MaxLength(12)]
		[ResourceStringData("GPSEvent|EventTypeCode", Caption = "Event Type", ShortCaption = "Type")]
		public ZString EventTypeCode
		{
			get { return fEventTypeCode; }
			set { SetNonPersistentPropertyValue(EventTypeCodeInfo, ref fEventTypeCode, value); }
		}
		ZString fEventTypeCode;

		public ZPropertyInfo EventTypeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.EventTypeCode); }
		}

		public bool EventTypeCode_ReadOnly
		{
			get { return true; }
		}

		[ResourceStringData("GPSEvent|EventTypeDescription", Caption = "Event Type Desc.", ShortCaption = "Type", FullDescription = "Event Type Description")]
		public ZString EventTypeDescription
		{
			get
			{
				return GPSInOutActivityType.GetDescriptionFromCode(EventTypeCode);
			}
		}

		GPSConstants.GPSInOutActivityType GPSInOutActivityType
		{
			get { return gpsInOutActivityType ?? (gpsInOutActivityType = new GPSConstants.GPSInOutActivityType()); }
		}
		GPSConstants.GPSInOutActivityType gpsInOutActivityType;

		[MaxLength(20)]
		[ResourceStringData("GPSEvent|EventLegSequence", Caption = "Sequence", ShortCaption = "Seq.", FullDescription = "Leg Sequence")]
		public ZString EventLegSequence
		{
			get { return fEventLegSequence; }
			set { SetNonPersistentPropertyValue(EventLegSequenceInfo, ref fEventLegSequence, value); }
		}
		ZString fEventLegSequence;

		public ZPropertyInfo EventLegSequenceInfo
		{
			get { return GetZPropertyInfo(Schema.EventLegSequence); }
		}

		public bool EventLegSequence_ReadOnly
		{
			get { return true; }
		}
	}
}
