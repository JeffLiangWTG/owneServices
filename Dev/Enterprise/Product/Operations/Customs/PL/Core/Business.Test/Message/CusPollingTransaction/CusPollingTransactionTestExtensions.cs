using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.Business.Testing;

public static class CusPollingTransactionTestExtensions
{
	public static CusPollingTransaction[] GetBacklogs(this CusPollingTransaction plcTransaction)
	{
		var query = new ZQuery { OrderBy = CusPollingTransactionSchema.CPT_StatusTimeUtc.Name + OrderByClause.Ascending }
			.AddToFilter(CusPollingTransactionSchema.CPT_ApplicationCode, ApplicationCodeList.Codes.PLCustoms)
			.AddToFilter(CusPollingTransactionSchema.CPT_ParentID, plcTransaction.CPT_ParentID)
			.AddToFilter(CusPollingTransactionSchema.CPT_Type, Core.Constants.Customs.CusPollingTransactionType.Codes.BLG);
		return plcTransaction.Factory.Load<CusPollingTransaction>(query);
	}

	public static EDIMessage[] GetMessages(this CusPollingTransaction plcTransaction)
	{
		var query = new ZQuery { OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name + OrderByClause.Ascending }
			.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.PLCustoms)
			.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, plcTransaction.PK);
		return plcTransaction.Factory.Load<EDIMessage>(query);
	}
}
