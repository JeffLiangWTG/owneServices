using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

public partial class CustomsDeclarationSendingForm : MessageSendingForm
{
	public CustomsDeclarationSendingForm(CustomsDeclarationMessageSendingObjectParent messageParent)
		: base(messageParent)
	{
	}

	const string IsVisibleForBindingString = nameof(IIsVisibleForBindingControl.IsVisibleForBinding);

	protected override void InitialiseForm()
	{
		base.InitialiseForm();
		InitializeComponent();
	}

	public override void SetDataBinding(object dataSource, string dataMember)
	{
		base.SetDataBinding(dataSource, dataMember);
		SpecificDataUserControl.DataBindings.RemoveBinding(IsVisibleForBindingString);

		if (DataSource != null)
		{
			SpecificDataUserControl.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, nameof(CustomsDeclarationMessageSendingObjectParent.HasEvidences)));
			SpecificDataUserControl.UpdateLayout(PanelLayoutProvider);
		}
	}

	IPanelLayoutProvider PanelLayoutProvider => panelLayoutProvider ??= new AlternativeEvidenceDataLayout();
	IPanelLayoutProvider panelLayoutProvider;

	protected override bool CheckIsOKToSend()
	{
		return base.CheckIsOKToSend() && (BusinessEntity.AnySelectedSendingObjects || Globals.Message.Show(
			Res.GetString("PLMessageSendingForm|CheckIsOKToSend|NoDeclaration", "No declaration data will be sent."),
			Res.GetString("PLMessageSendingForm|CheckIsOKToSend|ShouldSendNoDeclaration", "Send eDocs only? "), MessageBoxButtons.YesNo,
			MessageBoxIcon.Question) == DialogResult.Yes);
	}

	public new CustomsDeclarationMessageSendingObjectParent BusinessEntity => (CustomsDeclarationMessageSendingObjectParent)base.BusinessEntity;
}
