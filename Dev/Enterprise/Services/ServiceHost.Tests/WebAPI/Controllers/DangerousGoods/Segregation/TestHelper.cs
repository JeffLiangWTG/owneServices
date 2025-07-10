using System.Linq;
using CargoWise.Definitions.Freight.DangerousGoods;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.DangerousGoods.Segregation
{
	public static class TestHelper
	{
		public static (UNDGClassificationData classificationData, UNDGSubstance substance, string errorMessage) MockFind(UNDGDataItemDTO dto, string standard)
		{
			var firstSubstanceMatchingTheStandard = dto.UNDGSubstanceDTOs?.FirstOrDefault(s => s.Standard == standard);
			if (firstSubstanceMatchingTheStandard == null)
			{
				var errorMessage = $"Cannot find UNDG substance for standard {standard}";
				return (null, null, errorMessage);
			}

			var factory = new BusinessObjectFactory();
			var substance = factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_UNNO = firstSubstanceMatchingTheStandard.Unno;
			substance.DG_Variant = firstSubstanceMatchingTheStandard.Variant;
			substance.DG_Standard = firstSubstanceMatchingTheStandard.Standard;
			return (dto.UNDGClassificationData, substance, string.Empty);
		}

		public static UNDGSubstance CreateUNDGSubstance(BusinessObjectFactory factory, string undgUNNOCode, string undgClass, string undgCode, string undgVariant, string undgPSN = "", decimal lqMaxAmt = 0m, string lqMaxAmtUQ = "KG", string undgStandard = "IMO", string undgMode = "")
		{
			var uNDGSubstance = factory.New<UNDGSubstance>();
			uNDGSubstance.DG_UNNO = undgUNNOCode;
			uNDGSubstance.DG_Class = undgClass;
			uNDGSubstance.DG_Variant = undgVariant;
			uNDGSubstance.DG_PSN = undgPSN;
			uNDGSubstance.DG_LQMaxAmt = lqMaxAmt;
			uNDGSubstance.DG_Standard = undgStandard;
			uNDGSubstance.DG_Mode = undgMode;
			return uNDGSubstance;
		}

		public static UNDGClassificationData RegulatedQuantity()
		{
			return new UNDGClassificationData(QuantityClassifications.Regulated);
		}

		public static UNDGClassificationData LimitedQuantity()
		{
			return new UNDGClassificationData(QuantityClassifications.Limited);
		}

		public static UNDGClassificationData ExceptedQuantity()
		{
			return new UNDGClassificationData(QuantityClassifications.Excepted);
		}
	}
}
