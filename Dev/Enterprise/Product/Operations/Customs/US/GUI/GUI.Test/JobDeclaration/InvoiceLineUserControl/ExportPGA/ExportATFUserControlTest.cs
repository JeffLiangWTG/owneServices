using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<ExportATFUserControl>))]
	sealed class ExportATFUserControlTest : ZPGAFormBasherAbstractTest<ExportATFUserControl>
	{
		protected override string BindMember => "FilteredInvoiceLines.ExportATF";

		protected override BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine) => invoiceLine.ExportATF;
	}

	[TestedType(typeof(ProductPGATestForm<ExportATFUserControl>))]
	sealed class ExportProductATFUserControlTest : ZProductPGAFormBasherAbstractTest<ExportATFUserControl>
	{
		protected override string BindMember => "PivotsForBinding.ExportATF";

		protected override BusinessObject GetPGABusinessObject(CusClassPartPivot pivot)
		{
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			return pivot.ExportATF;
		}
	}
}
