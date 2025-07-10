using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class CustomsDeliveryOrderForm : ZChildForm
	{
		public CustomsDeliveryOrderForm(JobDeclaration declaration)
			: base(declaration)
		{
			this.HeaderLineSplitContainer.Panel2MinSize = 270; // need to be set after the HeaderLineSplitContainer.Size is set or else the system throw an exception
			this.BillsContainersSplitContainer.Panel2MinSize = 260; // need to be set after the BillsContainersSplitContainer.Size is set or else the system throw an exception
			this.BillsContainersLinesSplitContainer.Panel2MinSize = 130; // need to be set after the BillsContainersLinesSplitContainer.Size is set or else the system throw an exception
			ChangeFormBorderStyleForRendering();
		}

		public new JobDeclaration BusinessEntity
		{
			get { return (JobDeclaration)base.BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return "Customs Delivery Order"; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
		}

		void CanclButton_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
			Close();
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.None;
			FireSaveButton();
		}

		void PrintButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.DeliveryOrderHeaders.HasAtLeastOneSelectedForPrinting)
			{
				if (FireSaveButton() == ContinueWithSave.Yes)
				{
					DialogResult = DialogResult.OK;
					Close();
				}
			}
			else
			{
				Globals.Message.ShowError("Cannot print as no Delivery Order has been selected for printing.", "No Delivery Order Selected");
			}
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, "Delivery Order", "save", "saved", includeIgnoreOption);
		}
	}
}
