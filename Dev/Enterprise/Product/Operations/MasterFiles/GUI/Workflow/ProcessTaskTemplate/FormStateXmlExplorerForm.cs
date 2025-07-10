using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Design;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	[DesignerSerializer(typeof(ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class FormStateXmlExplorerForm : Form, IFormStateXmlExplorerView
	{
#if DEBUG
		public
#endif // DEBUG
		FormStateXmlExplorerForm()
		{
			InitializeComponent();
		}

		public FormStateXmlExplorerForm(FormStateXmlExplorerPresenter presenter)
			: this()
		{
			Argument.NotNull(presenter, "presenter");
			this.presenter = presenter;
			Init();
		}
		protected override void OnLayout(LayoutEventArgs levent)
		{
			this.AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			this.AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(levent);
		}

		readonly FormStateXmlExplorerPresenter presenter;

		string IFormStateXmlExplorerView.Xml
		{
			get { return richTextBox.Text; }
			set { richTextBox.Text = value; }
		}

		void Init()
		{
			presenter.Initialise(this);

			validateButton.Click += (s, e) => ValidateFormState();
			validateAndCommitButton.Click += (s, e) => CommitChanges();
			exitButton.Click += (s, e) => Close();
		}

		void ValidateFormState()
		{
			string message = presenter.Validate();

			if (!string.IsNullOrEmpty(message))
			{
				FormStateXmlExplorerMessageForm.ShowInformationDialog(message);
			}
			else
			{
				Globals.Message.Show((NoResString)"All good!");
			}
		}

		void CommitChanges()
		{
			string message = presenter.Validate();

			if (string.IsNullOrEmpty(message) || FormStateXmlExplorerMessageForm.ShowConfirmationDialog(message) == DialogResult.Yes)
			{
				message = presenter.SubmitChanges();

				if (!string.IsNullOrEmpty(message))
				{
					FormStateXmlExplorerMessageForm.ShowInformationDialog(message);
				}
				else
				{
					Close();
				}
			}
		}
	}
}