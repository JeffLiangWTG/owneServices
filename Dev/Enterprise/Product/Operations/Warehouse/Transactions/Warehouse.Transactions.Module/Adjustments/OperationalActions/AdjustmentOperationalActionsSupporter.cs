using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class AdjustmentOperationalActionSupporter : WhsOperationalActionSupporter
	{
		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsAdjustment;

		public override BusinessContext BusinessContext => BusinessContext.WhsAdjustment;

		public override Type RootType => typeof(WhsAdjustment);

		#region SingularElementNoun

		public override string SingularElementNoun => Res.GetString("Warehouse|AdjustmentOperationalActionSupporter|SingularElementNoun", "Adjustment");

		#endregion

		#region PluralElementNoun

		public override string PluralElementNoun => Res.GetString("Warehouse|AdjustmentOperationalActionSupporter|PluralElementNoun", "Adjustments");

		#endregion
	}
}
