using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefExchangeRateForm : ZForm
	{
		[Obsolete("This constructor is just for the designer")]
		public RefExchangeRateForm()
		{
			InitializeComponent();
		}

		public RefExchangeRateForm(RefExchangeRate exRate)
			: base(exRate)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		ZToolStripButton ToolStripSaveAndNewButton;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			RE_AsPublishedTextBox.Visible = true;
			Update_RE_SellRateCalcEdit_DecimalFormat();
			RefExchangeRate.RE_RX_NKExCurrencyInfo.ValueChanged += RE_RX_NKExCurrencyInfo_ValueChanged;
			ToolStripSaveAndNewButton = PostingButtonsUserControl.InsertAdditionalButton(
					"SaveAndNewButton",
				Res.GetString("2a8dc093-d51a-45ea-bd5b-d11066cc3c95", "Sav&e && New"),
				Icons.GetImage(IconTypes.SaveNewButtonRest), 2);
			ToolStripSaveAndNewButton.Click += SaveAndNewButton_Click;
		}

		void RE_RX_NKExCurrencyInfo_ValueChanged(object sender, EventArgs e) => Update_RE_SellRateCalcEdit_DecimalFormat();

		void Update_RE_SellRateCalcEdit_DecimalFormat()
		{
			var exRateHelper = GetDecimalPlacesFromCountry(RefExchangeRate.Company.Country.Code);
			if (exRateHelper == null)
			{
				return;
			}
			var sellRateFormat = exRateHelper.DecimalPlacesForSellRate(RefExchangeRate.RE_RX_NKExCurrency);
			RE_SellRateCalcEdit.DecimalPlaces = sellRateFormat;
			RE_SellRateCalcEdit.Decimals = sellRateFormat;
		}

		Enterprise.Integration.Customs.Shared.IExRateHelper GetDecimalPlacesFromCountry(string countryCode)
		{
			if (string.IsNullOrEmpty(countryCode))
			{
				return null;
			}

			var types = ObjectFactory.Get<Hashtable>("ExRateHelpers");
			var objectHandle = (ObjectHandle)types[countryCode];
			return objectHandle?.GetObject() as Enterprise.Integration.Customs.Shared.IExRateHelper;
		}

		public override ODisplayMode DisplayMode
		{
			get { return base.DisplayMode; }
			set
			{
				var fromMode = base.DisplayMode;
				base.DisplayMode = value;
				if (fromMode != DisplayMode)
				{
					var enabled = false;
					var visible = false;
					switch (value)
					{
						case ODisplayMode.Browse:
							visible = true;
							break;
						case ODisplayMode.Edit:
						case ODisplayMode.New:
						case ODisplayMode.NewSaved:
							enabled = true;
							visible = true;
							break;
					}

					ToolStripSaveAndNewButton.Enabled = enabled;
					ToolStripSaveAndNewButton.Visible = visible;
				}
			}
		}

		RefExchangeRate RefExchangeRate => (RefExchangeRate)BusinessEntity;

		public override string FormCaption
		{
			get
			{
				var formName = Res.GetString("RefExchangeRateForm|FormCaptionPrefix", "Exchange Rates");
				return RefExchangeRate.IsInDatabase ?
							Res.GetString("BFB79859-FE31-4CB6-A705-53F20928F87D", "{0} - {1} - {2}", formName, RefExchangeRate.RE_RX_NKExCurrency, RefExchangeRate.ExCurrency?.RX_DescMultilingual ?? ZString.Empty)
							: formName;
			}
		}

		const string IsVisibleForBindingString = nameof(IIsVisibleForBindingControl.IsVisibleForBinding);

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			RE_AsPublishedTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			RE_OH_ClientGuidFindBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			RefExchangeRateCalculatorButton.DataBindings.RemoveBinding(IsVisibleForBindingString);
			if (DataSource != null)
			{
				RE_AsPublishedTextBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, nameof(RefExchangeRate.AsPublishedIsApplicable)));
				RE_OH_ClientGuidFindBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, nameof(RefExchangeRate.LocalClientIsApplicable)));
				RefExchangeRateCalculatorButton.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, nameof(RefExchangeRate.IsIndiaCustomsRate)));
			}
		}

		protected override void ShowNewForm()
		{
			if (DataSource is RefExchangeRate exRate)
			{
				var exRateNew = exRate.Factory.New<RefExchangeRate>();
				exRateNew.RE_SellRate = exRate.RE_SellRate;
				exRateNew.RE_StartDate = exRate.RE_StartDate;
				exRateNew.RE_ExpiryDate = exRate.RE_ExpiryDate;
				exRateNew.RE_ExRateType = exRate.RE_ExRateType;
				exRateNew.RE_OH_Client = exRate.RE_OH_Client;

				EnterpriseFormLookStrategy.SavePositionAndSize(this);
				var controller = ZControllerFactory.Create(ControllerIDs.ExchangeRate);
				controller?.ShowFormForNewEntity(exRateNew);
			}
		}

		void SaveAndNewButton_Click(object sender, EventArgs e)
		{
			if (FireSaveButton() == CargoWise.EntityFramework.ContinueWithSave.Yes)
			{
				ShowNewForm();
				Close();
			}
		}

		void RefExchangeRateCalculatorButton_Click(object sender, EventArgs e)
		{
			if (DataSource is RefExchangeRate exRate && !exRate.RE_RX_NKExCurrency.IsEmpty)
			{
				var exchangeRateCalculator = new RefExchangeRateCalculator(exRate);

				if (ZFormModaliser.ShowDialogAndDispose(new CalculateExchangeRateForm(exchangeRateCalculator)) == DialogResult.OK)
				{
					exchangeRateCalculator.CalculateExchangeRate(RE_SellRateCalcEdit.DecimalPlaces);
				}
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ToolStripSaveAndNewButton.Click -= SaveAndNewButton_Click;
			}
			base.Dispose(disposing);
		}
	}
}
