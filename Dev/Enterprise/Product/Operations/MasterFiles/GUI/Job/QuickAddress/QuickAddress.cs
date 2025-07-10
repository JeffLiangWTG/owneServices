using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class QuickAddressForm : ZChildForm
	{
		public QuickAddressForm(DocAddressCreatorHost host, Control positioningControl = null)
			: base(host)
		{
			PositioningControl = positioningControl;

			HideAndMoveControlsWhenEditingExistingJobDocAddress();
		}

		readonly Control PositioningControl;

		#region CloseButtonText

		string CloseButtonText
		{
			get { return Res.GetString("72e8ce58-1297-4f1c-a070-a685f6946f83", "Close"); }
		}

		#endregion

		#region HideAndMoveControlsWhenEditingExistingJobDocAddress

		void HideAndMoveControlsWhenEditingExistingJobDocAddress()
		{
			if (Host.IsEditingExistingJobDocAddress)
			{
				OkButton.Visible = false;
				CancelButtonX.Text = CloseButtonText;
				CartageAddressTypeDropEdit.Visible = false;

				MinimumSize = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(MinimumSize.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(MinimumSize.Height - NewAddressDocAddressControl.Top));
				Size = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(Size.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Size.Height - NewAddressDocAddressControl.Top));
				ControlDpiScalingHelper.SetTop(OkButton, OkButton.Top - NewAddressDocAddressControl.Top, false);
				ControlDpiScalingHelper.SetTop(CancelButtonX, CancelButtonX.Top - NewAddressDocAddressControl.Top, false);
				ControlDpiScalingHelper.SetTop(ref NewAddressDocAddressControl, 0, true);
			}
		}

		#endregion

		#region Overrides

		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupLayout();
		}

		#endregion

		#region SetDataBinding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (Host != null)
			{
				Host.AddressCaptionInfo.ValueChanged -= new EventHandler(AddressCaptionInfo_ValueChanged);
			}

			base.SetDataBinding(dataSource, dataMember);

			if (Host != null)
			{
				Host.AddressCaptionInfo.ValueChanged += new EventHandler(AddressCaptionInfo_ValueChanged);
				SetAddressCaption();
			}
		}

		#endregion

		#endregion

		#region SetupLayout

		void SetupLayout()
		{
			if (PositioningControl != null)
			{
				SuspendLayout();

				try
				{
					Point topLeft = GetDropDownLocation(PositioningControl);
					ControlDpiScalingHelper.SetTop(this, topLeft.Y, false);
					ControlDpiScalingHelper.SetLeft(this, topLeft.X, false);
				}
				finally
				{
					ResumeLayout();
				}
			}
		}

		#endregion

		#region GetDropDownLocation

		Point GetDropDownLocation(Control positioningControl)
		{
			Point topLeft = positioningControl.Parent.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(positioningControl.Left, positioningControl.Bottom, false));
			Rectangle screen = CachedScreenInfo.Instance.FromPoint(topLeft);

			var topRight = ControlDpiScalingHelper.NewScaledPoint(topLeft.X + Width, topLeft.Y, false);

			if (!screen.Contains(topRight))
			{
				ControlDpiScalingHelper.SetX(ref topLeft, topLeft.X + positioningControl.Width - Width, false);
			}

			var bottomLeft = ControlDpiScalingHelper.NewScaledPoint(topLeft.X, topLeft.Y + Height, false);

			if (!screen.Contains(bottomLeft))
			{
				ControlDpiScalingHelper.SetY(ref topLeft, topLeft.Y - positioningControl.Height - Height, false);
			}

			return topLeft;
		}

		#endregion

		#region Event Methods

		void AddressCaptionInfo_ValueChanged(object sender, EventArgs e)
		{
			SetAddressCaption();
		}

		void OkButton_Click(object sender, EventArgs e)
		{
			Host.Validation.ValidateAll();
			Host.DocAddress.Validation.ValidateAll();

			if (!Host.HasErrors && !Host.DocAddress.HasErrors)
			{
				Close();
				DialogResult = DialogResult.OK;
			}
		}

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			if (CancelButtonX.Text == CloseButtonText)
			{
				Host.DocAddress.Validation.ValidateAll();

				if (!Host.DocAddress.HasErrors)
				{
					DialogResult = DialogResult.Cancel;
					Close();
				}
			}
			else
			{
				DialogResult = DialogResult.Cancel;
				Close();
			}
		}

		#endregion

		#region SetAddressCaption

		void SetAddressCaption()
		{
			NewAddressDocAddressControl.Text = Host.AddressCaption;
		}

		#endregion

		#region Host

		DocAddressCreatorHost Host
		{
			get { return (DocAddressCreatorHost)DataSource; }
		}

		#endregion
	}
}
