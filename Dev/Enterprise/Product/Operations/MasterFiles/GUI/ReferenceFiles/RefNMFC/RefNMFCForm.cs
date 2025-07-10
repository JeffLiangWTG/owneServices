using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	/// <summary>
	/// Summary description for RefNMFCForm.
	/// </summary>
	public partial class RefNMFCForm : ZTemplateForm
	{
		public RefNMFCForm()
		{
		}

		public RefNMFCForm(RefNMFC nMFC)
			: base(nMFC)
		{
		}

		#region ZForm Overrides

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#endregion
	}
}
