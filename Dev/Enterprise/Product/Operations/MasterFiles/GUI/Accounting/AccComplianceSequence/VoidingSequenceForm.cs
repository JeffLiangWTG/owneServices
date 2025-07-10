using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class VoidingSequenceForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public VoidingSequenceForm()
		{
			InitializeComponent();
		}

		readonly AccComplianceSequenceForm SequenceForm;

		public VoidingSequenceForm(VoidingSequenceNumberBusinessObject bo, AccComplianceSequenceForm sequenceForm)
			: base(bo)
		{
			InitializeComponent();
			this.SequenceForm = sequenceForm;
		}

		internal void OKButton_Click(object sender, EventArgs e)
		{
			VoidingSequenceNumberBusinessObject bo = (VoidingSequenceNumberBusinessObject)BusinessEntity;
			if (!bo.HasErrors)
			{
				DialogResult result = Globals.Message.ShowConfirmation(Res.GetString("9821EC11-D2D6-4C4B-946F-CA22B0804AD4", "You are going to void sequence numbers. Are you sure you want to continue to save?"),
					Res.GetString("8BAE4F72-2282-468C-AE16-3AB6A7404967", "Continue To Save?"),
					Res.GetString("B8621EEB-B317-4775-B030-9A65F54E313F", "To continue, type:") + " ",
					Res.GetString("11555F69-0774-49EB-AD9C-DAD3EF9D1F33", "Yes"), MessageBoxIcon.Question);
				if (result == DialogResult.OK)
				{
					bo.VoidNumbersInRange();
					this.Close();
					if (SequenceForm != null)
					{
						SequenceForm.UpdateGUIIfBookFull(true);
					}
				}
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
