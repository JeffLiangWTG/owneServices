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
	public class LinerAndAgencyContainerColumnProvider : GridColumnProvider
	{
		public const string ViewContainerCommand = "ViewContainer";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			if (WebEnv.AppInstance != null)
			{
				var siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;

				ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).ContainerNumber);

				if (siteUser != null)
				{
					if (siteUser.IsShipmentQuickViewUser)
					{
						if (!WebDataRegistry.Instance.WebTrackerContainerQuickView.Value)
						{
							AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("64dfaffe-d618-4acf-a738-219424cc3f2e", "Container #"), LinerAndAgencyContainer.Schema.ContainerNumber) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ContainerNumber });
						}
						else
						{
							var containerNumberColumn1 = new ZHyperLinkColumn(Res.GetString("64dfaffe-d618-4acf-a738-219424cc3f2e", "Container #"), LinerAndAgencyContainer.Schema.ContainerNumber) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ContainerNumber };
							containerNumberColumn1.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.LinerAndAgencyContainerDetailsPage) + (NoResString)"?Ref={0}&ContainerQuickViewNumber={1}"; // Part of URL string
							containerNumberColumn1.DataNavigateUrlFields = new string[2] { "PK", LinerAndAgencyContainer.Schema.ContainerNumber };
							AddToDictionaryAsRequired(containerNumberColumn1);
						}
					}
					else
					{
						var containerNumberColumn1 = new ZHyperLinkColumn(Res.GetString("64dfaffe-d618-4acf-a738-219424cc3f2e", "Container #"), LinerAndAgencyContainer.Schema.ContainerNumber) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ContainerNumber };
						containerNumberColumn1.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.LinerAndAgencyContainerDetailsPage) + (NoResString)"?Ref={0}"; // Part of URL string
						containerNumberColumn1.DataNavigateUrlFields = new string[1] { "PK" };
						AddToDictionaryAsRequired(containerNumberColumn1);

						ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).ShipmentNumbers);
						AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("2f261f9d-e582-4581-b1a6-6215d8d2951f", "Shipment #"), LinerAndAgencyContainer.Schema.ShipmentNumbers) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ShipmentNumber });
					}
				}
				else
				{
					var containerNumberColumn1 = new ZHyperLinkColumn(Res.GetString("64dfaffe-d618-4acf-a738-219424cc3f2e", "Container #"), LinerAndAgencyContainer.Schema.ContainerNumber) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ContainerNumber };
					containerNumberColumn1.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.LinerAndAgencyContainerDetailsPage) + (NoResString)"?Ref={0}"; // Part of URL string
					containerNumberColumn1.DataNavigateUrlFields = new string[1] { "PK" };
					AddToDictionaryAsRequired(containerNumberColumn1);
				}
			}

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("d201bf72-5ea0-4dd2-bbe1-402ec7d57adb", "Seal #"), JobContainerSchema.JC_SealNum.Name) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.SealNumber });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).TypeDescription);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b2f04e57-70c0-46c3-937e-f99245ebbdb1", "Container Type"), LinerAndAgencyContainer.Schema.TypeDescription) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.TypeDescription });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).Mode);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("4950fb0d-a88a-44c6-b3a9-8e72215e3bea", "Container Mode"), LinerAndAgencyContainer.Schema.Mode) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Mode });

			ZBindToChecker.CheckBindTo((ZInt)((LinerAndAgencyContainer)null).Packs);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("8f83decc-4cf1-4627-a370-8f8ac88b2e10", "Packages"), LinerAndAgencyContainer.Schema.Packs) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Packs });

			ZBindToChecker.CheckBindTo((ZDateTime)((LinerAndAgencyContainer)null).RequiredDelivery);
			AddToDictionary(new ZDateTimeStatusColumn(Res.GetString("d53c403e-898b-41f0-b029-b7fdf1ac26d1", "Estimated Full Delivery"), LinerAndAgencyContainer.Schema.RequiredDelivery, LinerAndAgencyContainer.Schema.RequiredDeliveryStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.RequiredDelivery });

			ZBindToChecker.CheckBindTo((ZDateTime)((LinerAndAgencyContainer)null).ActualDelivery);
			AddToDictionary(new ZDateTimeStatusColumn(Res.GetString("46330e3c-11d0-481d-9e11-615020d8feb9", "Actual Delivery"), LinerAndAgencyContainer.Schema.ActualDelivery, LinerAndAgencyContainer.Schema.ActualDeliveryStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ActualDelivery });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).ConsigneesExtended);
			AddToDictionary(new ZTextEditColumn(Res.GetString("9a2569f7-23bf-4dfc-9a1e-b96691c769cd", "Deliver"), LinerAndAgencyContainer.Schema.ConsigneesExtended) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Deliver });

			ZBindToChecker.CheckBindTo((ZDateTime)((LinerAndAgencyContainer)null).EmptyReady);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("c7f1840e-0ebf-4a50-88a9-b55c6f67003f", "Empty Ready for Return"), LinerAndAgencyContainer.Schema.EmptyReady, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyReady });

			ZBindToChecker.CheckBindTo((ZDateTime)((LinerAndAgencyContainer)null).EmptyReturnRequired);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("b60c1ba7-0d3a-4dab-9788-4ec8b19fc4c9", "Empty Return By"), LinerAndAgencyContainer.Schema.EmptyReturnRequired, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.EmptyReturnRequired });

			ZBindToChecker.CheckBindTo((ZDateTime)((LinerAndAgencyContainer)null).ActualDehire);
			AddToDictionary(new ZDateTimeStatusColumn(Res.GetString("9e1c4d7a-01d5-47c6-be27-b323a141edec", "Empty Returned On"), LinerAndAgencyContainer.Schema.ActualDehire, LinerAndAgencyContainer.Schema.ActualDehireStatus, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ActualDehire });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).VesselName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("da116615-d69a-427e-9ebb-4a8812400cb1", "Vessel Name"), LinerAndAgencyContainer.Schema.VesselName) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Vessel });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).Voyage);
			AddToDictionary(new ZTextEditColumn(Res.GetString("6385752c-bf58-48b4-9971-1a969fd8b317", "Voyage No"), LinerAndAgencyContainer.Schema.Voyage) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.Voyage });

			ZBindToChecker.CheckBindTo((TrackingMilestoneCollection)((LinerAndAgencyContainer)null).Milestones);
			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // Data column name
			{
				AddToDictionary(column);
			}

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).ContainerStatus);
			AddToDictionary(new ZTextEditColumn(Res.GetString("0dd23dd0-5ae3-4c48-afb5-aaaeb6ac16de", "Container Status"), LinerAndAgencyContainer.Schema.ContainerStatus) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.ContainerStatus });

			ZBindToChecker.CheckBindTo(((LinerAndAgencyContainer)null).WeightWithUnits);
			AddToDictionary(new ZTextEditColumn(Res.GetString("91B0DD8C-023C-42D7-AF99-F3627208413C", "Verified Weight"), LinerAndAgencyContainer.Schema.WeightWithUnits) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.GrossWeight });

			ZBindToChecker.CheckBindTo((ZDateTime)((LinerAndAgencyContainer)null).JC_GrossWeightVerificationDateTime);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("5A00CE29-590F-4FAB-A6E5-94C1F5520444", "Verified Date"), LinerAndAgencyContainer.Schema.JC_GrossWeightVerificationDateTime, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedDate });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).VerifiedMethod);
			AddToDictionary(new ZTextEditColumn(Res.GetString("FA2831A3-3B6C-4DB2-97AA-BFD60FC4995A", "Verified Method"), LinerAndAgencyContainer.Schema.VerifiedMethod) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedMethod });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).VerifiedByCompany);
			AddToDictionary(new ZTextEditColumn(Res.GetString("7BDD443F-24EF-44D8-847D-80322A4D9A3B", "Verified Company"), LinerAndAgencyContainer.Schema.VerifiedByCompany) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedCompany });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).VerifiedByPerson);
			AddToDictionary(new ZTextEditColumn(Res.GetString("7D74D9C3-355C-4B38-88C8-68E402148DA5", "Verified Contact"), LinerAndAgencyContainer.Schema.VerifiedByPerson) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedContact });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).VerifiedByPhone);
			AddToDictionary(new ZTextEditColumn(Res.GetString("173D149B-B5A7-465A-BDC5-1644DC92BCCF", "Verified Phone"), LinerAndAgencyContainer.Schema.VerifiedByPhone) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedPhone });

			ZBindToChecker.CheckBindTo((ZString)((LinerAndAgencyContainer)null).VerifiedByEmail);
			AddToDictionary(new ZTextEditColumn(Res.GetString("826FA30A-FB52-4265-85B5-D68EDB105B43", "Verified Email"), LinerAndAgencyContainer.Schema.VerifiedByEmail) { ColumnKey = WebTracker.Grids.LinerAndAgencyContainers.VerifiedEmail });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			var result = new List<int>();
			result.Add((int)WebTracker.Grids.LinerAndAgencyContainers.ContainerNumber);
			result.Add((int)WebTracker.Grids.LinerAndAgencyContainers.ShipmentNumber);
			result.Add((int)WebTracker.Grids.LinerAndAgencyContainers.TypeDescription);
			result.Add((int)WebTracker.Grids.LinerAndAgencyContainers.Mode);
			result.Add((int)WebTracker.Grids.LinerAndAgencyContainers.Packs);
			result.Add((int)WebTracker.Grids.LinerAndAgencyContainers.RequiredDelivery);
			result.Add((int)WebTracker.Grids.LinerAndAgencyContainers.ActualDelivery);
			result.Add((int)WebTracker.Grids.LinerAndAgencyContainers.Deliver);
			result.Add((int)WebTracker.Grids.LinerAndAgencyContainers.EmptyReady);
			result.Add((int)WebTracker.Grids.LinerAndAgencyContainers.EmptyReturnRequired);
			result.Add((int)WebTracker.Grids.LinerAndAgencyContainers.ActualDehire);
			result.Add((int)WebTracker.Grids.LinerAndAgencyContainers.Vessel);
			result.Add((int)WebTracker.Grids.LinerAndAgencyContainers.Voyage);
#pragma warning disable IDE0004 // Remove Unnecessary Cast Justification = "ZBindToChecker requires a redundant cast"
			ZBindToChecker.CheckBindTo((TrackingMilestoneCollection)((LinerAndAgencyContainer)null).Milestones);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // Data column name
			{
				result.Add(column.UniqueKey);
			}
			return result;
		}
	}
}
