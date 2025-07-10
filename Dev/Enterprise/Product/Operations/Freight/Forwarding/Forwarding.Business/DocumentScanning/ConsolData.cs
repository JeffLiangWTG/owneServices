using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ConsolData),
	Enterprise.Core.Constants.DocManagerCodes.Consol)]

namespace Enterprise.Freight.Forwarding.Business
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.Schema;
	using Enterprise.Freight.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using Enterprise.ZArchitecture.Schema;

	class ConsolData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ForwardingConsol); } }
		protected override Type CollectionType
		{
			get { return typeof(ForwardingConsolCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new ForwardingConsolCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.JobConsol; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("716703ef-5b53-4967-95b2-433ef4e4f767", "Consol"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		//query is tested in DocumentScanning in ArchiveEDocsManager class
		public override ZQuery GetQuery(AssemblyDataParams assemblyDataParams)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonConsol));

			if (assemblyDataParams.IncludeConsignor)
			{
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolSchema.JK_OA_SendingForwarderAddress);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, assemblyDataParams.Organisation);
				result.AddSubQuery(orgAddressSubQuery, JoinCondition.Or);
			}

			if (assemblyDataParams.IncludeConsignee)
			{
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolSchema.JK_OA_ReceivingForwarderAddress);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, assemblyDataParams.Organisation);
				result.AddSubQuery(orgAddressSubQuery, JoinCondition.Or);
			}

			if (assemblyDataParams.IsDateConstrained)
			{
				ZDBOnlySubQuery transports = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
				transports.AddSubQuery(GetSailingSubquery(JobConsolTransportSchema.JW_JX, assemblyDataParams), JoinCondition.And);
				result.AddSubQuery(JobConsolSchema.PK, transports, JoinCondition.And);
			}

			if (assemblyDataParams.IsJobClosedDatesSpecified)
			{
				result.AddSubQuery(assemblyDataParams.GetJobClosedQuery(), JoinCondition.And);
			}

			return result;
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
