using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	class MiscOptionsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new MiscOptionsLayoutUserControl();
		public static MiscOptionsControlBag Instance => instance ??= new MiscOptionsControlBag();

		[ThreadStatic]
		static MiscOptionsControlBag instance;

		public MiscOptionsControlBag()
		{
			RelatedDeclarationsUserControl = RegisterControl(nameof(MiscOptionsLayoutUserControl.RelatedDeclarationsUserControl));
		}

		public ControlReference RelatedDeclarationsUserControl { get; }
	}
}
