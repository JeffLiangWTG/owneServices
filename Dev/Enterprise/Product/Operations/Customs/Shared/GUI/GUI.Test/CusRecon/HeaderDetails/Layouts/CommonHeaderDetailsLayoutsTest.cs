using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CommonHeaderDetailsLayouts))]
	sealed class CommonHeaderDetailsLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonHeaderDetailsLayoutBuilder<CusReconDeclaration>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonHeaderDetailsControlBag.Instance.EntryTypeDropEdit, ControlWidthClass.Long);
				yield return (CommonHeaderDetailsControlBag.Instance.EntryStatusTextBox, ControlWidthClass.Auto);
				yield return (CommonHeaderDetailsControlBag.Instance.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
				yield return (CommonHeaderDetailsControlBag.Instance.PeriodFromDateEdit, ControlWidthClass.Auto);
				yield return (CommonHeaderDetailsControlBag.Instance.PeriodToDateEdit, ControlWidthClass.Auto);
				yield return (CommonHeaderDetailsControlBag.Instance.AuthorizationNumberGuidDropEdit, ControlWidthClass.Long);
			}
		}
	}
}
