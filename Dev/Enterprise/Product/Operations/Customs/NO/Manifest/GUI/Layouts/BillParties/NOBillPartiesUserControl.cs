using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI
{
	sealed partial class NOBillPartiesUserControl : ZUserControl
	{
		public NOBillPartiesUserControl()
		{
			InitializeComponent();
#if DEBUG
			System.ComponentModel.TypeDescriptor.AddAttributes(RepresentativeStateDropEdit, new SuppressFormsLocalizedTestAttribute());
#endif
		}
	}
}
