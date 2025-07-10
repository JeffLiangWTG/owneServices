using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	class OrgSalesProspectCompanyBulkPopulater
	{
		public OrgSalesProspectCompanyBulkPopulater(BusinessObjectFactory factory, ZGuid viewpointOrgPk)
		{
			this.factory = factory;
			this.viewpointOrgPk = viewpointOrgPk;
		}

		readonly BusinessObjectFactory factory;
		readonly ZGuid viewpointOrgPk;

		class Schema
		{
			public const string VSP_ID = "VSP_ID";
			public const string VSP_GC = "VSP_GC";
			public const string VSP_TableCode = "VSP_TableCode";
		}

		#region Execute

		public void Execute(IEnumerable<EntitySalesWrapper> prospectSales)
		{
			if (!prospectSales.Any() || viewpointOrgPk.IsEmpty)
			{
				return;
			}

			var companyDictionary = BuildDictionary();

			foreach (var sales in prospectSales)
			{
				factory.AddFetchHint(OrgTradeDetailSchema.PA_OW, sales.PK);
			}

			foreach (var sales in prospectSales)
			{
				if (companyDictionary.TryGetValue(new Tuple<ZGuid, string>(sales.PK, OrgSalesSchema.Constants.Prefix), out List<ZGuid> salesCompanyPks))
				{
					sales.ProspectCompanyPks = salesCompanyPks;
				}

				foreach (var detail in sales.EntityTradeDetails)
				{
					if (companyDictionary.TryGetValue(new Tuple<ZGuid, string>(detail.PK, OrgTradeDetailSchema.Constants.Prefix), out List<ZGuid> detailCompanyPks))
					{
						detail.ProspectCompanyPks = detailCompanyPks;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		Dictionary<Tuple<ZGuid, string>, List<ZGuid>> BuildDictionary()
		{
			var companyDictionary = new Dictionary<Tuple<ZGuid, string>, List<ZGuid>>();

			var loadSql = "SElECT VSP_ID, VSP_GC, VSP_TableCode FROM dbo.ViewSalesProspectCompany WHERE VSP_OH = @OrgPk";

			using (var command = Db.Connection.Command(loadSql))
			{
				command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, viewpointOrgPk.ToGuid());

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var salesValueId = (Guid)reader[Schema.VSP_ID];
						var companyPk = (reader[Schema.VSP_GC] == DBNull.Value ? ZGuid.Empty : (Guid)reader[Schema.VSP_GC]);
						var tableCode = (string)reader[Schema.VSP_TableCode];

						var key = new Tuple<ZGuid, string>(salesValueId, tableCode);
						if (!companyDictionary.TryGetValue(key, out List<ZGuid> companyPks))
						{
							companyPks = new List<ZGuid>();
							companyDictionary[key] = companyPks;
						}

						companyPks.Add(companyPk);
					}
				}
			}

			return companyDictionary;
		}

		#endregion
	}
}
