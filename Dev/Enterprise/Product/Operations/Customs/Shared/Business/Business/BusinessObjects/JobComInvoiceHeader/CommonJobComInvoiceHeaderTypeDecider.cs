using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CommonJobComInvoiceHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(System.Data.DataRow row, BusinessObjectFactory factory)
		{
			if (new ZBool(row[JobComInvoiceHeaderSchema.JZ_GroupInvoice.Name]))
			{
				return BaseJobComInvoiceGroupHeader.TypeDecider.GetTypeForLoad(row, factory);
			}
			else
			{
				return BaseJobComInvoiceHeader.TypeDecider.GetTypeForLoad(row, factory);
			}
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
