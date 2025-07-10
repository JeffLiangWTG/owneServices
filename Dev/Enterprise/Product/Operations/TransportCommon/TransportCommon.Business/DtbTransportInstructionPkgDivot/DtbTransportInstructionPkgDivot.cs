using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business.Common;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportCommon.Business
{
	public abstract class DtbTransportInstructionPkgDivot : AutoDtbBookingInstructionPkgDivot
	{
		protected DtbTransportInstructionPkgDivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region Instruction

		public DtbTransportInstruction Instruction
		{
			get { return (DtbTransportInstruction)Factory.Load(InstructionType, KD_KN_BookingInstruction); }
		}

		protected abstract Type InstructionType { get; }

		#endregion

		#region Package

		public PkgPackage Package
		{
			get { return Factory.Load<PkgPackage>(KD_KP_Package); }
		}

		#endregion

		#region Confirmations

		#region ConfirmationsDivotOnly

		public IDtbTransportConfirmationCollection ConfirmationsDivotOnly
		{
			get { return confirmationsDivotOnly ?? (confirmationsDivotOnly = GetNewConfirmationsDivotOnlyCollection()); }
		}

		protected abstract IDtbTransportConfirmationCollection GetNewConfirmationsDivotOnlyCollection();

		IDtbTransportConfirmationCollection confirmationsDivotOnly;

		#endregion

		#region Confirmations

		public IDtbTransportConfirmationCollection Confirmations
		{
			get { return confirmations ?? (confirmations = GetNewConfirmationsCollection()); }
		}

		protected abstract IDtbTransportConfirmationCollection GetNewConfirmationsCollection();

		IDtbTransportConfirmationCollection confirmations;

		#endregion

		#endregion

		#endregion

		#region Properties

		#region KD_KN_BookingInstruction

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

						if (!KD_KP_Package.IsEmpty)
						{
							// tested by Instruction.TestSetDropModeFromInstructionAddress
							instruction.DefaultDropMode();
						}
					}
				}
			}
		}

		protected virtual void OnDivotAssignedToInstruction()
		{
		}

		#endregion

		#region KD_KP_Package

		[RelatedBusinessObject("Package")]
		public override ZGuid KD_KP_Package
		{
			get { return base.KD_KP_Package; }
			set
			{
				OnBeforePackageAssignedToDivot();

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
			}
		}

		protected void SetQuantityFromPackage()
		{
			var package = Package;
			KD_Quantity = (package != null) ? package.KP_PackageQty : ZInt.Zero;
		}

		protected virtual void OnBeforePackageAssignedToDivot()
		{
		}

		protected virtual void OnPackageAssignedOrUnassignedToDivot()
		{
		}

		#endregion

		#region IsContainerised

		public bool IsContainerised { get { return Package?.IsContainer ?? false; } }

		#endregion

		#region Weight

		[ResourceStringData("DtbTransportInstructionPkgDivot|Weight", Caption = "Weight")]
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

		#endregion

		#region Volume

		[ResourceStringData("DtbTransportInstructionPkgDivot|Volume", Caption = "Volume")]
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

		#endregion

		#region WeightUQ

		public ZString WeightUQ
		{
			get
			{
				return Package?.KP_WeightUQ ?? "";
			}
		}

		#endregion

		#region VolumeUQ

		public ZString VolumeUQ
		{
			get
			{
				return Package?.KP_VolumeUQ ?? "";
			}
		}

		#endregion

		#region Proportion

		ZDecimal Proportion
		{
			get
			{
				var package = Package;
				var proportion = package != null && package.KP_PackageQty > 0 ? (ZDecimal)KD_Quantity / package.KP_PackageQty : 0m;
				return Math.Min(proportion, 1m);
			}
		}

		#endregion

		#region PackType

		public ZString PackType
		{
			get { return Package?.KP_F3_NKPackType ?? ""; }
		}

		#endregion

		#region PackageID

		public ZString PackageID
		{
			get { return Package?.KP_PackageID ?? ""; }
		}

		#endregion

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DtbTransportInstructionPkgDivotFetchStrategy(this);
		}

		#endregion

		#region Lookups

		public new DtbTransportInstructionPkgDivotLookups Lookups
		{
			get { return (DtbTransportInstructionPkgDivotLookups)base.Lookups; }
		}

		protected sealed override DtbBookingInstructionPkgDivotLookups GetNewLookups()
		{
			return GetNewLookupsCore();
		}

		protected abstract DtbTransportInstructionPkgDivotLookups GetNewLookupsCore();

		#endregion

		#region Validation

		public new DtbTransportInstructionPkgDivotValidation Validation
		{
			get { return (DtbTransportInstructionPkgDivotValidation)base.Validation; }
		}

		protected sealed override DtbBookingInstructionPkgDivotValidation GetNewValidation()
		{
			return GetNewValidationCore();
		}

		protected abstract DtbTransportInstructionPkgDivotValidation GetNewValidationCore();

		#endregion

		#region Delete

		// tested by Instruction.TestSetDropModeFromInstructionAddress
		public override void Delete()
		{
			DeleteCore();

			var instruction = !IsDeleted ? Instruction : null;

			ConfirmationsDivotOnly.DeleteAll();

			base.Delete();

			if (instruction != null)
			{
				instruction.DefaultDropMode();
			}
		}

		protected virtual void DeleteCore()
		{
		}

		#endregion
	}
}
