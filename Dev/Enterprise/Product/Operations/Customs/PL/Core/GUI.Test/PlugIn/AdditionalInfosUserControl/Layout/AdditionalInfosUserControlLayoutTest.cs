using System.Collections.Generic;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(AdditionalInfosUserControlLayout))]
sealed class AdditionalInfosUserControlLayoutTest : LayoutsAbstractTest
{
	protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
	{
		get
		{
			var euBag = AdditionalInformationDetailsControlBag.Instance;

			yield return new List<(ControlReference, ControlWidthClass)>
			{
				(euBag.KindDropEdit, ControlWidthClass.Auto),
				(euBag.FullTypeCodeFindBox, ControlWidthClass.Auto),
				(euBag.ReferenceTextBox, ControlWidthClass.Auto),
				(euBag.DescriptionTextBox, ControlWidthClass.Auto),
			};
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new AdditionalInformationDetailsLayoutBuilder();
}
