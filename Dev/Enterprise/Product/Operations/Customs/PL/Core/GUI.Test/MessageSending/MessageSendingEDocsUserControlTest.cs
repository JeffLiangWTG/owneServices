using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class MessageSendingEDocsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSources()
	{
		RunForControl(control =>
		{
			AssertEquals("Control", typeof(BaseMessageSendingObjectParent), control.BindingSource.DataSourceType);
			AssertEquals("CustomsOfficeFindBox", "CustomsOffice", control.BindingSource.GetBindingMember(control.CustomsOfficeFindBox));
			AssertEquals("PurposeOfSendingDropEdit", "PurposeOfSending", control.BindingSource.GetBindingMember(control.PurposeOfSendingDropEdit));
			AssertEquals("ProcedureDropEdit", "Procedure", control.BindingSource.GetBindingMember(control.ProcedureDropEdit));
			AssertEquals("RefNumberTextBox", "RefNumber", control.BindingSource.GetBindingMember(control.RefNumberTextBox));
			AssertEquals("MrnNumberTextBox", "MrnNumber", control.BindingSource.GetBindingMember(control.MrnNumberTextBox));
			AssertEquals("CommentsTextBox", "Comments", control.BindingSource.GetBindingMember(control.CommentsTextBox));
		});
	}

	public void TestEDocsRelatedControls()
	{
		RunForControl(control =>
		{
			CombineAssertions(() =>
			{
				AssertEDocsControls(control, false, "should not be visible - before add");
				var addedObject = sendingObjectParent.EDocs.AddNew();
				AssertEDocsControls(control, true, "should be visible - after add");
				sendingObjectParent.EDocs.Remove(addedObject);
				AssertEDocsControls(control, false, "should not be visible - after remove");
			});
		});
	}

	public void TestEDocsGroupBox()
	{
		RunForControl(control =>
		{
			AssertEquals("eDocs", control.EDocsGroupBox.CaptionResourceString.Caption);
		});
	}

	public void TestRefMRNNumberCombination()
	{
		RunForControl(control =>
		{
			CombineAssertions(() =>
			{
				AssertEquals("RefMrnNumberLabel caption", "Ref/MRN No.", control.RefMrnNumberLabel.CaptionResourceString.Caption);
				AssertEquals("RefNumberTextBox caption", null, control.RefNumberTextBox.CaptionResourceString.Caption);
				AssertEquals("RefMRNNumberSeparatorLabel caption", "/", control.RefMRNNumberSeparatorLabel.Text);
				AssertEquals("MrnNumberTextBox caption", null, control.MrnNumberTextBox.CaptionResourceString.Caption);
			});
		});
	}

	public void TestEDocsToBeSentGrid()
	{
		RunForControl(control =>
		{
			var grid = control.SupportingDocumentsGrid;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 4, grid.ColumnStyles.Count);
				AssertEquals("EDoc column", AutoJobDeclarationMessageSendingEDocs.Schema.EDoc, ((ZGuidDropEditColumnStyleInfo)grid.ColumnStyles[0]).ColumnName);
				AssertEquals("AdditionalInformation column", AutoJobDeclarationMessageSendingEDocs.Schema.AdditionalInformation, ((ZCodeFindBoxColumnStyleInfo)grid.ColumnStyles[1]).ColumnName);
				AssertEquals("SupportingDocument column", AutoJobDeclarationMessageSendingEDocs.Schema.SupportingDocument, ((ZCodeFindBoxColumnStyleInfo)grid.ColumnStyles[2]).ColumnName);
				AssertEquals("DocumentDescription column", AutoJobDeclarationMessageSendingEDocs.Schema.DocumentDescription, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[3]).ColumnName);
			});
		});
	}

	public void TestUserInputControls()
	{
		RunForControl(control =>
		{
			CombineAssertions(() =>
			{
				var userInputGroupBox = control.UserInputGroupBox;
				AssertEquals("UserInputGroupBox should not be visible", false, userInputGroupBox.Visible);
				var addedObject = sendingObjectParent.EDocs.AddNew();
				AssertEquals("UserInputGroupBox should be visible", true, userInputGroupBox.Visible);
				sendingObjectParent.EDocs.Remove(addedObject);
				AssertEquals("UserInputGroupBox should not be visible after remove", false, userInputGroupBox.Visible);
			});
		});
	}

	void RunForControl(Action<MessageSendingEDocsUserControl> methodToRun)
	{
		using (var form = new ZForm(sendingObjectParent))
		{
			using (var control = new MessageSendingEDocsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				methodToRun.Invoke(control);
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		sendingObjectParent = new BaseMessageSendingObjectParent(declaration);
	}

	static void AssertEDocsControls(MessageSendingEDocsUserControl control, bool shouldBeVisible, string message)
	{
		AssertEquals($"CustomsOfficeFindBox {message}", shouldBeVisible, control.CustomsOfficeFindBox.Visible);
		AssertEquals($"PurposeOfSendingDropEdit {message}", shouldBeVisible, control.PurposeOfSendingDropEdit.Visible);
		AssertEquals($"ProcedureDropEdit {message}", shouldBeVisible, control.ProcedureDropEdit.Visible);
		AssertEquals($"RefNumberTextBox {message}", shouldBeVisible, control.RefNumberTextBox.Visible);
		AssertEquals($"MrnNumberTextBox {message}", shouldBeVisible, control.MrnNumberTextBox.Visible);
		AssertEquals($"CommentsTextBox {message}", shouldBeVisible, control.CommentsTextBox.Visible);
		AssertEquals($"RefMRNNumberSeparatorLabel {message}", shouldBeVisible, control.RefMRNNumberSeparatorLabel.Visible);
	}

	BaseMessageSendingObjectParent sendingObjectParent;
}
