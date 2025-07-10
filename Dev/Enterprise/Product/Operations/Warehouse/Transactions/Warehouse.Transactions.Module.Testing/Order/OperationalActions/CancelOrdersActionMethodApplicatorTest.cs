using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(CancelOrdersActionMethodApplicator))]
	class CancelOrdersActionMethodApplicatorTest : CancelDocketsActionMethodApplicatorTest<CancelOrdersActionMethodApplicator, WhsOrder>
	{
		#region TestCancelDocket

		protected override string GetExpectedMessage()
		{
			return string.Format(@"INFO: Warehouse Order W1 [HL W1] - is canceled successfully.
WARNING: Warehouse Order W2 [HL W2] - You can only cancel dockets with Entered (Saved) status
WARNING: Warehouse Order W3 [HL W3] - You can only cancel dockets with Entered (Saved) status
WARNING: Warehouse Order W4 [HL W4] - You can only cancel dockets with Entered (Saved) status
WARNING: Warehouse Order W5 [HL W5] - Warehouse Order W5 cannot be deactivated.
Accounting Transaction(s) have been saved against this Invoicing Job Header (JobNum01) in the company EDI.
");
		}

		protected override void AddDocketSpecificFetchHint(Dictionary<string, int> expectedDbHits)
		{
			expectedDbHits.Add(WhsLoadOrderSchema.Constants.TableName, 1);
		}

		#endregion

		protected override WhsDocketLine GetNewDocketLine(WhsOrder docket, OrgSupplierPart part, ZDecimal quantity)
		{
			return Helper.CreateWhsOrderLine(docket, part, quantity);
		}

		protected override void AddCustomFetchHint(Dictionary<string, int> expectedDbHits)
		{
			expectedDbHits.Add(StmEventSchema.Constants.TableName, 2);
		}
	}
}
