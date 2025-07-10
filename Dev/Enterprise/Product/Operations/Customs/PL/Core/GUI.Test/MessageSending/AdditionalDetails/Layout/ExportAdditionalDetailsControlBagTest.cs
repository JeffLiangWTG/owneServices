using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ExportAdditionalDetailsControlBag))]
sealed class ExportAdditionalDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ExportAdditionalDetailsControlBag.SecurityDropEdit);
			yield return nameof(ExportAdditionalDetailsControlBag.AmendmentInvalidationReasonUserControl);
			yield return nameof(ExportAdditionalDetailsControlBag.CorrectionAcceptanceDropEdit);
			yield return nameof(ExportAdditionalDetailsControlBag.AcceptanceCommentUserControl);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ExportAdditionalDetailsControlBag.Instance;
}
