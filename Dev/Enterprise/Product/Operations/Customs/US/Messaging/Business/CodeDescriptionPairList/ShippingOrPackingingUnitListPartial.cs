using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Messaging.Business
{
	partial class ShippingOrPackingingUnitList : CodeDescriptionPairList, Integration.Customs.US.IShippingOrPackingingUnitList
	{
		public const string PiecesCode = "PCS";
		public const string PiecesDescription = "Pieces";

		public static CodeDescriptionPairList GetWithPieceType(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<CodeDescriptionPairList>("ShippingOrPackingingUnitListWithPieceType", delegate
			{
				var result = new ShippingOrPackingingUnitList();
				result.AddPair(PiecesCode, PiecesDescription);
				result.Sort();
				return result;
			});
		}

		public static ZString FromFreightPackageType(ZString freightPackageType)
		{
			switch (freightPackageType)
			{
				case Constants.PkgUnit.Bag:
					return Codes.Bag;
				case Constants.PkgUnit.BaleCompressed:
					return Codes.BaleCompressed;
				case Constants.PkgUnit.BaleUncompressed:
					return Codes.BaleNonCompressed;
				case Constants.PkgUnit.Basket:
					return Codes.Basket;
				case Constants.PkgUnit.Bottle:
					return Codes.BottleNonProtectedCylindrical;
				case Constants.PkgUnit.Box:
					return Codes.Box;
				case Constants.PkgUnit.Bundle:
					return Codes.Bundle;
				case Constants.PkgUnit.Carton:
					return Codes.Carton;
				case Constants.PkgUnit.Case:
					return Codes.Case;
				case Constants.PkgUnit.Coil:
					return Codes.Coil;
				case Constants.PkgUnit.Container:
					return Codes.Container;
				case Constants.PkgUnit.Crate:
					return Codes.Crate;
				case Constants.PkgUnit.Cylinder:
					return Codes.Cylinder;
				case Constants.PkgUnit.Drum:
					return Codes.Drum;
				case Constants.PkgUnit.Envelope:
					return Codes.Envelope;
				case Constants.PkgUnit.Keg:
					return Codes.Keg;
				case Constants.PkgUnit.Package:
					return Codes.Package;
				case Constants.PkgUnit.Pail:
					return Codes.Pail;
				case Constants.PkgUnit.Pallet:
					return Codes.Pallet;
				case Constants.PkgUnit.Piece:
					return PiecesCode;
				case Constants.PkgUnit.Reel:
					return Codes.Reel;
				case Constants.PkgUnit.Roll:
					return Codes.Roll;
				case Constants.PkgUnit.Sheet:
					return Codes.Sheet;
				case Constants.PkgUnit.Tube:
					return Codes.Tube;
				case Constants.PkgUnit.Unit:
					return Codes.Package;
				default:
					return freightPackageType;
			}
		}

		public static ZString ToFreightPackageType(ZString customsPackageType)
		{
			switch (customsPackageType)
			{
				case Codes.Bag:
					return Constants.PkgUnit.Bag;
				case Codes.BaleCompressed:
					return Constants.PkgUnit.BaleCompressed;
				case Codes.BaleNonCompressed:
					return Constants.PkgUnit.BaleUncompressed;
				case Codes.Barrel:
					return Constants.PkgUnit.Drum;
				case Codes.Basket:
					return Constants.PkgUnit.Basket;
				case Codes.BeerCrate:
					return Constants.PkgUnit.Crate;
				case Codes.BottleCrateBottleRack:
				case Codes.BottleNonProtectedBulbous:
				case Codes.BottleNonProtectedCylindrical:
				case Codes.BottleProtectedBulbous:
				case Codes.BottleProtectedCylindrical:
					return Constants.PkgUnit.Bottle;
				case Codes.Box:
					return Constants.PkgUnit.Box;
				case Codes.Bundle:
					return Constants.PkgUnit.Bundle;
				case Codes.CanCylindrical:
					return Constants.PkgUnit.Cylinder;
				case Codes.Carton:
					return Constants.PkgUnit.Carton;
				case Codes.Case:
					return Constants.PkgUnit.Case;
				case Codes.Container:
					return Constants.PkgUnit.Container;
				case Codes.Crate:
					return Constants.PkgUnit.Crate;
				case Codes.Cylinder:
					return Constants.PkgUnit.Cylinder;
				case Codes.Drum:
					return Constants.PkgUnit.Drum;
				case Codes.Envelope:
					return Constants.PkgUnit.Envelope;
				case Codes.FramedCrate:
				case Codes.FruitCrate:
					return Constants.PkgUnit.Crate;
				case Codes.GasBottle:
					return Constants.PkgUnit.Bottle;
				case Codes.JerricanCylindrical:
					return Constants.PkgUnit.Cylinder;
				case Codes.Keg:
					return Constants.PkgUnit.Keg;
				case Codes.Matchbox:
					return Constants.PkgUnit.Box;
				case Codes.MilkCrate:
					return Constants.PkgUnit.Crate;
				case Codes.MultiplyBag:
					return Constants.PkgUnit.Bag;
				case Codes.Package:
				case Codes.Packet:
					return Constants.PkgUnit.Package;
				case Codes.Pail:
					return Constants.PkgUnit.Pail;
				case Codes.Pallet:
					return Constants.PkgUnit.Pallet;
				case PiecesCode:
					return Constants.PkgUnit.Piece;
				case Codes.Reel:
					return Constants.PkgUnit.Reel;
				case Codes.Roll:
					return Constants.PkgUnit.Roll;
				case Codes.ShallowCrate:
					return Constants.PkgUnit.Crate;
				case Codes.Sheet:
				case Codes.Sheetmetal:
				case Codes.SheetsInBundleBunchTruss:
					return Constants.PkgUnit.Sheet;
				case Codes.SkeletonCase:
					return Constants.PkgUnit.Case;
				case Codes.Slipsheet:
					return Constants.PkgUnit.Sheet;
				case Codes.Suitcase:
					return Constants.PkgUnit.Case;
				case Codes.TankCylindrical:
					return Constants.PkgUnit.Cylinder;
				case Codes.Tube:
				case Codes.TubeCollapsible:
				case Codes.TubesInBundleBunchTruss:
					return Constants.PkgUnit.Tube;
				case Codes.WickerBottle:
					return Constants.PkgUnit.Bottle;
				default:
					return customsPackageType;
			}
		}
	}
}
