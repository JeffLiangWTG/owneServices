using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(CC583SpecificDataControlBag))]
sealed class CC583SpecificDataControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(CC583SpecificDataControlBag.AlternativeEvidenceGrid);
			yield return nameof(CC583SpecificDataControlBag.OfficeOfExitCodeFindBox);
			yield return nameof(CC583SpecificDataControlBag.EnquiryInformationCodeDropEdit);
			yield return nameof(CC583SpecificDataControlBag.ExitDateDateTimeOffsetEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => CC583SpecificDataControlBag.Instance;
}
