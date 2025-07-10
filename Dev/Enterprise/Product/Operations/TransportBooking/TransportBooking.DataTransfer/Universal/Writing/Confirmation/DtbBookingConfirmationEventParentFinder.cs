using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Packing.DataTransfer.Universal.UnitHelper;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	class DtbBookingConfirmationEventParentFinder : EventParentFinder
	{
		internal DtbBookingConfirmationEventParentFinder(BusinessObjectFactory factory, DtbBookingConfirmationDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		/// <summary>
		/// If event contains multiple container numbers, match (and possibly split) to a confirmation per container.
		/// </summary>
		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject)
		{
			var result = new List<BusinessObject>();
			var context = ((IXmlEventValueObject)eventDataObject).Context;
			var matchingFieldsPerContainer = new List<ConfirmationMatchingFields>();

			if (context.ContainerNumbers != null && context.ContainerNumbers.Any())
			{
				foreach (var containerNumber in context.ContainerNumbers)
				{
					matchingFieldsPerContainer.Add(new ConfirmationMatchingFields(eventDataObject, factory, containerNumber));
				}
			}
			else
			{
				matchingFieldsPerContainer.Add(new ConfirmationMatchingFields(eventDataObject, factory, ""));
			}

			foreach (var matchingField in matchingFieldsPerContainer)
			{
				var booking = GetBooking(matchingField.BookingID);
				if (booking != null)
				{
					result.AddRange(GetLogParentsForEventUsingContextAndReferenceParameters(booking, matchingField));
				}
			}

			var resultToReturn = result.Where(o => o != null);
			return resultToReturn.Any() ? resultToReturn.ToArray() : null;
		}

		BusinessObject[] GetLogParentsForEventUsingContextAndReferenceParameters(DtbBooking booking, ConfirmationMatchingFields fields)
		{
			BusinessObject[] result = null;

			var instruction = GetInstruction(booking, fields);
			if (instruction != null)
			{
				var confirmations = GetOrCreateConfirmations(instruction, fields);
				if (confirmations.Any())
				{
					result = confirmations;
				}
			}

			// scan events
			if (result == null && !fields.PackageID.IsEmpty)
			{
				var firstMatchingPackage = booking.PackageJob.Packages.FirstOrDefault(p => p.KP_PackageID == fields.PackageID);
				result = firstMatchingPackage == null ? null : new[] { firstMatchingPackage };
			}

			// fall back to booking
			if (result == null)
			{
				logger.Log(LogType.Warning, string.Format(CultureInfo.CurrentCulture, @"Couldn't determine exact Instruction match for Event '{0}'. Adding to Transport Booking instead.", fields.EventType));
				result = new[] { booking };
			}

			return result;
		}

		DtbBookingInstruction GetInstruction(DtbBooking booking, ConfirmationMatchingFields fields)
		{
			return !fields.InstructionID.IsEmpty ? GetInstructionWithID(booking, fields.InstructionID) : GetInstructionWithoutID(booking, fields);
		}

		DtbBookingInstruction GetInstructionWithID(DtbBooking booking, ZString instructionID)
		{
			var sequence = ZInt.ParseSafe(instructionID, -1);
			return sequence != -1 ? booking.Instructions.FirstOrDefault(i => i.KN_Sequence == sequence) : null;
		}

		DtbBookingInstruction GetInstructionWithoutID(DtbBooking transportBooking, ConfirmationMatchingFields fields)
		{
			var result = new List<DtbBookingInstruction>();
			var instructionTypes = GetInstructionTypes(fields.EventType, fields.Reason, fields.Facility);

			result.AddRange(GetMatchedInstructions(transportBooking.Instructions, fields, instructionTypes, compareCity: true));

			if (!result.Any())
			{
				result.AddRange(GetMatchedInstructions(transportBooking.Instructions, fields, instructionTypes, compareCity: false));
			}

			if (result.Count > 1)
			{
				result.Clear(); // could not find exact match
			}

			return result.SingleOrDefault();
		}

		IEnumerable<DtbBookingInstruction> GetMatchedInstructions(DtbBookingInstructionCollection instructions, ConfirmationMatchingFields fields, ZString[] instructionTypes, bool compareCity = true)
		{
			foreach (var instruction in instructions)
			{
				var address = instruction.Address;

				var match = instructionTypes.Contains(instruction.KN_InstructionType)
							&& fields.IsMatchingAddress(address, compareCity)
							&& IsInstructionPackageMatch(instruction, fields);

				if (match)
				{
					yield return instruction;
				}
			}
		}

		ZBool IsInstructionPackageMatch(DtbBookingInstruction instruction, ConfirmationMatchingFields fields)
		{
			var result = false;

			if (!fields.PackageID.IsEmpty)
			{
				result = instruction.DivotsWithPackages.Packages.Any(p => p.KP_PackageID == fields.PackageID);
			}
			else
			{
				result = !fields.HasPackageInfo
					|| IsPackageInfoMatch(instruction.PackageDivots, fields, PackageInfoMatch.Exact)
					|| IsPackageInfoMatch(instruction.PackageDivots, fields, PackageInfoMatch.LessThan);
			}

			return result;
		}

		/// <summary>
		/// Find Match
		/// Otherwise create Confirmation for a Package, or fall back to an ALL confirmation
		/// If ALL confirmation, try and find the match again (by splitting the ALL etc)
		/// </summary>
		DtbBookingConfirmation[] GetOrCreateConfirmations(DtbBookingInstruction instruction, ConfirmationMatchingFields fields)
		{
			var matchingConfirmations = GetMatchingConfirmations(instruction, fields);
			if (!matchingConfirmations.Any())
			{
				matchingConfirmations = CreateConfirmation(instruction, fields);
				if (matchingConfirmations.Length == 1 && matchingConfirmations.First().PackageDivot == null)
				{
					matchingConfirmations = GetMatchingConfirmations(instruction, fields);
				}
			}

			return matchingConfirmations;
		}

		DtbBookingConfirmation[] CreateConfirmation(DtbBookingInstruction instruction, ConfirmationMatchingFields fields)
		{
			var result = Array.Empty<DtbBookingConfirmation>();
			var confirmationType = GetConfirmationType(fields.EventType, fields.Reason, fields.Facility, instruction);

			if (!fields.PackageID.IsEmpty)
			{
				var packageDivot = instruction.PackageDivots.FirstOrDefault(p => p.Package.KP_PackageID == fields.PackageID);
				if (packageDivot != null)
				{
					result = new[] { packageDivot.ConfirmationsDivotOnly.AddNew(confirmationType) };
				}
				// else // why was an instruction found with this package id, but no divot is found? Inner package?
			}

			// Create All Confirmation but only if no confirmations of that type exist yet
			if (!result.Any() && !instruction.Confirmations.Any(c => c.KK_ConfirmationType == confirmationType))
			{
				result = new[] { instruction.Confirmations.AddNew(confirmationType) };
			}

			return result;
		}

		/// <summary>
		/// Find matching Confirmation
		///		1. Package ID
		///		2. Exact Package Info Match		...but not if it has that event
		///		3. Part Info match				...but not if it has that event			...requires split
		///		4. Multi Match					...but not if it has that event			...may require split if greater than total
		///		5. Fallback to ALL														...requires split if has Pack Info
		/// </summary>
		DtbBookingConfirmation[] GetMatchingConfirmations(DtbBookingInstruction instruction, ConfirmationMatchingFields fields)
		{
			var matches = new ConfirmationMatches();
			var confirmationType = GetConfirmationType(fields.EventType, fields.Reason, fields.Facility, instruction);
			var validConfirmations = instruction.Confirmations.Where(c => c.KK_ConfirmationType == confirmationType).ToArray();
			foreach (var confirmation in validConfirmations)
			{
				var divot = confirmation.PackageDivot;
				if (divot != null)
				{
					if (!fields.PackageID.IsEmpty && divot.Package != null && divot.Package.KP_PackageID == fields.PackageID)
					{
						matches.ID = confirmation;
						break; // best match
					}
					else if (fields.HasPackageInfo && !HasEventAlready(confirmation, fields))
					{
						PopulatePackageInfoMatches(confirmation, matches, fields); // cases 2, 3, 4
					}
				}
				else
				{
					matches.All = confirmation;
				}
			}

			return GetMatchingConfirmationsAfterSplitIfRequired(instruction, fields, matches);
		}

		void PopulatePackageInfoMatches(DtbBookingConfirmation confirmation, ConfirmationMatches matches, ConfirmationMatchingFields fields)
		{
			if (matches.ExactPackage == null && IsPackageInfoMatch(confirmation, fields, PackageInfoMatch.Exact))
			{
				matches.ExactPackage = confirmation;
			}
			else if (matches.PartPackage == null && IsPackageInfoMatch(confirmation, fields, PackageInfoMatch.LessThan))
			{
				matches.PartPackage = confirmation;
			}
			else if (IsPackageInfoMatch(confirmation, fields, PackageInfoMatch.GreaterThan))
			{
				var runningTotal = matches.MultiPackage.Sum(c => c.KK_Quantity);
				if (runningTotal < fields.Packs)
				{
					matches.MultiPackage.Add(confirmation); // split later if total breaks packcount
				}
			}
		}

		bool HasEventAlready(DtbBookingConfirmation confirmation, ConfirmationMatchingFields fields)
		{
			return confirmation.Logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == fields.EventType);
		}

		DtbBookingConfirmation[] GetMatchingConfirmationsAfterSplitIfRequired(DtbBookingInstruction instruction, ConfirmationMatchingFields fields, ConfirmationMatches matches)
		{
			var result = Array.Empty<DtbBookingConfirmation>();

			if (matches.ID != null)
			{
				result = new[] { matches.ID };
			}
			else if (matches.ExactPackage != null)
			{
				result = new[] { matches.ExactPackage };
			}
			else if (matches.PartPackage != null)
			{
				SplitConfirmation(matches.PartPackage, fields.Packs);
				result = new[] { matches.PartPackage };
			}
			else if (matches.MultiPackage.Any())
			{
				SplitMultiIfRequired(matches.MultiPackage, fields.Packs);
				result = matches.MultiPackage.ToArray();
			}
			else if (matches.All != null)
			{
				var maySplit = (!fields.Packs.IsEmpty || !fields.ContainerNumber.IsEmpty) && fields.Packs < instruction.PackageQty;
				var hasNoSplitsAlready = !instruction.Confirmations.Any(c => c.KK_ConfirmationType == matches.All.KK_ConfirmationType && c.PackageDivot != null);
				if (maySplit && hasNoSplitsAlready)
				{
					matches.All.SplitFromInstructionToPackageDivots(null); // split and try match again if split successfull
					var hasDivotConfirmations = instruction.Confirmations.Any(c => c.PackageDivot != null);
					if (hasDivotConfirmations)
					{
						result = GetMatchingConfirmations(instruction, fields);
					}
				}
				else
				{
					result = new[] { matches.All };
				}
			}

			return result;
		}

		void SplitConfirmation(DtbBookingConfirmation confirmationToSplit, ZInt qtyToKeep)
		{
			var splitConfirmation = (DtbBookingConfirmation)confirmationToSplit.Clone();
			splitConfirmation.KK_Quantity = confirmationToSplit.KK_Quantity - qtyToKeep;
			confirmationToSplit.KK_Quantity = qtyToKeep;
		}

		void SplitMultiIfRequired(List<DtbBookingConfirmation> confirmations, ZInt qtyToKeep)
		{
			var total = confirmations.Sum(c => c.KK_Quantity);
			if (total > qtyToKeep)
			{
				var qtyToRemoved = total - qtyToKeep;
				var confirmationMatchingQtyToRemove = confirmations.FirstOrDefault(c => c.KK_Quantity == qtyToRemoved);
				if (confirmationMatchingQtyToRemove != null)
				{
					confirmations.Remove(confirmationMatchingQtyToRemove);
				}
				else
				{
					var confirmationToSplit = confirmations.FirstOrDefault(c => c.KK_Quantity > qtyToRemoved);
					if (confirmationToSplit != null)
					{
						SplitConfirmation(confirmationToSplit, confirmationToSplit.KK_Quantity - qtyToRemoved);
					}
				}
			}
			//else (total < qtyToKeep)
			//{
			//	// create some new confirmations to reach the total (unless the qty is greater than whats on the instruction, 
			//	// but that shouldn't be the case because the instruction was chosen based on less then or exact
			//}
		}

		ZString[] GetInstructionTypes(ZString eventType, ZString reason, ZString facility)
		{
			switch (eventType)
			{
				case Events.PickedUpCode:
					return new ZString[] { InstructionTypes.Codes.PickUp, InstructionTypes.Codes.Multi };
				case Events.DeliveredCode:
					return new ZString[] { InstructionTypes.Codes.Multi, InstructionTypes.Codes.Delivery };
				case Events.DeliveryCartageCompleteFinalisedCode:
					return new ZString[] { InstructionTypes.Codes.Delivery, InstructionTypes.Codes.Multi };
				case Events.PickupCartageCompleteFinalisedCode:
					return new ZString[] { InstructionTypes.Codes.PickUp, InstructionTypes.Codes.Multi };
				case Events.GateInCode when facility == CargoWise.EventReference.Constants.Facilities.Code.ContainerYard:
					return new ZString[] { InstructionTypes.Codes.Delivery };
				case Events.CartageCompleteFinalisedCode:
					return new ZString[] { InstructionTypes.Codes.Delivery };
				case Events.ServiceCommencedCode:
					return new ZString[] { InstructionTypes.Codes.Delivery };

				default: // in, out, etc. fallback to reason
					switch (reason)
					{
						case Core.Constants.EventReferenceParameterReasons.Pickup:
						case Core.Constants.EventReferenceParameterReasons.Pack:
							return new ZString[] { InstructionTypes.Codes.PickUp, InstructionTypes.Codes.Multi };
						case Core.Constants.EventReferenceParameterReasons.Delivery:
						case Core.Constants.EventReferenceParameterReasons.Unpack:
							return new ZString[] { InstructionTypes.Codes.Multi, InstructionTypes.Codes.Delivery };

						default:
							return new ZString[] { InstructionTypes.Codes.PickUp, InstructionTypes.Codes.Multi, InstructionTypes.Codes.Delivery };
					}
			}
		}

		ZString GetConfirmationType(ZString eventType, ZString reason, ZString facility, DtbBookingInstruction instruction)
		{
			switch (eventType)
			{
				case Events.PickedUpCode:
					return ConfirmationTypes.Codes.PickUp;
				case Events.DeliveredCode:
					return ConfirmationTypes.Codes.Delivery;
				case Events.DeliveryCartageCompleteFinalisedCode:
					return ConfirmationTypes.Codes.Delivery;
				case Events.PickupCartageCompleteFinalisedCode:
					return ConfirmationTypes.Codes.PickUp;
				case Events.GateInCode when facility == CargoWise.EventReference.Constants.Facilities.Code.ContainerYard:
					return ConfirmationTypes.Codes.Delivery;
				case Events.CartageCompleteFinalisedCode:
					return ConfirmationTypes.Codes.Delivery;
				case Events.ServiceCommencedCode:
					return ConfirmationTypes.Codes.ConNoteNo;

				default: // in, out, etc. fall back to reason
					switch (reason)
					{
						case Core.Constants.EventReferenceParameterReasons.Pickup:
						case Core.Constants.EventReferenceParameterReasons.Pack:
							return ConfirmationTypes.Codes.PickUp;
						case Core.Constants.EventReferenceParameterReasons.Delivery:
						case Core.Constants.EventReferenceParameterReasons.Unpack:
							return ConfirmationTypes.Codes.Delivery;

						default: // no reason - fall back to booking direction
							return instruction.GetDefaultConfirmationCode();
					}
			}
		}

		DtbBooking GetBooking(ZString transportBookingJobID)
		{
			return !transportBookingJobID.IsEmpty ? factory.LoadTop1<DtbBooking>(new ZQuery(DtbBookingSchema.KM_JobID, transportBookingJobID)) : null;
		}

		enum PackageInfoMatch
		{
			Exact,
			LessThan,
			GreaterThan
		}

		ZBool IsPackageInfoMatch(IEnumerable<DtbBookingInstructionPkgDivot> packageDivots, ConfirmationMatchingFields fields, PackageInfoMatch matchType)
		{
			if (!fields.HasPackageInfo)
			{
				throw new InvalidOperationException("Can only match if Package Info is provided on the event.");
			}

			var validPackageDivots = fields.IgnorePackType ? packageDivots : packageDivots.Where(d => d.Package.KP_F3_NKPackType == fields.PackType);

			return IsPackCountMatch(validPackageDivots.Select(d => d.KD_Quantity), fields, matchType)
				&& IsWeightMatch(validPackageDivots.Select(d => new ZWeight(d.Weight, d.Package.KP_WeightUQ)), fields, matchType)
				&& IsVolumeMatch(validPackageDivots.Select(d => new ZVolume(d.Volume, d.Package.KP_VolumeUQ)), fields, matchType);
		}

		ZBool IsPackageInfoMatch(DtbBookingConfirmation confirmation, ConfirmationMatchingFields fields, PackageInfoMatch matchType)
		{
			if (!fields.HasPackageInfo)
			{
				throw new InvalidOperationException("Can only match if Package Info is provided on the event.");
			}

			var packageDivot = confirmation.PackageDivot
				?? throw new InvalidOperationException("Can only match if Confirmation has a Package Divot.");

			var isPackTypeMatch = fields.IgnorePackType || packageDivot.Package.KP_F3_NKPackType == fields.PackType;
			var proportion = Math.Min(1m, packageDivot.KD_Quantity > 0 ? (ZDecimal)confirmation.KK_Quantity / packageDivot.KD_Quantity : 0m);

			return isPackTypeMatch
				&& IsPackCountMatch(new[] { confirmation.KK_Quantity }, fields, matchType)
				&& IsWeightMatch(new[] { new ZWeight(proportion * packageDivot.Weight, packageDivot.Package.KP_WeightUQ) }, fields, matchType)
				&& IsVolumeMatch(new[] { new ZVolume(proportion * packageDivot.Volume, packageDivot.Package.KP_VolumeUQ) }, fields, matchType);
		}

		ZBool IsPackCountMatch(IEnumerable<ZInt> packCounts, ConfirmationMatchingFields fields, PackageInfoMatch matchType)
		{
			var result = fields.Packs.IsEmpty;
			if (!result)
			{
				var divotPackageCount = packCounts.Sum(d => d);
				result = IsMatch((ZDecimal)fields.Packs, divotPackageCount, matchType);
			}

			return result;
		}

		ZBool IsWeightMatch(IEnumerable<ZWeight> weights, ConfirmationMatchingFields fields, PackageInfoMatch matchType)
		{
			var result = fields.Weight.IsEmpty;
			if (!result)
			{
				var divotWeight = (ZDecimal)weights.Sum(w => ConvertWeightIfValid(w.Amount, w.Unit, fields.Weight.Unit));
				var weightRounded = fields.Weight.Amount.Truncate(3);
				var divotWeightRounded = divotWeight.Truncate(3);
				result = IsMatch(weightRounded, divotWeightRounded, matchType);
			}

			return result;
		}

		ZBool IsVolumeMatch(IEnumerable<ZVolume> volumes, ConfirmationMatchingFields fields, PackageInfoMatch matchType)
		{
			var result = fields.Volume.IsEmpty;
			if (!result)
			{
				var divotVolume = (ZDecimal)volumes.Sum(v => ConvertVolumeIfValid(v.Amount, v.Unit, fields.Volume.Unit));
				var volumeRounded = fields.Volume.Amount.Truncate(3);
				var divotVolumeRounded = divotVolume.Truncate(3);
				result = IsMatch(volumeRounded, divotVolumeRounded, matchType);
			}

			return result;
		}

		ZBool IsMatch(ZDecimal value1, ZDecimal value2, PackageInfoMatch matchType)
		{
			switch (matchType)
			{
				case PackageInfoMatch.Exact:
					return value1 == value2;
				case PackageInfoMatch.LessThan:
					return value1 < value2;
				case PackageInfoMatch.GreaterThan:
					return value1 > value2;
				default:
					return false;
			}
		}

		ZDecimal ConvertWeightIfValid(decimal confirmationWeightValue, ZString confirmationWeightUnit, ZString eventWeightUnit)
		{
			var areUnitsValid = Constants.Weight.ContainsCode(confirmationWeightUnit) && Constants.Weight.ContainsCode(eventWeightUnit);
			return areUnitsValid ? Constants.Weight.ConvertSafe(confirmationWeightValue, confirmationWeightUnit, eventWeightUnit) : confirmationWeightValue;
		}

		ZDecimal ConvertVolumeIfValid(decimal confirmationVolumeValue, ZString confirmationVolumeUnit, ZString eventVolumeUnit)
		{
			var areUnitsValid = Constants.Volume.ContainsCode(confirmationVolumeUnit) && Constants.Volume.ContainsCode(eventVolumeUnit);
			return areUnitsValid ? Constants.Volume.ConvertSafe(confirmationVolumeValue, confirmationVolumeUnit, eventVolumeUnit) : confirmationVolumeValue;
		}

		class ConfirmationMatches
		{
			public DtbBookingConfirmation ID { get; set; }
			public DtbBookingConfirmation ExactPackage { get; set; }
			public DtbBookingConfirmation PartPackage { get; set; }
			public List<DtbBookingConfirmation> MultiPackage
			{
				get { return multiPackage ?? (multiPackage = new List<DtbBookingConfirmation>()); }
			}
			List<DtbBookingConfirmation> multiPackage;
			public DtbBookingConfirmation All { get; set; }
		}

		class ConfirmationMatchingFields : TransportMatchingFields
		{
			public ConfirmationMatchingFields(UniversalEvent eventDataObject, BusinessObjectFactory factory, string containerNumber)
				: base(eventDataObject, factory, containerNumber)
			{
			}

			public ZString BookingID
			{
				get { return Context.TransportBookingJobID; }
			}

			public ZString InstructionID
			{
				get { return Context.TransportBookingInstructionID; }
			}

			public ZInt Packs
			{
				get { return Context.NumberOfPieces; }
			}

			public ZString PackType
			{
				get { return Context.PackageType; }
			}

			public ZBool IgnorePackType
			{
				get { return PackType.IsEmpty || PackType == Constants.PkgUnit.Piece || !PackageTypes.ContainsCode(PackType); }
			}

			CodeDescriptionPairList PackageTypes
			{
				get { return packageTypes ?? (packageTypes = new RefPackTypeCollection(Factory).GetAsCodeDescriptionPair()); }
			}
			CodeDescriptionPairList packageTypes;

			public ZWeight Weight
			{
				get
				{
					var weightAndUnit = Context.WeightOfGoods;
					return new ZWeight(GetValueFromValueAndUnit(weightAndUnit), GetUnitFromValueAndUnit(weightAndUnit));
				}
			}

			public ZVolume Volume
			{
				get
				{
					var volumeAndUnit = Context.VolumeOfGoods;
					return new ZVolume(GetValueFromValueAndUnit(volumeAndUnit), GetUnitFromValueAndUnit(volumeAndUnit));
				}
			}

			public ZBool HasPackageInfo
			{
				get { return !Packs.IsEmpty || !Weight.IsEmpty || !Volume.IsEmpty || !PackType.IsEmpty; }
			}

			public ZString Reason
			{
				get { return EventParameters.GetEventParameter(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, Parameters, FallbackReference).GetValueOrDefault(); }
			}
		}
	}
}
