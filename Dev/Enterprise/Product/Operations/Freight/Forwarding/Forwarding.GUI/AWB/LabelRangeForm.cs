using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class LabelRangeForm : ZChildForm
	{
		public LabelRangeForm(AWBActions businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
			ParentPackagesRadioButton.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new KBinding("Caption", businessEntity, AWBActions.Schema.ParentName, true));  // Property Name
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("b441372d-4bd1-4f7b-a435-76b663863be7", "document"), Res.GetString("73ebddb8-2f1e-4e94-a826-8ae2c7e16001", "print"), Res.GetString("62cc1420-a6e9-4ab2-b2b8-28a6374c5e2d", "printed"), includeIgnoreOption); // Hard-coded constant
		}

		AWBActions AWBActions
		{
			get { return (AWBActions)BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#region Buttons

		void OKButton_Click(object sender, EventArgs e)
		{
			if (AWBActions.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				AWBActions.PrintAWBBarcodeLabel();
				AWBActions.SaveSettings();
				DialogResult = DialogResult.OK;
			}
		}

		void Cancel_Button_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		#endregion
	}
}
