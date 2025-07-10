using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business.CusTempStorage;

public class CusTempStorageLineItem : EU.Business.CusTempStorage.CusTempStorageLineItem
	, Integration.Customs.PL.ICusTempStorageLineItem
{
	public CusTempStorageLineItem(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}
}
