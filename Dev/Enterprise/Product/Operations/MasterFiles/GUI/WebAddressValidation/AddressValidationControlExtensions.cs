using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	public static class AddressValidationControlExtensions
	{
		public static void AdjustLocation(
			this Control validationControl,
			Form applicationForm,
			Control parentControl,
			Control referenceControl,
			Orientation orientation,
			int maxHeight = 0)
		{
			var isRelatedControlDisposed =
				(validationControl?.IsDisposed ?? true) ||
				(applicationForm?.IsDisposed ?? true) ||
				(parentControl?.IsDisposed ?? true) ||
				(referenceControl?.IsDisposed ?? true);

			if (isRelatedControlDisposed)
			{
				return;
			}

			var referenceControlIsTextBox = referenceControl is TextBox;
			var marginX = referenceControlIsTextBox ? ControlDpiScalingHelper.ScaleToCurrentDpiX(2) : 0;
			var marginY = referenceControlIsTextBox ? ControlDpiScalingHelper.ScaleToCurrentDpiY(2) : 0;

			var parentScreenLocation = parentControl.PointToScreen(Point.Empty);
			var referenceScreenLocation = referenceControl.PointToScreen(Point.Empty);

			var referenceRelativeLeftX = referenceScreenLocation.X - parentScreenLocation.X - marginX;
			var referenceRelativeRightX = referenceRelativeLeftX + referenceControl.Width;

			var referenceRelativeTopY = referenceScreenLocation.Y - parentScreenLocation.Y - marginY;
			var referenceRelativeBottomY = referenceRelativeTopY + referenceControl.Height;

			var x = 0;
			var y = 0;

			if (orientation == Orientation.Horizontal)
			{
				marginX = ControlDpiScalingHelper.ScaleToCurrentDpiX(1);

				x = referenceRelativeRightX + marginX + validationControl.Width > applicationForm.Width
					? referenceRelativeLeftX - marginX - validationControl.Width
					: referenceRelativeRightX + marginX;

				if (maxHeight != 0)
				{
					y = validationControl.Height > maxHeight
						? referenceRelativeBottomY - validationControl.Height
						: referenceRelativeTopY;
				}
				else
				{
					y = referenceRelativeTopY + validationControl.Height > applicationForm.Height
						? referenceRelativeBottomY - validationControl.Height
						: referenceRelativeTopY;
				}
			}
			else
			{
				marginY = ControlDpiScalingHelper.ScaleToCurrentDpiY(1);

				x = referenceRelativeRightX + validationControl.Width > applicationForm.Width
					? referenceRelativeRightX - validationControl.Width
					: referenceRelativeLeftX;

				y = referenceRelativeBottomY + marginY + validationControl.Height > applicationForm.Height
					? referenceRelativeTopY - marginY - validationControl.Height
					: referenceRelativeBottomY + marginY;
			}

			validationControl.Location = ControlDpiScalingHelper.NewScaledPoint(x, y, false);
		}
	}
}
