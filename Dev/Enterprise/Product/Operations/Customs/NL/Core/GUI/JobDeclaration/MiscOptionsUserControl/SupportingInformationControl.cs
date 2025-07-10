using System;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.GUI;

public partial class SupportingInformationControl : EU.GUI.SupportingInformationControl
{
	public SupportingInformationControl()
	{
		InitializeComponent();
		InitFiscalReferencesUserControl();
		FiscalReferencesTabPage.RunWhenBindingOrFirstShown((s, args) => InitFiscalReferencesUserControl());
	}

	void InitFiscalReferencesUserControl()
	{
		FiscalReferencesUserControl.UserControlType = typeof(NLFiscalReferencesUserControl);
		FiscalReferencesUserControl.HostedControlCreated += (sender, args) =>
		{
			var control = FiscalReferencesUserControl.HostedControl as NLFiscalReferencesUserControl;
			if (control != null)
			{
				SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(control, "", SupportingInfoColumnLayoutContext);
				control.ShowHideEntryInstruction(true);
			}
		};
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);

		var declaration = Declaration;
		if (declaration != null)
		{
			declaration.JE_MessageTypeInfo.ValueChanged += DeclarationValuesChanged;
			foreach (var entryInstrucion in Declaration.CustomsEntryInstructions)
			{
				entryInstrucion.CEI_StyleInfo.ValueChanged += DeclarationValuesChanged;
			}
		}
	}

	void DeclarationValuesChanged(object sender, EventArgs e)
	{
		SetTabVisibility();
	}

	void SetTabVisibility()
	{
		var declaration = Declaration;
		if (declaration != null)
		{
			GuaranteesTabPage.TabVisible = declaration.Configuration.MiscGuaranteesSupport(declaration);
		}
	}

	JobDeclaration Declaration => CurrentDataItem as JobDeclaration;

	protected override Type GetSupportingDocumentsUserControlType() => typeof(NLSupportingDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType() => typeof(AdditionalDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(NLPreviousDocumentsUserControl);
}
