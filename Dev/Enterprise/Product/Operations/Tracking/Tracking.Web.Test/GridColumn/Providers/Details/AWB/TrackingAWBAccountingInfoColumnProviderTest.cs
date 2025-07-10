using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingAWBAccountingInfoColumnProvider))]
	sealed class TrackingAWBAccountingInfoColumnProviderTest : GridColumnProviderTest
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
			AddRequiredColumn(new ZDropDownListColumn("Code", ExportAWBAccountingInformation.Schema.EA_InformationID, "AccountingCodes")
			{
				DisplayStyle = OComboBoxDropDownStyle.CodeOnly,
				ColumnKey = WebTracker.Grids.TrackingAWBAccountingInfo.Code
			});

			AddDefaultsColumn(new ZTextEditColumn("Information", ExportAWBAccountingInformation.Schema.EA_Information) { ColumnKey = WebTracker.Grids.TrackingAWBAccountingInfo.Information });
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingAWBAccountingInfoColumnProvider();
		}

		#endregion
	}
}
