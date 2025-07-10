using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ForwardingShipmentFormCustomisationSettingsProvider))]
	public class ForwardingShipmentFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<ForwardingShipmentFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var propertiesThatAffectWorkflowMatchCriteria = new string[]
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

			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(propertiesThatAffectWorkflowMatchCriteria, provider.PropertiesThatAffectWorkflow);

			var shipment = Factory.New<ForwardingShipment>();
			var triggered = new List<string>();

			foreach (var propertyName in propertiesThatAffectWorkflowMatchCriteria)
			{
				var info = shipment.ZPropertyInfoHash[propertyName];
				AssertNotNull(info);

				info.ValueChanged += (s, e) =>
				{
					triggered.Add(propertyName);
				};
			}

			var randomOrg = Factory.New<OrgHeader>();
			var randomOrg2 = Factory.New<OrgHeader>();

			void AssertValueChangeWasTriggered(string propertyName)
			{
				AssertCollectionContains($"{propertyName} has fired on value changed", propertyName, triggered);
				triggered.Clear();
			}

			void AssertValueChangeWasNotTriggered(string propertyName)
			{
				AssertCollectionNotContains($"{propertyName} has not fired on value changed", propertyName, triggered);
				triggered.Clear();
			}

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertValueChangeWasTriggered(JobShipmentSchema.JS_TransportMode.Name);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertValueChangeWasTriggered(JobShipmentSchema.JS_TransportMode.Name);

			shipment.JS_TransportMode = ZString.Empty;
			AssertValueChangeWasTriggered(JobShipmentSchema.JS_TransportMode.Name);

			shipment.ConsignorPK = randomOrg.PK;
			AssertValueChangeWasTriggered(ForwardingShipment.Schema.ConsignorPK);

			shipment.ConsignorPK = randomOrg2.PK;
			AssertValueChangeWasTriggered(ForwardingShipment.Schema.ConsignorPK);

			shipment.ConsignorPK = ZGuid.Empty;
			AssertValueChangeWasTriggered(ForwardingShipment.Schema.ConsignorPK);

			shipment.ConsigneePK = randomOrg.PK;
			AssertValueChangeWasTriggered(ForwardingShipment.Schema.ConsigneePK);

			shipment.ConsigneePK = randomOrg2.PK;
			AssertValueChangeWasTriggered(ForwardingShipment.Schema.ConsigneePK);

			shipment.ConsigneePK = ZGuid.Empty;
			AssertValueChangeWasTriggered(ForwardingShipment.Schema.ConsigneePK);

			shipment.JS_RL_NKOrigin = "AUSYD";
			AssertValueChangeWasTriggered(JobShipmentSchema.JS_RL_NKOrigin.Name);

			shipment.JS_RL_NKDestination = "SGSIN";
			AssertValueChangeWasTriggered(JobShipmentSchema.JS_RL_NKDestination.Name);

			shipment.JS_RL_NKDestination = "AUSYD";
			AssertValueChangeWasTriggered(ForwardingShipment.Schema.IsDomesticFreight);

			shipment.ControllingCustomerAddress.OrganisationPK = randomOrg.PK;
			AssertValueChangeWasTriggered(ForwardingShipment.Schema.ControllingCustomerPK);

			shipment.ControllingCustomerAddress.OrganisationPK = randomOrg2.PK;
			AssertValueChangeWasTriggered(ForwardingShipment.Schema.ControllingCustomerPK);

			shipment.ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
			AssertValueChangeWasTriggered(ForwardingShipment.Schema.ControllingCustomerPK);

			var loader = new JobHeader.Loader(shipment);
			var jobHeader = loader.TryCreate();

			AssertEquals("prerequisite; jobheader was created", jobHeader, shipment.ShipmentJobHeader);
			AssertEquals("prerequisite; jobheader address has not been set", ZGuid.Empty, shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr);
			AssertValueChangeWasNotTriggered(ForwardingShipment.Schema.ShipmentJobHeaderPK);

			shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr = randomOrg.MainAddress.PK;
			AssertValueChangeWasTriggered(ForwardingShipment.Schema.ShipmentJobHeaderPK);

			shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr = randomOrg2.MainAddress.PK;
			AssertValueChangeWasTriggered(ForwardingShipment.Schema.ShipmentJobHeaderPK);

			shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr = ZGuid.Empty;
			AssertValueChangeWasTriggered(ForwardingShipment.Schema.ShipmentJobHeaderPK);
		}

		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			var tabs = provider.DisplayTabs;

			var expectedTabNames = new string[]
			{
				"ShipmentDetailsTabPage",
				"AdditionalTabPage",
				"RelatedShipmentsTabPage",
				"RoutingTabPage",
				"ContainerDetailsTabPage",
				"PickupTabPage",
				"DeliveryTabPage",
				"WorkflowTabPage",
				"NCTSTabPage",
				"BillingTabPage",
				"AddressesTabPage",
				"BrokerageTabPage",
				"DocDataTabPage",
				"ComplianceRiskTabPage",
				"eDocsTabPage",
				"NotesTabPage",
				"EventTabPage",
				"CommercialInvoiceTabPage"
			};
			AssertEquals("Expected 18 tabs", 18, tabs.Count);
			AssertContainsExactElementsInAnyOrder(expectedTabNames, tabs.Cast<FormCustomisableElement>().Select(elem => (string)elem.ElementName).ToArray());
		}

		public void TestGetDisplayFields()
		{
			AssertDisplayFieldCaption("HouseBill", "House bill Number");
			AssertDisplayFieldCaption("OriginDestinationDates", "Origin, Destination and Dates");
			AssertDisplayFieldCaption("WeightVolumeChargeable", "Weight/Volume");

			AssertDisplayFieldCaption("Chargeable", "Chargeable");
			AssertDisplayFieldCaption("PacksValues", "Packages and Goods Value");
			AssertDisplayFieldCaption("Description", "Goods Description");
			AssertDisplayFieldCaption("MarksNumbers", "Marks & Numbers");
			AssertDisplayFieldCaption("PaymentTerm", "INCO / Payment Term");
			AssertDisplayFieldCaption("AdditionalTerms", "Additional Terms");
			AssertDisplayFieldCaption("ServiceLevel", "Service Level");
			AssertDisplayFieldCaption("CustomsEntryNumber", "Customs Clearance/Permit No.");
			AssertDisplayFieldCaption("AviationSecurity", "Aviation Security");
			AssertDisplayFieldCaption("ReleaseType", "Release Type");
			AssertDisplayFieldCaption("AirwayBillDims", "Air Waybill Dims");
			AssertDisplayFieldCaption("HousebillType", "House bill Type");
			AssertDisplayFieldCaption("OnBoard", "On Board Details");
			AssertDisplayFieldCaption("ContainerModeOverride", "HBL Delivery Mode");
			AssertDisplayFieldCaption("BillDetails", "Bill Prints/Issue Date");
			AssertDisplayFieldCaption("ChargesApply", "Charges Apply");
			AssertDisplayFieldCaption("ExportStatement", "Exporter Statement");
			AssertDisplayFieldCaption("ShipperCOD", "Shipper COD");
			AssertDisplayFieldCaption("ScreeningStatus", "Screening Status");
			AssertDisplayFieldCaption("Phase", "Phase");
			AssertDisplayFieldCaption("ISFBillStatus", "ISF Bill Status");

			AssertDisplayFieldCaption("FreightSpotRate", "Spot Rate");
			AssertDisplayFieldCaption("EFreightStatus", "e-freight Status");

			AssertDisplayFieldCaption("OrderLinks", "Order Management");
			AssertDisplayFieldCaption("CustomFields", "Custom Fields");

			AssertDisplayFieldCaption("Consols", "Consolidation Details");
			AssertDisplayFieldCaption("Services", "Services");
			AssertDisplayFieldCaption("WeightVolume", "Weight/Vol Client/Carrier");
			AssertDisplayFieldCaption("FreightRatesAndGateways", "Freight Rates and Gateways");
			AssertDisplayFieldCaption("NotifyParty", "Notify Party");
			AssertDisplayFieldCaption("ReferenceNumbers", "Reference Numbers");

			AssertDisplayFieldCaption("EstExportCustomsClearLabel", "Estimated Export Clearance Date");

			AssertDisplayFieldCaption("ControllingCustomerAddress", "Controlling Customer");
			AssertDisplayFieldCaption("ControllingAgentAddress", "Controlling Agent");
			AssertDisplayFieldCaption("OrderUpdateCutOff", "Order Update Cutoff Date");
			AssertDisplayFieldCaption("DestinationGoodsValueCalcFindBox", "Destination Goods Value");
		}

		void AssertDisplayFieldCaption(string fieldName, string expectedFieldDescription)
		{
			var provider = GetNewProvider();
			var displayField = provider.DisplayFields.Cast<FormCustomisableElement>().FirstOrDefault(x => x.ElementName == fieldName);

			AssertEquals(string.Format("Expected field caption for {0}", fieldName), expectedFieldDescription, displayField.ElementDescription);
		}

		public void TestDisplayFields_DetailsGroup()
		{
			var provider = GetNewProvider();
			var detailsFields = provider.DisplayFields.Cast<FormCustomisableElement>().Where(x => x.DisplayTabCode == "ShipmentDetailsTabPage" && x.ElementGroup == "Details");

			AssertEquals("Details group fields count", 34, detailsFields.Count());
			AssertEquals("Details group fields have distinct row numbers", 29, detailsFields.Select(x => x.RowNumber).Distinct().Count());
		}

		public void TestDisplayFields_DetailsGroup_CommunityTransitStatusForEu()
		{
			var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			var tradeGroup1 = helper.CreateTradeGroup("EUN", "EUC", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup1, Core.Constants.CountryCodes.Germany, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			var tradeGroup2 = helper.CreateTradeGroup("EUN", "EUCTP", new ZDateTime(2019, 1, 1), new ZDateTime(2040, 12, 31));
			helper.AddCountry(tradeGroup2, Core.Constants.CountryCodes.Andorra, new ZDate(2019, 1, 1), new ZDate(2040, 12, 31));
			Factory.Save();

			var getCTSField = GetNewProvider().DisplayFields.Cast<FormCustomisableElement>().FirstOrDefault(element => element.ElementName == "CommunityTransitStatus").IsAvailableFunction;

			var originalBranchCountry = GlbBranch.CurrentBranch.GB_RN_NKCountryCode;

			try
			{
				GlbBranch.CurrentBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				AssertEquals("CommunityTransit field is available to Germany (EU member)", true, getCTSField());

				GlbBranch.CurrentBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Andorra;
				AssertEquals("CommunityTransit field is available to Andorra (CT participant)", true, getCTSField());

				GlbBranch.CurrentBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				AssertEquals("CommunityTransit field is hidden to Australia (too many convicts)", false, getCTSField());

				GlbBranch.CurrentBranch.GB_RN_NKCountryCode = "";
				AssertEquals("CommunityTransit field is hidden when branch has no country specified", false, getCTSField());
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RN_NKCountryCode = originalBranchCountry;
			}
		}

		public void TestDisplayFields_DetailsGroup_LoadingMeters()
		{
			var loadingMetersField = GetNewProvider().DisplayFields.Cast<FormCustomisableElement>().FirstOrDefault(element => element.ElementName == "LoadingMeters");

			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition", true, FreightDataRegistry.Instance.EnableRoadLoadingMeters.Value);
			AssertEquals("LoadingMeters field is available when loading meters registry is enabled", true, loadingMetersField.IsAvailable);

			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition", false, FreightDataRegistry.Instance.EnableRoadLoadingMeters.Value);
			AssertEquals("LoadingMeters field is not available when loading meters registry is disabled", false, loadingMetersField.IsAvailable);
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();

			AssertEquals(1, provider.TabPlacementProhibitions.Length);
			AssertEquals("ShipmentDetailsTabPage", provider.TabPlacementProhibitions[0].TabName);
			AssertContainsExactElementsInAnyOrder(new string[2] { TabPlacement.Placements.TopLeft, TabPlacement.Placements.BottomLeft },
				provider.TabPlacementProhibitions[0].ProhibitedPlacements);
		}

		public void TestDisplayFields_DetailsGroup_TotalCO2e()
		{
			var totalCO2eField = GetNewProvider().DisplayFields.Cast<FormCustomisableElement>().FirstOrDefault(element => element.ElementName == "TotalCO2e");
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				AssertEquals("TotalCO2e field is available when Green House Has Emission calculation registry is enabled", true, totalCO2eField.IsAvailable);
				AssertEquals("TotalCO2e field is Visible by default", true, totalCO2eField.Visible);
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			{
				AssertEquals("TotalCO2e field is not available when Green House Has Emission calculation registry is disabled", false, totalCO2eField.IsAvailable);
			}
		}

		public override ForwardingShipmentFormCustomisationSettingsProvider GetNewProvider()
		{
			return new ForwardingShipmentFormCustomisationSettingsProvider();
		}
	}
}
