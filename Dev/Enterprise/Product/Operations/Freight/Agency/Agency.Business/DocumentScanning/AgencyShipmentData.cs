using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(
	typeof(AgencyShipmentData),
	Enterprise.Core.Constants.DocManagerCodes.AgencyShipment)]

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class AgencyShipmentData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(AgencyShipment); } }
		protected override Type CollectionType
		{
			get { return typeof(AgencyShipmentCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AgencyShipmentCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AgencyBillOfLading; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("e22c4465-735b-4de0-83dc-58338290e8b1", "Shipping Manager Shipment"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		//query is tested in DocumentScanning in ArchiveEDocsManager class
		public override ZQuery GetQuery(AssemblyDataParams assemblyDataParams)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobShipmentSchema.JS_IsShipping, true);

			List<string> orgs = new List<string>();
			if (assemblyDataParams.IncludeConsignee)
			{
				orgs.Add(DocAddressTypes.Codes.ConsigneeDocumentaryAddress);
			}

			if (assemblyDataParams.IncludeConsignor)
			{
				orgs.Add(DocAddressTypes.Codes.ConsignorDocumentaryAddress);
			}

			ZDBOnlyQuery subQuery = null;
			if (orgs.Count > 0)
			{
				subQuery = FilterByOrg(assemblyDataParams.Organisation, orgs.ToArray());
				query.AddToFilter(subQuery);
			}

			query.AddToFilter(DateRangeFilter(JobShipmentSchema.JS_E_DEP, assemblyDataParams.ETDFrom, assemblyDataParams.ETDTo));
			query.AddToFilter(DateRangeFilter(JobShipmentSchema.JS_E_ARV, assemblyDataParams.ETAFrom, assemblyDataParams.ETATo));

			if (assemblyDataParams.IsJobClosedDatesSpecified && subQuery != null)
			{
				subQuery.AddSubQuery(GetJobClosedQuery(assemblyDataParams), JoinCondition.And);
			}

			return query;
		}

		ZDBOnlyQuery FilterByOrg(ZGuid orgPK, string[] addressType)
		{
			ZDBOnlySubQuery orgAddressFilter = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressFilter.AddToFilter(OrgAddressSchema.OA_OH, orgPK);

			ZDBOnlySubQuery docAddressFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddressFilter.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressFilter, JoinCondition.And);
			docAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonShipment));
			result.AddSubQuery(JobShipmentSchema.PK, docAddressFilter, JoinCondition.And);

			return result;
		}

		ZDBOnlySubQuery GetJobClosedQuery(AssemblyDataParams assemblyDataParams)
		{
			ZDBOnlySubQuery logQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			logQuery.AddToFilter(JoinCondition.And, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.JobClose.Code);
			logQuery.AddToFilter(JoinCondition.And, StmALogSchema.SL_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
			logQuery.AddToFilter(DateRangeFilter(StmALogSchema.SL_PostedTimeUtc, assemblyDataParams.JobClosedFrom, assemblyDataParams.JobClosedTo));

			ZDBOnlySubQuery jobQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobQuery.AddSubQuery(logQuery, JoinCondition.And);
			return jobQuery;
		}

		ZQuery DateRangeFilter(SchemaDateTimeColumn column, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery result = new ZQuery();

			if (!fromDate.IsEmpty)
			{
				result.AddToFilter(column, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, fromDate);
			}

			if (!toDate.IsEmpty)
			{
				result.AddToFilter(column, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, toDate);
			}

			return result;
		}
	}
}
