using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class MessageSendingEntryLinesGridUserControlTest : TestCaseWithFactory
{
	public void TestBindingSources()
	{
		RunForControl(control =>
		{
			AssertEquals("Control", typeof(RetrospectiveQuotaRequestMessageSendingObjectParent), control.BindingSource.DataSourceType);
			AssertEquals("EntryLinesGrid", "EntryLines", control.BindingSource.GetBindingMember(control.EntryLinesGrid));
		});
	}

	public void TestEntryLinesGrid()
	{
		RunForControl(control =>
		{
			var grid = control.EntryLinesGrid;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 7, grid.ColumnStyles.Count);
				AssertEquals("Send column", AutoJobDeclarationMessageSendingEntryLine.Schema.Send, ((ZCheckBoxColumnStyleInfo)grid.ColumnStyles[0]).ColumnName);
				AssertEquals("Entry line no column", AutoJobDeclarationMessageSendingEntryLine.Schema.LineNumber, ((ZCalcEditColumnStyleInfo)grid.ColumnStyles[1]).ColumnName);
				AssertEquals("Tariff code column", AutoJobDeclarationMessageSendingEntryLine.Schema.TariffCode, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[2]).ColumnName);
				AssertEquals("Description column", AutoJobDeclarationMessageSendingEntryLine.Schema.Description, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[3]).ColumnName);
				AssertEquals("Quota ord no column", AutoJobDeclarationMessageSendingEntryLine.Schema.QuotaOrdNo, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[4]).ColumnName);
				AssertEquals("Quota quantity column", AutoJobDeclarationMessageSendingEntryLine.Schema.QuotaQuantity, ((ZCalcEditColumnStyleInfo)grid.ColumnStyles[5]).ColumnName);
				AssertEquals("Sup uq column", AutoJobDeclarationMessageSendingEntryLine.Schema.SupUq, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[6]).ColumnName);
			});
		});
	}

	void RunForControl(Action<MessageSendingEntryLinesGridUserControl> methodToRun)
	{
		using (var form = new ZForm(sendingObjectParent))
		{
			using (var control = new MessageSendingEntryLinesGridUserControl())
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
		sendingObjectParent = new RetrospectiveQuotaRequestMessageSendingObjectParent(declaration);
	}

	RetrospectiveQuotaRequestMessageSendingObjectParent sendingObjectParent;
}
