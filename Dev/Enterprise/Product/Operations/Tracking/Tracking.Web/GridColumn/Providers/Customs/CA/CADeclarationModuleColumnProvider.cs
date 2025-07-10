using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class CADeclarationModuleColumnProvider : BaseDeclarationModuleColumnProvider
	{
		protected override void AddCountrySpecificColumns()
		{
			base.AddCountrySpecificColumns();

			AddToDictionary(
				new ZDateTimeColumn(Res.GetString("08044171-401d-412d-b603-a03b6858169f", "Accepted Date"),
				$"{nameof(TrackingDeclaration.Declaration)}.B3AcceptedDate")
				{
					ColumnKey = WebTracker.Grids.TrackingDeclarations.CA_AcceptedDate,
				});
			AddToDictionary(
				new ZDateTimeColumn(Res.GetString("5d0067d8-7e61-4455-aa97-3c1bf458e5d7", "Accounting Date"),
				$"{nameof(TrackingDeclaration.Declaration)}.CA_K84AccountingDate")
				{
					ColumnKey = WebTracker.Grids.TrackingDeclarations.CA_AccountingDate,
				});
		}
	}
}
