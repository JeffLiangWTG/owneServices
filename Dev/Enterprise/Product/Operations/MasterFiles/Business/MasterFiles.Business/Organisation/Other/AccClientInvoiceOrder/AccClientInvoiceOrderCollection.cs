using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccClientInvoiceOrderCollection : ActiveBusinessObjectCollection<AccClientInvoiceOrder>
	{
		public AccClientInvoiceOrderCollection(BusinessObjectFactory factory, OrgHeader master)
			: base(factory)
		{
			string sQL = string.Format("SELECT {0} FROM {1} WHERE {2} = @Company",
				AccChargeCodeSchema.Constants.PK,
				AccChargeCodeSchema.Constants.TableName,
				AccChargeCodeSchema.Constants.AC_GC);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccChargeCodeSchema.AC_GC);
			DynamicBusinessObjectCollection chargeCodes = new DynamicBusinessObjectCollection(factory);
			chargeCodes.Load(sQL, parameters);
			List<ZGuid> chargeCodePKs = new List<ZGuid>();
			foreach (DynamicBusinessObject chargeCode in chargeCodes)
			{
				chargeCodePKs.Add((ZGuid)chargeCode[AccChargeCodeSchema.Constants.PK]);
			}

			chargeCodePKs.AddRange(new[] { ZGuid.Empty, ZGuid.Invalid, ZGuid.Missing });
			ZQuery accClientInvoiceOrderQuery = new ZQuery();
			accClientInvoiceOrderQuery.AddToFilter(AccClientInvoiceOrderSchema.AI_OH_Client, master.PK);
			ZQuery chargeCodeQuery = new ZQuery(AccClientInvoiceOrderSchema.AI_AC, chargeCodePKs);
			accClientInvoiceOrderQuery.AddToFilter(chargeCodeQuery);
			AdditionalFilter = accClientInvoiceOrderQuery;

			fMaster = master;
		}

		readonly OrgHeader fMaster;

		protected override void SetDefaultsForNewElementCore(AccClientInvoiceOrder newElement)
		{
			newElement.AI_OH_Client = fMaster.PK;
		}
	}
}
