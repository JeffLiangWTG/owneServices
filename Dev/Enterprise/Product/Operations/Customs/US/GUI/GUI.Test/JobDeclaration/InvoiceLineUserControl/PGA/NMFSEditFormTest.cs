using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(NMFSEditForm))]
	sealed class NMFSEditFormTest : ZFormBasherTest
	{
		public void TestControlsVisibilityForProgramType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var nmfsLine = invoiceLine.NMFSLines.AddNew();
			using (var form = new NMFSEditForm(nmfsLine))
			{
				form.Show();
				AssertEquals("LeftTopPanel.Visible", true, form.LeftTopPanel.Visible);
				AssertEquals("No. of visible controls", 6, form.LeftTopPanel.Controls.Cast<Control>().Count(c => c.Visible));
				AssertEquals("LineNoTextBox.Visible", true, form.LineNoTextBox.Visible);
				AssertEquals("DISDocumentIDDropEdit.Visible", true, form.DISDocumentIDDropEdit.Visible);
				AssertEquals("IFTPPermitNumberTextBox.Visible", true, form.IFTPPermitNumberTextBox.Visible);
				AssertEquals("EBCDNumberTextBox.Visible", true, form.EBCDNumberTextBox.Visible);
				AssertEquals("HMSPermitNumberTextBox.Visible", true, form.HMSPermitNumberTextBox.Visible);
				AssertEquals("DocumentTypeDropEdit.Visible", true, form.DocumentTypeDropEdit.Visible);
				AssertEquals("SourceTypeDropEdit.Visible", false, form.SourceTypeDropEdit.Visible);
			}

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			using (var form = new NMFSEditForm(nmfsLine))
			{
				form.Show();
				AssertEquals("LeftTopPanel.Visible", true, form.LeftTopPanel.Visible);
				AssertEquals("No. of visible controls", 8, form.LeftTopPanel.Controls.Cast<Control>().Count(c => c.Visible));
				AssertEquals("LineNoTextBox.Visible", true, form.LineNoTextBox.Visible);
				AssertEquals("DISDocumentIDDropEdit.Visible", true, form.DISDocumentIDDropEdit.Visible);
				AssertEquals("ObserverStatementCheckBox.Visible", true, form.ObserverStatementCheckBox.Visible);
				AssertEquals("IFTPPermitNumberTextBox.Visible", true, form.IFTPPermitNumberTextBox.Visible);
				AssertEquals("IDCPMemberCertificationCheckBox.Visible", true, form.IDCPMemberCertificationCheckBox.Visible);
				AssertEquals("CaptainStatementCheckBox.Visible", true, form.CaptainStatementCheckBox.Visible);
				AssertEquals("DocumentTypeDropEdit.Visible", true, form.DocumentTypeDropEdit.Visible);
				AssertEquals("DolphinSafeStatusDropEdit.Visible", true, form.DolphinSafeStatusDropEdit.Visible);
				AssertEquals("SourceTypeDropEdit.Visible", false, form.SourceTypeDropEdit.Visible);
			}

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			using (var form = new NMFSEditForm(nmfsLine))
			{
				form.Show();
				AssertEquals("LeftTopPanel.Visible", true, form.LeftTopPanel.Visible);
				AssertEquals("No. of visible controls", 6, form.LeftTopPanel.Controls.Cast<Control>().Count(c => c.Visible));
				AssertEquals("LineNoTextBox.Visible", true, form.LineNoTextBox.Visible);
				AssertEquals("DISDocumentIDDropEdit.Visible", true, form.DISDocumentIDDropEdit.Visible);
				AssertEquals("IFTPPermitNumberTextBox.Visible", true, form.IFTPPermitNumberTextBox.Visible);
				AssertEquals("EBCDNumberTextBox.Visible", true, form.EBCDNumberTextBox.Visible);
				AssertEquals("HMSPermitNumberTextBox.Visible", true, form.HMSPermitNumberTextBox.Visible);
				AssertEquals("DocumentTypeDropEdit.Visible", true, form.DocumentTypeDropEdit.Visible);
				AssertEquals("SourceTypeDropEdit.Visible", false, form.SourceTypeDropEdit.Visible);
			}

			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			using (var form = new NMFSEditForm(nmfsLine))
			{
				form.Show();
				AssertEquals("LeftTopPanel.Visible", true, form.LeftTopPanel.Visible);
				AssertEquals("No. of visible controls", 9, form.LeftTopPanel.Controls.Cast<Control>().Count(c => c.Visible));
				AssertEquals("LineNoTextBox.Visible", true, form.LineNoTextBox.Visible);
				AssertEquals("SpeciesCodeFindBox.Visible", true, form.SpeciesCodeFindBox.Visible);
				AssertEquals("OtherAuthorizationNumberTextBox.Visible", true, form.OtherAuthorizationNumberTextBox.Visible);
				AssertEquals("IFTPPermitNumberTextBox.Visible", true, form.IFTPPermitNumberTextBox.Visible);
				AssertEquals("NetWeightCalcEdit.Visible", true, form.NetWeightCalcEdit.Visible);
				AssertEquals("ConfidentialCheckBox.Visible", true, form.ConfidentialCheckBox.Visible);
				AssertEquals("NetWeightUQDropEdit.Visible", true, form.NetWeightUQDropEdit.Visible);
				AssertEquals("AuthorizationTypeDropEdit.Visible", true, form.AuthorizationTypeDropEdit.Visible);
				AssertEquals("SourceTypeDropEdit.Visible", true, form.SourceTypeDropEdit.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var line = Factory.New<NMFSLine>();
			line.HasChanges = false;
			return new NMFSEditForm(line);
		}
	}
}
