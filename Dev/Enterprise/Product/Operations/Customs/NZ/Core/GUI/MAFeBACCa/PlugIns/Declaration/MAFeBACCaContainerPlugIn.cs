using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa
{
	public sealed class MAFeBACCaContainerPlugIn : BaseMAFeBACCaDeclarationPlugIn // TODO: , Enterprise.Freight.GUI.IAddColumnsToGrid
	{
		public MAFeBACCaContainerPlugIn(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override System.Windows.Forms.Control GetNewUserControl()
		{
			return new MAFeBACCaContainerUserControl(declaration);
		}
	}
}
