using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(JobDeclarationData),
	Enterprise.Core.Constants.DocManagerCodes.JobDeclaration)]

namespace Enterprise.Customs.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Schema;

	public class JobDeclarationData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(BaseJobDeclaration); } }
		protected override Type CollectionType
		{
			get { return typeof(BaseJobDeclarationCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new BaseJobDeclarationCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Customs.JobDeclaration; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("f371f7aa-37d6-4d22-8ac5-fc6b2713d825", "Declaration"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		//query is tested in DocumentScanning in ArchiveEDocsManager class
		public override ZQuery GetQuery(AssemblyDataParams assemblyDataParams)
		{
			ZQuery query = new ZQuery();
			if (assemblyDataParams.IncludeConsignee)
			{
				query.AddToFilter(JobDeclarationSchema.JE_OH_Importer, assemblyDataParams.Organisation);
			}

			if (assemblyDataParams.IncludeConsignor)
			{
				query.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_OH_Supplier, SQLComparisonOperator.Equal, assemblyDataParams.Organisation);
			}

			if (!assemblyDataParams.ETDFrom.IsEmpty)
			{
				query.AddToFilter(JobDeclarationSchema.JE_ExportDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, assemblyDataParams.ETDFrom);
			}

			if (!assemblyDataParams.ETDTo.IsEmpty)
			{
				query.AddToFilter(JobDeclarationSchema.JE_ExportDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, assemblyDataParams.ETDTo);
			}

			if (!assemblyDataParams.ETAFrom.IsEmpty)
			{
				query.AddToFilter(JobDeclarationSchema.JE_DateOfArrival, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, assemblyDataParams.ETAFrom);
			}

			if (!assemblyDataParams.ETATo.IsEmpty)
			{
				query.AddToFilter(JobDeclarationSchema.JE_DateOfArrival, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, assemblyDataParams.ETATo);
			}

			if (assemblyDataParams.IsJobClosedDatesSpecified)
			{
				var subQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
				subQuery.AddSubQuery(assemblyDataParams.GetJobClosedQuery(), JoinCondition.And);
				query.AddToFilter(subQuery);
			}
			return query;
		}
	}
}
