using System.ComponentModel;
using System.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI;

public interface IChangePasswordViewModel : INotifyPropertyChanged
{
	public MultilingualString Title { get; }
	public MultilingualString UserNameLabel { get; }
	public string UserName { get; }
	public MultilingualString OldPasswordLabel { get; }
	public SecureString OldPassword { get; set; }
	public bool RequireOldPassword { get; }
	public MultilingualString NewPasswordLabel { get; }
	public SecureString NewPassword { get; set; }
	public MultilingualString NewPasswordConfirmLabel { get; }
	public SecureString NewPasswordConfirm { get; set; }
	public MultilingualString CancelCaption { get; }
	public MultilingualString ApplyCaption { get; }
	public string ErrorMessage { get; }
	public bool Apply();
	public bool Cancel();
}
