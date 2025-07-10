#if !WINZOR
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	/// <summary>
	/// Interaction logic for ChangePassword.xaml
	/// </summary>
	public partial class ChangePassword : Window
	{
		public static Window ChangeDialog(GlbStaff staff, nint owner)
		{
			var window = new ChangePassword(new ChangePasswordViewModel(staff, requireOldPassword: true));

			var interopHelper = new WindowInteropHelper(window);
			interopHelper.Owner = owner;

			return window;
		}
		public static Window ResetDialog(GlbStaff staff, nint owner)
		{
			var window = new ChangePassword(new ChangePasswordViewModel(staff, requireOldPassword: false));

			var interopHelper = new WindowInteropHelper(window);
			interopHelper.Owner = owner;

			return window;
		}

		internal ChangePassword(IChangePasswordViewModel viewModel)
		{
			InitializeComponent();
			ViewModel = viewModel;
			DataContext = ViewModel;

			if (ViewModel.RequireOldPassword)
			{
				OldPasswordInput.Focus();
			}
			else
			{
				NewPasswordInput.Focus();
			}
		}

		readonly IChangePasswordViewModel ViewModel;

		void OldPassword_PasswordChanged(object sender, EventArgs e)
		{
			ViewModel.OldPassword = ((PasswordBox)sender).SecurePassword;
		}

		void NewPassword_PasswordChanged(object sender, EventArgs e)
		{
			ViewModel.NewPassword = ((PasswordBox)sender).SecurePassword;
		}

		void NewPasswordConfirm_PasswordChanged(object sender, EventArgs e)
		{
			ViewModel.NewPasswordConfirm = ((PasswordBox)sender).SecurePassword;
		}

		void ApplyButton_Click(object sender, EventArgs e)
		{
			if (ViewModel.Apply())
			{
				DialogResult = true;
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			if (ViewModel.Cancel())
			{
				DialogResult = false;
			}
		}

		void OldPasswordInput_GotFocus(object sender, RoutedEventArgs e)
		{
			Dispatcher.BeginInvoke(() => OldPasswordInput.SelectAll());
		}

		void NewPasswordInput_GotFocus(object sender, RoutedEventArgs e)
		{
			Dispatcher.BeginInvoke(() => NewPasswordInput.SelectAll());
		}

		void NewPasswordConfirmInput_GotFocus(object sender, RoutedEventArgs e)
		{
			Dispatcher.BeginInvoke(() => NewPasswordConfirmInput.SelectAll());
		}
	}
}
#endif
