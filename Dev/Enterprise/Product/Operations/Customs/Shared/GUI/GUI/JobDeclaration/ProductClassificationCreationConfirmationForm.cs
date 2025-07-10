using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class ProductClassificationCreationConfirmationForm : ZChildForm
	{
		public ProductClassificationCreationConfirmationForm(BaseJobDeclaration declaration)
			: base()
		{
			this.declaration = declaration;
			DialogResult = DialogResult.None;
		}

		readonly BaseJobDeclaration declaration;

		public override string FormHeading
		{
			get { return Res.GetString("C14F83BF-3EE5-4ADD-8113-C4AE638C221F", "No Existing Product Classification To Match"); }
		}

		public override string FormCaption
		{
			get { return Res.GetString("D908517F-35BA-4D84-812E-B1DAEE858CBE", "Products for which there is no classification that matches the line on the invoice have been identified on this customs declaration.\r\n\r\nAll necessary classifications will be added to the products"); }
		}

		#region Events

		#region Accessibility Events

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.FormBorderStyle = FormBorderStyle.Sizable;
			InformationLabel.Text = FormCaption;
		}

		#endregion

		#region Click Events

		void ConfirmButton_Click(object sender, EventArgs e)
		{
			declaration.SaveNewPartClassifications();
			DialogResult = DialogResult.OK;
			Close();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion

		#endregion
	}
}
