using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	[ExcludeFromOverriddenAddNewTest]
	public class InvoiceLinesForEntryLineCollection : Customs.Business.InvoiceLinesForEntryLineCollection
	{
		public InvoiceLinesForEntryLineCollection(CusEntryLine entryLine)
			: base(entryLine)
		{
		}

		public new JobComInvoiceLine this[int index]
		{
			get { return (JobComInvoiceLine)base[index]; }
		}

		public new JobComInvoiceLine AddNew()
		{
			return (JobComInvoiceLine)base.AddNew();
		}
	}
}
