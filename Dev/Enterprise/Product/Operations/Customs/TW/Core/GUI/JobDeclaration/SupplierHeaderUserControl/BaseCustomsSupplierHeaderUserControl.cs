using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TW.GUI
{
	public partial class BaseCustomsSupplierHeaderUserControl : Customs.GUI.DeclarationInvoiceHeaderUserControl
	{
		public BaseCustomsSupplierHeaderUserControl()
		{
			InitializeComponent();
			AddExchangeRateColumns(InvoiceChargesGrid);
			AddExchangeRateColumns(BaseGroupChargesGrid);
			ResetColumnsInInvoiceChargesGrid();

			SetJobComInvoiceHeadersBoundGridColumns();
			BaseGroupChargesGrid.ColumnStyles.Remove(BaseGroupChargesGrid.GetColumnStyle(AutoJobComInvHeaderCharge.Schema.J7_Percentage));
		}

		protected virtual void SetJobComInvoiceHeadersBoundGridColumns()
		{
			var calcEditColumnStyleInfo = JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_NoOfPacks) as ZCalcEditColumnStyleInfo;
			if (calcEditColumnStyleInfo != null)
			{
				calcEditColumnStyleInfo.Decimals = 0;
			}
		}

		public static new string IsGSTApplicableCaption => Enterprise.Customs.TW.GUI.Res.GetString("7D2178A5-B979-4177-B297-35CF1E8B732F", "Add to MoF?");

		protected override string ColumnTitleForGSTApplies => IsGSTApplicableCaption;

		protected new JobComInvoiceHeader CurrentInvoiceHeader => (JobComInvoiceHeader)base.CurrentInvoiceHeader;

		protected void AddExchangeRateColumns(ZGrid chargesGrid)
		{
			var exchangeRateUserEnterableCheckBoxColumnStyleInfo = new ZCheckBoxColumnStyleInfo();
			exchangeRateUserEnterableCheckBoxColumnStyleInfo.ColumnName = "IsJ7_ExchangeRateUserEnterable";
			exchangeRateUserEnterableCheckBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("B8344ED1-9583-428B-B2B4-0278DD76D684", "Fixed Rate?");
			exchangeRateUserEnterableCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(84);
			chargesGrid.ColumnStyles.Add(exchangeRateUserEnterableCheckBoxColumnStyleInfo);
			var exchangeRateCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			exchangeRateCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			exchangeRateCalcEditColumnStyleInfo.ColumnName = "J7_ExchangeRate";
			exchangeRateCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("BC7674DB-E679-4D1C-8052-D56B4F9EB042", "Exchange Rate");
			exchangeRateCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			chargesGrid.ColumnStyles.Add(exchangeRateCalcEditColumnStyleInfo);
		}

		void ResetColumnsInInvoiceChargesGrid()
		{
			InvoiceChargesGrid.SetColumnVisible(false, Customs.Business.BaseJobComInvHeaderCharge.Schema.ChargeCodeDescription);
			InvoiceChargesGrid.RemoveFromAvailableColumns(Customs.Business.BaseJobComInvHeaderCharge.Schema.ChargeCodeDescription);
			var chargeDescriptionTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			chargeDescriptionTextBoxColumnStyleInfo.ColumnName = AutoJobComInvHeaderCharge.Schema.J7_ChargeDescription;
			chargeDescriptionTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("62573401-8E42-4500-8C63-944F653419B6", "Description");
			chargeDescriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			InvoiceChargesGrid.ColumnStyles.Insert(1, chargeDescriptionTextBoxColumnStyleInfo);

			var isIncludedInInvoiceAmountColumnInfo = InvoiceChargesGrid.GetColumnStyle(Enterprise.Customs.Business.BaseJobComInvHeaderCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount);
			isIncludedInInvoiceAmountColumnInfo.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("7E4B628B-7D28-4F95-A3F8-7A8F1E3B2129", "Included in Inv. Amt", "Included in Inv. Amt", "It indicates whether the charge is included in the invoice amount. The Incoterm and charge code determine whether the charge is included by default.");
			isIncludedInInvoiceAmountColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(157);
		}

		#region The Columns of JobComInvoiceHeadersBoundGrid (Order, DisplayHidden)
		readonly string[] defaultColumnsForGrid = new string[]
		{
			JobComInvoiceHeader.Schema.JZ_InvoiceNumber,
			JobComInvoiceHeader.Schema.JZ_IncoTerm,
			JobComInvoiceHeader.Schema.JZ_IncoTermPlace,
			JobComInvoiceHeader.Schema.JZ_InvoiceDate,
			JobComInvoiceHeader.Schema.JZ_InvoiceAmount,
			JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency,
			JobComInvoiceHeader.Schema.JZ_Calc_BalanceString,
			JobComInvoiceHeader.Schema.InvoiceLineTotal,
			JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate,
			JobComInvoiceHeader.Schema.TW_MarksAndNumbers,
			JobComInvoiceHeader.Schema.JZ_Weight,
			JobComInvoiceHeader.Schema.JZ_WeightUQ,
			JobComInvoiceHeader.Schema.JZ_NetWeight,
			JobComInvoiceHeader.Schema.JZ_NetWeightUQ
		};

		string[] fColumnNamesInSortOrder;
		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (fColumnNamesInSortOrder == null)
				{
					var result = new List<string>(defaultColumnsForGrid);
					result.Add(JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill);
					result.Add(JobComInvoiceHeader.Schema.JZ_Calc_ChargesExcludedFromITOT);
					result.Add(JobComInvoiceHeader.Schema.JZ_Calc_CIFAmount);
					result.Add(JobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency);
					result.Add(JobComInvoiceHeader.Schema.JZ_Calc_FOBAmount);
					result.Add(JobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency);
					result.Add(JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice);
					result.Add(JobComInvoiceHeader.Schema.JZ_InvoiceCurrLandedCostExRate);
					result.Add(JobComInvoiceHeader.Schema.JZ_NoOfPacks);
					result.Add(JobComInvoiceHeader.Schema.NoOfPacksPackType);
					result.Add(JobComInvoiceHeader.Schema.JZ_PaymentNo);
					result.Add(JobComInvoiceHeader.Schema.JZ_PaymentAmount);
					result.Add(JobComInvoiceHeader.Schema.JZ_PaymentDate);
					result.Add(JobComInvoiceHeader.Schema.JZ_PaymentExRate);
					result.Add(JobComInvoiceHeader.Schema.JZ_InvoiceDisplaySequence);
					result.Add(JobComInvoiceHeader.Schema.JZ_Volume);
					result.Add(JobComInvoiceHeader.Schema.JZ_VolumeUQ);
					fColumnNamesInSortOrder = result.ToArray();
				}
				return fColumnNamesInSortOrder;
			}
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			using (JobComInvoiceHeadersBoundGrid.InnerGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(JobComInvoiceHeader.Schema.JZ_OH_Supplier);
				JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(JobComInvoiceHeader.Schema.SupplierName);
				JobComInvoiceHeadersBoundGrid.InnerGrid.RemoveFromAvailableColumns(JobComInvoiceHeader.Schema.JZ_OH_Buyer);

				JobComInvoiceHeadersBoundGrid.InnerGrid.ReOrderColumns(ColumnNamesInSortOrder);
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(false, ColumnNamesInSortOrder);
				JobComInvoiceHeadersBoundGrid.InnerGrid.SetColumnVisible(true, defaultColumnsForGrid);

				var invoiceLineTotalColumnInfo = JobComInvoiceHeadersBoundGrid.InnerGrid.GetColumnStyle(JobComInvoiceHeader.Schema.InvoiceLineTotal);
				invoiceLineTotalColumnInfo.CaptionResourceString = Res.GetData("74350141-D54B-41B9-A703-B3A1F5132905", "Line Total", "Invoice Line Total", "");
			}
		}
		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			JobComInvoiceHeadersBoundGrid.InnerGrid.ContextMenu.Popup += CustomsInvoiceLinesBoundGridContextMenuPopup;
			AddPackingListMenuItemToGrid();
		}

		void CustomsInvoiceLinesBoundGridContextMenuPopup(object sender, EventArgs e)
		{
			if (packingListMenuItem != null)
			{
				packingListMenuItem.Text = GetPackingListMenuItemCaption();
			}
		}

		MenuItem packingListMenuItemSeparatorMenuItem;
		MenuItem packingListMenuItem;
		void AddPackingListMenuItemToGrid()
		{
			packingListMenuItemSeparatorMenuItem = new ZMenuItem("-") { Name = nameof(packingListMenuItemSeparatorMenuItem) };
			packingListMenuItem = new ZMenuItem(GetPackingListMenuItemCaption(), PackingListMenuItem_Click) { Name = nameof(packingListMenuItem) };
			var headersGridMenuItems = JobComInvoiceHeadersBoundGrid.InnerGrid.ContextMenu.MenuItems;
			headersGridMenuItems.Add(packingListMenuItemSeparatorMenuItem);
			headersGridMenuItems.Add(packingListMenuItem);
		}

		ZString GetPackingListMenuItemCaption() => CurrentInvoiceHeader?.HasCusPackingList ?? false ? ResString.GetMultilingualString("5D27A6BE-B331-4CB4-9859-0E765434FCC3", "Edit Packing List") : ResString.GetMultilingualString("1573A616-7359-44E5-AE0E-701D81AC6BF6", "Create Packing List");

		protected void PackingListMenuItem_Click(object sender, EventArgs e)
		{
			if (PreSaveInvoiceHeader(CurrentInvoiceHeader) && CurrentInvoiceHeader.IsInDatabase)
			{
				var factoryForPackingList = new BusinessObjectFactory();
				factoryForPackingList.Saved += FactoryForPackingList_Saved;

				var packingList = CurrentInvoiceHeader.LoadCusPackingList(factoryForPackingList);

				try
				{
					if (packingList == null)
					{
						packingList = CreateNewPackingList(factoryForPackingList);
					}

					if (packingList != null)
					{
						packingList.AddDefaultPackageIfNeeded();

						var controller = ZControllerFactory.Create(ControllerIDs.Customs.CusPackingList);
						controller.SetFormsModalTo(ParentForm);
						var form = packingList.IsInDatabase ? controller.ShowEditForm(packingList) : controller.ShowFormForNewEntity(packingList);
						if (form != null)
						{
							form.Closed += PackingListForm_Closed;
						}
#if DEBUG
						if (Globals.IsTest)
						{
							LastController = controller;
						}
#endif
					}
				}
				catch (Exception)
				{
					UnlockMutexIfLockedByThisInstance();
					throw;
				}
			}
		}

		bool PreSaveInvoiceHeader(JobComInvoiceHeader header)
		{
			var topLevelBizObj = header.JobDeclaration?.Shipment ?? header.JobDeclaration ?? header as BusinessObject;
			return Enterprise.Customs.GUI.PlugIn.CustomsPlugIn.FormPreSaved(topLevelBizObj, ParentForm as ZForm);
		}

		CusPackingList CreateNewPackingList(BusinessObjectFactory factoryForPackingList)
		{
			CusPackingList packingList = null;

			if (!MutexForCreatePackingList.IsLocked)
			{
				if (MutexForCreatePackingList.Lock())
				{
					packingList = CurrentInvoiceHeader.CreateCusPackingList(factoryForPackingList);
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("12DEC6C5-C2A0-4F03-85D7-F5E8C40B9148", "Someone else is already in the process of creating a Packing List.\r\nYou should be able to access the Packing List when the person has saved the record. Please try later."));
			}

			return packingList;
		}

		void PackingListForm_Closed(object sender, EventArgs e)
		{
			UnlockMutexIfLockedByThisInstance();
		}

		void FactoryForPackingList_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= FactoryForPackingList_Saved;
			UnlockMutexIfLockedByThisInstance();
		}

		void UnlockMutexIfLockedByThisInstance()
		{
			if (MutexForCreatePackingList.HasLock)
			{
				MutexForCreatePackingList.Unlock();
			}
		}

		ZGlobalMutex MutexForCreatePackingList
		{
			get { return mutex ?? (mutex = new ZGlobalMutex(MutexIDs.CusPackingListMutex, CurrentInvoiceHeader.PK.ToString())); }
		}
		ZGlobalMutex mutex;

#if DEBUG
		public ZController LastController;
#endif
	}
}
