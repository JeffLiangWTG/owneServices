using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class SharedCusPermitLineTransactionTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding() => null;

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row != null)
			{
				var permitHeaderPk = new ZGuid(row[CusPermitLineTransactionSchema.Constants.CPL_CPH_PermitHeader]);
				var permit = factory.Load<SharedCusPermitHeader>(permitHeaderPk);
				if (permit != null)
				{
					result = permit.GetTransactionType();
				}
			}

			return result ?? GetTypeForNew();
		}

		public override Type GetTypeForNew() => typeof(BaseCusPermitLineTransaction);
	}
}
