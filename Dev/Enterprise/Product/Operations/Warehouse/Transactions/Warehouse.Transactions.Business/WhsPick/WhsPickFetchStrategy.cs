using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions
{
	class WhsPickFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsPickFetchStrategy(WhsPick pick)
			: base(pick)
		{
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			foreach (WhsPickableDocket order in Pick.Orders)
			{
				Factory.AddFetchHint(typeof(OrgAddress), OrgAddressSchema.OA_OH, order.WD_OH_Client);
				Factory.AddFetchHint(WhsDocketContainerSchema.WC_WD, order.PK);

				// JobDocAddress
				Factory.AddFetchHint(typeof(JobDocAddress), FetchHintsHelper.GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, order.PK));

				// Docket
				var docketQuery = new ZQuery(WhsDocketSchema.WD_OH_Client, order.WD_OH_Client);
				docketQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
				docketQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, order.WD_ExternalReference);
				docketQuery.AddToFilter(WhsDocketSchema.WD_ExternalReferenceSplit, order.WD_ExternalReferenceSplit);
				Factory.AddFetchHint(WhsDocketSchema.Instance, docketQuery);
			}
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			var requireDocket = false;
			var requireSalesChannel = false;
			var requireWareHouse = false;
			var requireOrgHeader = false;
			var requireOrgAddressAndJobDocAddress = false;
			var requireDDL = false;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case nameof(WhsPick.SalesChannelDescriptions):
						requireDocket = true;
						requireSalesChannel = true;
						break;
					case nameof(WhsPick.ClientCodes):
					case nameof(WhsPick.ClientNames):
						requireDocket = true;
						requireOrgHeader = true;
						break;
					case nameof(WhsPick.TransportReferences):
					case nameof(WhsPick.ServiceLevels):
					case nameof(WhsPick.PickPriority):
					case nameof(WhsPick.EarliestRequiredDate):
						requireDocket = true;
						break;
					case nameof(WhsPick.TransportCodes):
					case nameof(WhsPick.ConsigneeCodes):
					case nameof(WhsPick.ConsigneeNames):
					case nameof(WhsPick.DistributionCentreCodes):
						requireWareHouse = true;
						requireOrgAddressAndJobDocAddress = true;
						break;
					case nameof(WhsPick.DockDoorPK):
						requireWareHouse = true;
						requireDDL = true;
						break;
					default:
						break;
				}
			}

			if (requireDocket)
			{
				Factory.AddFetchHint(WhsDocketSchema.WD_WP, Pick.PK);
			}

			if (requireSalesChannel)
			{
				// Fetch WD.WSH
				var whsSalesChannelQuery = new ZDBOnlyQuery(typeof(WhsSalesChannel));
				var whsDocketQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WSH_SalesChannel);
				whsDocketQuery.AddToFilter(WhsDocketSchema.WD_WP, Pick.PK);
				whsSalesChannelQuery.AddSubQuery(whsDocketQuery, JoinCondition.And);
				Factory.AddFetchHint(WhsSalesChannelSchema.Instance, whsSalesChannelQuery);
			}

			if (requireWareHouse)
			{
				Factory.AddFetchHint(WhsWarehouseSchema.PK, Pick.WP_WW_Whs);
			}

			if (requireDDL)
			{
				Factory.AddFetchHint(WhsLocationViewSchema.PK, Pick.WP_WL_DockDoor);
				Factory.AddFetchHint(WhsDockDoorAssignmentSchema.PK, Pick.WP_WDA_DockDoorAssignment);
			}

			if (requireOrgHeader)
			{
				var whsClientCodeAndNameQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				var whsDocketQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_OH_Client);
				whsDocketQuery.AddToFilter(WhsDocketSchema.WD_WP, Pick.PK);
				whsClientCodeAndNameQuery.AddToFilter(OrgHeaderSchema.OH_IsActive, true);
				whsClientCodeAndNameQuery.AddSubQuery(whsDocketQuery, JoinCondition.And);
				Factory.AddFetchHint(OrgHeaderSchema.Instance, whsClientCodeAndNameQuery);
			}

			if (requireOrgAddressAndJobDocAddress)
			{
				var orgAddressQuery = new ZDBOnlyQuery(typeof(OrgAddress));
				var whsWarehouseQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsWarehouseSchema.WW_OA_WarehouseAddress);
				whsWarehouseQuery.AddToFilter(WhsWarehouseSchema.PK, Pick.WP_WW_Whs);
				orgAddressQuery.AddSubQuery(whsWarehouseQuery, JoinCondition.And);
				Factory.AddFetchHint(typeof(OrgAddress), orgAddressQuery);

				Factory.AddFetchHint(typeof(JobDocAddress), FetchHintsHelper.GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, Pick.Orders.Select(o => o.PK).ToArray()));
			}
		}

		WhsPick Pick
		{
			get { return (WhsPick)BusinessObject; }
		}
	}
}
