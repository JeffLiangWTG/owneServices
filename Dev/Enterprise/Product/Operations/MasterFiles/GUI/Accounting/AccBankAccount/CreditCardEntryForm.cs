using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CreditCardEntryForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public CreditCardEntryForm()
		{
			InitializeComponent();
		}

		public CreditCardEntryForm(CreditCardNumberBusinessObject bo)
			: base(bo)
		{
			InitializeComponent();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			CreditCardNumberBusinessObject bo = (CreditCardNumberBusinessObject)BusinessEntity;
			if (!bo.HasErrors)
			{
				bo.Encrypt();
				this.Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
