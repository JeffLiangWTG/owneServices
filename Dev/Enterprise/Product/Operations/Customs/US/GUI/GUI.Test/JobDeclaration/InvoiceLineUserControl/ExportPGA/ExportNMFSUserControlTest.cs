using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<ExportNMFSUserControl>))]
	sealed class ExportNMFSUserControlTest : ZPGAFormBasherAbstractTest<ExportNMFSUserControl>
	{
		protected override string BindMember => "FilteredInvoiceLines.NMFSLines";

		protected override BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine) => invoiceLine.NMFSLines.AddNew();
	}

	[TestedType(typeof(ProductPGATestForm<ExportNMFSUserControl>))]
	sealed class ExportProductNMFSUserControlTest : ZProductPGAFormBasherAbstractTest<ExportNMFSUserControl>
	{
		protected override string BindMember => "PivotsForBinding.NMFSLines";

		protected override BusinessObject GetPGABusinessObject(CusClassPartPivot pivot)
		{
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			return pivot.NMFSLines.AddNew();
		}
	}
}
