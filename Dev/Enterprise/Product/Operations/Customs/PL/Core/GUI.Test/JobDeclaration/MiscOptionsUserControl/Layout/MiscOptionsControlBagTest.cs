using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(MiscOptionsControlBag))]
sealed class MiscOptionsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(MiscOptionsControlBag.SupportingInformationUserControl);
			yield return nameof(MiscOptionsControlBag.ExciseZDropEdit);
			yield return nameof(MiscOptionsControlBag.ExciseZDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => MiscOptionsControlBag.Instance;
}
