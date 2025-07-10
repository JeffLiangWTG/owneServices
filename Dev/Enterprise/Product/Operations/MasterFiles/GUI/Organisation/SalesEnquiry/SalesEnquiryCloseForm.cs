using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesEnquiryCloseForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public SalesEnquiryCloseForm()
		{
			InitializeComponent();
		}

		public SalesEnquiryCloseForm(SalesEnquiryCloseAction action)
			: base(action)
		{
			CloseButton.DataBindings.Add(new KBinding("IsEnabledForBinding", Action, "IsClosable"));
		}

		SalesEnquiryCloseAction Action
		{
			get { return BusinessEntity as SalesEnquiryCloseAction; }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Action.Synchronise();
			if (!Action.HasErrors)
			{
				DialogResult = DialogResult.OK;
			}
			else
			{
				ShowErrorsDialog();
				DialogResult = DialogResult.None;
			}
		}

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}
