using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public class CusTempStorageRegLineTransactionLookups(CusTempStorageRegLineTransaction parent) : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionLookups(parent)
{
	public override CodeDescriptionPairList TransactionTypeList => Factory.GetCachedValue<EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList>();
}
