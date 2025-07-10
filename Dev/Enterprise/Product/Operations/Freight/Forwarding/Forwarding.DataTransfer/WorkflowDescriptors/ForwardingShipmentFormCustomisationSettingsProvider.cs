using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.GlobalCommercialInvoice.Integration.Constants;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingShipmentFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override FormCustomisableElementCollection GetDisplayTabs()
		{
			FormCustomisableElementCollection result = new FormCustomisableElementCollection();
			result.SuspendValidation();
			result.Add(ResString.GetMultilingualString("b8477d0c-faf6-4fa2-aea5-aa8cc929d619", "Basic Registration"), "ShipmentDetailsTabPage", true);
			result.Add(ResString.GetMultilingualString("da97decd-33c9-4c90-b684-8bb84a7f180b", "Additional Detail"), "AdditionalTabPage", true);
			result.Add(ResString.GetMultilingualString("930faacd-1dfa-48df-8e1b-1eb0b27e2ad0", "Related Shipments"), "RelatedShipmentsTabPage");
			result.Add(ResString.GetMultilingualString("95358b90-7479-49f0-8420-d0cd0e621d90", "Routing"), "RoutingTabPage");
			result.Add(ResString.GetMultilingualString("816f4605-b2e2-455e-83fe-05c7d9acb6be", "Packing"), "ContainerDetailsTabPage");
			result.Add(ResString.GetMultilingualString("6ec79461-9820-4443-8223-4ee81294ba55", "Pickup"), "PickupTabPage");
			result.Add(ResString.GetMultilingualString("0f6a77d2-59da-48fc-8086-ffa77e2128e0", "Delivery"), "DeliveryTabPage");
			result.Add(ResString.GetMultilingualString("d2e3fadb-c4ab-4de0-8044-5d2379a0d33d", "Workflow & Tracking"), "WorkflowTabPage");
			result.Add(ResString.GetMultilingualString("920906f2-453d-4752-8058-bd02765d760f", "Billing"), "BillingTabPage");
			result.Add(ResString.GetMultilingualString("d48d8582-0743-4a7f-8bf6-839c69c4bc44", "Addresses"), "AddressesTabPage");
			result.Add(ResString.GetMultilingualString("1ff1aa17-724e-45a9-b7d5-fcd607cb7da1", "Brokerage"), "BrokerageTabPage");
			result.Add(ResString.GetMultilingualString("4a2f6f51-295c-413e-920a-8a642eb92ceb", "NCTS"), "NCTSTabPage");
			result.Add(ResString.GetMultilingualString("e19d2bbd-446b-4877-bf89-5e2ce10cf0a3", "Doc Data"), "DocDataTabPage");
			result.Add(ResString.GetMultilingualString("1449352A-7C60-4FAF-835A-9415E9A019B4", "Compliance Risk"), ComplianceRisk.Integration.ComplianceWiseConstants.ComplianceRiskTabPageName);
			result.Add(ResString.GetMultilingualString("c2990a2a-68f2-4b46-9d25-4aa64e4c3eeb", "eDocs"), "eDocsTabPage");
			result.Add(ResString.GetMultilingualString("d94c275d-ea99-4504-a7da-6dc0209b245d", "Notes"), "NotesTabPage");
			result.Add(ResString.GetMultilingualString("dd87c632-73c8-4d02-9823-22773cc05feb", "Logs"), "EventTabPage");
			result.Add(ResString.GetMultilingualString("865E78AC-5A74-4137-B2DE-434DC7FAA562", PluginName), PluginTabPageName);
			result.ResumeValidation();
			return result;
		}

		protected override FormCustomisableElementCollection GetDisplayFields()
		{
			#region SuppressResourceStringsCheckRegion

			FormCustomisableElementCollection result = new FormCustomisableElementCollection();
			result.SuspendValidation();
			result.Add(ResString.GetMultilingualString("6016c4f2-fa82-49bb-b8f7-9207e9f385d5", "House bill Number"), "HouseBill", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 0);
			result.Add(ResString.GetMultilingualString("459b2e07-2718-477d-b830-517018bff000", "Origin, Destination and Dates"), "OriginDestinationDates", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 1);
			result.Add(ResString.GetMultilingualString("ccac8ad0-439f-4eb3-a180-98afb11b14a3", "Weight/Volume"), "WeightVolumeChargeable", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 3);

			FormCustomisableElement loadingMeters = result.Add(ResString.GetMultilingualString("4fc3a7d2-096a-4ebc-9647-c89006242e2e", "Loading Meters"), "LoadingMeters", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 4);
			loadingMeters.IsAvailableFunction = () => FreightDataRegistry.Instance.EnableRoadLoadingMeters.Value;

			result.Add(ResString.GetMultilingualString("a3e30ea8-8c79-45a6-a264-c28a9eb2ccd7", "Chargeable"), "Chargeable", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 5);
			result.Add(ResString.GetMultilingualString("5376ab18-3b1f-4407-bf70-223b722c79c4", "Packages and Goods Value"), "PacksValues", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 6);
			result.Add(ResString.GetMultilingualString("107085d9-1550-4144-a64d-53ba6a4f1afa", "Goods Description"), "Description", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 9);
			result.Add(ResString.GetMultilingualString("56648136-3893-4a76-9cfd-93541f8d07e6", "Marks & Numbers"), "MarksNumbers", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 10);
			result.Add(ResString.GetMultilingualString("b2c9f387-446b-4b15-bb5b-f60e51e6108b", "INCO / Payment Term"), "PaymentTerm", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 11);
			result.Add(ResString.GetMultilingualString("401a9e36-150e-4dee-bf40-ce093ca28265", "Additional Terms"), "AdditionalTerms", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 12);
			result.Add(ResString.GetMultilingualString("07b04a85-411f-40b5-a6b7-12452bf9d637", "Service Level"), "ServiceLevel", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 13);
			result.Add(ResString.GetMultilingualString("79b73a09-fbb7-4b43-a630-3400a27d4280", "Customs Clearance/Permit No."), "CustomsEntryNumber", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 14);
			result.Add(ResString.GetMultilingualString("97b7f821-c6a2-4504-9588-d9e2d860c0a0", "Aviation Security"), "AviationSecurity", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 16);
			result.Add(ResString.GetMultilingualString("42ed62df-8495-4360-81fc-85d3ae132efd", "Release Type"), "ReleaseType", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 17);
			result.Add(ResString.GetMultilingualString("d19135a3-ebf1-409f-90b5-32d54cd2546e", "Air Waybill Dims"), "AirwayBillDims", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 18);
			result.Add(ResString.GetMultilingualString("9ed45488-15a7-40fe-931e-ddfa079e5b81", "House bill Type"), "HousebillType", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 19);
			result.Add(ResString.GetMultilingualString("4d48cca1-8216-43c7-8b84-db7e8be59ecf", "On Board Details"), "OnBoard", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 21);
			result.Add(ResString.GetMultilingualString("28d36dd7-e0f2-494a-a954-fc6cc00c0f14", "HBL Delivery Mode"), "ContainerModeOverride", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 22);
			result.Add(ResString.GetMultilingualString("b52d1ebf-1335-4271-ba9c-85e028d58304", "Bill Prints/Issue Date"), "BillDetails", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 23);
			result.Add(ResString.GetMultilingualString("0ac99c6e-e9bb-4f36-a27e-9060f6358138", "Charges Apply"), "ChargesApply", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 24);
			result.Add(ResString.GetMultilingualString("f3b834b7-4d86-410e-be52-7ef8dae40ef5", "Exporter Statement"), "ExportStatement", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 25);
			result.Add(ResString.GetMultilingualString("8a1d5fce-c25a-4756-9713-b7ed4d8c2b18", "Shipper COD"), "ShipperCOD", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 26);
			result.Add(ResString.GetMultilingualString("803c6a44-030b-4f71-bebd-7c545e95a0ad", "Screening Status"), "ScreeningStatus", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 27);
			result.Add(ResString.GetMultilingualString("8805b630-dbac-4ac4-903b-4ab8ea1f8a68", "Phase"), "Phase", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 28);
			result.Add(ResString.GetMultilingualString("2b7dd8b0-d85c-4761-9091-9fef948fd0cc", "ISF Bill Status"), "ISFBillStatus", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 29);

			var communityTransitStatus = result.Add(ResString.GetMultilingualString("6E9EFB66-6033-4363-AF6F-8C37200785A7", "Community Transit Status"), "CommunityTransitStatus", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 30);
			communityTransitStatus.IsAvailableFunction = () => ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsCountryEuOrCtCountry(GlbBranch.CurrentBranch?.BaseCountry?.Code ?? string.Empty);

			result.Add(ResString.GetMultilingualString("b12c531d-3673-4291-a123-3ffa7d4b2703", "Spot Rate"), "FreightSpotRate", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 31);
			result.Add(ResString.GetMultilingualString("dbe059a4-7cd6-417e-bc1f-c2fadff9ccd3", "e-freight Status"), "EFreightStatus", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 32);

			var emptyResString = (NoResString)string.Empty;
			result.Add(ResString.GetMultilingualString("6c125bdf-5c85-4ef7-9e49-4fd771da9e45", "Order Management"), "OrderLinks", false, emptyResString, TabNames.ShipmentDetailsTab, TabPlacement.Placements.BottomRight);
			result.Add(ResString.GetMultilingualString("b50503e5-9cb0-4fb3-a428-5f9dacd7b896", "Custom Fields"), "CustomFields", false, emptyResString, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopRight);

			result.Add(ResString.GetMultilingualString("784d2830-f02b-4bcb-a8cb-d29377927c3d", "Consolidation Details"), "Consols", false, emptyResString, TabNames.AdditionalTab, TabPlacement.Placements.TopLeft);
			result.Add(ResString.GetMultilingualString("0084c932-cd87-4704-9429-189f0ca5f8a6", "Services"), "Services", false, emptyResString, TabNames.AdditionalTab, TabPlacement.Placements.BottomMiddle);
			result.Add(ResString.GetMultilingualString("a040ac8d-9542-42b5-8fe6-786d0031a846", "Weight/Vol Client/Carrier"), "WeightVolume", false, emptyResString, TabNames.AdditionalTab, TabPlacement.Placements.TopMiddle);
			result.Add(ResString.GetMultilingualString("9782721b-f4ad-4501-b0a9-b09b7f30b0de", "Freight Rates and Gateways"), "FreightRatesAndGateways", false, emptyResString, TabNames.AdditionalTab, TabPlacement.Placements.BottomLeft);
			result.Add(ResString.GetMultilingualString("dc27ba8e-db9b-441b-b63d-6475120583e1", "Notify Party"), "NotifyParty", false, emptyResString, TabNames.AdditionalTab, TabPlacement.Placements.TopRight);
			result.Add(ResString.GetMultilingualString("7af2a5d6-b9d3-4c58-97d3-3226433aab7b", "Reference Numbers"), "ReferenceNumbers", false, emptyResString, TabNames.AdditionalTab, TabPlacement.Placements.BottomRight);

			result.Add(ResString.GetMultilingualString("ac9a07f8-bc38-4215-a518-8231275a2c85", "Estimated Export Clearance Date"), "EstExportCustomsClearLabel", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 60, false);

			result.Add(ResString.GetMultilingualString("cba5f1ce-effa-4abb-b087-e9e7c413741d", "Controlling Customer"), "ControllingCustomerAddress", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 33, false);
			result.Add(ResString.GetMultilingualString("a0a1b1bf-bb92-4918-ac33-6c7c95f4988e", "Controlling Agent"), "ControllingAgentAddress", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 34, false);
			result.Add(ResString.GetMultilingualString("95e502e5-0711-43e5-ab3e-d3b7a72255a8", "Order Update Cutoff Date"), "OrderUpdateCutOff", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 35, false);

			result.Add(ResString.GetMultilingualString("77829aab-9372-4fca-8ab8-90daa2f566d8", "Destination Goods Value"), "DestinationGoodsValueCalcFindBox", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 36, false);

			var totalCO2e = result.Add(ResString.GetMultilingualString("15990953-48b3-40f9-8332-07070d415ae3", "CO2e"), "TotalCO2e", false, GroupNames.DetailsGroup, TabNames.ShipmentDetailsTab, TabPlacement.Placements.TopMiddle, 7, true);
			totalCO2e.IsAvailableFunction = () => ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled;

			result.ResumeValidation();
			return result;

			#endregion
		}

		class GroupNames
		{
			[ThreadSafe]
			public static MultilingualString DetailsGroup = ResString.GetMultilingualString("8cb4ad4b-b179-425b-ab01-feb2245ffce4", "Details");
		}

		class TabNames
		{
			#region SuppressResourceStringsCheckRegion

			public const string ShipmentDetailsTab = "ShipmentDetailsTabPage";
			public const string AdditionalTab = "AdditionalTabPage";

			#endregion
		}

		public override TabPlacementProhibition[] TabPlacementProhibitions
		{
			get
			{
				List<TabPlacementProhibition> result = new List<TabPlacementProhibition>();
				result.Add(new TabPlacementProhibition("ShipmentDetailsTabPage", TabPlacement.Placements.TopLeft, TabPlacement.Placements.BottomLeft));
				return result.ToArray();
			}
		}

		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new[]
			{
				ForwardingShipment.Schema.ConsignorPK,
				ForwardingShipment.Schema.ConsigneePK,
				JobShipmentSchema.JS_RL_NKOrigin.Name,
				JobShipmentSchema.JS_RL_NKDestination.Name,
				ForwardingShipment.Schema.IsDomesticFreight,
				JobShipmentSchema.JS_TransportMode.Name,
				ForwardingShipment.Schema.ControllingCustomerPK,
				ForwardingShipment.Schema.ShipmentJobHeaderPK
			};
		}
	}
}
