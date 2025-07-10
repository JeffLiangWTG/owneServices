using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingAWBSpecialHandlingColumnProvider))]
	sealed class TrackingAWBSpecialHandlingColumnProviderTest : GridColumnProviderTest
	{
		#region Implementation

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
			AddRequiredColumn(new ZDropDownListColumn("Code", ExportAWBSpecialHandling.Schema.EP_SpecialHandling, "Lookups.SpecialHandlingCodeDescriptionList")
			{
				DisplayStyle = OComboBoxDropDownStyle.CodeOnly,
				ColumnKey = WebTracker.Grids.TrackingAWBSpecialHandling.Code
			});

			AddDefaultsColumn(new ZTextEditColumn("Description", "SpecialHandlingDescription")
			{
				ColumnKey = WebTracker.Grids.TrackingAWBSpecialHandling.Description
			});
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingAWBSpecialHandlingColumnProvider();
		}

		#endregion
	}
}
