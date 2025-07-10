using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.ZA.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using AsycudaContainer = Enterprise.Customs.ZA.Manifest.Business.AsycudaContainer;
using AsycudaManifestHeader = Enterprise.Customs.ZA.Manifest.Business.AsycudaManifestHeader;
using CusPerson = Enterprise.Customs.ZA.Manifest.Business.CusPerson;

namespace Enterprise.Customs.ZA.Manifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(Business.ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IEnumerable<ZGridColumnInfo> GetPersonGridExtraColumnInfosCore()
		{
			yield return new ZDropEditColumnStyleInfo
			{
				ColumnName = CusPerson.Schema.OccupationInZA,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(77)
			};

			yield return new ZDropEditColumnStyleInfo
			{
				ColumnName = CusPerson.Schema.TravellerTypeInZA,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92)
			};

			yield return new ZDropEditColumnStyleInfo
			{
				ColumnName = CusPerson.Schema.ReasonForMovementInZA,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(129)
			};

			yield return new ZDropEditColumnStyleInfo
			{
				ColumnName = CusPerson.Schema.TravelDocumentTypeInZA,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133)
			};
		}

		protected override IReadOnlyDictionary<bool, string[]> GetContainersGridColumnVisibilityCore()
		{
			return new Dictionary<bool, string[]>
			{
				{
					true,
					new []
					{
						AsycudaContainer.Schema.ACN_ContainerNumber,
						AsycudaContainer.Schema.LandedPurpose,
						AsycudaContainer.Schema.ACN_EmptyFullIndicator,
						AsycudaContainer.Schema.ACN_RC_ContainerType,
						AsycudaContainer.Schema.ACN_Seal1,
						AsycudaContainer.Schema.ACN_SealType1,
						AsycudaContainer.Schema.ACN_SealingPartyType,
						AsycudaContainer.Schema.ACN_Seal2,
						AsycudaContainer.Schema.ACN_SealType2,
						AsycudaContainer.Schema.ACN_SealingPartyType2,
						AsycudaContainer.Schema.ACN_Seal3,
						AsycudaContainer.Schema.ACN_SealType3,
						AsycudaContainer.Schema.ACN_SealingPartyType3,
						AsycudaContainer.Schema.ACN_SealingPartyName,
						AsycudaContainer.Schema.ACN_NumberOfPackages,
						AsycudaContainer.Schema.ACN_CommodityCode,
						AsycudaContainer.Schema.ACN_GoodsWeight,
						AsycudaContainer.Schema.ACN_GoodsWeightUQ,
						AsycudaContainer.Schema.ACN_StowageLocation
					}
				},
				{
					false,
					new []
					{
						AsycudaContainer.Schema.ACN_Seal1UnloadingState,
						AsycudaContainer.Schema.ACN_Seal2UnloadingState,
						AsycudaContainer.Schema.ACN_Seal3UnloadingState,
					}
				}
			};
		}

		protected override IEnumerable<ZMenuItem> GetMessagesGridExtraMenuItemsCore(ZGrid messagesGrid, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var requestCustomsResendOfResponsesMenuItem = new ZMenuItem(RequestCustomsResendOfResponsesCaption);
			var latestResponseCaptionMenuItem = new ZMenuItem(LatestResponseCaption);
			requestCustomsResendOfResponsesMenuItem.MenuItems.Add(latestResponseCaptionMenuItem);

			latestResponseCaptionMenuItem.Click += (s, e) =>
			{
				if (header is AsycudaManifestHeader zaManifest
						&& messagesGrid.GetCurrent() is CUSCAREDIMessage selectedMessage)
				{
					var notification = new MessageNotificationCollector() as IMessageNotificationCollector;
					Business.MessagingProvider.RequestLatestResponse(zaManifest, selectedMessage, notification);
					var notifications = notification.Notifications;
					if (notifications.ContainsError())
					{
						Globals.Message.ShowError(notifications.ErrorNotificationsAsString(), Res.GetString("825756F0-7E91-4474-94D0-62F591D1B320", "Error in Requesting Latest Response"));
					}
					if (notifications.ContainsInformation())
					{
						Globals.Message.ShowInformation(notifications.InformationNotificationsAsString(), Res.GetString("40FE4FFC-6B5C-4641-9B4C-A509241B2A31", "Request Latest Response Result"));
					}
				}
			};

			yield return requestCustomsResendOfResponsesMenuItem;
		}

		ResourceString RequestCustomsResendOfResponsesCaption => ResString.GetMultilingualString("07FED08C-5983-4A20-8A72-37D07461A10C", "Request Customs Resend of Responses");
		ResourceString LatestResponseCaption => ResString.GetMultilingualString("D791241F-0D39-4B5A-BE00-2D601F496661", "Latest Response");

		protected override void SetMessagesGridExtraMenuItemsVisibilityCore(ZMenuItem[] messagesGridExtraMenuItems, EDIMessage message)
		{
			var requestCustomsResendOfResponsesMenuItem = messagesGridExtraMenuItems.FirstOrDefault(x => x.Text == RequestCustomsResendOfResponsesCaption.EnglishText);
			if (requestCustomsResendOfResponsesMenuItem != null)
			{
				var isTransmitCusCarMessage = message is CUSCAREDIMessage cusCar && cusCar.IsTransmitMessage;
				requestCustomsResendOfResponsesMenuItem.Visible = isTransmitCusCarMessage;
				foreach (MenuItem menuItem in requestCustomsResendOfResponsesMenuItem.MenuItems)
				{
					menuItem.Visible = isTransmitCusCarMessage;
				}
			}
		}

		protected override IPanelLayoutProvider GetManifestLayoutCore()
		{
			return new ZAManifestLayouts();
		}

		protected override IPanelLayoutProvider GetBillLayoutCore()
		{
			return new ZABillLayouts();
		}

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
		}

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new ZADefaultBillPartiesLayouts();

		protected override IEnumerable<ZGridColumnInfo> GetContainersGridExtraColumnInfosCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			yield return new ZDropEditColumnStyleInfo
			{
				ColumnName = AsycudaContainer.Schema.LandedPurpose,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				IsMandatory = true,
			};
		}

		protected override IEnumerable<string> GetContainersGridColumnsOrderCore()
		{
			yield return AsycudaContainer.Schema.ACN_ContainerNumber;
			yield return AsycudaContainer.Schema.LandedPurpose;
			yield return AsycudaContainer.Schema.ACN_EmptyFullIndicator;
			yield return AsycudaContainer.Schema.ACN_RC_ContainerType;
			yield return AsycudaContainer.Schema.ACN_Seal1;
			yield return AsycudaContainer.Schema.ACN_SealType1;
			yield return AsycudaContainer.Schema.ACN_SealingPartyType;
			yield return AsycudaContainer.Schema.ACN_Seal2;
			yield return AsycudaContainer.Schema.ACN_SealType2;
			yield return AsycudaContainer.Schema.ACN_SealingPartyType2;
			yield return AsycudaContainer.Schema.ACN_Seal3;
			yield return AsycudaContainer.Schema.ACN_SealType3;
			yield return AsycudaContainer.Schema.ACN_SealingPartyType3;
			yield return AsycudaContainer.Schema.ACN_SealingPartyName;
			yield return AsycudaContainer.Schema.ACN_NumberOfPackages;
			yield return AsycudaContainer.Schema.ACN_CommodityCode;
			yield return AsycudaContainer.Schema.ACN_GoodsWeight;
			yield return AsycudaContainer.Schema.ACN_GoodsWeightUQ;
			yield return AsycudaContainer.Schema.ACN_StowageLocation;
		}
	}
}
