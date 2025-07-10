using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.MessageManagers;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Customs.TW.GUI
{
	public partial class AdditionalDocumentForm : MessageSendingForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public AdditionalDocumentForm(AdditionalDocumentMessageSendingObjectParent declarationWrapper) : base(declarationWrapper)
		{
		}

		protected override bool CheckIsOKToSend()
		{
			if (IsMessageTypeADM)
			{
				var selectedSendingObjects = BusinessEntity?.SelectedSendingObjects.Cast<AdditionalDocumentMessageSendingObject>();
				if (selectedSendingObjects != null && selectedSendingObjects.Any(o => o.SupportingDocuments.Count == 0))
				{
					Globals.Message.Show(ResString.GetMultilingualString("9C853FCF-73B6-4D41-A15D-5FE49731436D", "You have not selected any documents from the eDocs."),
						MessageManager.Constants.CannotSendMessageCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
			}
			return base.CheckIsOKToSend();
		}

		ZBool IsMessageTypeADM => BusinessEntity != null && BusinessEntity.MessageType == MessageTypeList.Codes.ADM;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			InitializeNewColumns();
		}

		void InitializeNewColumns()
		{
			var isMessageTypeADM = IsMessageTypeADM;
			if (isMessageTypeADM)
			{
				var contactOfficeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
				contactOfficeDropEditColumnStyleInfo.ColumnName = AdditionalDocumentMessageSendingObject.TWSchema.ContactOffice;
				contactOfficeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				MessageSendingObjectsGrid.ColumnStyles.Add(contactOfficeDropEditColumnStyleInfo);
			}

			MessageSendingObjectsGrid.SetAvailability(!isMessageTypeADM, MessageSendingObject.TWSchema.Action);
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control.Name == "ContinueToSendCheckBox" && previousControl.Name == "CancelButton2") || (control.Name == "CancelButton2" && previousControl.Name == "ContinueToSendCheckBox");
		}
	}
}
