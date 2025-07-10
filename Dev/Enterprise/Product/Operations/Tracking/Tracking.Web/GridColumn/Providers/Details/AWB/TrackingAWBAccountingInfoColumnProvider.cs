using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingAWBAccountingInfoColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			ZBindToChecker.CheckBindTo((string)((ExportAWBAccountingInformation)null).EA_InformationID);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((ExportAWBAccountingInformation)null).Lookups.AccountingCodes);
			AddToDictionaryAsRequired(new ZDropDownListColumn(Res.GetString("6cbd19ba-e64b-4f22-9805-0939dd2e7455", "Code"), ExportAWBAccountingInformation.Schema.EA_InformationID, "Lookups.AccountingCodes")
			{
				DisplayStyle = OComboBoxDropDownStyle.CodeOnly,
				ColumnKey = WebTracker.Grids.TrackingAWBAccountingInfo.Code
			});

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("da8865ef-541d-4ca4-999a-aefd2794dabc", "Information"), ExportAWBAccountingInformation.Schema.EA_Information)
			{
				ColumnKey = WebTracker.Grids.TrackingAWBAccountingInfo.Information
			});
		}
	}
}
