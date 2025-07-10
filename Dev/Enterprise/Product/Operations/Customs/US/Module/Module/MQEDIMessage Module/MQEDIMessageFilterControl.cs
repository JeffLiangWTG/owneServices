using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Messaging.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Module
{
	public partial class MQEDIMessageFilterControl : EDIMessageFilterControl
	{
		public MQEDIMessageFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject, string[] columnNamesToHide)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			this.columnNamesToHide = columnNamesToHide;
		}

		readonly string[] columnNamesToHide;

		protected override void BindCore()
		{
			using (FilteredGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				FilteredGrid.RemoveFromAvailableColumns(columnNamesToHide);
				FilteredGrid.ReOrderColumns(ColumnsOrder);
				FilteredGrid.SetColumnWidth(MQEDIMessage.Schema.EM_MessageTextShort, 300);
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
						MQEDIMessage.Schema.EM_MessageText,
						MQEDIMessage.Schema.EM_SystemCreateTimeUtc
					};
				}
				return columnsOrder;
			}
		}
		string[] columnsOrder;
	}
}
