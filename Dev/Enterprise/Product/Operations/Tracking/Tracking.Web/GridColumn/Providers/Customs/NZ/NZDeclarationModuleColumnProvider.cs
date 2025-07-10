using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class NZDeclarationModuleColumnProvider : BaseDeclarationModuleColumnProvider
	{
		protected override void AddCountrySpecificColumns()
		{
			base.AddCountrySpecificColumns();

			AddToDictionary(
				new ZTextEditColumn(Res.GetString("2B2F4E3B-309F-4BD4-9B6F-DC02FBDF9344", "TSW Status"),
				$"{nameof(TrackingDeclaration.Declaration)}.{nameof(Integration.Customs.NZ.IJobDeclaration.JE_TSWCombinedStatusDesc)}")
				{
					ColumnKey = WebTracker.Grids.TrackingDeclarations.TSWStatus,
				});
		}
	}
}
