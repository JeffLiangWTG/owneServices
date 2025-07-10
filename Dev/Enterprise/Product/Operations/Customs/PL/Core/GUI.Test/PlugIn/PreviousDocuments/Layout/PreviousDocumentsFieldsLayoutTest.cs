using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(PreviousDocumentsFieldsLayout))]
sealed class PreviousDocumentsFieldsLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			var euBag = PreviousDocumentsFieldsControlBag.Instance;

			yield return new List<(ControlReference, ControlWidthClass)>
			{
				(euBag.CodeDropEdit, ControlWidthClass.Auto),
				(euBag.ReferenceTextBox, ControlWidthClass.Long),
				(euBag.QuantityCalcDropEdit, ControlWidthClass.Auto),
				(euBag.Quantity2CalcDropEdit, ControlWidthClass.Auto),
				(euBag.PackageQuantityCalcDropEdit, ControlWidthClass.Auto),
			};

			yield return new List<(ControlReference, ControlWidthClass)>
			{
				(euBag.SubTypeDropEdit, ControlWidthClass.Auto),
				(euBag.LineNoCalcEdit, ControlWidthClass.Auto),
				(euBag.Reference2TextBox, ControlWidthClass.Long),
			};
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new PreviousDocumentsFieldsLayoutBuilder();
}
