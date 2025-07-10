using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSContainerStorageJobDateProvider : JobDatesProvider<CFSContainer>
	{
		public CFSContainerStorageJobDateProvider(CFSContainer container)
			: base(container) { }

		protected override ZDateTime GetArrivalDateCore()
		{
			return Parent.JC_ArrivalTime;
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return Parent.JC_DepartureTime;
		}
	}
}
