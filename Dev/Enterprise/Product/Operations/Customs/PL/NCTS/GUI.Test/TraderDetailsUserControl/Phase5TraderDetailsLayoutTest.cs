using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.GUI.Testing.TraderDetailsUserControl;

[TestedType(typeof(Phase5TraderDetailsLayout))]
sealed class Phase5TraderDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 1;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new TraderDetailsLayoutBuilder<EU.NCTS.Business.NctsHeader>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (TraderDetailsControlBag.Instance.PrincipalDocAddressControl, ControlWidthClass.LongControl);
			yield return (TraderDetailsControlBag.Instance.ConsignorDocAddressControl, ControlWidthClass.LongControl);
			yield return (TraderDetailsControlBag.Instance.ConsigneeDocAddressControl, ControlWidthClass.LongControl);
			yield return (TraderDetailsControlBag.Instance.RepresentativeDocAddressControl, ControlWidthClass.LongControl);
		}
	}

	public void TestAddControlBehaviour()
	{
		AssertEquals("ZDocAddressControl", true, LayoutForTesting.HasBehaviourByBehaviourType(TraderDetailsControlBag.Instance.PrincipalDocAddressControl, typeof(CompactDisplayModeBehaviour)));
		AssertEquals("ZDocAddressControl", true, LayoutForTesting.HasBehaviourByBehaviourType(TraderDetailsControlBag.Instance.ConsignorDocAddressControl, typeof(CompactDisplayModeBehaviour)));
		AssertEquals("ZDocAddressControl", true, LayoutForTesting.HasBehaviourByBehaviourType(TraderDetailsControlBag.Instance.ConsigneeDocAddressControl, typeof(CompactDisplayModeBehaviour)));
	}
}
