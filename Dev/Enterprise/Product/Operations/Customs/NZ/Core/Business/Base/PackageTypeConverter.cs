using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.NZ.Business
{
	public static class PackageTypeConverter
	{
		public static ZString GetCustomsPackageType(ZString freightPackageType)
		{
			ZString result = freightPackageType;

			switch (freightPackageType)
			{
				case Constants.PkgUnit.Bag:
					result = PackageTypeList.Codes.Bag;
					break;
				case Constants.PkgUnit.BaleCompressed:
					result = PackageTypeList.Codes.BaleCompressed;
					break;
				case Constants.PkgUnit.BaleUncompressed:
					result = PackageTypeList.Codes.BaleNonCompressed;
					break;
				case Constants.PkgUnit.Bundle:
					result = PackageTypeList.Codes.Bundle;
					break;
				case Constants.PkgUnit.Box:
					result = PackageTypeList.Codes.Box;
					break;
				case Constants.PkgUnit.Basket:
					result = PackageTypeList.Codes.Basket;
					break;
				case Constants.PkgUnit.BulkBag:
					result = PackageTypeList.Codes.BagSuperBulk;
					break;
				case Constants.PkgUnit.Bottle:
					result = PackageTypeList.Codes.BottleNonProtCylindrical;
					break;
				case Constants.PkgUnit.Case:
					result = PackageTypeList.Codes.Case;
					break;
				case Constants.PkgUnit.Container:
					result = PackageTypeList.Codes.ContainerCN;
					break;
				case Constants.PkgUnit.Coil:
					result = PackageTypeList.Codes.Coil;
					break;
				case Constants.PkgUnit.Crate:
					result = PackageTypeList.Codes.Crate;
					break;
				case Constants.PkgUnit.Carton:
					result = PackageTypeList.Codes.Carton;
					break;
				case Constants.PkgUnit.Cylinder:
					result = PackageTypeList.Codes.Cylinder;
					break;
				case Constants.PkgUnit.Drum:
					result = PackageTypeList.Codes.Drum;
					break;
				case Constants.PkgUnit.Envelope:
					result = PackageTypeList.Codes.Envelope;
					break;
				case Constants.PkgUnit.Keg:
					result = PackageTypeList.Codes.Keg;
					break;
				case Constants.PkgUnit.Pail:
					result = PackageTypeList.Codes.Pail;
					break;
				case Constants.PkgUnit.Package:
					result = PackageTypeList.Codes.Package;
					break;
				case Constants.PkgUnit.Pallet:
					result = PackageTypeList.Codes.PalletPX;
					break;
				case Constants.PkgUnit.Piece:
					result = PackageTypeList.Codes.Package;
					break;
				case Constants.PkgUnit.Reel:
					result = PackageTypeList.Codes.Reel;
					break;
				case Constants.PkgUnit.Roll:
					result = PackageTypeList.Codes.Roll;
					break;
				case Constants.PkgUnit.Sheet:
					result = PackageTypeList.Codes.Sheet;
					break;
				case Constants.PkgUnit.Skid:
					result = PackageTypeList.Codes.Skid;
					break;
				case Constants.PkgUnit.Spool:
					result = PackageTypeList.Codes.Spool;
					break;
				case Constants.PkgUnit.Tote:
					result = PackageTypeList.Codes.Bag;
					break;
				case Constants.PkgUnit.Tube:
					result = PackageTypeList.Codes.Tube;
					break;
				case Constants.PkgUnit.Unit:
					result = PackageTypeList.Codes.Unit;
					break;
			}
			return result;
		}

		public static ZString GetFreightPackageType(ZString customsPackageType)
		{
			ZString result = customsPackageType;

			switch (customsPackageType)
			{
				case PackageTypeList.Codes.Bag:
				case PackageTypeList.Codes.BagFlexibleContainer:
				case PackageTypeList.Codes.BagLarge:
				case PackageTypeList.Codes.BagMultiply:
				case PackageTypeList.Codes.BagPaper:
				case PackageTypeList.Codes.BagPaperMultiWall:
				case PackageTypeList.Codes.BagPaperMultiwallWaterRes:
				case PackageTypeList.Codes.BagPlastic:
				case PackageTypeList.Codes.BagPlasticsFilm:
				case PackageTypeList.Codes.BagSuperBulk:
				case PackageTypeList.Codes.BagTextile:
				case PackageTypeList.Codes.BagTextileSiftProof:
				case PackageTypeList.Codes.BagTextileWaterResistant:
				case PackageTypeList.Codes.BagTextileWOCoatLiner:
				case PackageTypeList.Codes.BagWovenPlastic:
				case PackageTypeList.Codes.BagWovenPlasticSiftProof:
				case PackageTypeList.Codes.BagWovenPlasticWaterRes:
				case PackageTypeList.Codes.BagWovenPlasticWoCoatline:
					result = Constants.PkgUnit.Bag;
					break;
				case PackageTypeList.Codes.BaleCompressed:
					result = Constants.PkgUnit.BaleCompressed;
					break;
				case PackageTypeList.Codes.BaleNonCompressed:
					result = Constants.PkgUnit.BaleUncompressed;
					break;
				case PackageTypeList.Codes.Bundle:
					result = Constants.PkgUnit.Bundle;
					break;
				case PackageTypeList.Codes.Box:
				case PackageTypeList.Codes.BoxAluminium:
				case PackageTypeList.Codes.BoxChepEurobox:
				case PackageTypeList.Codes.BoxFibreboard:
				case PackageTypeList.Codes.BoxForLiquids:
				case PackageTypeList.Codes.BoxNaturalWood:
				case PackageTypeList.Codes.BoxNaturalWoodOrdinary:
				case PackageTypeList.Codes.BoxNaturalWoodSiftProof:
				case PackageTypeList.Codes.BoxPlastic:
				case PackageTypeList.Codes.BoxPlasticExpanded:
				case PackageTypeList.Codes.BoxPlasticSolid:
				case PackageTypeList.Codes.BoxPlywood:
				case PackageTypeList.Codes.BoxReconstitutedWood:
				case PackageTypeList.Codes.BoxSteel:
					result = Constants.PkgUnit.Box;
					break;
				case PackageTypeList.Codes.Basket:
				case PackageTypeList.Codes.BasketWithHandleCardboard:
				case PackageTypeList.Codes.BasketWithHandlePlastic:
				case PackageTypeList.Codes.BasketWithHandleWooden:
					result = Constants.PkgUnit.Basket;
					break;
				case PackageTypeList.Codes.Case:
				case PackageTypeList.Codes.CaseIsothermic:
				case PackageTypeList.Codes.CasePalletBaseCardboard:
				case PackageTypeList.Codes.CasePalletBasePlastic:
				case PackageTypeList.Codes.CaseSkeleton:
				case PackageTypeList.Codes.CaseSteel:
				case PackageTypeList.Codes.CaseWithPalletBase:
				case PackageTypeList.Codes.CaseWithPalletBaseMetal:
				case PackageTypeList.Codes.CaseWithPalletBaseWooden:
					result = Constants.PkgUnit.Case;
					break;
				case PackageTypeList.Codes.Container69:
				case PackageTypeList.Codes.ContainerCN:
					result = Constants.PkgUnit.Container;
					break;
				case PackageTypeList.Codes.Coil:
					result = Constants.PkgUnit.Coil;
					break;
				case PackageTypeList.Codes.Crate:
				case PackageTypeList.Codes.CrateBeer:
				case PackageTypeList.Codes.CrateBulkCardboard:
				case PackageTypeList.Codes.CrateBulkPlastic:
				case PackageTypeList.Codes.CrateBulkWooden:
				case PackageTypeList.Codes.CrateFramed:
				case PackageTypeList.Codes.CrateFruit:
				case PackageTypeList.Codes.CrateMilk:
				case PackageTypeList.Codes.CrateMultiLayerCardboard:
				case PackageTypeList.Codes.CrateMultiLayerPlastic:
				case PackageTypeList.Codes.CrateMultiLayerWooden:
				case PackageTypeList.Codes.CrateShallow:
					result = Constants.PkgUnit.Crate;
					break;
				case PackageTypeList.Codes.Carton:
					result = Constants.PkgUnit.Carton;
					break;
				case PackageTypeList.Codes.Cylinder:
					result = Constants.PkgUnit.Cylinder;
					break;
				case PackageTypeList.Codes.Drum:
				case PackageTypeList.Codes.DrumAluminium:
				case PackageTypeList.Codes.DrumAluminiumNonRemHead:
				case PackageTypeList.Codes.DrumAluminiumRemHead:
				case PackageTypeList.Codes.DrumFibre:
				case PackageTypeList.Codes.DrumIron:
				case PackageTypeList.Codes.DrumPlastic:
				case PackageTypeList.Codes.DrumPlasticNonRemHead:
				case PackageTypeList.Codes.DrumPlasticRemovableHead:
				case PackageTypeList.Codes.DrumPlywood:
				case PackageTypeList.Codes.DrumSteel:
				case PackageTypeList.Codes.DrumSteelNonRemHead:
				case PackageTypeList.Codes.DrumSteelRemovableHead:
				case PackageTypeList.Codes.DrumWooden:
					result = Constants.PkgUnit.Drum;
					break;
				case PackageTypeList.Codes.Envelope:
				case PackageTypeList.Codes.EnvelopeSteel:
					result = Constants.PkgUnit.Envelope;
					break;
				case PackageTypeList.Codes.Keg:
					result = Constants.PkgUnit.Keg;
					break;
				case PackageTypeList.Codes.Pail:
					result = Constants.PkgUnit.Pail;
					break;
				case PackageTypeList.Codes.Package:
				case PackageTypeList.Codes.PackageCdbdBottleGripholes:
				case PackageTypeList.Codes.PackageDisplayCardboard:
				case PackageTypeList.Codes.PackageDisplayMetal:
				case PackageTypeList.Codes.PackageDisplayPlastic:
				case PackageTypeList.Codes.PackageDisplayWooden:
				case PackageTypeList.Codes.PackageFlow:
				case PackageTypeList.Codes.PackagePaperWrapped:
				case PackageTypeList.Codes.PackageShow:
				case PackageTypeList.Codes.Packet:
					result = Constants.PkgUnit.Package;
					break;
				case PackageTypeList.Codes.Pallet07:
				case PackageTypeList.Codes.Pallet100cmsX110cms:
				case PackageTypeList.Codes.PalletBox:
				case PackageTypeList.Codes.PalletModularCollar80x100:
				case PackageTypeList.Codes.PalletModularCollar80x120:
				case PackageTypeList.Codes.PalletModularCollars80x60:
				case PackageTypeList.Codes.PalletPX:
				case PackageTypeList.Codes.PalletShrinkwrapped:
					result = Constants.PkgUnit.Pallet;
					break;
				case PackageTypeList.Codes.Reel:
					result = Constants.PkgUnit.Reel;
					break;
				case PackageTypeList.Codes.Roll:
					result = Constants.PkgUnit.Roll;
					break;
				case PackageTypeList.Codes.Sheet:
					result = Constants.PkgUnit.Sheet;
					break;
				case PackageTypeList.Codes.Skid:
					result = Constants.PkgUnit.Skid;
					break;
				case PackageTypeList.Codes.Spool:
					result = Constants.PkgUnit.Spool;
					break;
				case PackageTypeList.Codes.Tube:
				case PackageTypeList.Codes.TubeCollapsible:
				case PackageTypeList.Codes.TubesInBundleBunchTruss:
				case PackageTypeList.Codes.TubeWithNozzle:
					result = Constants.PkgUnit.Tube;
					break;
				case PackageTypeList.Codes.Unit:
					result = Constants.PkgUnit.Unit;
					break;
			}
			return result;
		}

		public static ZString GetCustomsTSWPackagingType(BusinessObjectFactory factory, ZString freightPackageType)
		{
			var cache = factory.GetCachedValue("CustomsTSWPackagingTypeListCache", () => CusRefPacksHelper.LoadFilteredRefPacks(factory, CountryCodes.NewZealand, RPTypeList.Codes.CommercialInvoice, ZString.Empty));

			var packType = cache.FirstOrDefault(c => string.Equals(c.RP_CommercialPack, freightPackageType, System.StringComparison.OrdinalIgnoreCase));

			return packType?.RP_CustomsPack ?? string.Empty;
		}
	}
}
