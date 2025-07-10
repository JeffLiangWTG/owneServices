using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	sealed class CargoControlAndTransit : DocDataObject, IDataSourceProvider
	{
		public CargoControlAndTransit(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region Top

		#region Shipper

		public IAddress Shipper
		{
			get => shipper;
			set => shipper = SetChild(shipper, value);
		}
		IAddress shipper;

		#endregion

		#region Consignee

		public IAddress Consignee
		{
			get => consignee;
			set => consignee = SetChild(consignee, value);
		}
		IAddress consignee;

		#endregion

		#region ImportAgent

		public IAddress ImportAgent
		{
			get => importAgent;
			set => importAgent = SetChild(importAgent, value);
		}
		IAddress importAgent;

		#endregion

		#region ExportAgent

		public IAddress ExportAgent
		{
			get => exportAgent;
			set => exportAgent = SetChild(exportAgent, value);
		}
		IAddress exportAgent;

		#endregion

		#region Issuer

		public IAddress Issuer
		{
			get => issuer;
			set => issuer = SetChild(issuer, value);
		}
		IAddress issuer;

		#endregion

		#region Header

		#region AirlinePrefix

		public ZString AirlinePrefix
		{
			get => airlinePrefix;
			set
			{
				if (SetNonPersistentPropertyValue(AirlinePrefixInfo, ref airlinePrefix, value))
				{
				}
			}
		}

		ZString airlinePrefix;

		public ZPropertyInfo AirlinePrefixInfo => GetZPropertyInfo(nameof(AirlinePrefix));

		#endregion AirlinePrefix

		#region SerialNo

		public ZString SerialNo
		{
			get => serialNo;
			set
			{
				if (SetNonPersistentPropertyValue(SerialNoInfo, ref serialNo, value))
				{
				}
			}
		}

		ZString serialNo;

		public ZPropertyInfo SerialNoInfo => GetZPropertyInfo(nameof(SerialNo));

		#endregion SerialNo

		#region AWBNumber

		public ZString AWBNumber
		{
			get => awbNumber;
			set
			{
				if (SetNonPersistentPropertyValue(AWBNumberInfo, ref awbNumber, value))
				{
				}
			}
		}

		ZString awbNumber;

		public ZPropertyInfo AWBNumberInfo => GetZPropertyInfo(nameof(AWBNumber));

		#endregion AWBNumber

		#endregion Header

		#region AirportOfDeparture

		public ICodeDescription AirportOfDeparture
		{
			get => airportOfDeparture;
			set => airportOfDeparture = SetChild(airportOfDeparture, value);
		}

		ICodeDescription airportOfDeparture;

		#endregion AirportOfDeparture

		#region PortOfFirstArrival

		public IUnloco PortOfFirstArrival
		{
			get => portOfFirstArrival;
			set => portOfFirstArrival = SetChild(portOfFirstArrival, value);
		}

		IUnloco portOfFirstArrival;

		#endregion

		#region To1st

		public ICodeDescription To1st
		{
			get => to1st;
			set => to1st = SetChild(to1st, value);
		}

		ICodeDescription to1st;

		#endregion To1st

		#region AirportOfDestination

		public ICodeDescription AirportOfDestination
		{
			get => airportOfDestination;
			set => airportOfDestination = SetChild(airportOfDestination, value);
		}

		ICodeDescription airportOfDestination;

		#endregion AirportOfDestination

		#region ReferenceNumber

		public ZString ReferenceNumber
		{
			get => referenceNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ReferenceNumberInfo, ref referenceNumber, value))
				{
				}
			}
		}

		ZString referenceNumber;

		public ZPropertyInfo ReferenceNumberInfo => GetZPropertyInfo(nameof(ReferenceNumber));

		#endregion ReferenceNumber

		#region OptionalShippingInformation

		public ZString OptionalShippingInformation
		{
			get => optionalShippingInformation;
			set
			{
				if (SetNonPersistentPropertyValue(OptionalShippingInformationInfo, ref optionalShippingInformation, value))
				{
				}
			}
		}

		ZString optionalShippingInformation;

		public ZPropertyInfo OptionalShippingInformationInfo => GetZPropertyInfo(nameof(OptionalShippingInformation));

		#endregion OptionalShippingInformation

		#region OptionalShippingInformation2

		public ZString OptionalShippingInformation2
		{
			get => optionalShippingInformation2;
			set
			{
				if (SetNonPersistentPropertyValue(OptionalShippingInformation2Info, ref optionalShippingInformation2, value))
				{
				}
			}
		}

		ZString optionalShippingInformation2;

		public ZPropertyInfo OptionalShippingInformation2Info => GetZPropertyInfo(nameof(OptionalShippingInformation2));

		#endregion OptionalShippingInformation2

		#region Currency

		public ICodeDescription Currency
		{
			get => currency;
			set => currency = SetChild(currency, value);
		}

		ICodeDescription currency;

		#endregion Currency

		#region Charges

		public ICodeDescription Charges
		{
			get => charges;
			set => charges = SetChild(charges, value);
		}

		ICodeDescription charges;

		#endregion Charges

		#region WeightPrepaidCollect

		public ICodeDescription WeightPrepaidCollect
		{
			get => weightPrepaidCollect;
			set => weightPrepaidCollect = SetChild(weightPrepaidCollect, value);
		}

		ICodeDescription weightPrepaidCollect;

		#endregion WeightPrepaidCollect

		#region OtherPrepaidCollect

		public ICodeDescription OtherPrepaidCollect
		{
			get => otherPrepaidCollect;
			set => otherPrepaidCollect = SetChild(otherPrepaidCollect, value);
		}

		ICodeDescription otherPrepaidCollect;

		#endregion OtherPrepaidCollect

		#region CarriageValue

		public IMoney CarriageValue
		{
			get => carriageValue;
			set => carriageValue = SetChild(carriageValue, value);
		}
		IMoney carriageValue;

		#endregion CarriageValue

		#region CustomsValue

		public IMoney CustomsValue
		{
			get => customsValue;
			set => customsValue = SetChild(customsValue, value);
		}
		IMoney customsValue;

		#endregion CustomsValue

		#region InsuranceValue

		public IMoney InsuranceValue
		{
			get => insuranceValue;
			set => insuranceValue = SetChild(insuranceValue, value);
		}
		IMoney insuranceValue;

		#endregion InsuranceValue

		#region SpecialHandling

		public IReadOnlyCollection<CargoControlAndTransitSpecialHandling> SpecialHandling
		{
			get => specialHandling;
			set => specialHandling = SetChildCollection(specialHandling, value);
		}
		IReadOnlyCollection<CargoControlAndTransitSpecialHandling> specialHandling;

		#endregion SpecialHandling

		#region SpecialServiceRequest 

		public ZString SpecialServiceRequest
		{
			get => specialServiceRequest;
			set
			{
				if (SetNonPersistentPropertyValue(SpecialServiceRequestInfo, ref specialServiceRequest, value))
				{
					Validate(SpecialServiceRequestInfo);
				}
			}
		}

		ZString specialServiceRequest;

		public ZPropertyInfo SpecialServiceRequestInfo => GetZPropertyInfo(nameof(SpecialServiceRequest));

		#endregion

		#region OtherServiceInformation 

		public ZString OtherServiceInformation
		{
			get => otherServiceInformation;
			set
			{
				if (SetNonPersistentPropertyValue(OtherServiceInformationInfo, ref otherServiceInformation, value))
				{
					Validate(OtherServiceInformationInfo);
				}
			}
		}

		ZString otherServiceInformation;

		public ZPropertyInfo OtherServiceInformationInfo => GetZPropertyInfo(nameof(OtherServiceInformation));

		#endregion

		#endregion Top

		#region Middle

		#region RateLines

		public IReadOnlyCollection<CargoControlAndTransitRateLine> RateLines
		{
			get => rateLines;
			set => rateLines = SetChildCollection(rateLines, value);
		}
		IReadOnlyCollection<CargoControlAndTransitRateLine> rateLines;

		#endregion RateLines

		#region TotalNoOfPieces

		public ZInt TotalNoOfPieces
		{
			get => totalNoOfPieces;
			set
			{
				if (SetNonPersistentPropertyValue(TotalNoOfPiecesInfo, ref totalNoOfPieces, value))
				{
				}
			}
		}
		ZInt totalNoOfPieces;

		public ZPropertyInfo TotalNoOfPiecesInfo => GetZPropertyInfo(nameof(TotalNoOfPieces));

		#endregion

		#region TotalGrossWeight

		public IMeasurement TotalGrossWeight
		{
			get => totalGrossWeight;
			set => totalGrossWeight = SetChild(totalGrossWeight, value);
		}

		IMeasurement totalGrossWeight;

		#endregion

		#endregion Middle

		#region Bottom

		#region TotalWeightPPD

		public ZDecimal TotalWeightPPD
		{
			get => totalWeightPPD;
			set
			{
				if (SetNonPersistentPropertyValue(TotalWeightPPDInfo, ref totalWeightPPD, value))
				{
				}
			}
		}

		ZDecimal totalWeightPPD;

		public ZPropertyInfo TotalWeightPPDInfo => GetZPropertyInfo(nameof(TotalWeightPPD));

		#endregion TotalWeightPPD

		#region TotalWeightCOL

		public ZDecimal TotalWeightCOL
		{
			get => totalWeightCOL;
			set
			{
				if (SetNonPersistentPropertyValue(TotalWeightCOLInfo, ref totalWeightCOL, value))
				{
				}
			}
		}

		ZDecimal totalWeightCOL;

		public ZPropertyInfo TotalWeightCOLInfo => GetZPropertyInfo(nameof(TotalWeightCOL));

		#endregion TotalWeightCOL

		#region ValuationPPD

		public ZDecimal ValuationPPD
		{
			get => valuationPPD;
			set
			{
				if (SetNonPersistentPropertyValue(ValuationPPDInfo, ref valuationPPD, value))
				{
				}
			}
		}

		ZDecimal valuationPPD;

		public ZPropertyInfo ValuationPPDInfo => GetZPropertyInfo(nameof(ValuationPPD));

		#endregion ValuationPPD

		#region ValuationCOL

		public ZDecimal ValuationCOL
		{
			get => valuationCOL;
			set
			{
				if (SetNonPersistentPropertyValue(ValuationCOLInfo, ref valuationCOL, value))
				{
				}
			}
		}

		ZDecimal valuationCOL;

		public ZPropertyInfo ValuationCOLInfo => GetZPropertyInfo(nameof(ValuationCOL));

		#endregion ValuationCOL

		#region TaxesPPD

		public ZDecimal TaxesPPD
		{
			get => taxesPPD;
			set
			{
				if (SetNonPersistentPropertyValue(TaxesPPDInfo, ref taxesPPD, value))
				{
				}
			}
		}

		ZDecimal taxesPPD;

		public ZPropertyInfo TaxesPPDInfo => GetZPropertyInfo(nameof(TaxesPPD));

		#endregion TaxesPPD

		#region TaxesCOL

		public ZDecimal TaxesCOL
		{
			get => taxesCOL;
			set
			{
				if (SetNonPersistentPropertyValue(TaxesCOLInfo, ref taxesCOL, value))
				{
				}
			}
		}

		ZDecimal taxesCOL;

		public ZPropertyInfo TaxesCOLInfo => GetZPropertyInfo(nameof(TaxesCOL));

		#endregion TaxesCOL

		#region OtherChargesDueAgentPPD

		public ZDecimal OtherChargesDueAgentPPD
		{
			get => otherChargesDueAgentPPD;
			set
			{
				if (SetNonPersistentPropertyValue(OtherChargesDueAgentPPDInfo, ref otherChargesDueAgentPPD, value))
				{
				}
			}
		}

		ZDecimal otherChargesDueAgentPPD;

		public ZPropertyInfo OtherChargesDueAgentPPDInfo => GetZPropertyInfo(nameof(OtherChargesDueAgentPPD));

		#endregion OtherChargesDueAgentPPD

		#region OtherChargesDueAgentCOL

		public ZDecimal OtherChargesDueAgentCOL
		{
			get => otherChargesDueAgentCOL;
			set
			{
				if (SetNonPersistentPropertyValue(OtherChargesDueAgentCOLInfo, ref otherChargesDueAgentCOL, value))
				{
				}
			}
		}

		ZDecimal otherChargesDueAgentCOL;

		public ZPropertyInfo OtherChargesDueAgentCOLInfo => GetZPropertyInfo(nameof(OtherChargesDueAgentCOL));

		#endregion OtherChargesDueAgentCOL

		#region OtherChargesDueCarrierPPD

		public ZDecimal OtherChargesDueCarrierPPD
		{
			get => otherChargesDueCarrierPPD;
			set
			{
				if (SetNonPersistentPropertyValue(OtherChargesDueCarrierPPDInfo, ref otherChargesDueCarrierPPD, value))
				{
				}
			}
		}

		ZDecimal otherChargesDueCarrierPPD;

		public ZPropertyInfo OtherChargesDueCarrierPPDInfo => GetZPropertyInfo(nameof(OtherChargesDueCarrierPPD));

		#endregion OtherChargesDueCarrierPPD

		#region OtherChargesDueCarrierCOL

		public ZDecimal OtherChargesDueCarrierCOL
		{
			get => otherChargesDueCarrierCOL;
			set
			{
				if (SetNonPersistentPropertyValue(OtherChargesDueCarrierCOLInfo, ref otherChargesDueCarrierCOL, value))
				{
				}
			}
		}

		ZDecimal otherChargesDueCarrierCOL;

		public ZPropertyInfo OtherChargesDueCarrierCOLInfo => GetZPropertyInfo(nameof(OtherChargesDueCarrierCOL));

		#endregion OtherChargesDueCarrierCOL

		#region ShippersSignature

		public ZString ShippersSignature
		{
			get => shippersSignature;
			set
			{
				if (SetNonPersistentPropertyValue(ShippersSignatureInfo, ref shippersSignature, value))
				{
					Validate(ShippersSignatureInfo);
				}
			}
		}

		ZString shippersSignature;

		public ZPropertyInfo ShippersSignatureInfo => GetZPropertyInfo(nameof(ShippersSignature));

		#endregion ShippersSignature

		#region IssueDate

		public ZDateTime IssueDate
		{
			get => issueDate;
			set
			{
				if (SetNonPersistentPropertyValue(IssueDateInfo, ref issueDate, value))
				{
					Validate(IssueDateInfo);
				}
			}
		}

		ZDateTime issueDate;

		public ZPropertyInfo IssueDateInfo => GetZPropertyInfo(nameof(IssueDate));

		#endregion IssueDate

		#region IssuePlace

		public ZString IssuePlace
		{
			get => issuePlace;
			set
			{
				if (SetNonPersistentPropertyValue(IssuePlaceInfo, ref issuePlace, value))
				{
					Validate(IssuePlaceInfo);
				}
			}
		}

		ZString issuePlace;

		public ZPropertyInfo IssuePlaceInfo => GetZPropertyInfo(nameof(IssuePlace));

		#endregion IssuePlace

		#region AgentsSignature

		public ZString AgentsSignature
		{
			get => agentsSignature;
			set
			{
				if (SetNonPersistentPropertyValue(AgentsSignatureInfo, ref agentsSignature, value))
				{
					Validate(AgentsSignatureInfo);
				}
			}
		}

		ZString agentsSignature;

		public ZPropertyInfo AgentsSignatureInfo => GetZPropertyInfo(nameof(AgentsSignature));

		#endregion AgentsSignature

		#region AgentApprovedExporterNumber

		public ZString AgentApprovedExporterNumber
		{
			get => agentApprovedExporterNumber;
			set
			{
				if (SetNonPersistentPropertyValue(AgentApprovedExporterNumberInfo, ref agentApprovedExporterNumber, value))
				{
					Validate(AgentApprovedExporterNumberInfo);
				}
			}
		}

		ZString agentApprovedExporterNumber;

		public ZPropertyInfo AgentApprovedExporterNumberInfo => GetZPropertyInfo(nameof(AgentApprovedExporterNumber));

		#endregion AgentApprovedExporterNumber

		#region RUCReferenceNumber

		public ZString RUCReferenceNumber
		{
			get => rUCReferenceNumber;
			set
			{
				if (SetNonPersistentPropertyValue(RUCReferenceNumberInfo, ref rUCReferenceNumber, value))
				{
				}
			}
		}

		ZString rUCReferenceNumber;

		public ZPropertyInfo RUCReferenceNumberInfo => GetZPropertyInfo(nameof(RUCReferenceNumber));

		#endregion RUCReferenceNumber

		#region CustomsWarehouse

		public ZString CustomsWarehouse
		{
			get => customsWarehouse;
			set
			{
				if (SetNonPersistentPropertyValue(CustomsWarehouseInfo, ref customsWarehouse, value))
				{
				}
			}
		}

		ZString customsWarehouse;

		public ZPropertyInfo CustomsWarehouseInfo => GetZPropertyInfo(nameof(CustomsWarehouse));

		#endregion CustomsWarehouse

		#region WoodenParts

		public ZBool WoodenParts
		{
			get => woodenParts;
			set
			{
				if (SetNonPersistentPropertyValue(WoodenPartsInfo, ref woodenParts, value))
				{
					Validate(WoodenPartsInfo);
				}
			}
		}

		ZBool woodenParts;

		public ZPropertyInfo WoodenPartsInfo => GetZPropertyInfo(nameof(WoodenParts));

		#endregion

		#region TotalPrepaid

		public ZDecimal TotalPrepaid
		{
			get => totalPrepaid;
			set
			{
				if (SetNonPersistentPropertyValue(TotalPrepaidInfo, ref totalPrepaid, value))
				{
					Validate(TotalPrepaidInfo);
					Validate(TotalCollectInfo);
				}
			}
		}

		ZDecimal totalPrepaid;

		public ZPropertyInfo TotalPrepaidInfo => GetZPropertyInfo(nameof(TotalPrepaid));

		#endregion TotalPrepaid

		#region TotalCollect

		public ZDecimal TotalCollect
		{
			get => totalCollect;
			set
			{
				if (SetNonPersistentPropertyValue(TotalCollectInfo, ref totalCollect, value))
				{
					Validate(TotalCollectInfo);
					Validate(TotalPrepaidInfo);
				}
			}
		}

		ZDecimal totalCollect;

		public ZPropertyInfo TotalCollectInfo => GetZPropertyInfo(nameof(TotalCollect));

		#endregion TotalCollect

		#region IsSignatureReadOnly

		public ZBool IsSignatureReadOnly
		{
			get => isSignatureReadOnly;
			set
			{
				if (SetNonPersistentPropertyValue(IsSignatureReadOnlyInfo, ref isSignatureReadOnly, value))
				{
					Validate(IsSignatureReadOnlyInfo);
				}
			}
		}

		ZBool isSignatureReadOnly;

		public ZPropertyInfo IsSignatureReadOnlyInfo => GetZPropertyInfo(nameof(IsSignatureReadOnly));

		#endregion

		#endregion Bottom

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion
	}
}
