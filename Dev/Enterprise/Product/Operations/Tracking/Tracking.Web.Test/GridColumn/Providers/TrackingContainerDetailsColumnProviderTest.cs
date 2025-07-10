using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingContainerDetailsColumnProvider))]
	[HttpContextEnabledTest]
	sealed class TrackingContainerDetailsColumnProviderTest : TrackingContainerColumnProviderTest
	{
		#region TestCases

		public override void TestColumnKeys()
		{
			base.TestColumnKeys();

			LoginAsQuickViewUserForTest();
			SetupNewProvider();
			base.TestColumnKeys();
		}

		public override void TestDefaultColumns()
		{
			base.TestDefaultColumns();

			LoginAsQuickViewUserForTest();
			SetupNewProvider();
			base.TestDefaultColumns();
		}

		public override void TestRequiredColumns()
		{
			base.TestRequiredColumns();

			LoginAsQuickViewUserForTest();
			SetupNewProvider();
			base.TestRequiredColumns();
		}

		public override void TestUniqueColumns()
		{
			base.TestUniqueColumns();

			LoginAsQuickViewUserForTest();
			SetupNewProvider();
			base.TestUniqueColumns();
		}

		#endregion

		protected override bool SupportsOldLayoutFix
		{
			get
			{
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			AddColumn(new ZCalcEditColumn("Number of Containers", JobContainerSchema.JC_ContainerCount.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainersCount });
			if (LoggedSiteUserForTest != null && !LoggedSiteUserForTest.IsShipmentQuickViewUser)
			{
				AddDefaultsColumn(new ZTextEditColumn("Tare Weight", TrackingContainer.Schema.JC_TareWeightWithSuppression) { ColumnKey = WebTracker.Grids.TrackingContainers.TareWeight });
				AddDefaultsColumn(new ZTextEditColumn("Weight", TrackingContainer.Schema.JC_TotalWeightWithSuppression) { ColumnKey = WebTracker.Grids.TrackingContainers.Weight });
			}

			AddDefaultsColumn(new ZTextEditColumn("Delivery Mode", JobContainerSchema.JC_DeliveryMode.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.DeliveryMode });
			AddDefaultsColumn(new ZDateTimeColumn("Est. Delivery", TrackingContainer.Schema.JC_ArrivalEstimatedDelivery, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.EstimatedDelivery });
			AddDefaultsColumn(new ZDateTimeColumn("Est. Return", TrackingContainer.Schema.JC_EmptyReturnedBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.EstimatedReturn });
			AddDefaultsColumn(new ZDateTimeColumn("Act. Return", TrackingContainer.Schema.JC_ContainerYardEmptyReturnGateIn, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.ActualReturn });
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingContainerDetailsColumnProvider();
		}
	}
}
