using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Customs.Business.AMS
{
	public abstract class AMSConverter
	{
		protected AMSConverter()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public virtual ZString GetPackageType(ZString unitCode)
		{
			ZString result = unitCode;
			switch (unitCode)
			{
				case Constants.PkgUnit.Bag:
					result = AMSConstants.PackageType.Bag;
					break;

				case Constants.PkgUnit.BaleCompressed:
					result = AMSConstants.PackageType.BaleCompressed;
					break;

				case Constants.PkgUnit.BaleUncompressed:
					result = AMSConstants.PackageType.BaleUncompressed;
					break;

				case Constants.PkgUnit.Bundle:
					result = AMSConstants.PackageType.Bundle;
					break;

				case Constants.PkgUnit.BulkBag:
					result = AMSConstants.PackageType.BulkBag;
					break;

				case Constants.PkgUnit.Bottle:
					result = AMSConstants.PackageType.Bottle;
					break;

				case Constants.PkgUnit.Box:
					result = AMSConstants.PackageType.Box;
					break;

				case Constants.PkgUnit.Basket:
					result = AMSConstants.PackageType.Basket;
					break;

				case Constants.PkgUnit.Case:
					result = AMSConstants.PackageType.Case;
					break;

				case Constants.PkgUnit.BreakBulk:
					result = AMSConstants.PackageType.ContainerBulkCargo;
					break;

				case Constants.PkgUnit.Container:
					result = AMSConstants.PackageType.Container;
					break;

				case Constants.PkgUnit.Coil:
					result = AMSConstants.PackageType.Coil;
					break;

				case Constants.PkgUnit.Cradle:
					result = AMSConstants.PackageType.Cradle;
					break;

				case Constants.PkgUnit.Crate:
					result = AMSConstants.PackageType.Crate;
					break;

				case Constants.PkgUnit.Carton:
					result = AMSConstants.PackageType.Carton;
					break;

				case Constants.PkgUnit.Cylinder:
					result = AMSConstants.PackageType.Cylinder;
					break;

				case Constants.PkgUnit.Drum:
					result = AMSConstants.PackageType.Drum;
					break;

				case Constants.PkgUnit.Envelope:
					result = AMSConstants.PackageType.Envelope;
					break;

				case Constants.PkgUnit.Keg:
					result = AMSConstants.PackageType.Keg;
					break;

				case Constants.PkgUnit.Pail:
					result = AMSConstants.PackageType.Pail;
					break;

				case Constants.PkgUnit.Piece:
					result = AMSConstants.PackageType.Pieces;
					break;

				case Constants.PkgUnit.Package:
					result = AMSConstants.PackageType.Package;
					break;

				case Constants.PkgUnit.Pallet:
					result = AMSConstants.PackageType.Pallet;
					break;

				case Constants.PkgUnit.Reel:
					result = AMSConstants.PackageType.Reel;
					break;

				case Constants.PkgUnit.Roll:
					result = AMSConstants.PackageType.Roll;
					break;

				case Constants.PkgUnit.Sheet:
					result = AMSConstants.PackageType.Sheet;
					break;

				case Constants.PkgUnit.Skid:
					result = AMSConstants.PackageType.Skid;
					break;

				case Constants.PkgUnit.Spool:
					result = AMSConstants.PackageType.Spool;
					break;

				case Constants.PkgUnit.Tube:
					result = AMSConstants.PackageType.Tube;
					break;

				case Constants.PkgUnit.Unit:
					result = AMSConstants.PackageType.Unit;
					break;
			}
			return result;
		}
	}
}
