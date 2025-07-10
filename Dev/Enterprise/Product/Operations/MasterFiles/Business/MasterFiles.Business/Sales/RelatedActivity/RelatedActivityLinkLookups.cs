using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RelatedActivityLinkLookups : ZLookups
	{
		#region New

		public static RelatedActivityLinkLookups New(RelatedActivityLink parent)
		{
			RelatedActivityLinkLookups result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(parent);
			}
			else
			{
				result = new RelatedActivityLinkLookups(parent);
			}
			return result;
		}

		protected delegate RelatedActivityLinkLookups NewDelegate(RelatedActivityLink parent);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		protected RelatedActivityLinkLookups(RelatedActivityLink parent)
			: base(parent)
		{
		}

		internal RelatedActivityLink Link
		{
			get { return (RelatedActivityLink)Parent; }
		}

		#region RelatableActivityTypes

		public ICodeDescriptionPairList RelatableActivityTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (var typeCodeInfoPair in RelatableActivityTypeDefinitions)
				{
					if (typeCodeInfoPair.Key == RelatableActivityTypeList.Codes.CrmOpportunityManager)
					{
						continue;
					}
					result.AddPair(typeCodeInfoPair.Key, typeCodeInfoPair.Value.Description);
				}
				result.SortByDescription();

				return result;
			}
		}

		#endregion

		#region ToActivityCollection

		public IBusinessObjectCollection ToActivityCollection
		{
			get
			{
				ZString typeCode = Link.ToActivityTypeForBinding;
				RelatableActivityTypeDefinition relatedActivityTypeInformation;
				if (RelatableActivityTypeDefinitions.TryGetValue(typeCode, out relatedActivityTypeInformation))
				{
					return relatedActivityTypeInformation.GetNewCollection();
				}

				return null;
			}
		}

		#endregion

		#region RelatableActivityTypeDefinitions

		public IDictionary<string, RelatableActivityTypeDefinition> RelatableActivityTypeDefinitions
		{
			get { return GetRelatableActivityTypeDefinitionsCore(Factory); }
		}

		IDictionary<string, RelatableActivityTypeDefinition> GetRelatableActivityTypeDefinitionsCore(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("RelatableActivityTypeDefinitions", GetNewRelatableActivityTypeDefinitions);
		}

		protected virtual IDictionary<string, RelatableActivityTypeDefinition> GetNewRelatableActivityTypeDefinitions()
		{
			return new Dictionary<string, RelatableActivityTypeDefinition>
			{
				{ RelatableActivityTypeList.Codes.Communication,          new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.Communication,         ModuleIDs.Communication,            ControllerIDs.Communication,            OrgSalesCallSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.OpportunityManager,     new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.OpportunityManager,    ModuleIDs.Opportunity,              ControllerIDs.Opportunity,              OrgOpportunitySchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.CrmOpportunityManager,  new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.CrmOpportunityManager, ModuleIDs.CrmOpportunity,           ControllerIDs.CrmOpportunity,           CrmOpportunitySchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.CampaignManagement,     new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.CampaignManagement,    ModuleIDs.GlbCompanyCampaign,       ControllerIDs.GlbCompanyCampaign,       GlbCompanyCampaignSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.InquiryManager,         new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.InquiryManager,        ModuleIDs.SalesEnquiry,             ControllerIDs.SalesEnquiry,             OrgColdCallRegisterSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.OneOffQuotes,           new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.OneOffQuotes,         ModuleIDs.OneOffQuotes,             ControllerIDs.OneOffQuotes,             ViewQuotedBookingSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.Quotations,             new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.Quotations,            ModuleIDs.Quotations,               ControllerIDs.Quotations,               RatingHeaderSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.ClientRates,            new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.ClientRates,           ModuleIDs.ClientRates,              ControllerIDs.ClientRates,              RatingHeaderSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.Shipments,              new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.Shipments,             ModuleIDs.JobShipment,              ControllerIDs.JobShipment,              JobShipmentSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.Consolidations,         new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.Consolidations,        ModuleIDs.JobConsol,                ControllerIDs.JobConsol,                JobConsolSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.TransportBooking,       new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.TransportBooking,      ModuleIDs.DtbBooking,               ControllerIDs.DtbBooking,               DtbBookingSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.TransportConsignments,  new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.TransportConsignments, ModuleIDs.DtbBookingConsignment,    ControllerIDs.DtbBookingConsignment,    DtbBookingSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.CustomsDeclarations,    new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.CustomsDeclarations,   ModuleIDs.Customs.JobDeclaration,   ControllerIDs.Customs.JobDeclaration,   JobDeclarationSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.Orders,                 new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.Orders,                ModuleIDs.Orders,                   ControllerIDs.Orders,                   JobOrderHeaderSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.LocalTransport,         new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.LocalTransport,        ModuleIDs.Cartage,                  ControllerIDs.Cartage,                  JobCartageSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.WarehouseReceive,       new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.WarehouseReceive,      ModuleIDs.WhsReceive,               ControllerIDs.WhsReceive,               WhsDocketSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.WarehouseOrders,        new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.WarehouseOrders,       ModuleIDs.WhsOrder,                 ControllerIDs.WhsOrder,                 WhsDocketSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.WorkItem,               new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.WorkItem,              ModuleIDs.WorkItem,                 ControllerIDs.WorkItem,                 WorkItemSchema.Constants.Prefix) },
				{ RelatableActivityTypeList.Codes.ArInvoice,              new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.ArInvoice,             ModuleIDs.ARTransaction,            ControllerIDs.ARInvoice,                AccTransactionHeaderSchema.Constants.Prefix,    true) },
				{ RelatableActivityTypeList.Codes.Projects,               new RelatableActivityTypeDefinition(RelatableActivityTypeList.Descriptions.Projects,              ModuleIDs.Project,                  ControllerIDs.Project,                  WorkProjectSchema.Constants.Prefix) }
			};
		}

		public static IDictionary<string, RelatableActivityTypeDefinition> GetRelatableActivityTypeDefinitions(BusinessObjectFactory factory)
		{
			var lookups = RelatedActivityLinkLookups.New(null);
			return lookups.GetRelatableActivityTypeDefinitionsCore(factory);
		}

		public static IDictionary<string, Type> GetTablePrefixesThatShouldBeLoadedWithSpecificElementType(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("RelatableActivityTypeDefinitions.TablePrefixesThatShouldBeLoadedWithSpecificElementType", () =>
				{
					var result = new Dictionary<string, Type>();
					foreach (var typeDef in RelatedActivityLinkLookups.GetRelatableActivityTypeDefinitions(factory))
					{
						if (typeDef.Value.ShouldUseElementTypeWhenLoading)
						{
							result.Add(typeDef.Value.TablePrefix, typeDef.Value.ElementType);
						}
					}

					return result;
				});
		}

		public static IDictionary<ZString, ZString> GetTablePrefixesForSuperAndSubActivities(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("RelatableActivityTypeDefinitions.TablePrefixesForSuperAndSubActivities", () =>
				{
					return new Dictionary<ZString, ZString>
						{
							{ GlbCompanyCampaignSchema.Constants.Prefix, GlbCompanyCampaignItemSchema.Constants.Prefix }
						};
				});
		}

		#endregion
	}
}
