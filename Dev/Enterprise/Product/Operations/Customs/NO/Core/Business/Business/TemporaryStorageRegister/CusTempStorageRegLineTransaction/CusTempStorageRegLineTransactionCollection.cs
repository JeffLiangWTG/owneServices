namespace Enterprise.Customs.NO.Business;

public class CusTempStorageRegLineTransactionCollection(CusTempStorageRegLine line) : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionCollection<CusTempStorageRegLineTransaction>(line)
{
	protected override void SetDefaultsForNewElementCore(EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransaction transaction)
	{
		base.SetDefaultsForNewElementCore(transaction);
		transaction.SRT_TransactionType = Count == 0 ?
			EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance :
			EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList.Codes.Adjustment;
	}
}
