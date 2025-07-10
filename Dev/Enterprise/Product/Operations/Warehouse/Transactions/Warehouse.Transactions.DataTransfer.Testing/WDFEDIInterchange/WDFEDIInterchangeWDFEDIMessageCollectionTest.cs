using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	[TestedType(typeof(WDFEDIInterchangeWDFEDIMessageCollection))]
	public class WDFEDIInterchangeWDFEDIMessageCollectionTest : EDIInterchangeEDIMessageCollectionTest
	{
		#region Implementaiton

		protected override EDIInterchangeEDIMessageCollection GetNewMessageCollection(EDIInterchange interchange)
		{
			return new WDFEDIInterchangeWDFEDIMessageCollection((WDFEDIInterchange)interchange, Factory);
		}

		protected override EDIInterchange GetNewInterchange()
		{
			return Factory.New<WDFEDIInterchange>();
		}

		#endregion
	}
}
