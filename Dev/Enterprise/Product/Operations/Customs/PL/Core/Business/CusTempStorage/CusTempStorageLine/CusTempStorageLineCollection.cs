using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.PL.Business.CusTempStorage;

public class CusTempStorageLineCollection : CusTempStorageLineCollection<CusTempStorageLine, CusTempStorageDec>
{
	public CusTempStorageLineCollection(CusTempStorageDec parentStorageDec) : base(parentStorageDec)
	{
	}
}
