using Enterprise.Customs.NZ.Business.MessageBuilders.ECIWriteOff;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI.Declaration.ECIWriteOff
{
	public partial class SubmitToCustomsForm : Base.SubmitToCustomsForm
	{
		public SubmitToCustomsForm()
			: base()
		{
		}

		public SubmitToCustomsForm(MessageManager messageManager)
			: base(messageManager)
		{
		}

		protected new MessageManager MessageManager
		{
			get { return (MessageManager)base.MessageManager; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (MessageManager != null)
			{
				SetFormDefaultsFromMessageManager();
			}
		}

		void SetFormDefaultsFromMessageManager()
		{
			queueForManifestingCheckBox.Visible = MessageManager.CanQueueForManifesting;
		}
	}
}
