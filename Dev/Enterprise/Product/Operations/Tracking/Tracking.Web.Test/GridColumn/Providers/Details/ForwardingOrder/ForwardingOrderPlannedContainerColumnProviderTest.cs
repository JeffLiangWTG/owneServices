using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ForwardingOrderPlannedContainerColumnProvider))]
	sealed class ForwardingOrderPlannedContainerColumnProviderTest : GridColumnProviderTest
	{
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddDefaultsColumn(new ZTextEditColumn("Container No.", JobOrderContainerSchema.J1_ContainerNumber.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerNumber });
			AddDefaultsColumn(new ZCalcEditColumn("Count", JobOrderContainerSchema.J1_ContainerCount.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainersCount });
			AddDefaultsColumn(new ZFindBoxColumn("Type", JobOrderContainerSchema.J1_RC.Name, "J1_RC_List", typeof(RefContainer)) { ColumnKey = WebTracker.Grids.TrackingContainers.Type });
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
			return new ForwardingOrderPlannedContainerColumnProvider();
		}
	}
}
