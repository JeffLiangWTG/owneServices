using System.Linq;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.GUI;

public partial class RetrospectiveQuotaRequestSendingForm : MessageSendingForm
{
	public RetrospectiveQuotaRequestSendingForm(RetrospectiveQuotaRequestMessageSendingObjectParent messageParent)
		: base(messageParent)
	{
	}

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);

		if (dataSource is RetrospectiveQuotaRequestMessageSendingObjectParent parent)
		{
			AddActionListeners(parent);
			SendingObjectsCollectionChanged();
		}

		if (EntryLinesTabPage.TabVisible)
		{
			AdditionalDataTabControl.SelectTab(EntryLinesTabPage);
		}
	}

	void AddActionListeners(RetrospectiveQuotaRequestMessageSendingObjectParent parent)
	{
		foreach (var businessObject in parent.SendingObjectsCollection.Cast<RetrospectiveQuotaRequestMessageSendingObject>())
		{
			businessObject.ActionInfo.ValueChanged += SendingObjectsCollectionChanged;
		}
	}

	void RemoveActionListeners(RetrospectiveQuotaRequestMessageSendingObjectParent parent)
	{
		foreach (var businessObject in parent.SendingObjectsCollection.Cast<RetrospectiveQuotaRequestMessageSendingObject>())
		{
			businessObject.ActionInfo.ValueChanged -= SendingObjectsCollectionChanged;
		}
	}

	void SendingObjectsCollectionChanged(object sender, System.EventArgs e)
	{
		SendingObjectsCollectionChanged();
	}

	void SendingObjectsCollectionChanged()
	{
		EntryLinesTabPage.TabRelevant = BusinessEntity.AnyZCX05;
	}

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
	}

	protected override void Dispose(bool disposing)
	{
		RemoveActionListeners(BusinessEntity);
		if (disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	public new RetrospectiveQuotaRequestMessageSendingObjectParent BusinessEntity => (RetrospectiveQuotaRequestMessageSendingObjectParent)base.BusinessEntity;
}
