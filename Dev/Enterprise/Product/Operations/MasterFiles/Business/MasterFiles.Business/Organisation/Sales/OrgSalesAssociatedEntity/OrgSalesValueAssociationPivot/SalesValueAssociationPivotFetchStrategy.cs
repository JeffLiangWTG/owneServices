using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class SalesValueAssociationPivotFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public SalesValueAssociationPivotFetchStrategy(OrgSalesValueAssociationPivot pivot)
			: base(pivot)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			var pivot = (OrgSalesValueAssociationPivot)BusinessObject;

			var associatedProperties = new HashSet<string>(StringComparer.Ordinal)
				{
					"AssociatedDate",
					"AssociatingUser"
				};

			if (columns.Any(x => x.ColumnName.StartsWith("AssociatedEntity", StringComparison.Ordinal) || associatedProperties.Contains(x.ColumnName)))
			{
				AddFetchHintForAccessingAssociatedEntity(pivot);
			}

			if (columns.Any(x => associatedProperties.Contains(x.ColumnName)))
			{
				if (!pivot.SVP_TradeId.IsEmpty)
				{
					Factory.AddFetchHint(OrgSalesSchema.Constants.TableName, pivot.SVP_TradeId);
					Factory.AddFetchHint(OrgTradeDetailSchema.Constants.TableName, pivot.SVP_TradeId);
				}
			}
		}

		public static void AddFetchHintForAccessingAssociatedEntity(OrgSalesValueAssociationPivot pivot)
		{
			if (!pivot.SVP_ActivityId.IsEmpty && !pivot.SVP_ActivityTableCode.IsEmpty)
			{
				var tableSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(pivot.SVP_ActivityTableCode);
				if (tableSchema != null)
				{
					pivot.Factory.AddFetchHint(tableSchema.TableName, pivot.SVP_ActivityId);
				}
			}
		}
	}
}
