using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EInvoicingPendingTransactionsNotificationGroupRegistryItemEditor))]
	public class EInvoicingPendingTransactionsNotificationGroupRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new EInvoicingPendingTransactionsNotificationGroupRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((EInvoicingPendingTransactionsNotificationGroupControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EInvoicingPendingTransactionsNotificationGroupControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EInvoicingPendingTransactionsNotificationGroupRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default,
				new EInvoicingPendingTransactionsNotificationGroup()
				{
					GroupPK = TestGroup.PK,
					DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.PostDate,
					Days = 1
				});
		}

		protected override object[] GetValidRegistryValues()
		{
			var item1 = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestGroup.PK,
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.PostDate,
				Days = 1
			};

			var item2 = new EInvoicingPendingTransactionsNotificationGroup()
			{
				GroupPK = TestGroup.PK,
				DateType = EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate,
				Days = 2
			};

			return new[] { item1, item2 };
		}

		protected override void SetUp()
		{
			base.SetUp();
			var factory = new BusinessObjectFactory();
			TestGroup = factory.NewWithValidTestData<GlbGroup>();

			factory.Save();
		}

		GlbGroup TestGroup;
	}
}
