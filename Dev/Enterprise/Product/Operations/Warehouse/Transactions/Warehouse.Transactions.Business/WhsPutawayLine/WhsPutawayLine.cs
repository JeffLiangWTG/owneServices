using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPutawayLine : AutoWhsPutawayLine
	{
		public WhsPutawayLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region PutawayJob

		public WhsPutawayJob PutawayJob => Factory.Load<WhsPutawayJob>(WPL_WPJ_PutawayJob);

		#endregion

		#endregion

		#region Properties

		[RelatedBusinessObject("PutawayJob")]
		public override ZGuid WPL_WPJ_PutawayJob
		{
			get => base.WPL_WPJ_PutawayJob;
			set => base.WPL_WPJ_PutawayJob = value;
		}

		#endregion

		#region Finalize

		public void FinalizeLine()
		{
			WPL_IsFinalized = true;
			WPL_IsPuttingAway = false;
		}

		#endregion
	}
}
