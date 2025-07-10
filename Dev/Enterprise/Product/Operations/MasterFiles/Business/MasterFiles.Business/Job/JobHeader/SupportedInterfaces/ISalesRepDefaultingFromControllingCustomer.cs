using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface ISalesRepDefaultingFromControllingCustomer
	{
		ZBool IsAllowedToDefaultSalesRepFromControllingCustomer { get; }

		void NotifyControllingCustomerChanged(EventHandler onChangedAction);
	}
}
