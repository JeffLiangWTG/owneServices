using Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists
{
	public class GoodsTypeList : CodeDescriptionEnumList<BACCApplicationTypeDetailsCommodityGoodsType>
	{
		public static class Codes
		{
			public const string AgCompound = "AGC";
			public const string AnimalProducts = "APD";
			public const string Animals = "ANM";
			public const string Biologicals = "BIO";
			public const string CarParts = "CPT";
			public const string Containers = "CTS";
			public const string Equipment = "EQT";
			public const string Fertiliser = "FTL";
			public const string Miscellaneous = "MSC";
			public const string NurseryStock = "NSK";
			public const string PersonalEffects = "PEP";
			public const string PlantProducts = "PPD";
			public const string Produce = "PRD";
			public const string SeedGrain = "SED";
			public const string StoredProducts = "SPD";
			public const string Timber = "TBR";
			public const string Tyres = "TYR";
			public const string Vehicles = "VHC";
		}

		public static class Descriptions
		{
			public const string AgCompound = "Ag Compound";
			public const string AnimalProducts = "Animal Products";
			public const string Animals = "Animals";
			public const string Biologicals = "Biologicals";
			public const string CarParts = "Car Parts";
			public const string Containers = "Containers";
			public const string Equipment = "Equipment";
			public const string Fertiliser = "Fertiliser";
			public const string Miscellaneous = "Miscellaneous";
			public const string NurseryStock = "Nursery Stock";
			public const string PersonalEffects = "Personal Effects";
			public const string PlantProducts = "Plant Products";
			public const string Produce = "Produce";
			public const string SeedGrain = "Seed/Grain";
			public const string StoredProducts = "Stored Products";
			public const string Timber = "Timber";
			public const string Tyres = "Tyres";
			public const string Vehicles = "Vehicles";
		}

		public GoodsTypeList()
		{
			AddPair(Codes.AgCompound, Descriptions.AgCompound, BACCApplicationTypeDetailsCommodityGoodsType.AGC);
			AddPair(Codes.AnimalProducts, Descriptions.AnimalProducts, BACCApplicationTypeDetailsCommodityGoodsType.APD);
			AddPair(Codes.Animals, Descriptions.Animals, BACCApplicationTypeDetailsCommodityGoodsType.ANM);
			AddPair(Codes.Biologicals, Descriptions.Biologicals, BACCApplicationTypeDetailsCommodityGoodsType.BIO);
			AddPair(Codes.CarParts, Descriptions.CarParts, BACCApplicationTypeDetailsCommodityGoodsType.CPT);
			AddPair(Codes.Containers, Descriptions.Containers, BACCApplicationTypeDetailsCommodityGoodsType.CTS);
			AddPair(Codes.Equipment, Descriptions.Equipment, BACCApplicationTypeDetailsCommodityGoodsType.EQT);
			AddPair(Codes.Fertiliser, Descriptions.Fertiliser, BACCApplicationTypeDetailsCommodityGoodsType.FTL);
			AddPair(Codes.Miscellaneous, Descriptions.Miscellaneous, BACCApplicationTypeDetailsCommodityGoodsType.MSC);
			AddPair(Codes.NurseryStock, Descriptions.NurseryStock, BACCApplicationTypeDetailsCommodityGoodsType.NSK);
			AddPair(Codes.PersonalEffects, Descriptions.PersonalEffects, BACCApplicationTypeDetailsCommodityGoodsType.PEP);
			AddPair(Codes.PlantProducts, Descriptions.PlantProducts, BACCApplicationTypeDetailsCommodityGoodsType.PPD);
			AddPair(Codes.Produce, Descriptions.Produce, BACCApplicationTypeDetailsCommodityGoodsType.PRD);
			AddPair(Codes.SeedGrain, Descriptions.SeedGrain, BACCApplicationTypeDetailsCommodityGoodsType.SED);
			AddPair(Codes.StoredProducts, Descriptions.StoredProducts, BACCApplicationTypeDetailsCommodityGoodsType.SPD);
			AddPair(Codes.Timber, Descriptions.Timber, BACCApplicationTypeDetailsCommodityGoodsType.TBR);
			AddPair(Codes.Tyres, Descriptions.Tyres, BACCApplicationTypeDetailsCommodityGoodsType.TYR);
			AddPair(Codes.Vehicles, Descriptions.Vehicles, BACCApplicationTypeDetailsCommodityGoodsType.VHC);
		}

		public static string GetCodeFromTariff(string tariffCode)
		{
			if (tariffCode == null || tariffCode.Length < 2)
			{
				return Codes.Miscellaneous;
			}
			switch (tariffCode.Substring(0, 2))
			{
				case "01": // Live animals
					return Codes.Animals;
				case "02": // Meat and edible meat offal
				case "03": // Fish and crustaceans, molluscs and other aquatic invertebrates
				case "04": // Dairy produce; birds' eggs; natural honey; edible products of animal origin, not elsewhere specified
				case "05": // Products of animal origin, not elsewhere specified or included
					return Codes.AnimalProducts;
				case "06": // Live trees and other plants; bulbs, roots and the like; cut flowers and ornamental foliage
					return Codes.NurseryStock;
				case "07": // Edible vegetables and certain roots and tubers
				case "08": // Edible fruit and nuts; peel of citrus fruit or melons
				case "09": // Coffee, tea, maté and spices
					return Codes.Produce;
				case "10": // Cereals
					return Codes.SeedGrain;
				case "11": // Products of the milling industry; malt; starches; inulin; wheat gluten
				case "12": // Oil seeds and oleaginous fruits; miscellaneous grains, seeds and fruit; industrial or medical plants
				case "13": // Lac; gums, resins and other vegetable saps and extracts
				case "14": // Vegetable plaiting materials; vegetable products not elsewhere specified or included
					return Codes.PlantProducts;
				case "15": // Animal or vegetable fats and oils and their cleavage products; prepared edible fats; animal or veget
					return Codes.Miscellaneous;
				case "16": // Preparations of meat, of fish or of crustaceans, molluscs or other aquatic invertebrates
					return Codes.AnimalProducts;
				case "31": // Fertilisers
					return Codes.Fertiliser;
				case "87": // Vehicles other than railway or tramway rolling-stock, and parts and accessories thereof
					if (tariffCode.StartsWith("8708"))
					{
						return Codes.CarParts;
					}
					return Codes.Vehicles;
				case "40": // Rubber and articles thereof
					if (tariffCode.StartsWith("4011") || tariffCode.StartsWith("4012") || tariffCode.StartsWith("4013"))
					{
						return Codes.Tyres;
					}
					return Codes.Miscellaneous;
				case "44": // Wood and articles of wood; wood charcoal
					return Codes.Timber;

					#region All Other Codes Default to "Miscellaneous"
					// **** AND THE REST DEFAULT TO MISCELLANEOUS ****
					//case "17": // Sugars and sugar confectionery
					//case "18": // Cocoa and cocoa preparations
					//case "19": // Preparations of cereals, flour, starch or milk; pastrycooks' products
					//case "20": // Preparations of vegetables, fruit, nuts or other parts of plants
					//case "21": // Miscellaneous edible preparations
					//case "22": // Beverages, spirits and vinegar
					//case "23": // Residues and waste from the food industries; prepared animal fodder
					//case "24": // Tobacco and manufactured tobacco substitutes
					//case "25": // Salt; sulphur; earths and stone; plastering materials, lime and cement
					//case "26": // Ores, slag and ash
					//case "27": // Mineral fuels, mineral oils and products of their distillation; bituminous substances; mineral waxes
					//case "28": // Inorganic chemicals; organic or inorganic compounds of precious metals, of rare-earth metals, of rad
					//case "29": // Organic chemicals
					//case "30": // Pharmaceutical products
					//case "32": // Tanning or dyeing extracts; tannins and their derivatives; dyes, pigments and other colouring matter
					//case "33": // Essential oils and resinoids; perfumery, cosmetic or toilet preparations
					//case "34": // Soap, organic surface-active agents, washing preparations, lubricating preparations, artificial waxe
					//case "35": // Albuminoidal substances; modified starches; glues; enzymes
					//case "36": // Explosives; pyrotechnic products; matches; pyrophoric alloys; certain combustible preparations
					//case "37": // Photographic or cinematographic goods
					//case "38": // Miscellaneous chemical products
					//case "39": // Plastics and articles thereof
					//case "41": // Raw hides and skins (other than furskins) and leather
					//case "42": // Articles of leather; saddlery and harness; travel goods, handbags and similar containers; articles o
					//case "43": // Furskins and artificial fur; manufactures thereof
					//case "45": // Cork and articles of cork
					//case "46": // Manufactures of straw, of esparto or of other plaiting materials; basketware and wickerwork
					//case "47": // Pulp of wood or of other fibrous cellulosic material; recovered (waste and scrap) paper or paperboar
					//case "48": // Paper and paperboard; articles of paper pulp, of paper or of paperboard
					//case "49": // Printed books, newspapers, pictures and other products of the printing industry; manuscripts, typesc
					//case "50": // Silk
					//case "51": // Wool, fine or coarse animal hair; horsehair yarn and woven fabric
					//case "52": // Cotton
					//case "53": // Other vegetable textile fibres; paper yarn and woven fabrics of paper yarn
					//case "54": // Man-made filaments
					//case "55": // Man-made staple fibres
					//case "56": // Wadding, felt and nonwovens; special yarns; twine, cordage, ropes and cables and articles thereof
					//case "57": // Carpets and other textile floor coverings
					//case "58": // Special woven fabrics; tufted textile fabrics; lace; tapestries; trimmings; embroidery
					//case "59": // Impregnated, coated, covered or laminated textile fabrics; textile articles of a kind suitable for i
					//case "60": // Knitted or crocheted fabrics
					//case "61": // Articles of apparel and clothing accessories, knitted or crocheted
					//case "62": // Articles of apparel and clothing accessories, not knitted or crocheted
					//case "63": // Other made up textile articles; sets; worn clothing and worn textile articles; rags
					//case "64": // Footwear, gaiters and the like; parts of such articles
					//case "65": // Headgear and parts thereof
					//case "66": // Umbrellas, sun umbrellas, walking-sticks, seat-sticks, whips, riding-crops and parts thereof
					//case "67": // Prepared feathers and down and articles made of feathers or of down; artificial flowers; articles of
					//case "68": // Articles of stone, plaster, cement, asbestos, mica or similar materials
					//case "69": // Ceramic products
					//case "70": // Glass and glassware
					//case "71": // Natural or cultured pearls, precious or semi-precious stones, precious metals, metals clad with prec
					//case "72": // Iron and steel
					//case "73": // Articles of iron or steel
					//case "74": // Copper and articles thereof
					//case "75": // Nickel and articles thereof
					//case "76": // Aluminium and articles thereof
					//case "78": // Lead and articles thereof
					//case "79": // Zinc and articles thereof
					//case "80": // Tin and articles thereof
					//case "81": // Other base metals; cermets; articles thereof
					//case "82": // Tools, implements, cutlery, spoons and forks, of base metal; parts thereof of base metal
					//case "83": // Miscellaneous articles of base metal
					//case "84": // Nuclear reactors, boilers, machinery and mechanical appliances; parts thereof
					//case "85": // Electrical machinery and equipment and parts thereof; sound recorders and reproducers, television im
					//case "86": // Railway or tramway locomotives, rolling-stock and parts thereof; railway or tramway track fixtures a
					//case "88": // Aircraft, spacecraft and parts thereof
					//case "89": // Ships, boats and floating structures
					//case "90": // Optical, photographic, cinematographic, measuring, checking, precision, medical or surgical instrume
					//case "91": // Clocks and watches and parts thereof
					//case "92": // Musical instruments; parts and accessories of such articles
					//case "93": // Arms and ammunition; parts and accessories thereof
					//case "94": // Furniture; bedding, mattresses, mattress supports, cushions and similar stuffed furnishings; lamps a
					//case "95": // Toys, games and sports requisites; parts and accessories thereof
					//case "96": // Miscellaneous manufactured articles
					//case "97": // Works of art, collectors' pieces and antiques
					//case "98": // Miscellaneous New Zealand Provisions
					//case "99": // Excise Tariff
					#endregion
			}

			return Codes.Miscellaneous;

			// Could not find any Tariff relation for these Goods Types:
			// return Codes.Biologicals;
			// return Codes.Containers;
			// return Codes.Equipment;
			// return Codes.PersonalEffects;
			// return Codes.StoredProducts;
		}
	}
}
