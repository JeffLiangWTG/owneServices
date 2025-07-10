using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa
{
	public sealed class MAFeBACCaInvoiceLinePlugIn : BaseMAFeBACCaDeclarationPlugIn
	{
		public MAFeBACCaInvoiceLinePlugIn(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override System.Windows.Forms.Control GetNewUserControl()
		{
			return new MAFeBACCaInvoiceLineUserControl(declaration);
		}
	}
}
