using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Module
{
	class ProofOfPaymentFilterBusinessObject : FilterStripBusinessObject
	{
		public static class FilterConstants
		{
			public const string Importer = "Importer";
			public const string JobNumber = "Job Number";
			public const string FinancialAccountNumber = "Financial Account Number";
			public const string TransactionDate = "Transaction Date";
			public const string ReceiptDate = "Receipt Date";
			public const string CustomsOffice = "Customs Office";
			public const string LocalReferenceNumber = "Local Reference Number";
			public const string MRN = "MRN";
			public const string VATAmount = "VAT Amount";
			public const string ReceiptNumber = "Receipt Number";
			public const string TotalVATforReceipt = "TOTAL VAT for Receipt";
			public const string AgentsReference = "Agents Reference";
			public const string DeclarationCompany = "Declaration Company";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			var importerFilter = result.AddGuidFilter(ProofOfPaymentFilterBusinessObject.FilterConstants.Importer, ModuleIDs.Organisation, new GetGuidQueryWithOperator((comparisonOperator, value) => GetImporterQuery(comparisonOperator, value)), ImporterList);
			importerFilter.MultilingualDescription = ResString.GetMultilingualString("ProofOfPaymentFilter|Importer", "Importer");

			var textFilterJobNumber = result.AddTextFilter(ProofOfPaymentFilterBusinessObject.FilterConstants.JobNumber, new GetTextQueryWithOperator((comparisonOperator, value) => GetJobQuery(comparisonOperator, value)));
			textFilterJobNumber.WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_DeclarationReference);
			textFilterJobNumber.MultilingualDescription = ResString.GetMultilingualString("ProofOfPaymentFilter|JobNumber", "Job Number");

			var financialAccountNumberFilter = result.AddTextFilter(ProofOfPaymentFilterBusinessObject.FilterConstants.FinancialAccountNumber, GetFANQuery, FANumbers);
			financialAccountNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ProofOfPaymentFilter|FinancialAccountNumber", "Financial Account Number");

			var transactionDateFilter = result.AddDateFilter(ProofOfPaymentFilterBusinessObject.FilterConstants.TransactionDate, CusEntryPayInfoSchema.C9_PaymentDate);
			transactionDateFilter.MultilingualDescription = ResString.GetMultilingualString("ProofOfPaymentFilter|TransactionDate", "Transaction Date");

			var receiptDateFilter = result.AddDateFilter(ProofOfPaymentFilterBusinessObject.FilterConstants.ReceiptDate, CusEntryPayInfoSchema.C9_ReceiptDate);
			receiptDateFilter.MultilingualDescription = ResString.GetMultilingualString("ProofOfPaymentFilter|ReceiptDate", "Receipt Date");

			var textFilterCustomsOffice = result.AddTextFilter(ProofOfPaymentFilterBusinessObject.FilterConstants.CustomsOffice, new GetTextQueryWithOperator((comparisonOperator, value) => GetCustomsOfficeQuery(comparisonOperator, value)));
			textFilterCustomsOffice.WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_CustomsOffice);
			textFilterCustomsOffice.MultilingualDescription = ResString.GetMultilingualString("ProofOfPaymentFilter|CustomsOffice", "Customs Office");

			var textFilterLocalReferenceNumber = result.AddTextFilter(ProofOfPaymentFilterBusinessObject.FilterConstants.LocalReferenceNumber, new GetTextQueryWithOperator((comparisonOperator, value) => GetLRNumberQuery(comparisonOperator, value)));
			textFilterLocalReferenceNumber.WithMaxLengthOf<ModuleTextFilter>(CusEntryHeaderSchema.CH_BGMReference);
			textFilterLocalReferenceNumber.MultilingualDescription = ResString.GetMultilingualString("ProofOfPaymentFilter|LocalReferenceNumber", "Local Reference Number");

			var textFilterMRN = result.AddTextFilter(ProofOfPaymentFilterBusinessObject.FilterConstants.MRN, new GetTextQueryWithOperator((comparisonOperator, value) => GetMRNumberQuery(comparisonOperator, value)));
			textFilterMRN.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			textFilterMRN.MultilingualDescription = ResString.GetMultilingualString("ProofOfPaymentFilter|MRN", "MRN");

			var numberRangeFilterVatAmount = result.AddNumberRangeSubFilter(ProofOfPaymentFilterBusinessObject.FilterConstants.VATAmount, CusEntryPayInfoSchema.C9_PaymentAmount);
			numberRangeFilterVatAmount.DefaultPropertySearch = ModuleNumberRangeSubFilter.SearchTexts.GreaterThan.GetUnresolvedString();
			numberRangeFilterVatAmount.MultilingualDescription = ResString.GetMultilingualString("ProofOfPaymentFilter|VATAmount", "VAT Amount");

			var receiptNumberFilter = result.AddTextFilter(ProofOfPaymentFilterBusinessObject.FilterConstants.ReceiptNumber, CusEntryPayInfoSchema.C9_PaymentReference);
			receiptNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ProofOfPaymentFilter|ReceiptNumber", "Receipt Number");

			var totalVATforReceiptFilter = result.AddNumberRangeFilter(ProofOfPaymentFilterBusinessObject.FilterConstants.TotalVATforReceipt, GetTotalVatForReceiptQuery);
			totalVATforReceiptFilter.MultilingualDescription = ResString.GetMultilingualString("ProofOfPaymentFilter|TotalVATforReceipt", "TOTAL VAT for Receipt");

			var textFilterAgentsReference = result.AddTextFilter(ProofOfPaymentFilterBusinessObject.FilterConstants.AgentsReference, GetAgentsReferenceQuery);
			textFilterAgentsReference.WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_AgentsReference);
			textFilterAgentsReference.MultilingualDescription = ResString.GetMultilingualString("ProofOfPaymentFilter|AgentsReference", "Agents Reference");

			return result;
		}

		ZQuery GetAgentsReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			AddJobDeclarationSubQueryUsingClusterKey(query, JobDeclarationSchema.JE_AgentsReference, value, comparisonOperator);
			return query;
		}

		ZQuery GetImporterQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			AddJobDeclarationSubQueryUsingClusterKey(query, JobDeclarationSchema.JE_OH_Importer, value, comparisonOperator);
			return query;
		}

		public IBusinessObjectCollection ImporterList
		{
			get { return importerList ?? (importerList = new OrganisationsFindBoxCollection(Factory)); }
		}
		IBusinessObjectCollection importerList;

		ZQuery GetJobQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			AddJobDeclarationSubQueryUsingClusterKey(query, JobDeclarationSchema.JE_DeclarationReference, value, comparisonOperator);
			return query;
		}

		ZQuery GetFANQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			var collection = ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.Value;
			if (collection.Count > 0)
			{
				var fanPortMap = collection.GetByFinancialAccountNumber(value);
				if (fanPortMap != null)
				{
					var agentpk = fanPortMap.OrganizationPK;
					var customsOffice = fanPortMap.CustomsOfficeCode;

					var declarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.JE_ClusterKey);
					var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
					orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, SQLComparisonOperator.Equal, agentpk);
					declarationQuery.AddSubQuery(JobDeclarationSchema.JE_OA_DeclarantAddress, orgAddressQuery, JoinCondition.And);
					declarationQuery.AddToFilter(JobDeclarationSchema.JE_CustomsOffice, SQLComparisonOperator.Equal, customsOffice);
					query.AddSubQuery(CusEntryPayInfoSchema.C9_ClusterKey, declarationQuery, JoinCondition.And);
				}
			}

			return query;
		}

		ICodeDescriptionPairList FANumbers
		{
			get
			{
				if (faNumbers == null)
				{
					faNumbers = new CodeDescriptionPairList();
					foreach (FinancialAccountNumberPortMap item in ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.Value)
					{
						faNumbers.AddPair(item.FinancialAccountNumber, item.Organization?.OH_Code ?? ZString.Empty + " - " + item.CustomsOfficeCode);
					}
					faNumbers.Sort();
				}
				return faNumbers;
			}
		}
		CodeDescriptionPairList faNumbers;

		ZQuery GetCustomsOfficeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			AddJobDeclarationSubQueryUsingClusterKey(query, JobDeclarationSchema.JE_CustomsOffice, value, comparisonOperator);
			return query;
		}

		ZQuery GetLRNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			var sub = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey);
			AddSubQueryUsingClusterKey(query, sub, CusEntryHeaderSchema.CH_BGMReference, value, comparisonOperator);
			return query;
		}

		ZQuery GetMRNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			var sub1 = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey);
			var sub2 = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			sub2.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			sub2.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			sub1.AddSubQuery(CusEntryHeaderSchema.PK, sub2, JoinCondition.And);
			query.AddSubQuery(CusEntryPayInfoSchema.C9_ClusterKey, sub1, JoinCondition.And);
			return query;
		}

		ZQuery GetTotalVatForReceiptQuery(INumericZType value1, INumericZType value2)
		{
			var sqlText = string.Format(CultureInfo.CurrentCulture, @"{0} IN (
SELECT c1.{0} FROM {1} AS c1
WHERE c1.{2} = '{3}'
AND (
	SELECT amount =
		CASE c2.{0}
			WHEN '' THEN 0
			ELSE (SELECT SUM(c3.{4}) FROM {1} AS c3 WHERE c3.{0} = c2.{0} GROUP BY c3.{0})
			END
	FROM {1} AS c2
	WHERE c2.{5} = c1.{5}
) >= {6}
AND (
	SELECT amount =
		CASE c2.{0}
			WHEN '' THEN 0
			ELSE (SELECT SUM(c3.{4}) FROM {1} AS c3 WHERE c3.{0} = c2.{0} GROUP BY c3.{0})
			END
	FROM {1} AS c2
	WHERE c2.{5} = c1.{5}
) <= {7}
GROUP BY c1.{0}
)",
CusEntryPayInfoSchema.Constants.C9_PaymentReference,
CusEntryPayInfoSchema.Constants.TableName,
CusEntryPayInfoSchema.Constants.C9_TransactionType,
UniversalReferenceConstants.TaxOrFeeTypeCode.VAT,
CusEntryPayInfoSchema.Constants.C9_PaymentAmount,
CusEntryPayInfoSchema.Constants.PK,
value1, value2);

			var query = new ZDBOnlyQuery(typeof(CusEntryPayInfo));
			query.AddFilterAndZSQLParameterCollection(sqlText, null);
			return query;
		}

		void AddSubQueryUsingClusterKey(ZDBOnlyQuery query, ZDBOnlySubQuery subQuery, SchemaColumn column, object value, SQLComparisonOperator comparisonOperator)
		{
			subQuery.AddToFilter(column, comparisonOperator, value);
			query.AddSubQuery(CusEntryPayInfoSchema.C9_ClusterKey, subQuery, JoinCondition.And);
		}

		void AddJobDeclarationSubQueryUsingClusterKey(ZDBOnlyQuery query, SchemaColumn column, object value, SQLComparisonOperator comparisonOperator)
		{
			var sub = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.JE_ClusterKey);
			AddSubQueryUsingClusterKey(query, sub, column, value, comparisonOperator);
		}
	}
}
