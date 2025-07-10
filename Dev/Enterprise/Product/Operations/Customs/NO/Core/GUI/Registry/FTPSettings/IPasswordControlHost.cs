using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

public interface IPasswordControlHost
{
	ZButton ViewButton { get; }
	ZTextBox PasswordTextBox { get; }
}
