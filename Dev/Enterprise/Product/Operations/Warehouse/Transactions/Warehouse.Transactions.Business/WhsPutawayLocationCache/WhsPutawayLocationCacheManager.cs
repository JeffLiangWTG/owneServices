using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Statistics;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class WhsPutawayLocationCacheManager : IWhsPutawayLocationCacheManager
	{
		public static class LocationCacheType
		{
			public const string LOC = nameof(LOC);
			public const string FIX = nameof(FIX);
			public const string DYN = nameof(DYN);
			public const string PLT = nameof(PLT);
		}

		public void CreateCache(BusinessObjectFactory factory, ZGuid locationPK) => CreateCache(factory, new[] { locationPK });

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public void CreateCache(BusinessObjectFactory factory, IEnumerable<ZGuid> locationPK)
		{
			Argument.NotNull(factory, nameof(factory));

			var sql = @"EXEC WhsCreatePutawayLocationCache @Locations = @locations, @LastGeneratedDateUtc = @lastGeneratedDateUtc";

			var command = ((IDbConnected)factory).Connection.Command(sql);
			command.AddTableValuedParameter((NoResString)"@locations", WhsLocationViewSchema.PK, locationPK); // Query parameter
			command.AddParameter(ZSqlParameter.New("@lastGeneratedDateUtc", ZDateTime.UtcNow, WhsLocationViewSchema.WLV_SystemLastEditTimeUtc));
			command.ExecuteNonQuery();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL Query")]
		public IEnumerable<DataRow> GetCache(BusinessObjectFactory factory, ZGuid warehousePK, IEnumerable<ZGuid> clientPKs, IEnumerable<ZGuid> partPKs, IEnumerable<ZGuid> skipLocationPKs = null)
		{
			var whsParamName = $"@WhsPK_{ParameterSuffixer.Instance.GetParameterSuffix(ZDateTime.Now.ToDateTime(), WhsPutawayLocationCacheSchema.WPC_WW_Warehouse, warehousePK.ToGuid())}";
			var whsParameter = ZSqlParameter.New(whsParamName, warehousePK, WhsPutawayLocationCacheSchema.WPC_WW_Warehouse);

			var distinctSkipLocationPKs = skipLocationPKs?.Distinct().ToArray();
			var needSkipLocation = distinctSkipLocationPKs != null && distinctSkipLocationPKs.Length > 0;
			var sqlFilterSkipLocation = needSkipLocation ? "(WPC_WL_Location NOT IN (SELECT Value FROM @SkipLocationPKs)) AND" : string.Empty;

			var sql = Invariant($@"
SELECT 
	*
FROM
	dbo.WhsPutawayLocationCache
WHERE
	WPC_WW_Warehouse = {whsParamName} AND
	(WPC_OH_Client IS NULL OR (WPC_OH_Client IN (SELECT Value FROM @ClientPKs))) AND
	(WPC_OP_Product IS NULL OR (WPC_OP_Product IN (SELECT Value FROM @ProductPKs))) AND
	{sqlFilterSkipLocation}
	(WPC_MaxQuantity = 0 OR WPC_AvailableQuantity > 0)");  // Non-translateable SQL Query String

			void addParameters(DbCommand command)
			{
				command.AddTableValuedParameter("@ClientPKs", OrgHeaderSchema.PK, clientPKs.Distinct());
				command.AddTableValuedParameter("@ProductPKs", OrgSupplierPartSchema.PK, partPKs.Distinct());
				if (needSkipLocation)
				{
					command.AddTableValuedParameter("@SkipLocationPKs", WhsLocationSchema.PK, distinctSkipLocationPKs);
				}
				command.AddParameter(whsParameter);
			}

			return DataRowLoader.Load(factory, sql, addParameters);
		}
	}
}
