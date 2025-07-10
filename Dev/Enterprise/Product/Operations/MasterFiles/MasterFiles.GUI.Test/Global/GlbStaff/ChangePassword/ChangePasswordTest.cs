#if !WINZOR
using System.Windows;
using System.Windows.Controls;
using CargoWise.Main.Navigation.WPF.Test;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test;

[TestedType(typeof(ChangePassword))]
[SuppressDataContextCheck]
class ChangePasswordTest : WPFControlBasherTest
{
	protected override Control GetControlToBashCore()
	{
		var mockViewModel = new Mock<IChangePasswordViewModel>();
		return new ChangePassword(mockViewModel.Object);
	}

	[GuiTest]
	[RequiresSTA]
	public void TestUserName()
	{
		// Arrange
		var viewModel = new Mock<IChangePasswordViewModel>();
		var userName = "John Due";
		viewModel.SetupGet(m => m.UserName).Returns(userName);

		var view = new ChangePassword(viewModel.Object);

		try
		{
			// Act
			view.Show();
			var userNameTextBox = (TextBox)view.FindName("UserNameTextBox");

			// Assert
			AssertEquals("Check User Name", userName, userNameTextBox.Text);
		}
		finally
		{
			view.Close();
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestOldPassword()
	{
		// Arrange
		var viewModel = new Mock<IChangePasswordViewModel>();
		viewModel.SetupProperty(m => m.OldPassword);
		var view = new ChangePassword(viewModel.Object);

		try
		{
			// Act
			view.Show();
			AssertNull("Check Old Password", viewModel.Object.OldPassword);

			var input = (PasswordBox)view.FindName("OldPasswordInput");
			input.Password = "abc";

			// Assert
			AssertNotNull("Check Old Password", viewModel.Object.OldPassword);
		}
		finally
		{
			view.Close();
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestNewPassword()
	{
		// Arrange
		var viewModel = new Mock<IChangePasswordViewModel>();
		viewModel.SetupProperty(m => m.NewPassword);
		var view = new ChangePassword(viewModel.Object);

		try
		{
			// Act
			view.Show();
			AssertNull("Check New Password", viewModel.Object.NewPassword);

			var input = (PasswordBox)view.FindName("NewPasswordInput");
			input.Password = "abc";

			// Assert
			AssertNotNull("Check New Password", viewModel.Object.NewPassword);
		}
		finally
		{
			view.Close();
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestNewPasswordConfirm()
	{
		// Arrange
		var viewModel = new Mock<IChangePasswordViewModel>();
		viewModel.SetupProperty(m => m.NewPasswordConfirm);
		var view = new ChangePassword(viewModel.Object);

		try
		{
			// Act
			view.Show();
			AssertNull("Check New Password Confirm", viewModel.Object.NewPasswordConfirm);

			var input = (PasswordBox)view.FindName("NewPasswordConfirmInput");
			input.Password = "abc";

			// Assert
			AssertNotNull("Check New Password Confirm", viewModel.Object.NewPasswordConfirm);
		}
		finally
		{
			view.Close();
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestErrorBlock()
	{
		// Arrange
		var viewModel = new Mock<IChangePasswordViewModel>();
		var errorMessage = "This is a very critical error message!";
		viewModel.SetupGet(m => m.ErrorMessage).Returns(errorMessage);

		var view = new ChangePassword(viewModel.Object);

		try
		{
			// Act
			view.Show();
			var control = (TextBlock)view.FindName("ErrorTextBlock");

			// Assert
			AssertEquals("Check Error Message", errorMessage, control.Text);
		}
		finally
		{
			view.Close();
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestCancel()
	{
		// Arrange
		var count = 0;
		var viewModel = new Mock<IChangePasswordViewModel>();
		viewModel.Setup(m => m.Cancel()).Callback(() => count++).Returns(false);
		var view = new ChangePassword(viewModel.Object);

		try
		{
			// Act
			view.Show();
			var control = (Button)view.FindName("CancelButton");
			control.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

			// Assert
			AssertEquals("Check Cancel call", 1, count);
		}
		finally
		{
			view.Close();
		}
	}

	[GuiTest]
	[RequiresSTA]
	public void TestApply()
	{
		// Arrange
		var count = 0;
		var viewModel = new Mock<IChangePasswordViewModel>();
		viewModel.Setup(m => m.Apply()).Callback(() => count++).Returns(false);
		var view = new ChangePassword(viewModel.Object);

		try
		{
			// Act
			view.Show();
			var control = (Button)view.FindName("ApplyButton");
			control.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

			// Assert
			AssertEquals("Check Apply call", 1, count);
		}
		finally
		{
			view.Close();
		}
	}
}
#endif
