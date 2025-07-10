using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	class TrackingShipmentFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public TrackingShipmentFetchStrategy(EnterpriseBusinessObject businessObject)
			: base(businessObject)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns1)
		{
			List<TableColumn> columns = new List<TableColumn>();
			foreach (TableColumn column in columns1)
			{
				if (string.IsNullOrEmpty(column.TableName))
				{
					if (column.ColumnName.StartsWith("JS_OH_"))
					{
						columns.Add(new TableColumn(OrgHeaderSchema.Constants.TableName, column.ColumnName));
					}
					else if (column.ColumnName == ShipmentDeclarationSchema.Constants.ReceivedDate ||
						column.ColumnName == ShipmentDeclarationSchema.Constants.ReceivedBy ||
						column.ColumnName == ShipmentDeclarationSchema.Constants.PiecesReceived) // Passed from ShipmentDeclaration
					{
						Factory.AddFetchHint(JobPickupDeliveryConfirmSchema.EU_JS, BusinessObject.PK);
						ZQuery packLines = new ZQuery(JobPackLinesSchema.JL_JS, BusinessObject.PK);
						packLines.AddToFilter(JobPackLinesSchema.JL_FreightMode, "OUT");
						Factory.AddFetchHint(JobPackLinesSchema.Instance, packLines);
						Factory.AddFetchHint(JobConShipLinkSchema.JN_JS, BusinessObject.PK);
						Factory.AddFetchHint(JobDocsAndCartageSchema.JP_ParentID, BusinessObject.PK);
					}
				}
			}

			base.FetchForViewCore(columns.ToArray());
		}
	}
}
