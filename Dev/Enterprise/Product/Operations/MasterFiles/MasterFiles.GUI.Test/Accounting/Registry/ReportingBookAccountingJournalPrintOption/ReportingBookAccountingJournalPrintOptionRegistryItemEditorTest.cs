using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(ReportingBookAccountingJournalPrintOptionRegistryItemEditor))]
	sealed class ReportingBookAccountingJournalPrintOptionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ReportingBookAccountingJournalPrintOptionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ReportingBookAccountingJournalPrintOptionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ReportingBookAccountingJournalPrintOptionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ReportingBookAccountingJournalPrintOptionRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var accountingTestObjectCreator = new AccountingTestObjectCreator(Factory);
			var chart = accountingTestObjectCreator.CreateAlternateChart("ABC", isGlobal: true);
			var reportingBook = accountingTestObjectCreator.CreateAccReportingBook("XXA", chart.PK, GlbCompany.CurrentCompany.PK, "EET", "Description");

			Factory.Save();
			var collection = new ReportingBookAccountingJournalPrintOptionCollection();
			var copy = collection.AddNew();

			copy.ReportingBook = reportingBook.PK;
			copy.Default = true;
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
