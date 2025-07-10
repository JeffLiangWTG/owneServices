using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.ISF.Business
{
	static class ManufacturerDataGenerator
	{
		public static IEnumerable<IManufacturerData> Generate(CusISFLineCollection collection, TariffDataCollection.MergeStyle mergeStyle)
		{
			Dictionary<ZGuid, ManufacturerData> dictionary = new Dictionary<ZGuid, ManufacturerData>();
			foreach (CusISFLine line in collection)
			{
				ManufacturerData data;
				if (!dictionary.TryGetValue(line.BL_ManufacturerDocAddressPK, out data))
				{
					data = new ManufacturerData(line.ManufacturerDocAddress, mergeStyle);
					dictionary.Add(line.BL_ManufacturerDocAddressPK, data);
				}
				data.Tariffs.Add(line.BL_RN_NKGoodsOrigin, line.HarmonisedNumToReportToCustoms);
			}
			return new TypedEnumerable<IManufacturerData>(dictionary.Values);
		}

		class ManufacturerData : IManufacturerData
		{
			public ManufacturerData(ISFDocAddress manufacturer, TariffDataCollection.MergeStyle mergeStyle)
			{
				if (manufacturer != null)
				{
					this.manufacturer = new SanitizedISFDocAddressWrapper(manufacturer);
				}
				this.mergeStyle = mergeStyle;
			}

			readonly IISFDocAddress manufacturer;
			readonly TariffDataCollection.MergeStyle mergeStyle;

			public TariffDataCollection Tariffs
			{
				get { return tariffs ?? (tariffs = new TariffDataCollection(mergeStyle)); }
			}
			TariffDataCollection tariffs;

			#region IManufacturerData Members

			public IISFDocAddress Manufacturer
			{
				get { return manufacturer; }
			}

			IEnumerable<ITariffData> IManufacturerData.Tariffs
			{
				get { return Tariffs; }
			}

			#endregion
		}
	}
}
