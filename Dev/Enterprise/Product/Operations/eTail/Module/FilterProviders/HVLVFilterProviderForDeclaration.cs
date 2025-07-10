using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Module
{
	public class HVLVFilterProviderForDeclaration : IHVLVFilterProviderForDeclaration
	{
		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string RelatedHVLShipment = "Related HVL Shipment";

			#endregion
		}

		public void AddHVLVFilters(IModuleFilterCollection filterCollection, BusinessObjectFactory factory)
		{
			var filters = Argument.NotNull(filterCollection as ModuleFilterCollection, nameof(filterCollection));
			var relatedHVLShipmentFilter = filters.AddGuidFilter(Descriptions.RelatedHVLShipment, ModuleIDs.JobShipment, GetDeclarationFromShipmentQuery, HVLShipmentsList(factory));
			relatedHVLShipmentFilter.MultilingualDescription = ResString.GetMultilingualString("7c35daf9-dba4-489c-ba12-ed43d12c0982", "Related HVL Shipment");
		}

		ZQuery GetDeclarationFromShipmentQuery(ZGuid shipmentPK)
		{
			var headerSubQuery = new ZDBOnlySubQuery(typeof(HVLVConsignmentHeader), HVLVConsignmentHeaderSchema.PK);
			headerSubQuery.AddToFilter(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipmentPK);

			var consignmentSubQueryForImportDeclaration = new ZDBOnlySubQuery(typeof(HVLVConsignment), HVLVConsignmentSchema.HVC_JE_ImportDeclaration);
			consignmentSubQueryForImportDeclaration.AddSubQuery(HVLVConsignmentSchema.HVC_HCH_Header, headerSubQuery, JoinCondition.And);

			var consignmentSubQueryForExportDeclaration = new ZDBOnlySubQuery(typeof(HVLVConsignment), HVLVConsignmentSchema.HVC_JE_ExportDeclaration);
			consignmentSubQueryForExportDeclaration.AddSubQuery(HVLVConsignmentSchema.HVC_HCH_Header, headerSubQuery, JoinCondition.And);

			var declarationQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			declarationQuery.AddSubQuery(consignmentSubQueryForImportDeclaration, JoinCondition.And);
			declarationQuery.AddSubQuery(consignmentSubQueryForExportDeclaration, JoinCondition.Union);
			return declarationQuery;
		}

		ForwardingShipmentCollection HVLShipmentsList(BusinessObjectFactory factory)
		{
			var collection = new ForwardingShipmentCollection(factory);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Shipment Type", "Property5", ZBool.True)); //Default only HVL Shipments
			return collection;
		}
	}
}
