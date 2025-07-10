using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class SharedCusPermitRuleTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var permitHeaderPk = new ZGuid(row[CusPermitRuleSchema.Constants.CPR_CPH_PermitHeader]);
				var permit = factory.Load<SharedCusPermitHeader>(permitHeaderPk);
				if (permit != null)
				{
					result = permit.GetRuleType();
				}
			}

			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForNew() => typeof(BaseCusPermitRule);
	}
}
