using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public sealed class USExportManifestControlBag : ControlBag
	{
		public static USExportManifestControlBag Instance => usExportManifestControlBag.Value;

		USExportManifestControlBag()
		{
			DeparturePortUNLOCOCodeFindBox = RegisterControl(nameof(USExportManifestUserControl.DeparturePortUNLOCOCodeFindBox));
			ScheduleDCodeFindBox = RegisterControl(nameof(USExportManifestUserControl.ScheduleDCodeFindBox));
			IssuerSCACUserControl = RegisterControl(nameof(USExportManifestUserControl.IssuerSCACUserControl));
		}

		public ControlReference DeparturePortUNLOCOCodeFindBox { get; }
		public ControlReference ScheduleDCodeFindBox { get; }
		public ControlReference IssuerSCACUserControl { get; }

		protected override Control CreateTemplate() => new USExportManifestUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<USExportManifestControlBag> usExportManifestControlBag = new Lazy<USExportManifestControlBag>(() => new USExportManifestControlBag());
	}
}
