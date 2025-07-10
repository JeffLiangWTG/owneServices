using System.ComponentModel;
using Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry;

namespace Enterprise.Customs.NZ.GUI.Declaration.FormalEntry
{
	public partial class SubmitToCustomsForm : Base.SubmitToCustomsForm
	{
		public SubmitToCustomsForm(MessageManager messageManager)
			: base(messageManager)
		{
			mergeMonitorSuspender = new Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(messageManager.EntryHeader.Declaration);
		}
		Customs.Business.MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge mergeMonitorSuspender;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected new MessageManager MessageManager
		{
			get { return (MessageManager)base.MessageManager; }
		}

		#region Dispose

		readonly Container components;
		protected override void Dispose(bool disposing)
		{
			if (mergeMonitorSuspender != null)
			{
				mergeMonitorSuspender.Dispose();
				mergeMonitorSuspender = null;
			}
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
