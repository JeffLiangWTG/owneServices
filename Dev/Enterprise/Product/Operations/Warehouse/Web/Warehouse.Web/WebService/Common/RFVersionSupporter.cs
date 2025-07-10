using System;
using System.IO;

namespace Enterprise.Warehouse.Web.WebService.Common
{
	public class RFVersionSupporter : IRFVersionSupporter
	{
		public string GetAndroidWebServiceVersion(string versionFilePath)
		{
			try
			{
				using (var reader = new StreamReader(versionFilePath))
				{
					var version = reader.ReadToEnd().Trim();
					if (string.IsNullOrWhiteSpace(version))
					{
						throw new InvalidOperationException("Unable to determine local Android version.");
					}
					return version;
				}
			}
			catch (IOException ex)
			{
				throw new InvalidOperationException(FormattableString.Invariant($"Unable to determine local Android version. The following exception was thrown:\r\n{ex.Message}"), ex);
			}
		}
	}
}
