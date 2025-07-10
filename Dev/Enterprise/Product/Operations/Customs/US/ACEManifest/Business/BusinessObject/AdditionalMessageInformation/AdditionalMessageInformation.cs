using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AdditionalMessageInformation : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string AM_FlightNo = "AM_FlightNo";
			public const string AM_FlightReference = "AM_FlightReference";
			public const string AM_FlightArrivalDate = "AM_FlightArrivalDate";
			public const string AM_FlightDepartureTime = "AM_FlightDepartureTime";
			public const string AM_IsConsolidation = "AM_IsConsolidation";
			public const string AM_IsSplitShipment = "AM_IsSplitShipment";
			public const string AM_BoardedQty = "AM_BoardedQty";
			public const string AM_BoardedWeight = "AM_BoardedWeight";
			public const string AM_BoardedWeightUQ = "AM_BoardedWeightUQ";
			public const string AM_Agent = "AM_Agent";
		}

		#endregion

		public AdditionalMessageInformation(AsycudaManifestHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}

		readonly AsycudaManifestHeader header;
		public AsycudaManifestHeader Header => header;

		#region AM_FlightNo

		[MaxLength(10)]
		public ZString AM_FlightNo
		{
			get { return fFlightNo; }
			set
			{
				CheckMaximumLength(FlightNoInfo, value);
				SetNonPersistentPropertyValue(FlightNoInfo, ref fFlightNo, value);
			}
		}
		ZString fFlightNo;

		public ZPropertyInfo FlightNoInfo
		{
			get { return GetZPropertyInfo(Schema.AM_FlightNo); }
		}

		#endregion

		#region AM_FlightArrivalDate

		public ZDate AM_FlightArrivalDate
		{
			get { return fFlightArrivalDate; }
			set { SetNonPersistentPropertyValue(FlightArrivalDateInfo, ref fFlightArrivalDate, value); }
		}
		ZDate fFlightArrivalDate;

		public ZPropertyInfo FlightArrivalDateInfo
		{
			get { return GetZPropertyInfo(Schema.AM_FlightArrivalDate); }
		}

		#endregion

		#region AM_FlightDepartureTime

		public ZDateTime AM_FlightDepartureTime
		{
			get { return fAM_FlightDepartureTime; }
			set
			{
				SetNonPersistentPropertyValue(AM_FlightDepartureTimeInfo, ref fAM_FlightDepartureTime, value);
				ValidateAM_FlightDepartureTime();
			}
		}
		ZDateTime fAM_FlightDepartureTime;

		public ZPropertyInfo AM_FlightDepartureTimeInfo
		{
			get { return GetZPropertyInfo(Schema.AM_FlightDepartureTime); }
		}

		void ValidateAM_FlightDepartureTime()
		{
			AM_FlightDepartureTimeInfo.ClearAllNotifications();
			if (AM_FlightDepartureTimeIsActive)
			{
				if (AM_FlightDepartureTime.IsEmpty || !AM_FlightDepartureTime.IsValid)
				{
					AM_FlightDepartureTimeInfo.AddError(ResString.GetMultilingualString("368A5298-6A18-4977-93F8-9B44038CDF2B", "Lift Off Time is required."));
				}
				else if (FlightDepartureTimeUTC.IsInTheFutureUtc())
				{
					AM_FlightDepartureTimeInfo.AddError(ResString.GetMultilingualString("7A17D69F-3242-419E-8408-A840E148C89D", "Lift Off Time cannot be in the future."));
				}
			}
		}

		public void InitialiseFlightDepartureTime()
		{
			AM_FlightDepartureTime = header.AMA_E_DEP;
			AM_FlightDepartureTimeIsActive = true;
		}

		public ZDateTime FlightDepartureTimeUTC
		{
			get
			{
				var result = AM_FlightDepartureTime.IsValid ? AM_FlightDepartureTime : ZDateTime.Empty;
				if (!result.IsEmpty && header.PortOfLoading?.TimeZoneSet is RefTimeZoneSet timeZoneSet)
				{
					result = timeZoneSet.GetCalculationTimeZone().ToUniversalTime(result.ToDateTime());
				}

				return result;
			}
		}

		bool AM_FlightDepartureTimeIsActive { get; set; }

		#endregion

		#region AM_FlightReference

		[MaxLength(35)]
		public ZString AM_FlightReference
		{
			get { return fFlightReference; }
			set
			{
				CheckMaximumLength(ReferenceInfo, value);
				SetNonPersistentPropertyValue(ReferenceInfo, ref fFlightReference, value);
			}
		}
		ZString fFlightReference;

		public ZPropertyInfo ReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.AM_FlightReference); }
		}

		#endregion

		#region AM_IsConsolidation

		public ZBool AM_IsConsolidation
		{
			get
			{
				if (fAM_IsConsolidation.HasValue)
				{
					return (ZBool)fAM_IsConsolidation;
				}
				else
				{
					return true;
				}
			}
			set { SetNonPersistentPropertyValue(AM_IsConsolidationInfo, ref fAM_IsConsolidation, value); }
		}
		ZBool? fAM_IsConsolidation;

		public ZPropertyInfo AM_IsConsolidationInfo
		{
			get { return GetZPropertyInfo(Schema.AM_IsConsolidation); }
		}

		#endregion

		#region AM_IsSplitShipment

		public ZBool AM_IsSplitShipment
		{
			get { return fAM_IsSplitShipment; }
			set
			{
				SetNonPersistentPropertyValue(AM_IsSplitShipmentInfo, ref fAM_IsSplitShipment, value);
				if (!value)
				{
					AM_BoardedQty = 0;
					AM_BoardedWeight = 0;
					AM_BoardedWeightUQ = ZString.Empty;
				}
			}
		}
		ZBool fAM_IsSplitShipment;

		public ZPropertyInfo AM_IsSplitShipmentInfo
		{
			get { return GetZPropertyInfo(Schema.AM_IsSplitShipment); }
		}

		#endregion

		#region AM_BoardedQty

		[ResourceStringData("AdditionalMessageInformation.AM_BoardedQty", Caption = "Boarded Qty.", FullDescription = "The quantity loaded for this split shipment")]
		public ZInt AM_BoardedQty
		{
			get { return fAM_BoardedQty; }
			set
			{
				SetNonPersistentPropertyValue(AM_BoardedQtyInfo, ref fAM_BoardedQty, value);
				if (!IsValidationSuspended)
				{
					ValidateAM_BoardedQty();
				}
			}
		}
		ZInt fAM_BoardedQty;

		public ZPropertyInfo AM_BoardedQtyInfo
		{
			get { return GetZPropertyInfo(Schema.AM_BoardedQty); }
		}

		public bool AM_BoardedQty_ReadOnly
		{
			get { return !AM_IsSplitShipment; }
		}

		void ValidateAM_BoardedQty()
		{
			AM_BoardedQtyInfo.ClearAllNotifications();
			if (AM_IsSplitShipment)
			{
				MandatoryValidation.MessageErrorIfNotEntered(AM_BoardedQtyInfo, "Boarded Qty. This value is required for Split Shipments.");
			}
		}

		#endregion

		#region AM_BoardedWeight

		[ResourceStringData("AdditionalMessageInformation.AM_BoardedWeight", Caption = "Boarded Weight")]
		public ZDecimal AM_BoardedWeight
		{
			get { return fAM_BoardedWeight; }
			set
			{
				SetNonPersistentPropertyValue(AM_BoardedWeightInfo, ref fAM_BoardedWeight, value);
				if (!IsValidationSuspended)
				{
					ValidateAM_BoardedWeight();
				}
			}
		}
		ZDecimal fAM_BoardedWeight;

		public ZPropertyInfo AM_BoardedWeightInfo
		{
			get { return GetZPropertyInfo(Schema.AM_BoardedWeight); }
		}

		public bool AM_BoardedWeight_ReadOnly
		{
			get { return !AM_IsSplitShipment; }
		}

		void ValidateAM_BoardedWeight()
		{
			AM_BoardedWeightInfo.ClearAllNotifications();
			if (AM_IsSplitShipment)
			{
				MandatoryValidation.MessageErrorIfNotEntered(AM_BoardedWeightInfo, "Boarded Weight. This value is required for Split Shipments.");
			}
		}

		#endregion

		#region AM_BoardedWeightUQ

		[ResourceStringData("AdditionalMessageInformation.AM_BoardedWeightUQ", Caption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(AdditionalMessageInformationLookups.BoardedWeightUQList))]
		public ZString AM_BoardedWeightUQ
		{
			get { return fAM_BoardedWeightUQ; }
			set
			{
				SetNonPersistentPropertyValue(AM_BoardedWeightUQInfo, ref fAM_BoardedWeightUQ, value);
				if (!IsValidationSuspended)
				{
					ValidateAM_BoardedWeightUQ();
				}
			}
		}
		ZString fAM_BoardedWeightUQ;

		public ZPropertyInfo AM_BoardedWeightUQInfo
		{
			get { return GetZPropertyInfo(Schema.AM_BoardedWeightUQ); }
		}

		public bool AM_BoardedWeightUQ_ReadOnly
		{
			get { return !AM_IsSplitShipment; }
		}

		void ValidateAM_BoardedWeightUQ()
		{
			AM_BoardedWeightUQInfo.ClearAllNotifications();
			if (AM_IsSplitShipment)
			{
				MandatoryValidation.MessageErrorIfNotEntered(AM_BoardedWeightUQInfo, "Boarded Weight Unit of Quantity. This value is required for Split Shipments.");
				ListValidation.ErrorIfInvalidCode(AM_BoardedWeightUQInfo, Lookups.BoardedWeightUQList);
			}
		}

		#endregion

		#region AM_Agent

		[ResourceStringData("AdditionalMessageInformation.AM_Agent", Caption = "Agent")]
		[MaxLength(7)]
		public ZString AM_Agent
		{
			get { return fAM_Agent; }
			set
			{
				CheckMaximumLength(AM_AgentInfo, value);
				SetNonPersistentPropertyValue(AM_AgentInfo, ref fAM_Agent, value);
			}
		}
		ZString fAM_Agent;

		public ZPropertyInfo AM_AgentInfo
		{
			get { return GetZPropertyInfo(Schema.AM_Agent); }
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAM_BoardedQty();
			ValidateAM_BoardedWeight();
			ValidateAM_BoardedWeightUQ();
			ValidateAM_FlightDepartureTime();
		}

		[ReadOnly(true)]
		[ChildEditable]
		public FlightDetailCollection FlightArrivalDetails
		{
			get
			{
				if (fFlightArrivalDetails == null)
				{
					fFlightArrivalDetails = new FlightDetailCollection(header);
					fFlightArrivalDetails.InitialiseFlightArrival();
					RegisterEditableChildObject(fFlightArrivalDetails);
				}
				return fFlightArrivalDetails;
			}
		}
		FlightDetailCollection fFlightArrivalDetails;

		public AdditionalMessageInformationLookups Lookups => fLookups ?? (fLookups = GetNewLookups());
		AdditionalMessageInformationLookups fLookups;

		protected virtual AdditionalMessageInformationLookups GetNewLookups() => new AdditionalMessageInformationLookups(this);
	}
}
