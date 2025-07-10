using System;
using System.Collections.Generic;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IReleaseLinesOnOrderCollection
	{
		int Count { get; }
		IEnumerable<WhsReleaseLine> Typed { get; }

		event EventHandler CountChanged;
	}
}
