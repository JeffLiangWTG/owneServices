using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ExternalValidationResultForm : ZChildForm
	{
		public ExternalValidationResultForm()
		{
			InitializeComponent();
		}

		public ExternalValidationResultForm(OrgHeader organisation, ExternalValidationResult result)
			: base(new ExternalValidationResultWrapper(result))
		{
			InitializeComponent();
			this.organisation = organisation;
		}

		ExternalValidationResultWrapper validationResultWrapper
		{
			get { return BusinessEntity as ExternalValidationResultWrapper; }
		}

		readonly OrgHeader organisation;

		public override string FormCaption
		{
			get { return Res.GetString("ExternalValidationResultForm|ad69d454-8ff1-4fe2-a850-87b04a89f0b9", "{0} Validation Results", organisation.HumanReadableName); }
		}

		void ExternalValidationResultForm_Load(object sender, EventArgs e)
		{
			UpdateLabel();
		}

		void UpdateLabel()
		{
			if (validationResultWrapper.ResultIsValid)
			{
				zLabel.ForeColor = System.Drawing.Color.Green;
				zLabel.Text = Res.GetString("ExternalValidationResultForm|0820c9da-f66e-4d88-bb37-c879b995da21", "This organization passed validation against the external web service.\r\nPlease review the messages below.");
			}
			else
			{
				zLabel.ForeColor = System.Drawing.Color.Red;
				zLabel.Text = Res.GetString("ExternalValidationResultForm|564208d0-039a-4b91-bb0e-6880e6add726", "This organization failed validation against the external web service.\r\nPlease review the messages below.");
			}
		}

		#region GUI Setup

		public override string FormVerb
		{
			get
			{
				return "";
			}
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}

		#endregion

		void buttonClose_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
