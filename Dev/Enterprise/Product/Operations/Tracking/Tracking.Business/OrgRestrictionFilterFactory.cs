using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class OrgRestrictionFilterFactory
	{
		#region Constructor & Static Instance

		OrgRestrictionFilterFactory()
		{
		}

		public static OrgRestrictionFilterFactory Instance
		{
			get { return fInstance ?? (fInstance = new OrgRestrictionFilterFactory()); }
		}
		[ThreadStatic]
		static OrgRestrictionFilterFactory fInstance;

		internal static OrgContactWebUser SiteUser
		{
			get
			{
				if (WebEnv.AppInstance != null && WebEnv.AppInstance.SiteUser != null
				&& (fSiteUser == null || fSiteUser != WebEnv.AppInstance.SiteUser as OrgContactWebUser))
				{
					fSiteUser = WebEnv.AppInstance.SiteUser as OrgContactWebUser;
				}
				return fSiteUser;
			}
		}
		[ThreadStatic]
		static OrgContactWebUser fSiteUser;

		#endregion Constructor & Static Instance

		#region GetFilter
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery GetFilterCore(Type businessObjectType)
		{
			var filter = new ZQuery();

			if (businessObjectType == typeof(ForwardingConsol))
			{
				ZDBOnlyQuery consolQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));

				consolQuery.AddToFilter(GetOrgAddressSubQuery(JobConsolSchema.JK_OA_SendingForwarderAddress, SiteUser.OrganisationRelatedOrgAddressPKs), JoinCondition.Or);
				consolQuery.AddToFilter(GetOrgAddressSubQuery(JobConsolSchema.JK_OA_ReceivingForwarderAddress, SiteUser.OrganisationRelatedOrgAddressPKs), JoinCondition.Or);

				filter.AddToFilter(consolQuery);
			}
			// ToDo : might need to merge case WhsOrder with case TrackingWhsOrder
			else if (businessObjectType == typeof(WhsOrder) || businessObjectType == typeof(TrackingWhsOrder) || businessObjectType == typeof(WhsReceive) || businessObjectType == typeof(TrackingWhsReceive))
			{
				var query = new ZDBOnlyQuery(businessObjectType);
				query.AddToFilter(new ZQuery(WhsDocketSchema.WD_OH_Client, SQLComparisonOperator.Equal, SiteUser.OrganisationRelatedOrgPKs), JoinCondition.Or);

				if (businessObjectType == typeof(WhsReceive) || businessObjectType == typeof(TrackingWhsReceive))
				{
					var transportSubQuery = GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress);
					query.AddSubQuery(transportSubQuery, JoinCondition.Or);
				}

				AddSubQueryToExcludeProhibitedWarehousesForDocket(query, SiteUser.LoggedInUser.PK);
				return query;
			}
			else if (businessObjectType == typeof(TrackingWhsInventory))
			{
				var query = new ZDBOnlyQuery(businessObjectType);
				query.AddToFilter(new ZQuery(WhsInventoryViewSchema.WI_OH_Client, SQLComparisonOperator.Equal, SiteUser.OrganisationRelatedOrgPKs), JoinCondition.Or);

				var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsInventoryViewSchema.WI_WD);
				AddSubQueryToExcludeProhibitedWarehousesForDocket(docketSubQuery, SiteUser.LoggedInUser.PK);
				query.AddSubQuery(docketSubQuery, JoinCondition.And);
				return query;
			}
			else if (businessObjectType == typeof(OrgPartRelation))
			{
				filter.AddToFilter(new ZQuery(OrgPartRelationSchema.OU_OH, SQLComparisonOperator.Equal, SiteUser.OrganisationRelatedOrgPKs), JoinCondition.Or);
			}
			else
			{
				SchemaColumnCollection columns = GetAllSchemaColumnsForType(businessObjectType);

				if (columns != null)
				{
					var relatedOrgFilterExtension = new ZQuery();
					foreach (SchemaColumn column in columns)
					{
						if (column.Name.EndsWith("_OH") || column.Name.IndexOf("_OH_") != -1)
						{
							if (!SkipOHFieldFilter(businessObjectType, column))
							{
								relatedOrgFilterExtension.AddToFilter(JoinCondition.Or, column, SQLComparisonOperator.Equal, SiteUser.OrganisationRelatedOrgPKs);
							}
						}
					}
					filter.AddToFilter(relatedOrgFilterExtension, JoinCondition.Or);
				}
				else
				{
					filter.IsNoResultQuery = ZBool.True;
				}
			}

			if (businessObjectType == typeof(OrgPartRelation))
			{
				string[] relTypes = new string[] { OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both };
				filter.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_Relationship, relTypes);
			}

			if (businessObjectType == typeof(TrackingShipment) || businessObjectType == typeof(TrackingCFSShipment))
			{
				var filterAsSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK);
				filterAsSubQuery.AddToFilter(filter);
				ZDBOnlyQuery shipmentQuery = GetShipmentQuery(filterAsSubQuery);
				return shipmentQuery;
			}

			if (businessObjectType == typeof(AgencyShipment))
			{
				var shipmentQuery = GetAgencyShipmentQuery();
				shipmentQuery.AddToFilter(filter, JoinCondition.Or);
				return shipmentQuery;
			}

			if (businessObjectType == typeof(TrackingBooking))
			{
				ZDBOnlyQuery bookingQuery = GetBookingQuery();
				bookingQuery.AddToFilter(filter, JoinCondition.Or);
				return bookingQuery;
			}

			if (businessObjectType == typeof(TrackingDeclaration))
			{
				ZDBOnlyQuery declarationQuery = GetDeclarationQuery();
				return declarationQuery;
			}

			if (businessObjectType == typeof(TrackingContainer))
			{
				ZDBOnlyQuery containerQuery = GetJobContainerQuery();
				containerQuery.AddToFilter(filter, JoinCondition.Or);
				return containerQuery;
			}

			if (businessObjectType == typeof(LinerAndAgencyContainer))
			{
				var linerAndAgencyContainerQuery = GetLinerAndAgencyContainerQuery();
				linerAndAgencyContainerQuery.AddToFilter(JoinCondition.And, JobContainerSchema.JC_ContainerNum, SQLComparisonOperator.NotEqual, ZString.Empty);
				linerAndAgencyContainerQuery.AddToFilter(filter, JoinCondition.Or);
				return linerAndAgencyContainerQuery;
			}

			if (businessObjectType == typeof(TrackingOrder))
			{
				ZDBOnlyQuery trackingOrderQuery = new ZDBOnlyQuery(typeof(TrackingOrder));
				trackingOrderQuery.AddToFilter(filter, JoinCondition.Or);

				ZDBOnlySubQuery controllingCustomerSubQuery = GetControllingCustomerSubQuery();
				trackingOrderQuery.AddSubQuery(controllingCustomerSubQuery, JoinCondition.Or);
				trackingOrderQuery.AddToFilter(GetOrgAddressSubQuery(JobOrderHeaderSchema.JD_OA_BuyerAddress, SiteUser.OrganisationRelatedOrgAddressPKs), JoinCondition.Or);
				trackingOrderQuery.AddToFilter(GetOrgAddressSubQuery(JobOrderHeaderSchema.JD_OA_SupplierAddress, SiteUser.OrganisationRelatedOrgAddressPKs), JoinCondition.Or);

				return trackingOrderQuery;
			}

			if (businessObjectType == typeof(WhsWarehouse))
			{
				ZQuery whsQuery = GetWarehouseQuery();
				whsQuery.AddToFilter(filter, JoinCondition.Or);
				return whsQuery;
			}

			if (businessObjectType == typeof(CommonCartage))
			{
				ZDBOnlyQuery cartageQuery = GetCartageQuery();
				cartageQuery.AddToFilter(filter, JoinCondition.Or);
				return cartageQuery;
			}

			if (businessObjectType == typeof(AgencyBooking) || businessObjectType.IsSubclassOf(typeof(AgencyBooking)))
			{
				ZDBOnlySubQuery docAddressSubQuery = GetJobDocAddressQuery(new string[] { AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress });

				var jobShipmentQuery = new ZDBOnlyQuery(typeof(TrackingLinerAndAgencyBooking));
				jobShipmentQuery.AddSubQuery(JobShipmentSchema.PK, docAddressSubQuery, JoinCondition.And);

				return jobShipmentQuery;
			}

			if (businessObjectType == typeof(BillOfLading) || businessObjectType.IsSubclassOf(typeof(BillOfLading)))
			{
				ZDBOnlyQuery jobShipmentQuery1 = new ZDBOnlyQuery(typeof(TrackingBillOfLading));

				string[] addressTypes = new string[] {
AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress,
AutoDocAddressTypes.Codes.NotifyParty,
AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress,
AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress };
				ZDBOnlySubQuery docAddressSubQuery = GetJobDocAddressQuery(addressTypes);

				ZDBOnlySubQuery relatedOrgsSubQuery = new ZDBOnlySubQuery(typeof(TrackingBillOfLading), JobShipmentSchema.PK);
				relatedOrgsSubQuery.AddSubQuery(GetAgencyBillOfLadingJobHeaderSubQuery(), JoinCondition.Or);
				relatedOrgsSubQuery.AddSubQuery(docAddressSubQuery, JoinCondition.Or);

				jobShipmentQuery1.AddToFilter(filter, JoinCondition.Or);
				jobShipmentQuery1.AddSubQuery(relatedOrgsSubQuery, JoinCondition.Or);

				return jobShipmentQuery1;
			}

			return filter;
		}

		bool SkipOHFieldFilter(Type businessObjectType, SchemaColumn column) => businessObjectType != typeof(TrackingCFSShipment) && column.Name == Freight.Common.Business.AutoJobShipment.Schema.JS_OH_HandledOnBehalfOfForwarder;

		void AddSubQueryToExcludeProhibitedWarehousesForDocket(ZDBOnlyQuery docketQuery, ZGuid orgContactPK)
		{
			var genPivotQuery = GetProhibitedWarehouseSubQuery(orgContactPK);
			docketQuery.AddSubQuery(WhsDocketSchema.WD_WW_Whs, genPivotQuery, JoinCondition.And);
		}

		public static ZQuery GetOrderedProhibitedWarehouseQuery(ZGuid? orgContactPK = null, ZGuid[] organisationRelatedOrgPKs = null)
		{
			if (SiteUser is TrackingSiteUser trackingSiteUser && trackingSiteUser.LoggedInOrganisation != null)
			{
				orgContactPK = trackingSiteUser.LoggedInUser.PK;
				organisationRelatedOrgPKs = trackingSiteUser.OrganisationRelatedOrgPKs;
			}

			if (orgContactPK != null && orgContactPK.Value.IsValid)
			{
				var warehouseQuery = new ZDBOnlyQuery(typeof(WhsWarehouse));

				var docketQuery = new ZDBOnlySubQuery(typeof(WhsOrder), WhsDocketSchema.WD_WW_Whs);
				docketQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, organisationRelatedOrgPKs);
				warehouseQuery.AddSubQuery(docketQuery, JoinCondition.And);

				var genPivotQuery = GetProhibitedWarehouseSubQuery(orgContactPK.Value);
				warehouseQuery.AddSubQuery(genPivotQuery, JoinCondition.And);

				return warehouseQuery;
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}

		public static ZDBOnlySubQuery GetProhibitedWarehouseSubQuery(ZGuid orgContactPK)
		{
			var genPivotQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID, notIn: true);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, orgContactPK);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.OrgContactDeniedWarehouse);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, OrgContactSchema.Constants.Prefix);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, WhsWarehouseSchema.Constants.Prefix);
			return genPivotQuery;
		}

		enum JobHeaderRelatedOrg
		{
			LocalClient = 1,
			Agent = 2
		}

		static ZDBOnlySubQuery GetRatingHeaderSubQuery(SchemaGuidColumn fKColumn, ZGuid[] organisationRelatedOrgAddressPKs, JobHeaderRelatedOrg relatedOrg)
		{
			var ratingHeaderQuery = new ZDBOnlySubQuery(typeof(RatingHeader), fKColumn);
			ratingHeaderQuery.AddSubQuery(GetJobHeaderSubQuery(relatedOrg, organisationRelatedOrgAddressPKs, RatingHeaderSchema.Constants.Prefix), JoinCondition.And);

			return ratingHeaderQuery;
		}

		ZDBOnlySubQuery GetJobHeaderSubQuery(JobHeaderRelatedOrg relatedOrg, params string[] parentTableCodes)
		{
			return GetJobHeaderSubQuery(relatedOrg, SiteUser.OrganisationRelatedOrgAddressPKs, parentTableCodes);
		}

		static ZDBOnlySubQuery GetJobHeaderSubQuery(JobHeaderRelatedOrg relatedOrg, ZGuid[] organisationRelatedOrgAddressPKs, params string[] parentTableCodes)
		{
			var headerAddressQuery = new ZDBOnlyQuery(typeof(JobHeader));

			if ((relatedOrg & JobHeaderRelatedOrg.LocalClient) == JobHeaderRelatedOrg.LocalClient)
			{
				headerAddressQuery.AddToFilter(GetOrgAddressSubQuery(JobHeaderSchema.JH_OA_LocalChargesAddr, organisationRelatedOrgAddressPKs), JoinCondition.Or);
				headerAddressQuery.AddToFilter(GetOrgAddressSubQuery(JobHeaderSchema.JH_OA_AgentCollectAddr, organisationRelatedOrgAddressPKs), JoinCondition.Or);
			}

			var jobheaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobheaderSubQuery.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_ParentTableCode, parentTableCodes);
			jobheaderSubQuery.AddToFilter(headerAddressQuery, JoinCondition.And);

			return jobheaderSubQuery;
		}

		internal static ZDBOnlySubQuery GetJobHeaderSubQueryForLocalClient(SchemaGuidColumn column, ZGuid[] organisationRelatedOrgAddressPKs, params string[] parentTableCodes)
		{
			var headerAddressQuery = new ZDBOnlyQuery(typeof(JobHeader));
			headerAddressQuery.AddToFilter(column, SQLComparisonOperator.NotEqual, DBNull.Value);
			headerAddressQuery.AddToFilter(GetOrgAddressSubQuery(column, organisationRelatedOrgAddressPKs), JoinCondition.And);

			var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobHeaderSubQuery.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_ParentTableCode, parentTableCodes);
			jobHeaderSubQuery.AddToFilter(headerAddressQuery, JoinCondition.And);

			return jobHeaderSubQuery;
		}

		internal static ZQuery GetOrgAddressSubQuery(SchemaGuidColumn schemaGuidColumn, ZGuid[] organisationRelatedOrgAddressPKs)
		{
			var query = new ZQuery { AllowTableValuedParameters = true };
			query.AddToFilter(schemaGuidColumn, organisationRelatedOrgAddressPKs);

			return query;
		}

		ZDBOnlySubQuery GetAgencyBillOfLadingJobHeaderSubQuery()
		{
			return GetJobHeaderSubQuery(JobHeaderRelatedOrg.LocalClient, JobShipmentSchema.Constants.Prefix);
		}

		internal ZDBOnlySubQuery GetControllingCustomerSubQuery()
		{
			return GetJobDocAddressQuery(new string[] { AutoDocAddressTypes.Codes.ControllingCustomer });
		}

		#endregion GetFilter

		#region LoadFilteredByContact

		//public ZQuery GetFilter<T>(OrgContactWebUser SiteUser) where T : BusinessObject
		//{
		//    ZQuery result = ZQuery.NoResultQuery;
		//    if (SiteUser != null && SiteUser.IsLoggedIn)
		//    {
		//        result = GetFilter(typeof(T), SiteUser.LoggedInOrganisation.PK);
		//    }
		//    return result;
		//}
		public ZQuery GetFilter(Type bizOType)
		{
			ZQuery result = ZQuery.NoResultQuery;
			if (SiteUser != null && SiteUser.IsLoggedIn)
			{
				result = GetFilterCore(bizOType);
			}
			return result;
		}

		public ZQuery GetFilter<T>() where T : BusinessObject
		{
			ZQuery result = null;
			result = GetFilter(typeof(T));
			return result;
		}

		public static T LoadFilteredByContact<T>(BusinessObjectFactory factory, SchemaColumn column, object value) where T : BusinessObject
		{
			if (SiteUser != null && SiteUser.IsLoggedIn)
			{
				ZQuery filter = new ZQuery(column, value);
				filter.AddToFilter(Instance.GetFilter(typeof(T)));
				filter.IgnoreActiveFilter = true;
				T result = factory.LoadTop1<T>(filter);
				if (result != null)
				{
					result.GetLogs().AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
				}
				return result;
			}
			return null;
		}

		#endregion

		#region GetSubQuery

		public ZDBOnlyQuery GetSubQuery(Type businessObjectType, Type businessObjectSubQueryType, SchemaGuidColumn foreignKeyField)
		{
			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(businessObjectType);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(businessObjectSubQueryType, foreignKeyField);
			subQuery.AddToFilter(Instance.GetFilter(businessObjectSubQueryType));
			dBQuery.AddSubQuery(subQuery, JoinCondition.And);
			return dBQuery;
		}

		#endregion

		#region Liner&Agency Container Queries

		ZDBOnlyQuery GetLinerAndAgencyContainerQuery()
		{
			var mainOrgRestrictionQuery = new ZDBOnlyQuery(typeof(LinerAndAgencyContainer));
			var relatedShipmentsSubQuery = GetLinerAndAgencyContainerShipmentSubQuery();
			mainOrgRestrictionQuery.AddSubQuery(relatedShipmentsSubQuery, JoinCondition.Or);
			return mainOrgRestrictionQuery;
		}

		#endregion

		#region Container Queries

		ZDBOnlyQuery GetJobContainerQuery()
		{
			var mainOrgRestrictionQuery = new ZDBOnlyQuery(typeof(TrackingContainer));
			mainOrgRestrictionQuery.LoadSmallBlobs = 0;

			var relatedConsolsSubQuery = GetContainerConsolsSubQuery();
			var relatedShipmentsSubQuery = GetContainerShipmentSubQuery();
			var relatedOrderSubQuery = GetContainerOrdersSubQuery();
			var relatedSADecSubQuery = GetContainerStandAloneDeclarationSubQuery();

			mainOrgRestrictionQuery.AddSubQuery(relatedConsolsSubQuery, JoinCondition.Or);
			mainOrgRestrictionQuery.AddSubQuery(relatedShipmentsSubQuery, JoinCondition.Or);
			mainOrgRestrictionQuery.AddSubQuery(relatedOrderSubQuery, JoinCondition.Or);
			mainOrgRestrictionQuery.AddSubQuery(JobContainerSchema.PK, relatedSADecSubQuery, JoinCondition.Or);

			return mainOrgRestrictionQuery;
		}

		#region Container -> Consols Relationship Query

		ZDBOnlySubQuery GetContainerConsolsSubQuery()
		{
			ZDBOnlySubQuery jobConsolQuery = new ZDBOnlySubQuery(typeof(TrackingConsol), JobContainerSchema.JC_JK);
			jobConsolQuery.AddToFilter(GetContainerConsolRestrictionQuery(), JoinCondition.And);
			return jobConsolQuery;
		}

		ZQuery GetContainerConsolRestrictionQuery()
		{
			ZQuery consolRestrictionQuery = Instance.GetFilter<ForwardingConsol>();

			ZDBOnlyQuery consolQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
			consolQuery.AddToFilter(GetOrgAddressSubQuery(JobConsolSchema.JK_OA_ShippingLineAddress, SiteUser.OrganisationRelatedOrgAddressPKs), JoinCondition.And);

			consolRestrictionQuery.AddToFilter(consolQuery, JoinCondition.Or);
			return consolRestrictionQuery;
		}

		#endregion

		ZDBOnlySubQuery GetContainerShipmentSubQuery()
		{
			var shipmentSubQuery = new ZDBOnlySubQuery(typeof(TrackingShipment), JobShipmentSchema.PK);
			shipmentSubQuery.AddToFilter(GetFilter<TrackingShipment>());
			shipmentSubQuery.AddToFilter(new ZQuery(JobShipmentSchema.JS_IsForwardRegistered, ZBool.True), JoinCondition.And);
			return ContainerFilterFactory.Instance.GetContainerShipmentSubQuery(shipmentSubQuery);
		}

		ZDBOnlySubQuery GetLinerAndAgencyContainerShipmentSubQuery()
		{
			var shipmentSubQuery = new ZDBOnlySubQuery(typeof(AgencyShipment), JobShipmentSchema.PK);
			shipmentSubQuery.AddToFilter(GetFilter<AgencyShipment>());
			shipmentSubQuery.AddToFilter(new ZQuery(JobShipmentSchema.JS_IsShipping, ZBool.True), JoinCondition.And);
			return ContainerFilterFactory.Instance.GetLinerAndAgencyContainerShipmentSubQuery(shipmentSubQuery);
		}

		ZDBOnlySubQuery GetContainerStandAloneDeclarationSubQuery()
		{
			ZDBOnlySubQuery sADecQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
			sADecQuery.AddToFilter(GetFilter<BaseJobDeclaration>(), JoinCondition.And);
			sADecQuery.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_JS, SQLComparisonOperator.Equal, DBNull.Value);
			return ContainerFilterFactory.Instance.GetContainerStandAloneDeclarationSubQuery(sADecQuery);
		}

		ZDBOnlySubQuery GetContainerOrdersSubQuery()
		{
			ZDBOnlySubQuery ordersSubQuery = new ZDBOnlySubQuery(typeof(TrackingOrder), JobOrderHeaderSchema.JD_JS);
			ordersSubQuery.AddToFilter(GetFilter<TrackingOrder>(), JoinCondition.And);

			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(TrackingShipment), JobShipmentSchema.PK);
			shipmentSubQuery.AddSubQuery(ordersSubQuery, JoinCondition.And);

			return ContainerFilterFactory.Instance.GetContainerShipmentSubQuery(shipmentSubQuery);
		}

		#endregion

		#region GetJobConsolSubQuery

		internal ZDBOnlySubQuery GetJobConsolSubQuery()
		{
			ZDBOnlySubQuery jobConsolAgentsQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
			jobConsolAgentsQuery.AddToFilter(Instance.GetFilter<ForwardingConsol>());

			ZDBOnlySubQuery jobConShipLinkQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			jobConShipLinkQuery.AddSubQuery(JobConShipLinkSchema.JN_JK, jobConsolAgentsQuery, JoinCondition.And);

			return jobConShipLinkQuery;
		}

		#endregion

		#region GetColumnsForType

		internal SchemaColumnCollection GetAllSchemaColumnsForType(Type businessObjectType)
		{
			if (businessObjectType == typeof(TrackingShipment) || businessObjectType == typeof(TrackingCFSShipment) || businessObjectType == typeof(AgencyShipment))
			{
				return JobShipmentSchema.All;
			}
			else if (businessObjectType == typeof(TrackingBooking))
			{
				return ViewQuotedBookingSchema.All;
			}
			else if (businessObjectType == typeof(TrackingDeclaration) || businessObjectType == typeof(BaseJobDeclaration))
			{
				return JobDeclarationSchema.All;
			}
			else if (businessObjectType.Equals(typeof(TrackingOrder)))
			{
				return JobOrderHeaderSchema.All;
			}
			else if (businessObjectType.Equals(typeof(WhsWarehouse)))
			{
				return WhsWarehouseSchema.All;
			}
			else if (businessObjectType.Equals(typeof(TrackingWhsOrder)) ||
			businessObjectType.Equals(typeof(TrackingWhsReceive)))
			{
				return WhsDocketSchema.All;
			}
			else if (businessObjectType.Equals(typeof(TrackingWhsInventory)))
			{
				return WhsInventoryViewSchema.All;
			}
			else if (businessObjectType.Equals(typeof(TrackingWhsReceive)))
			{
				return WhsDocketSchema.All;
			}
			else if (businessObjectType.Equals(typeof(InvoicingBase)))
			{
				return AccTransactionHeaderSchema.All;
			}
			else if (businessObjectType.Equals(typeof(ForwardingConsol)))
			{
				return JobConsolSchema.All;
			}
			else if (businessObjectType.Equals(typeof(OrgPartRelation)))
			{
				return OrgPartRelationSchema.All;
			}
			else if (businessObjectType.Equals(typeof(TrackingContainer)) || businessObjectType == typeof(LinerAndAgencyContainer))
			{
				return JobContainerSchema.All;
			}
			else if (businessObjectType.Equals(typeof(AgencyBooking)) || businessObjectType.IsSubclassOf(typeof(AgencyBooking)))
			{
				return JobShipmentSchema.All;
			}
			else if (businessObjectType.Equals(typeof(BillOfLading)) || businessObjectType.IsSubclassOf(typeof(BillOfLading)))
			{
				return JobShipmentSchema.All;
			}
			else if (businessObjectType.Equals(typeof(CommonCartage)))
			{
				return JobCartageSchema.All;
			}
			else
			{
				ErrorReporter.ReportOnce("OrgRestrictionFilterGetColumns_" + businessObjectType.FullName, String.Format("OrgRestrictionFilterFactory is not aware of the type {0}. Please add column selection logic to OrgRestrictionFilterFactory to return the correct columns to filter by.", businessObjectType.FullName));
				return null;
			}
		}

		#endregion GetColumnsForType

		#region GetWarehouseQuery

		ZQuery GetWarehouseQuery()
		{
			if (SiteUser != null && SiteUser.LoggedInOrganisation != null)
			{
				ZDBOnlyQuery warehouseQuery = new ZDBOnlyQuery(typeof(WhsWarehouse));

				ZDBOnlySubQuery docketQuery = new ZDBOnlySubQuery(typeof(WhsOrder), WhsDocketSchema.WD_WW_Whs);
				docketQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, SiteUser.LoggedInOrganisation.PK);

				warehouseQuery.AddSubQuery(docketQuery, JoinCondition.And);
				warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);

				warehouseQuery.OrderBy = AutoWhsWarehouse.Schema.WW_WarehouseName;

				return warehouseQuery;
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}

		#endregion

		#region GetCartageQuery

		ZDBOnlyQuery GetCartageQuery()
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(CommonCartage));
			string[] localCartageAddressTypes =
{
AutoDocAddressTypes.Codes.LocalCartageCFS,
AutoDocAddressTypes.Codes.LocalCartageCTO,
AutoDocAddressTypes.Codes.LocalCartageMSC,
AutoDocAddressTypes.Codes.LocalCartageYard,
AutoDocAddressTypes.Codes.LocalCartageService,
AutoDocAddressTypes.Codes.LocalCartageExporter,
AutoDocAddressTypes.Codes.LocalCartageImporter,
};

			ZDBOnlySubQuery docAddressSubquery = GetJobDocAddressQuery(localCartageAddressTypes);
			filter.AddSubQuery(JobCartageSchema.PK, docAddressSubquery, JoinCondition.Or);
			filter.AddToFilter(GetLocalClientQuery(), JoinCondition.Or);
			return filter;
		}

		ZQuery GetLocalClientQuery()
		{
			ZQuery addressQuery = new ZQuery(JobHeaderSchema.JH_OA_LocalChargesAddr, SiteUser.OrganisationRelatedOrgAddressPKs);

			ZDBOnlyQuery headerQuery = new ZDBOnlyQuery(typeof(JobHeader));
			headerQuery.AddToFilter(addressQuery, JoinCondition.And);

			return GetJobHeaderQuery(headerQuery);
		}

		ZDBOnlyQuery GetJobHeaderQuery(ZQuery headerQuery)
		{
			ZDBOnlyQuery jobCartageDBOnlyQuery = new ZDBOnlyQuery(typeof(CommonCartage));

			ZDBOnlySubQuery jobCartageJobHeader = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobCartageJobHeader.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			jobCartageJobHeader.AddToFilter(headerQuery);

			jobCartageDBOnlyQuery.AddSubQuery(jobCartageJobHeader, JoinCondition.And);

			return jobCartageDBOnlyQuery;
		}

		#endregion

		#region JobDocAddressQuery

		ZDBOnlySubQuery GetJobDocAddressQuery(params string[] jobDocAddressTypes)
		{
			return GetJobDocAddressQuery(SiteUser.OrganisationRelatedOrgAddressPKs, null, jobDocAddressTypes);
		}

		internal static ZDBOnlySubQuery GetJobDocAddressQuery(ZGuid[] organisationRelatedOrgAddressPKs, string parentTableCode, params string[] jobDocAddressTypes)
		{
			ZDBOnlySubQuery jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			if (!string.IsNullOrEmpty(parentTableCode))
			{
				jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, parentTableCode);
			}
			jobDocAddressSubQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, jobDocAddressTypes);

			var orgAddressSubQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			ZQuery addressPkQuery = new ZQuery(JobDocAddressSchema.E2_OA_Address, organisationRelatedOrgAddressPKs);
			orgAddressSubQuery.AddToFilter(addressPkQuery, JoinCondition.And);

			jobDocAddressSubQuery.AddToFilter(orgAddressSubQuery);
			return jobDocAddressSubQuery;
		}

		#endregion

		#region GetShipmentQuery

		internal static IEnumerable<string> GetShipmentAddressTypes()
		{
			var addressTypes = new List<string> {
				AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress,
				AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress,
				AutoDocAddressTypes.Codes.NotifyParty,
				AutoDocAddressTypes.Codes.NotifyParty2,
				AutoDocAddressTypes.Codes.NotifyParty3,
				AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress
			};

			if (WebDataRegistry.Instance.AccessFromManagementGroupAndClientControlled.Value)
			{
				addressTypes.Add(AutoDocAddressTypes.Codes.ControllingCustomer);
			}

			return addressTypes;
		}

		ZDBOnlyQuery GetShipmentQuery(ZDBOnlySubQuery filter)
		{
			var addressTypes = GetShipmentAddressTypes().ToArray();
			var shipmentFilter = new ZDBOnlyQuery(typeof(TrackingShipment));
			shipmentFilter.AddSubQuery(JobShipmentSchema.PK, GetJobDocAddressQuery(SiteUser.OrganisationRelatedOrgAddressPKs, "JS", addressTypes), JoinCondition.Or);

			shipmentFilter.AddSubQuery(JobShipmentSchema.PK, GetJobHeaderSubQueryForLocalClient(JobHeaderSchema.JH_OA_LocalChargesAddr, SiteUser.OrganisationRelatedOrgAddressPKs, JobShipmentSchema.Constants.Prefix), JoinCondition.Or);
			shipmentFilter.AddSubQuery(JobShipmentSchema.PK, GetJobHeaderSubQueryForLocalClient(JobHeaderSchema.JH_OA_AgentCollectAddr, SiteUser.OrganisationRelatedOrgAddressPKs, JobShipmentSchema.Constants.Prefix), JoinCondition.Or);

			Action<SchemaGuidColumn> addConsolAddressQuery = (SchemaGuidColumn column) =>
			{
				var addressQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				var subQueryForAddressQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
				subQueryForAddressQuery.AddToFilter(GetOrgAddressSubQuery(column, SiteUser.OrganisationRelatedOrgAddressPKs), JoinCondition.Or);
				addressQuery.AddSubQuery(JobConShipLinkSchema.JN_JK, subQueryForAddressQuery, JoinCondition.And);
				shipmentFilter.AddSubQuery(JobShipmentSchema.PK, addressQuery, JoinCondition.Or);
			};

			addConsolAddressQuery(JobConsolSchema.JK_OA_SendingForwarderAddress);
			addConsolAddressQuery(JobConsolSchema.JK_OA_ReceivingForwarderAddress);

			shipmentFilter.AddSubQuery(JobShipmentSchema.PK, filter, JoinCondition.Or);

			return shipmentFilter;
		}

		#endregion

		#region GetAgencyShipmentQuery

		ZDBOnlyQuery GetAgencyShipmentQuery()
		{
			var filter = new ZDBOnlyQuery(typeof(AgencyShipment));

			var unionQuery = GetJobDocAddressQuery(SiteUser.OrganisationRelatedOrgAddressPKs, "JS", new string[] { "CED", "CRD", "NPP", "N2D", "N3D", "BKD" });

			var unionQuery2 = GetJobHeaderSubQuery(JobHeaderRelatedOrg.LocalClient | JobHeaderRelatedOrg.Agent, JobShipmentSchema.Constants.Prefix);
			unionQuery.AddAsUnionQuery(unionQuery2);

			filter.AddSubQuery(JobShipmentSchema.PK, unionQuery, JoinCondition.Or);
			return filter;
		}

		#endregion

		ZDBOnlyQuery GetBookingQuery()
		{
			return GetBookingQuery(SiteUser.OrganisationRelatedOrgAddressPKs, SiteUser.LoggedInOrganisation.PK);
		}

		internal static ZDBOnlyQuery GetBookingQuery(ZGuid[] organisationRelatedOrgAddressPKs, ZGuid contactOrgPK)
		{
			var result = new ZDBOnlyQuery(typeof(ViewTrackingBooking));
			result.AddToFilter(ViewQuotedBookingSchema.VB_JS, SQLComparisonOperator.NotEqual, DBNull.Value);

			var orgRestrictionQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK)
			{
				IgnoreActiveFilter = true
			};
			string[] bookingAddressTypes =
			{
				AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress,
				AutoDocAddressTypes.Codes.ConsigneeDocumentaryAddress,
				AutoDocAddressTypes.Codes.NotifyParty,
				AutoDocAddressTypes.Codes.PickupAgent
			};
			orgRestrictionQuery.AddSubQuery(GetJobDocAddressQuery(organisationRelatedOrgAddressPKs, null, bookingAddressTypes), JoinCondition.Or);
			orgRestrictionQuery.AddSubQuery(GetJobHeaderSubQuery(JobHeaderRelatedOrg.LocalClient, organisationRelatedOrgAddressPKs, JobShipmentSchema.Constants.Prefix), JoinCondition.Or);

			var deliveryAgent = new ZQuery(JobShipmentSchema.JS_OH_DeliveryAgent, contactOrgPK);
			orgRestrictionQuery.AddToFilter(deliveryAgent, JoinCondition.Or);

			var exportBroker = new ZQuery(JobShipmentSchema.JS_OH_ExportBroker, contactOrgPK);
			orgRestrictionQuery.AddToFilter(exportBroker, JoinCondition.Or);

			result.AddSubQuery(ViewQuotedBookingSchema.VB_JS, orgRestrictionQuery, JoinCondition.And);
			result.AddSubQuery(GetRatingHeaderSubQuery(ViewQuotedBookingSchema.VB_TH, organisationRelatedOrgAddressPKs, JobHeaderRelatedOrg.LocalClient), JoinCondition.Or);

			return result;
		}

		#region GetDeclarationQuery

		ZDBOnlyQuery GetDeclarationQuery()
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(BaseJobDeclaration));

			//Supplier
			var supplierQuery = new ZQuery(JobDeclarationSchema.JE_OH_Supplier, SiteUser.LoggedInOrganisation.PK);
			supplierQuery.AddToFilter(new ZQuery(JobDeclarationSchema.JE_MessageType, exportMessageTypes));

			//Importer
			var importerQuery = new ZQuery(JobDeclarationSchema.JE_OH_Importer, SiteUser.LoggedInOrganisation.PK);
			importerQuery.AddToFilter(new ZQuery(JobDeclarationSchema.JE_MessageType, importMessageTypes));

			//US Declaration
			var usDeclarationQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			usDeclarationQuery.AddToFilter(new ZQuery(JobDeclarationSchema.JE_OA_DeclarantAddress, SiteUser.OrganisationRelatedOrgAddressPKs), JoinCondition.And);
			usDeclarationQuery.AddToFilter(new ZQuery(JobDeclarationSchema.JE_MessageType, importMessageTypes), JoinCondition.And);

			//US Branch subquery
			var uSBranchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);
			uSBranchSubQuery.AddToFilter(JoinCondition.And, GlbBranchSchema.GB_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.UnitedStates);
			usDeclarationQuery.AddSubQuery(uSBranchSubQuery, JoinCondition.And);

			//Bill To Party
			var billToPartyQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			billToPartyQuery.AddToFilter(JobHeaderSchema.JH_OA_LocalChargesAddr, SiteUser.OrganisationRelatedOrgAddressPKs);
			billToPartyQuery.AddToFilter(JobHeaderSchema.JH_ParentTableCode, JobDeclarationSchema.Constants.Prefix);

			//Shipment that matches LocalClient
			var shipmentDeclarationSubQuery = GetJobHeaderSubQuery(JobHeaderRelatedOrg.LocalClient, JobShipmentSchema.Constants.Prefix);
			shipmentDeclarationSubQuery.AddSubQuery(JobHeaderSchema.JH_GC, GetDeclarationBranchSubQuery(), JoinCondition.And);

			//Full query
			filter.AddToFilter(supplierQuery);
			filter.AddToFilter(importerQuery, JoinCondition.Or);
			filter.AddToFilter(usDeclarationQuery, JoinCondition.Or);
			filter.AddSubQuery(JobDeclarationSchema.PK, billToPartyQuery, JoinCondition.Or);
			filter.AddSubQuery(JobDeclarationSchema.JE_JS, shipmentDeclarationSubQuery, JoinCondition.Or);

			return filter;
		}

		ZDBOnlySubQuery GetDeclarationBranchSubQuery()
		{
			var branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_GC);
			branchSubQuery.AddToFilter(GlbBranchSchema.PK, SQLComparisonOperator.Equal, JobDeclarationSchema.JE_GB);

			return branchSubQuery;
		}

		// Agreed with the Customs team to hard-code these message types, as the IsExport/IsImport logic is overridden by some countries
		readonly string[] exportMessageTypes = new string[] { "EXP", "EXX", "AQS", "TNP", "COO" };
		readonly string[] importMessageTypes = new string[] { "IMP", "IMX", "WEA", "EXW", "LVS", "LVX", "IM2", "IPT", "INP", "TNP", "FTZ", "MSC" };

		#endregion
	}
}
