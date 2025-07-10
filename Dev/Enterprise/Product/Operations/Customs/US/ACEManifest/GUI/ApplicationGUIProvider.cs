using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override ZString GetIdentifierCore(ASYCUDA.Business.AsycudaManifestHeader header) => header != null && !header.IsDeleted
				? header.AMA_RN_NKCountry.PadRight(3) + header.AMA_ManifestType
				: string.Empty;

		protected override ASYCUDA.GUI.AsycudaItemSelectionDialog GetNewAsycudaItemSelectionDialogCore(ASYCUDA.Business.MessageChooser messageChooser, string itemsType, string messageType)
		{
			if (messageChooser is TransferHeaderMessageChooser arrivalsMessageChooser)
			{
				return new TransferHeaderItemSelectionDialog(arrivalsMessageChooser, itemsType);
			}

			switch (messageType)
			{
				case AIMMessageSubTypes.FRI:
				case AIMMessageSubTypes.FXI:
				case AIMMessageSubTypes.FRC:
				case AIMMessageSubTypes.FXC:
				case AIMMessageSubTypes.FRX:
				case AIMMessageSubTypes.FXX:
					{
						return new AIMBillsSelectionDialog((AIMMessageChooser)messageChooser, itemsType);
					}
				case AIMMessageSubTypes.FSQ:
					{
						return new StatusQueryBillSelectionDialog((AIMMessageChooser)messageChooser, itemsType);
					}
			}
			return base.GetNewAsycudaItemSelectionDialogCore(messageChooser, itemsType, messageType);
		}

		protected override void CustomizeBillsGridCore(ASYCUDA.GUI.ZGridWithDynamicColumnHandler billsGrid)
		{
			base.CustomizeBillsGridCore(billsGrid);

			var columnInfo = billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKOrigin);
			if (columnInfo != null)
			{
				columnInfo.CaptionResourceString = Res.GetData("31BCE433-9D25-48AB-BC2C-E400757F2D18", "Flight Origin");
				columnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			}
		}

		#region BillsGridExtraMenuItems

		protected override IEnumerable<ZMenuItem> GetBillsGridExtraMenuItemsCore(ASYCUDA.GUI.ZGridWithDynamicColumnHandler billsGrid)
		{
			var resetMessageStatusMenuItem = new ZMenuItem(ResetMessageStatusMenuItemText);
			resetMessageStatusMenuItem.Click += (s, e) => ResetMessageStatus(billsGrid);
			yield return resetMessageStatusMenuItem;
		}

		void ResetMessageStatus(ASYCUDA.GUI.ZGridWithDynamicColumnHandler billsGrid)
		{
			var mainForm = (ZForm)billsGrid.TopLevelControl;
			var businessEntity = mainForm.BusinessEntityForHasChanges;

			var continueWithSave = ContinueWithSave.Yes;
			if (businessEntity.HasChanges && Globals.Message.Show(FormHasChangesMessage, "Save", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				continueWithSave = mainForm.FireSaveButton();
			}
			if (!businessEntity.HasChanges && continueWithSave == ContinueWithSave.Yes)
			{
				foreach (var bill in billsGrid.GetSelectedElements<AsycudaBill>())
				{
					bill.ResetMessageStatus();
				}

				try
				{
					businessEntity.Factory.Save();
				}
				catch (ZSaveException e1)
				{
					ZExceptionReporting.HandleSaveException(e1);
				}
			}
		}

		protected override void SetBillsGridExtraMenuItemsVisibilityCore(ZMenuItem[] billsGridExtraMenuItems, ASYCUDA.Business.AsycudaBill[] selectedBills, ASYCUDA.Business.AsycudaBill currentBill)
		{
			var resetMessageStatusMenuItem = billsGridExtraMenuItems.FirstOrDefault(x => x.Text == ResetMessageStatusMenuItemText);
			if (resetMessageStatusMenuItem != null)
			{
				resetMessageStatusMenuItem.Visible = true;
				resetMessageStatusMenuItem.Enabled = selectedBills.OfType<AsycudaBill>().Any(b => b.IsSent || b.IsError);
			}
		}

		string ResetMessageStatusMenuItemText => ResString.GetMultilingualString("ACEManifest.GUI|ResetMessageStatusMenuItemText", "Reset Message Status");
		string FormHasChangesMessage => ResString.GetMultilingualString("ACEManifest.GUI|ResetMessageStatus|FormHasChangesMessage", "There are changes in this form. Do you want to save and proceed?");

		#endregion

		protected override IPanelLayoutProvider GetManifestLayoutCore()
		{
			return new ACEManifestLayouts();
		}

		protected override IPanelLayoutProvider GetBillLayoutCore()
		{
			return new ACEBillLayouts();
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var goodsValueGroup = Enterprise.Customs.US.ACEManifest.GUI.Res.GetData("7C848235-955A-45FA-9F6E-7B3422B739B7", "Goods Value (AIM)"); // the group name 'Goods Value' is also used on ABL_FreightValue!

			var goodsValueCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			goodsValueCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			goodsValueCalcEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			goodsValueCalcEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_GoodsValue;
			goodsValueCalcEditColumnStyleInfo.GroupName = goodsValueGroup;
			goodsValueCalcEditColumnStyleInfo.IsVisible = true;
			goodsValueCalcEditColumnStyleInfo.Decimals = 0;
			goodsValueCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return goodsValueCalcEditColumnStyleInfo;

			var goodsValueCurrencyCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			goodsValueCurrencyCodeFindBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_RX_NKGoodsValueCurrency;
			goodsValueCurrencyCodeFindBoxColumnStyleInfo.GroupName = goodsValueGroup;
			goodsValueCurrencyCodeFindBoxColumnStyleInfo.IsVisible = true;
			goodsValueCurrencyCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			yield return goodsValueCurrencyCodeFindBoxColumnStyleInfo;

			var messageStatusTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			messageStatusTextBoxColumnStyleInfo.IsVisible = false;
			messageStatusTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			messageStatusTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_MessageStatus;
			messageStatusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return messageStatusTextBoxColumnStyleInfo;

			var billStatusDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			billStatusDropEditColumnStyleInfo.IsVisible = false;
			billStatusDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			billStatusDropEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_BillStatus;
			billStatusDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return billStatusDropEditColumnStyleInfo;

			var entryNumberTypeTextBoxColumnStyleInfo = new ZDropEditColumnStyleInfo();
			entryNumberTypeTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("FDBFEFCD-4327-4330-BB67-F7F0DBA27BA1", "Entry Type");
			entryNumberTypeTextBoxColumnStyleInfo.IsVisible = true;
			entryNumberTypeTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.CustomsEntryNumberType;
			entryNumberTypeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return entryNumberTypeTextBoxColumnStyleInfo;

			var entryNumberTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			entryNumberTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("CCB84FCB-76DC-470D-9539-3E367009EBAA", "Entry Number");
			entryNumberTextBoxColumnStyleInfo.IsVisible = true;
			entryNumberTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			entryNumberTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.CustomsEntryNumber;
			entryNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return entryNumberTextBoxColumnStyleInfo;

			var goodsOriginColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			goodsOriginColumnStyleInfo.ColumnName = AsycudaBill.Schema.GoodsOrigin;
			goodsOriginColumnStyleInfo.IsVisible = true;
			goodsOriginColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			yield return goodsOriginColumnStyleInfo;

			var tariffColumnStyleInfo = new Universal.GUI.TariffColumnStyleInfo();
			tariffColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_Tariff;
			tariffColumnStyleInfo.IsVisible = true;
			tariffColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			yield return tariffColumnStyleInfo;
		}

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>
			{
				{
					false,
					new[]
					{
						AsycudaBill.Schema.CustomsJobNumber,
						AsycudaBill.Schema.ABL_CustomsValue,
						AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency,
						AsycudaBill.Schema.DiscountValue,
						AsycudaBill.Schema.DiscountValueCurrency,
						AsycudaBill.Schema.ABL_FreightValue,
						AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency,
						AsycudaBill.Schema.ABL_TransportValue,
						AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency,
						AsycudaBill.Schema.ABL_InsuranceValue,
						AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency,
						AsycudaBill.Schema.OtherChargesValue,
						AsycudaBill.Schema.OtherChargesValueCurrency,
						AsycudaBill.Schema.ABL_PrepaidCollect,
						AsycudaBill.Schema.ABL_UCRNumber,
						AsycudaBill.Schema.ABL_Volume,
						AsycudaBill.Schema.ABL_VolumeUQ,
						AsycudaBill.Schema.ABL_MarksAndNumbers,
						AsycudaBill.Schema.ABL_Remarks,
						AsycudaBill.Schema.ABL_CargoStatus,
						AsycudaBill.Schema.ABL_CarrierReference,
						AsycudaBill.Schema.ABL_ManifestUQ,
						AsycudaBill.Schema.ABL_BolType
					}
				}
			};
		}

		protected override IReadOnlyDictionary<bool, string[]> GetArrivalHeadersGridColumnAvailabilityCore()
		{
			return new Dictionary<bool, string[]>
			{
				{
					false, new[] { AsycudaArrivalHeader.Schema.ATH_ETAAtDischargePort, AsycudaArrivalHeader.Schema.ATH_ReferenceIssueDate, AsycudaArrivalHeader.Schema.ATH_ArrivalSequence }
				}
			};
		}

		protected override IReadOnlyDictionary<bool, string[]> GetArrivalLinesGridColumnAvailabilityCore()
		{
			return new Dictionary<bool, string[]>
			{
				{
					false, new[] { Customs.ASYCUDA.Business.AsycudaArrivalLine.Schema.ATL_Weight, Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalLine.Schema.ATL_WeightUQ, Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalLine.Schema.ATL_APA_AsycudaPack, Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalLine.Schema.ATL_ABL_AsycudaBill }
				}
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetTransferBillsGridExtraColumnInfosCore()
		{
			yield return new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = AsycudaTransferBill.Schema.InBondNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
			};

			yield return new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = AsycudaTransferBill.Schema.ATB_MessageStatusDescription,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
			};
		}

		protected override IEnumerable<string> GetTransferBillsGridColumnsOrderCore()
		{
			yield return AsycudaTransferBill.Schema.ATB_BillNumber;
			yield return AsycudaTransferBill.Schema.InBondNumber;
			yield return AsycudaTransferBill.Schema.ATB_MessageStatus;
			yield return AsycudaTransferBill.Schema.ATB_MessageStatusDescription;
		}

		protected override ZUserControl GetTransferBillCountrySpecificUserControlCore() => new TransferBillInBondNumberAllocationUserControl();
	}
}
