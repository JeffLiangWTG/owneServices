using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	partial class PreviousProcedureUserControl : ZUserControl
	{
		public PreviousProcedureUserControl()
		{
			InitializeComponent();
			PreviousProceduresGrid.ApplyGridColumnLayout(PreviousProcedureGridLayout);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource is JobDeclaration declaration && declaration != currentItem)
			{
				currentItem = declaration;
				PreviousProcedureHeaderDynamicPanel.UpdateLayout(PreviousProcedureHeaderLayout);
			}
		}

		protected IGridColumnLayoutProvider PreviousProcedureGridLayout => gridColumnLayoutProvider ??= new PreviousProcedureGridLayout();
		IGridColumnLayoutProvider gridColumnLayoutProvider;

		protected IPanelLayoutProvider PreviousProcedureHeaderLayout => headerLayout ??= new PreviousProcedureHeaderLayout();
		IPanelLayoutProvider headerLayout;

		JobDeclaration currentItem;
	}
}
