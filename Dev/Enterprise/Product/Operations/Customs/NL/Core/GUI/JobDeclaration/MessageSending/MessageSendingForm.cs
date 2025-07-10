using System.Windows.Forms;
using Enterprise.Customs.NL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI;

public partial class MessageSendingForm : Customs.GUI.MessageSendingFormWithValidationDetails
{
	public MessageSendingForm(JobDeclarationMessageSendingObjectParent declarationWrapper) : base(declarationWrapper)
	{
		SetLayout();

		var dataSource = (JobDeclarationMessageSendingObjectParent)DataSource;
		foreach (JobDeclarationMessageSendingObject sendingObject in dataSource.SendingObjectsCollection)
		{
			sendingObject.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
		}
	}

	void MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
	{
		SetLayout();
	}

	void SetLayout()
	{
		var isEXTMessageType = false;
		if (MessageSendingObjectsGrid.ListManager is CurrencyManager listManager)
		{
			if (listManager.Count > 0)
			{
				var currentRow = (JobDeclarationMessageSendingObject)listManager.GetCurrent();
				if (currentRow.MessageType.ToUpper().Equals(ExportSendMessageTypes.Codes.EXT))
				{
					isEXTMessageType = true;
				}
			}
		}
		if (isEXTMessageType)
		{
			StatementGroupBox.Hide();
			bottomSectionUserControl.Show();
		}
		else
		{
			StatementGroupBox.Show();
			bottomSectionUserControl.Hide();
		}
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
	}

	protected override ZUserControl GetBottomSectionUserControl()
	{
		if (bottomSectionUserControl == null)
		{
			bottomSectionUserControl = new ExportBottomSectionUserControl();
		}
		return bottomSectionUserControl;
	}
	ExportBottomSectionUserControl bottomSectionUserControl;
}
