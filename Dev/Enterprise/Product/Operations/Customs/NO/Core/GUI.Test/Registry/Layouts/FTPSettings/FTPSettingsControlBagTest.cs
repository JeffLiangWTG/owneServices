using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(FTPSettingsControlBag))]
sealed class FTPSettingsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(FTPSettingsControlBag.Instance.UserNameTextBox);
			yield return nameof(FTPSettingsControlBag.Instance.PasswordTextBox);
			yield return nameof(FTPSettingsControlBag.Instance.ViewButton);
			yield return nameof(FTPSettingsControlBag.Instance.UrlAddressTextBox);
			yield return nameof(FTPSettingsControlBag.Instance.PortTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => FTPSettingsControlBag.Instance;
}
