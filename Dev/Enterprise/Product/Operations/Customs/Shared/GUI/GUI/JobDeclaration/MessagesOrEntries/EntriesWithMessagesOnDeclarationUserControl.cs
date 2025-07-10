using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	public partial class EntriesWithMessagesOnDeclarationUserControl : CustomsEntryAndDiscardedMessagesUserControl
	{
		public EntriesWithMessagesOnDeclarationUserControl()
			: this(null)
		{
		}

		public EntriesWithMessagesOnDeclarationUserControl(BaseJobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new EntriesAndEntryLinesUserControl();
		}
	}
}
