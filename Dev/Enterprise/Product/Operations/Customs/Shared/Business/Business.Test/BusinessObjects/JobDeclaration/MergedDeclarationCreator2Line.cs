using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class MergedDeclarationCreator2Line<T> : MergedDeclarationCreator<T>
		where T : BaseJobDeclaration
	{
		public MergedDeclarationCreator2Line(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public MergedDeclarationCreator2Line(BusinessObjectFactory factory, string applicationCode)
			: base(factory, applicationCode)
		{
		}

		protected override void SetupDeclarationPreMerge(T declaration)
		{
			var invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines.AddNew();
			declaration.FilteredInvoiceLines.Add(invoiceLine);
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		}

		public CusEntryLine EntryLine2
		{
			get { return Declaration.CustomsEntryHeaders[0].MergedLines[1]; }
		}

		public BaseJobComInvoiceLine InvoiceLine2
		{
			get { return Declaration.FilteredInvoiceLines[1]; }
		}
	}
}
