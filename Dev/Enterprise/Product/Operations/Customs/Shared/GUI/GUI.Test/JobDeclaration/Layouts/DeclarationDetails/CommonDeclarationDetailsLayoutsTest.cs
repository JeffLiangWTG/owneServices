using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonDeclarationDetailsLayouts))]
	sealed class CommonDeclarationDetailsLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference controlReference, ControlWidthClass controlWidth)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
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

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonDeclarationDetailsLayoutBuilder<BaseJobDeclaration>();

		static CommonDeclarationDetailsControlBag CommonBag => CommonDeclarationDetailsControlBag.Instance;
	}
}
