using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists
{
	public class MeasurementUQList : CodeDescriptionEnumList<GoodsMeasurementTypeMeasurementUnitQualifer>
	{
		public static class Codes
		{
			public const string animal = "ANI";
			public const string bag = "BAG";
			public const string bales = "BAL";
			public const string bin = "BIN";
			public const string bottles = "BOT";
			public const string box = "BOX";
			public const string bulkBag = "BBG";
			public const string bunch = "BCH";
			public const string cage = "CAG";
			public const string cans = "CAN";
			public const string cartons = "CTN";
			public const string cartonsAndBins = "CAB";
			public const string cases = "CAS";
			public const string container = "CTR";
			public const string crate = "CRT";
			public const string cubicMetres = "CBM";
			public const string doses = "DOS";
			public const string drums = "DRM";
			public const string egg = "EGG";
			public const string embryo = "EMB";
			public const string emptyContainer = "EMC";
			public const string foils = "FOI";
			public const string grams = "GMS";
			public const string item = "ITM";
			public const string jar = "JAR";
			public const string kilograms = "KGM";
			public const string litres = "LTR";
			public const string metricTonnes = "TNE";
			public const string mls = "MLS";
			public const string package = "PKG";
			public const string packet = "PKT";
			public const string pail = "PAI";
			public const string Pallecon = "PLC";
			public const string pallets = "PLT";
			public const string parcel = "PAR";
			public const string piece = "PCE";
			public const string sacks = "SAK";
			public const string stem = "STM";
			public const string straws = "STR";
			public const string tank = "TNK";
			public const string tin = "TIN";
			public const string tray = "TRY";
			public const string unit = "UNT";
			public const string vials = "VIA";
		}

		public static class Descriptions
		{
			public static MultilingualString animal { get { return ResString.GetMultilingualString("MeasurementUQList|animal", "Animals"); } }
			public static MultilingualString bag { get { return ResString.GetMultilingualString("MeasurementUQList|bag", "Bags"); } }
			public static MultilingualString bales { get { return ResString.GetMultilingualString("MeasurementUQList|bales", "Bales"); } }
			public static MultilingualString bin { get { return ResString.GetMultilingualString("MeasurementUQList|bin", "Bins"); } }
			public static MultilingualString bottles { get { return ResString.GetMultilingualString("MeasurementUQList|bottles", "Bottles"); } }
			public static MultilingualString box { get { return ResString.GetMultilingualString("MeasurementUQList|box", "Boxes"); } }
			public static MultilingualString bulkBag { get { return ResString.GetMultilingualString("MeasurementUQList|bulkBag", "Bulk Bags"); } }
			public static MultilingualString bunch { get { return ResString.GetMultilingualString("MeasurementUQList|bunch", "Bunches"); } }
			public static MultilingualString cage { get { return ResString.GetMultilingualString("MeasurementUQList|cage", "Cages"); } }
			public static MultilingualString cans { get { return ResString.GetMultilingualString("MeasurementUQList|cans", "Cans"); } }
			public static MultilingualString cartons { get { return ResString.GetMultilingualString("MeasurementUQList|cartons", "Cartons"); } }
			public static MultilingualString cartonsAndBins { get { return ResString.GetMultilingualString("MeasurementUQList|cartonsAndBins", "Cartons And Bins"); } }
			public static MultilingualString cases { get { return ResString.GetMultilingualString("MeasurementUQList|cases", "Cases"); } }
			public static MultilingualString container { get { return ResString.GetMultilingualString("MeasurementUQList|container", "Containers"); } }
			public static MultilingualString crate { get { return ResString.GetMultilingualString("MeasurementUQList|crate", "Crates"); } }
			public static MultilingualString cubicMetres { get { return ResString.GetMultilingualString("MeasurementUQList|cubicMetres", "Cubic Meters"); } }
			public static MultilingualString doses { get { return ResString.GetMultilingualString("MeasurementUQList|doses", "Doses"); } }
			public static MultilingualString drums { get { return ResString.GetMultilingualString("MeasurementUQList|drums", "Drums"); } }
			public static MultilingualString egg { get { return ResString.GetMultilingualString("MeasurementUQList|egg", "Eggs"); } }
			public static MultilingualString embryo { get { return ResString.GetMultilingualString("MeasurementUQList|embryo", "Embryos"); } }
			public static MultilingualString emptyContainer { get { return ResString.GetMultilingualString("MeasurementUQList|emptyContainer", "Empty Containers"); } }
			public static MultilingualString foils { get { return ResString.GetMultilingualString("MeasurementUQList|foils", "Foils"); } }
			public static MultilingualString grams { get { return ResString.GetMultilingualString("MeasurementUQList|grams", "Grams"); } }
			public static MultilingualString item { get { return ResString.GetMultilingualString("MeasurementUQList|item", "Items"); } }
			public static MultilingualString jar { get { return ResString.GetMultilingualString("MeasurementUQList|jar", "Jars"); } }
			public static MultilingualString kilograms { get { return ResString.GetMultilingualString("MeasurementUQList|kilograms", "Kilograms"); } }
			public static MultilingualString litres { get { return ResString.GetMultilingualString("MeasurementUQList|litres", "Liters"); } }
			public static MultilingualString metricTonnes { get { return ResString.GetMultilingualString("MeasurementUQList|metricTonnes", "Metric Tonnes"); } }
			public static MultilingualString mls { get { return ResString.GetMultilingualString("MeasurementUQList|mls", "Mls"); } }
			public static MultilingualString package { get { return ResString.GetMultilingualString("MeasurementUQList|package", "Packages"); } }
			public static MultilingualString packet { get { return ResString.GetMultilingualString("MeasurementUQList|packet", "Packets"); } }
			public static MultilingualString pail { get { return ResString.GetMultilingualString("MeasurementUQList|pail", "Pails"); } }
			public static MultilingualString Pallecon { get { return ResString.GetMultilingualString("MeasurementUQList|Pallecon", "Pallecons"); } }
			public static MultilingualString pallets { get { return ResString.GetMultilingualString("MeasurementUQList|pallets", "Pallets"); } }
			public static MultilingualString parcel { get { return ResString.GetMultilingualString("MeasurementUQList|parcel", "Parcels"); } }
			public static MultilingualString piece { get { return ResString.GetMultilingualString("MeasurementUQList|piece", "Pieces"); } }
			public static MultilingualString sacks { get { return ResString.GetMultilingualString("MeasurementUQList|sacks", "Sacks"); } }
			public static MultilingualString stem { get { return ResString.GetMultilingualString("MeasurementUQList|stem", "Stems"); } }
			public static MultilingualString straws { get { return ResString.GetMultilingualString("MeasurementUQList|straws", "Straws"); } }
			public static MultilingualString tank { get { return ResString.GetMultilingualString("MeasurementUQList|tank", "Tanks"); } }
			public static MultilingualString tin { get { return ResString.GetMultilingualString("MeasurementUQList|tin", "Tins"); } }
			public static MultilingualString tray { get { return ResString.GetMultilingualString("MeasurementUQList|tray", "Trays"); } }
			public static MultilingualString unit { get { return ResString.GetMultilingualString("MeasurementUQList|unit", "Units"); } }
			public static MultilingualString vials { get { return ResString.GetMultilingualString("MeasurementUQList|vials", "Vials"); } }
		}

		public MeasurementUQList()
		{
			AddPair(Codes.animal, Descriptions.animal, GoodsMeasurementTypeMeasurementUnitQualifer.animal);
			AddPair(Codes.bag, Descriptions.bag, GoodsMeasurementTypeMeasurementUnitQualifer.bag);
			AddPair(Codes.bales, Descriptions.bales, GoodsMeasurementTypeMeasurementUnitQualifer.bales);
			AddPair(Codes.bin, Descriptions.bin, GoodsMeasurementTypeMeasurementUnitQualifer.bin);
			AddPair(Codes.bottles, Descriptions.bottles, GoodsMeasurementTypeMeasurementUnitQualifer.bottles);
			AddPair(Codes.box, Descriptions.box, GoodsMeasurementTypeMeasurementUnitQualifer.box);
			AddPair(Codes.bulkBag, Descriptions.bulkBag, GoodsMeasurementTypeMeasurementUnitQualifer.bulkBag);
			AddPair(Codes.bunch, Descriptions.bunch, GoodsMeasurementTypeMeasurementUnitQualifer.bunch);
			AddPair(Codes.cage, Descriptions.cage, GoodsMeasurementTypeMeasurementUnitQualifer.cage);
			AddPair(Codes.cans, Descriptions.cans, GoodsMeasurementTypeMeasurementUnitQualifer.cans);
			AddPair(Codes.cartons, Descriptions.cartons, GoodsMeasurementTypeMeasurementUnitQualifer.cartons);
			AddPair(Codes.cartonsAndBins, Descriptions.cartonsAndBins, GoodsMeasurementTypeMeasurementUnitQualifer.cartonsAndBins);
			AddPair(Codes.cases, Descriptions.cases, GoodsMeasurementTypeMeasurementUnitQualifer.cases);
			AddPair(Codes.container, Descriptions.container, GoodsMeasurementTypeMeasurementUnitQualifer.container);
			AddPair(Codes.crate, Descriptions.crate, GoodsMeasurementTypeMeasurementUnitQualifer.crate);
			AddPair(Codes.cubicMetres, Descriptions.cubicMetres, GoodsMeasurementTypeMeasurementUnitQualifer.cubicMetres);
			AddPair(Codes.doses, Descriptions.doses, GoodsMeasurementTypeMeasurementUnitQualifer.doses);
			AddPair(Codes.drums, Descriptions.drums, GoodsMeasurementTypeMeasurementUnitQualifer.drums);
			AddPair(Codes.egg, Descriptions.egg, GoodsMeasurementTypeMeasurementUnitQualifer.egg);
			AddPair(Codes.embryo, Descriptions.embryo, GoodsMeasurementTypeMeasurementUnitQualifer.embryo);
			AddPair(Codes.emptyContainer, Descriptions.emptyContainer, GoodsMeasurementTypeMeasurementUnitQualifer.emptyContainer);
			AddPair(Codes.foils, Descriptions.foils, GoodsMeasurementTypeMeasurementUnitQualifer.foils);
			AddPair(Codes.grams, Descriptions.grams, GoodsMeasurementTypeMeasurementUnitQualifer.grams);
			AddPair(Codes.item, Descriptions.item, GoodsMeasurementTypeMeasurementUnitQualifer.item);
			AddPair(Codes.jar, Descriptions.jar, GoodsMeasurementTypeMeasurementUnitQualifer.jar);
			AddPair(Codes.kilograms, Descriptions.kilograms, GoodsMeasurementTypeMeasurementUnitQualifer.kilograms);
			AddPair(Codes.litres, Descriptions.litres, GoodsMeasurementTypeMeasurementUnitQualifer.litres);
			AddPair(Codes.metricTonnes, Descriptions.metricTonnes, GoodsMeasurementTypeMeasurementUnitQualifer.metricTonnes);
			AddPair(Codes.mls, Descriptions.mls, GoodsMeasurementTypeMeasurementUnitQualifer.mls);
			AddPair(Codes.package, Descriptions.package, GoodsMeasurementTypeMeasurementUnitQualifer.package);
			AddPair(Codes.packet, Descriptions.packet, GoodsMeasurementTypeMeasurementUnitQualifer.packet);
			AddPair(Codes.pail, Descriptions.pail, GoodsMeasurementTypeMeasurementUnitQualifer.pail);
			AddPair(Codes.Pallecon, Descriptions.Pallecon, GoodsMeasurementTypeMeasurementUnitQualifer.Pallecon);
			AddPair(Codes.pallets, Descriptions.pallets, GoodsMeasurementTypeMeasurementUnitQualifer.pallets);
			AddPair(Codes.parcel, Descriptions.parcel, GoodsMeasurementTypeMeasurementUnitQualifer.parcel);
			AddPair(Codes.piece, Descriptions.piece, GoodsMeasurementTypeMeasurementUnitQualifer.piece);
			AddPair(Codes.sacks, Descriptions.sacks, GoodsMeasurementTypeMeasurementUnitQualifer.sacks);
			AddPair(Codes.stem, Descriptions.stem, GoodsMeasurementTypeMeasurementUnitQualifer.stem);
			AddPair(Codes.straws, Descriptions.straws, GoodsMeasurementTypeMeasurementUnitQualifer.straws);
			AddPair(Codes.tank, Descriptions.tank, GoodsMeasurementTypeMeasurementUnitQualifer.tank);
			AddPair(Codes.tin, Descriptions.tin, GoodsMeasurementTypeMeasurementUnitQualifer.tin);
			AddPair(Codes.tray, Descriptions.tray, GoodsMeasurementTypeMeasurementUnitQualifer.tray);
			AddPair(Codes.unit, Descriptions.unit, GoodsMeasurementTypeMeasurementUnitQualifer.unit);
			AddPair(Codes.vials, Descriptions.vials, GoodsMeasurementTypeMeasurementUnitQualifer.vials);
		}

		public static string TranslateFromCustomsPackageTypeCode(string customsCode)
		{
			switch (customsCode)
			{
				case PackageTypeList.Codes.BaleCompressed:
				case PackageTypeList.Codes.BaleNonCompressed:
					return Codes.bales;
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
					return Codes.bag;
				case PackageTypeList.Codes.Bin:
					return Codes.bin;
				case PackageTypeList.Codes.BottleNonProtBulbous:
				case PackageTypeList.Codes.BottleNonProtCylindrical:
				case PackageTypeList.Codes.BottleProtectedBulbous:
				case PackageTypeList.Codes.BottleProtectedCylindrical:
					return Codes.bottles;
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
					return Codes.box;
				case PackageTypeList.Codes.Bunch:
					return Codes.bunch;
				case PackageTypeList.Codes.Cage:
				case PackageTypeList.Codes.CageChep:
				case PackageTypeList.Codes.CageRoll:
					return Codes.cage;
				case PackageTypeList.Codes.CanCylindrical:
				case PackageTypeList.Codes.CanRectangular:
				case PackageTypeList.Codes.CanWithHandleAndSpout:
					return Codes.cans;
				case PackageTypeList.Codes.Carton:
					return Codes.cartons;
				case PackageTypeList.Codes.Case:
				case PackageTypeList.Codes.CaseIsothermic:
				case PackageTypeList.Codes.CasePalletBaseCardboard:
				case PackageTypeList.Codes.CasePalletBasePlastic:
				case PackageTypeList.Codes.CaseSkeleton:
				case PackageTypeList.Codes.CaseSteel:
					return Codes.cases;
				case PackageTypeList.Codes.ContainerCN:
				case PackageTypeList.Codes.Container69:
					return Codes.container;
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
					return Codes.crate;
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
					return Codes.drums;
				case PackageTypeList.Codes.Jar:
					return Codes.jar;
				case PackageTypeList.Codes.Package:
					return Codes.package;
				case PackageTypeList.Codes.Packet:
					return Codes.packet;
				case PackageTypeList.Codes.Pail:
					return Codes.pail;
				case PackageTypeList.Codes.Pallet07:
				case PackageTypeList.Codes.Pallet100cmsX110cms:
				case PackageTypeList.Codes.PalletBox:
				case PackageTypeList.Codes.PalletModularCollar80x100:
				case PackageTypeList.Codes.PalletModularCollar80x120:
				case PackageTypeList.Codes.PalletModularCollars80x60:
				case PackageTypeList.Codes.PalletPX:
				case PackageTypeList.Codes.PalletShrinkwrapped:
					return Codes.pallets;
				case PackageTypeList.Codes.Parcel:
					return Codes.parcel;
				case PackageTypeList.Codes.Sack:
				case PackageTypeList.Codes.SackMultiWall:
					return Codes.sacks;
				case PackageTypeList.Codes.TankCylindrical:
				case PackageTypeList.Codes.TankRectangular:
					return Codes.tank;
				case PackageTypeList.Codes.Tin:
					return Codes.tin;
				case PackageTypeList.Codes.Tray:
				case PackageTypeList.Codes.TrayOneLayerCardboard:
				case PackageTypeList.Codes.TrayOneLayerPlastic:
				case PackageTypeList.Codes.TrayOneLayerPolystyrene:
				case PackageTypeList.Codes.TrayOneLayerWooden:
				case PackageTypeList.Codes.TrayTwoLayersCardboard:
				case PackageTypeList.Codes.TrayTwoLayersPlasticTray:
				case PackageTypeList.Codes.TrayTwoLayersWooden:
					return Codes.tray;
				case PackageTypeList.Codes.Unit:
					return Codes.unit;
				case PackageTypeList.Codes.Vial:
					return Codes.vials;
			}
			return string.Empty;
		}

		public static string TranslateFromCustomsStatisticalUQCode(string customsCode)
		{
			switch (customsCode)
			{
				case StatisticalUQList.Codes.BoneDryUnits:
					return Codes.unit;
				case StatisticalUQList.Codes.CubicMetre:
					return Codes.cubicMetres;
				case StatisticalUQList.Codes.Dozen:
					return string.Empty;
				case StatisticalUQList.Codes.Gigsajoule:
					return string.Empty;
				case StatisticalUQList.Codes.Grams:
					return Codes.grams;
				case StatisticalUQList.Codes.Hank:
					return string.Empty;
				case StatisticalUQList.Codes.Hundred:
					return string.Empty;
				case StatisticalUQList.Codes.HundredBoxes:
					return string.Empty;
				case StatisticalUQList.Codes.Kilograms:
					return Codes.kilograms;
				case StatisticalUQList.Codes.KilogramsOfNamedSubstance:
					return string.Empty;
				case StatisticalUQList.Codes.KilogramsOfPureTobaccoContent:
					return string.Empty;
				case StatisticalUQList.Codes.Litres:
					return Codes.litres;
				case StatisticalUQList.Codes.LitresOfPureAlcohol:
					return string.Empty;
				case StatisticalUQList.Codes.Metre:
					return string.Empty;
				case StatisticalUQList.Codes.Number:
					return Codes.item;
				case StatisticalUQList.Codes.NumberOfCells:
					return string.Empty;
				case StatisticalUQList.Codes.NumberOfPacks:
					return Codes.package;
				case StatisticalUQList.Codes.NumberOfPairs:
					return string.Empty;
				case StatisticalUQList.Codes.NumberOfRolls:
					return string.Empty;
				case StatisticalUQList.Codes.SquareMetre:
					return string.Empty;
				case StatisticalUQList.Codes.Thousand:
					return string.Empty;
				case StatisticalUQList.Codes.Tonnes:
					return Codes.metricTonnes;
			}
			return null;
		}
	}
}
