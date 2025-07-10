using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	public class TemperatureWrapper : ITemperatureRequirements
	{
		public TemperatureWrapper(ZDecimal store, ZDecimal min, ZDecimal max)
		{
			storageTemp = store;
			minStorageTemp = min;
			maxStorageTemp = max;
		}
		readonly ZDecimal storageTemp;
		readonly ZDecimal minStorageTemp;
		readonly ZDecimal maxStorageTemp;
		const string tempUnit = "CEL";

		public ZDecimal StorageTemp
		{
			get { return storageTemp; }
		}

		public ZString StorageTempUnit
		{
			get { return tempUnit; }
		}

		public ZDecimal MinStorageTemp
		{
			get { return minStorageTemp; }
		}

		public ZString MinStorageTempUnit
		{
			get { return tempUnit; }
		}

		public ZDecimal MaxStorageTemp
		{
			get { return maxStorageTemp; }
		}

		public ZString MaxStorageTempUnit
		{
			get { return tempUnit; }
		}
	}
}
