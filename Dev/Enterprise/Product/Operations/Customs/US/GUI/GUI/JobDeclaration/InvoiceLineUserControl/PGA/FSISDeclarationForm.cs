using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class FSISDeclarationForm : ZChildForm
	{
		public FSISDeclarationForm(JobDeclaration declaration, ZString certificateNumber)
			: base(declaration)
		{
			this.certificateNumber = certificateNumber;
			InitializeComponent();

			FSISCertificatesGrid.ColourDeciding += FSISCertificatesGrid_ColourDeciding;
		}
		readonly ZString certificateNumber;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				FSISCertificatesGrid.ColourDeciding -= FSISCertificatesGrid_ColourDeciding;
			}
			base.Dispose(disposing);
		}

		void FSISCertificatesGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			var fsisLine = e.ObjectAtRow as USDeclarationFSISLine;
			if (fsisLine != null)
			{
				if (fsisLine.US_HealthCertificateNumber == certificateNumber)
				{
					e.Colour = System.Drawing.Color.LightGreen;
				}
			}
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
