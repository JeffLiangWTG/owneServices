using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	internal class OrgReceivablesModuleFilter : ModuleTextFilter
	{
		#region Construction

		public OrgReceivablesModuleFilter(ZString description)
			: base(description, DummyQuery)
		{
			this.SubGroup = new OrgReceivablesModuleSubGroup();
		}

		#endregion

		#region InvoiceType

		[List("InvoiceTypeList")]
		public ZString InvoiceType
		{
			get { return invoiceType; }
			set
			{
				invoiceType = value;
				InvoiceTypeInfo.RefreshBinding();
				InvalidateCachedQuery();
			}
		}

		public ZPropertyInfo InvoiceTypeInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceType)); }
		}
		public ZString invoiceType;

		#endregion

		#region InvoiceTypes

		public CodeDescriptionPairList InvoiceTypeList
		{
			get
			{
				if (invoiceTypesList == null)
				{
					invoiceTypesList = new CodeDescriptionPairList();
					invoiceTypesList.AddPair(InvoiceTypeConstants.Codes.All, Res.GetString("MasterFiles|InvoiceTypeList|All", "All Invoice Types"));
					invoiceTypesList.AddPair(InvoiceTypeConstants.Codes.InvoiceNumber, Res.GetString("MasterFiles|InvoiceTypeList|InvoiceNumber", "Invoice Number"));
					invoiceTypesList.AddPair(InvoiceTypeConstants.Codes.GovtTaxNumber, Res.GetString("MasterFiles|InvoiceTypeList|GovtTaxNumber", "Govt. Tax Invoice Number"));
					invoiceTypesList.AddPair(InvoiceTypeConstants.Codes.JobInvoiceNumber, Res.GetString("MasterFiles|InvoiceTypeList|JobInvoiceNumber", "Job Invoice Number"));
					invoiceTypesList.AddPair(InvoiceTypeConstants.Codes.BatchInvoiceNumber, Res.GetString("MasterFiles|InvoiceTypeList|BatchInvoiceNumber", "Invoice Batch Number"));
				}
				return invoiceTypesList;
			}
		}
		CodeDescriptionPairList invoiceTypesList;

		public static class InvoiceTypeConstants
		{
			public static class Codes
			{
				public const string All = "ALL";
				public const string InvoiceNumber = "INV";
				public const string GovtTaxNumber = "GVT";
				public const string JobInvoiceNumber = "JOB";
				public const string BatchInvoiceNumber = "BAT";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public static class Descriptions
			{
				public const string All = "All Invoice Types";
				public const string InvoiceNumber = "Invoice Number";
				public const string GovtTaxNumber = "Govt. Tax Invoice Number";
				public const string JobInvoiceNumber = "Job Invoice Number";
				public const string BatchInvoiceNumber = "Invoice Batch Number";
			}
		}

		#endregion

		#region Overrides

		protected override void ClearCore()
		{
			base.ClearCore();
			InvoiceType = "";
		}

		protected override int Property_MaxLength => 38;

		#endregion

		#region Dummies

		static ZQuery DummyQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery();
		}

		#endregion

		#region Query

		class OrgReceivablesModuleSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery headerQuery = new ZDBOnlyQuery(typeof(OrgHeader));

				ZDBOnlySubQuery invoiceSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_OH);
				invoiceSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				invoiceSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				invoiceSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new string[] { TransactionTypes.Invoice, TransactionTypes.CreditNote });

				ZQuery invoiceNumberFilters = new ZQuery();
				invoiceNumberFilters.AddToFilter(filter);
				invoiceSubQuery.AddToFilter(invoiceNumberFilters);
				headerQuery.AddSubQuery(invoiceSubQuery, JoinCondition.And);

				return headerQuery;
			}
		}

		protected override ZQuery GetQuery()
		{
			ZQuery query = new ZQuery();

			if (!Property.IsEmpty)
			{
				query.AddToFilter(GetDebtorInvoiceNumberQuery());
			}
			return query;
		}

		ZQuery GetDebtorInvoiceNumberQuery()
		{
			var invoiceNumberFilters = new ZQuery();

			if (InvoiceType == InvoiceTypeConstants.Codes.All)
			{
				var joinCondition = JoinCondition.Or;
				AddToFilter(new[] { AccTransactionHeaderSchema.AH_TransactionNum, AccTransactionHeaderSchema.AH_TransactionReference, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, AccTransactionHeaderSchema.AH_ReceiptBatchNo }, joinCondition, invoiceNumberFilters);
			}
			else
			{
				var joinCondition = JoinCondition.And;
				switch (InvoiceType)
				{
					case InvoiceTypeConstants.Codes.InvoiceNumber:
						AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, joinCondition, invoiceNumberFilters);
						break;
					case InvoiceTypeConstants.Codes.GovtTaxNumber:
						AddToFilter(AccTransactionHeaderSchema.AH_TransactionReference, joinCondition, invoiceNumberFilters);
						break;
					case InvoiceTypeConstants.Codes.JobInvoiceNumber:
						AddToFilter(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, joinCondition, invoiceNumberFilters);
						break;
					case InvoiceTypeConstants.Codes.BatchInvoiceNumber:
						AddToFilter(AccTransactionHeaderSchema.AH_ReceiptBatchNo, joinCondition, invoiceNumberFilters);
						break;
					default:
						break;
				}
			}
			return invoiceNumberFilters;
		}

		void AddToFilter(SchemaStringColumn[] schemaStringColumns, JoinCondition joinCondition, ZQuery query)
		{
			foreach (var item in schemaStringColumns)
			{
				AddToFilter(item, joinCondition, query);
			}
		}

		void AddToFilter(SchemaStringColumn schemaStringColumn, JoinCondition joinCondition, ZQuery query)
		{
			if (Property.Length <= schemaStringColumn.MaxLength)
			{
				query.AddToFilter(joinCondition, schemaStringColumn, SqlComparisonOperator, Property);
			}
		}

		#endregion
	}
}
