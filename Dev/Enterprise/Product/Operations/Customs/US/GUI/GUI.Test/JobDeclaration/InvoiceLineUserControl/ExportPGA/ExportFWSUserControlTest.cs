using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<ExportFWSUserControl>))]
	sealed class ExportFWSUserControlTest : ZPGAFormBasherAbstractTest<ExportFWSUserControl>
	{
		protected override string BindMember => "FilteredInvoiceLines.ExportFWS";

		protected override BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine) => invoiceLine.ExportFWS;
	}

	[TestedType(typeof(ProductPGATestForm<ExportFWSUserControl>))]
	sealed class ExportProductFWSUserControlTest : ZProductPGAFormBasherAbstractTest<ExportFWSUserControl>
	{
		protected override string BindMember => "PivotsForBinding.ExportFWS";

		protected override BusinessObject GetPGABusinessObject(CusClassPartPivot pivot)
		{
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			return pivot.ExportFWS;
		}
	}
}
