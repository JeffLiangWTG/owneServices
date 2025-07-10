using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class WarehouseReceiveValueSource : IEnumerable<INumberGeneratorValueProvider>
	{
		public WarehouseReceiveValueSource(WhsReceive receive)
		{
			Receive = Argument.NotNull(receive, nameof(receive));

			Providers = new NumberGeneratorValueProviderCollection
			{
				new NumberGeneratorValueProvider(Keys.WarehouseSupplierCode, WarehouseSupplierCode),
				new NumberGeneratorValueProvider(Keys.WarehouseReceiveCategoryCode, WarehouseReceiveCategoryCode),
			};
		}

		public IEnumerator<INumberGeneratorValueProvider> GetEnumerator()
		{
			return Providers.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#region WarehouseSupplierCode

		string WarehouseSupplierCode(NumberGenerator generator, string detail)
		{
			return Receive.Supplier?.OH_Code.Left(new ZInt(detail)) ?? string.Empty;
		}

		#endregion

		#region WarehouseReceiveCategoryCode

		string WarehouseReceiveCategoryCode(NumberGenerator generator, string detail)
		{
			return Receive.WD_ReceiveCategory.Left(new ZInt(detail));
		}

		#endregion

		NumberGeneratorValueProviderCollection Providers { get; }

		WhsReceive Receive { get; }
	}
}
