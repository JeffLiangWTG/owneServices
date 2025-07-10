using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ImportAdditionalDetailsControlBag))]
sealed class ImportAdditionalDetailsControlBagTest : ControlBagAbstractTest
{
	public new void TestControlNames() => Assert("this test should be removed and base test should be executed, if there exists at least 1 RegisteredControl", true);

	public new void TestControlRegistered() => Assert("this test should be removed and base test should be executed, if there exists at least 1 RegisteredControl", true);

	protected override IEnumerable<string> RegisteredControlNames
	{
		get => Array.Empty<string>();
	}

	protected override ControlBag GetControlBagForTesting() => ImportAdditionalDetailsControlBag.Instance;
}
