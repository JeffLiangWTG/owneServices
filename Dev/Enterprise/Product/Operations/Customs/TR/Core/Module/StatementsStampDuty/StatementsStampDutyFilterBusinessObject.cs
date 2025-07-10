using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Module
{
	public class StatementsStampDutyFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string StatementNumber = "Statement Number";
			public const string PaymentStatus = "Payment Status";
			public const string StatementStatus = "Statement Status";
			public const string DueDate = "Due Date";
			public const string PrintDate = "Print Date";
			public const string ProcessDate = "Process Date";
			public const string PaymentType = "Payment Type";
			public const string StatementType = "Statement Type";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var statementNumberFilter = result.AddNumberFilter(Schema.StatementNumber, CusStatementHeaderSchema.B2_StatementNumber);
			statementNumberFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterSchema|StatementNumber", Schema.StatementNumber);

			var paymentStatusFilter = result.AddNumberFilter(Schema.PaymentStatus, CusStatementHeaderSchema.B2_PaymentStatus);
			paymentStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterSchema|PaymentStatus", Schema.PaymentStatus);

			var statusFilter = result.AddNumberFilter(Schema.StatementStatus, CusStatementHeaderSchema.B2_Status);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterSchema|StatementStatus", Schema.StatementStatus);

			var dueDateFilter = result.AddDateFilter(Schema.DueDate, CusStatementHeaderSchema.B2_DueDate);
			dueDateFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterSchema|DueDate", Schema.DueDate);

			var printDateFilter = result.AddDateFilter(Schema.PrintDate, CusStatementHeaderSchema.B2_PrintDate);
			printDateFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterSchema|PrintDate", Schema.PrintDate);

			var processDateFilter = result.AddDateFilter(Schema.ProcessDate, CusStatementHeaderSchema.B2_ProcessDate);
			processDateFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterSchema|ProcessDate", Schema.ProcessDate);

			var paymentTypeFilter = result.AddTextFilter(Schema.PaymentType, CusStatementHeaderSchema.B2_PaymentType);
			paymentTypeFilter.Category = FilterCategories.ModesAndTypes;
			paymentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterSchema|PaymentType", Schema.PaymentType);

			var statementTypeFilter = result.AddTextFilter(Schema.StatementType, CusStatementHeaderSchema.B2_StatementType);
			statementTypeFilter.Category = FilterCategories.ModesAndTypes;
			statementTypeFilter.MultilingualDescription = ResString.GetMultilingualString("ModuleFilterSchema|StatementType", Schema.StatementType);

			return result;
		}
	}
}
