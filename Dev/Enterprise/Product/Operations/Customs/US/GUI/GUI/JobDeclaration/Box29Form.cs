using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class Box29Form : ZChildForm
	{
		public Box29Form(Box29Data bo)
			: base(bo) { }

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
