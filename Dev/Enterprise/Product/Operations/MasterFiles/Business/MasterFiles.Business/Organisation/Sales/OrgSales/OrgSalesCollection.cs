using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCollection : BusinessObjectCollection<OrgSales>
	{
		public OrgSalesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSalesCollection(OrgHeader master)
			: base(master.Factory)
		{
			this.master = master;
		}

		#region Master

		public OrgHeader Master
		{
			get { return master; }
		}

		readonly OrgHeader master;

		protected override ZQuery CreateRelationshipFilter()
		{
			return GetRelationshipFilter(Master);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Not required with sql string")]
		public static ZQuery GetRelationshipFilter(OrgHeader master)
		{
			ZQuery filter;

			if (master != null)
			{
				filter = new ZDBOnlyQuery(typeof(OrgSales));

				var masterFilterSQL =
$@"{OrgSalesSchema.Constants.PK} IN
(
	SELECT {OrgSalesSchema.Constants.PK} FROM {OrgSalesSchema.Constants.SqlSchemaName}.{OrgSalesSchema.Constants.TableName} WHERE OW_OH_Primary = @OrgPK
	UNION ALL
	SELECT {OrgSalesSchema.Constants.PK} FROM {OrgSalesSchema.Constants.SqlSchemaName}.{OrgSalesSchema.Constants.TableName} WHERE OW_OH_Buyer = @OrgPK
	UNION ALL
	SELECT {OrgSalesSchema.Constants.PK} FROM {OrgSalesSchema.Constants.SqlSchemaName}.{OrgSalesSchema.Constants.TableName} WHERE OW_OH_Supplier = @OrgPK
	UNION ALL
	SELECT {OrgTradeDetailSchema.Constants.PA_OW} 
	FROM {OrgTradeDetailSchema.Constants.SqlSchemaName}.{OrgTradeDetailSchema.Constants.TableName} 
	WHERE {OrgTradeDetailSchema.Constants.PK} IN 
	(
		SELECT {OrgTradePeriodSchema.Constants.PAS_PA} 
		FROM {OrgTradePeriodSchema.Constants.SqlSchemaName}.{OrgTradePeriodSchema.Constants.TableName} 
		WHERE {OrgTradePeriodSchema.Constants.PAS_OH_Client} = @OrgPK
	)
)";
				filter.AddFilterAndZSQLParameterCollection(masterFilterSQL, new ZSqlParameterCollection(ZSqlParameter.New("@OrgPK", master.PK, OrgHeaderSchema.PK)));
			}
			else
			{
				filter = new ZQuery();
			}

			AddIsNotHiddenFilter(filter);

			return filter;
		}

		public static void AddIsNotHiddenFilter(ZQuery query)
		{
			var notHiddenPart = new ZQuery(OrgSalesSchema.OW_OH_Primary, SQLComparisonOperator.NotEqual, OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation);
			notHiddenPart.AddToFilter(JoinCondition.Or, OrgSalesSchema.OW_OH_Primary, SQLComparisonOperator.Equal, null);

			query.AddToFilter(notHiddenPart);
		}

		protected override bool FetchOnlyFromLocalCache
		{
			get { return Master != null && !Master.IsInDatabase; }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (master != null)
			{
				var childSales = (OrgSales)child;
				using (SuspendListChanged())
				using (SuspendSettingHasChanges())
				{
					childSales.OW_OH_Primary = master.PK;
				}
			}
		}

		#endregion

		#region Sales Calls Integration

		public void SetTradeLanesAsChanged()
		{
			OnTradeLanesChanged?.Invoke(this, EventArgs.Empty);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			OnTradeLanesChanged?.Invoke(this, EventArgs.Empty);
		}

		internal event EventHandler OnTradeLanesChanged;

		#endregion
	}
}
