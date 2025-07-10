using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class WarehouseJobValueSource : IEnumerable<INumberGeneratorValueProvider>
	{
		public WarehouseJobValueSource(WhsDocket docket)
		{
			Docket = Argument.NotNull(docket, nameof(docket));

			Providers = new NumberGeneratorValueProviderCollection
			{
				new NumberGeneratorValueProvider(Keys.WarehouseCode, GetWarehouseCode),
				new NumberGeneratorValueProvider(Keys.WarehouseClientCode, GetClientCode),
				new NumberGeneratorValueProvider(Keys.WarehouseSubType, GetSubType),
			};
		}

		#region WarehouseCode

		string GetWarehouseCode(NumberGenerator generator, string detail)
		{
			return Docket.Warehouse.WW_WarehouseCode.Left(new ZInt(detail));
		}

		#endregion

		#region ClientCode

		string GetClientCode(NumberGenerator generator, string detail)
		{
			return Docket.Client.OH_Code.Left(new ZInt(detail));
		}

		#endregion

		#region SubType

		string GetSubType(NumberGenerator generator, string detail)
		{
			return Docket.WD_DocketSubType;
		}

		#endregion

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

		NumberGeneratorValueProviderCollection Providers { get; }
		WhsDocket Docket { get; }
	}
}
