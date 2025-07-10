using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentConfirmation : DtbTransportConfirmation,
		IDtbConsignmentConfirmation,
		IConsignmentAction,
		IDocumentSupportable,
		IPalletTransactionParent,
		ISignatureSupporter
	{
		public DtbConsignmentConfirmation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(KK_K1_RunSheetInstruction), ConcurrencyPolicy.Strict);
		}

		#region Related Entities

		#region Instruction

		public new DtbConsignmentInstruction Instruction
		{
			get { return (DtbConsignmentInstruction)base.Instruction; }
		}

		protected override Type InstructionType
		{
			get { return typeof(DtbConsignmentInstruction); }
		}

		public ZString InstructionAddress
		{
			get { return base.Instruction.Address.AddressAsASingleLine; }
		}

		public ZString InstructionOrganisationName
		{
			get { return base.Instruction.Address.E2_CompanyName; }
		}

		#endregion

		#region Transport

		public new DtbBookingConsignment Transport
		{
			get { return (DtbBookingConsignment)base.Transport; }
		}

		#endregion

		#region PackageDivot

		protected override Type PackageDivotType
		{
			get { return typeof(DtbConsignmentInstructionPkgDivot); }
		}

		#endregion

		#region PackageTotalsBusinessObject

		PackageTotals PackageTotalsBusinessObject
		{
			get
			{
				if (packageTotalsBusinessObject == null && KeyForCache != null)
				{
					CalculatePackageTotalsForAllConfirmations();
				}

				return packageTotalsBusinessObject;
			}
			set { packageTotalsBusinessObject = value; }
		}

		void CalculatePackageTotalsForAllConfirmations()
		{
			var packageTotals = Factory.GetCachedConfirmationsTotals(KeyForCache);
			if (packageTotals != null)
			{
				Func<PackageTotals> result;
				if (packageTotals.TryGetValue(PK, out result))
				{
					PackageTotalsBusinessObject = result();
					packageTotals.Remove(PK); // remove all references held by the delegate
				}
			}
		}

		public override string KeyForCache
		{
			get { return keyForCache; }
			set
			{
				if (value == null)
				{
					throw new InvalidOperationException("Should not be setting KeyForCache to null.");
				}
				else if (keyForCache != null)
				{
					throw new InvalidOperationException("Should not be setting KeyForCache twice.");
				}

				keyForCache = value;
			}
		}

		PackageTotals packageTotalsBusinessObject;
		string keyForCache;

		#endregion

		#region RunSheetInstruction

		public DtbConsignmentRunSheetInstruction RunSheetInstruction
		{
			get { return Factory.Load<DtbConsignmentRunSheetInstruction>(KK_K1_RunSheetInstruction); }
		}

		#endregion

		#region GetRelatedConfirmation

		public DtbConsignmentConfirmation GetRelatedConfirmation()
		{
			DtbConsignmentConfirmation result = null;

			var transport = Transport;

			if (IsPickUp)
			{
				var nextInstructionSequence = Instruction.KN_Sequence + 1;
				var nextInstruction = transport.Instructions.Single(i => i.KN_Sequence == nextInstructionSequence);
				result = nextInstruction.DeliveryConfirmation;
			}
			else if (IsDelivery)
			{
				var previousInstructionSequence = Instruction.KN_Sequence - 1;
				var previousInstruction = transport.Instructions.Single(i => i.KN_Sequence == previousInstructionSequence);
				result = previousInstruction.PickupConfirmation;
			}

			return result;
		}

		public DtbConsignmentConfirmation[] GetRelatedDepotConfirmations()
		{
			return Transport.DepotInstructions.SelectMany(i => i.Confirmations).ToArray();
		}

		public DtbConsignmentConfirmation GetRelatedConfirmationExcludingDepot()
		{
			var instruction = Transport.Instructions.SingleOrDefault(i => i.IsPickUp != IsPickUp && !i.IsOwnDepot);
			return instruction != null ? (IsPickUp ? instruction.DeliveryConfirmation : instruction.PickupConfirmation) : null;
		}

		DtbConsignmentConfirmation GetRelatedConfirmationForDepot()
		{
			if (!IsOwnDepot)
			{
				throw new InvalidOperationException("GetRelatedConfirmationForDepot() should only be invoked from a Depot Confirmation.");
			}

			return IsPickUp
				? Instruction.Booking.DeliveryInstruction.DeliveryConfirmation
				: Instruction.Booking.PickupInstruction.PickupConfirmation;
		}

		#endregion

		#region Pallet Transactions

		[ChildEditable]
		public PkgPalletTransactionDependentCollection PalletTransactions
		{
			get
			{
				if (palletTransactions == null)
				{
					palletTransactions = new PkgPalletTransactionDependentCollection(this);
					RegisterEditableChildObject(palletTransactions);
				}
				return palletTransactions;
			}
		}

		PkgPalletTransactionDependentCollection palletTransactions;

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();
			PalletTransactions.DeleteAll();
		}

		#endregion

		#region Properties

		#region KK_ConfirmationType

		[ReadOnly(true)]
		[ResourceStringData("DtbConsignmentConfirmation|KK_ConfirmationType", Caption = "Job Type", ShortCaption = "Job")]
		public override ZString KK_ConfirmationType
		{
			get { return base.KK_ConfirmationType; }
			set
			{
				if (KK_ConfirmationType != value)
				{
					base.KK_ConfirmationType = value;
					UpdateInstructionStatus();
				}
			}
		}

		#endregion

		#region KK_K1_RunSheetInstruction

		public override ZGuid KK_K1_RunSheetInstruction
		{
			get { return base.KK_K1_RunSheetInstruction; }
			set
			{
				var previousValue = KK_K1_RunSheetInstruction;
				base.KK_K1_RunSheetInstruction = value;

				if (previousValue != KK_K1_RunSheetInstruction)
				{
					UpdateInstructionStatus();
					AlterRunSheetEventForPickUpOrDelivery(previousValue);
				}
			}
		}

		void AlterRunSheetEventForPickUpOrDelivery(ZGuid previousValue)
		{
			// Attach event to new consignment
			if (RunSheetInstruction != null)
			{
				var instruction = Instruction;
				if (instruction != null && (instruction.IsPickUp || instruction.IsDelivery))
				{
					var consignment = instruction.Booking;
					var runSheetNumber = RunSheetInstruction.RunSheet != null ? RunSheetInstruction.RunSheet.KG_RunSheetNumber : ZString.Empty;
					consignment.Logs.AddNew(Events.Attached, runSheetNumber, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.RunSheet));
				}
			}

			// Detach event from old consignment
			var previousRunSheetInstruction = Factory.Load<DtbConsignmentRunSheetInstruction>(previousValue);
			if (previousRunSheetInstruction != null && !previousRunSheetInstruction.IsDeleted)
			{
				var instruction = Instruction;
				if (instruction != null && (instruction.IsPickUp || instruction.IsDelivery))
				{
					var consignment = instruction.Booking;
					var runSheetNumber = previousRunSheetInstruction.RunSheet != null ? previousRunSheetInstruction.RunSheet.KG_RunSheetNumber : ZString.Empty;
					var paramPairs = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.RunSheet);
					consignment.Logs.CreateOrRecreateEventLog(Events.Detached, EstimateActual.Actual, ZDateTimeOffset.Now, runSheetNumber, paramPairs);
				}
			}
		}

		#endregion

		#region KK_RequiredFrom

		[ResourceStringData("DtbConsignmentConfirmation|KK_RequiredFrom", Caption = "Requested From", ShortCaption = "Req. From")]
		public override ZDateTime KK_RequiredFrom
		{
			get { return base.KK_RequiredFrom; }
			set
			{
				var previousValue = KK_RequiredFrom;
				base.KK_RequiredFrom = value;

				if (previousValue != KK_RequiredFrom)
				{
					if (Transport != null && Instruction != null && Instruction.IsPickUp && IsPickUp)
					{
						Transport.UpdateConfirmationsEstimateTime();
					}

					RefreshRequiredFromOnInstruction();
				}
			}
		}

		void RefreshRequiredFromOnInstruction()
		{
			var instruction = Instruction;
			if (instruction != null)
			{
				instruction.ReqFromInfo.RefreshBinding();
			}
		}

		#endregion

		#region KK_RequiredTo

		[ResourceStringData("DtbConsignmentConfirmation|KK_RequiredTo", Caption = "Requested To", ShortCaption = "Req. To")]
		public override ZDateTime KK_RequiredTo
		{
			get { return base.KK_RequiredTo; }
			set
			{
				var previousValue = KK_RequiredTo;
				base.KK_RequiredTo = value;

				if (previousValue != KK_RequiredTo)
				{
					RefreshRequiredToOnInstruction();
				}
			}
		}

		void RefreshRequiredToOnInstruction()
		{
			var instruction = Instruction;
			if (instruction != null)
			{
				instruction.ReqToInfo.RefreshBinding();
			}
		}

		#endregion

		#region KK_ReceivedBySignature

		public override ZBlob KK_ReceivedBySignature
		{
			get { return base.KK_ReceivedBySignature; }
			set
			{
				base.KK_ReceivedBySignature = value;
				RequiresSignatureEventLog = true;
			}
		}

		#endregion

		#region KK_ReceivedBy

		protected override void OnKK_ReceivedBy()
		{
			RequiresSignatureEventLog = true;
		}

		#endregion

		#region KK_Estimated

		#region UpdatePickupEstimatedTime

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
			var booking = Transport;
			return booking == null ? ZDateTime.Empty : (booking.KM_Status.EqualsIgnoringCase(TransportStatuses.Codes.Available) ? new[] { KK_RequiredFrom, ZDateTime.Now }.Max() : ZDateTime.Empty); // available means we have confirmed
		}

		#endregion

		public override ZDateTime KK_Estimated
		{
			get { return base.KK_Estimated; }
			set
			{
				base.KK_Estimated = value;
				var booking = Transport;
				var instruction = Instruction;
				if (booking != null && instruction != null)
				{
					if (instruction.IsPickUp && IsPickUp)
					{
						booking.UpdateDeliveryConfirmationEstimateTime();
					}

					instruction.EstimatedInfo.RefreshBinding();
				}
			}
		}

		#endregion

		// calculated

		#region AllocationTime

		public ZDateTime AllocationTime
		{
			get
			{
				var result = ZDateTime.Today;
				if (!KK_RequiredFrom.IsEmpty && KK_RequiredFrom > ZDateTime.Today)
				{
					result = KK_RequiredFrom;
				}
				return result;
			}
		}

		#endregion

		#region BillToPartyCode

		[ResourceStringData("DtbConsignmentConfirmation|BillToPartyCode", Caption = "Bill to Party", ShortCaption = "Bill To")]
		public ZString BillToPartyCode
		{
			get
			{
				ZString result = ZString.Empty;

				var job = Instruction.Booking.Job;
				if (job != null)
				{
					var billToParty = job.LocalCharges;
					result = (billToParty != null) ? billToParty.OH_Code : ZString.Empty;
				}

				return result;
			}
		}

		#endregion

		#region BookingID

		[ResourceStringData("DtbConsignmentConfirmation|BookingID", Caption = "Booking Number", MediumCaption = "Booking #", ShortCaption = "Booking")]
		public ZString BookingID
		{
			get { return Instruction.Booking.BookingID; }
		}

		#endregion

		#region ConsignmentID

		[ResourceStringData("DtbConsignmentConfirmation|ConsignmentID", Caption = "Consignment ID")]
		public ZString ConsignmentID
		{
			get { return Instruction?.Booking?.KM_JobID ?? ZString.Empty; }
		}

		#endregion

		#region Sequence

		[ResourceStringData("DtbConsignmentConfirmation|Sequence", Caption = "Sequence")]
		public ZString Sequence
		{
			get
			{
				var instruction = Instruction;
				var suffix = ZString.Empty;

				if (instruction.Confirmations.Count > 1)
				{
					suffix = IsDelivery ? (NoResString)"a" : (NoResString)"b"; // Constants for order sequence
				}

				return instruction.KN_Sequence + suffix;
			}
		}

		#endregion

		#region Services

		[ResourceStringData("DtbConsignmentConfirmation|Services", Caption = "Services", ShortCaption = "Services")]
		public ZString Services
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				foreach (var service in Instruction.Booking.Services.Cast<JobService>().Where(s => s.ES_CurrentContextID.IsEmpty || s.ES_CurrentContextID == Instruction.PK))
				{
					builder.Append(service.ES_ServiceCode);
				}
				return builder.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		#endregion

		// calculated -- consignor

		#region ConsignorAddressAsSingleLine

		[ResourceStringData("DtbConsignmentConfirmation|ConsignorAddressAsSingleLine", Caption = "Origin Full Address", ShortCaption = "Origin")]
		public ZString ConsignorAddressAsSingleLine
		{
			get { return Instruction.Booking.PickupInstruction.Address.GetAddressLine(Factory); }
		}

		#endregion

		#region ConsignorCity

		[ResourceStringData("DtbConsignmentConfirmation|ConsignorCity", Caption = "Origin City")]
		public ZString ConsignorCity
		{
			get { return Instruction.Booking.PickupInstruction.Address.E2_City; }
		}

		#endregion

		#region ConsignorName

		[ResourceStringData("DtbConsignmentConfirmation|ConsignorName", Caption = "Consignor")]
		public ZString ConsignorName
		{
			get { return Instruction.Booking.PickupInstruction.Address.E2_CompanyNameTruncated; }
		}

		#endregion

		#region ConsignorPostcode

		[ResourceStringData("DtbConsignmentConfirmation|ConsignorPostcode", Caption = "Origin Postcode", ShortCaption = "Origin P/C")]
		public ZString ConsignorPostcode
		{
			get { return Instruction.Booking.PickupInstruction.Address.E2_Postcode; }
		}

		#endregion

		#region ConsignorReference

		[ResourceStringData("DtbConsignmentConfirmation|ConsignorReference", Caption = "Consignor Reference", MediumCaption = "Consignor Ref.", ShortCaption = "CNR Ref.")]
		public ZString ConsignorReference
		{
			get { return IsPickUp ? KK_ReferenceNum : Instruction.Booking.PickupInstruction.Confirmations[0].KK_ReferenceNum; }
		}

		#endregion

		#region ConsignorState

		[ResourceStringData("DtbConsignmentConfirmation|ConsignorState", Caption = "Origin State")]
		public ZString ConsignorState
		{
			get { return Instruction.Booking.PickupInstruction.Address.E2_State; }
		}

		#endregion

		#region ConsignorZone

		[ResourceStringData("DtbConsignmentConfirmation|ConsignorZone", Caption = "Origin Zone")]
		public ZString ConsignorZone
		{
			get { return Instruction.Booking.PickupInstruction.Zone != null ? Instruction.Booking.PickupInstruction.Zone.TZ_ZoneName : ZString.Empty; }
		}

		#endregion

		// calculated -- consignee

		#region ConsigneeAddressAsSingleLine

		[ResourceStringData("DtbConsignmentConfirmation|ConsigneeAddressAsSingleLine", Caption = "Destination Full Address", MediumCaption = "Destination", ShortCaption = "Dest.")]
		public ZString ConsigneeAddressAsSingleLine
		{
			get { return Instruction.Booking.DeliveryInstruction.Address.GetAddressLine(Factory); }
		}

		#endregion

		#region ConsigneeCity

		[ResourceStringData("DtbConsignmentConfirmation|ConsigneeCity", Caption = "Destination City", ShortCaption = "Dest. City")]
		public ZString ConsigneeCity
		{
			get { return Instruction.Booking.DeliveryInstruction.Address.E2_City; }
		}

		#endregion

		#region ConsigneeName

		[ResourceStringData("DtbConsignmentConfirmation|ConsigneeName", Caption = "Consignee")]
		public ZString ConsigneeName
		{
			get { return Instruction.Booking.DeliveryInstruction.Address.E2_CompanyNameTruncated; }
		}

		#endregion

		#region ConsigneePostcode

		[ResourceStringData("DtbConsignmentConfirmation|ConsigneePostcode", Caption = "Destination Postcode", MediumCaption = "Destination P/C", ShortCaption = "Dest. P/C")]
		public ZString ConsigneePostcode
		{
			get { return Instruction.Booking.DeliveryInstruction.Address.E2_Postcode; }
		}

		#endregion

		#region ConsigneeReference

		[ResourceStringData("DtbConsignmentConfirmation|ConsigneeReference", Caption = "Consignee Reference", MediumCaption = "Consignee Ref.", ShortCaption = "CNE Ref.")]
		public ZString ConsigneeReference
		{
			get { return IsDelivery ? KK_ReferenceNum : Instruction.Booking.DeliveryInstruction.Confirmations[0].KK_ReferenceNum; }
		}

		#endregion

		#region ConsigneeState

		[ResourceStringData("DtbConsignmentConfirmation|ConsigneeState", Caption = "Destination State", ShortCaption = "Dest. State")]
		public ZString ConsigneeState
		{
			get { return Instruction.Booking.DeliveryInstruction.Address.E2_State; }
		}

		#endregion

		#region ConsignorZone

		[ResourceStringData("DtbConsignmentConfirmation|ConsigneeZone", Caption = "Destination Zone", ShortCaption = "Dest. Zone")]
		public ZString ConsigneeZone
		{
			get { return Instruction.Booking.DeliveryInstruction.Zone != null ? Instruction.Booking.DeliveryInstruction.Zone.TZ_ZoneName : ZString.Empty; }
		}

		#endregion

		// calculated - Received By / Signature

		#region HasSignature

		[ResourceStringData("DtbConsignmentConfirmation|HasSignature", ShortCaption = "Signature", Caption = "Has Signature")]
		public ZBool HasSignature
		{
			get { return !ReceivedBySignature.IsEmpty; }
		}

		#endregion

		#region ReceivedBySignature

		public override ZBlob ReceivedBySignature
		{
			get { return FallbackToRunSheetInstructionIfEmpty(KK_ReceivedBySignature, rsi => rsi.K1_ReceivedBySignature); }
		}

		#endregion

		#region ReceivedBy

		[ResourceStringData("DtbConsignmentConfirmation|ReceivedBy", Caption = "Signed By")]
		public override ZString ReceivedBy
		{
			get { return FallbackToRunSheetInstructionIfEmpty(KK_ReceivedBy, rsi => rsi.K1_ReceivedBy); }
		}

		#endregion

		#region FallbackToRunSheetInstructionIfEmpty

		T FallbackToRunSheetInstructionIfEmpty<T>(T onConfirmation, Func<DtbConsignmentRunSheetInstruction, T> getFromRunSheetInstruction)
			where T : IZType
		{
			var value = onConfirmation;

			if (value.IsEmpty)
			{
				var runSheetInstruction = RunSheetInstruction;
				value = runSheetInstruction != null ? getFromRunSheetInstruction(runSheetInstruction) : value;
			}

			return value;
		}

		#endregion

		// calculated -- totals

		#region BookedPickupPackageList

		public GroupedPackTypeCounts BookedPickupPackageList
		{
			get { return PackageTotalsBusinessObject != null ? PackageTotalsBusinessObject.BookedPickupPackageList : GroupedPackTypeCounts.Empty; }
		}

		#endregion

		#region PackageSummary

		[ResourceStringData("DtbConsignmentConfirmation|PackageSummary", Caption = "Summary")]
		public ZString PackageSummary
		{
			get { return GetTotal(() => PackageTotalsBusinessObject.PackageSummary, c => c.PackageSummary); }
		}

		#endregion

		#region PackageSummaryList

		public IEnumerable<PackTypeCount> PackageSummaryList
		{
			get { return PackageTotalsBusinessObject != null ? PackageTotalsBusinessObject.PackageSummaryList : Enumerable.Empty<PackTypeCount>(); }
		}

		#endregion

		#region TotalPackages

		[ResourceStringData("DtbConsignmentConfirmation|TotalPackages", Caption = "No. Packages", MediumCaption = "Packages", ShortCaption = "Packs")]
		public override ZInt TotalPackages
		{
			get { return GetTotal(DtbConsignmentConfirmationTotalsCache.TotalPackages, c => c.TotalPackages); }
		}

		#endregion

		#region TotalWeight

		[ResourceStringData("DtbConsignmentConfirmation|TotalWeight", Caption = "Weight", ShortCaption = "Wgt.")]
		public override ZDecimal TotalWeight
		{
			get { return GetTotal(DtbConsignmentConfirmationTotalsCache.TotalWeight, c => c.TotalWeight); }
		}

		#endregion

		#region TotalWeightUnit

		[ResourceStringData("DtbConsignmentConfirmation|TotalWeightUnit", Caption = "Weight Unit", MediumCaption = "Unit", ShortCaption = "UQ")]
		public override ZString TotalWeightUnit
		{
			get { return DtbTransportTotalsHelper.TotalWeightUnit; }
		}

		#endregion

		#region TotalVolume

		[ResourceStringData("DtbConsignmentConfirmation|TotalVolume", Caption = "Volume", ShortCaption = "Vol.")]
		public override ZDecimal TotalVolume
		{
			get { return GetTotal(DtbConsignmentConfirmationTotalsCache.TotalVolume, c => c.TotalVolume); }
		}

		#endregion

		#region TotalVolumeUnit

		[ResourceStringData("DtbConsignmentConfirmation|TotalVolumeUnit", Caption = "Volume Unit", MediumCaption = "Unit", ShortCaption = "UQ")]
		public override ZString TotalVolumeUnit
		{
			get { return DtbTransportTotalsHelper.TotalVolumeUnit; }
		}

		#endregion

		// see DtbConsignmentRunSheetInstructionTest.TestAutoCreatedDepotConfirmationsPullTotalsFromPicAndDlvInstructionConfirmations()
		// for the test, and to understand why we do this.
		T GetTotal<T>(ZString cachePropertyName, Func<DtbConsignmentConfirmation, T> getTotalForDepot)
			where T : IZType
		{
			return GetTotal(() => (T)PackageTotalsBusinessObject[cachePropertyName], getTotalForDepot);
		}

		T GetTotal<T>(Func<T> getSelfValue, Func<DtbConsignmentConfirmation, T> getTotalForDepot)
			where T : IZType
		{
			var result = default(T);

			if (PackageTotalsBusinessObject != null)
			{
				result = getSelfValue();
			}
			else if (IsOwnDepot)
			{
				var relatedConfirmation = GetRelatedConfirmationForDepot();
				result = getTotalForDepot(relatedConfirmation);
			}

			return result;
		}

		#endregion

		#region Flags

		#region IsHazardous

		[ResourceStringData("DtbConsignmentConfirmation|IsHazardous", Caption = "Hazardous", ShortCaption = "Haz.")]
		public ZBool IsHazardous
		{
			get { return Instruction.Booking.KM_IsHazardous; }
		}

		#endregion

		#region IsUnAllocated

		public bool IsUnAllocated
		{
			get { return KK_K1_RunSheetInstruction.IsEmpty; }
		}

		#endregion

		#region RequiresRefrigeration

		[ResourceStringData("DtbConsignmentConfirmation|RequiresRefrigeration", Caption = "Refrigeration", ShortCaption = "Re-frig.")]
		public ZBool RequiresRefrigeration
		{
			get { return Instruction.Booking.KM_RequiresRefrigeration; }
		}

		#endregion

		#region RequiresSignatureEventLog

		public bool RequiresSignatureEventLog
		{
			get { return requiresSignatureEventLog; }
			set
			{
				requiresSignatureEventLog = value;
				if (value && Instruction != null)
				{
					Instruction.MarkRequiresSignatureEventLog();
				}
			}
		}
		bool requiresSignatureEventLog;

		#endregion

		#endregion

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new[] { DtbBookingConfirmationSchema.Constants.KK_KN_BookingInstruction, DtbBookingConfirmationSchema.Constants.KK_KD_BookingInstructionPkgDivot });
			var clone = base.CloneInternal(args);
			return clone;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Lookups

		public new DtbConsignmentConfirmationLookups Lookups
		{
			get { return (DtbConsignmentConfirmationLookups)base.Lookups; }
		}

		protected override DtbTransportConfirmationLookups GetNewLookupsCore()
		{
			return new DtbConsignmentConfirmationLookups(this);
		}

		#endregion

		#region Validation

		public new DtbConsignmentConfirmationValidation Validation
		{
			get { return (DtbConsignmentConfirmationValidation)base.Validation; }
		}

		protected override DtbTransportConfirmationValidation GetNewValidationCore()
		{
			return new DtbConsignmentConfirmationValidation(this);
		}

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbConsignmentConfirmationFetchStrategy(this);
		}

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new DtbConsignmentConfirmationDocumentSupporter(this)); }
		}

		DocumentSupporter documentSupporter;

		#endregion

		#region IPalletTransactionParent Members

		ZString IPalletTransactionParent.GetJobDescription(Type contextType)
		{
			if (contextType == typeof(DtbConsignmentRunSheetInstruction))
			{
				return Res.GetString("28657b81-9f40-43b1-bc81-c5c6f4fc5b13", "Consignment {0}", ConsignmentID);
			}
			else if (contextType == typeof(DtbBookingConsignment))
			{
				return IsPickUp ? Res.GetString("eb5d4909-2525-413b-bb76-bce5a52e3782", "Pickup") : Res.GetString("86ed7026-98a4-47a6-ae4e-21a034430841", "Delivery");
			}

			var companyName = IsDelivery ? ConsigneeName : ConsignorName;
			var city = IsDelivery ? ConsigneeCity : ConsignorCity;

			return Invariant($"{ConsignmentID} - {KK_ConfirmationType} @ {Instruction.OrganisationType}: {companyName} {city}"); // Job description doesn't need translation
		}

		IDocAddress IPalletTransactionParent.TransferFrom(string transferType)
		{
			IDocAddress result = null;
			if (transferType == PalletTransferTypeList.Codes.Direct || (IsPickUp && transferType == PalletTransferTypeList.Codes.TransferOn))
			{
				result = Instruction.Booking.PickupInstruction.Address;
			}
			else if (IsDelivery && transferType == PalletTransferTypeList.Codes.TransferOff)
			{
				result = GlbCompany.CurrentCompany.OrgProxy.MainAddress;
			}
			return result;
		}

		IDocAddress IPalletTransactionParent.TransferTo(string transferType)
		{
			IDocAddress result = null;
			if (transferType == PalletTransferTypeList.Codes.Direct || (IsDelivery && transferType == PalletTransferTypeList.Codes.TransferOff))
			{
				result = Instruction.Booking.DeliveryInstruction.Address;
			}
			else if (IsPickUp && transferType == PalletTransferTypeList.Codes.TransferOn)
			{
				result = GlbCompany.CurrentCompany.OrgProxy.MainAddress;
			}
			return result;
		}

		GlbBranch IPalletTransactionParent.RelevantBranch
		{
			get { return Instruction != null ? Instruction.DepotForAddress : null; }
		}

		IEnumerable<string> IPalletTransactionParent.JobReferences
		{
			get
			{
				yield return ConsignmentID;
				if (Instruction != null && !Instruction.Booking.KM_TransportReference.IsEmpty && Instruction.Booking.KM_TransportReference != ConsignmentID)
				{
					yield return Instruction.Booking.KM_TransportReference;
				}
			}
		}

		#endregion

		public override void SetIsEmptyContainerOnThisAndAllRelatedConfirmations()
		{
			// does nothing - DtbBookingConsignments are to be imminently deprecated
		}
	}
}
