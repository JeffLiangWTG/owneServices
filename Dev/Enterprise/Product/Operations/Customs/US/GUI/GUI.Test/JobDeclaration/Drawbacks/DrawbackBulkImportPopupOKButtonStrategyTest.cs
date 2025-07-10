using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class DrawbackBulkImportPopupOKButtonStrategyTest : TestCaseWithFactory
	{
		public void TestBulkImport()
		{
			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var entryFiler = new EntryFiler();
			entryFiler.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryFiler);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_ParentID = entryHeader.PK;
			entryNum.CE_EntryNum = "ENTRY1";
			entryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var line1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = line1.PK;
			line1.CL_LineNumber = 1;
			var line2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = line2.PK;
			line2.CL_LineNumber = 2;
			Factory.Save();
			var drawback = Factory.New<JobDeclaration>();
			drawback.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var strategy = new DrawbackBulkImportPopupOKButtonStrategy(drawback);
			strategy.HandleFindBoxOKButton(Array.Empty<BusinessObject>());
			AssertEquals(DrawbackBulkImportPopupOKButtonStrategy.SelectAtLeaseOneLine, UnitTestUserNotification.Instance.LastMessage.Text);
			strategy.HandleFindBoxOKButton(new[] { line1 });
			AssertEquals(1, drawback.InvoiceLines.Count);
			var drawbackLine1 = drawback.InvoiceLines[0];
			AssertEquals("drawbackLine1.US_ImportEntryNo", "XJ5ENTRY1", drawbackLine1.US_ImportEntryNo);
			AssertEquals("drawbackLine1.US_DRWImportEntryLine", 1, drawbackLine1.US_DRWImportEntryLine);
			AssertEquals("drawbackLine1.US_DRWClaimAmountOverriden_New", ZBool.False, drawbackLine1.US_DRWClaimAmountOverriden_New);
			AssertEquals(string.Format(DrawbackBulkImportPopupOKButtonStrategy.NoOfLinesImportedAndIgnored, 1, 0), UnitTestUserNotification.Instance.LastMessage.Text);
			strategy.HandleFindBoxOKButton(new[] { line1, line2 });
			AssertEquals(2, drawback.InvoiceLines.Count);
			AssertCollectionContains(drawbackLine1, drawback.InvoiceLines);
			AssertEquals("drawbackLine1.US_ImportEntryNo", "XJ5ENTRY1", drawbackLine1.US_ImportEntryNo);
			AssertEquals("drawbackLine1.US_DRWImportEntryLine", 1, drawbackLine1.US_DRWImportEntryLine);
			AssertEquals("drawbackLine1.US_DRWClaimAmountOverriden_New", ZBool.False, drawbackLine1.US_DRWClaimAmountOverriden_New);
			var drawbackLine2 = drawback.InvoiceLines[1];
			if (drawbackLine2 == drawbackLine1)
			{
				drawbackLine2 = drawback.InvoiceLines[0];
			}

			AssertEquals("drawbackLine2.US_ImportEntryNo", "XJ5ENTRY1", drawbackLine2.US_ImportEntryNo);
			AssertEquals("drawbackLine2.US_DRWImportEntryLine", 2, drawbackLine2.US_DRWImportEntryLine);
			AssertEquals("drawbackLine2.US_DRWClaimAmountOverriden_New", ZBool.False, drawbackLine2.US_DRWClaimAmountOverriden_New);
			AssertEquals(string.Format(DrawbackBulkImportPopupOKButtonStrategy.NoOfLinesImportedAndIgnored, 1, 1), UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
