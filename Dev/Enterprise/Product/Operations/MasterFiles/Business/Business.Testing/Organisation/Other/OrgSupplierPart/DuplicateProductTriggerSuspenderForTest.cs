using System;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class DuplicateProductTriggerSuspenderForTest
	{
		public static IDisposable Suspend()
		{
			return new DisposableAction(() =>
			{
				Db.Connection.ExecuteNonQuery(@"
					DISABLE TRIGGER TG_OrgSupplierPart_DuplicateCheck ON OrgSupplierPart;
					DISABLE TRIGGER TG_OrgPartRelation_DuplicateCheck ON OrgPartRelation;
				");
			}, () =>
			{
				Db.Connection.ExecuteNonQuery(@"
					ENABLE TRIGGER TG_OrgSupplierPart_DuplicateCheck ON OrgSupplierPart;
					ENABLE TRIGGER TG_OrgPartRelation_DuplicateCheck ON OrgPartRelation;
				");
			});
		}
	}
}
