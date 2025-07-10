using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(EntryInstructionSupportingDocumentsFieldsLayout))]
sealed class EntryInstructionSupportingDocumentsFieldsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			var euBag = SupportingDocumentFieldsControlBag.Instance;

			yield return new List<(ControlReference, ControlWidthClass)>
			{
				(euBag.CodeCodeFindBox, ControlWidthClass.Auto),
				(euBag.ReferenceNumberTextBox, ControlWidthClass.Auto),
				(euBag.DocumentLineNoCalcEdit, ControlWidthClass.Auto),
				(euBag.AdditionalDescriptionTextBox, ControlWidthClass.Auto),
				(euBag.DateOfExpiryDateEdit, ControlWidthClass.Auto),
			};
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new SupportingDocumentFieldsLayoutBuilder<SupportingDocument>();
}
