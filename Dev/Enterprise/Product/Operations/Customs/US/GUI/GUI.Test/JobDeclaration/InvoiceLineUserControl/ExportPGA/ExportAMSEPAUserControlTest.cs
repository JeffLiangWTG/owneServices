using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<ExportAMSEPAUserControl>))]
	sealed class ExportAMSEPAUserControlTest : ZPGAFormBasherAbstractTest<ExportAMSEPAUserControl>
	{
		protected override string BindMember => "FilteredInvoiceLines";

		protected override CargoWise.EntityFramework.BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine) => invoiceLine;
	}
}
