using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public class CC583SpecificDataControlBag : ControlBag
{
	public CC583SpecificDataControlBag()
	{
		AlternativeEvidenceGrid = RegisterControl(nameof(AlternativeEvidenceControl.AlternativeEvidenceGrid));
		OfficeOfExitCodeFindBox = RegisterControl(nameof(AlternativeEvidenceControl.OfficeOfExitCodeFindBox));
		EnquiryInformationCodeDropEdit = RegisterControl(nameof(AlternativeEvidenceControl.EnquiryInformationCodeDropEdit));
		ExitDateDateTimeOffsetEdit = RegisterControl(nameof(AlternativeEvidenceControl.ExitDateDateTimeOffsetEdit));
	}

	public static CC583SpecificDataControlBag Instance => instance ??= new CC583SpecificDataControlBag();

	[ThreadStatic]
	static CC583SpecificDataControlBag instance;

	protected override Control CreateTemplate() => new AlternativeEvidenceControl();

	public ControlReference AlternativeEvidenceGrid { get; }
	public ControlReference OfficeOfExitCodeFindBox { get; }
	public ControlReference EnquiryInformationCodeDropEdit { get; }
	public ControlReference ExitDateDateTimeOffsetEdit { get; }
}
