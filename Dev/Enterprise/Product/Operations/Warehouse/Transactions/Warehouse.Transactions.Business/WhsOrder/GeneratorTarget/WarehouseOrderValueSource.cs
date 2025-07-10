using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class WarehouseOrderValueSource : IEnumerable<INumberGeneratorValueProvider>
	{
		public WarehouseOrderValueSource(WhsOrder order)
		{
			Order = Argument.NotNull(order, nameof(order));

			Providers = new NumberGeneratorValueProviderCollection
			{
				new NumberGeneratorValueProvider(Keys.WarehouseSalesChannelCode, GetSalesChannelCode),
			};
		}

		#region IEnumerable<INumberGeneratorValueProvider>

		public IEnumerator<INumberGeneratorValueProvider> GetEnumerator()
		{
			return Providers.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region SalesChannelCode

		string GetSalesChannelCode(NumberGenerator generator, string detail)
		{
			return Order.SalesChannelCode.Left(new ZInt(detail));
		}

		#endregion

		NumberGeneratorValueProviderCollection Providers { get; }

		WhsOrder Order { get; }
	}
}
