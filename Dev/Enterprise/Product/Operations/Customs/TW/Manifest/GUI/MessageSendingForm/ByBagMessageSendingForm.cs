using Enterprise.Customs.TW.Manifest.Business;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public partial class ByBagMessageSendingForm : MessageSendingForm
	{
		public ByBagMessageSendingForm(MessageSendingObjectParent headerWrapper)
		: base(headerWrapper)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeColumns();
		}

		void InitializeColumns()
		{
			var bagNumberTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			bagNumberTextBoxColumnStyleInfo.ColumnName = MessageSendingObject.Schema.BagNumber;
			bagNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			bagNumberTextBoxColumnStyleInfo.IsMandatory = true;
			MessageSendingObjectsGrid.ColumnStyles.Add(bagNumberTextBoxColumnStyleInfo);
		}
	}
}
