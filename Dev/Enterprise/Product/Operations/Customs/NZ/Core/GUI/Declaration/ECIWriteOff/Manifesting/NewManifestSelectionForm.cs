using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting
{
	public partial class NewManifestSelectionForm : ZChildForm
	{
		private ZArchitecture.ZGrid potentialManifestsGrid;
		private ZGroupBox potentialManifestsGroupBox;
		private ZButton createManifestButton;
		private ZButton closeButton;
		private readonly System.ComponentModel.Container components;

		public NewManifestSelectionForm()
		{
		}

		public NewManifestSelectionForm(NewManifestCreator manifestCreator)
			: base(manifestCreator)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected NewManifestCreator ManifestCreator => (NewManifestCreator)BusinessEntity;

		private void NewManifestSelectionForm_Load(object sender, System.EventArgs e)
		{
			ManifestCreator.PotentialManifests.Load();
		}

		private void CreateManifestButton_Click(object sender, System.EventArgs e)
		{
			CreateManifestFromCurrentlySelectedRow();
		}

		private void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		void CreateManifestFromCurrentlySelectedRow()
		{
			if (potentialManifestsGrid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowError(Res.GetString("Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting.NewManifestSelectionForm|SelectARowInTheGrid",
					"Please select a Row in the Grid to turn into a Manifest."), this.Text);
			}
			else
			{
				var manifest = (PotentialManifest)potentialManifestsGrid.SelectedElements[0];
				var pertinentQuestion = Res.GetString("Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting.NewManifestSelectionForm|PertinentQuestion",
					"Are you sure you want to Create a Manifest for Flight: {0} on {1}\r\ncontaining {2} ECI Write-Off Declarations?",
					manifest.JE_VoyageFlightNo, manifest.BarrierDate.ToShortDateString(), manifest.DeclarationCount.ToString());
				if (Globals.Message.Show(pertinentQuestion, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes)
				{
					var manifestEntryHeader = ManifestCreator.CreateManifestFrom(manifest);
					if (manifestEntryHeader != null)
					{
						Close();
						var controller = ZControllerFactory.Create(ControllerIDs.Customs.NZ.ECIWriteOffManifesting);
						controller.ShowEditForm(manifestEntryHeader);
					}
					else
					{
						Globals.Message.ShowWarning(Res.GetString("Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting.NewManifestSelectionForm|ManifestCouldNotBeCreated",
							"A Manifest could not be created from the selection."));
					}
				}
			}
		}
	}
}


