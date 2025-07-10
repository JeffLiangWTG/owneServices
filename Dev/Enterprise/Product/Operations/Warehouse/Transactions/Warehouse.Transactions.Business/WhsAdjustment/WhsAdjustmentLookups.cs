using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdjustmentLookups : WhsDocketLookups
	{
		public WhsAdjustmentLookups(WhsAdjustment parent)
			: base(parent)
		{
		}

		#region SubTypes

		protected override CodeDescriptionPairList SubTypesCore => new AdjustmentType();

		#endregion
	}
}
