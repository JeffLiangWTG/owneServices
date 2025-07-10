using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class PhoneNumberValidationOverrideControl : ZUserControl
	{
		public PhoneNumberValidationOverrideControl(ZPropertyInfo phoneManuallyVerifiedPropertyInfo, string notificationMessage)
		{
			PhoneManuallyVerifiedPropertyInfo = phoneManuallyVerifiedPropertyInfo;
			InitializeComponent();
			this.notificationMessage = notificationMessage;
			InfoLabel.Text = notificationMessage + System.Environment.NewLine + System.Environment.NewLine + Res.GetString("16C9A31F-BC67-4EF3-85B5-5AAE5F03715D", "If you are very confident you can Accept as Entered.");

			HeaderLabel.AllowOverlap(Panel);

			ManualVerifyButton.MouseEnter += ManualVerifyButton_MouseEnter;
			ManualVerifyButton.MouseLeave += ManualVerifyButton_MouseLeave;
			ManualVerifyButton.Leave += ManualVerifyButton_Leave;
			ManualVerifyButton.Click += ManualVerifyButton_Click;
		}

		string notAcceptedReason;
		public string NotAcceptedReason
		{
			get { return notAcceptedReason; }
			set
			{
				notAcceptedReason = value;
				if (!string.IsNullOrEmpty(value))
				{
					ManualVerifyButton.Visible = false;
					this.InfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 168, true);
					InfoLabel.Text = notificationMessage + System.Environment.NewLine + System.Environment.NewLine + value;
				}
			}
		}

		readonly string notificationMessage;
		public readonly ZPropertyInfo PhoneManuallyVerifiedPropertyInfo;
		public event EventHandler PhoneNumberConfirmed;

		#region Enter

		/// <summary>
		/// Gets whether the control is entered.
		/// </summary>
		public bool IsEntered { get; private set; }

		#endregion

		#region Methods

		public void Close()
		{
			ManualVerifyButton.MouseEnter -= ManualVerifyButton_MouseEnter;
			ManualVerifyButton.MouseLeave -= ManualVerifyButton_MouseLeave;
			ManualVerifyButton.Leave -= ManualVerifyButton_Leave;
			ManualVerifyButton.Click -= ManualVerifyButton_Click;
			if (Parent != null)
			{
				Parent.Controls.Remove(this);
			}

			if (IsHandleCreated)
			{
				// Don't Dispose synchronously since we could be in the middle of Control.WmMouseDown
				BeginInvoke(new MethodInvoker(Dispose));
			}
			else
			{
				// Will almost certainly only get here during unit tests
				Dispose();
			}
		}

		#endregion

		#region Event Handlers

		void ManualVerifyButton_MouseEnter(object sender, EventArgs e)
		{
			IsEntered = true;
		}

		void ManualVerifyButton_MouseLeave(object sender, EventArgs e)
		{
			if (!ManualVerifyButton.Focused)
			{
				IsEntered = false;
			}
		}

		void ManualVerifyButton_Leave(object sender, EventArgs e)
		{
			IsEntered = false;
			Close();
		}

		void ManualVerifyButton_Click(object sender, EventArgs e)
		{
			if (PhoneManuallyVerifiedPropertyInfo != null)
			{
				PhoneManuallyVerifiedPropertyInfo.Value = ZBool.True;
				if (PhoneNumberConfirmed != null)
				{
					PhoneNumberConfirmed(this, EventArgs.Empty);
				}
			}
			Close();
		}

		#endregion
	}
}
