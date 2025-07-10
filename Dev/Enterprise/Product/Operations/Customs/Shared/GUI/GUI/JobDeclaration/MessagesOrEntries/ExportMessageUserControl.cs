using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class ExportMessageUserControl : BaseCustomsEntryUserControl
	{
		public ExportMessageUserControl()
			: this(null)
		{
		}

		public ExportMessageUserControl(BaseJobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
			MessagesGrid.ReadOnly = true;

			var queryInterchangeCreator = new QueryInterchangeCreator(MessagesGrid);
			queryInterchangeCreator.AddColumnAndMenuForQuery();

			RequiresMergeLabel.AllowOverlap(MessageTextPanel);
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
