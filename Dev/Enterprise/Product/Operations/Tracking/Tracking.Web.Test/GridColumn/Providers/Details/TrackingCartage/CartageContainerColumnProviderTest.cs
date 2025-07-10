using System.Collections.Generic;
using Enterprise.Freight.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(CartageContainerColumnProvider))]
	sealed class CartageContainerColumnProviderTest : GridColumnProviderTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Container No.", CommonContainer.Schema.JC_ContainerNum) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerNumber });
			AddDefaultsColumn(new ZTextEditColumn("Count", CommonContainer.Schema.JC_ContainerCount) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainersCount });
			AddDefaultsColumn(new ZTextEditColumn("Seal/Rate Class", CommonContainer.Schema.JC_SealNum) { ColumnKey = WebTracker.Grids.TrackingContainers.SealNumber });
			AddDefaultsColumn(new ZTextEditColumn("Mode", CommonContainer.Schema.JC_ContainerMode) { ColumnKey = WebTracker.Grids.TrackingContainers.Mode });
			AddDefaultsColumn(new ZFindBoxColumn("Type", CommonContainer.Schema.JC_RC, "RefContainer_List") { ColumnKey = WebTracker.Grids.TrackingContainers.Type });
			AddDefaultsColumn(new ZTextEditColumn("Delivery Mode", CommonContainer.Schema.JC_DeliveryMode) { ColumnKey = WebTracker.Grids.TrackingContainers.DeliveryMode });
			AddDefaultsColumn(new ZTextEditColumn("Gross Weight (Kg)", CommonContainer.Schema.JC_Calc_ActualGrossWeightInKgs) { ColumnKey = WebTracker.Grids.TrackingContainers.Weight });
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
			return new CartageContainerColumnProvider();
		}
	}
}
