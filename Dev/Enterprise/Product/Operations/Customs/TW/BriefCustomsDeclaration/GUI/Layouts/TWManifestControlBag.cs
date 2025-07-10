using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public class TWManifestControlBag : ControlBag
	{
		public static TWManifestControlBag Instance => manifestControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<TWManifestControlBag> manifestControlBag = new Lazy<TWManifestControlBag>(() => new TWManifestControlBag());

		protected override Control CreateTemplate() => new TWManifestLayoutUserControl();

		TWManifestControlBag()
		{
			PersonGuidFindBox = RegisterControl(nameof(TWManifestLayoutUserControl.PersonGuidFindBox));
			CustomsAgentCodeFindBox = RegisterControl(nameof(TWManifestLayoutUserControl.CustomsAgentCodeFindBox));
			ImporterAddressUserControl = RegisterControl(nameof(TWManifestLayoutUserControl.ImporterAddressUserControl));
			ExporterAddressUserControl = RegisterControl(nameof(TWManifestLayoutUserControl.ExporterAddressUserControl));
			DeclarationDateEdit = RegisterControl(nameof(TWManifestLayoutUserControl.DeclarationDateEdit));
			BagNumberTextBox = RegisterControl(nameof(TWManifestLayoutUserControl.BagNumberTextBox));
			EntryNumberUserControl = RegisterControl(nameof(TWManifestLayoutUserControl.EntryNumberUserControl));
		}

		public ControlReference CustomsAgentCodeFindBox { get; }
		public ControlReference ImporterAddressUserControl { get; }
		public ControlReference ExporterAddressUserControl { get; }
		public ControlReference PersonGuidFindBox { get; }
		public ControlReference DeclarationDateEdit { get; }
		public ControlReference BagNumberTextBox { get; }
		public ControlReference EntryNumberUserControl { get; }
	}
}
