using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GLJournalExchangeRateTypeControl))]
	public class GLJournalExchangeRateTypeControlTest : Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new GLJournalExchangeRateType() { BalanceSheetAccountTypeExchangeRateType = "PER", ProfitAndLossAccountTypeExchangeRateType = "PER" };
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var gLJournalExchangeRateTypeControl = (GLJournalExchangeRateTypeControl)control;
			var balanceSheetAccountTypeExchangeRateTypeDropEdit = (ZDropEdit)gLJournalExchangeRateTypeControl.Controls.Find("BalanceSheetAccountTypeExchangeRateTypeDropEdit", true)[0];
			var profitAndLossAccountTypeExchangeRateTypeDropEdit = (ZDropEdit)gLJournalExchangeRateTypeControl.Controls.Find("ProfitAndLossAccountTypeExchangeRateTypeDropEdit", true)[0];
			return balanceSheetAccountTypeExchangeRateTypeDropEdit.ReadOnly && profitAndLossAccountTypeExchangeRateTypeDropEdit.ReadOnly;
		}
	}
}
