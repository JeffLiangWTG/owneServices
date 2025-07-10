using System.Data;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefExchangeRateModuleForTest : RefExchangeRateModule
	{
		public DataRow FilterOnMultiRowResultExploded(DataTable table) => base.FilterOnMultiRowResult(table);
	}
}
