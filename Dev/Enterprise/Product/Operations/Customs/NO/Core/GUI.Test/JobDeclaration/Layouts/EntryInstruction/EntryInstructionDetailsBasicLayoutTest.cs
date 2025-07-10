using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsBasicLayout))]
sealed class EntryInstructionDetailsBasicLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
			yield return SecondColumnControls;
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EntryInstructionBasicDetailsControlBag.Instance.StyleDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.DescriptionTextBox, ControlWidthClass.Long);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.SubStyleDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionBasicDetailsControlBag.Instance.CPCDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionDetailsControlBag.Instance.PackageCountCalcEdit, ControlWidthClass.Long);
			yield return (EntryInstructionDetailsControlBag.Instance.GoodsNumberUserControl, ControlWidthClass.Long);
			yield return (EntryInstructionDetailsControlBag.Instance.RequestProcessingDateDateEdit, ControlWidthClass.Auto);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (EntryInstructionDetailsControlBag.Instance.RelatedDeclarationSeparatorUserControl, ControlWidthClass.LongNoCaption);
			yield return (EntryInstructionDetailsControlBag.Instance.SelectedDeclTypeDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionDetailsControlBag.Instance.OriginalDeclarationDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionDetailsControlBag.Instance.CaseCodeDropEdit, ControlWidthClass.Long);
			yield return (EntryInstructionDetailsControlBag.Instance.ReasonTextBox, ControlWidthClass.Long);
			yield return (EntryInstructionDetailsControlBag.Instance.CustomsReplyMessageTextBox, ControlWidthClass.Long);
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();

	public void TestRelatedDeclarationAreaVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			AssertControlIsVisible("RelatedDeclarationSeparator", EntryInstructionDetailsControlBag.Instance.RelatedDeclarationSeparatorUserControl);
			AssertControlIsVisible("SelectedDeclType", EntryInstructionDetailsControlBag.Instance.SelectedDeclTypeDropEdit);
			AssertControlIsVisible("OriginalDeclaration", EntryInstructionDetailsControlBag.Instance.OriginalDeclarationDropEdit);
			AssertControlIsVisible("Reason", EntryInstructionDetailsControlBag.Instance.ReasonTextBox);
			AssertControlIsVisible("CustomsReplyMessage", EntryInstructionDetailsControlBag.Instance.CustomsReplyMessageTextBox);
		});

		void AssertControlIsVisible(string description, ControlReference control)
		{
			var isRelatedDeclarationAreaVisible = () => LayoutForTesting.IsVisible(control, instruction);
			AssertEquals($"(base-case): {description} should NOT be visible", expected: false, isRelatedDeclarationAreaVisible());

			declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.Recalculation;
			AssertEquals($"(is-recalc): {description} should be visible", expected: true, isRelatedDeclarationAreaVisible());

			declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.ReExport;
			AssertEquals($"(is-reexport): {description} should be visible", expected: true, isRelatedDeclarationAreaVisible());

			declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.FinalImport;
			AssertEquals($"(is-final): {description} should be visible", expected: true, isRelatedDeclarationAreaVisible());

			declaration.JE_CopyStatus = ZString.Empty;
			AssertEquals($"(is-not-recalc): {description} should NOT be visible", expected: false, isRelatedDeclarationAreaVisible());
		}
	}

	public void TestCaseCodeVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		var isCaseCodeVisible = () => LayoutForTesting.IsVisible(EntryInstructionDetailsControlBag.Instance.CaseCodeDropEdit, instruction);

		CombineAssertions(() =>
		{
			AssertEquals("(base-case): CaseCode should NOT be visible", expected: false, isCaseCodeVisible());

			declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.Recalculation;
			AssertEquals("(is-recalc): CaseCode should be visible", expected: true, isCaseCodeVisible());

			declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.ReExport;
			AssertEquals("(is-reexport): CaseCode should NOT be visible", expected: false, isCaseCodeVisible());

			declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.FinalImport;
			AssertEquals("(is-final): CaseCode should be NOT visible", expected: false, isCaseCodeVisible());

			declaration.JE_CopyStatus = ZString.Empty;
			AssertEquals("(is-not-recalc): CaseCode should NOT be visible", expected: false, isCaseCodeVisible());
		});
	}

	public void TestRelatedDeclarationSeparatorCaption()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		var allCaptions = new Dictionary<ZString, ZString>()
		{
			{ NODeclarationCopyStatus.Codes.Recalculation, "Recalculation" },
			{ NODeclarationCopyStatus.Codes.ReExport, "Re-Export" },
			{ NODeclarationCopyStatus.Codes.FinalImport, "Standard Import" }
		};

		CombineAssertions(() =>
		{
			foreach (var caption in allCaptions)
			{
				declaration.JE_CopyStatus = caption.Key;
				LayoutForTesting.TryGetCaption(EntryInstructionDetailsControlBag.Instance.RelatedDeclarationSeparatorUserControl, instruction, out var resourceStringData);
				AssertEquals($"{caption.Key} caption", caption.Value, resourceStringData.Caption);
			}
		});
	}
}
