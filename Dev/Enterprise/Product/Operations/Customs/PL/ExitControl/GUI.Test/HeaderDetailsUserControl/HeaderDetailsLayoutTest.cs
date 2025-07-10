using System.Collections.Generic;
using Enterprise.Customs.PL.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.GUI.Testing;

[TestedType(typeof(HeaderDetailsLayout))]
sealed class HeaderDetailsLayoutTest : LayoutsAbstractTest
{
	protected override int ControlBagCount => 2;

	protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
	{
		get
		{
			yield return FirstColumnControls;
		}
	}

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new EU.ExitControl.GUI.HeaderDetailsLayoutBuilder<CusExitHeader>();

	IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
	{
		get
		{
			yield return (EU.ExitControl.GUI.HeaderDetailsControlBag.Instance.BranchGuidFindBox, ControlWidthClass.Long);
			yield return (EU.ExitControl.GUI.HeaderDetailsControlBag.Instance.ExporterOrgAddressControl, ControlWidthClass.Long);
			yield return (EU.ExitControl.GUI.HeaderDetailsControlBag.Instance.CarrierAddressWithContactControl, ControlWidthClass.Auto);
			yield return (HeaderDetailsControlBag.Instance.BrokerCodeFindBox, ControlWidthClass.Auto);
			yield return (HeaderDetailsControlBag.Instance.CertificateDropEdit, ControlWidthClass.Auto);
			yield return (HeaderDetailsControlBag.Instance.TrainingCheckBox, ControlWidthClass.Long);
			yield return (HeaderDetailsControlBag.Instance.StoringFlagCheckBox, ControlWidthClass.Long);
		}
	}

	public void TestControlVisibility()
	{
		var header = Factory.New<CusExitHeader>();
		AssertEquals("BrokerCodeFindBox not visible", false, LayoutForTesting.IsVisible(HeaderDetailsControlBag.Instance.BrokerCodeFindBox, header));
		AssertEquals("CertificateDropEdit not visible", false, LayoutForTesting.IsVisible(HeaderDetailsControlBag.Instance.CertificateDropEdit, header));
		AssertEquals("TrainingCheckBox not visible", false, LayoutForTesting.IsVisible(HeaderDetailsControlBag.Instance.TrainingCheckBox, header));
		AssertEquals("StoringFlagCheckBox is visible", true, LayoutForTesting.IsVisible(HeaderDetailsControlBag.Instance.StoringFlagCheckBox, header));
	}
}
