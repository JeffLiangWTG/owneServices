using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public interface IGenerateQuoteForSalesValueAssociatedEntityController
	{
		ZForm ParentModalForm
		{
			get;
			set;
		}

		void Execute(ISalesValueAssociatedEntity entity);
		void Execute(ISalesValueAssociatedEntity entity, IEnumerable<OrgTradeDetail> tradeDetails);
	}
}
