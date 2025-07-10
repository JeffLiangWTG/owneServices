using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterData.GUI
{
	[CodeAlive("This class is under development")]
	public partial class PersonMergeConfirmationForm : ZChildForm
	{
		public PersonMergeConfirmationForm()
		{
			InitializeComponent();
		}

		public override string FormCaption => Res.GetString("E7779128-EA05-45A8-B1CB-9DBD0212C029", "Confirm Person Merge");
	}
}
