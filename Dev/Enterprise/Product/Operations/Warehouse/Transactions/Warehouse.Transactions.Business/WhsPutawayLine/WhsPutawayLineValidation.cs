using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPutawayLineValidation : AutoWhsPutawayLineValidation
	{
		public WhsPutawayLineValidation(AutoWhsPutawayLine parent)
			: base(parent)
		{
		}

		protected new WhsPutawayLine Parent
		{
			get { return (WhsPutawayLine)base.Parent; }
		}

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			=> WhsPutawayLineSchema.Constants.WPL_WPJ_PutawayJob != info.Name && base.ShouldValidateFKToCancelledRecord(info);

		#endregion
	}
}
