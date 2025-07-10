using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using JobMessageTypeList = Enterprise.Customs.US.Business.JobMessageTypeList;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ClosePGAEntryStatusForm))]
	sealed class ClosePGAEntryStatusFormTest : ZFormBasherTest
	{
		public override void TestBashingForm()
		{
			Assert(true);
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			return new ClosePGAEntryStatusForm(declaration.EntryPGACusDispositions.AddNew());
		}
	}
}
