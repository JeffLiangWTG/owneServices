using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NO.Business;

sealed class JobComInvoiceHeaderFetchStrategy(JobComInvoiceHeader invoiceHeader) : Customs.Business.FetchStrategies.BaseJobComInvoiceHeaderFetchStrategy(invoiceHeader)
{
	protected override void FetchForViewCore(TableColumn[] columns)
	{
		base.FetchForViewCore(columns);

		var fetchHints = new Dictionary<string, Lazy<Action>>()
		{
			{ JobComInvoiceHeader.Schema.EffectiveValuationDate, new (() => () => Factory.AddFetchHint(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_JZ, BusinessObject.PK)) }
			// TODO: add missing fetch hints (See: JobComInvoiceHeaderTest.TestCalcPropertiesWithDbHitsUseFetchHints)
		};

		foreach (var column in columns)
		{
			if (fetchHints.TryGetValue(column.ColumnName, out var fetchHint))
			{
				fetchHint.Value.Invoke();
			}
		}
	}
}
