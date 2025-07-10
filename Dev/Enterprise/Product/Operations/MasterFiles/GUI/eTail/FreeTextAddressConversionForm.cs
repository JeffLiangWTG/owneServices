using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI;

public partial class FreeTextAddressConversionForm<T> : ZChildForm where T : BusinessObject, Customs.IConsignmentAddressProvider
{
	public FreeTextAddressConversionForm(FreeTextAddressConversion<T> conversion, bool ignoreButtonVisible = true, bool enablePostingButtons = true, IGridColumnLayoutProvider freeTextAddressGridLayoutProvider = null) : base(conversion)
	{
		IgnoreButtonVisible = ignoreButtonVisible;
		EnablePostingButtons = enablePostingButtons;
		InitializeComponent();
		InitializeGridColumns(freeTextAddressGridLayoutProvider);
		SetupPostingButtons(enablePostingButtons);
		conversion.OnFinished += Conversion_OnFinished;
	}

	void InitializeGridColumns(IGridColumnLayoutProvider freeTextAddressGridLayoutProvider)
	{
		if (freeTextAddressGridLayoutProvider != null)
		{
			FreeTextAddressGrid.ApplyGridColumnLayout(freeTextAddressGridLayoutProvider);
		}
	}

	protected override bool AllowNew => false;

	public override string FormVerb => ZString.Empty;

	FreeTextAddress<T> SelectedFreeTextAddress => FreeTextAddressGrid.ListManager.GetCurrent() as FreeTextAddress<T>;

	OrgPatternMatch SelectedSimilarAddress => SimilarOrgsDisplayGrid.ListManager.GetCurrent() as OrgPatternMatch;

	void SetupPostingButtons(bool enablePostingButtons)
	{
		if (enablePostingButtons)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtons);
		}
		else
		{
			PostingButtons.CloseButton.Click += (_, _) => Close();
		}
	}

	void Conversion_OnFinished(object sender, EventArgs e)
	{
		Close();
	}

	void IgnoreButton_Click(object sender, EventArgs e)
	{
		SelectedFreeTextAddress?.Ignore();
	}

	void SelectButton_Click(object sender, EventArgs e)
	{
		if (SelectedSimilarAddress is { } selectedSimilarAddress)
		{
			SelectedFreeTextAddress?.SelectAddress(selectedSimilarAddress.Address.PK);
		}
	}

	void CreateNewOrgButton_Click(object sender, EventArgs e)
	{
		if (SelectedFreeTextAddress == null)
		{
			Globals.Message.ShowError(Res.GetString("5f0026c4-0474-495b-acbf-5a437c0e8558", "There is no address that needs to be used to create the organization."));
		}
		else
		{
			var newOrg = SelectedFreeTextAddress.CreateNewOrganizationByCurrentAddressInfo();
			var controller = ZControllerFactory.Create(ControllerIDs.Organisation);
			controller.ShowChildrenAsDialog = true;
			controller.ShowFormForNewEntity(newOrg);

			if (newOrg.IsInDatabase)
			{
				SelectedFreeTextAddress?.SelectAddress(newOrg.MainAddress.PK);
			}
			else
			{
				newOrg.Delete();
			}
		}
	}

	bool IgnoreButtonVisible { get; }
	bool EnablePostingButtons { get; }
}
