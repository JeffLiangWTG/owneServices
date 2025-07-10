using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;

namespace Enterprise.TransportCommon.Business
{
	public abstract class Package_PackageView : NonPersistentBusinessObject
	{
		protected Package_PackageView(PkgPackage package, DtbTransport transport)
			: base(package.Factory)
		{
			this.package = package;
			this.transport = transport;
		}

		/// <summary>
		/// Collection requires a new Package_PackageView for binding, so just return an empty instance.
		/// </summary>
		protected Package_PackageView(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Related Business Objects

		#region Package

		public PkgPackage Package
		{
			get { return package; }
		}

		readonly PkgPackage package;

		#endregion

		#region Booking

		public DtbTransport Transport
		{
			get { return transport; }
		}

		readonly DtbTransport transport;

		#endregion

		#region InstructionDivots

		[ChildEditable]
		public IDtbTransportInstructionPkgDivotCollection InstructionDivots
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

		protected abstract IDtbTransportInstructionPkgDivotCollection GetInstructionDivots();

		IDtbTransportInstructionPkgDivotCollection instructionDivots;

		#endregion

		#region Confirmations

		[ChildEditable]
		public IDtbTransportConfirmationCollection Confirmations
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

		protected abstract IDtbTransportConfirmationCollection GetConfirmations();

		IDtbTransportConfirmationCollection confirmations;

		#endregion

		#endregion

		#region Properties

		#region PackageDescription

		[ResourceStringData("PackageDescription|PackageDescription", Caption = "Description")]
		public ZString PackageDescription => Package?.ToStringPackageSummary() ?? ZString.Empty;

		#endregion

		#region TotalsFromInstructions

		#region QuantityFromInstructions

		public ZInt QuantityFromInstructions
		{
			get
			{
				var picTotal = 0;
				var dlvTotal = 0;

				foreach (DtbTransportInstructionPkgDivot divot in InstructionDivots)
				{
					// we don't care about MLT ... as these would be the middle instructions and the totals should either come from the pickups or deliveries
					if (divot == null)
					{
						throw new InvalidOperationException("divot should not be null in Get_QuantityFromInstructions");
					}
					var instruction = divot.Instruction
						?? throw new InvalidOperationException("divot.Instruction should not be null in Get_QuantityFromInstructions");

					if (instruction.KN_InstructionType == InstructionTypes.Codes.PickUp)
					{
						picTotal += divot.KD_Quantity;
					}
					else if (instruction.KN_InstructionType == InstructionTypes.Codes.Delivery)
					{
						dlvTotal += divot.KD_Quantity;
					}
				}
				return Math.Min(Package.KP_PackageQty, Math.Max(picTotal, dlvTotal));
			}
		}

		#endregion

		#region WeightFromInstructions

		public ZWeight WeightFromInstructions
		{
			get { return new ZWeight(Package.KP_Weight * QuantityProportionFromInstructions, Package.KP_WeightUQ); }
		}

		#endregion

		#region VolumeFromInstructions

		public ZVolume VolumeFromInstructions
		{
			get { return new ZVolume(Package.KP_Volume * QuantityProportionFromInstructions, Package.KP_VolumeUQ); }
		}

		#endregion

		#region QuantityProportionFromInstructions

		ZDecimal QuantityProportionFromInstructions
		{
			get
			{
				var qty = QuantityFromInstructions;
				return Package.KP_PackageQty > 0 ? (ZDecimal)qty / Package.KP_PackageQty : 0m;
			}
		}

		#endregion

		#endregion

		#endregion

		#region Delete

		public override void Delete()
		{
			InstructionDivots.DeleteAll();

			base.Delete();
		}

		public override bool IsDeleted
		{
			get { return base.IsDeleted || (Package != null && Package.IsDeleted); }
		}

		#endregion
	}
}
