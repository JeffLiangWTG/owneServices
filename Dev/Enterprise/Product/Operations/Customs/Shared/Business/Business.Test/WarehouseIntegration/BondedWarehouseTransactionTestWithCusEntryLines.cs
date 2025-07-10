namespace Enterprise.Customs.Business.Testing
{
	sealed class BondedWarehouseTransactionTestWithCusEntryLines : BondedWarehouseTransactionTestWithInvoiceLines
	{
		protected override void SetupObjects()
		{
			declaration = GetNewJobDeclaration();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			entry = declaration.CustomsEntryHeaders.AddNew();
			entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			bondedWarehouseTransaction = declaration.GetNewBondedWarehouseTransactionForTesting();
			bondedWarehouseTransaction.SetEntryLineMode();
		}
	}
}
