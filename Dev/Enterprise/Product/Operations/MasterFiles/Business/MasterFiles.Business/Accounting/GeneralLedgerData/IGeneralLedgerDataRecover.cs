using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.MasterFiles.Business
{
	public interface IGeneralLedgerDataRecover
	{
		void RecoverPartOfGLD(SchemaColumn pkSchemaColumn, BusinessObject[] recoverGldTransactions);
	}
}
