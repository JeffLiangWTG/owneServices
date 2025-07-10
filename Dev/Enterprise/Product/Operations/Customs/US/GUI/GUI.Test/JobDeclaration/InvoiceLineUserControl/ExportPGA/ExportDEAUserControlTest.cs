using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<ExportDEAUserControl>))]
	sealed class ExportDEAUserControlTest : ZPGAFormBasherAbstractTest<ExportDEAUserControl>
	{
		protected override string BindMember => "FilteredInvoiceLines.DEAHeaders";

		protected override CargoWise.EntityFramework.BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine) => invoiceLine.DEAHeaders.AddNew();
	}
}
