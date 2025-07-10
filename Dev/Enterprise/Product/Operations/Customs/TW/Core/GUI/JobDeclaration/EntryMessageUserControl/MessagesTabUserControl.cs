using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.TW.GUI
{
	public partial class MessagesTabUserControl : Customs.GUI.BaseMessagesTabUserControl
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected void InitializeGridLayout()
		{
			using (MessagesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				MessagesGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
				{
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = TWMessage.Schema.EM_Calc_MessageTypeCode,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104)
					},
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = TWMessage.Schema.EM_Calc_MessageTypeDescription,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				});
			}

			ResetGridDefaultOrderAndVisibleColumns(MessagesGrid, messagesBoundGridGridDefaultOrderColumns, messagesBoundGridDefaultVisibleColumns);
		}

		void ResetGridDefaultOrderAndVisibleColumns(ZGrid grid, string[] orderColumns, string[] visibleColumns)
		{
			using (grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				grid.ReOrderColumns(orderColumns);
				grid.SetAllColumnsVisible(false);
				grid.SetColumnVisible(true, visibleColumns);
			}
		}

		readonly string[] messagesBoundGridGridDefaultOrderColumns = new string[]
		{
			TWMessage.Schema.EM_Calc_MessageTypeDescription,
			TWMessage.Schema.EM_Calc_MessageTypeCode,
			TWMessage.Schema.EM_MessageType,
			TWMessage.Schema.EM_ReceiveTransmit,
			TWMessage.Schema.EM_InterchangeNumber,
			TWMessage.Schema.EM_DateTimeInterchangeSent,
			TWMessage.Schema.EM_User,
			TWMessage.Schema.EM_ApplicationReference,
			TWMessage.Schema.EM_SystemCreateTimeUtc,
			TWMessage.Schema.InterchangeeHubID,
			TWMessage.Schema.EM_InterchangeStatus,
			TWMessage.Schema.EM_MessageNum,
			TWMessage.Schema.EM_MessageDateTime,
			TWMessage.Schema.EM_Status,
			TWMessage.Schema.EM_MessageSubType
		};

		readonly string[] messagesBoundGridDefaultVisibleColumns = new string[]
		{
			TWMessage.Schema.EM_Calc_MessageTypeDescription,
			TWMessage.Schema.EM_Calc_MessageTypeCode,
			TWMessage.Schema.EM_MessageType,
			TWMessage.Schema.EM_ReceiveTransmit,
			TWMessage.Schema.EM_InterchangeNumber,
			TWMessage.Schema.EM_DateTimeInterchangeSent,
			TWMessage.Schema.EM_User
		};
	}
}
