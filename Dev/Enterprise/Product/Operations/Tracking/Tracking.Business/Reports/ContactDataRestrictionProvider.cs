using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Business
{
	public class ContactDataRestrictionProvider : IContactDataRestrictionProvider
	{
		public ZQuery GetFilter(ModuleIdentifier moduleIdentifier, ZGuid orgContactPK, BusinessObjectFactory factory)
		{
			if (orgContactPK.IsValid && moduleIdentifier != WebModuleIDs.NotAssigned)
			{
				var contact = factory.Load<OrgContact>(orgContactPK);
				if (contact?.ParentOrg?.PK != null && contact.ParentOrg.PK.IsValid)
				{
					var contactOrg = contact.ParentOrg;
					var relatedOrgPKs = GetRelatedOrgPKs(contactOrg.PK, factory);
					var relatedOrgAddressPKs = GetRelatedOrgAddressPKs(contactOrg.PK, relatedOrgPKs, factory);

					if (moduleIdentifier == WebModuleIDs.OrganisationTracking)
					{
						return GetOrganisationFilter(contactOrg.PK);
					}
					else if (moduleIdentifier == WebModuleIDs.TrackingShipments)
					{
						return GetShipmentFilter(relatedOrgPKs, relatedOrgAddressPKs);
					}
					else if (moduleIdentifier == WebModuleIDs.RefServiceLevel)
					{
						return GetServiceLevelFilter(contactOrg.OrgServiceLevels);
					}
					else if (moduleIdentifier == WebModuleIDs.OrgSupplierPartTracking)
					{
						return GetOrgSupplierPartFilter(relatedOrgPKs);
					}
					else if (moduleIdentifier == WebModuleIDs.TrackingWarehouse)
					{
						return GetWarehouseFilter(contact.PK, relatedOrgPKs);
					}
					else if (moduleIdentifier == WebModuleIDs.TrackingBookings)
					{
						return GetBookingQuery(relatedOrgAddressPKs, contactOrg.PK);
					}
				}
			}

			return ZQuery.NoResultQuery;
		}

		#region Organisation Filter

		ZQuery GetOrganisationFilter(ZGuid contactOrgPK)
		{
			// the logic is copied from C:\git\wtg\CargoWise\Dev\Enterprise\Architecture\Web\GUI\Modules\Organisation\AutoOrganisationFilterBusinessObject.cs protected virtual ZQuery OrgDetailsGroupBoxFilter
			var query = new ZQuery();
			if (contactOrgPK.IsValid)
			{
				var ohQuery = new ZDBOnlyQuery(typeof(OrgHeader));

				ohQuery.AddToFilter(GetSupplierBuyerLinkQuery(OrgHeaderSchema.OH_IsConsignee, OrgSupplierBuyerLinkSchema.OL_OH_Buyer, OrgSupplierBuyerLinkSchema.OL_OH_Supplier, contactOrgPK, SQLComparisonOperator.Equal));
				ohQuery.AddToFilter(GetSupplierBuyerLinkQuery(OrgHeaderSchema.OH_IsConsignor, OrgSupplierBuyerLinkSchema.OL_OH_Supplier, OrgSupplierBuyerLinkSchema.OL_OH_Buyer, contactOrgPK, SQLComparisonOperator.Equal), JoinCondition.Or);

				ohQuery.AddToFilter(JoinCondition.Or, OrgHeaderSchema.PK, contactOrgPK);
				ohQuery.AddToFilter(JoinCondition.And, OrgHeaderSchema.OH_IsActive, ZBool.True);
				query.AddToFilter(ohQuery, JoinCondition.And);
			}
			return query;
		}

		ZDBOnlyQuery GetSupplierBuyerLinkQuery(SchemaColumn orgHeaderField, SchemaColumn foreignKey, SchemaColumn supplierBuyerField, object foreignValue, SQLComparisonOperator @operator)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddToFilter(orgHeaderField, "Y");

			var subQuery = new ZDBOnlySubQuery(typeof(OrgSupplierBuyerLink), foreignKey);
			subQuery.AddToFilter(JoinCondition.And, supplierBuyerField, @operator, foreignValue);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Shipments

		ZDBOnlyQuery GetShipmentFilter(ZGuid[] organisationRelatedOrgPKs, ZGuid[] relatedOrgAddressPKs)
		{
			var relatedOrgQuery = BuildRelatedOrgQuery(JobShipmentSchema.All, organisationRelatedOrgPKs);
			var filterAsSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK);
			filterAsSubQuery.AddToFilter(relatedOrgQuery);

			var addressTypes = OrgRestrictionFilterFactory.GetShipmentAddressTypes().ToArray();
			var shipmentFilter = new ZDBOnlyQuery(typeof(ForwardingShipment));
			shipmentFilter.AddSubQuery(JobShipmentSchema.PK, OrgRestrictionFilterFactory.GetJobDocAddressQuery(relatedOrgAddressPKs, "JS", addressTypes), JoinCondition.Or);

			shipmentFilter.AddSubQuery(JobShipmentSchema.PK, OrgRestrictionFilterFactory.GetJobHeaderSubQueryForLocalClient(JobHeaderSchema.JH_OA_LocalChargesAddr, relatedOrgAddressPKs, JobShipmentSchema.Constants.Prefix), JoinCondition.Or);
			shipmentFilter.AddSubQuery(JobShipmentSchema.PK, OrgRestrictionFilterFactory.GetJobHeaderSubQueryForLocalClient(JobHeaderSchema.JH_OA_AgentCollectAddr, relatedOrgAddressPKs, JobShipmentSchema.Constants.Prefix), JoinCondition.Or);

			Action<SchemaGuidColumn> addConsolAddressQuery = (SchemaGuidColumn column) =>
			{
				var addressQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				var subQueryForAddressQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
				subQueryForAddressQuery.AddToFilter(OrgRestrictionFilterFactory.GetOrgAddressSubQuery(column, relatedOrgAddressPKs), JoinCondition.Or);
				addressQuery.AddSubQuery(JobConShipLinkSchema.JN_JK, subQueryForAddressQuery, JoinCondition.And);
				shipmentFilter.AddSubQuery(JobShipmentSchema.PK, addressQuery, JoinCondition.Or);
			};

			addConsolAddressQuery(JobConsolSchema.JK_OA_SendingForwarderAddress);
			addConsolAddressQuery(JobConsolSchema.JK_OA_ReceivingForwarderAddress);

			shipmentFilter.AddSubQuery(JobShipmentSchema.PK, filterAsSubQuery, JoinCondition.Or);
			return shipmentFilter;
		}

		#endregion

		#region ServiceLevel

		ZQuery GetServiceLevelFilter(OrgServiceLevelCollection orgServiceLevels)
		{
			//the logic is copied from C:\git\wtg\CargoWise\Dev\Enterprise\Architecture\Web\Business\Collection\WebServiceLevelCollection.cs internal static ZQuery CombineWithPublishedFilter(ZQuery filter, JoinCondition condition)
			var publishedFilter = new ZQuery();
			var levels = orgServiceLevels?
						.Where(level => level.PM_IsPublished)
						.Select(level => level.PM_RS_NKSrvLvl)
						.ToArray();
			if (levels?.Length > 0)
			{
				publishedFilter.AddToFilter(JoinCondition.Or, RefServiceLevelSchema.RS_Code, levels);
			}
			else
			{
				publishedFilter = ZQuery.NoResultQuery;
			}
			return publishedFilter;
		}

		#endregion

		#region OrgSupplierPart

		ZQuery GetOrgSupplierPartFilter(ZGuid[] organisationRelatedOrgPKs)
		{
			// filter logic is extracted from OrgRestrictionFilterFactory.Instance.GetFilter(typeof(OrgPartRelation))
			var query = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			var subQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			subQuery.AddToFilter(GetOrgPartRelationQuery(organisationRelatedOrgPKs));
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetOrgPartRelationQuery(ZGuid[] organisationRelatedOrgPKs)
		{
			var query = new ZQuery();
			query.AddToFilter(new ZQuery(OrgPartRelationSchema.OU_OH, SQLComparisonOperator.Equal, organisationRelatedOrgPKs), JoinCondition.Or);

			var relTypes = new string[] { OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both };
			query.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_Relationship, relTypes);

			return query;
		}

		#endregion

		#region Warehouse

		ZQuery GetWarehouseFilter(ZGuid orgContactPK, ZGuid[] relatedOrgPKs)
		{
			return OrgRestrictionFilterFactory.GetOrderedProhibitedWarehouseQuery(orgContactPK, relatedOrgPKs);
		}

		#endregion

		#region Booking

		ZQuery GetBookingQuery(ZGuid[] organisationRelatedOrgAddressPKs, ZGuid contactOrgPK)
		{
			return OrgRestrictionFilterFactory.GetBookingQuery(organisationRelatedOrgAddressPKs, contactOrgPK);
		}

		#endregion

		#region Implements

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Loop over filters not lookup values, cannot be written as an IN statement")]
		ZQuery BuildRelatedOrgQuery(SchemaColumnCollection columns, ZGuid[] organisationRelatedOrgPKs)
		{
			var filter = new ZQuery();
			if (columns != null)
			{
				var relatedOrgFilterExtension = new ZQuery();
				foreach (var column in columns)
				{
					if (column.Name.EndsWith("_OH") || column.Name.IndexOf("_OH_") != -1)
					{
						if (column.Name != Freight.Common.Business.AutoJobShipment.Schema.JS_OH_HandledOnBehalfOfForwarder)
						{
							relatedOrgFilterExtension.AddToFilter(JoinCondition.Or, column, SQLComparisonOperator.Equal, organisationRelatedOrgPKs);
						}
					}
				}
				filter.AddToFilter(relatedOrgFilterExtension, JoinCondition.Or);
			}
			else
			{
				filter.IsNoResultQuery = ZBool.True;
			}
			return filter;
		}

		ZGuid[] GetRelatedOrgPKs(ZGuid contactOrgPK, BusinessObjectFactory factory)
		{
			var query = OrgContactWebUser.GetRelatedOrgQuery(contactOrgPK);
			var orginsations = factory.Load<OrgHeader>(query);
			return orginsations.Select(o => o.PK).ToArray();
		}

		ZGuid[] GetRelatedOrgAddressPKs(ZGuid contactOrgPK, ZGuid[] relatedOrgPKs, BusinessObjectFactory factory)
		{
			var query = OrgContactWebUser.GetOrgAddressQuery(contactOrgPK, relatedOrgPKs);
			var orgAddresses = factory.Load<OrgAddress>(query);
			return orgAddresses.Select(o => o.PK).ToArray();
		}

		#endregion
	}
}
