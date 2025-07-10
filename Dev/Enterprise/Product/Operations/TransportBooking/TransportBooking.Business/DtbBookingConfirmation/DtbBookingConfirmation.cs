using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	[UniversalDataContext(DataContextType.TransportBookingConfirmation)]
	[DocumentVisualizer.Integration.VisualizableDocumentsSupportable("DtbBookingConfirmationVisualizableDocumentSupporter")]
	public sealed class DtbBookingConfirmation :
		Common.AutoDtbBookingConfirmation,
		IDtbBookingConfirmation,
		IWorkflowProvider,
		IProcessHandlingInfoProvider,
		IWorkflowProviderEvent,
		IConsignmentAction,
		IDtbMasterBookingEntity
	{
		public DtbBookingConfirmation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			MasterBookingHelper = new DtbMasterBookingHelper(this);
		}

		public new class Schema : Common.AutoDtbBookingConfirmation.Schema
		{
			public const string ParentID_InstructionOrPackage = "ParentID_InstructionOrPackageDivot";

			public const string RequiredFromLabel = "RequiredFromLabel";
			public const string RequiredToLabel = "RequiredToLabel";

			public const string ConfirmationDescription = "ConfirmationDescription";
		}

		public DtbBooking Booking
		{
			get { return Instruction != null ? Instruction.Booking : null; }
		}

		IDtbBooking IDtbBookingConfirmation.Booking => Booking;

		public DtbBookingInstruction Instruction
		{
			get { return (DtbBookingInstruction)Factory.Load(InstructionType, KK_KN_BookingInstruction); }
		}

		IDtbBookingInstruction IDtbBookingConfirmation.Instruction => Instruction;

		Type InstructionType
		{
			get { return typeof(DtbBookingInstruction); }
		}

		Common.AutoDtbBookingInstructionPkgDivot IConsignmentAction.PackageDivot => PackageDivot;

		public DtbBookingInstructionPkgDivot PackageDivot
		{
			get { return !KK_KD_BookingInstructionPkgDivot.IsEmpty ? (DtbBookingInstructionPkgDivot)Factory.Load(PackageDivotType, KK_KD_BookingInstructionPkgDivot) : null; }
		}

		Type PackageDivotType
		{
			get { return typeof(DtbBookingInstructionPkgDivot); }
		}

		IEnumerable<DtbBookingInstructionPkgDivot> PackageDivots
		{
			get { return HasDirectDivot ? new[] { PackageDivot } : Instruction?.PackageDivots.Cast<DtbBookingInstructionPkgDivot>() ?? Enumerable.Empty<DtbBookingInstructionPkgDivot>(); }
		}

		public DtbBookingPackage_PackageView SelectedPackage_PackageView
		{
			get { return Booking != null ? Booking.SelectedPackage_PackageView : null; }
		}

		// persistent

		[List("Lookups.ConfirmationTypes")]
		public override ZString KK_ConfirmationType
		{
			get { return base.KK_ConfirmationType; }
			set
			{
				if (KK_ConfirmationType != value)
				{
					base.KK_ConfirmationType = value;
					SetIsEmptyContainer();
					UpdateInstructionStatus();
					UpdateInstructionConNoteNo();
					UpdateQuantity();
					MarkAsNeedingStatusCheck();

					confirmationDescriptionHasValue = false;
					ConfirmationDescriptionInfo.RefreshBinding();
				}
			}
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

			var booking = Booking;
			var packageDivot = PackageDivot;
			var packages = packageDivot != null ? new[] { packageDivot.Package } : Instruction != null ? Instruction.DivotsWithPackages.Packages.ToArray() : Array.Empty<PkgPackage>();
			if (booking != null && packages.Any())
			{
				var instructions = booking.Instructions;
				var sequence = Instruction.KN_Sequence;

				var nextInstructions = reverse
					? instructions.Cast<DtbBookingInstruction>().Where(i => i.KN_Sequence < sequence).OrderByDescending(i => i.KN_Sequence)
					: instructions.Cast<DtbBookingInstruction>().Where(i => i.KN_Sequence > sequence).OrderBy(i => i.KN_Sequence);

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

		bool ShouldAddEvent => !IsCopying && (IsPickUp || IsDelivery);

		void AddEvent(Event eventCode, ZDateTime value, string name = "", string reason = "", string type = "", bool isEstimate = false, bool publishEventToParent = true)
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

		void ClearEventLogCacheOnSaved() => EventLogCache.Clear();

		IEnumerable<StmALog> PublishEventsOnSaved => EventLogCache.Where(l => l.IsPublishedEvent).Select(l => l.EventLog);

		List<(StmALog EventLog, bool IsPublishedEvent)> EventLogCache => eventLogCache ?? (eventLogCache = new List<(StmALog EventLog, bool IsPublishedEvent)>());
		List<(StmALog EventLog, bool IsPublishedEvent)> eventLogCache;

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
				var booking = Booking;
				var transportReference = ZString.Empty;
				if (booking != null)
				{
					transportReference = !booking.KM_TransportReference.IsEmpty
						? booking.KM_TransportReference
						: booking.KM_JobID;
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

		public override ZDateTime KK_Actual
		{
			get { return base.KK_Actual; }
			set
			{
				if (base.KK_Actual != value)
				{
					base.KK_Actual = value;
					UpdateInstructionStatus();
					MarkAsNeedingStatusCheck();

					if (Instruction != null)
					{
						Instruction.ActualInfo.RefreshBinding();

						if (!IsValidationSuspended)
						{
							ValidateActualOnAllConfirmations();
						}
					}

					AddPickupOrDeliveryEvent(value);
					AddSignatureCapturedEvent(value);

					SetPUPOrDLVNeedsToBePublishedOnConsolidation();
				}
			}
		}

		void SetPUPOrDLVNeedsToBePublishedOnConsolidation()
		{
			if (IsPickUp || IsDelivery)
			{
				var consolidation = Instruction?.Booking?.ConsolidationSingleJob;
				if (consolidation != null)
				{
					if (IsPickUp)
					{
						consolidation.CheckIfPUPNeedsToBeSent = true;
					}
					else
					{
						consolidation.CheckIfDLVNeedsToBeSent = true;
					}
				}
			}
		}

		void ValidateActualOnAllConfirmations()
		{
			var booking = Booking;
			if (booking != null)
			{
				var bookingConfirmations = booking.Instructions.SelectMany(i => i.Confirmations);
				foreach (var confirmation in bookingConfirmations)
				{
					confirmation.Validation.ValidateKK_Actual(); // tested in DtbBookingConfirmationValidation
				}
			}
		}

		bool KK_Actual_ReadOnly
		{
			get { return DateAndReferenceRegistry == null || !DateAndReferenceRegistry.AllowActualDate; }
		}

		bool KK_Estimated_ReadOnly
		{
			get { return DateAndReferenceRegistry == null || !DateAndReferenceRegistry.AllowEstimatedDate; }
		}

		public void UpdatePickupEstimatedTime()
		{
			if (KK_ConfirmationType != ConfirmationTypes.Codes.PickUp)
			{
				throw new InvalidOperationException("Should not call this on a non Pickup Confirmation.");
			}

			var readyFrom = GetReadyTime();
			if (!readyFrom.IsEmpty)
			{
				var cutOffTime = Instruction.PortHubZonePivotPickupCutOffTime;
				if (!cutOffTime.IsEmpty)
				{
					var workTimeArithmetic = TransportWorkingDays.GetWorkTimeArithmetic(Factory, GetDepartmentPK());
					if ((readyFrom.Date != ZDateTime.Now.Date || readyFrom.Hour < cutOffTime.Hour) && workTimeArithmetic.IsWorkDateTime(readyFrom.ToDateTime()))
					{
						KK_Estimated = readyFrom.Date;
					}
					else
					{
						var dateCalculator = TransportWorkingDays.GetDateCalculator(Factory, GetDepartmentPK());
						KK_Estimated = dateCalculator.CalculateDate(readyFrom.ToDateTime(), DatesToCalculate.NextBusinessDay).Date;
					}
				}
			}
		}

		ZDateTime GetReadyTime()
		{
			return Booking.IsAvailable ? new[] { KK_RequiredFrom, ZDateTime.Now }.Max() : ZDateTime.Empty; // available means we have confirmed
		}

		public override ZDateTime KK_Estimated
		{
			get { return base.KK_Estimated; }
			set
			{
				base.KK_Estimated = value;
				SetKK_EstimatedUtc();
				AddPickupOrDeliveryEvent(value, isEstimate: true, publishEvent: false);

				if (Booking != null && Instruction != null)
				{
					if (Instruction.IsPickUp && IsPickUp)
					{
						Booking.UpdateDeliveryConfirmationEstimateTime();
					}

					Instruction.EstimatedInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						ValidateEstimatedOnAllConfirmations();
					}
				}
			}
		}

		void ValidateEstimatedOnAllConfirmations()
		{
			var booking = Booking;
			if (booking != null)
			{
				var bookingConfirmations = booking.Instructions.SelectMany(i => i.Confirmations);
				foreach (var confirmation in bookingConfirmations)
				{
					confirmation.Validation.ValidateKK_Estimated(); // tested in DtbBookingConfirmationValidation
				}
			}
		}

		public void SetKK_EstimatedUtc()
		{
			KK_EstimatedUtc = CalculateUtcTime(KK_Estimated);
		}

		[BusinessObjectTestExclude]
		public override ZInt KK_Quantity
		{
			get
			{
				if (KK_QuantityInfo.ReadOnly)
				{
					return GetInstructionQuantity();
				}

				return base.KK_Quantity;
			}
			set
			{
				if (base.KK_Quantity != value)
				{
					base.KK_Quantity = value;
					UpdateInstructionStatus();
				}
			}
		}

		void UpdateQuantity()
		{
			if (Instruction != null)
			{
				if (PackageDivot == null)
				{
					KK_Quantity = 0;
				}
				else
				{
					ZInt confirmationTotal = 0;

					foreach (var confirmation in Instruction.Confirmations)
					{
						if (confirmation.PK != PK && confirmation.KK_KD_BookingInstructionPkgDivot == KK_KD_BookingInstructionPkgDivot && confirmation.KK_ConfirmationType == KK_ConfirmationType)
						{
							confirmationTotal += confirmation.KK_Quantity;
						}
					}

					KK_Quantity = PackageDivot.KD_Quantity - confirmationTotal;
				}
			}
		}

		ZInt GetInstructionQuantity()
		{
			ZInt result = 0;

			if (Instruction != null)
			{
				foreach (var divot in Instruction.PackageDivots)
				{
					result += divot.KD_Quantity;
				}
			}
			return result;
		}

		bool KK_Quantity_ReadOnly
		{
			get { return Instruction != null && PackageDivot == null; }
		}

		bool KK_ReceivedBy_ReadOnly
		{
			get { return DateAndReferenceRegistry == null || !DateAndReferenceRegistry.AllowReceivedBy; }
		}

		public override ZString KK_ReferenceNum
		{
			get { return base.KK_ReferenceNum; }
			set
			{
				var valueChanged = (KK_ReferenceNum != value);
				base.KK_ReferenceNum = value;

				if (valueChanged && IsConNoteNo)
				{
					UpdateInstructionConNoteNo();
				}
			}
		}

		bool KK_ReferenceNum_ReadOnly
		{
			get { return DateAndReferenceRegistry == null || !DateAndReferenceRegistry.AllowReference; }
		}

		void UpdateInstructionConNoteNo()
		{
			var instruction = Instruction;
			if (instruction != null)
			{
				instruction.ConNoteNoInfo.RefreshBinding();
			}
		}

		public override ZDateTime KK_RequiredFrom
		{
			get { return base.KK_RequiredFrom; }
			set
			{
				base.KK_RequiredFrom = value;
				SetKK_RequiredFromUtc();
				AddCutOffDateEvent(value, Constants.EventReferenceParameterTypes.RequiredFrom);
				if (Booking != null && Instruction != null)
				{
					if (Instruction.IsPickUp && IsPickUp)
					{
						Booking.UpdateConfirmationsEstimateTime();
					}

					Instruction.ReqFromInfo.RefreshBinding();
				}
			}
		}

		bool KK_RequiredFrom_ReadOnly
		{
			get { return DateAndReferenceRegistry == null || !DateAndReferenceRegistry.AllowRequiredFromDate; }
		}

		public void SetKK_RequiredFromUtc()
		{
			KK_RequiredFromUtc = CalculateUtcTime(KK_RequiredFrom);
		}

		bool KK_RequiredTo_ReadOnly
		{
			get { return DateAndReferenceRegistry == null || !DateAndReferenceRegistry.AllowRequiredToDate; }
		}

		public override ZDateTime KK_RequiredTo
		{
			get { return base.KK_RequiredTo; }
			set
			{
				base.KK_RequiredTo = value;
				SetKK_RequiredToUtc();
				AddCutOffDateEvent(value, Constants.EventReferenceParameterTypes.RequiredTo);
				if (Instruction != null)
				{
					Instruction.ReqToInfo.RefreshBinding();
				}
			}
		}

		public void SetKK_RequiredToUtc()
		{
			KK_RequiredToUtc = CalculateUtcTime(KK_RequiredTo);
		}

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
					UpdateQuantity();
					SetIsEmptyContainer();

					ResetParentIDs_ForBinding();
					MarkAsNeedingStatusCheck();
				}
			}
		}

		void UpdateInstructionStatus(DtbBookingInstruction previousInstruction = null)
		{
			var instruction = previousInstruction ?? Instruction;
			if (instruction != null)
			{
				instruction.UpdateStatus();
			}
		}

		void ResetParentIDs_ForBinding()
		{
			parentID_InstructionOrPackageDivot = ZGuid.Missing;
		}

		ZGuid parentID_InstructionOrPackageDivot = ZGuid.Missing;

		void MarkAsNeedingStatusCheck()
		{
			var instruction = Instruction;
			if (instruction != null)
			{
				instruction.MarkAsNeedingStatusCheck();
			}
		}

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

				UpdateQuantity();
				SetIsEmptyContainer();

				ResetParentIDs_ForBinding();
				MarkAsNeedingStatusCheck();
			}
		}

		public override ZString KK_ReceivedBy
		{
			get { return base.KK_ReceivedBy; }
			set
			{
				if (base.KK_ReceivedBy != value)
				{
					base.KK_ReceivedBy = value;
					MarkAsNeedingStatusCheck();
					if (!KK_Actual.IsEmpty && (IsPickUp || IsDelivery))
					{
						var reason = IsPickUp ? Constants.EventReferenceParameterReasons.Pickup : Constants.EventReferenceParameterReasons.Delivery;
						AddEvent(Events.SignatureCaptured, !value.IsEmpty ? ZDateTime.Now : ZDateTime.Empty, KK_ReceivedBy, reason);
					}
				}
			}
		}

		public override ZBool KK_IsEmptyContainer
		{
			get { return base.KK_IsEmptyContainer; }
			set
			{
				base.KK_IsEmptyContainer = value;
				Booking?.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(KK_IsEmptyContainerInfo), IsCopying);
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var instruction = Instruction;
				var address = instruction != null ? instruction.Address : null;

				var confirmationName = Res.GetString("DtbBookingConfirmation|Name", "Confirmation");
				var confirmationTypeDescription = ConfirmationDescription;
				var city = address != null ? address.E2_City : ZString.Empty;
				var orgType = instruction != null ? instruction.OrganisationType : ZString.Empty;
				var packageDescription = PackageDescription;

				var result = confirmationName;
				if (!confirmationTypeDescription.IsEmpty)
				{
					result += " " + confirmationTypeDescription;
				}

				if (!orgType.IsEmpty)
				{
					result += ", " + orgType;
				}

				if (!city.IsEmpty)
				{
					result += "-" + city;
				}

				if (!packageDescription.IsEmpty)
				{
					result += ", " + packageDescription;
				}

				return result;
			}
		}

		ZString PackageDescription
		{
			get
			{
				var result = ZString.Empty;
				var packageDivot = PackageDivot;
				var instruction = Instruction;
				var packages = packageDivot != null ? new[] { packageDivot.Package } : instruction != null ? instruction.DivotsWithPackages.Packages.ToArray() : Array.Empty<PkgPackage>();
				if (packages.Any())
				{
					var firstPackage = packages.First();
					if (packages.Length == 1 && !firstPackage.KP_PackageID.IsEmpty)
					{
						result = firstPackage.KP_PackageID;
					}
					else
					{
						var totalQuantity = KK_Quantity; // calculates total if no divot
						var totalPackType = packages.All(p => p.KP_F3_NKPackType == firstPackage.KP_F3_NKPackType)
							? firstPackage.KP_F3_NKPackType.ToString()
							: Core.Constants.PkgUnit.Piece;
						result = Res.GetString("312b55b1-f72e-444f-aa5f-da34cf2df7fb", "{0}x {1}", totalQuantity, totalPackType);
					}
				}

				return result;
			}
		}

		// calculated

		[ResourceStringData("DtbBookingConfirmation|RequiredFromLabel", ShortCaption = "Req. From Label", Caption = "Required From Label")]
		public ZString RequiredFromLabel
		{
			get
			{
				var result = ZString.Empty;
				var dateAndReference = DateAndReferenceRegistry;

				if (dateAndReference != null && !KK_RequiredFrom_ReadOnly)
				{
					result = dateAndReference.AllowRequiredFromLabel.IsEmpty ? Res.GetString("3e4aaccd-ad36-4009-bfbd-e31efdcb9f1f", "Required From") : dateAndReference.AllowRequiredFromLabel.ToString();
				}

				return result;
			}
		}

		public ZPropertyInfo RequiredFromLabelInfo
		{
			get { return GetZPropertyInfo(Schema.RequiredFromLabel); }
		}

		[ResourceStringData("DtbBookingConfirmation|RequiredToLabel", ShortCaption = "Req. To Label", Caption = "Required To Label")]
		public ZString RequiredToLabel
		{
			get
			{
				var result = ZString.Empty;
				var dateAndReference = DateAndReferenceRegistry;

				if (dateAndReference != null && !KK_RequiredTo_ReadOnly)
				{
					result = dateAndReference.AllowRequiredToLabel.IsEmpty ? Res.GetString("181a991b-916a-4c86-ad17-41bd2eaad577", "Required To") : dateAndReference.AllowRequiredToLabel.ToString();
				}

				return result;
			}
		}

		public ZPropertyInfo RequiredToLabelInfo
		{
			get { return GetZPropertyInfo(Schema.RequiredToLabel); }
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("DtbBookingConfirmation|ParentID_InstructionOrPackageDivot", Caption = "Package")]
		[List("Lookups.InstructionOrPackageDivots")]
		public ZGuid ParentID_InstructionOrPackageDivot
		{
			get
			{
				RefreshParentID_InstructionOrPackageDivot_IfNeeded();
				return parentID_InstructionOrPackageDivot;
			}
			set
			{
				SetNonPersistentPropertyValue(ParentID_InstructionOrPackageDivotInfo, ref parentID_InstructionOrPackageDivot, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateParentID_InstructionOrPackageDivot();
				}

				if (KK_KN_BookingInstruction == value)
				{
					KK_KD_BookingInstructionPkgDivot = ZGuid.Empty;
				}
				else if (Instruction.PackageDivots.FindByPK(value) != null)
				{
					KK_KD_BookingInstructionPkgDivot = value;
				}

				ParentID_InstructionOrPackageDivotInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ParentID_InstructionOrPackageDivotInfo
		{
			get { return GetZPropertyInfo(Schema.ParentID_InstructionOrPackage); }
		}

		void RefreshParentID_InstructionOrPackageDivot_IfNeeded()
		{
			if (parentID_InstructionOrPackageDivot == ZGuid.Missing)
			{
				parentID_InstructionOrPackageDivot = KK_KD_BookingInstructionPkgDivot.IsEmpty ? KK_KN_BookingInstruction : KK_KD_BookingInstructionPkgDivot;
			}
		}

		[ResourceStringData("DtbBookingConfirmation|ConfirmationDescription", ShortCaption = "Type", Caption = "Confirmation Type")]
		[List("Lookups.ConfirmationDescriptions")]
		[MaxLength(80)]
		public ZString ConfirmationDescription
		{
			get
			{
				if (!confirmationDescriptionHasValue)
				{
					confirmationDescriptionHasValue = true;
					var matchingDateAndReference = TransportRegistry.Instance.DateAndReference.Value.Cast<DateAndReference>().FirstOrDefault(d => d.Code == KK_ConfirmationType);
					confirmationDescription = matchingDateAndReference != null ? matchingDateAndReference.Description : KK_ConfirmationType;
				}
				return confirmationDescription;
			}
			set
			{
				if (confirmationDescription != value)
				{
					SetNonPersistentPropertyValue(ConfirmationDescriptionInfo, ref confirmationDescription, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateConfirmationDescription();
					}

					TrySetConfirmationType();

					ConfirmationDescriptionInfo.RefreshBinding();
				}

				if (Booking != null && Instruction.IsPickUp && IsPickUp)
				{
					Booking.UpdateConfirmationsEstimateTime();
				}
			}
		}

		public ZPropertyInfo ConfirmationDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ConfirmationDescription); }
		}

		void TrySetConfirmationType()
		{
			if (ConfirmationDescription.IsEmpty)
			{
				KK_ConfirmationType = "";
			}
			else if (Lookups.ConfirmationDescriptions.ContainsCode(ConfirmationDescription))
			{
				KK_ConfirmationType = Lookups.ConfirmationTypes.GetCodeFromDescription(ConfirmationDescription);
			}
		}

		ZString confirmationDescription;
		bool confirmationDescriptionHasValue;

		[RelatedBusinessObject("Driver")]
		[List("Lookups.Drivers")]
		[ResourceStringData("DtbBookingConfirmation|KK_OC_Driver", Caption = "Staff Driver")]
		public override ZGuid KK_OC_Driver
		{
			get
			{
				return base.KK_OC_Driver;
			}
			set
			{
				base.KK_OC_Driver = value;
			}
		}

		[ReadOnlyMember(nameof(DriverNameIsReadOnly))]
		[ResourceStringData("DtbBookingConfirmation|DriverName", Caption = "Driver Name")]
		public ZString DriverName
		{
			get
			{
				if (Driver == null)
				{
					return KK_AdHocDriverName;
				}
				else
				{
					return Driver.Name;
				}
			}
			set
			{
				KK_AdHocDriverName = value;
			}
		}

		bool DriverNameIsReadOnly
		{
			get
			{
				return Driver != null;
			}
		}

		[ResourceStringData("DtbBookingConfirmation|KK_VehicleRegistration", ShortCaption = "V. Reg", MediumCaption = "Vehicle Reg.", Caption = "Vehicle Registration")]
		public override ZString KK_VehicleRegistration
		{
			get
			{
				return base.KK_VehicleRegistration;
			}
			set
			{
				base.KK_VehicleRegistration = value;
			}
		}

		[ResourceStringData("DtbBookingConfirmation|KK_DocumentID", ShortCaption = "Doc. ID", MediumCaption = "Document ID", Caption = "Driver Document ID")]
		public override ZString KK_DocumentID
		{
			get
			{
				return base.KK_DocumentID;
			}
			set
			{
				base.KK_DocumentID = value;
			}
		}

		[ResourceStringData("DtbBookingConfirmation|KK_DocumentIssuer", ShortCaption = "Doc. Iss.", MediumCaption = "Document Iss.", Caption = "Driver Document Issuer")]
		public override ZString KK_DocumentIssuer
		{
			get
			{
				return base.KK_DocumentIssuer;
			}
			set
			{
				base.KK_DocumentIssuer = value;
			}
		}

		[ResourceStringData("DtbBookingConfirmation|KK_DocumentType", ShortCaption = "Doc. Type", MediumCaption = "Document Type", Caption = "Driver Document Type")]
		public override ZString KK_DocumentType
		{
			get
			{
				return base.KK_DocumentType;
			}
			set
			{
				base.KK_DocumentType = value;
			}
		}

		public ZBool IsEmptyValues
		{
			get { return KK_Estimated.IsEmpty && KK_Actual.IsEmpty && KK_RequiredFrom.IsEmpty && KK_RequiredTo.IsEmpty && KK_ReferenceNum.IsEmpty; }
		}

		public new DtbBookingConfirmationLookups Lookups
		{
			get { return (DtbBookingConfirmationLookups)base.Lookups; }
		}

		protected override Common.DtbBookingConfirmationLookups GetNewLookups()
		{
			return new DtbBookingConfirmationLookups(this);
		}

		public new DtbBookingConfirmationValidation Validation
		{
			get { return (DtbBookingConfirmationValidation)base.Validation; }
		}

		protected override Common.DtbBookingConfirmationValidation GetNewValidation()
		{
			return new DtbBookingConfirmationValidation(this);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbBookingConfirmationFetchStrategy(this);
		}

		public bool IsConNoteNo
		{
			get { return this.KK_ConfirmationType.EqualsIgnoringCase(ConfirmationTypes.Codes.ConNoteNo); }
		}

		public bool HasPackages
		{
			get { return (PackageDivot != null && PackageDivot.Package != null) || (PackageDivot == null && Instruction != null && Instruction.PackageDivots.Any()); }
		}

		public bool IsDelivery
		{
			get { return this.KK_ConfirmationType.EqualsIgnoringCase(ConfirmationTypes.Codes.Delivery); }
		}

		public bool IsPickUp
		{
			get { return this.KK_ConfirmationType.EqualsIgnoringCase(ConfirmationTypes.Codes.PickUp); }
		}

		public ZBool IsOwnDepot
		{
			get
			{
				var instruction = Instruction;
				return instruction != null && instruction.IsOwnDepot;
			}
		}

		public override void Delete()
		{
			SetPUPOrDLVNeedsToBePublishedOnConsolidation();
			var instruction = IsConNoteNo ? Instruction : null;
			var confirmationInstruction = Instruction;

			var query = new ZQuery(DtbBookingConfirmationSchema.KK_KK_MasterBookingConfirmation, this.PK);
			var subs = new ActiveBusinessObjectCollection<DtbBookingConfirmation>(Factory, query);
			subs.DeleteAll();

			this.KK_KD_BookingInstructionPkgDivot = ZGuid.Empty; // to update parent status // may not need to do this anymore
			this.KK_KN_BookingInstruction = ZGuid.Empty; // to update parent status
			base.Delete();

			// tested by WorkflowProvider tests
			WorkflowItems.RemoveAndDeleteAll();

			if (instruction != null)
			{
				instruction.ConNoteNoInfo.RefreshBinding(); // to update (clear) parent connote #
			}

			if (confirmationInstruction != null)
			{
				confirmationInstruction.UpdateStatus();
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		// split and duplicate this confirmation if:
		//   parent is an Instruction
		//   the Instruction has more than 1 package

		public void SplitFromInstructionToPackageDivots(DtbBookingInstructionPkgDivot divotToAssignThis, DtbBookingInstructionPkgDivot divotToIgnore = null)
		{
			if (PackageDivot == null)
			{
				var packageDivots = Instruction.PackageDivots;
				if (packageDivots.Count > 0 && !IsCopying)
				{
					var divotToAssignThisPK = divotToAssignThis != null ? divotToAssignThis.PK : packageDivots[0].PK;
					var divotToIgnorePK = divotToIgnore != null ? divotToIgnore.PK : ZGuid.Empty;

					foreach (var divot in packageDivots)
					{
						if (divot.PK != divotToAssignThisPK && divot.PK != divotToIgnorePK)
						{
							var newConfirmation = (DtbBookingConfirmation)Clone();
							newConfirmation.KK_KD_BookingInstructionPkgDivot = divot.PK;
						}
					}

					if (!divotToAssignThisPK.IsEmpty)
					{
						KK_KD_BookingInstructionPkgDivot = divotToAssignThisPK;
					}
				}
			}
		}

		DateAndReference DateAndReferenceRegistry
		{
			get
			{
				if (Booking == null || Instruction == null)
				{
					return null;
				}
				if (dateAndReferenceRegistry == null
					|| descriptionForReg != KK_ConfirmationType
					|| bookingDirectionForRegistry != Booking.KM_Direction
					|| instructionTypeForRegistry != Instruction.KN_InstructionType
					|| organisationTypeForRegistry != OrganisationType
					|| containerModeForRegistry != ContainerMode)
				{
					descriptionForReg = KK_ConfirmationType;
					bookingDirectionForRegistry = Booking.KM_Direction;
					instructionTypeForRegistry = Instruction.KN_InstructionType;
					organisationTypeForRegistry = OrganisationType;
					containerModeForRegistry = ContainerMode;
					// TransportRegistry.Instance.DateAndReference must not be null.
					dateAndReferenceRegistry = TransportRegistry.Instance.DateAndReference.Value.FindConfirmation(descriptionForReg, bookingDirectionForRegistry,
						instructionTypeForRegistry, organisationTypeForRegistry, containerModeForRegistry);
				}
				return dateAndReferenceRegistry;
			}
		}

		DateAndReference dateAndReferenceRegistry;
		ZString descriptionForReg;
		ZString bookingDirectionForRegistry;
		ZString instructionTypeForRegistry;
		ZString organisationTypeForRegistry;
		ZString containerModeForRegistry;

		public ZString OrganisationType
		{
			get { return Instruction != null ? Instruction.OrganisationType : ZString.Empty; }
		}

		public ZString ContainerMode
		{
			get
			{
				bool isContainerised = PackageDivot != null
					? PackageDivot.Package != null && PackageDivot.Package.IsContainer
					: Instruction != null && Instruction.DivotsWithPackages.Count > 0 && Instruction.DivotsWithPackages.Packages.All(p => p.IsContainer);

				return isContainerised ? Constants.ContainerModes.Containerised : Constants.ContainerModes.Loose;
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			// Send events if successful
			if (saveSucceeded)
			{
				if (IsContainerised && PublishEventsOnSaved.Any())
				{
					var parentWithWorkflow = Instruction?.Booking?.ConsolidationSingleJob?.Parent?.ParentWithWorkflow;
					if (parentWithWorkflow != null)
					{
						var factory = new BusinessObjectFactory();
						using (factory.AddDisposableService())
						{
							foreach (var log in PublishEventsOnSaved)
							{
								PublishUniversalEventCore.PublishUniversalEvent(factory, this, parentWithWorkflow, log);
							}
							factory.Save();
						}
					}
				}

				ClearEventLogCacheOnSaved();
			}
		}

		public override void OnSaving()
		{
			InitialiseMasterFieldsIfNew();
			MasterBookingHelper.UpdateMasterBookingVersion();

			base.OnSaving();
		}

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new DtbBookingConfirmationProcessHandlingInfo(this); }
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable]
		[ChildEditableTestExclude]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new DtbBookingConfirmationProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		DtbBookingConfirmationProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		ZGuid IWorkflowProviderCore.PK
		{
			get { return PK; }
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.DtbBookingConfirmationWorkflowDescriptorCode; }
		}

		OrgHeader[] IWorkflowProviderEvent.RecipientOrganisations
		{
			get { return new OrgHeader[] { GlbBranch.CurrentBranch.OrgProxy }; }
		}

		public string KeyForCache { get; set; }
		public ZInt TotalPackages { get { return 0; } }
		public ZDecimal TotalVolume { get { return 0; } }
		public ZDecimal TotalWeight { get { return 0; } }
		public ZString TotalWeightUnit { get { return ZString.Empty; } }
		public ZString TotalVolumeUnit { get { return ZString.Empty; } }
		public ZString ConsignorOrConsigneeAddress { get; }

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

		public ZBlob ReceivedBySignature
		{
			get { return KK_ReceivedBySignature; }
		}

		public ZString ReceivedBy
		{
			get { return KK_ReceivedBy; }
		}

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

		ZGuid GetDepartmentPK()
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

		int Quantity
		{
			get { return HasDirectDivot ? (int)KK_Quantity : PackageDivots.Sum(d => d.KD_Quantity); }
		}

		ZDecimal Weight
		{
			get { return Proportion * PackageDivots.Sum(d => Constants.Weight.ConvertSafe(d.Weight, d.WeightUQ, WeightUQ)); }
		}

		ZString WeightUQ
		{
			get { return PackageDivots.Select(d => d.WeightUQ).Distinct().Count() == 1 ? PackageDivots.First().WeightUQ : DtbTransportTotalsHelper.TotalWeightUnit; }
		}

		ZDecimal Volume
		{
			get { return Proportion * PackageDivots.Sum(d => Constants.Volume.ConvertSafe(d.Volume, d.VolumeUQ, VolumeUQ)); }
		}

		ZString VolumeUQ
		{
			get { return PackageDivots.Select(d => d.VolumeUQ).Distinct().Count() == 1 ? PackageDivots.First().VolumeUQ : DtbTransportTotalsHelper.TotalVolumeUnit; }
		}

		ZDecimal Proportion
		{
			get { return HasDirectDivot ? KK_Quantity / PackageDivot?.Package?.KP_PackageQty ?? 1 : 1; }
		}

		bool HasDirectDivot
		{
			get { return PackageDivot != null; }
		}

		public bool IsContainerised
		{
			get { return (PackageDivot != null && PackageDivot.Package != null && PackageDivot.Package.IsContainer) || (PackageDivot == null && Instruction != null && Instruction.IsContainerised); }
		}

		ZString PackType
		{
			get { return PackageDivots.Select(d => d.PackType).Distinct().Count() == 1 ? PackageDivots.First().PackType.ToString() : Constants.PkgUnit.Piece; }
		}

		ZString ContainerNumbers
		{
			get { return IsContainerised ? ZString.Join(", ", PackageDivots.Select(p => p.PackageID).ToArray()) : ZString.Empty; }
		}

		void InitialiseMasterFieldsIfNew()
		{
			if (!IsInDatabase && !IsDeleted)
			{
				if (KK_KK_MasterBookingConfirmation != ZGuid.Empty)
				{
					if (KK_MasterBookingVersion == ZShort.Zero)
					{
						KK_MasterBookingVersion = (short)1;
					}
				}
				else
				{
					if (Booking != null)
					{
						if (!KK_IsMaster && Booking.KM_IsMaster)
						{
							KK_IsMaster = true;
						}
						else if (KK_IsMaster && !Booking.KM_IsMaster)
						{
							KK_IsMaster = false;
						}
						KK_MasterBookingVersion = (short)(KK_IsMaster ? 1 : 0);
					}
				}
			}
		}

		bool IDtbMasterBookingEntity.IsMaster => KK_IsMaster;

		short IDtbMasterBookingEntity.GetMasterBookingVersion() => KK_MasterBookingVersion;

		void IDtbMasterBookingEntity.SetMasterBookingVersion(short newMasterBookingVersion)
		{
			KK_MasterBookingVersion = newMasterBookingVersion;
		}

		IEnumerable<ZPropertyInfo> IDtbMasterBookingEntity.ReplicationFieldInfos
		{
			get
			{
				if (replicationFieldInfos == null)
				{
					replicationFieldInfos = DtbMasterBookingReplication.GetPropertyInfosFromListOfColumns(this, DtbMasterBookingReplication.DtbBookingConfirmationReplicatedColumns);
				}

				return replicationFieldInfos;
			}
		}
		IEnumerable<ZPropertyInfo> replicationFieldInfos;

		DtbMasterBookingHelper MasterBookingHelper { get; }
	}
}
