using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(TrackingAWBOtherChargesColumnProvider))]
	sealed class TrackingAWBOtherChargesColumnProviderTest : GridColumnProviderTest
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
			AddRequiredColumn(new ZDropDownListColumn("Charge Code", ExportAWBOtherCharges.Schema.EO_ChargeCode, "IATAChargeCodesList")
			{
				DisplayStyle = OComboBoxDropDownStyle.CodeOnly,
				ColumnKey = WebTracker.Grids.TrackingAWBOtherCharges.ChargeCode
			});

			AddRequiredColumn(new ZDropDownListColumn("Entitlement", ExportAWBOtherCharges.Schema.EO_EntitlementCode, "EntitlementCodesList")
			{
				DisplayStyle = OComboBoxDropDownStyle.CodeOnly,
				ColumnKey = WebTracker.Grids.TrackingAWBOtherCharges.Entitlement
			});

			AddDefaultsColumn(new ZTextEditColumn("Description", ExportAWBOtherCharges.Schema.EO_ChargeDescription)
			{
				ColumnKey = WebTracker.Grids.TrackingAWBOtherCharges.Description
			});

			AddDefaultsColumn(new ZCalcEditColumn("Amount", ExportAWBOtherCharges.Schema.EO_Amount)
			{
				ColumnKey = WebTracker.Grids.TrackingAWBOtherCharges.Amount,
				Decimals = 2
			});
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new TrackingAWBOtherChargesColumnProvider();
		}

		#endregion
	}
}
