using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(typeof(TransportJobData), Enterprise.Core.Constants.DocManagerCodes.TransportJob)]

namespace Enterprise.Freight.Forwarding.Business
{
	class TransportJobData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CommonCartage); } }
		protected override Type CollectionType
		{
			get { return typeof(CommonCartageCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CommonCartageCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Cartage; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return LocalCartage.Business.ResString.GetMultilingualString("532fe486-e6c0-4e18-a29a-0a1c627aa200", "Transport Job"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		//query is tested in DocumentScanning in ArchiveEDocsManager class
		public override ZQuery GetQuery(AssemblyDataParams assemblyDataParams)
		{
			// Main Address Query
			/* JobHeader of StandAlone JobCartage */
			ZDBOnlySubQuery jobCartageJobHeader = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobCartageJobHeader.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);

			ZDBOnlySubQuery addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, assemblyDataParams.Organisation);
			jobCartageJobHeader.AddSubQuery(addressQuery, JoinCondition.And);

			ZDBOnlyQuery jobCartageDBOnlyQuery = new ZDBOnlyQuery(typeof(CommonCartage));
			jobCartageDBOnlyQuery.AddSubQuery(jobCartageJobHeader, JoinCondition.And);

			// OrgAddress subquery
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, assemblyDataParams.Organisation);

			// DocAddress subquery
			string[] localCartageAddressTypes =
			{
				DocAddressTypes.Codes.LocalCartageCFS,
				DocAddressTypes.Codes.LocalCartageCTO,
				DocAddressTypes.Codes.LocalCartageMSC,
				DocAddressTypes.Codes.LocalCartageYard,
				DocAddressTypes.Codes.LocalCartageService,
				DocAddressTypes.Codes.LocalCartageExporter,
				DocAddressTypes.Codes.LocalCartageImporter,
			};

			ZDBOnlySubQuery docAddressSubquery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddressSubquery.AddToFilter(JobDocAddressSchema.E2_AddressType, localCartageAddressTypes);
			docAddressSubquery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressSubQuery, JoinCondition.And);
			jobCartageDBOnlyQuery.AddSubQuery(JobCartageSchema.PK, docAddressSubquery, JoinCondition.Or);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CommonCartage));
			query.AddToFilter(jobCartageDBOnlyQuery, JoinCondition.Or);

			if (assemblyDataParams.IsDateConstrained)
			{
				query.AddSubQuery(GetSailingSubquery(JobCartageSchema.JJ_JX_Sailing, assemblyDataParams), JoinCondition.And);
			}

			if (assemblyDataParams.IsJobClosedDatesSpecified)
			{
				query.AddSubQuery(assemblyDataParams.GetJobClosedQuery(), JoinCondition.And);
			}

			return query;
		}

		ZDBOnlySubQuery GetSailingSubquery(SchemaColumn foreignKey, AssemblyDataParams assemblyDataParams)
		{
			ZDBOnlySubQuery sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), foreignKey);

			if (!assemblyDataParams.ETDFrom.IsEmpty || !assemblyDataParams.ETDTo.IsEmpty)
			{
				ZDBOnlySubQuery originSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
				if (!assemblyDataParams.ETDFrom.IsEmpty)
				{
					originSubQuery.AddToFilter(JobVoyOriginSchema.JA_E_DEP, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, assemblyDataParams.ETDFrom);
				}

				if (!assemblyDataParams.ETDTo.IsEmpty)
				{
					originSubQuery.AddToFilter(JobVoyOriginSchema.JA_E_DEP, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, assemblyDataParams.ETDTo);
				}

				sailingSubQuery.AddSubQuery(originSubQuery, JoinCondition.And);
			}

			if (!assemblyDataParams.ETAFrom.IsEmpty || !assemblyDataParams.ETATo.IsEmpty)
			{
				ZDBOnlySubQuery destinationSubQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
				if (!assemblyDataParams.ETAFrom.IsEmpty)
				{
					destinationSubQuery.AddToFilter(JobVoyDestinationSchema.JB_E_ARV, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, assemblyDataParams.ETAFrom);
				}

				if (!assemblyDataParams.ETATo.IsEmpty)
				{
					destinationSubQuery.AddToFilter(JobVoyDestinationSchema.JB_E_ARV, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, assemblyDataParams.ETATo);
				}

				sailingSubQuery.AddSubQuery(destinationSubQuery, JoinCondition.And);
			}

			return sailingSubQuery;
		}
	}
}
