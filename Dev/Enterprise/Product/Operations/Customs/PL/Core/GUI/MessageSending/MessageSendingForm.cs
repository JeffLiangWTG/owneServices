using CargoWise.Windows.UI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class MessageSendingForm : MessageSendingObjectForm
{
	public MessageSendingForm()
	{
	}

	public MessageSendingForm(BaseMessageSendingObjectParent messageParent)
		: base(messageParent)
	{
	}

	const string IsEnabledForBindingString = "IsEnabledForBinding";

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
		InitializeNewColumns();

		AddDynamicLayoutUserControl();
	}

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);
		SendButton.DataBindings.RemoveBinding(IsEnabledForBindingString);

		if (dataSource != null)
		{
			SendButton.DataBindings.Add(new KBinding(IsEnabledForBindingString, BindingSource.DataSource, nameof(BaseMessageSendingObjectParent.SendButtonEnabled)));
		}
	}

	void InitializeNewColumns()
	{
		MessageSendingObjectsGrid.ApplyGridColumnLayout(MessageSendingGridColumnLayoutProvider);
	}

	public new BaseMessageSendingObjectParent BusinessEntity => (BaseMessageSendingObjectParent)base.BusinessEntity;

	internal IPanelLayoutProvider GetNewAdditionalDetailsUserControlPanelLayoutProvider() => (BusinessEntity?.IsExport ?? false)
		? new ExportAdditionalDetailsLayout()
		: new ImportAdditionalDetailsLayout();

	void AddDynamicLayoutUserControl()
	{
		AdditionalDetailsUserControl.UpdateLayout(PanelLayoutProvider);
	}

	IPanelLayoutProvider PanelLayoutProvider => panelLayoutProvider ?? (panelLayoutProvider = GetNewAdditionalDetailsUserControlPanelLayoutProvider());
	IPanelLayoutProvider panelLayoutProvider;

	IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => new MessageSendingGridColumnLayout();
}
