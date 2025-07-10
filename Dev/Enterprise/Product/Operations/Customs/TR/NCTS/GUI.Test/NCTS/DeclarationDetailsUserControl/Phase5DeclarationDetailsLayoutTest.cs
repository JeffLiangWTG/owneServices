using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5DeclarationDetailsLayout))]
	sealed class Phase5DeclarationDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new DeclarationDetailsLayoutBuilder<Business.NctsDepartureMovementHeader>();

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
				yield return (EU.NCTS.GUI.DeclarationDetailsControlBag.Instance.MrnTextBox, ControlWidthClass.Long);
				yield return (DeclarationDetailsControlBag.Instance.LrnTextBox, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DeclarationDetailsControlBag.Instance.DepartureStatusDropEdit, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DeclarationDetailsControlBag.Instance.PhaseStatusDropEdit, ControlWidthClass.Long);
				yield return (EU.NCTS.GUI.DeclarationDetailsControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (EU.NCTS.GUI.DeclarationDetailsControlBag.Instance.ReleaseDateEdit, ControlWidthClass.Auto);
				yield return (EU.NCTS.GUI.DeclarationDetailsControlBag.Instance.AcceptanceDateEdit, ControlWidthClass.Auto);
			}
		}
	}
}
