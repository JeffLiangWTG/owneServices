using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class DuplicationMergeModeUserControl : ZUserControl
	{
		readonly DuplicationModelDetailCollection modelDetailCollection;

		public DuplicationMergeModeUserControl(DuplicationModelDetailCollection modelDetailCollection)
		{
			InitializeComponent();

			this.modelDetailCollection = modelDetailCollection;
			SetDataBinding(this.modelDetailCollection, string.Empty);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (modelDetailCollection is null)
			{
				return;
			}

			base.SetDataBinding(modelDetailCollection, string.Empty);
		}
	}
}
