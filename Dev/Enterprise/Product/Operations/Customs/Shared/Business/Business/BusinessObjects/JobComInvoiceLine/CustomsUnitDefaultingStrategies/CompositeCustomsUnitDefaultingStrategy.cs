using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Customs.Business
{
	public class CompositeCustomsUnitDefaultingStrategy : ICustomsUnitDefaultingStrategy
	{
		readonly List<ICustomsUnitDefaultingStrategy> strategies;

		public CompositeCustomsUnitDefaultingStrategy(params ICustomsUnitDefaultingStrategy[] strategies)
		{
			this.strategies = Argument.NotNull(strategies, nameof(strategies)).ToList();
			Argument.GreaterThanOrEqual(strategies.Length, 2, $"{nameof(strategies)}.{nameof(strategies.Length)}");
		}

		public void Initialise(BaseJobComInvoiceLine invoiceLine)
		{
			foreach (var customsUnitDefaultingStrategy in strategies)
			{
				customsUnitDefaultingStrategy?.Initialise(invoiceLine);
			}
		}

		public void Deinitialise(BaseJobComInvoiceLine invoiceLine)
		{
			foreach (var customsUnitDefaultingStrategy in strategies)
			{
				customsUnitDefaultingStrategy?.Deinitialise(invoiceLine);
			}
		}

		public void DefaultUOMs(BaseJobComInvoiceLine invoiceLine)
		{
			foreach (var customsUnitDefaultingStrategy in strategies)
			{
				customsUnitDefaultingStrategy?.DefaultUOMs(invoiceLine);
			}
		}
	}
}
