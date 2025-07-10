using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ZArchitecture.Modules.ModuleId.CusDec)]
	public class JobDeclarationCollection : TypeSafeJobDeclarationCollection
	{
		public JobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory, companyPkToFilterOn)
		{
		}

		protected override bool AllowNewCore => false;

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new JobDeclarationCollectionFetchStrategy(this);

		class JobDeclarationCollectionFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationCollectionFetchStrategy
		{
			public JobDeclarationCollectionFetchStrategy(JobDeclarationCollection collection)
				: base(collection)
			{
			}

			protected new JobDeclarationCollection Collection => (JobDeclarationCollection)base.Collection;

			public class USRequiredFetchForViewData : RequiredFetchForViewData
			{
				public bool CusEntryNumRequiredFetchForView;

				public bool StatementHeaderQueryRequiredFetchForView
				{
					get => statementHeaderQueryRequiredFetchForView;
					set
					{
						statementHeaderQueryRequiredFetchForView = value;
						if (value)
						{
							StatementQuery = StatementQuery ?? new ZQuery() { DefaultJoinCondition = JoinCondition.Or };
						}
						else
						{
							StatementQuery = null;
						}
					}
				}
				bool statementHeaderQueryRequiredFetchForView;
				public ZQuery StatementQuery;

				public bool DISStatusFetchForView
				{
					get => disStatusFetchForView;
					set
					{
						disStatusFetchForView = value;
						if (value)
						{
							DocumentQueryParentPKs = DocumentQueryParentPKs ?? new List<ZGuid>();
						}
						else
						{
							DocumentQueryParentPKs = null;
						}
					}
				}
				bool disStatusFetchForView;
				public List<ZGuid> DocumentQueryParentPKs;

				public bool ISFStatusFetchForView
				{
					get => isfStatusFetchForView;
					set
					{
						isfStatusFetchForView = value;
						if (value)
						{
							ISFBillQuery = ISFBillQuery ?? new ZQuery() { DefaultJoinCondition = JoinCondition.Or };
							ISFBills = ISFBills ?? new List<Bill>();
						}
						else
						{
							ISFBillQuery = null;
							ISFBills = null;
						}
					}
				}
				bool isfStatusFetchForView;
				public ZQuery ISFBillQuery;
				public List<Bill> ISFBills;
			}

			protected override RequiredFetchForViewData CreateNewRequiredFetchForViewData() => new USRequiredFetchForViewData();
			protected override void FetchForViewDeclarationAdditionalDataComputation(RequiredFetchForViewData requiredFetchForViewData, string columnName)
			{
				base.FetchForViewDeclarationAdditionalDataComputation(requiredFetchForViewData, columnName);
				var usRequiredFetchForViewData = (USRequiredFetchForViewData)requiredFetchForViewData;
				switch (columnName)
				{
					case JobDeclaration.Schema.PaymentStatus:
					case JobDeclaration.Schema.PaymentStatusDesc:
					case JobDeclaration.Schema.StatementNo:
					case JobDeclaration.Schema.StatementPaidDate:
					case JobDeclaration.Schema.StatementStatus:
					case JobDeclaration.Schema.StatementStatusDesc:
						usRequiredFetchForViewData.CusEntryNumRequiredFetchForView = true;
						usRequiredFetchForViewData.StatementHeaderQueryRequiredFetchForView = true;
						break;

					case JobDeclaration.Schema.DISStatus:
					case JobDeclaration.Schema.DISStatusDescription:
						usRequiredFetchForViewData.DISStatusFetchForView = true;
						break;

					case JobDeclaration.Schema.ISFBillStatus:
					case JobDeclaration.Schema.ISFBillStatusDescription:
						usRequiredFetchForViewData.ISFStatusFetchForView = true;
						break;
				}
			}

			protected override bool ShouldAddRelatedDataFetchHintsForFirstParse(RequiredFetchForViewData requiredFetchForViewData)
			{
				return base.ShouldAddRelatedDataFetchHintsForFirstParse(requiredFetchForViewData) ||
					(requiredFetchForViewData is USRequiredFetchForViewData usRequiredFetchForViewData
					&& (usRequiredFetchForViewData.CusEntryNumRequiredFetchForView
						|| usRequiredFetchForViewData.StatementHeaderQueryRequiredFetchForView
						|| usRequiredFetchForViewData.DISStatusFetchForView));
			}

			protected override void AddRelatedDataFetchHintsForFirstParse(BusinessObjectFactory factory, BaseJobDeclaration baseDeclaration, RequiredFetchForViewData requiredFetchForViewData)
			{
				base.AddRelatedDataFetchHintsForFirstParse(factory, baseDeclaration, requiredFetchForViewData);
				var declaration = (JobDeclaration)baseDeclaration;
				var usRequiredFetchForViewData = (USRequiredFetchForViewData)requiredFetchForViewData;
				if (usRequiredFetchForViewData.CusEntryNumRequiredFetchForView)
				{
					factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, declaration.PK);
				}

				if (usRequiredFetchForViewData.StatementQuery is ZQuery statementQuery && !declaration.IsExport)
				{
					var entryNumber = declaration.ImportEntryNumber;
					var entryFilerCode = declaration.EntryFilerCode;
					if (!entryNumber.IsEmpty && !entryFilerCode.IsEmpty)
					{
						var statementLineQuery = new ZQuery(CusStatementLineSchema.B3_EntryNum, entryNumber);
						statementLineQuery.AddToFilter(CusStatementLineSchema.B3_EntryFilerCode, entryFilerCode);
						statementLineQuery.AddToFilter(CusStatementLineSchema.B3_Status, SQLComparisonOperator.NotEqual, StatementLineStatusList.Codes.Deleted);
						factory.AddFetchHint(CusStatementLineSchema.Instance, statementLineQuery);

						var statementHeaderQuery = new ZDBOnlyQuery(typeof(CusStatementHeader));
						statementHeaderQuery.AddToFilter(CusStatementHeaderSchema.B2_GC, declaration.Company.PK);
						var statementLineSubQuery = new ZDBOnlySubQuery(typeof(CusStatementLine), CusStatementLineSchema.B3_B2);
						statementLineSubQuery.AddToFilter(statementLineQuery);
						statementHeaderQuery.AddSubQuery(statementLineSubQuery, JoinCondition.And);
						statementQuery.AddToFilter(statementHeaderQuery);
					}
				}

				if (usRequiredFetchForViewData.DocumentQueryParentPKs is List<ZGuid> parentPKs && declaration.IsDISStatusApplicable)
				{
					parentPKs.Add(declaration.JE_JS.IsValid ? declaration.JE_JS : declaration.PK);
				}
			}

			protected override bool ShouldAddRelatedDataFetchHintsForSecondParse(RequiredFetchForViewData requiredFetchForViewData)
			{
				return base.ShouldAddRelatedDataFetchHintsForSecondParse(requiredFetchForViewData) ||
					(requiredFetchForViewData is USRequiredFetchForViewData usRequiredFetchForViewData
					&& usRequiredFetchForViewData.ISFStatusFetchForView);
			}

			protected override void AddRelatedDataFetchHintsForFirstParse(BusinessObjectFactory factory, RequiredFetchForViewData requiredFetchForViewData)
			{
				base.AddRelatedDataFetchHintsForFirstParse(factory, requiredFetchForViewData);
				var usRequiredFetchForViewData = (USRequiredFetchForViewData)requiredFetchForViewData;
				if (usRequiredFetchForViewData.StatementQuery is ZQuery statementQuery && !statementQuery.IsEmpty)
				{
					factory.AddFetchHint(CusStatementHeaderSchema.Instance, statementQuery);
				}

				if (usRequiredFetchForViewData.DocumentQueryParentPKs is List<ZGuid> parentPKs && parentPKs.Count > 0)
				{
					var documentQuery = new ZQuery() { AllowTableValuedParameters = true };
					var requiredDocumentQuery = new ZDBOnlyQuery(typeof(JobRequiredDocument));
					var docsAndCartageQuery = new ZDBOnlySubQuery(typeof(Freight.Business.JobDocsAndCartage), JobDocsAndCartageSchema.PK);
					docsAndCartageQuery.AddToFilter(new ZQuery(JobDocsAndCartageSchema.JP_ParentID, parentPKs));
					requiredDocumentQuery.AddSubQuery(JobRequiredDocumentSchema.EQ_ParentID, docsAndCartageQuery, JoinCondition.And);
					documentQuery.AddToFilter(requiredDocumentQuery);

					var requiredDocuments = factory.Load<JobRequiredDocument>(documentQuery);
					requiredDocuments.ForEach(x => x.FetchStrategy.FetchForLoadChildEditableObjects());
				}
			}

			protected override void AddRelatedDataFetchHintsForSecondParse(BusinessObjectFactory factory, BaseJobDeclaration baseDeclaration, RequiredFetchForViewData requiredFetchForViewData)
			{
				base.AddRelatedDataFetchHintsForSecondParse(factory, baseDeclaration, requiredFetchForViewData);
				var declaration = (JobDeclaration)baseDeclaration;
				var usRequiredFetchForViewData = (USRequiredFetchForViewData)requiredFetchForViewData;
				if (usRequiredFetchForViewData.ISFBills is List<Bill> billList && declaration.IsImport && declaration.IsSea)
				{
					billList.AddRange(declaration.LowestBills.Cast<Bill>());
				}
			}

			protected override void AddRelatedDataFetchHintsForSecondParse(BusinessObjectFactory factory, RequiredFetchForViewData requiredFetchForViewData)
			{
				base.AddRelatedDataFetchHintsForSecondParse(factory, requiredFetchForViewData);
				var usRequiredFetchForViewData = (USRequiredFetchForViewData)requiredFetchForViewData;
				if (usRequiredFetchForViewData.ISFBills is List<Bill> billList && billList.Count > 0)
				{
					var batchSize = 500;
					foreach (var billBatch in billList.Batch(batchSize))
					{
						var isfBillQuery = new ZQuery() { DefaultJoinCondition = JoinCondition.Or };
						billBatch.ForEach(bill =>
						{
							isfBillQuery.AddToFilter(Common.US.ISF.ISFStatusHelper.GetISFBillDataQuery(bill.IsMasterBill, new ZString[] { bill.US_UI_NKBillIssuerSCAC + bill.CU_BillNum }));
						});
						if (!isfBillQuery.IsEmpty)
						{
							var isfBills = factory.Load<Integration.Customs.US.ISF.ICusISFBill>(isfBillQuery);
							if (isfBills.Length > 0)
							{
								var isfHeaderPKs = isfBills.Select(x => x.BB_BF).Distinct().ToArray();
								factory.AddFetchHint(CusISFHeaderSchema.Instance, new ZQuery(CusISFHeaderSchema.PK, isfHeaderPKs));
								factory.AddFetchHint(EDIMessageSchema.Instance, new ZQuery(EDIMessageSchema.EM_LinkUniqueID, isfHeaderPKs));
							}
						}
					}
				}
			}

			protected override bool IsCusDecHouseBillRelatedColumn(string columnName)
			{
				return base.IsCusDecHouseBillRelatedColumn(columnName) || columnName == JobDeclaration.Schema.ISFBillStatus || columnName == JobDeclaration.Schema.ISFBillStatusDescription;
			}

			protected override bool IsJobComInvoiceLineRelatedColumn(string columnName)
			{
				return base.IsJobComInvoiceLineRelatedColumn(columnName) || columnName == JobDeclaration.Schema.EarliestExportDate;
			}

			protected override bool IsCusEntryHeaderRelatedColumn(string columnName)
			{
				return base.IsCusEntryHeaderRelatedColumn(columnName)
					|| columnName == JobDeclaration.Schema.AESSeverity
					|| columnName == JobDeclaration.Schema.AESSeverityDescription
					|| columnName == JobDeclaration.Schema.AESResponseCode
					|| columnName == JobDeclaration.Schema.AESResponseCodeDescription;
			}
		}
	}
}
