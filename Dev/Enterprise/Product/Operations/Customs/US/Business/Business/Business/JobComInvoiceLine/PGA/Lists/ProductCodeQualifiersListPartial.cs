using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	partial class ProductCodeQualifiersList
	{
		public static CodeDescriptionPairList GetAPHISProductCodeQualifiers(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("APHISProductCodeQualifiers", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.APHISVeterinaryBiologics, Descriptions.APHISVeterinaryBiologics);
				result.AddPair(Codes.GlobalProductClasBrickCode, Descriptions.GlobalProductClasBrickCode);
				result.AddPair(Codes.UNStandardProductsServicesCode, Descriptions.UNStandardProductsServicesCode);
				result.AddPair(Codes.TaxonomicSerialNumber, Descriptions.TaxonomicSerialNumber);
				return result;
			});
		}

		public static string GetShortDescription(string code)
		{
			switch (code)
			{
				case Codes.AccessionNumber:
					return "Accession Number";
				case Codes.ChemicalAbstractServicesNumber:
					return "CAS Number";
				case Codes.ControlledSubstancesActNumber:
					return "Controlled Substances Act Number";
				case Codes.FDAProductCode:
					return "FDA - Product Code";
				case Codes.FuelAdditiveID:
					return "Fuel/Additive ID";
				case Codes.GlobalProductClasBrickCode:
					return "Global Product Classification Brick Code";
				case Codes.InstitutionalMeatPurchaseSpecNumber:
					return "IMPS Number";
				case Codes.LoREXNumber:
					return "LoREX Number";
				case Codes.LVENumber:
					return "LVE Number";
				case Codes.NationalDrugCode:
					return "National Drug Code";
				case Codes.PCCode:
					return "PC Code";
				case Codes.PMNNumber:
					return "PMN Number";
				case Codes.PriceLookUpCode:
					return "Price Look-Up code";
				case Codes.StockKeepingUnit:
					return "Stock Keeping Unit";
				case Codes.TaxonomicSerialNumber:
					return "Taxonomic Serial Number";
				case Codes.TMENumber:
					return "TME Number";
				case Codes.UNStandardProductsServicesCode:
					return "UNSPSC Commodity Code";
				case Codes.APHISVeterinaryBiologics:
					return "Veterinary Biologics";
				default:
					return code;
			}
		}
	}
}
