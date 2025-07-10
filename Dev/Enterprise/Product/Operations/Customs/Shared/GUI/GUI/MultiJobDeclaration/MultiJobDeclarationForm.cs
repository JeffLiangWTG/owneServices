using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public partial class MultiJobDeclarationForm : ZForm
	{
		public MultiJobDeclarationForm(MultiJobDeclarationHeader topLevelBizO)
			: base(topLevelBizO)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, postButton, cancelButton);

			MenuItem fileNewMenuItem = FileMenuItem.MenuItems.FindByName(ZFormMenuStrategy.FileNewMenuItemName);
			if (fileNewMenuItem != null)
			{
				fileNewMenuItem.Dispose();
			}

			this.topLevelBizO = topLevelBizO;
		}
		readonly MultiJobDeclarationHeader topLevelBizO;

		internal class EmbeddedModulePopupWithNoButtonPanel : EmbeddedModulePopup
		{
			public EmbeddedModulePopupWithNoButtonPanel(ZFilterModule module)
				: base(module)
			{
				ButtonPanel.Visible = false;
			}
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);
			if (isNotFinalizing)
			{
				if (topLevelBizO.Collection.Count > 0 && topLevelBizO.Collection[0].IsInDatabase)
				{
					ZFilterGridModule filterModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration);
					EmbeddedModulePopupWithNoButtonPanel popup = new EmbeddedModulePopupWithNoButtonPanel(filterModule);
					filterModule.SetFormsModalTo(null);
					BusinessObjectCollection collection = (BusinessObjectCollection)filterModule.GridCollection;
					collection.Load(ZQuery.NoResultQuery);
					foreach (BaseJobComInvoiceLine bizO in topLevelBizO.Collection)
					{
						BaseJobDeclaration declaration = bizO.Declaration;
						BusinessObject bizOFactory2 = collection.Factory.ImportFromAnotherFactory(declaration);
						collection.Add(bizOFactory2);
					}
					filterModule.UpdateModuleResultsCache();
					popup.Show();
				}
			}
		}

		public MultiJobDeclarationForm()
			: base()
		{
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return Res.GetString("a71e20fd-947e-438a-b865-96f62a8887c8", "Bulk Declaration Entry"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			#region Transport Details

			ZTextBoxColumnStyleInfo masterBillTextBox = new ZTextBoxColumnStyleInfo("Declaration+JE_MasterBill", 80);
			masterBillTextBox.CharacterCasing = CharacterCasing.Upper;
			masterBillTextBox.Caption = Res.GetString("a58a2c90-0646-4bff-92b0-252be02ff2f6", "Master Bill");
			grid.ColumnStyles.Add(masterBillTextBox);

			ZCodeFindBoxColumnStyleInfo portOfLoadingFindBox = new ZCodeFindBoxColumnStyleInfo();
			portOfLoadingFindBox.Caption = Res.GetString("cfd6ec0b-d690-4ae2-842f-d1f15a7e134c", "Loading");
			ControlDpiScalingHelper.SetWidth(ref portOfLoadingFindBox, 50, true);
			portOfLoadingFindBox.ColumnName = "Declaration+JE_RL_NKPortOfLoading";
			portOfLoadingFindBox.BindToList = "Declaration+Lookups+PortOfLoadings";
			portOfLoadingFindBox.ModuleID = ModuleIDs.RefUNLOCO;
			this.grid.ColumnStyles.Add(portOfLoadingFindBox);

			ZCodeFindBoxColumnStyleInfo vesselFlightTextBox = new ZCodeFindBoxColumnStyleInfo();
			vesselFlightTextBox.ColumnName = "Declaration+JE_VesselName";
			ControlDpiScalingHelper.SetWidth(ref vesselFlightTextBox, 80, true);
			vesselFlightTextBox.CharacterCasing = CharacterCasing.Upper;
			vesselFlightTextBox.Caption = Res.GetString("eb28bc03-da71-4690-8be7-59871d269e6a", "Vessel");
			this.grid.ColumnStyles.Add(vesselFlightTextBox);

			ZTextBoxColumnStyleInfo voyageFolioTextBox = new ZTextBoxColumnStyleInfo("Declaration+JE_VoyageFlightNo", 80);
			voyageFolioTextBox.CharacterCasing = CharacterCasing.Upper;
			voyageFolioTextBox.Caption = Res.GetString("F8759629-04FA-4E65-AFFD-015DFDAFCD53", "Voyage/Flight");
			grid.ColumnStyles.Add(voyageFolioTextBox);

			ZCodeFindBoxColumnStyleInfo arrivalFindBox = new ZCodeFindBoxColumnStyleInfo();
			arrivalFindBox.Caption = Res.GetString("a7c3017a-a00e-4a51-9f51-1ee457e34552", "Arrival");
			ControlDpiScalingHelper.SetWidth(ref arrivalFindBox, 50, true);
			arrivalFindBox.ColumnName = "Declaration+JE_RL_NKPortOfArrival";
			arrivalFindBox.BindToList = "Declaration+Lookups+PortOfArrivals";
			arrivalFindBox.ModuleID = ModuleIDs.RefUNLOCO;
			this.grid.ColumnStyles.Add(arrivalFindBox);

			#endregion

			#region Shipment Details

			ZTextBoxColumnStyleInfo houseBillTextBox = new ZTextBoxColumnStyleInfo("Declaration+JE_HouseBill", 50);
			houseBillTextBox.CharacterCasing = CharacterCasing.Upper;
			houseBillTextBox.Caption = Res.GetString("8e03b9e7-d611-4de4-8d4c-57c165f2f666", "House Bill");
			grid.ColumnStyles.Add(houseBillTextBox);

			ZCodeFindBoxColumnStyleInfo originFindBox = new ZCodeFindBoxColumnStyleInfo();
			originFindBox.Caption = Res.GetString("a553cd8e-5876-4333-8844-595dfc0f6fc7", "Origin");
			ControlDpiScalingHelper.SetWidth(ref originFindBox, 50, true);
			originFindBox.ColumnName = "Declaration+JE_RL_NKOrigin";
			originFindBox.BindToList = "Declaration+Lookups+Origins";
			originFindBox.ModuleID = ModuleIDs.RefUNLOCO;
			this.grid.ColumnStyles.Add(originFindBox);

			ZCodeFindBoxColumnStyleInfo destinationFindBox = new ZCodeFindBoxColumnStyleInfo();
			destinationFindBox.Caption = Res.GetString("01d6fcf4-6413-4aa7-9e91-ff4070237462", "Destination");
			ControlDpiScalingHelper.SetWidth(ref destinationFindBox, 50, true);
			destinationFindBox.ColumnName = "Declaration+JE_RL_NKFinalDestination";
			destinationFindBox.BindToList = "Declaration+Lookups+FinalDestinations";
			destinationFindBox.ModuleID = ModuleIDs.RefUNLOCO;
			this.grid.ColumnStyles.Add(destinationFindBox);

			AddConsignorConsignee(this.grid);

			#endregion

			#region Invoice Header Details

			ZTextBoxColumnStyleInfo invoiceNumberTextBox = new ZTextBoxColumnStyleInfo("InvoiceHeader+JZ_InvoiceNumber", 50);
			invoiceNumberTextBox.Caption = Res.GetString("662d45cc-caff-4df4-b218-5146829d7187", "Invoice #");
			invoiceNumberTextBox.CharacterCasing = CharacterCasing.Upper;
			this.grid.ColumnStyles.Add(invoiceNumberTextBox);

			ZCalcEditColumnStyleInfo invoiceAmountCalcEdit = new ZCalcEditColumnStyleInfo();
			invoiceAmountCalcEdit.ColumnName = "InvoiceHeader+JZ_InvoiceAmount";
			invoiceAmountCalcEdit.Caption = Res.GetString("6626aa12-d807-46a8-af27-cbb45f063138", "Amount");
			ControlDpiScalingHelper.SetWidth(ref invoiceAmountCalcEdit, 50, true);
			this.grid.ColumnStyles.Add(invoiceAmountCalcEdit);

			ZCodeFindBoxColumnStyleInfo currencyFindBox = new ZCodeFindBoxColumnStyleInfo();
			currencyFindBox.Caption = Res.GetString("d94bd53a-a6b0-46a0-bc21-e318074b7df2", "Currency");
			ControlDpiScalingHelper.SetWidth(ref currencyFindBox, 50, true);
			currencyFindBox.ColumnName = "InvoiceHeader+JZ_RX_NKInvoice_Currency";
			currencyFindBox.BindToList = "InvoiceHeader+Lookups+CurrencyList";
			currencyFindBox.ModuleID = ModuleIDs.RefCurrency;
			this.grid.ColumnStyles.Add(currencyFindBox);

			#endregion

			ZGridColumnInfo tariffFindBox = GetTariffColumnStyleInfo();
			tariffFindBox.ColumnName = "JI_Tariff";
			this.grid.ColumnStyles.Add(tariffFindBox);

			ZCodeFindBoxColumnStyleInfo countryOfOriginFindBox = new ZCodeFindBoxColumnStyleInfo();
			countryOfOriginFindBox.Caption = Res.GetString("a86d3c40-440d-4b61-91d0-411b47cd1259", "Ctry.");
			ControlDpiScalingHelper.SetWidth(ref countryOfOriginFindBox, 40, true);
			countryOfOriginFindBox.ColumnName = "JI_CountryOfOrigin";
			countryOfOriginFindBox.BindToList = "Lookups+CountryOfOrigins";
			countryOfOriginFindBox.ModuleID = ModuleIDs.RefCountry;
			this.grid.ColumnStyles.Add(countryOfOriginFindBox);

			ZCalcEditColumnStyleInfo weightInfo = new ZCalcEditColumnStyleInfo();
			weightInfo.GroupName = Res.GetData("MultiJobDeclarationForm|c3504e0d-df81-4461-b188-e21d71e7a936", "Weight");
			weightInfo.ColumnName = "JI_Weight";
			weightInfo.GroupName = Res.GetData("MultiJobDeclarationForm|c3504e0d-df81-4461-b188-e21d71e7a936", "Weight");
			this.grid.ColumnStyles.Add(weightInfo);

			ZDropEditColumnStyleInfo weightUQInfo = new ZDropEditColumnStyleInfo();
			weightUQInfo.Caption = Res.GetString("F9D4A40C-E491-40A9-88FB-BC089193021B", "UQ");
			ControlDpiScalingHelper.SetWidth(ref weightUQInfo, 50, true);
			weightUQInfo.GroupName = Res.GetData("MultiJobDeclarationForm|c3504e0d-df81-4461-b188-e21d71e7a936", "Weight");
			weightUQInfo.BindToList = "Lookups.WeightUQList";
			weightUQInfo.ColumnName = "JI_WeightUQ";
			weightUQInfo.GroupName = Res.GetData("MultiJobDeclarationForm|c3504e0d-df81-4461-b188-e21d71e7a936", "Weight");
			this.grid.ColumnStyles.Add(weightUQInfo);

			ZCalcEditColumnStyleInfo qtyInfo = new ZCalcEditColumnStyleInfo();
			qtyInfo.GroupName = Res.GetData("MultiJobDeclarationForm|1e044e36-2ea4-4336-bb5d-57a7b0a666b2", "Customs Qty");
			qtyInfo.ColumnName = "JI_CustomsQuantity";
			this.grid.ColumnStyles.Add(qtyInfo);

			ZTextBoxColumnStyleInfo uQInfo = new ZTextBoxColumnStyleInfo("UQ", 50);
			uQInfo.GroupName = Res.GetData("MultiJobDeclarationForm|1e044e36-2ea4-4336-bb5d-57a7b0a666b2", "Customs Qty");
			uQInfo.ColumnName = "JI_CustomsUnitQty";
			this.grid.ColumnStyles.Add(uQInfo);

			grid.GridId = "GridLayout0vOX7B27P3cmSNhVPzLE8w==";
		}

		protected virtual void AddConsignorConsignee(ZGrid grid)
		{
			ZGuidFindBoxColumnStyleInfo consignorInfo = new ZGuidFindBoxColumnStyleInfo();
			consignorInfo.Caption = Res.GetString("7e3dc36e-f738-4746-bdf7-e593a1d60bb8", "Consignor");
			consignorInfo.ColumnName = "Declaration+JE_OH_Supplier";
			consignorInfo.BindToList = "Declaration+Lookups+SuppliersList";
			consignorInfo.ModuleID = ModuleIDs.Organisation;
			grid.ColumnStyles.Add(consignorInfo);

			ZGuidFindBoxColumnStyleInfo consigneeInfo = new ZGuidFindBoxColumnStyleInfo();
			consigneeInfo.Caption = Res.GetString("fcc6d3cc-a2e6-4146-a678-1eb027829341", "Consignee");
			consigneeInfo.ColumnName = "Declaration+JE_OH_Importer";
			consigneeInfo.BindToList = "Declaration+Lookups+ImportersList";
			consigneeInfo.ModuleID = ModuleIDs.Organisation;
			grid.ColumnStyles.Add(consigneeInfo);
		}

		protected virtual ZGridColumnInfo GetTariffColumnStyleInfo()
		{
			ZGridColumnInfo tariffFindBox = new ZTextBoxColumnStyleInfo((NoResString)"Tariff", 60);
			return tariffFindBox;
		}
	}
}
