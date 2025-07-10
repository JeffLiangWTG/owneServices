using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class QueryTariffOption : NonPersistentBusinessObject, IObsoleteValidation
	{
		public QueryTariffOption(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public QueryTariffsCollection TariffsToQuery
		{
			get { return tariffCollection ?? (tariffCollection = new QueryTariffsCollection(Factory)); }
		}
		QueryTariffsCollection tariffCollection;

		#region Message Sending

		public void SendQuery()
		{
			List<TariffDate> tariffsQuerying = new List<TariffDate>();

			foreach (QueryTariff tariffToQuery in TariffsToQuery)
			{
				ZDate asOfDate = tariffToQuery.AsOfDate.IsEmpty ? ZDate.Today : (ZDate)tariffToQuery.AsOfDate;
				TariffDate tariffDate = new TariffDate(tariffToQuery.FromTariff, tariffToQuery.ToTariff, asOfDate);
				tariffsQuerying.Add(tariffDate);
			}

			new ReferenceFileRequester().RequestTariffs(tariffsQuerying, null);
		}

		#endregion
	}
}
