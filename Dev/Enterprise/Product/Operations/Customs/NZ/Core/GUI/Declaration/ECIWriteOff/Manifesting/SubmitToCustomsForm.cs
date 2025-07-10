using Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff.Manifesting;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff.Manifesting
{
	public partial class SubmitToCustomsForm : ECIWriteOff.SubmitToCustomsForm
	{
		public SubmitToCustomsForm()
			: base()
		{
		}

		public SubmitToCustomsForm(MessageManager messageManager)
			: base(messageManager)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
