using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business.Common
{
	public class TransitWarehouseValidationHelper : IWhsUNDGLimitValidationHelper
	{
		public WhsUNDGLimitValidationInfo GetUNDGLimitValidationInfo(IWhsUNDGLimit undgLimit)
		{
			var undgLimitBO = (WhsUNDGLimit)undgLimit;
			Argument.NotNull(undgLimitBO, nameof(undgLimitBO));

			var limitType = UNDGLimitType.DG;
			var code = string.Empty;
			var totalWeight = 0m;
			var totalVolume = 0m;

			var totalsViewQuery = new ZQuery(WhsItemUNDGTotalsViewSchema.WDT_WW_Warehouse, undgLimitBO.WWD_WW_Warehouse);
			if (undgLimitBO.UNDGSubstance != null)
			{
				totalsViewQuery.AddToFilter(WhsItemUNDGTotalsViewSchema.WDT_DG_Substance, undgLimitBO.WWD_DG);
				code = undgLimitBO.UNDGSubstance.DG_Code;
			}
			else if (undgLimitBO.UNDGCountryReference != null)
			{
				totalsViewQuery.AddToFilter(WhsItemUNDGTotalsViewSchema.WDT_DCR_UNDGCountryReference, undgLimitBO.WWD_DCR_UNDGCountryReference);
				limitType = UNDGLimitType.CountryReference;
				code = undgLimitBO.UNDGCountryReference.DCR_Code;
			}
			else if (!undgLimitBO.WWD_UNDGClass.IsEmpty)
			{
				totalsViewQuery.AddToFilter(WhsItemUNDGTotalsViewSchema.WDT_Source, "CLS");
				totalsViewQuery.AddToFilter(WhsItemUNDGTotalsViewSchema.WDT_Code, undgLimitBO.WWD_UNDGClass);
				limitType = UNDGLimitType.UNDGClass;
				code = undgLimitBO.WWD_UNDGClass;
			}

			var newFactory = new BusinessObjectFactory();
			var totalsView = newFactory.LoadTop1<WhsItemUNDGTotalsView>(totalsViewQuery);
			if (totalsView != null)
			{
				totalWeight = totalsView.WDT_TotalWeight;
				totalVolume = totalsView.WDT_TotalVolume;
			}

			return new WhsUNDGLimitValidationInfo(limitType, code, totalWeight, totalVolume);
		}

		public ZString UNDGStorageUnit => Res.GetString("26eccd35-1392-4f9a-bfb9-7e9b9b629b38", "packages");
	}
}
