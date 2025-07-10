using System;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsTransferLine))]
	class WhsTransferLineWithMatchingLinesTest : LineWithMatchingLinesTestCase<WhsTransferLine>
	{
		#region TestParentDocketPK_MatchesParentDocket

		protected override Type DocketType
		{
			get { return typeof(WhsTransfer); }
		}

		#endregion

		#region TestProductPK_MatchesProduct

		protected override void SetProductPK(ILineWithInventory l, OrgSupplierPart part)
		{
			WhsTransferLine line = (WhsTransferLine)l;

			line.WE_OP = part.PK;
		}

		#endregion
	}
}
