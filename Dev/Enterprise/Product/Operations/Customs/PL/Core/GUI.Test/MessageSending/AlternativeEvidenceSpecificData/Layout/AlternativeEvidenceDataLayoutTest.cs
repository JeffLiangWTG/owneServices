using System.Collections.Generic;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(AlternativeEvidenceDataLayout))]
sealed class CC583SpecificDataLayoutTest : LayoutsAbstractTest
{
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
			yield return (CC583SpecificDataControlBag.Instance.EnquiryInformationCodeDropEdit, ControlWidthClass.Auto);
			yield return (CC583SpecificDataControlBag.Instance.OfficeOfExitCodeFindBox, ControlWidthClass.Auto);
			yield return (CC583SpecificDataControlBag.Instance.ExitDateDateTimeOffsetEdit, ControlWidthClass.Auto);
		}
	}

	static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
	{
		get
		{
			yield return (CC583SpecificDataControlBag.Instance.AlternativeEvidenceGrid, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new AlternativeEvidenceDataLayoutBuilder<BaseMessageSendingObjectParent>();
}
