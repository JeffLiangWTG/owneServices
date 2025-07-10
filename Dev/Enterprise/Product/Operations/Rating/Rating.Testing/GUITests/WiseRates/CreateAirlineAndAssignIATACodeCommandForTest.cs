using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;

namespace Enterprise.Rating.GUI.Test
{
	public class CreateAirlineAndAssignIATACodeCommandForTest : CreateAirlineAndAssignIATACodeCommand
	{
		public OrgHeader OrganizationToSelect { get; }

		public RefAirline Airline { get; }

		public CreateAirlineAndAssignIATACodeCommandForTest(OrgHeader orgHeader, bool shouldCreateAirline = false, RefAirline airline = null, IDialogService dialogService = null)
			: base(shouldCreateAirline, dialogService)
		{
			OrganizationToSelect = orgHeader;
			Airline = airline;
		}

		protected override OrgHeader SelectOrganization()
		{
			return OrganizationToSelect ?? base.SelectOrganization();
		}

		protected override RefAirline CreateAirLine()
		{
			return Airline ?? base.CreateAirLine();
		}
	}
}
