using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business
{
	public class CO2eLoadAction : ICO2eMatchAction
	{
		public DtbBookingInstructionPkgDivot LoadBy { get; }
		public List<ICO2eMatchAction> InnerPackagesActions { get; }
		public PkgPackage Package { get; }
		public bool IsPickup => true;
		public bool IsEmptyContainer { get; }
		public JobDocAddress Address { get; }
		public ZInt Quantity { get; }
		public ZDecimal Weight { get; }
		public ZString WeightUQ { get; }

		public CO2eLoadAction(DtbBookingInstructionPkgDivot loadBy, PkgPackage package, ZInt loadQuantity)
		{
			LoadBy = loadBy;
			InnerPackagesActions = new List<ICO2eMatchAction>();
			Package = package;
			IsEmptyContainer = loadBy.IsEmptyContainer(ConfirmationTypes.Codes.PickUp);
			Address = loadBy.Instruction.Address;
			Quantity = loadQuantity;
			Weight = Quantity * GetSinglePackageLoadWeight(package);
			WeightUQ = loadBy.Package.KP_WeightUQ;
		}

		ZDecimal GetSinglePackageLoadWeight(PkgPackage package)
		{
			if (IsEmptyContainer)
			{
				return package.KP_PackageQty > 0 ? package.ContainerTareWeight / package.KP_PackageQty : 0;
			}
			else
			{
				return package.KP_PackageQty > 0 ? package.KP_Weight / package.KP_PackageQty : 0;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			var other = (CO2eLoadAction)obj;
			return Package.PKEquals(other.Package) && LoadBy.PKEquals(other.LoadBy) && Quantity.Equals(other.Quantity);
		}

		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				var hash = 17;
				hash = hash * 23 + Package.PK.GetHashCode();
				hash = hash * 23 + LoadBy.PK.GetHashCode();
				hash = hash * 23 + Quantity.GetHashCode();
				return hash;
			}
		}

		public override string ToString() => $"Loaded '{Quantity}: {Weight}' of '{Package.PK}' by '{LoadBy.PK}'";
	}
}
