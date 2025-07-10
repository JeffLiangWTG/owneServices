
namespace Enterprise.eManifest.Module
{
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using Enterprise.Freight.Business;

	public class ELoadListForConsolFilterProvider : DefaultFilterProvider
	{
		public ELoadListForConsolFilterProvider(CommonConsol consol)
		{
			Argument.NotNull(consol, "consol");

			this.consol = consol;
		}

		protected override void SetDefaultFiltersCore(IFilterBusinessObjectDefaultsProvider collection)
		{
			AddFilterDefaults(collection, ELoadListFilterBusinessObject.Descriptions.MasterBill, consol.JK_MasterBillNum);

			if (consol.UnpackDepotAddress != null && consol.UnpackDepotAddress.Header != null)
			{
				AddFilterDefaults(collection, ELoadListFilterBusinessObject.Descriptions.DestinationDepot, consol.UnpackDepotAddress.Header.PK);
			}

			AddTextAndNkFilterDefaults(
				collection,
				ELoadListFilterBusinessObject.Descriptions.FlightVoyageNumAndVessel,
				consol.MostInterestingTransportForBinding[0].JW_VoyageFlight,
				consol.MostInterestingTransportForBinding[0].JW_Vessel);
		}

		readonly CommonConsol consol;
	}
}
