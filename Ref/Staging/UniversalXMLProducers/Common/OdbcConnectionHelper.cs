using System.Data.Odbc;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common
{
	public static class OdbcConnectionHelper
	{
		static readonly IOdbcDriverHelper OdbcDriverHelper = new OdbcDriverHelper();

		public static OdbcConnection GetOdbcConnection(string mdbFilePath)
		{
			var driver = OdbcDriverHelper.GetMicrosoftAccessDriver();

			var connectionString = $"Driver={{{driver}}};Dbq={mdbFilePath};";
			return  new OdbcConnection(connectionString);
		}
	}
}
