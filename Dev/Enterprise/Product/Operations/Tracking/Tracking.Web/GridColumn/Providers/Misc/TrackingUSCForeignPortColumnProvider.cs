using System.Collections.Generic;
using Enterprise.Customs.Universal;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingUSCForeignPortColumnProvider : GridColumnProvider
	{
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddButtonColumn(Res.GetString("6cbd19ba-e64b-4f22-9805-0939dd2e7455", "Code"), ZZRefCusCodeListCombined.Schema.ZZD_Code, WebTracker.Grids.TrackingUSCForeignPorts.Code);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("dfda10c3-9dbd-49af-a1bb-faed01d24e0d", "Name"), ZZRefCusCodeListCombined.Schema.ZZD_Description) { ColumnKey = WebTracker.Grids.TrackingUSCForeignPorts.Name });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingUSCForeignPorts.Code);
			result.Add((int)WebTracker.Grids.TrackingUSCForeignPorts.Name);
			return result;
		}
	}
}
