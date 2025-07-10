using System.Collections.Generic;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(DeclarationDetailsLayouts))]
sealed class DeclarationDetailsLayoutsTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

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
			yield return (CommonBag.DeclarationNumberTextBox, ControlWidthClass.Long);
			yield return (CommonBag.StatusTextBox, ControlWidthClass.Long);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (DeclarationDetailsControlBag.Instance.PhaseStatusTextBox, ControlWidthClass.Long);
			yield return (DeclarationDetailsControlBag.Instance.MessageStatusTextBox, ControlWidthClass.Long);
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new DeclarationDetailsLayoutBuilder();

	static CommonDeclarationDetailsControlBag CommonBag => CommonDeclarationDetailsControlBag.Instance;
}
