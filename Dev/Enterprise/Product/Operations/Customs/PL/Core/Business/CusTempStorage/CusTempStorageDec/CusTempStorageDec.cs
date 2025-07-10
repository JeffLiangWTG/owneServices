using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.CusTempStorage;

public class CusTempStorageDec : EU.Business.CusTempStorage.CusTempStorageDec
	, Integration.Customs.PL.ICusTempStorageDec
{
	public CusTempStorageDec(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public static CusTempStorageDec New(CusTempStorageJobHeader parent)
	{
		var result = parent.Factory.New<CusTempStorageDec>();
		using (result.SuspendSettingHasChanges())
		{
			result.STH_SJH = parent.PK;
		}
		return result;
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		STH_DeclarationType = "IST"; // TODO: proper code to be defined
		STH_SystemCreateTimeUtc = ZDateTime.Now;
	}

	protected override EU.Business.CusTempStorage.CusTempStorageLineCollection CreateNewCusTempStorageLines() => new CusTempStorageLineCollection(this);

	public new CusTempStorageLineCollection CusTempStorageLines => (CusTempStorageLineCollection)base.CusTempStorageLines;

	public new CusTempStorageJobHeader StorageHeader => base.StorageHeader as CusTempStorageJobHeader;
}
