using System.Collections.Generic;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(WarehouseDocketContainerColumnProvider))]
	sealed class WarehouseDocketContainerColumnProviderTest : GridColumnProviderTest
	{
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Container #", WhsDocketContainerSchema.WC_ContainerNum.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerNumber });
			AddDefaultsColumn(new ZTextEditColumn("Seal #", WhsDocketContainerSchema.WC_SealNum.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.SealNumber });
			AddDefaultsColumn(new ZFindBoxColumn("Type", WhsDocketContainerSchema.WC_RC.Name, "Lookups.RefContainers")
			{
				ColumnKey = WebTracker.Grids.TrackingContainers.Type,
				ValueFieldName = "PK",
				ModuleID = WebModuleIDs.RefContainer
			});
			AddDefaultsColumn(new ZCheckBoxColumn("Palletized", WhsDocketContainerSchema.WC_IsPalletised.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.Palletized });
			AddDefaultsColumn(new ZCheckBoxColumn("Chargeable", WhsDocketContainerSchema.WC_IsChargeable.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.Chargeable });
			AddDefaultsColumn(new ZCalcEditColumn("Items", WhsDocketContainerSchema.WC_ItemCount.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.Items });
			AddDefaultsColumn(new ZCalcEditColumn("Pallets", WhsDocketContainerSchema.WC_PalletCount.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.Pallets });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected override List<object> GetUnsortableColumnKeys() => new List<object>
		{
			WebTracker.Grids.TrackingContainers.Type
		};

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new WarehouseDocketContainerColumnProvider();
		}
	}
}
