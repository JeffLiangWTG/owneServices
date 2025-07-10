using System.Collections.Generic;
using Enterprise.Customs.Universal;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingUSCRegionDistrictPortColumnProvider : GridColumnProvider
	{
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			AddButtonColumn(Res.GetString("3d76f362-358e-4cda-b793-d99d28aea058", "Code"), ZZRefCusCodeListCombined.Schema.ZZD_Code, WebTracker.Grids.TrackingUSCRegionDistrictPorts.Code);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("dbe59ef2-84fb-4af4-83f8-ed25566b8fb8", "Name"), ZZRefCusCodeListCombined.Schema.ZZD_Description) { ColumnKey = WebTracker.Grids.TrackingUSCRegionDistrictPorts.Name });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingUSCRegionDistrictPorts.Code);
			result.Add((int)WebTracker.Grids.TrackingUSCRegionDistrictPorts.Name);
			return result;
		}
	}
}
