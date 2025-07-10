using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	public partial class MessageUserControl : BaseCustomsEntryUserControl
	{
		public MessageUserControl()
			: this(null)
		{
		}

		public MessageUserControl(BaseJobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
			MessagesGrid.ReadOnly = true;

			var queryInterchangeCreator = new QueryInterchangeCreator(MessagesGrid);
			queryInterchangeCreator.AddColumnAndMenuForQuery();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
