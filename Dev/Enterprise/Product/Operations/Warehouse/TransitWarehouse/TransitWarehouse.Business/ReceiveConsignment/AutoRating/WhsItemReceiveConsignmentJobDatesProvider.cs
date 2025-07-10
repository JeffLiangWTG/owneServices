using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveConsignmentJobDatesProvider : JobDatesProvider<WhsItemReceiveConsignment>
	{
		public WhsItemReceiveConsignmentJobDatesProvider(WhsItemReceiveConsignment parent) : base(parent)
		{
		}

		readonly Dictionary<ZString, ZDateTime> dates = new Dictionary<ZString, ZDateTime>();

		public void SetDate(ZString jobDateType, ZDateTime date)
		{
			if (!dates.ContainsKey(jobDateType))
			{
				dates.Add(jobDateType, date);
			}
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return dates.ContainsKey(JobDateTypes.Codes.DepartureDate) ? dates[JobDateTypes.Codes.DepartureDate] : ZDateTime.Empty;
		}

		protected override ZDateTime GetArrivalDateCore()
		{
			return dates.ContainsKey(JobDateTypes.Codes.ArrivalDate) ? dates[JobDateTypes.Codes.ArrivalDate] : ZDateTime.Empty;
		}
	}
}
