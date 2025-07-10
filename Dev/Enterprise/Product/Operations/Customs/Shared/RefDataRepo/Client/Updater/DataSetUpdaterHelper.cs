using System;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public static class DataSetUpdaterHelper
	{
		public static string GetUpdaterName(string tableName, UpdaterType updaterType)
		{
			return updaterType + "-" + tableName;
		}

		public static string GetTableName(string updaterName)
		{
			return updaterName.Substring(4);
		}

		public static UpdaterType GetUpdaterType(string updaterName)
		{
			if (updaterName.StartsWith($"{UpdaterType.REF}-"))
			{
				return UpdaterType.REF;
			}
			if (updaterName.StartsWith($"{UpdaterType.RDU}-"))
			{
				return UpdaterType.RDU;
			}
			throw new InvalidOperationException($"{updaterName} is not a valid UpdaterName. UpdaterName should starts with 'REF-' or 'RDU-'.");
		}
	}

	public enum UpdaterType
	{
		REF,
		RDU
	}
}
