using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

[assembly: AssemblyDataProvider(
	typeof(WarehouseInvoiceAssemblyData),
	Constants.DocManagerCodes.WarehouseInvoice)]

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public class WarehouseInvoiceAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(WhsInvoice); } }
		protected override Type CollectionType
		{
			get { return typeof(WhsInvoiceCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new WhsInvoiceCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.WhsInvoicing; } }
		public override string ReferenceType { get { return Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("b41d0a00-0ed0-4a3b-ba8a-200693e44ffc", "Warehouse Periodic Invoicing Job"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }

		public override ZQuery GetQuery(AssemblyDataParams assemblyDataParams)
		{
			var invoiceQuery = new ZDBOnlyQuery(typeof(WhsInvoice));
			if (assemblyDataParams.IncludeConsignor)
			{
				invoiceQuery.AddToFilter(JobStorageSchema.ET_OH_Client, assemblyDataParams.Organisation);

				var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
				jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_ParentTableCode, JobStorageSchema.Constants.Prefix);
				jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_GC, Env.CurrentCompany.PK);
				if (assemblyDataParams.IsJobClosedDatesSpecified)
				{
					jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_Status, SQLComparisonOperator.Equal, JobHeaderStatus.Closed.Code);
					AddDateTimeFilters(jobHeaderSubQuery, JobHeaderSchema.JH_A_JCL, assemblyDataParams.JobClosedFrom, assemblyDataParams.JobClosedTo);
				}
				invoiceQuery.AddSubQuery(jobHeaderSubQuery, JoinCondition.And);
			}
			else
			{
				invoiceQuery.IsNoResultQuery = true;
			}
			return invoiceQuery;
		}

		void AddDateTimeFilters(ZDBOnlySubQuery query, SchemaDateTimeColumn dateTimeColumn, ZDateTime fromDate, ZDateTime toDate)
		{
			if (!fromDate.IsEmpty)
			{
				query.AddToFilter(dateTimeColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, fromDate);
			}
			if (!toDate.IsEmpty)
			{
				query.AddToFilter(dateTimeColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, toDate);
			}
		}
	}
}