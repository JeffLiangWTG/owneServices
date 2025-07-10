using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesEnquiryForm : ZTemplateForm
	{
		public SalesEnquiryForm()
		{
			InitializeComponent();
		}

		public SalesEnquiryForm(SalesEnquiry enquiry)
			: base(enquiry)
		{
			InitializeComponent();
			WorkflowTabPage.Initialize(enquiry);
			enquiryDetailsControl.Initialize(enquiry);
		}

		internal SalesEnquiry SalesEnquiry
		{
			get { return (SalesEnquiry)base.BusinessEntity; }
		}

		public DialogResult PromptUserForSave()
		{
			DialogResult result = Globals.Message.Show(Res.GetString("759E0706-01B7-4A3D-B237-7513B790494E", @"You cannot proceed until the inquiry has been saved.

Do you wish to save changes?"),
				Res.GetString("13553F9B-10CB-4BFB-A91D-96944B884C11", "Save Before Proceeding?"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

			if (result == DialogResult.OK)
			{
				SalesEnquiry.RunPreSaveValidation();
				if (SalesEnquiry.HasErrors)
				{
					ShowErrorsDialog();
					result = DialogResult.Cancel;
				}
				else
				{
					try
					{
						SalesEnquiry.Factory.Save();
					}
					catch (ZSaveConcurrencyException ex)
					{
						ZExceptionReporting.HandleZSaveConcurrencyException(ex, NotificationHandler.Instance);
						result = DialogResult.Cancel;
					}
				}
			}

			return result;
		}

		protected override void Save(CargoWise.Integration.ITransactionParticipant[] factories)
		{
			base.Save(factories);
			enquiryDetailsControl.Refresh();
		}
	}
}
