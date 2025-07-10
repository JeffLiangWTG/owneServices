using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>()
			{
				{ false, new[]
					{
						AsycudaBill.Schema.ABL_SequenceNumber,
						AsycudaBill.Schema.ABL_BolType,
						AsycudaBill.Schema.ABL_RL_NKOrigin,
						AsycudaBill.Schema.ABL_CarrierReference,
						AsycudaBill.Schema.ABL_GoodsDescription,
						AsycudaBill.Schema.ABL_CustomsValue,
						AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency,
						AsycudaBill.Schema.ABL_FreightValue,
						AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency,
						AsycudaBill.Schema.ABL_InsuranceValue,
						AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency,
						AsycudaBill.Schema.ABL_TransportValue,
						AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency,
						AsycudaBill.Schema.ABL_PrepaidCollect,
						AsycudaBill.Schema.ABL_CargoStatus,
						AsycudaBill.Schema.ShipperOrgPK,
						AsycudaBill.Schema.ABL_OA_Shipper,
						AsycudaBill.Schema.ABL_ShipperName,
						AsycudaBill.Schema.ABL_ShipperStreet1,
						AsycudaBill.Schema.ABL_ShipperStreet2,
						AsycudaBill.Schema.ABL_ShipperCity,
						AsycudaBill.Schema.ABL_ShipperState,
						AsycudaBill.Schema.ABL_ShipperPostcode,
						AsycudaBill.Schema.ABL_RN_NKShipperCountry,
						AsycudaBill.Schema.ConsigneeOrgPK,
						AsycudaBill.Schema.ABL_OA_Consignee,
						AsycudaBill.Schema.ABL_ConsigneeName,
						AsycudaBill.Schema.ABL_ConsigneeStreet1,
						AsycudaBill.Schema.ABL_ConsigneeStreet2,
						AsycudaBill.Schema.ABL_ConsigneeCity,
						AsycudaBill.Schema.ABL_ConsigneeState,
						AsycudaBill.Schema.ABL_ConsigneePostcode,
						AsycudaBill.Schema.ABL_RN_NKConsigneeCountry,
						AsycudaBill.Schema.ABL_ConsigneePhone,
						AsycudaBill.Schema.NotifyPartyOrgPK,
						AsycudaBill.Schema.ABL_OA_NotifyParty,
						AsycudaBill.Schema.ABL_NotifyPartyName,
						AsycudaBill.Schema.ABL_NotifyPartyStreet1,
						AsycudaBill.Schema.ABL_NotifyPartyStreet2,
						AsycudaBill.Schema.ABL_NotifyPartyCity,
						AsycudaBill.Schema.ABL_NotifyPartyState,
						AsycudaBill.Schema.ABL_NotifyPartyPostcode,
						AsycudaBill.Schema.ABL_RN_NKNotifyPartyCountry,
						AsycudaBill.Schema.ABL_NotifyPartyPhone,
						AsycudaBill.Schema.DiscountValue,
						AsycudaBill.Schema.DiscountValueCurrency,
						AsycudaBill.Schema.OtherChargesValue,
						AsycudaBill.Schema.OtherChargesValueCurrency,
						AsycudaBill.Schema.CustomsJobNumber
					}
				}
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var portOfLoadingCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			portOfLoadingCodeFindBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_RL_NKPortOfLoading;
			portOfLoadingCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return portOfLoadingCodeFindBoxColumnStyleInfo;

			var locationInformationTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			locationInformationTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_LocationInformation;
			locationInformationTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			yield return locationInformationTextBoxColumnStyleInfo;

			var goodsLocationCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			goodsLocationCodeFindBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_GoodsLocation;
			goodsLocationCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return goodsLocationCodeFindBoxColumnStyleInfo;

			var shipmentTypeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			shipmentTypeDropEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_ShipmentType;
			shipmentTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			yield return shipmentTypeDropEditColumnStyleInfo;

			var goodsDescriptionMultiLineTextBoxColumnInfo = new ZMultiLineTextBoxColumnInfo();
			goodsDescriptionMultiLineTextBoxColumnInfo.ColumnName = AsycudaBill.Schema.GoodsDescription;
			goodsDescriptionMultiLineTextBoxColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			yield return goodsDescriptionMultiLineTextBoxColumnInfo;

			var bagNumberDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			bagNumberDropEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.BagNumber;
			bagNumberDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132);
			bagNumberDropEditColumnStyleInfo.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			yield return bagNumberDropEditColumnStyleInfo;

			var splitQuantityTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			splitQuantityTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_SplitQuantity;
			splitQuantityTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			splitQuantityTextBoxColumnStyleInfo.GroupName = Res.GetData("8AABC5BD-8D09-41D3-83B7-DB2E93060BAE", "Split Quantity");
			yield return splitQuantityTextBoxColumnStyleInfo;

			var splitQuantityUQDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			splitQuantityUQDropEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_SplitQuantityUQ;
			splitQuantityUQDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			splitQuantityUQDropEditColumnStyleInfo.GroupName = Res.GetData("8AABC5BD-8D09-41D3-83B7-DB2E93060BAE", "Split Quantity");
			yield return splitQuantityUQDropEditColumnStyleInfo;

			var customsEntryNumberTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			customsEntryNumberTextBoxColumnStyleInfo.ColumnName = ASYCUDA.Business.AsycudaBill.Schema.CustomsEntryNumber;
			customsEntryNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return customsEntryNumberTextBoxColumnStyleInfo;

			var billStatusDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			billStatusDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			billStatusDropEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_BillStatus;
			billStatusDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return billStatusDropEditColumnStyleInfo;

			var billStatusDescriptionTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			billStatusDescriptionTextBoxColumnStyleInfo.ColumnName = ASYCUDA.Business.AsycudaBill.Schema.ABL_BillStatusDescription;
			billStatusDescriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			yield return billStatusDescriptionTextBoxColumnStyleInfo;

			var messageStatusDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			messageStatusDropEditColumnStyleInfo.IsReadOnly = true;
			messageStatusDropEditColumnStyleInfo.CharacterCasing = CharacterCasing.Upper;
			messageStatusDropEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_MessageStatus;
			messageStatusDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return messageStatusDropEditColumnStyleInfo;

			var messageStatusTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			messageStatusTextBoxColumnStyleInfo.ColumnName = ASYCUDA.Business.AsycudaBill.Schema.StatusDescription;
			messageStatusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			yield return messageStatusTextBoxColumnStyleInfo;
		}

		protected override IReadOnlyDictionary<string, bool> GetBillsGridColumnVisiblilityOnValueChangedCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<string, bool>()
			{
				{ AsycudaBill.Schema.BagNumber, header.IsAir }
			};
		}

		protected override IReadOnlyDictionary<string, int> GetBillsGridColumnsWidthCore()
		{
			return new Dictionary<string, int>()
			{
				{ AsycudaBill.Schema.ABL_Remarks, 157 },
				{ AsycudaBill.Schema.ABL_ManifestQty, 70 },
				{ AsycudaBill.Schema.ABL_GrossWeight, 70 },
				{ AsycudaBill.Schema.ABL_Volume, 70 }
			};
		}

		protected override void AMA_VesselNameInfoValueChangedCore(ASYCUDA.Business.AsycudaManifestHeader header, object sender, EventArgs e)
		{
			var headerLloydsNumber = header.AMA_LloydsNumber;
			var vessel = header.Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, header.AMA_VesselName);
			if (vessel != null && headerLloydsNumber != vessel.RV_LloydsNumber)
			{
				if (headerLloydsNumber.IsEmpty ||
					Globals.Message.Show(
						Res.GetString("1729c897-af6a-4b9f-b592-0a2cbed45f42", "The selected Vessel has a different IMO Number ({0}). Do you want to replace the current value ({1})?", vessel.RV_LloydsNumber, headerLloydsNumber),
						Res.GetString("d7294dc3-9913-46b2-a152-a388d60823ed", "Vessel has a different IMO Number"),
						MessageBoxButtons.YesNo,
						DialogResult.Yes) == DialogResult.Yes)
				{
					header.DefaultVesselValues(vessel);
				}
			}
		}

		protected override void VesselCodeFindBox_PopupSelectedCore(ASYCUDA.Business.AsycudaManifestHeader header, object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
		}

		protected override IReadOnlyDictionary<bool, string[]> GetMessagesGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>()
			{
				{ false, new[]
					{
						AsycudaMessage.Schema.EM_MessageDateTime,
						TWMessage.Schema.InterchangeeHubID
					}
				}
			};
		}

		protected override IReadOnlyDictionary<bool, string[]> GetMessagesGridColumnVisibleCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>()
			{
				{ true, new[] { AsycudaMessage.Schema.EM_MessageSubType } },
				{ false, new[] { AsycudaMessage.Schema.EM_SystemCreateTimeUtc } }
			};
		}

		protected override IReadOnlyDictionary<string, int> GetMessagesGridColumnsWidthCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<string, int>()
			{
				{ AsycudaMessage.Schema.EM_MessageSubType, 130 },
				{ AsycudaMessage.Schema.EM_SystemCreateTimeUtc, 140 },
				{ AsycudaMessage.Schema.EM_SystemCreateUser, 140 }
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetMessagesGridExtraColumnInfosCore()
		{
			var applicationCodeTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			applicationCodeTextBoxColumnStyleInfo.ColumnName = "EM_ApplicationCode";
			applicationCodeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			applicationCodeTextBoxColumnStyleInfo.IsVisible = false;
			yield return applicationCodeTextBoxColumnStyleInfo;

			var messageOwnerTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			messageOwnerTextBoxColumnStyleInfo.ColumnName = "EM_MessageOwner";
			messageOwnerTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			messageOwnerTextBoxColumnStyleInfo.IsVisible = false;
			yield return messageOwnerTextBoxColumnStyleInfo;

			var sendWithMessageErrorsCheckBoxColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			sendWithMessageErrorsCheckBoxColumnStyleInfo.ColumnName = "EM_SendWithMessageErrors";
			sendWithMessageErrorsCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			sendWithMessageErrorsCheckBoxColumnStyleInfo.IsVisible = false;
			yield return sendWithMessageErrorsCheckBoxColumnStyleInfo;

			var systemCreateTimeUtcPlusEightDateEditColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			systemCreateTimeUtcPlusEightDateEditColumnStyleInfo.ColumnName = "SystemCreateTimeUtcPlusEight";
			systemCreateTimeUtcPlusEightDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			systemCreateTimeUtcPlusEightDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			yield return systemCreateTimeUtcPlusEightDateEditColumnStyleInfo;

			var systemLastEditTimeUtcDateEditColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			systemLastEditTimeUtcDateEditColumnStyleInfo.ColumnName = "EM_SystemLastEditTimeUtc";
			systemLastEditTimeUtcDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			systemLastEditTimeUtcDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			systemLastEditTimeUtcDateEditColumnStyleInfo.IsVisible = false;
			yield return systemLastEditTimeUtcDateEditColumnStyleInfo;

			var systemLastEditTimeUtcPlusEightDateEditColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			systemLastEditTimeUtcPlusEightDateEditColumnStyleInfo.ColumnName = "SystemLastEditTimeUtcPlusEight";
			systemLastEditTimeUtcPlusEightDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			systemLastEditTimeUtcPlusEightDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return systemLastEditTimeUtcPlusEightDateEditColumnStyleInfo;

			var systemLastEditUserTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			systemLastEditUserTextBoxColumnStyleInfo.ColumnName = "EM_SystemLastEditUser";
			systemLastEditUserTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return systemLastEditUserTextBoxColumnStyleInfo;

			var interchangeApplicationCodeTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			interchangeApplicationCodeTextBoxColumnStyleInfo.ColumnName = "InterchangeApplicationCode";
			interchangeApplicationCodeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return interchangeApplicationCodeTextBoxColumnStyleInfo;

			var interchangeSenderTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			interchangeSenderTextBoxColumnStyleInfo.ColumnName = "InterchangeSender";
			interchangeSenderTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			interchangeSenderTextBoxColumnStyleInfo.IsVisible = false;
			yield return interchangeSenderTextBoxColumnStyleInfo;

			var interchangeReceiverTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			interchangeReceiverTextBoxColumnStyleInfo.ColumnName = "InterchangeReceiver";
			interchangeReceiverTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			interchangeReceiverTextBoxColumnStyleInfo.IsVisible = false;
			yield return interchangeReceiverTextBoxColumnStyleInfo;

			var interchangeStatusTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			interchangeStatusTextBoxColumnStyleInfo.ColumnName = "InterchangeStatus";
			interchangeStatusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			interchangeStatusTextBoxColumnStyleInfo.IsVisible = false;
			yield return interchangeStatusTextBoxColumnStyleInfo;

			var interchangeNumberTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			interchangeNumberTextBoxColumnStyleInfo.ColumnName = "InterchangeNumber";
			interchangeNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return interchangeNumberTextBoxColumnStyleInfo;

			var interchangeDeliveredTimeDateTimeOffsetEditColumnStyleInfo = new ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			interchangeDeliveredTimeDateTimeOffsetEditColumnStyleInfo.ColumnName = "InterchangeDeliveredTime";
			interchangeDeliveredTimeDateTimeOffsetEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			interchangeDeliveredTimeDateTimeOffsetEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return interchangeDeliveredTimeDateTimeOffsetEditColumnStyleInfo;

			var interchangeCreateTimeDateEditColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			interchangeCreateTimeDateEditColumnStyleInfo.ColumnName = "InterchangeCreateTime";
			interchangeCreateTimeDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			interchangeCreateTimeDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			interchangeCreateTimeDateEditColumnStyleInfo.IsVisible = false;
			yield return interchangeCreateTimeDateEditColumnStyleInfo;

			var interchangeCreateTimePlusEightDateEditColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			interchangeCreateTimePlusEightDateEditColumnStyleInfo.ColumnName = "InterchangeCreateTimePlusEight";
			interchangeCreateTimePlusEightDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			interchangeCreateTimePlusEightDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			interchangeCreateTimePlusEightDateEditColumnStyleInfo.IsVisible = false;
			yield return interchangeCreateTimePlusEightDateEditColumnStyleInfo;

			var interchangeCreateUserTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			interchangeCreateUserTextBoxColumnStyleInfo.ColumnName = "InterchangeCreateUser";
			interchangeCreateUserTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			interchangeCreateUserTextBoxColumnStyleInfo.IsVisible = false;
			yield return interchangeCreateUserTextBoxColumnStyleInfo;

			var interchangeLastEditTimeDateEditColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			interchangeLastEditTimeDateEditColumnStyleInfo.ColumnName = "InterchangeLastEditTime";
			interchangeLastEditTimeDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			interchangeLastEditTimeDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			interchangeLastEditTimeDateEditColumnStyleInfo.IsVisible = false;
			yield return interchangeLastEditTimeDateEditColumnStyleInfo;

			var interchangeLastEditTimePlusEightDateEditColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			interchangeLastEditTimePlusEightDateEditColumnStyleInfo.ColumnName = "InterchangeLastEditTimePlusEight";
			interchangeLastEditTimePlusEightDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			interchangeLastEditTimePlusEightDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			interchangeLastEditTimePlusEightDateEditColumnStyleInfo.IsVisible = false;
			yield return interchangeLastEditTimePlusEightDateEditColumnStyleInfo;

			var interchangeLastEditUserTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			interchangeLastEditUserTextBoxColumnStyleInfo.ColumnName = "InterchangeLastEditUser";
			interchangeLastEditUserTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			interchangeLastEditUserTextBoxColumnStyleInfo.IsVisible = false;
			yield return interchangeLastEditUserTextBoxColumnStyleInfo;
		}

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new TWManifestLayouts();

		protected override IPanelLayoutProvider GetBillLayoutCore() => new TWBillLayouts();

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new TWBillPartiesLayouts();

		protected override IReadOnlyDictionary<bool, string[]> GetContainersGridColumnVisibilityCore()
		{
			return new Dictionary<bool, string[]>()
			{
				{ true, new[]
					{
						AsycudaContainer.Schema.ACN_Seal2,
						AsycudaContainer.Schema.ACN_Seal3
					}
				},
				{ false, new[]
					{
						AsycudaContainer.Schema.ACN_SealType1,
						AsycudaContainer.Schema.ACN_SealingPartyType,
						AsycudaContainer.Schema.ACN_SealingPartyName,
						AsycudaContainer.Schema.ACN_NumberOfPackages,
						AsycudaContainer.Schema.ACN_CommodityCode,
						AsycudaContainer.Schema.ACN_GoodsWeight,
						AsycudaContainer.Schema.ACN_GoodsWeightUQ,
						AsycudaContainer.Schema.ACN_StowageLocation,
						AsycudaContainer.Schema.ACN_Seal1UnloadingState,
						AsycudaContainer.Schema.ACN_Seal2UnloadingState,
						AsycudaContainer.Schema.ACN_Seal3UnloadingState,
					}
				},
			};
		}

		protected override IEnumerable<string> GetContainersGridColumnsOrderCore()
		{
			yield return AsycudaContainer.Schema.ACN_ContainerNumber;
			yield return AsycudaContainer.Schema.ACN_RC_ContainerType;
			yield return AsycudaContainer.Schema.ACN_EmptyFullIndicator;
			yield return AsycudaContainer.Schema.ACN_Seal1;
			yield return AsycudaContainer.Schema.ACN_Seal2;
			yield return AsycudaContainer.Schema.ACN_Seal3;
		}

		protected override IReadOnlyDictionary<string, int> GetContainersGridColumnsWidthCore()
		{
			return new Dictionary<string, int>()
			{
				{ AsycudaContainer.Schema.ACN_ContainerNumber, 130 },
				{ AsycudaContainer.Schema.ACN_EmptyFullIndicator, 130 },
				{ AsycudaContainer.Schema.ACN_RC_ContainerType, 130 },
				{ AsycudaContainer.Schema.ACN_Seal1, 125 },
				{ AsycudaContainer.Schema.ACN_Seal2, 125 },
				{ AsycudaContainer.Schema.ACN_Seal3, 125 }
			};
		}

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaContainerBillLinkUserControl();
		}
	}
}
