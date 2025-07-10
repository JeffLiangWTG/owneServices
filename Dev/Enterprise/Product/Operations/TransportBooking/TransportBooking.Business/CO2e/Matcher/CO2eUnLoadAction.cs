using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business
{
	public class CO2eUnLoadAction : ICO2eMatchAction
	{
		public DtbBookingInstructionPkgDivot UnLoadBy { get; }
		public DtbBookingInstructionPkgDivot LoadBy { get; }
		public List<ICO2eMatchAction> InnerPackagesActions { get; }
		public PkgPackage Package { get; }
		public bool IsPickup => false;
		public bool IsEmptyContainer { get; }
		public JobDocAddress Address { get; }
		public ZInt Quantity { get; }
		public ZDecimal Weight => CalculateWeight(Package);
		public ZString WeightUQ { get; }

		public CO2eUnLoadAction(DtbBookingInstructionPkgDivot unLoadBy, DtbBookingInstructionPkgDivot loadBy, PkgPackage package, ZInt unLoadQuantity)
		{
			UnLoadBy = unLoadBy;
			LoadBy = loadBy;
			InnerPackagesActions = new List<ICO2eMatchAction>();
			Package = package;
			IsEmptyContainer = loadBy.IsEmptyContainer(ConfirmationTypes.Codes.PickUp) || unLoadBy.IsEmptyContainer(ConfirmationTypes.Codes.Delivery);
			Address = unLoadBy.Instruction.Address;
			Quantity = unLoadQuantity;
			WeightUQ = unLoadBy.Package.KP_WeightUQ;
		}

		ZDecimal CalculateWeight(PkgPackage package)
		{
			if (IsEmptyContainer)
			{
				return Quantity * GetSinglePackageUnLoadWeight(package);
			}
			else
			{
				var allInnerPackages = package.Packages.ToList();
				var includedPackagesForUnLoad = InnerPackagesActions.Select(action => action.Package);

				var packagesAlreadyUnloaded = allInnerPackages.Except(includedPackagesForUnLoad);
				var totalWeight = Quantity * (GetSinglePackageUnLoadWeight(package) - packagesAlreadyUnloaded.Sum(pkg => pkg.KP_PackageQty * GetSinglePackageUnLoadWeight(pkg)));
				return totalWeight;
			}
		}

		ZDecimal GetSinglePackageUnLoadWeight(PkgPackage package)
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

			var other = (CO2eUnLoadAction)obj;
			return Package.PKEquals(other.Package) && UnLoadBy.PKEquals(other.UnLoadBy) && LoadBy.PKEquals(other.LoadBy) && Quantity.Equals(other.Quantity);
		}

		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				var hash = 17;
				hash = hash * 23 + Package.PK.GetHashCode();
				hash = hash * 23 + UnLoadBy.PK.GetHashCode();
				hash = hash * 23 + LoadBy.PK.GetHashCode();
				hash = hash * 23 + Quantity.GetHashCode();
				return hash;
			}
		}

		public override string ToString() => $"UnLoaded '{Quantity}: {Weight}' of '{Package.PK}' by '{UnLoadBy.PK}' from '{LoadBy.PK}'";
	}
}
