using System.Collections.Generic;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class DangerousGoodsColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_Code);
			ZHyperLinkColumn numberColumn = new ZHyperLinkColumn(Res.GetString("3deee561-0d44-4b7c-9675-01cb4a26f1d8", "Code"), UNDGSubstance.Schema.DG_Code) { ColumnKey = WebTracker.Grids.DangerousGoods.Code };
			if (HttpContext.Current != null && HttpContext.Current.Request != null)
			{
				numberColumn.DataNavigateUrlFormatString = (NoResString)@"javascript: parent." + HttpContext.Current.Request.QueryString[ZIFramePage.OKFunctionQuery] + (NoResString)"('{0}','{1}');"; // URL
			}
			numberColumn.DataNavigateUrlFields = new string[] { UNDGSubstance.Schema.DG_Code, "PK" };
			AddToDictionaryAsRequired(numberColumn);

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_UNNO);
			AddToDictionary(new ZTextEditColumn(Res.GetString("757e951f-edd0-4a64-83c3-50b03657ee92", "UN Number"), UNDGSubstance.Schema.DG_UNNO) { ColumnKey = WebTracker.Grids.DangerousGoods.UNNumber });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_Variant);
			AddToDictionary(new ZTextEditColumn(Res.GetString("bc82212f-cca7-4e50-a178-6af28fe4990a", "Variant"), UNDGSubstance.Schema.DG_Variant) { ColumnKey = WebTracker.Grids.DangerousGoods.Variant });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_Variation);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("bcccdb93-3469-4d2b-be12-a55b088acf28", "Variation"), UNDGSubstance.Schema.DG_Variation) { ColumnKey = WebTracker.Grids.DangerousGoods.Variation });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_PSN);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("41ff9a82-f4be-4fa5-a297-3b4dbb5f567d", "Proper Shipping Name"), UNDGSubstance.Schema.DG_PSN) { ColumnKey = WebTracker.Grids.DangerousGoods.ProperShippingName });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_UsrUSDOTShippingName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("a99bfaac-fcd1-4314-baed-a3425a1351ef", "US DOT Name"), UNDGSubstance.Schema.DG_UsrUSDOTShippingName) { ColumnKey = WebTracker.Grids.DangerousGoods.USDOTName });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_EMS);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("23d9a1d3-4b45-4b0c-97e6-badc68eddee6", "EMS"), UNDGSubstance.Schema.DG_EMS) { ColumnKey = WebTracker.Grids.DangerousGoods.EMS });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_Class);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("f374d471-b838-4c8a-8d00-73715bed9dc2", "IMO Class"), UNDGSubstance.Schema.DG_Class) { ColumnKey = WebTracker.Grids.DangerousGoods.IMOClass });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_CodedStow);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("d9f2d9d5-a04d-4c34-b379-b24aa9a2e6ec", "Stowage Requirements"), UNDGSubstance.Schema.DG_CodedStow) { ColumnKey = WebTracker.Grids.DangerousGoods.StowageRequirements });

			ZBindToChecker.CheckBindTo((ZBool)((UNDGSubstance)null).DG_IsActive);
			AddToDictionaryAsDefault(new ZCheckBoxColumn(Res.GetString("908fcbba-6cc7-44ee-b0b7-19769e339237", "Active"), UNDGSubstance.Schema.DG_IsActive) { ColumnKey = WebTracker.Grids.DangerousGoods.Active });

			ZBindToChecker.CheckBindTo((ZBool)((UNDGSubstance)null).DG_IsSystem);
			AddToDictionaryAsDefault(new ZCheckBoxColumn(Res.GetString("02269b6e-b78b-43b6-82d4-11a28d945471", "System"), UNDGSubstance.Schema.DG_IsSystem) { ColumnKey = WebTracker.Grids.DangerousGoods.System });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_State);
			AddToDictionaryAsDefault(new ZCodeFindBoxColumn(Res.GetString("f1d3165d-ffe0-418d-904b-6e997fd2681f", "State"), UNDGSubstance.Schema.DG_State) { ColumnKey = WebTracker.Grids.DangerousGoods.State });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_FlashPoint);
			AddToDictionary(new ZTextEditColumn(Res.GetString("0dfa7f67-0b8e-428a-b069-8e8f937c14c6", "Flash Point"), UNDGSubstance.Schema.DG_FlashPoint) { ColumnKey = WebTracker.Grids.DangerousGoods.FlashPoint });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_PG);
			AddToDictionary(new ZTextEditColumn(Res.GetString("4d357f37-5fda-420a-a46f-42c21902cbab", "Packing Group"), UNDGSubstance.Schema.DG_PG) { ColumnKey = WebTracker.Grids.DangerousGoods.PackingGroup });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_StowCat);
			AddToDictionary(new ZTextEditColumn(Res.GetString("2cd53c92-00af-435e-ae16-94b6efd80dbe", "Stowage Category"), UNDGSubstance.Schema.DG_StowCat) { ColumnKey = WebTracker.Grids.DangerousGoods.StowageCategory });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_TankProv);
			AddToDictionary(new ZTextEditColumn(Res.GetString("5efca380-9978-4efd-9891-02ab3c07b6fb", "Tank Provisions"), UNDGSubstance.Schema.DG_TankProv) { ColumnKey = WebTracker.Grids.DangerousGoods.TankProvisions });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_PackProv);
			AddToDictionary(new ZTextEditColumn(Res.GetString("a730bc50-bdd3-4991-b61d-abeea3b3abbb", "Packing Provisions"), UNDGSubstance.Schema.DG_PackProv) { ColumnKey = WebTracker.Grids.DangerousGoods.PackingProvisions });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_TreatAs);
			AddToDictionary(new ZTextEditColumn(Res.GetString("7841524e-171a-4505-9a1b-220f11095657", "Treat As"), UNDGSubstance.Schema.DG_TreatAs) { ColumnKey = WebTracker.Grids.DangerousGoods.TreatAs });

			ZBindToChecker.CheckBindTo((ZString)((UNDGSubstance)null).DG_TechName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("a84cbcfb-47f5-431a-ad45-c4c8d55d7da7", "Technical Name"), UNDGSubstance.Schema.DG_TechName) { ColumnKey = WebTracker.Grids.DangerousGoods.TechnicalName });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.DangerousGoods.Code);
			result.Add((int)WebTracker.Grids.DangerousGoods.UNNumber);
			result.Add((int)WebTracker.Grids.DangerousGoods.Variant);
			result.Add((int)WebTracker.Grids.DangerousGoods.Variation);
			result.Add((int)WebTracker.Grids.DangerousGoods.ProperShippingName);
			result.Add((int)WebTracker.Grids.DangerousGoods.USDOTName);
			result.Add((int)WebTracker.Grids.DangerousGoods.EMS);
			result.Add((int)WebTracker.Grids.DangerousGoods.IMOClass);
			result.Add((int)WebTracker.Grids.DangerousGoods.StowageRequirements);
			result.Add((int)WebTracker.Grids.DangerousGoods.Active);
			result.Add((int)WebTracker.Grids.DangerousGoods.System);
			result.Add((int)WebTracker.Grids.DangerousGoods.State);
			result.Add((int)WebTracker.Grids.DangerousGoods.FlashPoint);
			result.Add((int)WebTracker.Grids.DangerousGoods.PackingGroup);
			result.Add((int)WebTracker.Grids.DangerousGoods.StowageCategory);
			result.Add((int)WebTracker.Grids.DangerousGoods.TankProvisions);
			result.Add((int)WebTracker.Grids.DangerousGoods.PackingProvisions);
			result.Add((int)WebTracker.Grids.DangerousGoods.TreatAs);
			result.Add((int)WebTracker.Grids.DangerousGoods.TechnicalName);
			return result;
		}
	}
}
