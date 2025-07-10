using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class BaseJobComInvHeaderChargeTypeDecider : TypeDecider, Integration.Customs.IBaseJobComInvHeaderChargeTypeDecider
	{
		//Needed as it is abstract, but should not be hit as AllCharges is not exposed to users and a specific type is specified at each charge collection
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;

			if (row != null)
			{
				ZString parentTableName = new ZString(row[JobComInvHeaderChargeSchema.J7_ParentTableCode.Name]);
				ZBool isApportioned = new ZBool(row[JobComInvHeaderChargeSchema.J7_IsApportionedCharge.Name]);
				ZGuid parentPK = new ZGuid(row[JobComInvHeaderChargeSchema.J7_ParentID.Name]);

				if (parentTableName == JobComInvoiceHeaderSchema.Constants.Prefix)
				{
					var groupOrInvoice = factory.Load<CommonJobComInvoiceHeader>(parentPK);
					if (groupOrInvoice != null)
					{
						if (groupOrInvoice.JZ_GroupInvoice)
						{
							result = BaseGroupInvoiceCharge.TypeDecider.GetTypeForLoad(row, factory);
						}
						else
						{
							result = isApportioned ? BaseApportionedCharge.TypeDecider.GetTypeForLoad(row, factory) : BaseInvoiceCharge.TypeDecider.GetTypeForLoad(row, factory);
						}
					}
				}
				else if (parentTableName == JobComInvoiceLineSchema.Constants.Prefix)
				{
					result = isApportioned ? BaseInvoiceLineApportionedCharge.TypeDecider.GetTypeForLoad(row, factory) : BaseInvoiceLineCharge.TypeDecider.GetTypeForLoad(row, factory);
				}
			}
			return result;
		}
	}
}
