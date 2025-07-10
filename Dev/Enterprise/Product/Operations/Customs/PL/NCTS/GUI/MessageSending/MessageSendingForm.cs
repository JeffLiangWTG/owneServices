using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NctsHeader = Enterprise.Customs.PL.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.PL.NCTS.GUI;

public partial class MessageSendingForm : EU.NCTS.GUI.MessageSendingForm
{
	[Obsolete("Do not call. Only for designer use.")]
	public MessageSendingForm()
	{
	}

	public MessageSendingForm(NctsHeaderMessageSendingObjectParent sendingObjectWrapper) : base(sendingObjectWrapper)
	{
		InitializeComponent();

		AddDynamicLayoutUserControl();
	}

	public IPanelLayoutProvider GetNewAdditionalDetailsUserControlPanelLayoutProvider() => (NctsHeader?.IsArrivalMovement ?? false)
		? new ArrivalAdditionalDetailsLayout()
		: new DepartureAdditionalDetailsLayout();

	protected override void AddUserControlToBottomSection()
	{
		var bottomSectionUserControl = GetBottomSectionUserControl();
		bottomSectionUserControl.Dock = DockStyle.Top;
		WarningSplitContainer.Panel2.Controls.Add(bottomSectionUserControl);

		BindingSource.SetBindingMember(AdditionalDetailsUserControl, "SendingObjectsCollection");
		AdditionalDetailsUserControl.Dock = DockStyle.Fill;
		WarningSplitContainer.Panel2.Controls.Add(AdditionalDetailsUserControl);
		AdditionalDetailsUserControl.BringToFront();
	}

	void AddDynamicLayoutUserControl()
	{
		AdditionalDetailsUserControl.UpdateLayout(PanelLayoutProvider);
	}

	IPanelLayoutProvider PanelLayoutProvider => panelLayoutProvider ?? (panelLayoutProvider = GetNewAdditionalDetailsUserControlPanelLayoutProvider());
	IPanelLayoutProvider panelLayoutProvider;

	AdditionalDetailsUserControl AdditionalDetailsUserControl => additionalDetailsUserControl ?? (additionalDetailsUserControl = new AdditionalDetailsUserControl());

	AdditionalDetailsUserControl additionalDetailsUserControl;

	new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;
}
