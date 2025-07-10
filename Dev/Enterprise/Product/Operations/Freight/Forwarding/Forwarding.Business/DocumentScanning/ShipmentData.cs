using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ShipmentData),
	Enterprise.Core.Constants.DocManagerCodes.Shipment)]

namespace Enterprise.Freight.Forwarding.Business
{
	using System;
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Freight.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using Enterprise.ZArchitecture.Schema;

	class ShipmentData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ForwardingShipment); } }
		protected override Type CollectionType
		{
			get { return typeof(ForwardingShipmentCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new ForwardingShipmentCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.JobShipment; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("45b639f6-5de7-4f8a-8726-bb2a4497c2bc", "Shipment"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		//query is tested in DocumentScanning in ArchiveEDocsManager class
		public override ZQuery GetQuery(AssemblyDataParams assemblyDataParams)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(JobShipmentSchema.JS_IsShipping, false);

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

			if (!assemblyDataParams.ETDFrom.IsEmpty)
			{
				query.AddToFilter(JobShipmentSchema.JS_E_DEP, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, assemblyDataParams.ETDFrom);
			}

			if (!assemblyDataParams.ETDTo.IsEmpty)
			{
				query.AddToFilter(JobShipmentSchema.JS_E_DEP, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, assemblyDataParams.ETDTo);
			}

			if (!assemblyDataParams.ETAFrom.IsEmpty)
			{
				query.AddToFilter(JobShipmentSchema.JS_E_ARV, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, assemblyDataParams.ETAFrom);
			}

			if (!assemblyDataParams.ETATo.IsEmpty)
			{
				query.AddToFilter(JobShipmentSchema.JS_E_ARV, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, assemblyDataParams.ETATo);
			}

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
			var jobQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			var logQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			logQuery.AddToFilter(JoinCondition.And, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.JobClose.Code);
			logQuery.AddToFilter(JoinCondition.And, StmALogSchema.SL_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);

			if (!assemblyDataParams.JobClosedFrom.IsEmpty)
			{
				logQuery.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, assemblyDataParams.JobClosedFrom);
			}

			if (!assemblyDataParams.JobClosedTo.IsEmpty)
			{
				logQuery.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, assemblyDataParams.JobClosedTo);
			}

			jobQuery.AddSubQuery(logQuery, JoinCondition.And);
			return jobQuery;
		}
	}
}
