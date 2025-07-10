using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccountingEmailDef_ForTest : AccountingEmailDef
	{
		protected override GuidRegistryItem Recipient => AccountingMasterFilesRegistry.Instance.DebtorGlobalCreditLimitNotifyGroup;

		protected override string GetBody() => TheBody;

		protected override string GetSubject() => TheSubject;

		public string TheBody { get; set; } = string.Empty;
		public string TheSubject { get; set; } = string.Empty;
	}
}
