using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public abstract partial class UNDGSubstanceBaseForm : ZTemplateForm
	{
		protected UNDGSubstanceBaseForm(IBusiness specificSubstance) : base(specificSubstance)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;

		protected override bool AllowNew => false;

		abstract protected ZUserControl GetNewUNDGSubstanceUserControl();
	}
}
