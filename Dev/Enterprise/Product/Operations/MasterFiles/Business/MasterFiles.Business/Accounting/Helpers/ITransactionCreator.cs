using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface ITransactionCreator
	{
		AccTransactionHeader CreateTransaction(BusinessObjectFactory factory, string ledger, string transactionType, Guid? initialPK = null, int numberOfLines = 1);
		AccTransactionHeader CreateInvoiceBatch(BusinessObjectFactory factory, params AccTransactionHeader[] headers);
	}
}
