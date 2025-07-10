using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRuleTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(AccCommissionRule);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var hasGroup = row[AccCommissionRuleSchema.Constants.ACM_GG] != DBNull.Value;
			var hasStaff = (string)row[AccCommissionRuleSchema.Constants.ACM_GS_NKStaff] != ZString.Empty;
			if (hasStaff)
			{
				return typeof(AccStaffCommissionRule);
			}
			else if (hasGroup)
			{
				return typeof(AccGroupCommissionRule);
			}
			else
			{
				return typeof(AccCommissionRule);
			}
		}

		public override Type GetTypeForNew()
		{
			return typeof(AccCommissionRule);
		}
	}
}
