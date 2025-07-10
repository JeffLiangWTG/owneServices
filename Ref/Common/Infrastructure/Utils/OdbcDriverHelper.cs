using Microsoft.Win32;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public class OdbcDriverHelper : IOdbcDriverHelper
	{
		public string GetMicrosoftAccessDriver()
		{
			return GetMicrosoftAccessDriverCore(RegistryKeyFor64Bit);
		}

		static string GetMicrosoftAccessDriverCore(string registrySubKey)
		{
			using (var localMachine = Registry.LocalMachine)
			using (var registryKeys = localMachine.OpenSubKey(registrySubKey))
			{
				var drivers = registryKeys?.GetValueNames();
				if (drivers == null)
				{
					return null;
				}

				foreach (var driver in drivers)
				{
					if (driver.StartsWith("Microsoft Access Driver (", System.StringComparison.InvariantCultureIgnoreCase))
					{
						return driver;
					}
				}
			}

			return null;
		}

		const string RegistryKeyFor64Bit = @"SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers";
	}
}
