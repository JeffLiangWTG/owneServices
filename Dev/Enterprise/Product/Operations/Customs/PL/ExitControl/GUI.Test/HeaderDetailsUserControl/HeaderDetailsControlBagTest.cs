using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.GUI.Testing;

[TestedType(typeof(HeaderDetailsControlBag))]
sealed class HeaderDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(HeaderDetailsControlBag.BrokerCodeFindBox);
			yield return nameof(HeaderDetailsControlBag.CertificateDropEdit);
			yield return nameof(HeaderDetailsControlBag.TrainingCheckBox);
			yield return nameof(HeaderDetailsControlBag.StoringFlagCheckBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => HeaderDetailsControlBag.Instance;
}
