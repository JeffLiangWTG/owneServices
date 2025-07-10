using CargoWise.Windows.UI;
using Enterprise.Customs.TR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	class JobDeclarationFormTest : EU.GUI.Testing.JobDeclarationFormTest<JobDeclaration>
	{
		public override CargoWise.Types.ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			declaration.CusSupportingInfoList.AddNew();
			declaration.CusContainers.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			declaration.Bills.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.MergedLines.AddNew();
			return declaration;
		}

		public override void TestMinimumSizeNotTooBig()
		{
			var minScreenWidthSupported = ControlDpiScalingHelper.ScaleToCurrentDpiX(1366);
			var minScreenHeightSupported = ControlDpiScalingHelper.ScaleToCurrentDpiY(930);
			using (var form = new JobDeclarationForm(Declaration))
			{
				Assert("TR Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + minScreenWidthSupported, form.MinimumSize.Width <= minScreenWidthSupported);
				Assert("TR Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + minScreenHeightSupported, form.MinimumSize.Height <= minScreenHeightSupported);
			}
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;
	}
}
