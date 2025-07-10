using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefCurrencyForm : ZForm
	{
		public RefCurrencyForm()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		public RefCurrencyForm(RefCurrency currency) : base(currency)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			PlugIns.Add(ControllerIDs.Audit);
		}

		void LanguageDropButton_Click(object sender, EventArgs e)
		{
			new RefLanguageTextTranslationForm(new RefLanguageTextPage(RefCurrencySchema.Constants.Prefix, AutoRefCurrency.Schema.RX_Desc, DescriptionTextBox.Text, (BusinessObject)BusinessEntity, Res.GetString("c9f1fab0-73f8-4036-bdf6-ca48b1c51697", "Currency"), new string[] { RefCurrencySchema.Constants.RX_Desc })).Show();
		}

		void SubUnitLanguageDropButton_Click(object sender, EventArgs e)
		{
			new RefLanguageTextTranslationForm(new RefLanguageTextPage(RefCurrencySchema.Constants.Prefix, AutoRefCurrency.Schema.RX_SubUnitName, SubUnitTextBox.Text, (BusinessObject)BusinessEntity, Res.GetString("64d8dd7e-3d3d-451f-ba9e-6da638bbf3c8", "Currency"), new string[] { RefCurrencySchema.Constants.RX_SubUnitName })).Show();
		}

		void UnitNameDropButton_Click(object sender, EventArgs e)
		{
			new RefLanguageTextTranslationForm(new RefLanguageTextPage(RefCurrencySchema.Constants.Prefix, AutoRefCurrency.Schema.RX_UnitName, UnitNameTextBox.Text, (BusinessObject)BusinessEntity, Res.GetString("8c1689de-5a45-4763-82a7-08df33cabb69", "Currency"), new string[] { RefCurrencySchema.Constants.RX_UnitName })).Show();
		}
	}
}
