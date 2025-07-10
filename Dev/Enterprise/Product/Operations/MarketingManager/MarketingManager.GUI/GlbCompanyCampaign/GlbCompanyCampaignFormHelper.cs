using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MarketingManager.GUI
{
	public static class GlbCompanyCampaignFormHelper
	{
		public static void FocusOnCampaignItem<T>(CampaignTrackingControl campaignTrackingControl, FocusOnTrackingTabTypes focusType, T value)
		{
			switch (focusType)
			{
				case FocusOnTrackingTabTypes.CampaignItem:
					campaignTrackingControl.FindCampaignItemOnTrackingTab((IGlbCompanyCampaignItem)value);
					return;
				case FocusOnTrackingTabTypes.DestinationUrl:
					campaignTrackingControl.FindCampaignItemWithDestinationURL(value.ToString());
					return;
				case FocusOnTrackingTabTypes.ContextName:
					campaignTrackingControl.FindCampaignItemWithContextName(value.ToString());
					return;
				case FocusOnTrackingTabTypes.StatusDescription:
					TrackingStatusCodes statuses = new TrackingStatusCodes();
					var valueToString = value.ToString();
					var status = statuses.GetCodeFromDescription(valueToString);
					var statusIsCode = statuses.ContainsCode(valueToString);

					if (!string.IsNullOrEmpty(status))
					{
						campaignTrackingControl.FindCampaignItemWithDeliveryStatus(status);
					}
					else if (statusIsCode)
					{
						campaignTrackingControl.FindCampaignItemWithDeliveryStatus(valueToString);
					}
					else
					{
						status = new TrackingSummaryConstants().GetCodeFromDescription(valueToString);
						if (status == TrackingSummaryConstants.Codes.TRN)
						{
							campaignTrackingControl.FindCampaignItemWithTransitionStatus(true);
						}
						else if (status == TrackingSummaryConstants.Codes.FAI)
						{
							campaignTrackingControl.FindCampaignItemWithTransitionStatus(false);
						}
					}
					return;
				case FocusOnTrackingTabTypes.UnsubscribeStatus:
					campaignTrackingControl.FindCampaignItemWithUnsubscribeStatus((ZBool)Convert.ChangeType(value, typeof(ZBool), CultureInfo.InvariantCulture));
					return;
				case FocusOnTrackingTabTypes.CampaignItemList:
					campaignTrackingControl.DisplayCampaignItems(value as GlbCompanyCampaignItemCampaignDependentCollection);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(focusType), focusType, null);
			}
		}
	}

	public enum FocusOnTrackingTabTypes
	{
		CampaignItem = 0,
		DestinationUrl = 1,
		StatusDescription = 2,
		ContextName = 3,
		CampaignItemList = 4,
		UnsubscribeStatus = 5
	}
}
