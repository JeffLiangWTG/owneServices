using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingContainerColumnProvider : GridColumnProvider
	{
		public const string ViewContainerCommand = "ViewContainer";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			if (WebEnv.AppInstance != null)
			{
				TrackingSiteUser siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;

				ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).ContainerNumber);

				if (siteUser != null && siteUser.IsShipmentQuickViewUser && !WebDataRegistry.Instance.WebTrackerContainerQuickView.Value)
				{
					AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("64dfaffe-d618-4acf-a738-219424cc3f2e", "Container #"), TrackingContainer.Schema.ContainerNumber) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerNumber });
				}
				else if (siteUser != null && siteUser.IsShipmentQuickViewUser)
				{
					ZHyperLinkColumn containerNumberColumn1 = new ZHyperLinkColumn(Res.GetString("64dfaffe-d618-4acf-a738-219424cc3f2e", "Container #"), TrackingContainer.Schema.ContainerNumber) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerNumber };
					containerNumberColumn1.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.ContainerDetailsPage) + (NoResString)"?Ref={0}&ContainerQuickViewNumber={1}"; // Part of URL string
					containerNumberColumn1.DataNavigateUrlFields = new string[2] { "PK", TrackingContainer.Schema.ContainerNumber };
					AddToDictionaryAsRequired(containerNumberColumn1);
				}
				else
				{
					ZHyperLinkColumn containerNumberColumn1 = new ZHyperLinkColumn(Res.GetString("64dfaffe-d618-4acf-a738-219424cc3f2e", "Container #"), TrackingContainer.Schema.ContainerNumber) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerNumber };
					containerNumberColumn1.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.ContainerDetailsPage) + (NoResString)"?Ref={0}"; // Part of URL string
					containerNumberColumn1.DataNavigateUrlFields = new string[1] { "PK" };
					AddToDictionaryAsRequired(containerNumberColumn1);
				}

				if (siteUser != null && !siteUser.IsShipmentQuickViewUser)
				{
					ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).ShipmentNumbers);
					AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("2f261f9d-e582-4581-b1a6-6215d8d2951f", "Shipment #"), TrackingContainer.Schema.ShipmentNumbers) { ColumnKey = WebTracker.Grids.TrackingContainers.ShipmentNumber });
				}
			}

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("d201bf72-5ea0-4dd2-bbe1-402ec7d57adb", "Seal #"), JobContainerSchema.JC_SealNum.Name) { ColumnKey = WebTracker.Grids.TrackingContainers.SealNumber });

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).Type);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b2f04e57-70c0-46c3-937e-f99245ebbdb1", "Container Type"), TrackingContainer.Schema.Type) { ColumnKey = WebTracker.Grids.TrackingContainers.Type });

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).Mode);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("4950fb0d-a88a-44c6-b3a9-8e72215e3bea", "Container Mode"), TrackingContainer.Schema.Mode) { ColumnKey = WebTracker.Grids.TrackingContainers.Mode });

			ZBindToChecker.CheckBindTo((ZInt)((TrackingContainer)null).Packs);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("8f83decc-4cf1-4627-a370-8f8ac88b2e10", "Packages"), TrackingContainer.Schema.Packs) { ColumnKey = WebTracker.Grids.TrackingContainers.Packs });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).Departure);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("208e8cdf-f131-4af0-b94d-3b569223fe02", "Departure"), TrackingContainer.Schema.Departure, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.Departure });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).Arrival);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("944ec894-c0f5-4e95-aad1-c9372363314d", "Arrival"), TrackingContainer.Schema.Arrival, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.Arrival });

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).QuarantineCode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("038ed7eb-9b53-4db3-a63e-308895d4f3d1", "Quarantine"), TrackingContainer.Schema.QuarantineCode) { ColumnKey = WebTracker.Grids.TrackingContainers.Quarantine });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).Available);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("c69e031e-f26e-4a4e-9f42-686bb6fff0be", "Available"), TrackingContainer.Schema.Available, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.Available });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).JC_LastFreeDay);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("6158bd3c-2306-480c-b8db-b1fb352d0860", "Last Free Day"), TrackingContainer.Schema.JC_LastFreeDay, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.LastFreeDay });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).EmptyReturnRequired);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("f2978f70-d574-4dc2-ae23-4de311c4c0f1", "Detention Starts"), TrackingContainer.Schema.EmptyReturnRequired, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.DetentionStarts });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).SlotDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("22a234f1-003f-4783-a1c9-91f789914bda", "Time Slot"), TrackingContainer.Schema.SlotDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.TimeSlot });

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).JC_DepartureCartageRef);
			AddToDictionary(new ZTextEditColumn(Res.GetString("08735a6c-54fe-4ac8-821c-7b9c4fec3334", "Port Transport Ref"), TrackingContainer.Schema.JC_DepartureCartageRef) { ColumnKey = WebTracker.Grids.TrackingContainers.LocalTransportReference });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).RequiredDelivery);
			AddToDictionary(new ZDateTimeStatusColumn(Res.GetString("d53c403e-898b-41f0-b029-b7fdf1ac26d1", "Estimated Full Delivery"), TrackingContainer.Schema.RequiredDelivery, TrackingContainer.Schema.RequiredDeliveryStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.RequiredDelivery });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).ConfirmedDelivery);
			AddToDictionary(new ZDateTimeStatusColumn(Res.GetString("0eb6c81b-38ec-45ab-b472-73c2eae97b9d", "Transport Booked"), TrackingContainer.Schema.ConfirmedDelivery, TrackingContainer.Schema.ConfirmedDeliveryStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.ConfirmedDelivery });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).ActualDelivery);
			AddToDictionary(new ZDateTimeStatusColumn(Res.GetString("46330e3c-11d0-481d-9e11-615020d8feb9", "Actual Delivery"), TrackingContainer.Schema.ActualDelivery, TrackingContainer.Schema.ActualDeliveryStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.ActualDelivery });

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).Consignees);
			AddToDictionary(new ZTextEditColumn(Res.GetString("9a2569f7-23bf-4dfc-9a1e-b96691c769cd", "Deliver"), TrackingContainer.Schema.Consignees) { ColumnKey = WebTracker.Grids.TrackingContainers.Deliver });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).EmptyReady);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("c7f1840e-0ebf-4a50-88a9-b55c6f67003f", "Empty Ready for Return"), TrackingContainer.Schema.EmptyReady, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.EmptyReady });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).EmptyPickup);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("b60c1ba7-0d3a-4dab-9788-4ec8b19fc4c9", "Empty Return By"), TrackingContainer.Schema.EmptyPickup, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.EmptyPickup });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).ActualDehire);
			AddToDictionaryAsDefault(new ZDateTimeStatusColumn(Res.GetString("9e1c4d7a-01d5-47c6-be27-b323a141edec", "Empty Returned On"), TrackingContainer.Schema.ActualDehire, TrackingContainer.Schema.ActualDehireStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.ActualDehire });

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).VesselName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("da116615-d69a-427e-9ebb-4a8812400cb1", "Vessel Name"), TrackingContainer.Schema.VesselName) { ColumnKey = WebTracker.Grids.TrackingContainers.Vessel });

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).Voyage);
			AddToDictionary(new ZTextEditColumn(Res.GetString("6385752c-bf58-48b4-9971-1a969fd8b317", "Voyage No"), TrackingContainer.Schema.Voyage) { ColumnKey = WebTracker.Grids.TrackingContainers.Voyage });

			ZBindToChecker.CheckBindTo((ZShort)((TrackingContainer)null).JC_DeliverySequence);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("58734cc8-13c8-4649-a4fd-97acd246f003", "Delivery Sequence"), TrackingContainer.Schema.JC_DeliverySequence) { ColumnKey = WebTracker.Grids.TrackingContainers.DeliverySequence });

			if (WebDataRegistry.Instance.ShowContainerStatusFromShipment.Value)
			{
				ZString customText1Label = FreightDataRegistry.Instance.ShipmentCustomText1.Value.Caption;

				if (!customText1Label.IsEmpty)
				{
					AddToDictionary(new ZTextEditColumn(customText1Label, TrackingContainer.Schema.ShipmentStatuses) { ColumnKey = WebTracker.Grids.TrackingContainers.ShipmentStatuses });
				}
			}

			ZBindToChecker.CheckBindTo((TrackingMilestoneCollection)((TrackingContainer)null).Milestones);
			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // Data column name
			{
				AddToDictionary(column);
			}

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).StatusDescription);
			AddToDictionary(new ZTextEditColumn(Res.GetString("ee80b85a-b01a-49af-8c2a-61847166769d", "Status"), TrackingContainer.Schema.StatusDescription) { ColumnKey = WebTracker.Grids.TrackingContainers.StatusDescription });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).StorageBegins);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("c130a47b-e8f7-43e9-a1e2-376afc4968fe", "Storage Begins"), TrackingContainer.Schema.StorageBegins, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.StorageBegins });

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).ContainerStatus);
			AddToDictionary(new ZTextEditColumn(Res.GetString("0dd23dd0-5ae3-4c48-afb5-aaaeb6ac16de", "Container Status"), TrackingContainer.Schema.ContainerStatus) { ColumnKey = WebTracker.Grids.TrackingContainers.ContainerStatus });

			ZBindToChecker.CheckBindTo(((TrackingContainer)null).WeightWithUnits);
			AddToDictionary(new ZTextEditColumn(Res.GetString("91B0DD8C-023C-42D7-AF99-F3627208413C", "Verified Weight"), TrackingContainer.Schema.WeightWithUnits) { ColumnKey = WebTracker.Grids.TrackingContainers.GrossWeight });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingContainer)null).JC_GrossWeightVerificationDateTime);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("5A00CE29-590F-4FAB-A6E5-94C1F5520444", "Verified Date"), TrackingContainer.Schema.JC_GrossWeightVerificationDateTime, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingContainers.VerifiedDate });

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).VerifiedMethod);
			AddToDictionary(new ZTextEditColumn(Res.GetString("FA2831A3-3B6C-4DB2-97AA-BFD60FC4995A", "Verified Method"), TrackingContainer.Schema.VerifiedMethod) { ColumnKey = WebTracker.Grids.TrackingContainers.VerifiedMethod });

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).VerifiedByCompany);
			AddToDictionary(new ZTextEditColumn(Res.GetString("7BDD443F-24EF-44D8-847D-80322A4D9A3B", "Verified Company"), TrackingContainer.Schema.VerifiedByCompany) { ColumnKey = WebTracker.Grids.TrackingContainers.VerifiedCompany });

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).VerifiedByPerson);
			AddToDictionary(new ZTextEditColumn(Res.GetString("7D74D9C3-355C-4B38-88C8-68E402148DA5", "Verified Contact"), TrackingContainer.Schema.VerifiedByPerson) { ColumnKey = WebTracker.Grids.TrackingContainers.VerifiedContact });

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).VerifiedByPhone);
			AddToDictionary(new ZTextEditColumn(Res.GetString("173D149B-B5A7-465A-BDC5-1644DC92BCCF", "Verified Phone"), TrackingContainer.Schema.VerifiedByPhone) { ColumnKey = WebTracker.Grids.TrackingContainers.VerifiedPhone });

			ZBindToChecker.CheckBindTo((ZString)((TrackingContainer)null).VerifiedByEmail);
			AddToDictionary(new ZTextEditColumn(Res.GetString("826FA30A-FB52-4265-85B5-D68EDB105B43", "Verified Email"), TrackingContainer.Schema.VerifiedByEmail) { ColumnKey = WebTracker.Grids.TrackingContainers.VerifiedEmail });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingContainers.ContainerNumber);
			result.Add((int)WebTracker.Grids.TrackingContainers.ShipmentNumber);
			result.Add((int)WebTracker.Grids.TrackingContainers.Type);
			result.Add((int)WebTracker.Grids.TrackingContainers.Mode);
			result.Add((int)WebTracker.Grids.TrackingContainers.Packs);
			result.Add((int)WebTracker.Grids.TrackingContainers.Departure);
			result.Add((int)WebTracker.Grids.TrackingContainers.Arrival);
			result.Add((int)WebTracker.Grids.TrackingContainers.Quarantine);
			result.Add((int)WebTracker.Grids.TrackingContainers.Available);
			result.Add((int)WebTracker.Grids.TrackingContainers.LastFreeDay);
			result.Add((int)WebTracker.Grids.TrackingContainers.DetentionStarts);
			result.Add((int)WebTracker.Grids.TrackingContainers.TimeSlot);
			result.Add((int)WebTracker.Grids.TrackingContainers.LocalTransportReference);
			result.Add((int)WebTracker.Grids.TrackingContainers.RequiredDelivery);
			result.Add((int)WebTracker.Grids.TrackingContainers.ConfirmedDelivery);
			result.Add((int)WebTracker.Grids.TrackingContainers.ActualDelivery);
			result.Add((int)WebTracker.Grids.TrackingContainers.Deliver);
			result.Add((int)WebTracker.Grids.TrackingContainers.EmptyReady);
			result.Add((int)WebTracker.Grids.TrackingContainers.EmptyPickup);
			result.Add((int)WebTracker.Grids.TrackingContainers.ActualDehire);
			result.Add((int)WebTracker.Grids.TrackingContainers.Vessel);
			result.Add((int)WebTracker.Grids.TrackingContainers.Voyage);
			result.Add((int)WebTracker.Grids.TrackingContainers.DeliverySequence);
			if (WebDataRegistry.Instance.ShowContainerStatusFromShipment.Value)
			{
				ZString customText1Label = FreightDataRegistry.Instance.ShipmentCustomText1.Value.Caption;

				if (!customText1Label.IsEmpty)
				{
					result.Add((int)WebTracker.Grids.TrackingContainers.ShipmentStatuses);
				}
			}

#pragma warning disable IDE0004 // Remove Unnecessary Cast Justification = "ZBindToChecker requires a redundant cast"
			ZBindToChecker.CheckBindTo((TrackingMilestoneCollection)((TrackingContainer)null).Milestones);
#pragma warning restore IDE0004 // Remove Unnecessary Cast

			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // Data column name
			{
				result.Add(column.UniqueKey);
			}
			result.Add((int)WebTracker.Grids.TrackingContainers.StatusDescription);
			return result;
		}
	}
}
