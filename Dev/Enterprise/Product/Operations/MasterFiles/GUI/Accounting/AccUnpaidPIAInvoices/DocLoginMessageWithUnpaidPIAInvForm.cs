using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class DocLoginMessageWithPIAInvForm : ZChildForm
	{
		public static DialogResult ShowForm(IRelatedJobNumber relatedJobNumber, ZString message, ZString caption)
		{
			return ZFormModaliser.ShowDialogAndDispose(new DocLoginMessageWithPIAInvForm(relatedJobNumber, message, caption));
		}

		#region Protected Constructors

		protected internal DocLoginMessageWithPIAInvForm(IRelatedJobNumber relatedJobNumber, ZString message, ZString caption)
			: base()
		{
			fRelatedJobNumber = relatedJobNumber;
			fCaption = caption;
			fMessage = message;
#if DEBUG
			TypeDescriptor.AddAttributes(this, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(MessageLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		DocLoginMessageWithPIAInvForm()
		{
			InitializeComponent();
		}

		#endregion

		#region Overrides

		public override string FormCaption
		{
			get { return fCaption; }
		}

		#endregion

		#region Implementation

		readonly IRelatedJobNumber fRelatedJobNumber;
		readonly ZString fCaption;
		readonly ZString fMessage;

		void DocLoginMessageWithPIAInvForm_Load(object sender, EventArgs e)
		{
			MessageLabel.Text = fMessage;
		}

		void VewUnPaidPIAInvButton_Click(object sender, EventArgs e)
		{
			ShipmentUnpaidPIAInvoicesForm.ShowForm(fRelatedJobNumber);
		}

		void MessageLabel_SizeChanged(object sender, EventArgs e)
		{
			var scaleX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(MessageLabel.Bounds.Right);
			var scaleY = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(MessageLabel.Bounds.Bottom + BottomPanel.Height);

			ClientSize = ControlDpiScalingHelper.NewScaledSize(scaleX, scaleY);
		}

		#endregion
	}
}
