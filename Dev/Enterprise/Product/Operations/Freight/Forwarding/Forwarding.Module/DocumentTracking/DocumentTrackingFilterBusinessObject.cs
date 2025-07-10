using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public class DocumentTrackingFilterBusinessObject : JobShipmentFilterBusinessObject
	{
		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return null;
		}

		protected override ModuleFilterCollection GetJobShipmentModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetJobShipmentModuleFiltersCore();
			filters.AddFilter(base.GetModuleFilterThatOverridesAllOtherFiltersCore());

			ApplySubGroupWrapper(filters); // filters added before this line will be treated as filters on JobShipment.

			AddDocNumberFilters(filters);
			AddDocDateFilters(filters);
			AddDocOrganisationFilters(filters);
			AddDocStatusAndFlagsFilters(filters);

			return filters;
		}

		void ApplySubGroupWrapper(ModuleFilterCollection filters)
		{
			var lookups = new Dictionary<ModuleFilterSubGroup, JobShipmentFiltersSubGroup>();

			foreach (ModuleFilter filter in filters)
			{
				var innerSubGroup = filter.SubGroup ?? ModuleFilterSubGroup.Default;
				filter.SubGroup = GetSubGroupWrapper((ModuleFilterSubGroup)innerSubGroup, lookups);
			}
		}

		JobShipmentFiltersSubGroup GetSubGroupWrapper(ModuleFilterSubGroup subGroup, Dictionary<ModuleFilterSubGroup, JobShipmentFiltersSubGroup> lookups)
		{
			JobShipmentFiltersSubGroup result;

			if (!lookups.TryGetValue(subGroup, out result))
			{
				if (subGroup == ModuleFilterSubGroup.Default)
				{
					result = new JobShipmentFiltersSubGroup(ShipmentSubGroup);
				}
				else
				{
					result = new JobShipmentFiltersSubGroup(subGroup, GetSubGroupWrapper((ModuleFilterSubGroup)subGroup.Parent, lookups));
				}

				lookups.Add(subGroup, result);
			}

			return result;
		}

		#region Numbers

		void AddDocNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(FreightConstants.NumberFilterTypes.DocumentNumber, JobRequiredDocumentSchema.EQ_DocNumber).
				MultilingualDescription = ResString.GetMultilingualString("Forwarding|DocumentTrackingFilter|DocumentNumber", "Document #");
		}

		#endregion

		#region Dates

		void AddDocDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("HXD Received Date", JobRequiredDocumentSchema.EQ_RcvFromCustomsBroker).MultilingualDescription = ResString.GetMultilingualString("Forwarding|DocumentTrackingFilter|HXDReceivedDate", "HXD Received Date");
			filters.AddDateFilter("HXD Sent Date", JobRequiredDocumentSchema.EQ_SntToCustomsBroker).MultilingualDescription = ResString.GetMultilingualString("Forwarding|DocumentTrackingFilter|HXDSentDate", "HXD Sent Date");
			filters.AddDateFilter("HXD Return Date", JobRequiredDocumentSchema.EQ_ReturnToShipper).MultilingualDescription = ResString.GetMultilingualString("Forwarding|DocumentTrackingFilter|HXDReturnDate", "HXD Return Date");
		}

		#endregion

		#region Organisations

		void AddDocOrganisationFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter(FreightConstants.OrgFilterTypes.DocumentOwner, ModuleIDs.Organisation, JobRequiredDocumentSchema.EQ_OH_DocumentOwner, new OrganisationsFindBoxCollection(Factory)).
				MultilingualDescription = ResString.GetMultilingualString("Forwarding|DocumentTrackingFilter|DocumentOwner", "Doc Owner");
		}

		#endregion

		#region Statuses

		void AddDocStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleFilter hXDStatusFilter = filters.AddTextFilter("HXD Status", GetHXDDocumentStatusFilter, new HXDStatusList());
			hXDStatusFilter.Category = FilterCategories.StatusAndFlags;
			hXDStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|DocumentTrackingFilter|HXDStatus", "HXD Status");

			filters.AddTextFilter("Document Type", JobRequiredDocumentSchema.EQ_DocType, EQ_DocType_List).MultilingualDescription = ResString.GetMultilingualString("Forwarding|DocumentTrackingFilter|DocumentType", "Document Type");
		}

		ZQuery GetHXDDocumentStatusFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobRequiredDocumentSchema.EQ_DocType, Core.Constants.RefDocTypes.HeXiaoDan);

			switch (value)
			{
				case HXDStatusList.Codes.NotReceivedFromShipper:
					query.AddToFilter(GetHXDStatusFilter(false, false, false, false));
					break;

				case HXDStatusList.Codes.OnHandForBroker:
					query.AddToFilter(GetHXDStatusFilter(true, false, false, false));
					break;

				case HXDStatusList.Codes.WithBroker:
					query.AddToFilter(GetHXDStatusFilter(true, true, false, false));
					break;

				case HXDStatusList.Codes.OnHandForShipper:
					query.AddToFilter(GetHXDStatusFilter(true, true, true, false));
					break;

				case HXDStatusList.Codes.OnHandForBrokerShipper:
					ZQuery compoundQuery = new ZQuery(GetHXDStatusFilter(true, false, false, false), JoinCondition.Or, GetHXDStatusFilter(true, true, true, false));
					query.AddToFilter(compoundQuery);
					break;
			}

			return query;
		}

		ZQuery GetHXDStatusFilter(bool received, bool sentToBroker, bool receivedFromBroker, bool returnedToShipper)
		{
			ZQuery result = new ZQuery();

			result.AddToFilter(JobRequiredDocumentSchema.EQ_DateReceived, (received ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal), ZDateTimeOffset.Empty);
			result.AddToFilter(JobRequiredDocumentSchema.EQ_SntToCustomsBroker, (sentToBroker ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal), ZDateTime.Empty);
			result.AddToFilter(JobRequiredDocumentSchema.EQ_RcvFromCustomsBroker, (receivedFromBroker ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal), ZDateTime.Empty);
			result.AddToFilter(JobRequiredDocumentSchema.EQ_ReturnToShipper, (returnedToShipper ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal), ZDateTime.Empty);

			return result;
		}

		#endregion

		#region Workflow Filters

		protected override bool ShouldAddWorkflowFilters => false;

		#endregion

		#region CRM Security Filters

		protected override bool ShouldAddCRMSecurityFilters => false;

		#endregion

		#region Lookups

		#region EQ_DocType_List

		CodeDescriptionPairList EQ_DocType_List
		{
			get
			{
				if (docType_List == null)
				{
					docType_List = new CodeDescriptionPairList();
					RefDocTypeCollection docTypes = GetDocTypesForCategory();

					foreach (RefDocType docType in docTypes)
					{
						if (docType.RT_DocType != Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument &&
							docType.RT_DocType != Core.Constants.RefDocTypes.InternallyCreatedPublicDocument)
						{
							docType_List.AddPair(docType.RT_DocType, docType.RT_DescMultilingual);
						}
					}
				}

				return docType_List;
			}
		}
		CodeDescriptionPairList docType_List;

		RefDocTypeCollection GetDocTypesForCategory()
		{
			ZQuery visibleQuery = new ZQuery(RefDocTypeSchema.RT_IsActive, ZBool.True);

			RefDocTypeCollection docTypes = new RefDocTypeCollection(Factory);
			docTypes.AdditionalFilter = visibleQuery;
			docTypes.ApplySort(RefDocType.Schema.RT_DocType, ListSortDirection.Ascending);
			return docTypes;
		}

		#endregion

		#region HXD Status List

		public class HXDStatusList : CodeDescriptionPairList
		{
			public static class Codes
			{
				public const string NotReceivedFromShipper = "NRS";
				public const string OnHandForBroker = "ONB";
				public const string WithBroker = "WBR";
				public const string OnHandForShipper = "ONS";
				public const string OnHandForBrokerShipper = "OBS";
			}

			public static class Descriptions
			{
				public static string NotReceivedFromShipper { get { return Res.GetString("Forwarding|DocumentTrackingFilter|NotReceivedFromShipper", "Not Received from Shipper"); } }
				public static string OnHandForBroker { get { return Res.GetString("Forwarding|DocumentTrackingFilter|OnHandForBroker", "On Hand (For Broker)"); } }
				public static string WithBroker { get { return Res.GetString("Forwarding|DocumentTrackingFilter|WithBroker", "With Broker"); } }
				public static string OnHandForShipper { get { return Res.GetString("Forwarding|DocumentTrackingFilter|OnHandForShipper", "On Hand (For Shipper)"); } }
				public static string OnHandForBrokerShipper { get { return Res.GetString("Forwarding|DocumentTrackingFilter|OnHandForBrokerShipper", "On Hand (For Broker/Shipper)"); } }
			}

			public HXDStatusList()
			{
				AddPair(Codes.NotReceivedFromShipper, Descriptions.NotReceivedFromShipper);
				AddPair(Codes.OnHandForBroker, Descriptions.OnHandForBroker);
				AddPair(Codes.WithBroker, Descriptions.WithBroker);
				AddPair(Codes.OnHandForShipper, Descriptions.OnHandForShipper);
				AddPair(Codes.OnHandForBrokerShipper, Descriptions.OnHandForBrokerShipper);
			}
		}

		#endregion

		#endregion

		#region SubGroups

		ModuleFilterSubGroup ShipmentSubGroup
		{
			get { return shipmentSubGroup ?? (shipmentSubGroup = new ShipmentFilterSubGroup()); }
		}
		ShipmentFilterSubGroup shipmentSubGroup;

		class ShipmentFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobDocsAndCartageSchema.JP_ParentID);
				shipmentQuery.AddToFilter(filter);

				ZDBOnlySubQuery docsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobRequiredDocumentSchema.EQ_ParentID);
				docsAndCartageQuery.AddSubQuery(shipmentQuery, JoinCondition.And);

				ZDBOnlyQuery requiredDocsQuery = new ZDBOnlyQuery(typeof(JobRequiredDocument));
				requiredDocsQuery.AddSubQuery(docsAndCartageQuery, JoinCondition.And);

				return requiredDocsQuery;
			}
		}

		class JobShipmentFiltersSubGroup : ModuleFilterSubGroup
		{
			public JobShipmentFiltersSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{
				this.innerSubGroup = ModuleFilterSubGroup.Default;
			}

			public JobShipmentFiltersSubGroup(ModuleFilterSubGroup innerSubGroup, ModuleFilterSubGroup parent)
				: base(parent)
			{
				this.innerSubGroup = innerSubGroup ?? ModuleFilterSubGroup.Default;
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return innerSubGroup.GetSubQuery(filter);
			}

			readonly ModuleFilterSubGroup innerSubGroup;
		}

		#endregion
	}
}
