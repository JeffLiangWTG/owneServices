using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(DangerousGoodsColumnProvider))]
	sealed class DangerousGoodsColumnProviderTest : GridColumnProviderTest
	{
		#region Implementation

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.DangerousGoods.TechnicalName],
				TestProvider[WebTracker.Grids.DangerousGoods.IMOClass],
				TestProvider[WebTracker.Grids.DangerousGoods.PackingProvisions],
				TestProvider[WebTracker.Grids.DangerousGoods.State],
				TestProvider[WebTracker.Grids.DangerousGoods.Code]
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddRequiredColumn(new ZHyperLinkColumn("Code", UNDGSubstance.Schema.DG_Code) { ColumnKey = WebTracker.Grids.DangerousGoods.Code });
			AddColumn(new ZTextEditColumn("UN Number", UNDGSubstance.Schema.DG_UNNO) { ColumnKey = WebTracker.Grids.DangerousGoods.UNNumber });
			AddColumn(new ZTextEditColumn("Variant", UNDGSubstance.Schema.DG_Variant) { ColumnKey = WebTracker.Grids.DangerousGoods.Variant });
			AddDefaultsColumn(new ZTextEditColumn("Variation", UNDGSubstance.Schema.DG_Variation) { ColumnKey = WebTracker.Grids.DangerousGoods.Variation });
			AddDefaultsColumn(new ZTextEditColumn("Proper Shipping Name", UNDGSubstance.Schema.DG_PSN) { ColumnKey = WebTracker.Grids.DangerousGoods.ProperShippingName });
			AddDefaultsColumn(new ZTextEditColumn("US DOT Name", UNDGSubstance.Schema.DG_UsrUSDOTShippingName) { ColumnKey = WebTracker.Grids.DangerousGoods.USDOTName });
			AddDefaultsColumn(new ZTextEditColumn("EMS", UNDGSubstance.Schema.DG_EMS) { ColumnKey = WebTracker.Grids.DangerousGoods.EMS });
			AddDefaultsColumn(new ZTextEditColumn("IMO Class", UNDGSubstance.Schema.DG_Class) { ColumnKey = WebTracker.Grids.DangerousGoods.IMOClass });
			AddDefaultsColumn(new ZTextEditColumn("Stowage Requirements", UNDGSubstance.Schema.DG_CodedStow) { ColumnKey = WebTracker.Grids.DangerousGoods.StowageRequirements });
			AddDefaultsColumn(new ZCheckBoxColumn("Active", UNDGSubstance.Schema.DG_IsActive) { ColumnKey = WebTracker.Grids.DangerousGoods.Active });
			AddDefaultsColumn(new ZCheckBoxColumn("System", UNDGSubstance.Schema.DG_IsSystem) { ColumnKey = WebTracker.Grids.DangerousGoods.System });
			AddDefaultsColumn(new ZCodeFindBoxColumn("State", UNDGSubstance.Schema.DG_State) { ColumnKey = WebTracker.Grids.DangerousGoods.State });
			AddColumn(new ZTextEditColumn("Flash Point", UNDGSubstance.Schema.DG_FlashPoint) { ColumnKey = WebTracker.Grids.DangerousGoods.FlashPoint });
			AddColumn(new ZTextEditColumn("Packing Group", UNDGSubstance.Schema.DG_PG) { ColumnKey = WebTracker.Grids.DangerousGoods.PackingGroup });
			AddColumn(new ZTextEditColumn("Stowage Category", UNDGSubstance.Schema.DG_StowCat) { ColumnKey = WebTracker.Grids.DangerousGoods.StowageCategory });
			AddColumn(new ZTextEditColumn("Tank Provisions", UNDGSubstance.Schema.DG_TankProv) { ColumnKey = WebTracker.Grids.DangerousGoods.TankProvisions });
			AddColumn(new ZTextEditColumn("Packing Provisions", UNDGSubstance.Schema.DG_PackProv) { ColumnKey = WebTracker.Grids.DangerousGoods.PackingProvisions });
			AddColumn(new ZTextEditColumn("Treat As", UNDGSubstance.Schema.DG_TreatAs) { ColumnKey = WebTracker.Grids.DangerousGoods.TreatAs });
			AddColumn(new ZTextEditColumn("Technical Name", UNDGSubstance.Schema.DG_TechName) { ColumnKey = WebTracker.Grids.DangerousGoods.TechnicalName });
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.DangerousGoods.State
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new DangerousGoodsColumnProvider();
		}

		#endregion
	}
}
