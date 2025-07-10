using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(EntryInstructionBasicDetailsLayoutProvider))]
	sealed class EntryInstructionBasicDetailsLayoutProviderTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (EntryInstructionBasicDetailsControlBag.Instance.DetailsLabel, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.StyleDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.CPCDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.OtherPartiesSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.BondHolderOrganisationControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.NewOwnerOrganisationControl, ControlWidthClass.LongNoCaption);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EntryInstructionBasicDetailsControlBag.Instance.RemoverOrganisationControl, ControlWidthClass.LongNoCaption);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, ControlWidthClass.Auto);
				yield return (EntryInstructionBasicDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Auto);
			}
		}

		protected override int ControlBagCount => 1;
		protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();
	}
}
