using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPutawayLineCollection : ActiveBusinessObjectCollection<WhsPutawayLine>
	{
		public WhsPutawayLineCollection(WhsPutawayJob putawayJob)
			: base(putawayJob.Factory, putawayJob, null, WhsPutawayLineSchema.WPL_WPJ_PutawayJob)
		{
		}
	}
}
