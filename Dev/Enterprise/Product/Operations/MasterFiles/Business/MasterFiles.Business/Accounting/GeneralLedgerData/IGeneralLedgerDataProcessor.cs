using System.Data;

namespace Enterprise.MasterFiles.Business
{
	public interface IGeneralLedgerDataProcessor
	{
		void ProcessData(DataRow[] gLDDataSources);
	}
}
