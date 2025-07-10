using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(SupportingDocumentFieldsLayout))]
sealed class SupportingDocumentFieldsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			var euBag = EU.GUI.PlugIn.SupportingDocumentFieldsControlBag.Instance;
			var plBag = PlugIn.SupportingDocumentFieldsControlBag.Instance;

			yield return new List<(ControlReference, ControlWidthClass)>
			{
				(euBag.CodeCodeFindBox, ControlWidthClass.Long),
				(euBag.ReferenceNumberTextBox, ControlWidthClass.Auto),
				(plBag.SupDocReference2TextBox, ControlWidthClass.Auto),
				(plBag.SupDocDescriptionTextBox, ControlWidthClass.Auto),
				(euBag.DocumentLineNoCalcEdit, ControlWidthClass.Auto),
				(euBag.AdditionalDescriptionTextBox, ControlWidthClass.Auto),
				(euBag.DateOfIssueDateEdit, ControlWidthClass.Auto),
				(euBag.DateOfExpiryDateEdit, ControlWidthClass.Auto),
				(euBag.QuantityCalcDropEdit, ControlWidthClass.Auto),
				(plBag.ValueConvertToLocalCurrencyControl, ControlWidthClass.Auto),
			};
		}
	}

	protected override int ControlBagCount => 2;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new SupportingDocumentFieldsLayoutBuilder<SupportingDocument>();
}
