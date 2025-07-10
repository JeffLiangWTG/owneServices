using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketJobDatesProvider : JobDatesProvider<WhsDocket>
	{
		public WhsDocketJobDatesProvider(WhsDocket whsDocket)
			: base(whsDocket) { }

		protected override ZDateTime GetArrivalDateCore()
		{
			return GetWhsDocketJobDate();
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			return GetWhsDocketJobDate();
		}

		ZDateTime GetWhsDocketJobDate()
		{
			return Parent.WD_FinalisedDate.IsValid ? Parent.WD_FinalisedDate.ToZDateTime() : ZDateTime.Today;
		}
	}
}
