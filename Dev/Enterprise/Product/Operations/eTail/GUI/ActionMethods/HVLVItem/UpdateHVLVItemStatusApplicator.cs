using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.GUI
{
	public class UpdateHVLVItemStatusApplicator : OperationalActionMethodApplicator
	{
		#region Schema

		public static class Schema
		{
			public const string StatusCode = "StatusCode";
			public const string STUEventTime = "STUEventTime";
			public const int StatusCodeMaxLength = 3;
		}

		#endregion

		public UpdateHVLVItemStatusApplicator(BusinessObjectFactory factory)
			: base("UpdateHVLVItemStatusApplicator", factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length == 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("775a5481-6076-48fd-b14c-587f14c5611e", "No HVLV Consignments selected."));
			}
			else
			{
				var consignmentPKs = targets.Select(bizo => bizo.PK);
				var items = Factory.Load<HVLVItem>(new ZQuery(HVLVItemSchema.HVI_HVC_Consignment, consignmentPKs));

				foreach (var item in items)
				{
					var itemInfo = "\t" + Res.GetString("cb037ff3-5304-4ffd-be67-7adbe32d1b68", "Processing Item with Item ID {0}", item.HVI_ItemId.IsEmpty ? (ZString)(NoResString)"(Empty)" : item.HVI_ItemId);
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational, itemInfo);

					if (item.HVI_Status != StatusCode)
					{
						var latestLog = item.Logs.MostRecentLogByEventTime(AutoEvents.StatusUpdated);

						var successInfo = "\t" + Res.GetString("44bb50d3-6905-40b0-bad9-2379961e33d0", "Processed!");

						var referenceNumber = item.GetItemEventReferenceNumber();
						var referenceType = item.GetItemEventReferenceType();
						KeyValuePair<string, string>[] parameters;
						if (latestLog == null || latestLog.SL_EventTimeOffset < STUEventTime)
						{
							var oldStatus = item.HVI_Status;
							using (item.SuspendAddStatusUpdatedEvent())
							{
								item.HVI_Status = StatusCode;
							}

							parameters = new KeyValuePair<string, string>[]
							{
								new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Old, oldStatus),
								new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.New, StatusCode),
								new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.ReferenceNumber, referenceNumber),
								new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, referenceType)
							};
						}
						else
						{
							successInfo += " " + Res.GetString("cb214adf-505a-48ce-b9e3-255bb0ba8e49", "Status updated since entered event time so current status is maintained.");
							parameters = new KeyValuePair<string, string>[]
							{
								new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.New, StatusCode),
								new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.ReferenceNumber, referenceNumber),
								new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, referenceType)
							};
						}

						item.Logs.AddNew(
						AutoEvents.StatusUpdated,
						STUEventTime,
						isEstimate: StatusCode == HVLVItemStatus.Codes.ShipmentArrived || StatusCode == HVLVItemStatus.Codes.ShipmentDeparted,
						parameters);

						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, successInfo);
					}
					else
					{
						var processinfo = "\t\t" + Res.GetString("d06d45fb-8998-4ff9-a04b-ca6cfed35e74", "Skipped, because item status is the same");
						log.NotifyFormat(OperationalActionLogErrorLevel.Informational, processinfo);
					}
				}

				Factory.Save();
				var consignmentFinishedInfo = Res.GetString("b943e5a0-d718-4175-a994-eb2d0abea3f6", "Finished Processing") + "\r\n";
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, consignmentFinishedInfo);
			}
		}

		#region StatusCode

		[MaxLength(Schema.StatusCodeMaxLength)]
		[List("StatusCodeList")]
		public ZString StatusCode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return statusCode;
			}
			set
			{
				CheckMaximumLength(StatusCodeInfo, value);
				SetNonPersistentPropertyValue(StatusCodeInfo, ref statusCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateStatusCode();
				}
			}
		}

		public ZPropertyInfo StatusCodeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.StatusCode);
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		ZString statusCode;

		#endregion

		#region STUEventTime

		public ZDateTimeOffset STUEventTime
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return stuEventTime;
			}
			set
			{
				SetNonPersistentPropertyValue(STUEventTimeInfo, ref stuEventTime, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSTUEventTime();
				}
			}
		}

		public ZPropertyInfo STUEventTimeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.STUEventTime);
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		ZDateTimeOffset stuEventTime;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public UpdateHVLVItemStatusValidation Validation
		{
			get
			{
				return GetNewValidation();
			}
		}

		protected UpdateHVLVItemStatusValidation GetNewValidation()
		{
			return new UpdateHVLVItemStatusValidation(this);
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList StatusCodeList
		{
			get
			{
				if (statusCodeList == null)
				{
					statusCodeList = HVLVItemLookups.GetAllHVLVItemStatus();
				}

				return statusCodeList;
			}
		}

		CodeDescriptionPairList statusCodeList;

		#endregion
	}

	public class UpdateHVLVItemStatusValidation : ZValidation
	{
		public UpdateHVLVItemStatusValidation(UpdateHVLVItemStatusApplicator parent)
			: base(parent)
		{
			this.parent = parent;
			ZValidationInternals = this;
			ParentListInternals = parent;
		}

		public void Add(UpdateHVLVItemStatusValidation validation)
		{
			ZValidationInternals.Add(validation);
		}

		public void Remove(UpdateHVLVItemStatusValidation validation)
		{
			ZValidationInternals.Remove(validation);
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			using (ParentListInternals.SuspendListChanged())
			{
				ValidateAllCore();
			}
		}

		protected void ValidateAllCore()
		{
			ValidateStatusCode();
			ValidateSTUEventTime();
		}

		#endregion

		#region StatusCode

		public void ValidateStatusCode()
		{
			ZValidationInternals.Validate(Parent.StatusCodeInfo, new RunValidationInvoker(StatusCodeValidationInvoker));
		}

		void StatusCodeValidationInvoker()
		{
			CheckStatusCodeIsWesternEuropean();
			CheckStatusCode();
		}

		protected void CheckStatusCodeIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.StatusCodeInfo);
		}

		protected void CheckStatusCode()
		{
			MandatoryValidation.CheckEntered(Parent.StatusCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.StatusCodeInfo, Parent.StatusCodeList);
		}

		#endregion

		#region STUEventTime

		public void ValidateSTUEventTime()
		{
			ZValidationInternals.Validate(Parent.STUEventTimeInfo, new RunValidationInvoker(STUEventTimeValidationInvoker));
		}

		void STUEventTimeValidationInvoker()
		{
			CheckSTUEventTimeIsWesternEuropean();
			CheckSTUEventTime();
		}

		protected void CheckSTUEventTimeIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.STUEventTimeInfo);
		}

		protected void CheckSTUEventTime()
		{
			MandatoryValidation.CheckEntered(Parent.STUEventTimeInfo);
			if (Parent.STUEventTime.IsInTheFuture())
			{
				Parent.STUEventTimeInfo.AddError(Res.GetString("e4f16bbe-8766-47b3-bfcb-7b8233d5c6e2", "Entered time must be in the past."));
			}
		}

		#endregion

		public override Type AutoValidationType
		{
			get
			{
				return typeof(UpdateHVLVItemStatusValidation);
			}
		}

		public UpdateHVLVItemStatusApplicator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return parent;
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly UpdateHVLVItemStatusApplicator parent;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly IValidationInternals ZValidationInternals;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ISingleElementListInternal ParentListInternals;
	}
}
