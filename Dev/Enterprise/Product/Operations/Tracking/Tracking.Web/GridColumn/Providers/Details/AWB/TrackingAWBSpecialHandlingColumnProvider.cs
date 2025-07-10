using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingAWBSpecialHandlingColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			ZBindToChecker.CheckBindTo((string)((ExportAWBSpecialHandling)null).EP_SpecialHandling);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((ExportAWBSpecialHandling)null).Lookups.SpecialHandlingCodeDescriptionList);
			AddToDictionaryAsRequired(new ZDropDownListColumn(Res.GetString("6cbd19ba-e64b-4f22-9805-0939dd2e7455", "Code"), ExportAWBSpecialHandling.Schema.EP_SpecialHandling, "Lookups.SpecialHandlingCodeDescriptionList")
			{
				DisplayStyle = OComboBoxDropDownStyle.CodeOnly,
				ColumnKey = WebTracker.Grids.TrackingAWBSpecialHandling.Code
			});

			ZBindToChecker.CheckBindTo((string)((ExportAWBSpecialHandling)null).SpecialHandlingDescription);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b87fba70-d2e8-4789-a723-36436256b87b", "Description"), "SpecialHandlingDescription")
			{
				ColumnKey = WebTracker.Grids.TrackingAWBSpecialHandling.Description,
				ReadOnly = true
			});
		}
	}
}
