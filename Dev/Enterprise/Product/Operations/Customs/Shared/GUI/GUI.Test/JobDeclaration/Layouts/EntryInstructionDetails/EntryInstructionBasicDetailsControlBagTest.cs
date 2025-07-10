using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(EntryInstructionBasicDetailsControlBag))]
	sealed class EntryInstructionBasicDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override ControlBag GetControlBagForTesting() => EntryInstructionBasicDetailsControlBag.Instance;
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EntryInstructionBasicDetailsControlBag.StyleDropEdit);
				yield return nameof(EntryInstructionBasicDetailsControlBag.SubStyleDropEdit);
				yield return nameof(EntryInstructionBasicDetailsControlBag.CPCDropEdit);
				yield return nameof(EntryInstructionBasicDetailsControlBag.DescriptionTextBox);
				yield return nameof(EntryInstructionBasicDetailsControlBag.AssessmentDateEdit);
				yield return nameof(EntryInstructionBasicDetailsControlBag.DetailsLabel);
				yield return nameof(EntryInstructionBasicDetailsControlBag.OtherPartiesSeparatorUserControl);
				yield return nameof(EntryInstructionBasicDetailsControlBag.BondHolderOrganisationControl);
				yield return nameof(EntryInstructionBasicDetailsControlBag.NewOwnerOrganisationControl);
				yield return nameof(EntryInstructionBasicDetailsControlBag.RemoverOrganisationControl);
			}
		}
	}
}
