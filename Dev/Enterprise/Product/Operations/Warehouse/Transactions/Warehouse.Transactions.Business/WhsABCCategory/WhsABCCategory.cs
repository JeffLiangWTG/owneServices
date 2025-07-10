using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsABCCategory : AutoWhsABCCategory, IWhsABCCategory
	{
		public WhsABCCategory(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region GetABCCategory

		public static WhsABCCategory GetABCCategory(BusinessObjectFactory factory, ZGuid productPK, ZGuid clientPK, ZGuid warehousePK)
		{
			Argument.NotNull(factory, nameof(factory));
			return factory.LoadTop1<WhsABCCategory>(GetABCCategoriesQuery(productPK, new[] { clientPK }, warehousePK));
		}

		internal static ZQuery GetABCCategoriesQuery(ZGuid productPK, ZGuid[] clientPKs, ZGuid warehousePK)
		{
			var abcCategoryQuery = new ZQuery(WhsABCCategorySchema.WJ_OP_Product, productPK);
			abcCategoryQuery.AddToFilter(WhsABCCategorySchema.WJ_OH_Client, clientPKs);
			abcCategoryQuery.AddToFilter(WhsABCCategorySchema.WJ_WW_Warehouse, warehousePK);
			abcCategoryQuery.OrderBy = WhsABCCategorySchema.WJ_AnalysisDateTo.Name + " desc";
			return abcCategoryQuery;
		}

		#endregion

		#region Related Entities

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WJ_WW_Warehouse); }
		}

		#endregion

		#region Properties

		// persistent

		#region BK_WW_Warehouse

		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WJ_WW_Warehouse
		{
			get { return base.WJ_WW_Warehouse; }
			set { base.WJ_WW_Warehouse = value; }
		}

		#endregion

		// calculated

		#region ABCAnalysisPeriodAsString

		public string ABCAnalysisPeriodAsString
		{
			get
			{
				if (abcAnalysisPeriodAsString == null)
				{
					if (WJ_AnalysisPeriod.IsEmpty || WJ_AnalysisDateTo.IsEmpty)
					{
						abcAnalysisPeriodAsString = "";
					}
					else
					{
						var sql = "SELECT * FROM csfn_ABCAnalysisDateRange(@DateLastRan, @Period)";
						var sqlParameters = new ZSqlParameterCollection();
						sqlParameters.Add("@DateLastRan", WJ_AnalysisDateTo, WhsABCCategorySchema.WJ_AnalysisDateTo);
						sqlParameters.Add("@Period", WJ_AnalysisPeriod, WhsABCCategorySchema.WJ_AnalysisPeriod);

						var dateRange = new DynamicBusinessObjectCollection(Factory);
						dateRange.Load(sql, sqlParameters);
						abcAnalysisPeriodAsString = Res.GetString("7fddb2a3-650d-47f1-9415-504c6a5528e1", "{0} to {1}",
						((ZDateTime)dateRange[0]["DateFrom"]).ToShortDateString(),
						((ZDateTime)dateRange[0]["DateTo"]).AddDays(-1).ToShortDateString());
					}
				}

				return abcAnalysisPeriodAsString;
			}
		}

		string abcAnalysisPeriodAsString;

		#endregion

		#endregion
	}
}
