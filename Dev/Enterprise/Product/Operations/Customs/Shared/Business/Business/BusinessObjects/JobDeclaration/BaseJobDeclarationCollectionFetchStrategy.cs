using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.FetchStrategies
{
	public class BaseJobDeclarationCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public BaseJobDeclarationCollectionFetchStrategy(BaseJobDeclarationCollection collection)
			: base(collection)
		{
		}

		protected new BaseJobDeclarationCollection Collection
		{
			get { return (BaseJobDeclarationCollection)base.Collection; }
		}

		public class RequiredFetchForViewData
		{
			public bool CusEntryInstructionRequiredFetchForView;
			public bool CusEntryHeaderRequiredFetchForView;
			public bool CusEntryLineRequiredFetchForView;
			public bool CusEntryHeaderCusEntryNumRequiredFetchForView;
			public bool CusEntryHeaderEDIMessageRequiredFetchForView;
			public bool CusEntryHeaderStmALogRequiredFetchForView;
			public bool CusContainerRequiredFetchForView;
			public bool CusDecHouseBillRequiredFetchForView;
			public bool CusDecHouseContainerPivotRequiredFetchForView;
			public bool CusDecHouseContainerPackRequiredFetchForView;
			public bool EDIMessageRequiredFetchForView;
			public bool JobComInvoiceHeaderPivotRequiredFetchForView;
			public bool JobComInvoiceLineRequiredFetchForView;
			public bool JobDocAddressRequiredFetchForView;
			public bool JobDocsAndCartageRequiredFetchForView;
		}

		[SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		protected sealed override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);
			var requiredFetchForViewData = CreateNewRequiredFetchForViewData();
			var factory = Collection.Factory;
			foreach (var tableColumn in columns)
			{
				var columnName = tableColumn.ColumnName;
				if (!requiredFetchForViewData.CusDecHouseContainerPackRequiredFetchForView && IsCusDecHouseContainerPackRelatedColumn(columnName))
				{
					requiredFetchForViewData.CusDecHouseContainerPackRequiredFetchForView = true;
					requiredFetchForViewData.CusDecHouseBillRequiredFetchForView = true;
					requiredFetchForViewData.CusDecHouseContainerPivotRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.CusDecHouseContainerPivotRequiredFetchForView && IsCusDecHouseContainerPivotRelatedColumn(columnName))
				{
					requiredFetchForViewData.CusDecHouseBillRequiredFetchForView = true;
					requiredFetchForViewData.CusDecHouseContainerPivotRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.JobComInvoiceLineRequiredFetchForView && IsJobComInvoiceLineRelatedColumn(columnName))
				{
					requiredFetchForViewData.JobComInvoiceHeaderPivotRequiredFetchForView = true;
					requiredFetchForViewData.JobComInvoiceLineRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.CusEntryHeaderCusEntryNumRequiredFetchForView && IsCusEntryNumRelatedColumn(columnName))
				{
					requiredFetchForViewData.CusEntryHeaderRequiredFetchForView = true;
					requiredFetchForViewData.CusEntryHeaderCusEntryNumRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.CusEntryLineRequiredFetchForView && IsCusEntryLineRelatedColumn(columnName))
				{
					requiredFetchForViewData.CusEntryHeaderRequiredFetchForView = true;
					requiredFetchForViewData.CusEntryLineRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.CusEntryHeaderEDIMessageRequiredFetchForView && IsCusEntryHeaderEDIMessageRelatedColumn(columnName))
				{
					requiredFetchForViewData.CusEntryHeaderRequiredFetchForView = true;
					requiredFetchForViewData.CusEntryHeaderEDIMessageRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.CusEntryHeaderStmALogRequiredFetchForView && IsCusEntryHeaderStmALogRelatedColumn(columnName))
				{
					requiredFetchForViewData.CusEntryHeaderRequiredFetchForView = true;
					requiredFetchForViewData.CusEntryHeaderStmALogRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.CusEntryInstructionRequiredFetchForView && IsCusEntryInstructionRelatedColumn(columnName))
				{
					requiredFetchForViewData.CusEntryInstructionRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.CusEntryHeaderRequiredFetchForView && IsCusEntryHeaderRelatedColumn(columnName))
				{
					requiredFetchForViewData.CusEntryHeaderRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.EDIMessageRequiredFetchForView && IsEDIMessageRelatedColumn(columnName))
				{
					requiredFetchForViewData.EDIMessageRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.CusContainerRequiredFetchForView && IsCusContainerRelatedColumn(columnName))
				{
					requiredFetchForViewData.CusContainerRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.CusDecHouseBillRequiredFetchForView && IsCusDecHouseBillRelatedColumn(columnName))
				{
					requiredFetchForViewData.CusDecHouseBillRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.JobComInvoiceHeaderPivotRequiredFetchForView && IsJobComInvoiceHeaderRelatedColumn(columnName))
				{
					requiredFetchForViewData.JobComInvoiceHeaderPivotRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.JobDocAddressRequiredFetchForView && IsJobDocAddressRelatedColumn(columnName))
				{
					requiredFetchForViewData.JobDocAddressRequiredFetchForView = true;
				}
				if (!requiredFetchForViewData.JobDocsAndCartageRequiredFetchForView && IsJobDocsAndCartageRelatedColumn(columnName))
				{
					requiredFetchForViewData.JobDocsAndCartageRequiredFetchForView = true;
				}
				FetchForViewDeclarationAdditionalDataComputation(requiredFetchForViewData, columnName);
			}

			if (requiredFetchForViewData.EDIMessageRequiredFetchForView
				|| requiredFetchForViewData.JobComInvoiceHeaderPivotRequiredFetchForView
				|| requiredFetchForViewData.CusEntryInstructionRequiredFetchForView
				|| requiredFetchForViewData.CusEntryHeaderRequiredFetchForView
				|| requiredFetchForViewData.CusContainerRequiredFetchForView
				|| requiredFetchForViewData.CusDecHouseBillRequiredFetchForView
				|| requiredFetchForViewData.JobDocAddressRequiredFetchForView
				|| requiredFetchForViewData.JobDocsAndCartageRequiredFetchForView
				|| ShouldAddRelatedDataFetchHintsForFirstParse(requiredFetchForViewData))
			{
				foreach (BaseJobDeclaration declaration in businessObjects)
				{
					var declarationPK = declaration.PK;
					var clusterKey = declaration.JE_ClusterKey;
					if (requiredFetchForViewData.CusEntryInstructionRequiredFetchForView)
					{
						factory.AddFetchHint(CusEntryInstructionSchema.CEI_ClusterKey, clusterKey);
					}
					if (requiredFetchForViewData.CusEntryHeaderRequiredFetchForView)
					{
						factory.AddFetchHint(CusEntryHeaderSchema.CH_ClusterKey, clusterKey);
					}
					if (requiredFetchForViewData.CusContainerRequiredFetchForView)
					{
						factory.AddFetchHint(CusContainerSchema.CO_ClusterKey, clusterKey);
					}
					if (requiredFetchForViewData.CusDecHouseContainerPackRequiredFetchForView)
					{
						factory.AddFetchHint(CusDecHouseContainerPackSchema.CW_ClusterKey, clusterKey);
					}
					if (requiredFetchForViewData.CusDecHouseBillRequiredFetchForView)
					{
						factory.AddFetchHint(CusDecHouseBillSchema.CU_ClusterKey, clusterKey);
					}
					if (requiredFetchForViewData.JobComInvoiceHeaderPivotRequiredFetchForView)
					{
						factory.AddFetchHint(JobComInvoiceHeaderSchema.JZ_ClusterKey, clusterKey);

						if (declaration.SupportAdditionalInvoices)
						{
							factory.AddFetchHint(GenPivotSchema.XX_Relation2ID, declarationPK);
						}
					}
					if (requiredFetchForViewData.EDIMessageRequiredFetchForView)
					{
						factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, declarationPK);
					}
					if (requiredFetchForViewData.JobDocAddressRequiredFetchForView)
					{
						factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, declarationPK);
					}
					if (requiredFetchForViewData.JobDocsAndCartageRequiredFetchForView)
					{
						factory.AddFetchHint(JobDocsAndCartageSchema.JP_ParentID, declaration.JE_JS.IsValid ? declaration.JE_JS : declarationPK);
					}
					AddRelatedDataFetchHintsForFirstParse(factory, declaration, requiredFetchForViewData);
				}
				AddRelatedDataFetchHintsForFirstParse(factory, requiredFetchForViewData);
			}

			var shouldAddCusEntryHeaderRelatedDataFetchHints = requiredFetchForViewData.CusEntryHeaderCusEntryNumRequiredFetchForView
						|| requiredFetchForViewData.CusEntryLineRequiredFetchForView
						|| requiredFetchForViewData.CusEntryHeaderEDIMessageRequiredFetchForView
						|| requiredFetchForViewData.CusEntryHeaderStmALogRequiredFetchForView
						|| ShouldAddCusEntryHeaderRelatedDataFetchHints(requiredFetchForViewData);
			var shouldAddCusDecHouseBillRelatedDataFetchHints = requiredFetchForViewData.CusDecHouseContainerPivotRequiredFetchForView || ShouldAddCusDecHouseBillRelatedDataFetchHints(requiredFetchForViewData);
			// Second Parse Group
			if (shouldAddCusEntryHeaderRelatedDataFetchHints
				|| shouldAddCusDecHouseBillRelatedDataFetchHints
				|| requiredFetchForViewData.JobComInvoiceLineRequiredFetchForView
				|| requiredFetchForViewData.JobDocsAndCartageRequiredFetchForView
				|| ShouldAddRelatedDataFetchHintsForSecondParse(requiredFetchForViewData))
			{
				foreach (BaseJobDeclaration declaration in businessObjects)
				{
					if (shouldAddCusDecHouseBillRelatedDataFetchHints)
					{
						if (requiredFetchForViewData.CusDecHouseContainerPivotRequiredFetchForView)
						{
							factory.AddFetchHint(CusDecHouseContainerPivotSchema.CR_ClusterKey, declaration.JE_ClusterKey);
						}
						AddCusDecHouseBillRelatedDataFetchHints(factory, declaration, requiredFetchForViewData);
					}
					if (requiredFetchForViewData.JobComInvoiceLineRequiredFetchForView)
					{
						declaration.AddJobComInvoiceLineFetchHintsIfNeeded();
					}
					if (shouldAddCusEntryHeaderRelatedDataFetchHints)
					{
						if (requiredFetchForViewData.CusEntryLineRequiredFetchForView)
						{
							factory.AddFetchHint(CusEntryLineSchema.CL_ClusterKey, declaration.JE_ClusterKey);
						}
						AddCusEntryHeaderRelatedDataFetchHints(factory, declaration, requiredFetchForViewData);
					}
					if (requiredFetchForViewData.JobDocsAndCartageRequiredFetchForView)
					{
						if (columns.Any(x => x.ColumnName == BaseJobDeclaration.Schema.DeliveryOrPickupCartageCoPK))
						{
							factory.AddFetchHint(OrgAddressSchema.PK, declaration.JE_OA_DeliveryOrPickupCartageCoAddr);
						}
					}
					AddRelatedDataFetchHintsForSecondParse(factory, declaration, requiredFetchForViewData);
				}
				AddRelatedDataFetchHintsForSecondParse(factory, requiredFetchForViewData);
			}
			AddRelatedDataFetchHintsForThirdOrMoreParse(factory, businessObjects, requiredFetchForViewData);
		}

		protected virtual bool IsCusEntryHeaderStmALogRelatedColumn(string columnName)
		{
			return columnName == BaseJobDeclaration.Schema.WarehouseTransactionStatusDescription;
		}

		protected virtual bool IsCusEntryHeaderEDIMessageRelatedColumn(string columnName)
		{
			return false;
		}

		protected virtual bool IsEDIMessageRelatedColumn(string columnName)
		{
			return false;
		}

		protected virtual bool IsCusEntryLineRelatedColumn(string columnName)
		{
			return false;
		}

		protected virtual bool IsCusEntryNumRelatedColumn(string columnName)
		{
			return columnName == BaseJobDeclaration.Schema.DeclarationNumber
					|| columnName == BaseJobDeclaration.Schema.EarliestCustomsEntryIssueDate;
		}

		protected virtual bool IsJobComInvoiceLineRelatedColumn(string columnName)
		{
			return columnName.StartsWith("WorkflowItems+MilestonesIncludingRelated", StringComparison.Ordinal);
		}

		protected virtual bool IsJobComInvoiceHeaderRelatedColumn(string columnName)
		{
			return false;
		}

		protected virtual bool IsCusDecHouseBillRelatedColumn(string columnName)
		{
			return false;
		}

		protected virtual bool IsJobDocAddressRelatedColumn(string columnName)
		{
			return false;
		}

		protected virtual bool IsCusDecHouseContainerPackRelatedColumn(string columnName)
		{
			return false;
		}

		protected virtual bool IsCusDecHouseContainerPivotRelatedColumn(string columnName)
		{
			return columnName == BaseJobDeclaration.Schema.PackagesActualPackageCount;
		}

		protected virtual bool IsCusContainerRelatedColumn(string columnName)
		{
			return columnName == BaseJobDeclaration.Schema.FreightContainerMode;
		}

		protected virtual bool IsJobDocsAndCartageRelatedColumn(string columnName)
		{
			return columnName == BaseJobDeclaration.Schema.DeliveryOrPickupCartageCoPK;
		}

		protected virtual bool ShouldAddRelatedDataFetchHintsForFirstParse(RequiredFetchForViewData requiredFetchForViewData) => false;
		protected virtual void AddRelatedDataFetchHintsForFirstParse(BusinessObjectFactory factory, BaseJobDeclaration declaration, RequiredFetchForViewData requiredFetchForViewData) { }
		protected virtual void AddRelatedDataFetchHintsForFirstParse(BusinessObjectFactory factory, RequiredFetchForViewData requiredFetchForViewData) { }
		protected virtual bool ShouldAddRelatedDataFetchHintsForSecondParse(RequiredFetchForViewData requiredFetchForViewData) => false;
		protected virtual void AddRelatedDataFetchHintsForSecondParse(BusinessObjectFactory factory, BaseJobDeclaration declaration, RequiredFetchForViewData requiredFetchForViewData) { }
		protected virtual void AddRelatedDataFetchHintsForSecondParse(BusinessObjectFactory factory, RequiredFetchForViewData requiredFetchForViewData) { }
		protected virtual void AddRelatedDataFetchHintsForThirdOrMoreParse(BusinessObjectFactory factory, BusinessObject[] businessObjects, RequiredFetchForViewData requiredFetchForViewData) { }
		protected virtual RequiredFetchForViewData CreateNewRequiredFetchForViewData() => new RequiredFetchForViewData();

		protected virtual void FetchForViewDeclarationAdditionalDataComputation(RequiredFetchForViewData requiredFetchForViewData, string columnName) { }
		protected virtual bool IsCusEntryInstructionRelatedColumn(string columnName) => false;
		protected virtual bool IsCusEntryInstructionCusSupportingInfoRelatedColumn(string columnName) => false;

		protected virtual bool IsCusEntryHeaderRelatedColumn(string columnName)
		{
			bool related = columnName == BaseJobDeclaration.Schema.DeclarationNumber
							|| columnName == BaseJobDeclaration.Schema.EarliestCustomsEntryIssueDate
							|| columnName == BaseJobDeclaration.Schema.PhaseStatus
							|| columnName == BaseJobDeclaration.Schema.PhaseStatusDescription;

			if (!related && DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.PK))
			{
				related = columnName == BaseJobDeclaration.Schema.JE_EntryStatus
						|| columnName == BaseJobDeclaration.Schema.JE_EntryStatusDescription
						|| columnName == BaseJobDeclaration.Schema.JE_MessageStatus
						|| columnName == BaseJobDeclaration.Schema.JE_MessageStatusDescription;
			}

			return related;
		}

		protected virtual bool ShouldAddCusEntryHeaderRelatedDataFetchHints(RequiredFetchForViewData requiredFetchForViewData) => false;
		protected virtual void AddCusEntryHeaderRelatedDataFetchHints(BusinessObjectFactory factory, BaseJobDeclaration declaration, RequiredFetchForViewData requiredFetchForViewData)
		{
			var entriesQuery = new ZQuery(CusEntryHeaderSchema.CH_ClusterKey, declaration.JE_ClusterKey);
			if (!declaration.IsInDatabase)
			{
				entriesQuery.FetchOnlyFromLocalCache = true;
			}
			foreach (var entry in factory.Load<CusEntryHeader>(entriesQuery))
			{
				var entryPK = entry.PK;
				if (requiredFetchForViewData.CusEntryHeaderCusEntryNumRequiredFetchForView)
				{
					factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, entryPK);
				}
				if (requiredFetchForViewData.CusEntryLineRequiredFetchForView)
				{
					factory.AddFetchHint(CusEntryLineSchema.CL_CH, entryPK);
				}
				if (requiredFetchForViewData.CusEntryHeaderEDIMessageRequiredFetchForView)
				{
					factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, entryPK);
				}
				if (requiredFetchForViewData.CusEntryHeaderStmALogRequiredFetchForView)
				{
					factory.AddFetchHint(StmALogSchema.SL_Parent, entryPK);
				}
				AddCusEntryHeaderRelatedDataFetchHints(factory, declaration, entry, requiredFetchForViewData);
			}
		}

		protected virtual void AddCusEntryHeaderRelatedDataFetchHints(BusinessObjectFactory factory, BaseJobDeclaration declaration, CusEntryHeader entry, RequiredFetchForViewData requiredFetchForViewData) { }

		protected virtual bool ShouldAddCusDecHouseBillRelatedDataFetchHints(RequiredFetchForViewData requiredFetchForViewData) => false;
		protected virtual void AddCusDecHouseBillRelatedDataFetchHints(BusinessObjectFactory factory, BaseJobDeclaration declaration, RequiredFetchForViewData requiredFetchForViewData)
		{
			var billsQuery = new ZQuery(CusDecHouseBillSchema.CU_ClusterKey, declaration.JE_ClusterKey);
			if (!declaration.IsInDatabase)
			{
				billsQuery.FetchOnlyFromLocalCache = true;
			}
			foreach (Bill bill in factory.Load<Bill>(billsQuery))
			{
				if (requiredFetchForViewData.CusDecHouseContainerPivotRequiredFetchForView)
				{
					factory.AddFetchHint(CusDecHouseContainerPivotSchema.CR_CU_HouseBill, bill.PK);
				}
				AddCusDecHouseBillRelatedDataFetchHints(factory, declaration, bill, requiredFetchForViewData);
			}
		}

		protected virtual void AddCusDecHouseBillRelatedDataFetchHints(BusinessObjectFactory factory, BaseJobDeclaration declaration, Bill bill, RequiredFetchForViewData requiredFetchForViewData) { }
	}
}
