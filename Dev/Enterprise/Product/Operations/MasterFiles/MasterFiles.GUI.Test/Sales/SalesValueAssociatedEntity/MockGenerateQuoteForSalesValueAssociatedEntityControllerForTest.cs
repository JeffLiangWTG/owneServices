using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class MockGenerateQuoteForSalesValueAssociatedEntityControllerForTest : IGenerateQuoteForSalesValueAssociatedEntityController
	{
		public ZForm ParentModalForm
		{
			get;
			set;
		}

		public void Execute(ISalesValueAssociatedEntity entity)
		{
			EntityCalled = entity;
		}

		public void Execute(ISalesValueAssociatedEntity entity, IEnumerable<OrgTradeDetail> tradeDetails)
		{
			EntityCalled = entity;
			TradeDetailsCalled = tradeDetails;
		}

		public ISalesValueAssociatedEntity EntityCalled;
		public IEnumerable<OrgTradeDetail> TradeDetailsCalled;
	}
}
