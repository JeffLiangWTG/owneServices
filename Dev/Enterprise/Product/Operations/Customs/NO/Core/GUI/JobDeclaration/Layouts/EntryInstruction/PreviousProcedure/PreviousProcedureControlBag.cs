using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NO.GUI;

sealed class PreviousProcedureControlBag : ControlBag
{
	public static PreviousProcedureControlBag Instance => instance ??= new ();

	[ThreadSafe]
	static PreviousProcedureControlBag instance;

	PreviousProcedureControlBag()
	{
		PreviousProcedureDropEdit = RegisterControl(nameof(PreviousProcedureBasicUserControl.PreviousProcedureDropEdit));
		ImportFromTemporaryStorageRegisterButton = RegisterControl(nameof(PreviousProcedureBasicUserControl.ImportFromTemporaryStorageRegisterButton));
	}

	protected override Control CreateTemplate() => new PreviousProcedureBasicUserControl();

	public ControlReference PreviousProcedureDropEdit { get; }
	public ControlReference ImportFromTemporaryStorageRegisterButton { get; }
}
