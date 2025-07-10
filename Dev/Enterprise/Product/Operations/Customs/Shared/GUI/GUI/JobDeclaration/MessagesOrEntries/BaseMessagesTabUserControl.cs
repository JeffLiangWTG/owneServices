using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class BaseMessagesTabUserControl : ZUserControl
	{
		public BaseMessagesTabUserControl()
		{
			InitializeComponent();
			MessagesGrid.ReadOnly = true;

			var queryInterchangeCreator = new QueryInterchangeCreator(MessagesGrid);
			queryInterchangeCreator.AddColumnAndMenuForQuery();
		}
	}
}
