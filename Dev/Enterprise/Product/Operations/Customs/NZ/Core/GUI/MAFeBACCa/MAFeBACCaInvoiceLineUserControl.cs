using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa
{
	public partial class MAFeBACCaInvoiceLineUserControl : ZUserControl
	{
		public MAFeBACCaInvoiceLineUserControl()
		{
			InitializeComponent();
		}

		public MAFeBACCaInvoiceLineUserControl(JobDeclaration declaration)
		{
			this.Declaration = declaration;
			InitializeComponent();
		}

		public JobDeclaration Declaration
		{
			get;
			private set;
		}
	}
}
