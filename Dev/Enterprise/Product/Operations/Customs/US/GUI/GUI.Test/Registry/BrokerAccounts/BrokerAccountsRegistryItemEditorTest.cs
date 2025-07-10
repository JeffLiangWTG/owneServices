using System;
using System.Windows.Forms;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(BrokerAccountsRegistryItemEditor))]
	sealed class BankAccountBasedOnCurrencyRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new BrokerAccountsRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((BrokerAccountsUserControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(BrokerAccountsUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new BrokersAccountRegistryItem("", null, null, null, RegistryStorageFlags.System);

		protected override object[] GetValidRegistryValues()
		{
			var collection = new BrokersAccountCollection();
			var brokersAccount = collection.AddNew();
			brokersAccount.BankAccount = Factory.NewWithValidTestData<AccBankAccount>().PK;
			brokersAccount.PayerUnitNumber = "123456";
			Factory.Save();
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
