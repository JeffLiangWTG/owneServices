using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI
{
	public partial class EntrySelectionForm : ZChildForm
	{
		public EntrySelectionForm(JobDeclaration declaration, ResourceStringData formTitle) : base(declaration)
		{
			this.declaration = declaration;
			InitializeComponent();
			CaptionResourceString = formTitle;
			EntrySelectionGridGroupBox.CaptionResourceString = formTitle;
		}

		void OkButton_Click(object sender, System.EventArgs e)
		{
			var firstEntrySelection = declaration.EntrySelections[0];
			firstEntrySelection.Validation.ValidateAll();

			if (firstEntrySelection.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("EBCBCD5B-5C8F-43CD-A08E-547164D4168D", "There are errors - Please solve them before continuing"), Res.GetString("A17E832D-D75D-4B7A-B9E5-5FD2B98FF15D", "Errors!"));
				this.DialogResult = System.Windows.Forms.DialogResult.None;
			}
			else
			{
				Close();
			}
		}

		readonly JobDeclaration declaration;
	}
}
