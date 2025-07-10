using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class GlbCompanyCampaignItemFilterControl : ZFilterStripControl
	{
		public GlbCompanyCampaignItemFilterControl(IBusinessObjectCollection collection, GlbCompanyCampaignItemFilterBusinessObject filterBusinessObject)
			: base(collection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (FilterBusinessObject.Campaign != null && (FilterBusinessObject.Campaign.IsTargetList || FilterBusinessObject.Campaign.IsMasterCampaign))
			{
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.TrackingStatusDescription);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.G8_DeliveryMethod);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.FollowedUpStaffName);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.G8_GS_NKFollowedUpBy);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.G8_FollowedUp);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.SenderStaffName);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.LastSentTime);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.G8_GS_NKSender);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.G8_GS_NKSender + "_DescriptionVirtual");
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.G8_SenderEmailAddress);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.LastCommunicationDate);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.LastCommunicationStaffCode);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.ScheduleTime);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.ScheduleTimeUtc);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.ScheduleTimeRecipientTime);
				grid.RemoveFromAvailableColumns(GlbCompanyCampaignItem.Schema.G8_BatchNumber);
			}

			if (FilterBusinessObject.Campaign != null && FilterBusinessObject.Campaign.IsHRCampaign)
			{
				grid.RemoveFromAvailableColumns("Recipient+Organisation+OH_Code", "RecipientOrgFullName");
			}

			AddTouchColumnsIfRequired();
		}

		void AddTouchColumnsIfRequired()
		{
			if (FilterBusinessObject.Campaign != null && FilterBusinessObject.Campaign.IsMasterCampaign)
			{
				foreach (var horizontal in FilterBusinessObject.Campaign.Horizontals)
				{
					ZTextBoxColumnStyleInfo columnInfo = GetColumnInfo(horizontal, FilterBusinessObject.Campaign);
					Grid.ColumnStyles.Add(columnInfo);

					if (Grid.Columns.Count > 0)
					{
						Grid.Columns.Add(columnInfo);
						Grid.SetAvailability(!columnInfo.IsUnavailable, horizontal.Name);
					}
				}
			}
		}

		ZTextBoxColumnStyleInfo GetColumnInfo(CampaignHorizontal horizontal, GlbCompanyCampaign masterCampaign)
		{
			var columnInfo = new ZTextBoxColumnStyleInfo();

			columnInfo.ColumnName = horizontal.Name;
			columnInfo.Caption = horizontal.Name;
			columnInfo.IsVisible = true;
			columnInfo.IsReadOnly = true;

			((IOverridablePropertyDescriptor)columnInfo).PropertyDescriptor = new TouchIdPropertyDescriptor(horizontal.Id, masterCampaign);

			return columnInfo;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new GlbCompanyCampaignItemFilterBusinessObject FilterBusinessObject
		{
			get { return (GlbCompanyCampaignItemFilterBusinessObject)base.FilterBusinessObject; }
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new CampaignItemFilterStrip();
		}

		protected override int MaximumAllowableQueriesPerSqlStatement => GlbCompanyCampaignItemModule.MaxDisplayRecords;

		internal void FindCampaignItemOnModule(IGlbCompanyCampaignItem campaignItem)
		{
			FilterBusinessObject.CampaignItem = campaignItem;
			Find();
			FilterBusinessObject.CampaignItem = null;
		}

		internal void FindCampaignItemWithDestinationURL(string destinationURL)
		{
			ResetFilterStrips();
			FilterBusinessObject.FilterStrips[0].FilterDescription = ((DestinationURLLinkActivityModuleFilter)FilterBusinessObject["Has Destination URL Activity"]).Description;
			((DestinationURLLinkActivityModuleFilter)FilterBusinessObject.FilterStrips[0].CurrentModuleFilter).TypeProperty = destinationURL;
			Find();
		}

		internal void FindCampaignItemWithContextName(string contextName)
		{
			ResetFilterStrips();
			FilterBusinessObject.FilterStrips[0].FilterDescription = ((ContextLinkActivityModuleFilter)FilterBusinessObject["Has Context Activity"]).Description;
			((ContextLinkActivityModuleFilter)FilterBusinessObject.FilterStrips[0].CurrentModuleFilter).TypeProperty = contextName;
			Find();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Default Filter name!")]
		internal void FindCampaignItemWithDeliveryStatus(string deliveryStatus)
		{
			ResetFilterStrips();
			if (deliveryStatus == TrackingStatusCodes.Codes.SCH || deliveryStatus == TrackingStatusCodes.Codes.UNS)
			{
				FilterBusinessObject.FilterStrips[0].FilterDescription = ((ModuleTextFilter)FilterBusinessObject["Tracking Status"])
					.Description;
				((ModuleTextFilter)FilterBusinessObject.FilterStrips[0].CurrentModuleFilter).Property = TrackingStatusCodes.Codes.QUE;

				var scheduleTime = FilterBusinessObject.FilterStrips.AddNew("Scheduled Time");

				var propertySearch = deliveryStatus == TrackingStatusCodes.Codes.SCH
					? ModuleDateFilter.HasDateEntered
					: ModuleDateFilter.HasNoDateEntered;

				((ModuleDateFilter)scheduleTime.CurrentModuleFilter).PropertySearch = propertySearch;

				AddFilterStrip(scheduleTime);
			}
			else
			{
				FilterBusinessObject.FilterStrips[0].FilterDescription =
					((ModuleTextFilter)FilterBusinessObject["Tracking Status"])
					.Description;
				((ModuleTextFilter)FilterBusinessObject.FilterStrips[0].CurrentModuleFilter).Property = deliveryStatus;
			}

			Find();
		}

		internal void FindCampaignItemWithUnsubscribeStatus(bool unsubscribeStatus)
		{
			ResetFilterStrips();
			FilterBusinessObject.FilterStrips[0].FilterDescription = ((ModuleFlagsFilter)FilterBusinessObject["Unsubscribed state"]).Description;
			((ModuleFlagsFilter)FilterBusinessObject.FilterStrips[0].CurrentModuleFilter).Property0 = unsubscribeStatus;
			Find();
		}

		internal void FindCampaignItemWithTransitionStatus(bool transitioned)
		{
			ResetFilterStrips();
			FilterBusinessObject.FilterStrips[0].FilterDescription = ((ModuleFlagsFilter)FilterBusinessObject["Transition Status"]).Description;
			((ModuleFlagsFilter)FilterBusinessObject.FilterStrips[0].CurrentModuleFilter).Property0 = transitioned;
			Find();
		}

		internal class TouchIdPropertyDescriptor : PropertyDescriptor
		{
			readonly int horizontalId;
			readonly GlbCompanyCampaign masterCampaign;

			public TouchIdPropertyDescriptor(int horizontalId, GlbCompanyCampaign masterCampaign) : base(horizontalId.ToString(CultureInfo.InvariantCulture), Array.Empty<Attribute>())
			{
				this.horizontalId = horizontalId;
				this.masterCampaign = masterCampaign;

				masterCampaign.SummaryStats.OnStatsChanged += delegate(IEnumerable<GlbCompanyCampaignItem> updated)
				{
					foreach (var item in updated)
					{
						OnValueChanged(item, EventArgs.Empty);
					}
				};
			}

			public override bool CanResetValue(object component)
			{
				return false;
			}

			public override object GetValue(object component)
			{
				var campaignItem = component as GlbCompanyCampaignItem;

				if (campaignItem != null)
				{
					var transitions = masterCampaign.SummaryStats.GetTransitionResultsForRecipient(campaignItem.G8_RecipientID.ToGuid());

					var transition = transitions.FirstOrDefault(t => t.HorizontalId == horizontalId);
					var sb = new ZStringBuilder();

					if (transition != null)
					{
						var glbCompanyCampaign = masterCampaign.AllTouches.FirstOrDefault(t => t.PK == transition.CampaignId);

						if (glbCompanyCampaign != null)
						{
							if (transition.TrackingStatus == TrackingStatusCodes.Codes.OPQ || transition.TrackingStatus == TrackingStatusCodes.Codes.OPC)
							{
								sb.Append(InsideSalesTouchTypeList.Codes.OpportunityCreation);
								sb.Append(string.Format(CultureInfo.InvariantCulture, "({0})", transition.TrackingStatus));
								sb.Append(string.Format(CultureInfo.InvariantCulture, " - {0}", glbCompanyCampaign.G0_VerticalId));
								return sb.ToString();
							}

							var status = transition.IsBlocked
								? TrackingSummaryConstants.Codes.DND
								: transition.TrackingStatus == TrackingStatusCodes.Codes.QUE
									? transition.IsSuspended
										? TrackingSummaryConstants.Codes.SUS
										: TrackingStatusCodes.Codes.QUE
									: TrackingSummaryConstants.Codes.SNT;

							sb.Append(status);
							if (status == TrackingSummaryConstants.Codes.SNT)
							{
								sb.Append(string.Format(CultureInfo.InvariantCulture, "({0})", transition.TrackingStatus));
							}

							sb.Append(string.Format(CultureInfo.InvariantCulture, " - {0}", glbCompanyCampaign.G0_VerticalId));
						}
					}
					else
					{
						CampaignTransitionResults prevTransition = null;
						for (var i = horizontalId - 1; i >= 0; i--)
						{
							prevTransition = transitions.FirstOrDefault(t => t.HorizontalId == i);
							if (prevTransition != null)
							{
								break;
							}
						}

						if (prevTransition != null)
						{
							var status = prevTransition.IsBlocked
								? TrackingSummaryConstants.Codes.DND
								: prevTransition.IsSuspended
									? TrackingSummaryConstants.Codes.SUS
									: prevTransition.TrackingStatus == TrackingStatusCodes.Codes.QUE
										? string.Empty
										: TrackingSummaryConstants.Codes.FAI;

							sb.Append(status);
						}
					}

					return sb.ToString();
				}

				return string.Empty;
			}

			public override void ResetValue(object component)
			{
				throw new NotImplementedException();
			}

			public override void SetValue(object component, object value)
			{
				throw new NotImplementedException();
			}

			public override bool ShouldSerializeValue(object component)
			{
				return false;
			}

			public override Type ComponentType => typeof(GlbCompanyCampaignItem);

			public override bool IsReadOnly => true;

			public override Type PropertyType => typeof(string);
		}
	}
}
