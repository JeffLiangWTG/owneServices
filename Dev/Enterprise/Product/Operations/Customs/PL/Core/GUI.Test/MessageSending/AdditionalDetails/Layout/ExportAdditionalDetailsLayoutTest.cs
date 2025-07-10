using System.Collections.Generic;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ExportAdditionalDetailsLayout))]
sealed class ExportAdditionalDetailsLayoutTest : LayoutsAbstractTest
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
			yield return (ExportAdditionalDetailsControlBag.Instance.SecurityDropEdit, ControlWidthClass.Auto);
			yield return (ExportAdditionalDetailsControlBag.Instance.AmendmentInvalidationReasonUserControl, ControlWidthClass.Auto);
			yield return (ExportAdditionalDetailsControlBag.Instance.CorrectionAcceptanceDropEdit, ControlWidthClass.Auto);
			yield return (ExportAdditionalDetailsControlBag.Instance.AcceptanceCommentUserControl, ControlWidthClass.Auto);
		}
	}

	protected override int ControlBagCount => 1;

	protected override ICommonLayoutBuilder CommonLayoutBuilder => new ExportAdditionalDetailsLayoutBuilder<BaseMessageSendingObject>();
}
