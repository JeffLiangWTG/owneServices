using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressValidationControlExtensionsTest : TestCaseWithFactory
	{
		public void TestAdjustLocation_WhenGettingHorizontalOrientationAndNotEnoughSpaceToLeft_ShouldOpenToRight()
		{
			// Arrange.

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var referenceLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var referenceSize = ControlDpiScalingHelper.NewScaledSize(100, 100);

			var validationSize = ControlDpiScalingHelper.NewScaledSize(500, 250);

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var referenceControl = new Control { Location = referenceLocation, Size = referenceSize })
			using (var validationControl = new Control { Size = validationSize })
			{
				parentControl.Controls.Add(referenceControl);
				applicationForm.Controls.Add(parentControl);

				// Act.

				validationControl.AdjustLocation(
					applicationForm,
					parentControl,
					referenceControl,
					Orientation.Horizontal);

				// Assert.

				AssertEquals(
					100 + 1,
					ControlDpiScalingHelper.UnscaleFromCurrentDpiX(validationControl.Location.X));
			}
		}

		public void TestAdjustLocation_WhenGettingHorizontalOrientationAndNotEnoughSpaceToRight_ShouldOpenToLeft()
		{
			// Arrange.

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var referenceLocation = ControlDpiScalingHelper.NewScaledPoint(1900, 0);
			var referenceSize = ControlDpiScalingHelper.NewScaledSize(100, 100);

			var validationSize = ControlDpiScalingHelper.NewScaledSize(500, 250);

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var referenceControl = new Control { Location = referenceLocation, Size = referenceSize })
			using (var validationControl = new Control { Size = validationSize })
			{
				parentControl.Controls.Add(referenceControl);
				applicationForm.Controls.Add(parentControl);

				// Act.

				validationControl.AdjustLocation(
					applicationForm,
					parentControl,
					referenceControl,
					Orientation.Horizontal);

				// Assert.

				AssertEquals(
					1900 - 1 - 500,
					ControlDpiScalingHelper.UnscaleFromCurrentDpiX(validationControl.Location.X));
			}
		}

		public void TestAdjustLocation_WhenGettingHorizontalOrientationAndNotEnoughSpaceToTop_ShouldOpenToBottom()
		{
			// Arrange.

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var referenceLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var referenceSize = ControlDpiScalingHelper.NewScaledSize(100, 100);

			var validationSize = ControlDpiScalingHelper.NewScaledSize(500, 250);

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var referenceControl = new Control { Location = referenceLocation, Size = referenceSize })
			using (var validationControl = new Control { Size = validationSize })
			{
				parentControl.Controls.Add(referenceControl);
				applicationForm.Controls.Add(parentControl);

				// Act.

				validationControl.AdjustLocation(
					applicationForm,
					parentControl,
					referenceControl,
					Orientation.Horizontal);

				// Assert.

				AssertEquals(
					0,
					ControlDpiScalingHelper.UnscaleFromCurrentDpiY(validationControl.Location.Y));
			}
		}

		public void TestAdjustLocation_WhenGettingHorizontalOrientationAndNotEnoughSpaceToBottom_ShouldOpenToTop()
		{
			// Arrange.

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var referenceLocation = ControlDpiScalingHelper.NewScaledPoint(0, 1900);
			var referenceSize = ControlDpiScalingHelper.NewScaledSize(100, 100);

			var validationSize = ControlDpiScalingHelper.NewScaledSize(500, 250);

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var referenceControl = new Control { Location = referenceLocation, Size = referenceSize })
			using (var validationControl = new Control { Size = validationSize })
			{
				parentControl.Controls.Add(referenceControl);
				applicationForm.Controls.Add(parentControl);

				// Act.

				validationControl.AdjustLocation(
					applicationForm,
					parentControl,
					referenceControl,
					Orientation.Horizontal);

				// Assert.

				AssertEquals(
					2000 - 250,
					ControlDpiScalingHelper.UnscaleFromCurrentDpiY(validationControl.Location.Y));
			}
		}

		public void TestAdjustLocation_WhenSpaceIsHeigherThanValidationControlHeight_ShouldOpenToBottom()
		{
			// Arrange.

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var referenceLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var referenceSize = ControlDpiScalingHelper.NewScaledSize(100, 100);

			var validationSize = ControlDpiScalingHelper.NewScaledSize(500, 250);

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var referenceControl = new Control { Location = referenceLocation, Size = referenceSize })
			using (var validationControl = new Control { Size = validationSize })
			{
				parentControl.Controls.Add(referenceControl);
				applicationForm.Controls.Add(parentControl);

				// Act.

				validationControl.AdjustLocation(
					applicationForm,
					parentControl,
					referenceControl,
					Orientation.Horizontal,
					validationControl.Height + 10);

				// Assert.

				AssertEquals(
					0,
					ControlDpiScalingHelper.UnscaleFromCurrentDpiY(validationControl.Location.Y));
			}
		}

		public void TestAdjustLocation_WhenSpaceIsLowerThanValidationControlHeight__ShouldOpenToTop()
		{
			// Arrange.

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var referenceLocation = ControlDpiScalingHelper.NewScaledPoint(0, 1900);
			var referenceSize = ControlDpiScalingHelper.NewScaledSize(100, 100);

			var validationSize = ControlDpiScalingHelper.NewScaledSize(500, 250);

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var referenceControl = new Control { Location = referenceLocation, Size = referenceSize })
			using (var validationControl = new Control { Size = validationSize })
			{
				parentControl.Controls.Add(referenceControl);
				applicationForm.Controls.Add(parentControl);

				// Act.

				validationControl.AdjustLocation(
					applicationForm,
					parentControl,
					referenceControl,
					Orientation.Horizontal,
					validationControl.Height - 10);

				// Assert.

				AssertEquals(
					2000 - 250,
					ControlDpiScalingHelper.UnscaleFromCurrentDpiY(validationControl.Location.Y));
			}
		}

		public void TestAdjustLocation_WhenGettingVerticalOrientationAndNotEnoughSpaceToLeft_ShouldOpenToRight()
		{
			// Arrange.

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var referenceLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var referenceSize = ControlDpiScalingHelper.NewScaledSize(100, 100);

			var validationSize = ControlDpiScalingHelper.NewScaledSize(500, 250);

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var referenceControl = new Control { Location = referenceLocation, Size = referenceSize })
			using (var validationControl = new Control { Size = validationSize })
			{
				parentControl.Controls.Add(referenceControl);
				applicationForm.Controls.Add(parentControl);

				// Act.

				validationControl.AdjustLocation(
					applicationForm,
					parentControl,
					referenceControl,
					Orientation.Vertical);

				// Assert.

				AssertEquals(
					0,
					ControlDpiScalingHelper.UnscaleFromCurrentDpiX(validationControl.Location.X));
			}
		}

		public void TestAdjustLocation_WhenGettingVerticalOrientationAndNotEnoughSpaceToRight_ShouldOpenToLeft()
		{
			// Arrange.

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var referenceLocation = ControlDpiScalingHelper.NewScaledPoint(1900, 0);
			var referenceSize = ControlDpiScalingHelper.NewScaledSize(100, 100);

			var validationSize = ControlDpiScalingHelper.NewScaledSize(500, 250);

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var referenceControl = new Control { Location = referenceLocation, Size = referenceSize })
			using (var validationControl = new Control { Size = validationSize })
			{
				parentControl.Controls.Add(referenceControl);
				applicationForm.Controls.Add(parentControl);

				// Act.

				validationControl.AdjustLocation(
					applicationForm,
					parentControl,
					referenceControl,
					Orientation.Vertical);

				// Assert.

				AssertEquals(
					2000 - 500,
					ControlDpiScalingHelper.UnscaleFromCurrentDpiX(validationControl.Location.X));
			}
		}

		public void TestAdjustLocation_WhenGettingVerticalOrientationAndNotEnoughSpaceToTop_ShouldOpenToBottom()
		{
			// Arrange.

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var referenceLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var referenceSize = ControlDpiScalingHelper.NewScaledSize(100, 100);

			var validationSize = ControlDpiScalingHelper.NewScaledSize(500, 250);

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var referenceControl = new Control { Location = referenceLocation, Size = referenceSize })
			using (var validationControl = new Control { Size = validationSize })
			{
				parentControl.Controls.Add(referenceControl);
				applicationForm.Controls.Add(parentControl);

				// Act.

				validationControl.AdjustLocation(
					applicationForm,
					parentControl,
					referenceControl,
					Orientation.Vertical);

				// Assert.

				AssertEquals(
					100 + 1,
					ControlDpiScalingHelper.UnscaleFromCurrentDpiY(validationControl.Location.Y));
			}
		}

		public void TestAdjustLocation_WhenGettingVerticalOrientationAndNotEnoughSpaceToBottom_ShouldOpenToTop()
		{
			// Arrange.

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var referenceLocation = ControlDpiScalingHelper.NewScaledPoint(0, 1900);
			var referenceSize = ControlDpiScalingHelper.NewScaledSize(100, 100);

			var validationSize = ControlDpiScalingHelper.NewScaledSize(500, 250);

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var referenceControl = new Control { Location = referenceLocation, Size = referenceSize })
			using (var validationControl = new Control { Size = validationSize })
			{
				parentControl.Controls.Add(referenceControl);
				applicationForm.Controls.Add(parentControl);

				// Act.

				validationControl.AdjustLocation(
					applicationForm,
					parentControl,
					referenceControl,
					Orientation.Vertical);

				// Assert.

				AssertEquals(
					1900 - 1 - 250,
					ControlDpiScalingHelper.UnscaleFromCurrentDpiY(validationControl.Location.Y));
			}
		}

		public void TestAdjustLocation_LocationShouldBeSameWhenReferenceControlIsTextBoxOrNot()
		{
			// Arrange.

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);
			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(10, 10);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(500, 500);
			var referenceLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var referenceSize = ControlDpiScalingHelper.NewScaledSize(100, 100);
			var validationSize = ControlDpiScalingHelper.NewScaledSize(500, 250);

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var buttonControl = new ZButton { Location = referenceLocation, Size = referenceSize })
			using (var textBoxControl = new ZTextBox { Location = referenceLocation, Size = referenceSize })
			using (var referenceButtonControl = new Control() { Size = validationSize })
			using (var referenceTextBoxControl = new Control() { Size = validationSize })
			{
				parentControl.Controls.Add(buttonControl);
				parentControl.Controls.Add(textBoxControl);
				applicationForm.Controls.Add(parentControl);

				// Act.

				referenceButtonControl.AdjustLocation(
					applicationForm,
					parentControl,
					buttonControl,
					Orientation.Horizontal);

				referenceTextBoxControl.AdjustLocation(
					applicationForm,
					parentControl,
					textBoxControl,
					Orientation.Horizontal);

				// Assert.

				Assert(referenceButtonControl.Location.Equals(referenceTextBoxControl.Location));
			}
		}
	}
}
