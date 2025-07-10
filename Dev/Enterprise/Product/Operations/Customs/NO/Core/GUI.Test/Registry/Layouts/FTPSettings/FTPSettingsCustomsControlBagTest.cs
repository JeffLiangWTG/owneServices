using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(FTPSettingsCustomsControlBag))]
sealed class FTPSettingsCustomsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(FTPSettingsCustomsControlBag.Instance.SendToCustomFolderTextBox);
			yield return nameof(FTPSettingsCustomsControlBag.Instance.ReceiveFromCustomFolderTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => FTPSettingsCustomsControlBag.Instance;
}
