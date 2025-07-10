using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingAWBOtherChargesColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			ZBindToChecker.CheckBindTo((string)((ExportAWBOtherCharges)null).EO_ChargeCode);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((ExportAWBOtherCharges)null).IATAChargeCodesList);
			AddToDictionaryAsRequired(new ZDropDownListColumn(Res.GetString("d67f4474-722f-4841-875d-82d1682fc204", "Charge Code"), ExportAWBOtherCharges.Schema.EO_ChargeCode, "IATAChargeCodesList")
			{
				DisplayStyle = OComboBoxDropDownStyle.CodeOnly,
				ColumnKey = WebTracker.Grids.TrackingAWBOtherCharges.ChargeCode
			});

			ZBindToChecker.CheckBindTo((string)((ExportAWBOtherCharges)null).EO_EntitlementCode);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((ExportAWBOtherCharges)null).EntitlementCodesList);
			AddToDictionaryAsRequired(new ZDropDownListColumn(Res.GetString("0097e901-c49a-4af1-91a2-64f8b8bd7c0f", "Entitlement"), ExportAWBOtherCharges.Schema.EO_EntitlementCode, "EntitlementCodesList")
			{
				DisplayStyle = OComboBoxDropDownStyle.CodeOnly,
				ColumnKey = WebTracker.Grids.TrackingAWBOtherCharges.Entitlement
			});

			ZBindToChecker.CheckBindTo((string)((ExportAWBOtherCharges)null).EO_ChargeDescription);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("a3782265-114e-4919-9958-f959d8846029", "Description"), ExportAWBOtherCharges.Schema.EO_ChargeDescription)
			{
				ColumnKey = WebTracker.Grids.TrackingAWBOtherCharges.Description
			});

			ZBindToChecker.CheckBindTo((decimal)((ExportAWBOtherCharges)null).EO_Amount);
			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("2b75f7ab-5c8c-4815-9164-c55be01bf424", "Amount"), ExportAWBOtherCharges.Schema.EO_Amount)
			{
				ColumnKey = WebTracker.Grids.TrackingAWBOtherCharges.Amount,
				Decimals = 2
			});
		}
	}
}
