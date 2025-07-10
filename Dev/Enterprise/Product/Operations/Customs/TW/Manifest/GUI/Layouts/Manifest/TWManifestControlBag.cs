using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public sealed class TWManifestControlBag : ControlBag
	{
		public static TWManifestControlBag Instance => maniFestControlBag.Value;

		TWManifestControlBag()
		{
			GoodsLocationCodeFindBox = RegisterControl(nameof(TWManifestUserControl.GoodsLocationCodeFindBox));
			LoginCompanyGuidFindBox = RegisterControl(nameof(TWManifestUserControl.LoginCompanyGuidFindBox));
			MailBoxTextBox = RegisterControl(nameof(TWManifestUserControl.MailBoxTextBox));
			DeconsolidateVATTextBox = RegisterControl(nameof(TWManifestUserControl.DeconsolidateVATTextBox));
			MessageStatusDropEdit = RegisterControl(nameof(TWManifestUserControl.MessageStatusDropEdit));
		}

		public ControlReference GoodsLocationCodeFindBox { get; }

		public ControlReference LoginCompanyGuidFindBox { get; }

		public ControlReference MailBoxTextBox { get; }

		public ControlReference DeconsolidateVATTextBox { get; }

		public ControlReference MessageStatusDropEdit { get; }

		protected override Control CreateTemplate() => new TWManifestUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<TWManifestControlBag> maniFestControlBag = new Lazy<TWManifestControlBag>(() => new TWManifestControlBag());
	}
}
