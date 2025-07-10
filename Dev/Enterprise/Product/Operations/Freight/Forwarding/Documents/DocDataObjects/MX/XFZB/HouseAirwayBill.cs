using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.MX
{
	sealed class HouseAirwayBill : DocDataObject, IDataSourceProvider
	{
		public HouseAirwayBill(ZString sourceType, ZString sourceID)
		{
			this.sourceType = sourceType;
			this.sourceID = sourceID;
		}

		#region IDataSourceProvider members

		ZString IDataSourceProvider.SourceID => sourceID;
		readonly ZString sourceID;

		ZString IDataSourceProvider.SourceType => sourceType;
		readonly ZString sourceType;

		#endregion

		#region ErrorPlaceHolder
		public ZString ErrorPlaceHolder
		{
			get => errorPlaceHolder;
			set
			{
				if (SetNonPersistentPropertyValue(ErrorPlaceHolderInfo, ref errorPlaceHolder, value))
				{
					Validate(ErrorPlaceHolderInfo);
				}
			}
		}

		ZString errorPlaceHolder;

		public ZPropertyInfo ErrorPlaceHolderInfo => GetZPropertyInfo(nameof(ErrorPlaceHolder));

		#endregion ErrorPlaceHolder

		#region MessageHeaderDocument

		#region SendingParty

		public IAddress SendingParty
		{
			get => sendingParty;
			set => sendingParty = SetChild(sendingParty, value);
		}
		IAddress sendingParty;

		#endregion

		#region SendingPartyCode

		public RegistrationNumber SendingPartyCode
		{
			get => sendingPartyCode;
			set => sendingPartyCode = SetChild(sendingPartyCode, value);
		}

		RegistrationNumber sendingPartyCode;

		#endregion

		#endregion

		#region BusinessHeaderDocument

		#region AWBNumber

		public ZString AWBNumber
		{
			get => awbNumber;
			set
			{
				if (SetNonPersistentPropertyValue(AWBNumberInfo, ref awbNumber, value))
				{
					Validate(AWBNumberInfo);
				}
			}
		}

		ZString awbNumber;

		public ZPropertyInfo AWBNumberInfo => GetZPropertyInfo(nameof(AWBNumber));

		#endregion AWBNumber

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

		#endregion

		#region MasterConsignment

		#region AirlinePrefix

		public ZString AirlinePrefix
		{
			get => airlinePrefix;
			set
			{
				if (SetNonPersistentPropertyValue(AirlinePrefixInfo, ref airlinePrefix, value))
				{
					Validate(AirlinePrefixInfo);
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
					Validate(SerialNoInfo);
				}
			}
		}

		ZString serialNo;

		public ZPropertyInfo SerialNoInfo => GetZPropertyInfo(nameof(SerialNo));

		#endregion SerialNo

		#region AirportOfDeparture

		public IUnloco AirportOfDeparture
		{
			get => airportOfDeparture;
			set => airportOfDeparture = SetChild(airportOfDeparture, value);
		}
		IUnloco airportOfDeparture;

		#endregion AirportOfDeparture

		#region AirportOfDestination

		public IUnloco AirportOfDestination
		{
			get => airportOfDestination;
			set => airportOfDestination = SetChild(airportOfDestination, value);
		}
		IUnloco airportOfDestination;

		#endregion AirportOfDestination

		#endregion

		#region IncludedHouseConsignment

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

		#region TotalWeightPPD

		public ZDecimal TotalWeightPPD
		{
			get => totalWeightPPD;
			set
			{
				if (SetNonPersistentPropertyValue(TotalWeightPPDInfo, ref totalWeightPPD, value))
				{
					Validate(TotalWeightPPDInfo);
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
					Validate(TotalWeightCOLInfo);
				}
			}
		}

		ZDecimal totalWeightCOL;

		public ZPropertyInfo TotalWeightCOLInfo => GetZPropertyInfo(nameof(TotalWeightCOL));

		#endregion TotalWeightCOL

		#region Currency

		public ICodeDescription Currency
		{
			get => currency;
			set => currency = SetChild(currency, value);
		}

		ICodeDescription currency;

		#endregion Currency

		#region ValuationPPD

		public ZDecimal ValuationPPD
		{
			get => valuationPPD;
			set
			{
				if (SetNonPersistentPropertyValue(ValuationPPDInfo, ref valuationPPD, value))
				{
					Validate(ValuationPPDInfo);
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
					Validate(ValuationCOLInfo);
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
					Validate(TaxesPPDInfo);
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
					Validate(TaxesCOLInfo);
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
					Validate(OtherChargesDueAgentPPDInfo);
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
					Validate(OtherChargesDueAgentCOLInfo);
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
					Validate(OtherChargesDueCarrierPPDInfo);
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
					Validate(OtherChargesDueCarrierCOLInfo);
				}
			}
		}

		ZDecimal otherChargesDueCarrierCOL;

		public ZPropertyInfo OtherChargesDueCarrierCOLInfo => GetZPropertyInfo(nameof(OtherChargesDueCarrierCOL));

		#endregion OtherChargesDueCarrierCOL

		#region TotalPrepaid

		public ZDecimal TotalPrepaid
		{
			get => totalPrepaid;
			set
			{
				if (SetNonPersistentPropertyValue(TotalPrepaidInfo, ref totalPrepaid, value))
				{
					Validate(TotalPrepaidInfo);
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
				}
			}
		}

		ZDecimal totalCollect;

		public ZPropertyInfo TotalCollectInfo => GetZPropertyInfo(nameof(TotalCollect));

		#endregion TotalCollect

		#region WeightPrepaidCollect
		public ICodeDescription WeightPrepaidCollect
		{
			get => weightPrepaidCollect;
			set => weightPrepaidCollect = SetChild(weightPrepaidCollect, value);
		}

		ICodeDescription weightPrepaidCollect;

		#endregion

		#region OtherPrepaidCollect

		public ICodeDescription OtherPrepaidCollect
		{
			get => otherPrepaidCollect;
			set => otherPrepaidCollect = SetChild(otherPrepaidCollect, value);
		}

		ICodeDescription otherPrepaidCollect;

		#endregion

		#region TotalGrossWeight

		public IMeasurement TotalGrossWeight
		{
			get => totalGrossWeight;
			set => totalGrossWeight = SetChild(totalGrossWeight, value);
		}

		IMeasurement totalGrossWeight;

		#endregion

		#region TotalNoOfPieces

		public ZInt TotalNoOfPieces
		{
			get => totalNoOfPieces;
			set
			{
				if (SetNonPersistentPropertyValue(TotalNoOfPiecesInfo, ref totalNoOfPieces, value))
				{
					Validate(TotalNoOfPiecesInfo);
				}
			}
		}
		ZInt totalNoOfPieces;

		public ZPropertyInfo TotalNoOfPiecesInfo => GetZPropertyInfo(nameof(TotalNoOfPieces));

		#endregion

		#region SpecialHandling

		public IReadOnlyCollection<HouseAirwayBillSpecialHandling> SpecialHandling
		{
			get => specialHandling;
			set => specialHandling = SetChildCollection(specialHandling, value);
		}
		IReadOnlyCollection<HouseAirwayBillSpecialHandling> specialHandling;

		#endregion SpecialHandling

		#endregion

		#region Parties

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

		#region ExportAgent

		public IAddress ExportAgent
		{
			get => exportAgent;
			set => exportAgent = SetChild(exportAgent, value);
		}
		IAddress exportAgent;

		#endregion

		#region ExportAgentCode

		public RegistrationNumber ExportAgentCode
		{
			get => exportAgentCode;
			set => exportAgentCode = SetChild(exportAgentCode, value);
		}

		RegistrationNumber exportAgentCode;

		#endregion

		#endregion

		#region IncludedHouseConsignmentItem

		#region RateLines

		public IReadOnlyCollection<HouseAirwayBillRateLine> RateLines
		{
			get => rateLines;
			set => rateLines = SetChildCollection(rateLines, value);
		}
		IReadOnlyCollection<HouseAirwayBillRateLine> rateLines;

		#endregion RateLines

		#endregion
	}
}
