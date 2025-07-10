using System;
using Enterprise.Customs.Forwarding.Module;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Freight.Forwarding.Module;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.ShipmentDeclaration.Testing
{
	[TestedType(typeof(TrackingShipmentFilterBizOToXmlMappings))]
	sealed class TrackingShipmentFilterBizOToXmlMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		public void TestXsdAllFilters()
		{
			Assert("One const can be used for any list mappings",
						 nameof(Xsd.ShipmentDateFieldsList.ALL) == TrackingShipmentFilterBizOToXmlMappings.AllFilters &&
						 nameof(Xsd.ShipmentLocationFieldsList.ALL) == TrackingShipmentFilterBizOToXmlMappings.AllFilters &&
						 nameof(Xsd.ShipmentNumberFieldsList.ALL) == TrackingShipmentFilterBizOToXmlMappings.AllFilters &&
						 nameof(Xsd.ShipmentOrganisationFieldsList.ALL) == TrackingShipmentFilterBizOToXmlMappings.AllFilters);
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return new[]
				{
						JobShipmentFilterBusinessObject.Descriptions.CoLoadStatus,
						JobShipmentFilterBusinessObject.Descriptions.ShowTranshipCrossTradeNoConsol,
						JobShipmentFilterBusinessObject.Descriptions.HiddenFwdRegistered,
						JobShipmentFilterBusinessObject.Descriptions.WarehouseLocation,
						JobShipmentFilterBusinessObject.Descriptions.IsTemperatureControlled,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.SeaCargoMessageStatus,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.SeaCargoCustomsStatus,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.AirCargoMessageStatus,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.AirCargoCustomsStatus,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.EntryType,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.ITNumber,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.ITDate,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.ITType,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.ITCarrier,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.ITCarrierTOL,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.CargoReleaseStatus,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.BillHoldOrExam,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.SimplifiedEntryBillStatus,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.EntrySummaryStatus,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.ExportStatus,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.ISFBillStatus,
						ForwardingShipmentModuleCustomsFiltersProvider.Descriptions.AFRBillStatus,
						JobShipmentFilterBusinessObject.Descriptions.NAFTADutyDeferralStatus,
						JobShipmentFilterBusinessObject.Descriptions.FlightVoyageAndVessel,
						JobShipmentFilterBusinessObject.Descriptions.ReleaseType,
						JobShipmentFilterBusinessObject.Descriptions.AdditionalReferenceNumbers,
						JobShipmentFilterBusinessObject.Descriptions.ClientAssignedStaff,
						JobShipmentFilterBusinessObject.Descriptions.IncoTerms,
						JobShipmentFilterBusinessObject.Descriptions.PickupCFS,
						JobShipmentFilterBusinessObject.Descriptions.DeliveryCFS,
						JobShipmentFilterBusinessObject.Descriptions.ETALoad,
						JobShipmentFilterBusinessObject.Descriptions.ATALoad,
						JobShipmentFilterBusinessObject.Descriptions.ETDLoad,
						JobShipmentFilterBusinessObject.Descriptions.ATDLoad,
						JobShipmentFilterBusinessObject.Descriptions.ETADischarge,
						JobShipmentFilterBusinessObject.Descriptions.ATADischarge,
						JobShipmentFilterBusinessObject.Descriptions.EFreightStatus,
						JobShipmentFilterBusinessObject.Descriptions.RelatedConsols,
						JobShipmentFilterBusinessObject.Descriptions.Consignee,
						JobShipmentFilterBusinessObject.Descriptions.Consignor,
						JobShipmentFilterBusinessObject.Descriptions.LocalClientBilling,
						JobShipmentFilterBusinessObject.Descriptions.Buyer,
						JobShipmentFilterBusinessObject.Descriptions.Supplier,
						ZArchitecture.Business.FilterStripBusinessObject.TemplateRecordsDescription,
						JobShipmentFilterBusinessObject.Descriptions.BookingStatus,
						JobShipmentFilterBusinessObject.Descriptions.PlannedLoadDischarge,
						JobShipmentFilterBusinessObject.Descriptions.Gateway,
						JobShipmentFilterBusinessObject.Descriptions.IsHazardous,
						JobShipmentFilterBusinessObject.Descriptions.CTStatus,
						JobShipmentFilterBusinessObject.Descriptions.RelatedTransportBookings,
						JobShipmentFilterBusinessObject.Descriptions.ImportBroker,
						JobShipmentFilterBusinessObject.Descriptions.ExportBroker,
						JobShipmentFilterBusinessObject.Descriptions.PickupTransportCompany,
						JobShipmentFilterBusinessObject.Descriptions.DeliveryTransportCompany,
						JobShipmentFilterBusinessObject.Descriptions.PickupCFSReceiptRequested,
						JobShipmentFilterBusinessObject.Descriptions.PickupCFSDispatchRequested,
						JobShipmentFilterBusinessObject.Descriptions.DeliveryCFSReceiptRequested,
						JobShipmentFilterBusinessObject.Descriptions.DeliveryCFSDispatchRequested,
						JobShipmentFilterBusinessObject.Descriptions.DeliveryDueDate,
						JobShipmentFilterBusinessObject.Descriptions.RevisedDeliveryDueDate,
						JobShipmentFilterBusinessObject.Descriptions.HasDamagedPackages,
						JobShipmentFilterBusinessObject.Descriptions.HasPillagedPackages,
						JobShipmentFilterBusinessObject.Descriptions.OriginTransitWarehouseStatus,
						JobShipmentFilterBusinessObject.Descriptions.InterimReceiptDate,
						JobShipmentFilterBusinessObject.Descriptions.ActualPickupDate,
						JobShipmentFilterBusinessObject.Descriptions.ActualDeliveryDate,
						JobShipmentFilterBusinessObject.Descriptions.ExitStatus,
						JobShipmentFilterBusinessObject.Descriptions.HBLStatus,
						JobShipmentFilterBusinessObject.Descriptions.HBLType,
						JobShipmentFilterBusinessObject.Descriptions.HBLTerms,
					};
		}

		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(JobShipmentFilterBusinessObject.Descriptions) }; }
		}
	}
}
