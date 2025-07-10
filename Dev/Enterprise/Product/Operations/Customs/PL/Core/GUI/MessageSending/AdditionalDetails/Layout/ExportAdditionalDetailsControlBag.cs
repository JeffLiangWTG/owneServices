using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class ExportAdditionalDetailsControlBag : ControlBag
{
	public ExportAdditionalDetailsControlBag()
	{
		SecurityDropEdit = RegisterControl(nameof(ExportAdditionalDetailsUserControl.SecurityDropEdit));
		AmendmentInvalidationReasonUserControl = RegisterControl(nameof(ExportAdditionalDetailsUserControl.AmendmentInvalidationReasonUserControl));
		CorrectionAcceptanceDropEdit = RegisterControl(nameof(ExportAdditionalDetailsUserControl.CorrectionAcceptanceDropEdit));
		AcceptanceCommentUserControl = RegisterControl(nameof(ExportAdditionalDetailsUserControl.AcceptanceCommentUserControl));
	}

	public static ExportAdditionalDetailsControlBag Instance => instance ?? (instance = new ExportAdditionalDetailsControlBag());

	[ThreadStatic]
	static ExportAdditionalDetailsControlBag instance;

	protected override Control CreateTemplate() => new ExportAdditionalDetailsUserControl();

	public ControlReference SecurityDropEdit { get; }
	public ControlReference AmendmentInvalidationReasonUserControl { get; }
	public ControlReference CorrectionAcceptanceDropEdit { get; }
	public ControlReference AcceptanceCommentUserControl { get; }
}
