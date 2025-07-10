using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Design;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	[DesignerSerializer(typeof(ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class FormStateXmlExplorerMessageForm : Form
	{
		public FormStateXmlExplorerMessageForm()
		{
			InitializeComponent();

			button1.Click += (s, e) => Close();
			button2.Click += (s, e) => Close();
			AcceptButton = button1;
			CancelButton = button2;
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			this.AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			this.AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(levent);
		}

		public static void ShowInformationDialog(string message)
		{
			using (FormStateXmlExplorerMessageForm messageForm = new FormStateXmlExplorerMessageForm())
			{
				messageForm.messageTextBox.Text = message;
				messageForm.button1.Visible = false;
				messageForm.button2.Text = (NoResString)"Close";
				messageForm.ShowDialog();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need to localize tool message")]
		public static DialogResult ShowConfirmationDialog(string message)
		{
			using (FormStateXmlExplorerMessageForm messageForm = new FormStateXmlExplorerMessageForm())
			{
				messageForm.messageTextBox.Text = message;

				DialogResult dialogResult = DialogResult.No;
				messageForm.button1.Text = (NoResString)"Continue";
				messageForm.button1.Click += (s, e) => dialogResult = DialogResult.Yes;
				messageForm.button2.Text = "Cancel";
				messageForm.ShowDialog();

				return dialogResult;
			}
		}
	}
}
