using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class GlbExternalPasswordTypeDecider : TypeDecider
	{
		public override Type GetTypeForNew() => null;

		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var staffKey = (row != null) ? new ZGuid(row[GlbExternalPassword.Schema.GP_GS]) : ZGuid.Empty;
			return staffKey.IsEmpty ? typeof(GlbCompanyCredential) : typeof(GlbStaffCredential);
		}
	}
}
