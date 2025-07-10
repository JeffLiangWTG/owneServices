using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsControlBag))]
sealed class EntryInstructionDetailsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(EntryInstructionDetailsControlBag.GoodsNumberUserControl);
			yield return nameof(EntryInstructionDetailsControlBag.PackageCountCalcEdit);
			yield return nameof(EntryInstructionDetailsControlBag.RelatedDeclarationSeparatorUserControl);
			yield return nameof(EntryInstructionDetailsControlBag.CustomsReplyMessageTextBox);
			yield return nameof(EntryInstructionDetailsControlBag.ReasonTextBox);
			yield return nameof(EntryInstructionDetailsControlBag.CaseCodeDropEdit);
			yield return nameof(EntryInstructionDetailsControlBag.OriginalDeclarationDropEdit);
			yield return nameof(EntryInstructionDetailsControlBag.SelectedDeclTypeDropEdit);
			yield return nameof(EntryInstructionDetailsControlBag.RequestProcessingDateDateEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => EntryInstructionDetailsControlBag.Instance;
}
