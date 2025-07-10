using System;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IBaseInvoiceHeader
	{
		ZGuid PK { get; }
		ZShort JZ_InvoiceDisplaySequence { get; }
		ZString JZ_InvoiceNumber { get; }
		ZString JZ_IncoTerm { get; }
		IComparable OrderByColumn { get; }
	}
}
