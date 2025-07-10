using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(PreviousProcedureControlBag))]
sealed class PreviousProcedureControlBagTest : ControlBagAbstractTest
{
	protected override ControlBag GetControlBagForTesting() => PreviousProcedureControlBag.Instance;

	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(PreviousProcedureControlBag.PreviousProcedureDropEdit);
			yield return nameof(PreviousProcedureControlBag.ImportFromTemporaryStorageRegisterButton);
		}
	}
}
