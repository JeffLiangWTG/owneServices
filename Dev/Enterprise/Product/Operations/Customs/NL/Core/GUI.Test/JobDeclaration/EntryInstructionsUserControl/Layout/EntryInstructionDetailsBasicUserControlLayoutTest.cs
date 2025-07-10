using System.Collections.Generic;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.NL.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsBasicUserControlLayout))]
sealed class EntryInstructionDetailsBasicUserControlLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	protected override int ControlBagCount => 3;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.StyleDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.CPCDropEdit, ControlWidthClass.Long);
			yield return (EU.GUI.EntryInstructionBasicDetailsControlBag.Instance.LocationOfGoodsUserControl, ControlWidthClass.Long);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.IsHighValueOvrdCheckBox, ControlWidthClass.Medium);
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.OtherPartiesSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseLabel, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.EntryInstructionBasicDetailsControlBag.Instance.ToWarehouseUserControl, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseLabel, ControlWidthClass.LongNoCaption);
			yield return (EU.GUI.EntryInstructionBasicDetailsControlBag.Instance.FromWarehouseUserControl, ControlWidthClass.LongNoCaption);
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.BondHolderOrganisationControl, ControlWidthClass.LongNoCaption);
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.NewOwnerOrganisationControl, ControlWidthClass.LongNoCaption);
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.TransNatureDropEdit, ControlWidthClass.Long);
			yield return (Customs.GUI.EntryInstructionBasicDetailsControlBag.Instance.RemoverOrganisationControl, ControlWidthClass.LongNoCaption);
		}
	}
}
