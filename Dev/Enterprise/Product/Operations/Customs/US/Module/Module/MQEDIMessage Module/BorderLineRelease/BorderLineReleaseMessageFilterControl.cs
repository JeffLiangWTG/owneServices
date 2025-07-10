using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.Module
{
	public partial class BorderLineReleaseMessageFilterControl : MQEDIMessageFilterControl
	{
		public BorderLineReleaseMessageFilterControl(BorderLineReleaseMessageCollection gridCollection, BorderLineReleaseMessageFilterStripBusinessObject filterStripBusinessObject, string[] columnNamesToHide)
			: base(gridCollection, filterStripBusinessObject, columnNamesToHide)
		{
			InitializeComponent();
		}

		protected override void BindCore()
		{
			using (FilteredGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				FilteredGrid.ReOrderColumns(ColumnsOrder);
			}
			base.BindCore();
		}

		string[] ColumnsOrder
		{
			get
			{
				if (columnsOrder == null)
				{
					columnsOrder = new string[]
					{
						MQEDIMessage.Schema.MessageStatusToShowInQueryModule,
						MQEDIMessage.Schema.EM_ActionStatus,
						EDIMessage.Schema.EM_ApplicationReference,
						LineReleaseMQEDIMessage.Schema.US_ImporterNumber,
						LineReleaseMQEDIMessage.Schema.US_PortCode,
						LineReleaseMQEDIMessage.Schema.LinkedDeclarationReference,
						LineReleaseMQEDIMessage.Schema.US_ReleaseDateTime,
						EDIMessage.Schema.EM_GB,
						EDIMessage.Schema.EM_SystemCreateTimeUtc,
						EDIMessage.Schema.EM_MessageText,
					};
				}
				return columnsOrder;
			}
		}
		string[] columnsOrder;
	}
}
