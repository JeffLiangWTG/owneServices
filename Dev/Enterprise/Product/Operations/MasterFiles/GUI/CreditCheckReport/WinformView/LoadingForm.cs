using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class LoadingForm : KForm, ICaptionRenderingSupport
	{
		public LoadingForm(LoadingFormModel<object> model)
		{
			InitializeComponent();
			SetDataBinding(model, string.Empty);
			Text = model.Title;
			Model = model;
			Model.RunWorkerCompleted += ViewModel_RunWorkerCompleted;
		}

		public LoadingFormModel<object> Model { get; }

		public async Task ShowAndRunningTaskAsync(Form parentForm)
		{
			ZFormModaliser.Show(this, parentForm);
			await Model.StartRunning();
		}

		internal void ViewModel_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Close();
		}

		#region ICaptionRenderingSupport

		bool? ICaptionRenderingSupport.CaptionRenderingEnabled
		{
			get { return true; }
		}

		event EventHandler ICaptionRenderingSupport.CaptionRenderingEnabledChanged
		{
			add { }
			remove { }
		}

		#endregion
	}
}
