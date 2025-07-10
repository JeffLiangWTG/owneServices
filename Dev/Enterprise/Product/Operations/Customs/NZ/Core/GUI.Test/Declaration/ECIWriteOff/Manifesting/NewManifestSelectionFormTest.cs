using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting.Testing
{
	[TestedType(typeof(NewManifestSelectionForm))]
	public class NewManifestSelectionFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var manifestCreator = new NewManifestCreator(Factory, GlbCompany.CurrentCompany.PK);
			using (var form = new NewManifestSelectionForm(manifestCreator))
			{
				form.Show();
				Assert(form.Text.EndsWith("ECI Write-Off Manifest"));
			}
		}

		public void TestGridHasRecordsInItAfterShow()
		{
			var declaration = Business.Declaration.JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = Business.JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_EntryStatus = Business.LowValueConsignmentStatusList.Codes.ReadyForManifesting;
			Factory.Save();
			var manifestCreator = new NewManifestCreator(Factory, GlbCompany.CurrentCompany.PK);
			using (var form = new NewManifestSelectionForm(manifestCreator))
			{
				form.Show();
				var potentialManifestsGrid = form.FindSingle<ZGrid>("PotentialManifestsGrid");
				AssertEquals("Form.PotentialManifestsGrid.List.Count", 1, potentialManifestsGrid.List.Count);
			}
		}

		public void TestCreateManifestFailure()
		{
			var declaration = Business.Declaration.JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = Business.JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_EntryStatus = Business.LowValueConsignmentStatusList.Codes.ReadyForManifesting;
			declaration.JE_MasterBill = "081-11111111";
			declaration.JE_VoyageFlightNo = "QF253";
			declaration.JE_RL_NKPortOfLoading = "USLAX";
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			Factory.Save();
			var manifestCreator = new NewManifestCreator(Factory, GlbCompany.CurrentCompany.PK);
			using (var form = new NewManifestSelectionForm(manifestCreator))
			{
				var isClosed = false;
				form.FormClosed += (object sender, FormClosedEventArgs e) =>
				{
					isClosed = true;
				}

				;
				form.Show();
				var potentialManifestsGrid = form.FindSingle<ZGrid>("PotentialManifestsGrid");
				AssertEquals("Form.PotentialManifestsGrid.List.Count", 1, potentialManifestsGrid.List.Count);
				potentialManifestsGrid.Select(0);
				var manifest = (PotentialManifest)potentialManifestsGrid.SelectedElements[0];
				declaration.Delete();
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FindSingle<ZButton>("CreateManifestButton").PerformClick();
				AssertEquals("LastMessage.Text", "A Manifest could not be created from the selection.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!isClosed);
			}
		}

		protected override Form GetFormToBashCore() => new NewManifestSelectionForm(new NewManifestCreator(Factory, GlbCompany.CurrentCompany.PK));
	}
}
