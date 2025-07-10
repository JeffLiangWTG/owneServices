using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.GUI.Testing;

[TestedType(typeof(Phase5ArrivalDeclarationDetailsLayout))]
sealed class Phase5ArrivalDeclarationDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ArrivalDeclarationDetailsLayoutBuilder<NctsHeader>();

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (ArrivalDeclarationDetailsControlBag.Instance.StatusDropEdit, ControlWidthClass.Auto);
			yield return (ArrivalDeclarationDetailsControlBag.Instance.PhaseDropEdit, ControlWidthClass.Auto);
			yield return (ArrivalDeclarationDetailsControlBag.Instance.MessageStatusDropEdit, ControlWidthClass.Auto);
			yield return (ArrivalDeclarationDetailsControlBag.Instance.SeparatorLabel, ControlWidthClass.Auto);
			yield return (ArrivalDeclarationDetailsControlBag.Instance.ReleaseDateEdit, ControlWidthClass.Auto);
			yield return (ArrivalDeclarationDetailsControlBag.Instance.AcceptanceDateEdit, ControlWidthClass.Auto);
		}
	}
}
