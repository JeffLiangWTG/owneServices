using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.eManifest.Messaging;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class MessageSendingForm : MessageSendingObjectForm
	{
		public MessageSendingForm()
		{
		}

		public MessageSendingForm(BaseMessageSendingObjectParent sendingObjectParent)
		: base(sendingObjectParent)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeNewColumns();
		}

		void InitializeNewColumns()
		{
			var descriptionTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			descriptionTextBoxColumnStyleInfo.ColumnName = AutoeManifestMessageSendingObject.Schema.MessageDescription;
			descriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			MessageSendingObjectsGrid.ColumnStyles.Insert(0, descriptionTextBoxColumnStyleInfo);
			foreach (ZGridColumnInfo column in MessageSendingObjectsGrid.ColumnStyles)
			{
				column.IsSortable = false;
			}
		}
	}
}
