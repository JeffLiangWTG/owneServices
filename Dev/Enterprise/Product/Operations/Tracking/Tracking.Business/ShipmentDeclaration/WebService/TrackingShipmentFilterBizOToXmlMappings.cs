using System.Collections.Generic;
using Enterprise.Customs.Forwarding.Module;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.Module;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

[assembly: UsesConstants(typeof(ForwardingShipmentModuleCustomsFiltersProvider.Descriptions))]

namespace Enterprise.Tracking.Business.ShipmentDeclaration
{
	[Immutable]
	public class TrackingShipmentFilterBizOToXmlMappings : EnterpriseCodeExternalCodeMappings
	{
		public const string MostCommonNumberFilters = "MostCommonNumberFilters";
		public const string AnySendingOrReceivingAgents = "AnySendingOrReceivingAgents";
		public const string AllFilters = "ALL";

		TrackingShipmentFilterBizOToXmlMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(AllFilters, "ALL");

			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.ETA, nameof(Xsd.ShipmentDateFieldsList.ETA));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.ETD, nameof(Xsd.ShipmentDateFieldsList.ETD));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.FirstPortOfArrivalDate, nameof(Xsd.ShipmentDateFieldsList.FirstArrivalPort));
			//			yield return new Mapping(string.Empty, Xsd.ShipmentDateFieldsList.Tranship.ToString());
			//			yield return new Mapping(string.Empty, Xsd.ShipmentDateFieldsList.Forward.ToString());

			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.LoadDischarge, nameof(Xsd.ShipmentLocationFieldsList.ConsolLoadDischarge));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.OriginDestination, nameof(Xsd.ShipmentLocationFieldsList.ShipmentOriginDestination));

			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.BookingReferenceNum, nameof(Xsd.ShipmentNumberFieldsList.ClientReference));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.ConsolNum, nameof(Xsd.ShipmentNumberFieldsList.ConsolNumber));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.HouseBill, nameof(Xsd.ShipmentNumberFieldsList.HouseBill));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.MasterBill, nameof(Xsd.ShipmentNumberFieldsList.MasterBillNumber));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.OrderNum, nameof(Xsd.ShipmentNumberFieldsList.OrderNumber));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.ShipmentNum, nameof(Xsd.ShipmentNumberFieldsList.ShipmentNumber));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.InteriumReceiptNum, nameof(Xsd.ShipmentNumberFieldsList.InterimReceiptNumber));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.PackLineReferenceNum, nameof(Xsd.ShipmentNumberFieldsList.PackRefNumber));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.ContainerNum, nameof(Xsd.ShipmentNumberFieldsList.ContainerNo));
			yield return new Mapping(MostCommonNumberFilters, nameof(Xsd.ShipmentNumberFieldsList.Common));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.CO2e, nameof(Xsd.ReferenceType.CO2e));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.CompanyTariffLevelOverride, nameof(Xsd.ReferenceType.CompanyTariffLevelOverride));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.FMCTariffID, nameof(Xsd.ReferenceType.FMCTariffID));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.RateCommodity, nameof(Xsd.ReferenceType.RateCommodity));
			//			yield return new Mapping(string.Empty, Xsd.ShipmentNumberFieldsList.DirectMAWB.ToString());

			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.PickupAgent, nameof(Xsd.ShipmentOrganisationFieldsList.PickupAgent));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.DeliveryAgent, nameof(Xsd.ShipmentOrganisationFieldsList.DeliveryAgent));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.ExportBrokerCartage, nameof(Xsd.ShipmentOrganisationFieldsList.ExportBrokerCartage));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.ImportBrokerCartage, nameof(Xsd.ShipmentOrganisationFieldsList.ImportBrokerCartage));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.ConsignorConsignee, nameof(Xsd.ShipmentOrganisationFieldsList.ShipperConsignee));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.TranshipmentAgent, nameof(Xsd.ShipmentOrganisationFieldsList.TranshipAgent));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.ConsolSendReceiveAgents, nameof(Xsd.ShipmentOrganisationFieldsList.ConsolSendingRecvAgent));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.ShipmentSendReceiveForwarders, nameof(Xsd.ShipmentOrganisationFieldsList.ShipmentSendingRecvAgent));
			yield return new Mapping(AnySendingOrReceivingAgents, nameof(Xsd.ShipmentOrganisationFieldsList.SendingReceivingAgent));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.ConsignorRelatedParties, nameof(Xsd.ShipmentOrganisationFieldsList.ConsignorRelatedParties));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.ConsigneeRelatedParties, nameof(Xsd.ShipmentOrganisationFieldsList.ConsigneeRelatedParties));
			yield return new Mapping(JobShipmentFilterBusinessObject.Descriptions.LocalClientRelatedParties, nameof(Xsd.ShipmentOrganisationFieldsList.LocalClientRelatedParties));
		}

		public IReadOnlyCollection<string> MostCommonNumberFilterList
		{
			get { return new[] { JobShipmentFilterBusinessObject.Descriptions.ShipmentNum, JobShipmentFilterBusinessObject.Descriptions.HouseBill }; }
		}

		public IReadOnlyCollection<string> AnyAgentFilterList
		{
			get { return new[] { JobShipmentFilterBusinessObject.Descriptions.ConsolSendReceiveAgents, JobShipmentFilterBusinessObject.Descriptions.ShipmentSendReceiveForwarders }; }
		}

		public IReadOnlyCollection<string> AllDateFilters
		{
			get
			{
				return new[]
						{
						JobShipmentFilterBusinessObject.Descriptions.ETA,
						JobShipmentFilterBusinessObject.Descriptions.ETD,
						JobShipmentFilterBusinessObject.Descriptions.FirstPortOfArrivalDate
						};
			}
		}

		public IReadOnlyCollection<string> AllLocationFilters
		{
			get { return new[] { JobShipmentFilterBusinessObject.Descriptions.LoadDischarge, JobShipmentFilterBusinessObject.Descriptions.OriginDestination }; }
		}

		public IReadOnlyCollection<string> AllNumberFilters
		{
			get
			{
				return new[]
						{
						JobShipmentFilterBusinessObject.Descriptions.BookingReferenceNum,
						JobShipmentFilterBusinessObject.Descriptions.ConsolNum,
						JobShipmentFilterBusinessObject.Descriptions.HouseBill,
						JobShipmentFilterBusinessObject.Descriptions.MasterBill,
						JobShipmentFilterBusinessObject.Descriptions.OrderNum,
						JobShipmentFilterBusinessObject.Descriptions.ShipmentNum,
						JobShipmentFilterBusinessObject.Descriptions.InteriumReceiptNum,
						JobShipmentFilterBusinessObject.Descriptions.PackLineReferenceNum,
						JobShipmentFilterBusinessObject.Descriptions.ContainerNum
						};
			}
		}

		public IReadOnlyCollection<string> AllOrganizationFilters
		{
			get
			{
				return new[]
						{
							JobShipmentFilterBusinessObject.Descriptions.DeliveryAgent,
							JobShipmentFilterBusinessObject.Descriptions.ConsignorConsignee,
							JobShipmentFilterBusinessObject.Descriptions.TranshipmentAgent,
							JobShipmentFilterBusinessObject.Descriptions.ConsolSendReceiveAgents,
							JobShipmentFilterBusinessObject.Descriptions.ShipmentSendReceiveForwarders,
							JobShipmentFilterBusinessObject.Descriptions.ConsignorRelatedParties,
							JobShipmentFilterBusinessObject.Descriptions.ConsigneeRelatedParties,
							JobShipmentFilterBusinessObject.Descriptions.LocalClientRelatedParties,
							JobShipmentFilterBusinessObject.Descriptions.ImportBroker,
							JobShipmentFilterBusinessObject.Descriptions.ExportBroker,
							JobShipmentFilterBusinessObject.Descriptions.PickupTransportCompany,
							JobShipmentFilterBusinessObject.Descriptions.DeliveryTransportCompany,
							JobShipmentFilterBusinessObject.Descriptions.CO2e,
							JobShipmentFilterBusinessObject.Descriptions.CompanyTariffLevelOverride,
							JobShipmentFilterBusinessObject.Descriptions.FMCTariffID,
							JobShipmentFilterBusinessObject.Descriptions.RateCommodity
						};
			}
		}

		public static readonly TrackingShipmentFilterBizOToXmlMappings Instance = new TrackingShipmentFilterBizOToXmlMappings();

		protected override string Name
		{
			get { return Res.GetString("fb54d1b1-466e-45eb-90ba-0e3f823f4386", "Shipment Filter List"); }
		}
	}
}
