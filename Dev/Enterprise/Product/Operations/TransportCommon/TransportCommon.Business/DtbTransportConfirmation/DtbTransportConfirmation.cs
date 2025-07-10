using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business.Common;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using static System.FormattableString;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportConfirmation : AutoDtbBookingConfirmation, IConsignmentAction
	{
		protected DtbTransportConfirmation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region Instruction

		public DtbTransportInstruction Instruction
		{
			get { return (DtbTransportInstruction)Factory.Load(InstructionType, KK_KN_BookingInstruction); }
		}

		protected abstract Type InstructionType { get; }

		#endregion

		#region PackageDivot

		public DtbTransportInstructionPkgDivot PackageDivot
		{
			get { return !KK_KD_BookingInstructionPkgDivot.IsEmpty ? (DtbTransportInstructionPkgDivot)Factory.Load(PackageDivotType, KK_KD_BookingInstructionPkgDivot) : null; }
		}

		AutoDtbBookingInstructionPkgDivot IConsignmentAction.PackageDivot => PackageDivot;

		protected abstract Type PackageDivotType { get; }

		#endregion

		#region Transport

		public DtbTransport Transport
		{
			get
			{
				var instruction = Instruction;
				return instruction != null ? instruction.Booking : null;
			}
		}

		#endregion

		#endregion

		#region Properties

		#region KK_KN_BookingInstruction

		[RelatedBusinessObject("Instruction")]
		public override ZGuid KK_KN_BookingInstruction
		{
			get { return base.KK_KN_BookingInstruction; }
			set
			{
				var oldInstruction = Instruction;
				base.KK_KN_BookingInstruction = value;
				var newInstruction = Instruction;

				if (oldInstruction != null)
				{
					oldInstruction.UpdateStatus();
				}

				if (newInstruction != null)
				{
					newInstruction.UpdateStatus();
				}
			}
		}

		#endregion

		#region KK_KD_BookingInstructionPkgDivot

		public override ZGuid KK_KD_BookingInstructionPkgDivot
		{
			get { return base.KK_KD_BookingInstructionPkgDivot; }
			set
			{
				bool hasValueChanged = KK_KD_BookingInstructionPkgDivot != value;
				var previousInstruction = hasValueChanged ? Instruction : null;

				base.KK_KD_BookingInstructionPkgDivot = value;

				if (hasValueChanged)
				{
					// If previous Instruction is null, we will update status twice without this check
					if (previousInstruction != null)
					{
						UpdateInstructionStatus(previousInstruction); // prev
					}

					UpdateInstructionStatus(); // current
					OnKK_KD_BookingInstructionPkgDivotSet();
				}
			}
		}

		protected virtual void OnKK_KD_BookingInstructionPkgDivotSet()
		{
		}

		protected void UpdateInstructionStatus(DtbTransportInstruction previousInstruction = null)
		{
			var instruction = previousInstruction ?? Instruction;
			if (instruction != null)
			{
				instruction.UpdateStatus();
			}
		}

		#endregion

		#region KK_RequiredFrom

		public override ZDateTime KK_RequiredFrom
		{
			get { return base.KK_RequiredFrom; }
			set
			{
				base.KK_RequiredFrom = value;
				SetKK_RequiredFromUtc();
				AddCutOffDateEvent(value, Constants.EventReferenceParameterTypes.RequiredFrom);
			}
		}

		public void SetKK_RequiredFromUtc()
		{
			KK_RequiredFromUtc = CalculateUtcTime(KK_RequiredFrom);
		}

		#endregion

		#region KK_RequiredTo

		public override ZDateTime KK_RequiredTo
		{
			get { return base.KK_RequiredTo; }
			set
			{
				base.KK_RequiredTo = value;
				SetKK_RequiredToUtc();
				AddCutOffDateEvent(value, Constants.EventReferenceParameterTypes.RequiredTo);
			}
		}

		public void SetKK_RequiredToUtc()
		{
			KK_RequiredToUtc = CalculateUtcTime(KK_RequiredTo);
		}

		#endregion

		#region KK_Estimated

		public override ZDateTime KK_Estimated
		{
			get { return base.KK_Estimated; }
			set
			{
				base.KK_Estimated = value;
				SetKK_EstimatedUtc();
				AddPickupOrDeliveryEvent(value, isEstimate: true, publishEvent: false);
			}
		}

		public void SetKK_EstimatedUtc()
		{
			KK_EstimatedUtc = CalculateUtcTime(KK_Estimated);
		}

		#endregion

		#region KK_Actual

		public override ZDateTime KK_Actual
		{
			get { return base.KK_Actual; }
			set
			{
				if (base.KK_Actual != value)
				{
					base.KK_Actual = value;
					OnKK_ActualSet();

					AddPickupOrDeliveryEvent(value);
					AddSignatureCapturedEvent(value);
				}
			}
		}

		protected virtual void OnKK_ActualSet()
		{
		}

		void AddPickupOrDeliveryEvent(ZDateTime value, bool isEstimate = false, bool publishEvent = true)
		{
			if (ShouldAddEvent)
			{
				var eventCode = IsPickUp ? Events.PickedUp : Events.Delivered;
				AddEvent(eventCode, value, isEstimate: isEstimate, publishEventToParent: publishEvent);
			}
		}

		void AddSignatureCapturedEvent(ZDateTime value)
		{
			if (ShouldAddEvent && !string.IsNullOrEmpty(KK_ReceivedBy))
			{
				var reason = IsPickUp ? Constants.EventReferenceParameterReasons.Pickup : Constants.EventReferenceParameterReasons.Delivery;
				AddEvent(Events.SignatureCaptured, value, KK_ReceivedBy, reason);
			}
		}

		void AddCutOffDateEvent(ZDateTime value, string typeDescription)
		{
			if (ShouldAddEvent)
			{
				var type = Invariant($"{(IsPickUp ? Constants.EventReferenceParameterReasons.Pickup : Constants.EventReferenceParameterReasons.Delivery)} {typeDescription}");
				AddEvent(Events.CutOffDate, value, type: type, publishEventToParent: false);
			}
		}

		#region SetIsEmptyContainer
		public abstract void SetIsEmptyContainerOnThisAndAllRelatedConfirmations();

		public void SetIsEmptyContainer()
		{
			if (IsContainerised &&
				(
					(Instruction != null && Instruction.IsEmptyYard)
					|| (IsPickUp && IsAllNextConfirmationCYD())
					|| (IsDelivery && IsAllPreviousConfirmationCYD())
				)
			)
			{
				KK_IsEmptyContainer = true;
			}
			else
			{
				Validation.ValidateKK_IsEmptyContainer();
			}
		}

		bool IsAllNextConfirmationCYD()
		{
			return IsAllNextConfirmationCYD(false);
		}

		bool IsAllPreviousConfirmationCYD()
		{
			return IsAllNextConfirmationCYD(true);
		}

		bool IsAllNextConfirmationCYD(bool reverse)
		{
			var result = false;

			var booking = Transport;
			var packageDivot = PackageDivot;
			var packages = packageDivot != null ? new[] { packageDivot.Package } : Instruction != null ? Instruction.DivotsWithPackages.Packages.ToArray() : Array.Empty<PkgPackage>();
			if (booking != null && packages.Any())
			{
				var instructions = booking.Instructions;
				var sequence = Instruction.KN_Sequence;

				var nextInstructions = reverse
					? instructions.Cast<DtbTransportInstruction>().Where(i => i.KN_Sequence < sequence).OrderByDescending(i => i.KN_Sequence)
					: instructions.Cast<DtbTransportInstruction>().Where(i => i.KN_Sequence > sequence).OrderBy(i => i.KN_Sequence);

				var eachPackageNextInstructionIsCYD = true;
				foreach (var package in packages)
				{
					var packageNextInstruction = nextInstructions.FirstOrDefault(ni => ni.DivotsWithPackages.Contains(package));
					eachPackageNextInstructionIsCYD &= packageNextInstruction?.IsCYD ?? false;
				}

				result = eachPackageNextInstructionIsCYD;
			}

			return result;
		}

		#endregion

		bool ShouldAddEvent => !IsCopying && (IsPickUp || IsDelivery);

		protected void AddEvent(Event eventCode, ZDateTime value, string name = "", string reason = "", string type = "", bool isEstimate = false, bool publishEventToParent = true)
		{
			RemoveMatchingEventIfNotCommitted(eventCode, type, isEstimate);

			if (value.IsValid || eventCode == Events.PickedUp || eventCode == Events.Delivered)
			{
				var eventParams = new List<KeyValuePair<string, string>>();

				var facility = DocAddressTypeExtensions.GetEventReferenceFacilityCodeFromDocAddress(Instruction.Address);
				if (!string.IsNullOrEmpty(facility))
				{
					eventParams.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, facility));
				}

				eventParams.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, Constants.Departments.TransportProvider));

				if (!string.IsNullOrEmpty(name))
				{
					eventParams.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Name, name));
				}

				if (!string.IsNullOrEmpty(reason))
				{
					eventParams.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, reason));
				}

				if (!string.IsNullOrEmpty(type))
				{
					eventParams.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, type));
				}
				else if (IsContainerised)
				{
					var containerType = KK_IsEmptyContainer
						? Constants.EventReferenceParameterTypes.EmptyContainer
						: Constants.EventReferenceParameterTypes.FullContainer;

					eventParams.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, containerType));
				}

				var estimateActual = isEstimate ? EstimateActual.Estimate : EstimateActual.Actual;
				var newLog = Logs.CreateRecreateOrUpdateEventLog(eventCode, estimateActual, value.ToOffset(), EventReference, eventParams.ToArray());
				if (newLog != null)
				{
					EventLogCache.Add((newLog, publishEventToParent));
				}
			}
		}

		void RemoveMatchingEventIfNotCommitted(Event eventCode, string typeParameter = "", bool isEstimate = false)
		{
			var matchingLogs = EventLogCache.Select((e, index) => new { Event = e.EventLog, Index = index }).Where(o => IsMatchingEvent(o.Event, eventCode, typeParameter, isEstimate)).ToArray();
			if (matchingLogs.Length > 0)
			{
				var matchingLog = matchingLogs[0].Event;
				var estimateActual = matchingLog.SL_IsEstimate ? EstimateActual.Estimate : EstimateActual.Actual;
				Array.ForEach(matchingLogs, m => EventLogCache.RemoveAt(m.Index));
				Logs.CreateRecreateOrUpdateEventLog(eventCode, estimateActual, ZDateTimeOffset.Empty, matchingLog.ReferenceFreeText, matchingLog.Parameters.ToArray()); // reference + params need to match existing Log for it to be deleted.
			}
		}

		bool IsMatchingEvent(StmALog log, Event eventCode, ZString typeParameter, bool isEstimate)
		{
			const string Type = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type;

			return log.SL_SE_NKEvent == eventCode.Code
				&& log.SL_IsEstimate == isEstimate
				&& (typeParameter.IsEmpty || (log.Parameters.TryGetValue(Type, out var result) && result == typeParameter));
		}

		#region PublishEventsOnSaved

		protected void ClearEventLogCacheOnSaved() => EventLogCache.Clear();

		protected IEnumerable<StmALog> PublishEventsOnSaved => EventLogCache.Where(l => l.IsPublishedEvent).Select(l => l.EventLog);

		List<(StmALog EventLog, bool IsPublishedEvent)> EventLogCache => eventLogCache ?? (eventLogCache = new List<(StmALog EventLog, bool IsPublishedEvent)>());
		List<(StmALog EventLog, bool IsPublishedEvent)> eventLogCache;

		#endregion

		#region EventReference

		ZString EventReference
		{
			get
			{
				string packageInfo = "";
				if (IsContainerised)
				{
					packageInfo = ContainerNumbers;
				}
				else
				{
					string volumeInfo = Volume != 0 ? string.Format(CultureInfo.InvariantCulture, "{0} {1}", Volume, VolumeUQ).Trim() : "";
					string weightInfo = Weight != 0 ? string.Format(CultureInfo.InvariantCulture, "{0} {1}", Weight, WeightUQ).Trim() : "";
					packageInfo = string.Format(CultureInfo.InvariantCulture, "{0} {1}", volumeInfo, weightInfo).Trim();
				}

				string quantityAndPackType = Quantity != 0 ? string.Format(CultureInfo.InvariantCulture, "{0} {1}", Quantity, PackType).Trim() : "";
				var transport = Transport;
				var transportReference = ZString.Empty;
				if (transport != null)
				{
					transportReference = !transport.KM_TransportReference.IsEmpty
						? transport.KM_TransportReference
						: transport.KM_JobID;
				}
				var eventReference = ZString.Format("{0} {1}", quantityAndPackType, packageInfo).Trim();
				var stringBuilder = new ZStringBuilder();
				stringBuilder.AppendIfNotEmpty(transportReference);
				stringBuilder.AppendIfNotEmpty(eventReference);

				var reference = stringBuilder.ToStringWithDelimiterBetweenAppends(", ");

				if (reference.Length > AutoStmALog.Schema.SL_ReferenceMaxLength)
				{
					reference = reference.Substring(0, StmALog.Schema.SL_ReferenceMaxLength);
				}
				return reference;
			}
		}

		#endregion

		#endregion

		#region KK_ReceivedBy

		public override ZString KK_ReceivedBy
		{
			get { return base.KK_ReceivedBy; }
			set
			{
				if (base.KK_ReceivedBy != value)
				{
					base.KK_ReceivedBy = value;
					OnKK_ReceivedBy();
					if (!KK_Actual.IsEmpty && (IsPickUp || IsDelivery))
					{
						var reason = IsPickUp ? Constants.EventReferenceParameterReasons.Pickup : Constants.EventReferenceParameterReasons.Delivery;
						AddEvent(Events.SignatureCaptured, !value.IsEmpty ? ZDateTime.Now : ZDateTime.Empty, KK_ReceivedBy, reason);
					}
				}
			}
		}

		protected abstract void OnKK_ReceivedBy();

		#region KK_ReceivedBy_ReadOnly

		protected virtual bool KK_ReceivedBy_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#endregion

		#endregion

		#region UtcAndUnlocoTimeConversion

		#region CalculateUtcTime

		public ZDateTime CalculateUtcTime(ZDateTime? localTime)
		{
			var result = ZDateTime.Empty;
			if (localTime.HasValue && localTime.Value.IsValid)
			{
				var unloco = GetUNLOCO();
				if (unloco != null)
				{
					result = Env.Time.GetUtcFromUnlocoTime(unloco.Code, localTime.Value.ToDateTime());
				}
			}
			return result;
		}

		#endregion

		#region CalculateLocalTime

		public ZDateTime CalculateLocalTime(ZDateTime? utcTime)
		{
			var result = ZDateTime.Empty;
			if (utcTime.HasValue && utcTime.Value.IsValid)
			{
				var unloco = GetUNLOCO();
				if (unloco != null)
				{
					result = Env.Time.GetUnlocoTimeFromUtc(unloco.Code, utcTime.Value.ToDateTime());
				}
			}

			return result;
		}

		#endregion

		#region Implementation

		#region GetUNLOCO

		RefUNLOCO GetUNLOCO()
		{
			RefUNLOCO result = null;
			var address = Instruction != null ? Instruction.Address : null;
			if (address != null && !address.E2_AddressOverride)
			{
				var org = address.Organisation;
				result = org != null ? org.UNLOCO : null;
			}

			if (result == null)
			{
				result = GlbBranch.CurrentBranch != null ? GlbBranch.CurrentBranch.HomePort : null;
			}

			return result;
		}

		#endregion

		#endregion

		#endregion

		#region GetDepartmentPK

		protected ZGuid GetDepartmentPK()
		{
			var result = GlbDepartment.CurrentDepartment.PK;

			var instruction = Instruction;
			if (instruction != null)
			{
				var booking = instruction.Booking;
				if (booking != null)
				{
					var job = booking.Job;
					if (job != null)
					{
						var department = job.Department;
						if (department != null)
						{
							result = department.PK;
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region Quantity

		int Quantity
		{
			get { return HasDirectDivot ? (int)KK_Quantity : PackageDivots.Sum(d => d.KD_Quantity); }
		}

		#endregion

		#region Weight

		ZDecimal Weight
		{
			get { return Proportion * PackageDivots.Sum(d => Constants.Weight.ConvertSafe(d.Weight, d.WeightUQ, WeightUQ)); }
		}

		ZString WeightUQ
		{
			get { return PackageDivots.Select(d => d.WeightUQ).Distinct().Count() == 1 ? PackageDivots.First().WeightUQ : DtbTransportTotalsHelper.TotalWeightUnit; }
		}

		#endregion

		#region Volume

		ZDecimal Volume
		{
			get { return Proportion * PackageDivots.Sum(d => Constants.Volume.ConvertSafe(d.Volume, d.VolumeUQ, VolumeUQ)); }
		}

		ZString VolumeUQ
		{
			get { return PackageDivots.Select(d => d.VolumeUQ).Distinct().Count() == 1 ? PackageDivots.First().VolumeUQ : DtbTransportTotalsHelper.TotalVolumeUnit; }
		}

		#endregion

		#region Proportion

		ZDecimal Proportion
		{
			get { return HasDirectDivot ? KK_Quantity / PackageDivot?.Package?.KP_PackageQty ?? 1 : 1; }
		}

		#endregion

		#region PackageDivots

		IEnumerable<DtbTransportInstructionPkgDivot> PackageDivots
		{
			get { return HasDirectDivot ? new[] { PackageDivot } : Instruction?.PackageDivots.Cast<DtbTransportInstructionPkgDivot>() ?? Enumerable.Empty<DtbTransportInstructionPkgDivot>(); }
		}

		#endregion

		#region HasDivot

		bool HasDirectDivot
		{
			get { return PackageDivot != null; }
		}

		#endregion

		#region IsContainerised

		public bool IsContainerised
		{
			get { return (PackageDivot != null && PackageDivot.Package != null && PackageDivot.Package.IsContainer) || (PackageDivot == null && Instruction != null && Instruction.IsContainerised); }
		}

		#endregion

		#region PackType

		ZString PackType
		{
			get { return PackageDivots.Select(d => d.PackType).Distinct().Count() == 1 ? PackageDivots.First().PackType.ToString() : Constants.PkgUnit.Piece; }
		}

		#endregion

		#region ContainerNumbers

		ZString ContainerNumbers
		{
			get { return IsContainerised ? ZString.Join(", ", PackageDivots.Select(p => p.PackageID).ToArray()) : ZString.Empty; }
		}

		#endregion

		#region Flags

		#region IsDelivery

		public bool IsDelivery
		{
			get { return this.KK_ConfirmationType.EqualsIgnoringCase(ConfirmationTypes.Codes.Delivery); }
		}

		#endregion

		#region IsPickup

		public bool IsPickUp
		{
			get { return this.KK_ConfirmationType.EqualsIgnoringCase(ConfirmationTypes.Codes.PickUp); }
		}

		#endregion

		#region IsOwnDepot

		public ZBool IsOwnDepot
		{
			get
			{
				var instruction = Instruction;
				return instruction != null && instruction.IsOwnDepot;
			}
		}

		#endregion

		#endregion

		#region Lookups

		public new DtbTransportConfirmationLookups Lookups
		{
			get { return (DtbTransportConfirmationLookups)base.Lookups; }
		}

		protected sealed override DtbBookingConfirmationLookups GetNewLookups()
		{
			return GetNewLookupsCore();
		}

		protected abstract DtbTransportConfirmationLookups GetNewLookupsCore();

		#endregion

		#region Validation

		public new DtbTransportConfirmationValidation Validation
		{
			get { return (DtbTransportConfirmationValidation)base.Validation; }
		}

		protected sealed override DtbBookingConfirmationValidation GetNewValidation()
		{
			return GetNewValidationCore();
		}

		protected abstract DtbTransportConfirmationValidation GetNewValidationCore();

		#endregion

		#region IConsignmentAction

		public virtual string KeyForCache { get; set; }
		public virtual ZInt TotalPackages { get { return 0; } }
		public virtual ZDecimal TotalVolume { get { return 0; } }
		public virtual ZDecimal TotalWeight { get { return 0; } }
		public virtual ZString TotalWeightUnit { get { return ZString.Empty; } }
		public virtual ZString TotalVolumeUnit { get { return ZString.Empty; } }
		public virtual ZString ConsignorOrConsigneeAddress { get; }

		public abstract ZString ReceivedBy { get; }
		public abstract ZBlob ReceivedBySignature { get; }

		public ZInt ActionQuantity
		{
			get { return KK_Quantity; }
		}

		public ZDateTime Estimated
		{
			get { return KK_Estimated; }
		}

		public ZDateTime RequiredFrom
		{
			get { return KK_RequiredFrom; }
		}

		public ZDateTime RequiredTo
		{
			get { return KK_RequiredTo; }
		}

		public ZString ReferenceNumber
		{
			get { return KK_ReferenceNum; }
		}

		public ZString ActionType
		{
			get { return KK_ConfirmationType; }
		}

		public IConsignmentAddress ConsignmentAddress
		{
			get { return Instruction; }
		}

		public ZBool IsEmptyContainer
		{
			get { return KK_IsEmptyContainer; }
		}

		#endregion
	}
}
