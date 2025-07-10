using Enterprise.MasterFiles.Business.TemporaryOrgRemover;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TemporaryOrgRemoverErrorsForm : ZChildForm
	{
		public TemporaryOrgRemoverErrorsForm(Remover removerObject) : base(removerObject)
		{
		}

		public override string FormHeading
		{
			get { return FormCaption; }
		}
	}
}
