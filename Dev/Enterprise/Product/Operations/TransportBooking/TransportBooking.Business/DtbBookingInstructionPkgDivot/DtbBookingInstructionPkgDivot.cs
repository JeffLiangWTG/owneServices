using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Common = Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingInstructionPkgDivot :
		Common.AutoDtbBookingInstructionPkgDivot,
		IDtbBookingInstructionPkgDivot
	{
		public DtbBookingInstructionPkgDivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DtbBookingInstruction Instruction
		{
			get { return (DtbBookingInstruction)Factory.Load(InstructionType, KD_KN_BookingInstruction); }
		}

		Type InstructionType
		{
			get { return typeof(DtbBookingInstruction); }
		}

		public PkgPackage Package
		{
			get { return Factory.Load<PkgPackage>(KD_KP_Package); }
		}

		public DtbBookingConfirmationCollection ConfirmationsDivotOnly
		{
			get { return confirmationsDivotOnly ?? (confirmationsDivotOnly = GetNewConfirmationsDivotOnlyCollection()); }
		}

		DtbBookingConfirmationCollection confirmationsDivotOnly;

		DtbBookingConfirmationCollection GetNewConfirmationsDivotOnlyCollection()
		{
			return new DtbBookingConfirmationCollection(this);
		}

		public DtbBookingConfirmationCollection Confirmations
		{
			get { return confirmations ?? (confirmations = GetNewConfirmationsCollection()); }
		}

		DtbBookingConfirmationCollection confirmations;

		DtbBookingConfirmationCollection GetNewConfirmationsCollection()
		{
			return new DtbBookingConfirmationCollection(Factory, new PackageDivotInstructionConfirmationRelationship(this));
		}

		// persistent

		[RelatedBusinessObject("Instruction")]
		public override ZGuid KD_KN_BookingInstruction
		{
			get { return base.KD_KN_BookingInstruction; }
			set
			{
				var previousValue = KD_KN_BookingInstruction;
				base.KD_KN_BookingInstruction = value;

				if (previousValue != KD_KN_BookingInstruction)
				{
					var instruction = Instruction;
					if (instruction != null)
					{
						OnDivotAssignedToInstruction();
					}
				}
			}
		}

		void OnDivotAssignedToInstruction()
		{
			var instruction = Instruction;

			if (instruction.Booking is DtbBooking booking && !booking.DelayInstructionUpdatesFromAddingDivots)
			{
				AfterDivotsAssignedToInstruction(instruction, this);
			}
		}

		public static void AfterDivotsAssignedToInstruction(DtbBookingInstruction instruction)
		{
			AfterDivotsAssignedToInstruction(instruction, null);
		}

		static void AfterDivotsAssignedToInstruction(DtbBookingInstruction instruction, DtbBookingInstructionPkgDivot divot)
		{
			instruction.DefaultDatesAndReferencesFromParent(divot);

			if (divot == null || !divot.KD_KP_Package.IsEmpty)
			{
				instruction.CreateDefaultConfirmation();

				// tested by Instruction.TestSetDropModeFromInstructionAddress
				instruction.DefaultDropMode();
			}
		}

		[RelatedBusinessObject("Package")]
		public override ZGuid KD_KP_Package
		{
			get { return base.KD_KP_Package; }
			set
			{
				var previousPackage = Package;
				var previousValue = KD_KP_Package;
				base.KD_KP_Package = value;

				SetQuantityFromPackage();

				if (!previousValue.IsEmpty || !KD_KP_Package.IsEmpty)
				{
					var instruction = Instruction;
					if (instruction != null)
					{
						instruction.DefaultDropMode();
					}

					OnPackageAssignedOrUnassignedToDivot();
				}
				if (!KD_KP_Package.Equals(previousPackage?.PK))
				{
					OnPackageUpdated(previousPackage);
					previousPackage?.OuterPackage.ClearActionStrategyCacheIncludingChildren();
					Package?.OuterPackage.ClearActionStrategyCacheIncludingChildren();
				}
			}
		}

		void OnPackageAssignedOrUnassignedToDivot()
		{
			var instruction = Instruction;
			if (instruction != null)
			{
				instruction.CreateDefaultConfirmation();
			}
		}

		void OnPackageUpdated(PkgPackage previousPackage)
		{
			Instruction?.Booking?.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(KD_KP_PackageInfo, previousPackage?.PK ?? ZGuid.Empty), IsCopying);
			if (previousPackage != null)
			{
				previousPackage.KP_WeightInfo.ValueChanged -= Package_WeightChanged;
				previousPackage.KP_WeightUQInfo.ValueChanged -= Package_WeightChanged;
				previousPackage.KP_PackageQtyInfo.ValueChanged -= Package_WeightChanged;
			}

			if (Package != null)
			{
				Package.KP_WeightInfo.ValueChanged += Package_WeightChanged;
				Package.KP_WeightUQInfo.ValueChanged += Package_WeightChanged;
				Package.KP_PackageQtyInfo.ValueChanged += Package_WeightChanged;
			}
		}

		void Package_WeightChanged(object sender, EventArgs e)
		{
			if (!IsDeleted && !IsDeleting)
			{
				Instruction?.Booking?.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(e as ValueChangedEventArgs), IsCopying);
			}
		}

		void SetQuantityFromPackage()
		{
			var package = Package;
			KD_Quantity = (package != null) ? package.KP_PackageQty : ZInt.Zero;
		}

		// calculated

		public ZString PackageDescription
		{
			get
			{
				var package = Package;
				return package != null ? package.ToStringPackageSummary() : ZString.Empty;
			}
		}

		public ZString PackageDescriptionWithIDAndQty
		{
			get
			{
				var package = Package;
				var result = ZString.Empty;

				if (package != null)
				{
					if (!package.KP_PackageID.IsEmpty)
					{
						result = package.KP_PackageID;
					}
					else
					{
						result = Res.GetString("f2cf3553-d145-449c-948d-181ac43b0b62", "{0} of {1}", KD_Quantity, package.ToStringPackageSummary());
					}
				}

				return result;
			}
		}

		public bool IsContainerised { get { return Package?.IsContainer ?? false; } }

		[ResourceStringData("DtbBookingInstructionPkgDivot|Weight", Caption = "Weight")]
		public ZDecimal Weight
		{
			get
			{
				var package = Package;
				return (package != null) ? package.KP_Weight * Proportion : 0m;
			}
		}

		public ZPropertyInfo WeightInfo
		{
			get { return GetZPropertyInfo(nameof(Weight)); }
		}

		[ResourceStringData("DtbBookingInstructionPkgDivot|Volume", Caption = "Volume")]
		public ZDecimal Volume
		{
			get
			{
				var package = Package;
				return (package != null) ? package.KP_Volume * Proportion : 0m;
			}
		}

		public ZPropertyInfo VolumeInfo
		{
			get { return GetZPropertyInfo(nameof(Volume)); }
		}

		public ZString WeightUQ
		{
			get
			{
				return Package?.KP_WeightUQ ?? "";
			}
		}

		public ZString VolumeUQ
		{
			get
			{
				return Package?.KP_VolumeUQ ?? "";
			}
		}

		ZDecimal Proportion
		{
			get
			{
				var package = Package;
				var proportion = package != null && package.KP_PackageQty > 0 ? (ZDecimal)KD_Quantity / package.KP_PackageQty : 0m;
				return Math.Min(proportion, 1m);
			}
		}

		public ZString PackType
		{
			get { return Package?.KP_F3_NKPackType ?? ""; }
		}

		public ZString PackageID
		{
			get { return Package?.KP_PackageID ?? ""; }
		}

		// tested by Instruction.TestSetDropModeFromInstructionAddress
		public override void Delete()
		{
			Instruction?.Booking?.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(freeTextReason: (NoResString)"Instruction package divot deleted"), IsCopying);
			Package?.OuterPackage.ClearActionStrategyCacheIncludingChildren();

			var package = !IsDeleted ? Package : null;

			var instruction = !IsDeleted ? Instruction : null;

			ConfirmationsDivotOnly.DeleteAll();
			base.Delete();

			if (instruction?.Booking != null && package?.PackageJob != null && package.PackageJob == instruction.Booking.PackageJob)
			{
				instruction.Booking.KM_IsHazardous = instruction.Booking.IsAnyPackageHazardous;
				instruction.Booking.KM_RequiresRefrigeration = instruction.Booking.IsAnyPackageRequiresRefridgeration;
			}

			if (instruction != null)
			{
				instruction.DefaultDropMode();
			}
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = Factory.GetCachedValue("DtbBookingInstructionPkgDivotUniqueIndexHandler", () => new DtbBookingInstructionPkgDivotUniqueIndexHandler(new[]
				{
					DtbBookingInstructionPkgDivotSchema.Constants.Indexes.FK_UC__KD_KN_BookingInstruction_KD_KP_Package
				})));
			}
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbBookingInstructionPkgDivotFetchStrategy(this);
		}

		public new DtbBookingInstructionPkgDivotLookups Lookups
		{
			get { return (DtbBookingInstructionPkgDivotLookups)base.Lookups; }
		}

		protected override Common.DtbBookingInstructionPkgDivotLookups GetNewLookups()
		{
			return new DtbBookingInstructionPkgDivotLookups(this);
		}

		public new DtbBookingInstructionPkgDivotValidation Validation
		{
			get { return (DtbBookingInstructionPkgDivotValidation)base.Validation; }
		}

		protected override Common.DtbBookingInstructionPkgDivotValidation GetNewValidation()
		{
			return new DtbBookingInstructionPkgDivotValidation(this);
		}

		public ZBool IsLoose
		{
			get
			{
				var package = Package;
				return package != null && !package.IsContainer;
			}
		}

		public override bool ReadOnly
		{
			// view mode is tested once in DtbBookingConsolidationTest.TestReadOnlyForChildren()
			get { return ConsolidationViewModeService.GetViewMode(Factory) == ConsolidationViewMode.MultiJob || base.ReadOnly; }
			set { base.ReadOnly = value; }
		}
	}
}
