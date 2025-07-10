using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;

namespace Enterprise.TransportBookings.Business
{
	public sealed class DtbBookingPackage_PackageView : NonPersistentBusinessObject
	{
		public DtbBookingPackage_PackageView(PkgPackage package, DtbBooking booking)
			: base(package.Factory)
		{
			this.package = package;
			this.booking = booking;
		}

		/// <summary>
		/// Collection requires a new Package_PackageView for binding, so just return an empty instance.
		/// </summary>
		public DtbBookingPackage_PackageView(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[ChildEditable]
		public ConfirmationCollection_PackageView Confirmations_View
		{
			get
			{
				if (confirmations_View == null)
				{
					confirmations_View = new ConfirmationCollection_PackageView(this);
					confirmations_View.Initialise();
					RegisterEditableChildObject(confirmations_View);
				}

				return confirmations_View;
			}
		}

		ConfirmationCollection_PackageView confirmations_View;

		public PkgPackage Package
		{
			get { return package; }
		}

		readonly PkgPackage package;

		public DtbBooking Booking
		{
			get { return booking; }
		}

		readonly DtbBooking booking;

		[ChildEditable]
		public DtbBookingInstructionPkgDivotCollection InstructionDivots
		{
			get
			{
				if (instructionDivots == null)
				{
					instructionDivots = GetInstructionDivots();
					RegisterEditableChildObject(instructionDivots);
				}
				return instructionDivots;
			}
		}

		DtbBookingInstructionPkgDivotCollection GetInstructionDivots()
		{
			return new DtbBookingInstructionPkgDivotCollection(this);
		}

		DtbBookingInstructionPkgDivotCollection instructionDivots;

		[ChildEditable]
		public DtbBookingConfirmationCollection Confirmations
		{
			get
			{
				if (confirmations == null)
				{
					confirmations = GetConfirmations();
					RegisterEditableChildObject(confirmations);
				}
				return confirmations;
			}
		}

		DtbBookingConfirmationCollection GetConfirmations()
		{
			return new DtbBookingConfirmationCollection(this);
		}

		DtbBookingConfirmationCollection confirmations;

		[ResourceStringData("DtbBookingPackageDescription|PackageDescription", Caption = "Description")]
		public ZString PackageDescription => Package?.ToStringPackageSummary() ?? ZString.Empty;

		public ZInt QuantityFromInstructions
		{
			get
			{
				var picTotal = 0;
				var dlvTotal = 0;

				foreach (DtbBookingInstructionPkgDivot divot in InstructionDivots)
				{
					// we don't care about MLT ... as these would be the middle instructions and the totals should either come from the pickups or deliveries
					if (divot?.Instruction?.KN_InstructionType is ZString instructionType && (instructionType == InstructionTypes.Codes.PickUp || instructionType == InstructionTypes.Codes.Delivery))
					{
						switch (instructionType)
						{
							case InstructionTypes.Codes.PickUp:
								picTotal += divot.KD_Quantity;
								break;

							case InstructionTypes.Codes.Delivery:
								dlvTotal += divot.KD_Quantity;
								break;
						}
					}
				}
				return Math.Min(Package.KP_PackageQty, Math.Max(picTotal, dlvTotal));
			}
		}

		public ZWeight WeightFromInstructions
		{
			get { return new ZWeight(Package.KP_Weight * QuantityProportionFromInstructions, Package.KP_WeightUQ); }
		}

		public ZVolume VolumeFromInstructions
		{
			get { return new ZVolume(Package.KP_Volume * QuantityProportionFromInstructions, Package.KP_VolumeUQ); }
		}

		ZDecimal QuantityProportionFromInstructions
		{
			get
			{
				var qty = QuantityFromInstructions;
				return Package.KP_PackageQty > 0 ? (ZDecimal)qty / Package.KP_PackageQty : 0m;
			}
		}

		public override void Delete()
		{
			InstructionDivots.DeleteAll();

			base.Delete();
		}

		public override bool IsDeleted
		{
			get { return base.IsDeleted || (Package != null && Package.IsDeleted); }
		}
	}
}
