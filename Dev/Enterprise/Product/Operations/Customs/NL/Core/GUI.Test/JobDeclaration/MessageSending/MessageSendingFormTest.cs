using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(MessageSendingForm))]
sealed class MessageSendingFormTest : ZFormBasherTest
{
	protected override Form GetFormToBashCore()
	{
		var declaration = Factory.New<JobDeclaration>();
		var declarationWrapper = new JobDeclarationMessageSendingObjectParent(declaration);
		return new MessageSendingForm(declarationWrapper);
	}

	public void TestBottomSectionUserControl()
	{
		using (var messageSendingForm = GetFormToBashCore())
		{
			var bottomsSectionUserControl = messageSendingForm.Controls.Find("ExportBottomSectionUserControl", true).Single();
			AssertNotNull(bottomsSectionUserControl);
			AssertType<ExportBottomSectionUserControl>(bottomsSectionUserControl);
		}
	}

	public void TestColumnsInGrid()
	{
		var decl = Factory.New<JobDeclaration>();
		var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(decl);
		using (var messageSendingForm = new MessageSendingForm(messageSendingObjectParent))
		{
			var grid = (ZGrid)messageSendingForm.Controls.Find("MessageSendingObjectsGrid", true)[0];
			grid.SetDataBinding(messageSendingObjectParent.SendingObjectsCollection, "");
			messageSendingForm.Show();
			ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm);

			AssertionWithHtml.CombineAssertions(() =>
			{
				TestCaseWithFactory.AssertEquals("columns count", orderedColumnNamesAndColumnStyleTypes.Count(), grid.ColumnStyles.Count);

				var indexCounter = 0;
				foreach (var (columnName, columnType) in orderedColumnNamesAndColumnStyleTypes)
				{
					var column = grid.Columns.SingleOrDefault(x => x.ColumnName == columnName);
					if (column == null)
					{
						TestCaseWithFactory.Assert($"Column: {columnName} does not exists", false);
					}
					else
					{
						var actualColumnStyleType = column.ColumnStyle.GetType();
						TestCaseWithFactory.AssertEquals($"Column: {columnName}: exists at expected position '{indexCounter}'", columnName, grid.Columns[indexCounter].ColumnName);
						TestCaseWithFactory.AssertEquals($"Column: {columnName}: columnStyleType", columnType, actualColumnStyleType);
						TestCaseWithFactory.AssertEquals($"Column: {columnName}: visible", true, column.IsVisible);
					}
					indexCounter++;
				}
			});
		}
	}

	public void TestStatementBox() => CombineAssertions(() =>
	{
		using (var messageSendingForm = GetFormToBashCore())
		{
			messageSendingForm.Show();
			var statementBox = (ZTextBox)messageSendingForm.Controls.Find("StatementTextBox", true)[0];
			var statementGroupBox = (ZGroupBox)messageSendingForm.Controls.Find("StatementGroupBox", true)[0];
			AssertEquals("StatementBox should be visible", true, statementBox.Visible);
			AssertEquals("Statement box caption is Reason for Invalidation", statementGroupBox.CaptionResourceString.Caption, "Reason for Invalidation");
		}
	});

	public void TestLayout_Export()
	{
		var decl = Factory.New<JobDeclaration>();
		decl.JE_MessageType = JobMessageTypeList.Codes.Export;
		var entryHeader = decl.CustomsEntryHeaders.AddNew();
		var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(decl);
		using (var messageSendingForm = new MessageSendingForm(messageSendingObjectParent))
		{
			messageSendingForm.Show();
			var statementGroupBox = messageSendingForm.FindSingle<ZGroupBox>("StatementGroupBox");
			var exportBottomSectionUserControl = messageSendingForm.FindSingle<ExportBottomSectionUserControl>("ExportBottomSectionUserControl");

			CombineAssertions(() =>
			{
				messageSendingObjectParent.SendingObjectsCollection[0].MessageType = "XXX";
				AssertEquals(true, statementGroupBox.Visible);
				AssertEquals(false, exportBottomSectionUserControl.Visible);

				messageSendingObjectParent.SendingObjectsCollection[0].MessageType = "EXT";
				AssertEquals(false, statementGroupBox.Visible);
				AssertEquals(true, exportBottomSectionUserControl.Visible);
			});
		}
	}

	public void TestLayout_Import()
	{
		var decl = Factory.New<JobDeclaration>();
		decl.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryHeader = decl.CustomsEntryHeaders.AddNew();
		var messageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(decl);

		using (var messageSendingForm = new MessageSendingForm(messageSendingObjectParent))
		{
			messageSendingForm.Show();
			var statementGroupBox = messageSendingForm.FindSingle<ZGroupBox>("StatementGroupBox");
			var exportBottomSectionUserControl = messageSendingForm.FindSingle<ExportBottomSectionUserControl>("ExportBottomSectionUserControl");

			CombineAssertions(() =>
			{
				AssertEquals(true, statementGroupBox.Visible);
				AssertEquals(false, exportBottomSectionUserControl.Visible);
			});
		}
	}

	IEnumerable<(string ColumnName, Type ColumnType)> orderedColumnNamesAndColumnStyleTypes => new (string, Type)[]
	{
		("ShouldSend", typeof(ZCheckBoxColumnStyle)),
		("Update", typeof(ZCheckBoxColumnStyle)),
		("MessageType", typeof(ZDropEditColumnStyle)),
		("EntryType", typeof(ZTextBoxColumnStyle)),
		("SubStyle", typeof(ZTextBoxColumnStyle)),
		("Description", typeof(ZTextBoxColumnStyle)),
		("Date", typeof(ZDateEditColumnStyle)),
		("EntryStatus", typeof(ZTextBoxColumnStyle))
	};
}
