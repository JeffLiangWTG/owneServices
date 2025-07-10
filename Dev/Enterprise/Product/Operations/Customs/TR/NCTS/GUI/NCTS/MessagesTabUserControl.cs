using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public partial class MessagesTabUserControl : EU.NCTS.GUI.MessagesTabUserControl
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();
			SetupMessageColumns();
		}

		void SetupMessageColumns()
		{
			MessageGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = EM_CreateUserFullName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			});

			MessageGrid.ReOrderColumns(MessagesGridSortOrder);
		}

		string[] MessagesGridSortOrder
		{
			get
			{
				if (messagesGridSortOrder == null)
				{
					messagesGridSortOrder = new[]
					{
						EDIMessage.Schema.EM_MessageNum,
						EDIMessage.Schema.EM_MessageDateTime,
						EDIMessage.Schema.EM_MessageType,
						EDIMessage.Schema.EM_MessageSubType,
						EDIMessage.Schema.EM_ReceiveTransmit,
						EDIMessage.Schema.EM_SystemCreateUser,
						EM_CreateUserFullName,
						EDIMessage.Schema.EM_SystemCreateTimeUtc,
						EDIMessage.Schema.EM_InterchangeNumber,
						EDIMessage.Schema.EM_InterchangeStatus,
						EDIMessage.Schema.EM_ApplicationReference,
					};
				}
				return messagesGridSortOrder;
			}
		}
		string[] messagesGridSortOrder;

		const string EM_CreateUserFullName = "EM_CreateUserFullName";
	}
}
