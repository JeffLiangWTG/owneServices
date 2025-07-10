using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	sealed class StatementFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string ProcessDate = "Process Date";
			public const string PrintDate = "Print Date";
			public const string DueDate = "Due Date";
			public const string StatementNumber = "Statement Number";
			public const string ImporterCustomsID = "Importer Customs ID";
			public const string StatementProcessPort = "Statement Process Port";
			public const string StatementStatus = "Statement Status";
			public const string PaymentStatus = "Payment Status";
			public const string StatementEntryFilerCode = "Entry Filer Code";
			public const string ClientBranchDesignation = "Client Branch Designation";
			public const string EntryNumber = "Entry Number";
			public const string Importer = "Importer";
			public const string PayerUnitNumber = "Payer Unit Number";
			public const string PaymentType = "Payment Type";
			public const string StatementAmount = "Total Amount (excl. deleted entries)";
			public const string ActionAuthorized = "Authorization Permission Granted";
			public const string StatementType = "Statement Type";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddDateFilter(Schema.ProcessDate, CusStatementHeaderSchema.B2_ProcessDate);
			result.AddDateFilter(Schema.PrintDate, CusStatementHeaderSchema.B2_PrintDate);
			result.AddDateFilter(Schema.DueDate, CusStatementHeaderSchema.B2_DueDate);

			result.AddNumberFilter(Schema.StatementNumber, CusStatementHeaderSchema.B2_StatementNumber);
			result.AddNumberFilter(Schema.ImporterCustomsID, CusStatementHeaderSchema.B2_ImporterCustomsID);
			result.AddNumberFilter(Schema.StatementEntryFilerCode, CusStatementHeaderSchema.B2_EntryFilerCode);
			var number = result.AddNumberFilter(Schema.EntryNumber, GetEntryNumberQuery);
			number.MaxLength = CusStatementLineSchema.B3_EntryNum.MaxLength;
			result.AddNumberRangeFilter(Schema.StatementAmount, CusStatementHeaderSchema.B2_StatementAmount);
			result.AddTextFilter(Schema.StatementProcessPort, CusStatementHeaderSchema.B2_ProcessPort);
			result.AddTextFilter(Schema.ClientBranchDesignation, CusStatementHeaderSchema.B2_BranchDesignation);
			result.AddTextFilter(Schema.PayerUnitNumber, CusStatementHeaderSchema.B2_AccountNo);

			var statementStatusFilter = result.AddTextFilter(Schema.StatementStatus, CusStatementHeaderSchema.B2_Status, Factory.GetCachedValue<StatementHeaderStatusList>());
			statementStatusFilter.Category = FilterCategories.StatusAndFlags;

			var paymentStatusFilter = result.AddTextFilter(Schema.PaymentStatus, CusStatementHeaderSchema.B2_PaymentStatus, Factory.GetCachedValue<PaymentStatusList>());
			paymentStatusFilter.Category = FilterCategories.StatusAndFlags;

			var currentCompanyFilter = result.AddGuidFilter("Company", ModuleIDs.GlbCompany, GetCompanyQuery, new GlbCompanyCollection(Factory));
			currentCompanyFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			var importerFilter = result.AddGuidFilter(Schema.Importer, ModuleIDs.Organisation, CusStatementHeaderSchema.B2_OH_Importer, new ConsigneeCollection(Factory));
			importerFilter.Category = FilterCategories.Organisations;

			var paymentTypeFilter = result.AddTextFilter(Schema.PaymentType, CusStatementHeaderSchema.B2_PaymentType, Factory.GetCachedValue<PaymentTypeList>());
			paymentTypeFilter.Category = FilterCategories.ModesAndTypes;

			var actionAuthorizedFilter = result.AddTextFilter(Schema.ActionAuthorized, GetActionAuthorizedQuery, ActionAuthorizedList);
			actionAuthorizedFilter.Category = FilterCategories.StatusAndFlags;

			var statementTypeFilter = result.AddTextFilter(Schema.StatementType, GetStatementTypeQuery, StatementTypeList);
			statementTypeFilter.Category = FilterCategories.ModesAndTypes;

			return result;
		}

		ZQuery GetCompanyQuery(ZGuid value)//users cannot enter a value as it is always hidden
		{
			return new ZQuery(CusStatementHeaderSchema.B2_GC, GlbCompany.CurrentCompany.PK);
		}

		ZQuery GetEntryNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetStatementLineSubQuery(CusStatementLineSchema.B3_EntryNum, comparisonOperator, value);
		}

		ZQuery GetStatementLineSubQuery(SchemaColumn column, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusStatementHeader));

			var subQuery = new ZDBOnlySubQuery(typeof(CusStatementLine), CusStatementLineSchema.B3_B2);
			subQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, column, comparisonOperator, value);

			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetActionAuthorizedQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (value == "Granted" || value == "Not Granted")
			{
				var result = new ZDBOnlyQuery(typeof(CusStatementHeader));
				var notIn = value == "Not Granted";
				var logQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, notIn);

				logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.Authorised.Code);
				logQuery.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False);
				logQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CusStatementHeader.PermissionGranted);

				result.AddSubQuery(logQuery, JoinCondition.And);
				return result;
			}

			return new ZQuery();
		}

		CodeDescriptionPairList ActionAuthorizedList()
		{
			return Factory.GetCachedValue("ActionAuthorizedList", delegate
			{
				var result = new CodeDescriptionPairList();

				result.AddPair("All", "All");
				result.AddPair("Granted", "Permission Granted");
				result.AddPair("Not Granted", "Permission Not Granted");

				return result;
			});
		}

		ZQuery GetStatementTypeQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusStatementHeader));

			if (value == "Daily")
			{
				result.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, false);
			}
			else if (value == "Monthly")
			{
				result.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, true);
			}

			return result;
		}

		CodeDescriptionPairList StatementTypeList()
		{
			return Factory.GetCachedValue("StatementTypeList", delegate
			{
				var result = new CodeDescriptionPairList();

				result.AddPair("Daily", "Daily Statement");
				result.AddPair("Monthly", "Periodic Monthly Statement");

				return result;
			});
		}
	}
}
