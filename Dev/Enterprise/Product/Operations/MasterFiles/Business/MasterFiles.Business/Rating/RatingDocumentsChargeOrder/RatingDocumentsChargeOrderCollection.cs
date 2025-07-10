using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RatingDocumentsChargeOrderCollection : ActiveBusinessObjectCollection<RatingDocumentsChargeOrder>
	{
		public RatingDocumentsChargeOrderCollection(BusinessObjectFactory factory, OrgHeader master)
			: base(factory)
		{
			var sQL = string.Format("SELECT {0} FROM {1} WHERE {2} = @Company",
				AccChargeCodeSchema.Constants.PK,
				AccChargeCodeSchema.Constants.TableName,
				AccChargeCodeSchema.Constants.AC_GC);
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccChargeCodeSchema.AC_GC);
			var chargeCodes = new DynamicBusinessObjectCollection(factory);
			chargeCodes.Load(sQL, parameters);
			var chargeCodePKs = new List<ZGuid>();
			foreach (DynamicBusinessObject chargeCode in chargeCodes)
			{
				chargeCodePKs.Add((ZGuid)chargeCode[AccChargeCodeSchema.Constants.PK]);
			}

			chargeCodePKs.AddRange(new[] { ZGuid.Empty, ZGuid.Invalid, ZGuid.Missing });
			var accClientRatingDocumentsChargeOrderQuery = new ZQuery();
			accClientRatingDocumentsChargeOrderQuery.AddToFilter(RatingDocumentsChargeOrderSchema.RCO_OH_Client, master.PK);
			var chargeCodeQuery = new ZQuery(RatingDocumentsChargeOrderSchema.RCO_AC_ChargeCode, chargeCodePKs);
			accClientRatingDocumentsChargeOrderQuery.AddToFilter(chargeCodeQuery);
			AdditionalFilter = accClientRatingDocumentsChargeOrderQuery;

			fMaster = master;
		}

		readonly OrgHeader fMaster;

		protected override void SetDefaultsForNewElementCore(RatingDocumentsChargeOrder newElement)
		{
			newElement.RCO_OH_Client = fMaster.PK;
		}
	}
}
