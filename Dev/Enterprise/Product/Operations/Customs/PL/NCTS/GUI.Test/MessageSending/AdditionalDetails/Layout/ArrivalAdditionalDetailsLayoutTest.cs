using System.Collections.Generic;
using Enterprise.Customs.PL.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing;

[TestedType(typeof(ArrivalAdditionalDetailsLayout))]
sealed class ArrivalAdditionalDetailsLayoutTest : LayoutsAbstractTest
{
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
			yield return (ArrivalAdditionalDetailsControlBag.Instance.TirPageNumberDropEdit, ControlWidthClass.Auto);
			yield return (ArrivalAdditionalDetailsControlBag.Instance.TirUnloadingNumberDropEdit, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ArrivalAdditionalDetailsLayoutBuilder<MessageSendingObject>();
}
