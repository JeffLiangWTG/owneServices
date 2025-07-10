using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class CMDShipmentCusEntryNumberCollectionForm : ZChildForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public CMDShipmentCusEntryNumberCollectionForm(CMDShipmentWrapper cMDShipmentWrapper)
			: base(cMDShipmentWrapper)
		{
			cMDShipmentWrapper.CMDDataValuesForBinding.Load();
		}

		public override string FormHeading => ResString.GetMultilingualString("CMDMessaging|CMDShipmentCusEntryNumberCollectionForm|FormHeading", "Please Enter Permit or Exemption Details");

		public new CMDShipmentWrapper BusinessEntity => (CMDShipmentWrapper)base.BusinessEntity;

		void OKButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.CMDDataValuesForBinding.RunPreSaveValidation();
			if (!BusinessEntity.CMDDataValuesForBinding.HasErrors())
			{
				BusinessEntity.CMDDataValuesForBinding.CopyChangesToShipmentFactory();
				BusinessEntity.ResetPermitAndExemptionDetails();
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				Globals.Message.ShowError("There are errors that need to be fixed");
			}
		}

		#region IAllowTabBackwardBetweenSomeOfMyChildren Members

		public bool AllowTabBackward(Control control, Control previousControl)
		{
			return control == OKButton && previousControl == CloseButton;
		}

		#endregion
	}
}
