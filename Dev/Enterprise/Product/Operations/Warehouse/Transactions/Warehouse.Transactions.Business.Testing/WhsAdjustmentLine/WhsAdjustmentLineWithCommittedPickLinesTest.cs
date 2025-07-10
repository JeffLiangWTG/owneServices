using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdjustmentLine))]
	class WhsAdjustmentLineWithCommittedPickLinesTest : LineWithCommittedPickLinesTestCase
	{
		#region TestParentDocketPK_MatchesParentDocket

		protected override Type DocketType
		{
			get { return typeof(WhsAdjustment); }
		}

		#endregion

		#region TestProductPK_MatchesProduct

		protected override void SetProductPK(ILineWithInventory l, OrgSupplierPart part)
		{
			WhsAdjustmentLine line = (WhsAdjustmentLine)l;

			line.WE_OP = part.PK;
		}

		#endregion
	}
}
