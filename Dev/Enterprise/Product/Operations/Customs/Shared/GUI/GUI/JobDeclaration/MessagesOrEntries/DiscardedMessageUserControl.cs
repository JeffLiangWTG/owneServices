using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	public partial class DiscardedMessageUserControl : BaseCustomsEntryUserControl
	{
		public DiscardedMessageUserControl()
			: this(null)
		{
		}

		public DiscardedMessageUserControl(BaseJobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
			MessageGrid.ReadOnly = true;
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

