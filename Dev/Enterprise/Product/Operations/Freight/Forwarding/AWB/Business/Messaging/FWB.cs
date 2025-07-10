using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business.AWB;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	/**
	 * <summary>
	 * An airway bill data message (FWB) is a type of <see cref="CargoIMP">CargoIMP</see> message.
	 * It is the electronic equivalent to the Master Airway Bill (MAWB).
	 * </summary>
	 */
	public class FWB : FBase
	{
		public enum Version { No10 = 10, No16 = 16, None = 0 }

		public const int NatureAndQtyMaxLength = 20;
		public const int NatureAndQtyMaxLines = 12;
		public const string NatureAndQtyCharactersToKeep = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ. -"; // Developer constant

		#region Message Construction

		public FWB(IFWBMessageDetailsProvider fwbDetailsProvider, Version version, bool includeSecurityDeclaration = false)
			: base(fwbDetailsProvider)
		{
			this.version = version;
			this.includeSecurityDeclaration = includeSecurityDeclaration;
		}

		readonly Version version;
		readonly bool includeSecurityDeclaration;

		public override ZString StandardMessageIdentifier
		{
			get { return MessageTypes.FWB; }
		}

		protected override CharType OCIMUFormatType
		{
			get
			{
				if (MessageTypeVersionNumber == "16")
				{
					return CharType.EmailUserNameV16;
				}
				else
				{
					return CharType.Text;
				}
			}
		}

		protected override CharType OCIMDFormatType
		{
			get
			{
				if (MessageTypeVersionNumber == "16")
				{
					return CharType.EmailDomainNameV16;
				}
				else
				{
					return CharType.Text;
				}
			}
		}

		protected override ZString MessageTypeVersionNumber
		{
			get
			{
				switch (version)
				{
					case Version.No10:
						return "10";
					case Version.No16:
						return "16";
					default:
						throw new InvalidOperationException("FWB Version." + version.ToString() + " support not implemented properly.");
				}
			}
		}

		protected override void ConstructMessage()
		{
			AddAWBConsignmentDetails();
			AddFlightBookings();
			AddRouting();
			AddShipper();
			AddConsignee();
			AddAgent();
			AddSpecialServiceRequest();
			AddAlsoNotify();
			AddAccountingInformation();
			AddChargeDeclarations(FWBDetailsProvider, true);
			AddRateDescription();
			AddOtherCharges();
			AddChargeSummary();
			AddShippersCertification();
			AddCarriersExecution();
			AddOtherServiceInformation();
			AddSenderReference();
			AddCustomsOrigin();
			AddSpecialHandlingDetails();
			AddOtherCustomsInformation();
		}

		#endregion

		#region Implementation

		void AddShipper()
		{
			Elements.AddHeader(GetPartyElements(true, "SHP", FWBDetailsProvider.ShipperAccount, FWBDetailsProvider.ShipperName, FWBDetailsProvider.ShipperAddress, FWBDetailsProvider.ShipperAddress2, FWBDetailsProvider.ShipperPlace, FWBDetailsProvider.ShipperState, FWBDetailsProvider.ShipperCountryCode, FWBDetailsProvider.ShipperPostCode, FWBDetailsProvider.ShipperContactCode, FWBDetailsProvider.ShipperContactDetail));
		}

		void AddConsignee()
		{
			Elements.AddHeader(GetPartyElements(true, "CNE", FWBDetailsProvider.ConsigneeAccount, FWBDetailsProvider.ConsigneeName, FWBDetailsProvider.ConsigneeAddress, FWBDetailsProvider.ConsigneeAddress2, FWBDetailsProvider.ConsigneePlace, FWBDetailsProvider.ConsigneeState, FWBDetailsProvider.ConsigneeCountryCode, FWBDetailsProvider.ConsigneePostCode, FWBDetailsProvider.ConsigneeContactCode, FWBDetailsProvider.ConsigneeContactDetail));
		}

		void AddAlsoNotify()
		{
			if (SsrNfyOsiCorCount <= SsrNfyOsiCorLimit)
			{
				Elements.AddOptionalHeader(NotifyPartyElementList);
			}
		}

		ElementList NotifyPartyElementList
		{
			get
			{
				return notifyPartyElementList ?? (notifyPartyElementList = GetPartyElements(
					false, "NFY", "", FWBDetailsProvider.AlsoNotifyName, FWBDetailsProvider.AlsoNotifyAddress, FWBDetailsProvider.AlsoNotifyAddress2,
					FWBDetailsProvider.AlsoNotifyPlace, FWBDetailsProvider.AlsoNotifyState, FWBDetailsProvider.AlsoNotifyCountryCode,
					FWBDetailsProvider.AlsoNotifyPostCode, FWBDetailsProvider.AlsoNotifyContactCode, FWBDetailsProvider.AlsoNotifyContactDetail));
			}
		}

		ElementList notifyPartyElementList;

		void AddAWBConsignmentDetails()
		{
			ElementList aWBConsignmentDetails = new ElementList();

			aWBConsignmentDetails.AddHeader(AWBIdentification);
			aWBConsignmentDetails.AddHeader(AWBOriginAndDestination);
			aWBConsignmentDetails.AddHeader(AWBQuantityDetail);

			aWBConsignmentDetails.AddCRLF();

			Elements.AddHeader(aWBConsignmentDetails);
		}

		void AddFlightBookings()
		{
			//this segment is only to be inculded if all information in the first segment is available
			//each subsequent segment is only to be inculuded if all information in the subsequent 
			//segments are available

			if (!FWBDetailsProvider.Booking1stCarrier.IsEmpty && !FWBDetailsProvider.Booking1stFlight.IsEmpty && !FWBDetailsProvider.Booking1stFlightDate.IsEmpty)
			{
				ElementList flightBookings = new ElementList();
				flightBookings.AddLineIdentifier("FLT");

				//repeated
				ElementList valueElements = new ElementList();
				valueElements.AddSlant();
				valueElements.AddValue(new Format(2, CharType.AlphaNumeric), FWBDetailsProvider.Booking1stCarrier);
				valueElements.AddValue(new Format(5, CharType.AlphaNumeric), FWBDetailsProvider.Booking1stFlight);
				valueElements.AddSlant();
				valueElements.AddValue(new Format(2, CharType.Numeric), FWBDetailsProvider.Booking1stFlightDate);
				flightBookings.AddHeader(valueElements);

				if (!FWBDetailsProvider.Booking2ndCarrier.IsEmpty && !FWBDetailsProvider.Booking2ndFlight.IsEmpty && !FWBDetailsProvider.Booking2ndFlightDate.IsEmpty)
				{
					valueElements = new ElementList();
					valueElements.AddSlant();
					valueElements.AddValue(new Format(2, CharType.AlphaNumeric), FWBDetailsProvider.Booking2ndCarrier);
					valueElements.AddValue(new Format(5, CharType.AlphaNumeric), FWBDetailsProvider.Booking2ndFlight);
					valueElements.AddSlant();
					valueElements.AddValue(new Format(2, CharType.Numeric), FWBDetailsProvider.Booking2ndFlightDate);
					//second occurence is optional			
					flightBookings.AddOptionalHeader(valueElements);
				}

				flightBookings.AddCRLF();

				Elements.AddOptionalHeader(flightBookings);
			}
		}

		void AddRouting()
		{
			ElementList routing = new ElementList();
			routing.AddLineIdentifier("RTG");

			ElementList valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddOptionalValue(new Format(3, CharType.Alpha), FWBDetailsProvider.To1st);
			valueElements.AddValue(new Format(2, CharType.AlphaNumeric), FWBDetailsProvider.By1st);
			routing.AddHeader(valueElements);

			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddValue(new Format(3, CharType.Alpha), FWBDetailsProvider.To2nd);
			valueElements.AddOptionalValue(new Format(2, CharType.AlphaNumeric), FWBDetailsProvider.By2nd);
			routing.AddOptionalHeader(valueElements);

			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddValue(new Format(3, CharType.Alpha), FWBDetailsProvider.To3rd);
			valueElements.AddOptionalValue(new Format(2, CharType.AlphaNumeric), FWBDetailsProvider.By3rd);
			routing.AddOptionalHeader(valueElements);

			routing.AddCRLF();

			Elements.AddHeader(routing);
		}

		void AddAgent()
		{
			if (FWBDetailsProvider.AgentIATACodeFormatted.IsEmpty)
			{
				return;
			}

			ElementList agent = new ElementList();
			agent.AddLineIdentifier("AGT");

			//Agent Account Detail
			ElementList valueElements = new ElementList();
			//AccountNumber
			valueElements.AddSlant();
			valueElements.AddValue(new Format(14, 0, CharType.Text), FWBDetailsProvider.AgentAccountNo);
			ZString agentIATACodeAndCASSAddress = FWBDetailsProvider.AgentIATACodeFormatted.KeepChars("0123456789");
			//IATA Cargo Agent Numeric Code
			valueElements.AddSlant();
			valueElements.AddValue(new Format(7, CharType.Numeric), agentIATACodeAndCASSAddress.Left(7));
			//IATA Cargo Agent CASS Address
			valueElements.AddSlant(StatusType.Conditional);
			valueElements.AddOptionalValue(new Format(4, CharType.Numeric), agentIATACodeAndCASSAddress.SubstringSafe(7, 4));
			//Participant Identifier
			valueElements.AddSlant(StatusType.Conditional);
			valueElements.AddOptionalValue(new Format(3, 0, CharType.AlphaNumeric), FWBDetailsProvider.AgentParticipantIdentifier);
			agent.AddConditionalHeader(valueElements);

			agent.AddCRLF();

			//Agent Name
			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddValue(new Format(35, 0, CharType.Text), FWBDetailsProvider.AgentName);
			valueElements.AddCRLF();
			agent.AddHeader(valueElements);

			//Agent Place
			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddValue(new Format(17, 0, CharType.Text), FWBDetailsProvider.AgentPlace);
			valueElements.AddCRLF();
			agent.AddHeader(valueElements);

			Elements.AddHeader(agent);
		}

		void AddAccountingInformation()
		{
			ElementList accountingInformationElements = new ElementList();
			accountingInformationElements.AddLineIdentifier("ACC");

			ElementList valueElements;

			int noAccInformationsToAdd = 6;
			foreach (var num in FWBDetailsProvider.CustomsEntryNumbers.Where(IncludeCustomsEntryNumber))
			{
				noAccInformationsToAdd--;

				valueElements = new ElementList();
				valueElements.AddSlant();
				valueElements.AddValue(new Format(3, CharType.Alpha), "GEN");
				valueElements.AddSlant();
				valueElements.AddValue(new Format(34, 0, CharType.Text), string.Concat(num.Type, " ", num.Number).Trim());
				valueElements.AddCRLF();
				accountingInformationElements.AddHeader(valueElements);

				if (noAccInformationsToAdd == 0)
				{
					break;
				}
			}

			if (!FWBDetailsProvider.NetRateCode.IsEmpty && noAccInformationsToAdd > 0)
			{
				noAccInformationsToAdd--;

				valueElements = new ElementList();
				valueElements.AddSlant();
				valueElements.AddValue(new Format(3, CharType.Alpha), "GEN");
				valueElements.AddSlant();
				valueElements.AddValue(new Format(34, 0, CharType.Text), "NETRATE " + FWBDetailsProvider.NetRateCode);
				valueElements.AddCRLF();
				accountingInformationElements.AddHeader(valueElements);
			}

			var informations = FWBDetailsProvider.AWBAccountingInformations.Cast<IAWBAccountingInformationMessageDetailsProvider>().Where(info => !info.IsSkippedOnMessaging).Take(noAccInformationsToAdd);
			foreach (var information in informations)
			{
				if (!information.Information.IsCargoIMPEmpty())
				{
					valueElements = new ElementList();
					valueElements.AddSlant();
					string accountingCode = information.InformationID.IsEmpty ? "GEN" : information.InformationID.ToString();
					valueElements.AddValue(new Format(3, CharType.Alpha), accountingCode);
					valueElements.AddSlant();
					valueElements.AddValue(new Format(34, 0, CharType.Text), information.Information);
					valueElements.AddCRLF();
					accountingInformationElements.AddHeader(valueElements);
				}
			}

			Elements.AddOptionalHeader(accountingInformationElements);
		}

		bool IncludeCustomsEntryNumber(IAWBEntryNumberMessageDetailsProvider numberDetailsProvider)
		{
			return !numberDetailsProvider.Type.IsEmpty
				&& !numberDetailsProvider.Number.IsEmpty
				&& numberDetailsProvider.Type != CusEntryNumberTypes.Standard.MovementReferenceNumber;
		}

		void AddRateDescription()
		{
			var rateDescription = new ElementList();
			rateDescription.AddLineIdentifier("RTD");

			var rateLineCounter = (ZInt)0;

			var provider = new FWBRateLinesProvider(
				FWBDetailsProvider.AWBRateLines.Cast<ExportAWBRateLine>(),
				version);

			var rateLines = provider.CreateFWBRateLines();

			var uldRateLineProcessed = false;
			var dimensionsNatureAndQtyProcessed = false;
			var volumeNatureAndQtyProcessed = false;

			foreach (var rateLine in rateLines)
			{
				ElementList valueElements = new ElementList();
				valueElements.AddSlant();
				rateLineCounter++;
				valueElements.AddValue(new Format(2, 0, CharType.Numeric), rateLineCounter);
				rateDescription.AddHeader(valueElements);

				if (!rateLine.IsRateDescriptionEmpty)
				{
					//Number of Pieces
					valueElements = new ElementList();
					valueElements.AddSlant();

					valueElements.AddColumnIdentifier("P");
					if (rateLine.NoOfPiecesOrRCP.IsNumbersOnlyOrEmpty)
					{
						valueElements.AddValue(new Format(4, 0, CharType.Numeric), rateLine.NoOfPiecesOrRCP);
					}
					else
					{
						valueElements.AddValue(new Format(3, 0, CharType.AlphaNumeric), rateLine.NoOfPiecesOrRCP);
					}
					rateDescription.AddOptionalHeader(valueElements);

					//Gross Weight Details
					valueElements = new ElementList();
					valueElements.AddSlant();
					valueElements.AddValue(new Format(1, CharType.Alpha), rateLine.WeightInLBsOrKGs);
					valueElements.AddValue(new Format(7, 0, CharType.NumericWithDecimal), rateLine.GrossWeight);
					rateDescription.AddOptionalHeader(valueElements);

					//Rate Class Details
					valueElements = new ElementList();
					valueElements.AddSlant();
					valueElements.AddColumnIdentifier("C");
					valueElements.AddValue(new Format(1, CharType.Alpha), rateLine.RateClass);
					rateDescription.AddOptionalHeader(valueElements);

					//Commodity Item Number Details
					valueElements = new ElementList();
					valueElements.AddSlant();
					valueElements.AddColumnIdentifier("S");
					valueElements.AddValue(new Format(7, 0, CharType.AlphaNumeric), rateLine.CommodityItemNumber);
					rateDescription.AddOptionalHeader(valueElements);

					//Chargeable Weight Details
					valueElements = new ElementList();
					valueElements.AddSlant();
					valueElements.AddColumnIdentifier("W");
					valueElements.AddValue(new Format(7, 0, CharType.NumericWithDecimal), rateLine.ChargeableWeight);
					rateDescription.AddOptionalHeader(valueElements);

					//Rate/Charge Details
					valueElements = new ElementList();
					valueElements.AddSlant();
					valueElements.AddColumnIdentifier("R");
					valueElements.AddValue(new Format(8, 0, CharType.NumericWithDecimal), rateLine.RateChargeOrDiscount);
					rateDescription.AddOptionalHeader(valueElements);

					//Total Details
					valueElements = new ElementList();
					valueElements.AddSlant();
					valueElements.AddColumnIdentifier("T");
					valueElements.AddValue(new Format(12, 0, CharType.NumericWithDecimal), rateLine.Total);
					rateDescription.AddOptionalHeader(valueElements);

					rateDescription.AddCRLF();
				}

				if (rateLine.NatureAndQtyOfGoods != null)
				{
					var identifier = (version == Version.No16 && FreightDataRegistry.Instance.SendFWBNatureAndQuantityOfGoodsType.Value)
						? rateLine.NatureAndQtyOfGoods.Type
						: (ZString)Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription;

					// Nature and Quantity of Goods
					valueElements = new ElementList();
					valueElements.AddSlant();
					valueElements.AddColumnIdentifier("N");

					// Adds column identifier 'U' for RateClass 'X' line. Once we add support to auto-populate 'U' records from consol then this
					// can be removed.
					if (rateLine.RateClass == Core.Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation)//X RateLine
					{
						valueElements.AddColumnIdentifier("U");
					}
					else if (rateLine.NatureAndQtyOfGoods.Type == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery)
					{
						valueElements.AddColumnIdentifier(Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription);
					}
					else
					{
						valueElements.AddColumnIdentifier(identifier);
					}

					valueElements.AddSlant();

					switch (identifier)
					{
						case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions:
							var dims = (FWBNatureAndQtyOfGoodsDimensions)rateLine.NatureAndQtyOfGoods;

							var iataDims = ConvertDimensionsToIATA(new Dimensions
							{
								Length = dims.Length,
								Width = dims.Width,
								Height = dims.Height,
								Unit = dims.Unit
							});

							Format unitFormat = new Format(3, 3, CharType.AlphaNumeric);
							Format dimensionFormat = new Format(5, 1, CharType.Numeric);
							Format countFormat = new Format(4, 1, CharType.Numeric);

							valueElements.AddSlant();
							valueElements.AddValue(unitFormat, iataDims.Unit);
							valueElements.AddValue(dimensionFormat, iataDims.Length);
							valueElements.AddHyphen();
							valueElements.AddValue(dimensionFormat, iataDims.Width);
							valueElements.AddHyphen();
							valueElements.AddValue(dimensionFormat, iataDims.Height);
							valueElements.AddSlant();
							valueElements.AddValue(countFormat, dims.Count);

							dimensionsNatureAndQtyProcessed = true;

							break;

						case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.Volume:
							var vol = (FWBNatureAndQtyOfGoodsVolume)rateLine.NatureAndQtyOfGoods;

							var iataVolume = ConvertVolumeToIATA(new Volume
							{
								Value = vol.Value,
								Unit = vol.Unit
							});

							valueElements.AddValue(new Format(2, CharType.Alpha), iataVolume.Unit);
							valueElements.AddValue(new Format(9, 1, CharType.NumericWithDecimal), iataVolume.Value);

							volumeNatureAndQtyProcessed = true;

							break;

						case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount:
							var slac = (FWBNatureAndQtyOfGoodsSLAC)rateLine.NatureAndQtyOfGoods;
							valueElements.AddValue(new Format(5, 0, CharType.Numeric), slac.Count);
							break;

						case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin:
							var origin = (FWBNatureAndQtyOfGoodsOrigin)rateLine.NatureAndQtyOfGoods;
							valueElements.AddValue(new Format(2, 2, CharType.Alpha), origin.Country);
							break;

						case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery:
							var lithiumBatteryCode = rateLine.NatureAndQtyOfGoods.Text.ReplaceIgnoringCase((NoResString)"LITHIUM BATTERY:", (NoResString)"LI BATT"); // Business Constant
							valueElements.AddValue(new Format(20, 0, CharType.Text), lithiumBatteryCode);
							break;

						case Core.Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode:
							valueElements.AddValue(new Format(20, 0, CharType.Text), rateLine.NatureAndQtyOfGoods.Text.Replace("HS Code: ", string.Empty)); // Harmonised Code Description
							break;

						default:
							valueElements.AddValue(new Format(20, 0, CharType.Text), rateLine.NatureAndQtyOfGoods.Text);
							break;
					}

					rateDescription.AddOptionalHeader(valueElements);
					rateDescription.AddCRLF();
				}

				uldRateLineProcessed = uldRateLineProcessed
					|| rateLine.RateClass == Core.Constants.AWB.RateClass.UnitLoadDeviceAdditionalInformation;
			}

			//Dimensions. Sends as NDA on all non-ULD messages unless there is a 'D' rateline.
			if (version == Version.No16
				&& !uldRateLineProcessed
				&& !dimensionsNatureAndQtyProcessed
				&& !volumeNatureAndQtyProcessed
				&& rateLineCounter < NatureAndQtyMaxLines)
			{
				rateLineCounter++;

				ElementList elements = new ElementList();
				elements.AddSlant();
				elements.AddValue(new Format(2, 0, CharType.Numeric), rateLineCounter);
				elements.AddSlant();
				elements.AddColumnIdentifier("N");
				elements.AddColumnIdentifier("D");
				elements.AddSlant();
				elements.AddSlant();
				elements.AddValue(new Format(3, CharType.Alpha), "NDA");

				rateDescription.AddOptionalHeader(elements);
				rateDescription.AddCRLF();
			}

			Elements.AddHeader(rateDescription);
		}

		void AddOtherCharges()
		{
			ElementList otherChargesElements = new ElementList();
			otherChargesElements.AddLineIdentifier("OTH");

			foreach (var otherCharge in FWBDetailsProvider.AWBOtherCharges)
			{
				ElementList valueElements = new ElementList();
				valueElements.AddSlant();
				valueElements.AddValue(new Format(1, CharType.Alpha), FWBDetailsProvider.OtherPPDCOL.Left(1));
				valueElements.AddSlant();
				otherChargesElements.AddHeader(valueElements);

				valueElements = new ElementList();
				valueElements.AddValue(new Format(2, CharType.Alpha), otherCharge.ChargeCode);
				valueElements.AddValue(new Format(1, CharType.Alpha), otherCharge.EntitlementCode);
				valueElements.AddValue(new Format(12, 0, CharType.NumericWithDecimal), otherCharge.Amount);
				otherChargesElements.AddHeader(valueElements);
				otherChargesElements.AddCRLF();
			}

			Elements.AddOptionalHeader(otherChargesElements);
		}

		void AddChargeSummary()
		{
			var isPrepaid = FWBDetailsProvider.WeightVPPDCOL.Left(1) == ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			var prepaidChargeSummary = GetChargeSummary(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid, FWBDetailsProvider.TotalWeightPPD, FWBDetailsProvider.ValuationPPD, FWBDetailsProvider.TaxesPPD, FWBDetailsProvider.OtherChargesDueAgentPPD, FWBDetailsProvider.OtherChargesDueCarrierPPD, FWBDetailsProvider.TotalPPD);

			if ((version == Version.No16 && (isPrepaid || FWBDetailsProvider.WeightVPPDCOL.IsEmpty))
				|| (ForwardingConfigurationRegistry.Instance.ForceSendingOfFWBCVDSegment.Value && isPrepaid))
			{
				Elements.AddHeader(prepaidChargeSummary);
			}
			else
			{
				Elements.AddOptionalHeader(prepaidChargeSummary);
			}

			var isCollect = FWBDetailsProvider.WeightVPPDCOL.Left(1) == ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			var collectChargeSummary = GetChargeSummary(ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect, FWBDetailsProvider.TotalWeightCOL, FWBDetailsProvider.ValuationCOL, FWBDetailsProvider.TaxesCOL, FWBDetailsProvider.OtherChargesDueAgentCOL, FWBDetailsProvider.OtherChargesDueCarrierCOL, FWBDetailsProvider.TotalCOL);

			if (isCollect && (version == Version.No16 || ForwardingConfigurationRegistry.Instance.ForceSendingOfFWBCVDSegment.Value))
			{
				Elements.AddHeader(collectChargeSummary);
			}
			else
			{
				Elements.AddOptionalHeader(collectChargeSummary);
			}
		}

		ElementList GetChargeSummary(ZString lineIdentifier, ZDecimal weight, ZDecimal valuation, ZDecimal taxes, ZDecimal otherAgentCharges, ZDecimal otherCarrierCharges, ZDecimal total)
		{
			ElementList chargeSummary = new ElementList();
			chargeSummary.AddLineIdentifier(lineIdentifier);

			ElementList valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddColumnIdentifier("WT");
			valueElements.AddValue(new Format(12, 0, CharType.NumericWithDecimal), weight);
			chargeSummary.AddOptionalHeader(valueElements);

			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddColumnIdentifier("VC");
			valueElements.AddValue(new Format(12, 0, CharType.NumericWithDecimal), valuation);
			chargeSummary.AddOptionalHeader(valueElements);

			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddColumnIdentifier("TX");
			valueElements.AddValue(new Format(12, 0, CharType.NumericWithDecimal), taxes);
			chargeSummary.AddOptionalHeader(valueElements);

			chargeSummary.AddCRLF();

			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddColumnIdentifier("OA");
			valueElements.AddValue(new Format(12, 0, CharType.NumericWithDecimal), otherAgentCharges);
			chargeSummary.AddOptionalHeader(valueElements);

			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddColumnIdentifier("OC");
			valueElements.AddValue(new Format(12, 0, CharType.NumericWithDecimal), otherCarrierCharges);
			chargeSummary.AddOptionalHeader(valueElements);

			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddColumnIdentifier("CT");
			valueElements.AddValue(new Format(12, 0, CharType.NumericWithDecimal), total);

			chargeSummary.AddHeader(valueElements);

			chargeSummary.AddCRLF();

			return chargeSummary;
		}

		void AddShippersCertification()
		{
			ElementList shippersCertification = new ElementList();
			shippersCertification.AddLineIdentifier("CER");

			ElementList valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddValue(new Format(20, 0, CharType.Text), FWBDetailsProvider.ShippersSignature);
			valueElements.AddCRLF();
			shippersCertification.AddHeader(valueElements);

			Elements.AddOptionalHeader(shippersCertification);
		}

		void AddCarriersExecution()
		{
			ElementList carriersExecution = new ElementList();
			carriersExecution.AddLineIdentifier("ISU");

			ElementList valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddValue(new Format(7, 7, CharType.AlphaNumeric), FWBDetailsProvider.AWBIssueDate.ToString("ddMMMyy", CultureInfo.InvariantCulture));
			valueElements.AddSlant();
			valueElements.AddValue(new Format(17, 0, CharType.Text), FWBDetailsProvider.AWBIssuePlace);
			carriersExecution.AddHeader(valueElements);

			valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddValue(new Format(20, 0, CharType.Text), FWBDetailsProvider.AWBAgentsSignature);
			carriersExecution.AddOptionalHeader(valueElements);

			carriersExecution.AddCRLF();

			Elements.AddHeader(carriersExecution);
		}

		void AddOtherServiceInformation()
		{
			Elements.AddOptionalHeader(OtherServiceInformation);
		}

		#region Constants

		const int SsrNfyOsiCorLimit = 216;
		const int MaxOSICharPerLine = 65;

		#endregion

		#region SSR NFY OSI COR Characters

		int SsrNfyOsiCorCount
		{
			get
			{
				if (ssrNfyOsiCorCount == 0)
				{
					ssrNfyOsiCorCount = GetCharacterCount(SpecialServiceRequest) + GetCharacterCount(NotifyPartyElementList) +
						GetCharacterCount(OtherServiceInformation) + GetCharacterCount(CustomsOrigin);
				}

				return ssrNfyOsiCorCount;
			}
		}
		int ssrNfyOsiCorCount;

		#endregion

		int GetCharacterCount(ElementList list)
		{
			return list.ToStringValueTypes().Length > 0 ? list.ToString().Length : 0;
		}

		ElementList OtherServiceInformation
		{
			get
			{
				if (otherServiceInformation == null)
				{
					otherServiceInformation = new ElementList();
					otherServiceInformation.AddLineIdentifier("OSI");

					var osiCharsAllowed = SsrNfyOsiCorLimit - (GetCharacterCount(SpecialServiceRequest) + GetCharacterCount(CustomsOrigin));
					var osiCharsUsed = 3;   // "OSI"
					var chompPtr = 0;

					var handlingInformation = FWBDetailsProvider.HandlingInformation.Trim();

					var handlingValueElement = new ValueElement(new Format(osiCharsAllowed, CharType.Text), handlingInformation);
					handlingInformation = handlingValueElement.ToString().Trim();

					for (var lineNum = 0; lineNum < 3 && chompPtr < handlingInformation.Length; lineNum++)
					{
						osiCharsUsed += 3;  // initial slant plus CRLF
						var chompLength = MaxOSICharPerLine;
						if (osiCharsAllowed - osiCharsUsed < chompLength)
						{
							chompLength = osiCharsAllowed - osiCharsUsed;
						}

						var osiLine = handlingInformation.SubstringSafe(chompPtr, chompLength).Trim();
						chompPtr += chompLength;

						if (osiLine.Length > 0)
						{
							osiCharsUsed += osiLine.Length;

							otherServiceInformation.AddSlant();
							otherServiceInformation.AddValue(new Format(osiLine.Length, CharType.Text), osiLine);
							otherServiceInformation.AddCRLF();
							if (chompLength < MaxOSICharPerLine)
							{
								break; // Don't continue processing if we just ouput an incomplete line
							}
						}
					}
				}

				return otherServiceInformation;
			}
		}

		ElementList otherServiceInformation;

		void AddSpecialServiceRequest()
		{
			Elements.AddOptionalHeader(SpecialServiceRequest);
		}

		ElementList SpecialServiceRequest
		{
			get
			{
				if (specialServiceRequest == null)
				{
					specialServiceRequest = new ElementList();
					specialServiceRequest.AddLineIdentifier("SSR");

					ElementList valueElements = new ElementList();

					var knownConsignorCode = FWBDetailsProvider.KnownConsignorCode;
					if (!knownConsignorCode.IsEmpty)
					{
						valueElements.AddSlant();
						valueElements.AddValue(new Format(7, 0, CharType.Text), knownConsignorCode);
					}

					valueElements.AddCRLF();
					specialServiceRequest.AddOptionalHeader(valueElements);
				}

				return specialServiceRequest;
			}
		}

		ElementList specialServiceRequest;

		void AddSenderReference()
		{
			ElementList senderReference = new ElementList();
			senderReference.AddLineIdentifier("REF");
			senderReference.AddSlant();

			ElementList valueElements = new ElementList();
			valueElements.AddSlant();
			valueElements.AddValue(new Format(15, 0, CharType.Text), FWBDetailsProvider.ConsolNumber);
			senderReference.AddHeader(valueElements);

			valueElements = new ElementList();

			valueElements.AddSlant();
			valueElements.AddValue(new Format(3, CharType.Alpha), "FFW");
			valueElements.AddSlant();
			valueElements.AddValue(new Format(4, CharType.Alpha), "CWID");
			valueElements.AddValue(new Format(9, CharType.AlphaNumeric), GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			valueElements.AddSlant();

			var airportOrCityCode = GlbBranch.CurrentBranch.HomePort?.RL_IATA ?? string.Empty;
			valueElements.AddValue(new Format(3, CharType.Alpha), airportOrCityCode);

			senderReference.AddHeader(valueElements);

			senderReference.AddCRLF();
			Elements.AddHeader(senderReference);
		}

		void AddCustomsOrigin()
		{
			Elements.AddOptionalHeader(CustomsOrigin);
		}

		ElementList CustomsOrigin
		{
			get
			{
				if (customsOrigin == null)
				{
					customsOrigin = new ElementList();
					customsOrigin.AddLineIdentifier("COR");
					customsOrigin.AddSlant();
					customsOrigin.AddValue(new Format(2, CharType.AlphaNumeric), FWBDetailsProvider.SpecialHandlingCode);
					customsOrigin.AddCRLF();
				}

				return customsOrigin;
			}
		}

		ElementList customsOrigin;

		void AddSpecialHandlingDetails()
		{
			if (version == Version.No16 && FWBDetailsProvider.AWBSpecialHandlingItems.Any())
			{
				var sphLine = new ElementList();
				sphLine.AddLineIdentifier("SPH");

				foreach (var specialHandlingItem in FWBDetailsProvider.AWBSpecialHandlingItems)
				{
					var sphDetail = new ElementList();
					sphDetail.AddSlant();
					sphDetail.AddValue(new Format(3, CharType.Alpha), specialHandlingItem.SpecialHandling);
					sphLine.AddHeader(sphDetail);
				}

				sphLine.AddCRLF();
				Elements.AddOptionalHeader(sphLine);
			}
		}

		void AddOtherCustomsInformation()
		{
			if (version != Version.No16)
			{
				return;
			}

			AddOCIForMRNs(FWBDetailsProvider.MovementReferenceNumbers);
			AddOCIForGDRNs(FWBDetailsProvider.GoodsDeclarationReferenceNumbers);
			AddOCIForSecurityDeclaration();
			AddOCIForTSASecurityStatement();
			AddVATAndContactNumbersToOCI(FWBDetailsProvider);

			var unnoValues = FWBDetailsProvider.DGUNNOValues();
			if (unnoValues.Any())
			{
				AddDGCodes(unnoValues);
			}

			AddOCIForAcidNumbers(FWBDetailsProvider.HandlingInformation);
			AddOCIExportStatements(FWBDetailsProvider);
			Elements.AddOptionalHeader(OCISection);
		}

		void AddOCIForSecurityDeclaration()
		{
			if (includeSecurityDeclaration)
			{
				AddOCILine(FWBDetailsProvider.AgentApprovalCountryCode, "ISS", "RA", FWBDetailsProvider.AgentApprovalNumber);
				var expiryDate = FormatExpiryDate(FWBDetailsProvider.AgentApprovalExpiryDate);
				AddOCILine("ED", expiryDate);
				AddOCIKnownShipperLines(AviationSecuritySchemeMembership.Codes.AccountConsignor);
				AddOCIKnownShipperLines(AviationSecuritySchemeMembership.Codes.KnownConsignor);
				AddOCIKnownShipperLines(AviationSecuritySchemeMembership.Codes.RegulatedAgent);
				AddOCILines("L", FWBDetailsProvider.CargoSecurityExemptionGrounds);
				AddOCILines("SM", FWBDetailsProvider.CargoSecurityScreeningMethods);
				AddOCILine("SN", FWBDetailsProvider.SecurityStatusIssuedBy);
				AddOCILine("SD", FWBDetailsProvider.SecurityStatusIssueDate.ToString("ddMMMyyHHmm", CultureInfo.InvariantCulture));
				AddOCIAdditionalSecurityInformationLines();
				AddOCILines("ST", Wrap(FWBDetailsProvider.AdditionalScreeningMethods, 35));
			}
		}

		void AddOCIForTSASecurityStatement()
		{
			if (includeSecurityDeclaration)
			{
				return;
			}

			var securityStatement = FWBDetailsProvider.TSASecurityStatement;

			if (!securityStatement.IsEmpty)
			{
				securityStatement = new ValueElement(new Format(securityStatement.Length, CharType.Text), securityStatement).ToString();
				securityStatement = Regex.Replace(securityStatement, @"\s{1,}", @" ", RegexOptions.IgnoreCase).Trim();

				if (!securityStatement.IsEmpty)
				{
					AddOCILines("ST", Wrap(securityStatement, 35));
				}
			}
		}

		void AddOCIAdditionalSecurityInformationLines()
		{
			foreach (var additionalSecurityInformation in FWBDetailsProvider.AdditionalSecurityInformations)
			{
				var finalAdditionalSecurityInformation = additionalSecurityInformation;
				if (!finalAdditionalSecurityInformation.IsEmpty)
				{
					finalAdditionalSecurityInformation = new ValueElement(new Format(finalAdditionalSecurityInformation.Length, CharType.Text), finalAdditionalSecurityInformation).ToString();
					finalAdditionalSecurityInformation = Regex.Replace(finalAdditionalSecurityInformation, @"\s{1,}", @" ", RegexOptions.IgnoreCase).Trim();

					if (!finalAdditionalSecurityInformation.IsEmpty)
					{
						AddOCILines("ST", Wrap(finalAdditionalSecurityInformation, 35));
					}
				}
			}
		}

		void AddOCIKnownShipperLines(ZString approvalType)
		{
			var approvals = FWBDetailsProvider.CargoSecurityKnownShippers.Where(x => x.ApprovalCategory == approvalType);
			foreach (var approval in approvals)
			{
				switch (approvalType)
				{
					case AviationSecuritySchemeMembership.Codes.AccountConsignor:
						AddOCILine(approvalType, approval.ApprovalNumber);
						break;
					case AviationSecuritySchemeMembership.Codes.RegulatedAgent:
						AddOCILine(approval.CountryCode, "OSS", approvalType, approval.ApprovalNumber);
						AddOCILine("ED", FormatExpiryDate(approval.ApprovalExpiryDate));
						break;
					default:
						AddOCILine(approval.CountryCode, ZString.Empty, approvalType, approval.ApprovalNumber);
						AddOCILine("ED", FormatExpiryDate(approval.ApprovalExpiryDate));
						break;
				}
			}
		}

		ZString FormatExpiryDate(ZDateTime expiryDate)
		{
			return !expiryDate.IsEmpty ? expiryDate.ToString("MMyy", CultureInfo.InvariantCulture) : "1299";
		}

		IEnumerable<ZString> Wrap(ZString text, int length)
		{
			if (!text.IsEmpty)
			{
				foreach (var line in text.ToString().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
				{
					foreach (var split in SplitLine(line, length))
					{
						yield return split;
					}
				}
			}
		}

		IEnumerable<ZString> SplitLine(ZString line, int length)
		{
			if (line.Length < length)
			{
				yield return line;
			}
			else
			{
				int startWhitespacePtr = 0;
				int endWhitespacePtr = 0;

				for (int i = length - 1; i >= 0; i--)
				{
					if (char.IsWhiteSpace(line[i]))
					{
						startWhitespacePtr = i;
						endWhitespacePtr = Math.Max(endWhitespacePtr, i);
					}
					else if (startWhitespacePtr > 0)
					{
						foreach (var split in SplitLine(line, length, startWhitespacePtr, endWhitespacePtr + 1))
						{
							yield return split;
						}

						yield break;
					}
					else if (i == 0)
					{
						foreach (var split in SplitLine(line, length, length, length))
						{
							yield return split;
						}
					}
				}
			}
		}

		IEnumerable<ZString> SplitLine(ZString line, int length, int firstWordLength, int secondWordIndex)
		{
			yield return line.Substring(0, firstWordLength);

			var remainder = line.Substring(secondWordIndex);

			foreach (var remainingSplit in SplitLine(remainder, length))
			{
				yield return remainingSplit;
			}
		}

		#endregion

		#region IATA Conversions

		#region Volume

		struct Volume
		{
			public ZDecimal Value { get; set; }
			public ZString Unit { get; set; }
		}

		const int MaxVolumeLength = 9;

		Volume ConvertVolumeToIATA(Volume volume)
		{
			switch (volume.Unit)
			{
				case Core.Constants.Volume.CubicDecimetres:
				case Core.Constants.Volume.Litre:
					var unit = "CC";
					var value = volume.Value * 1000;

					if (value.ToString(CultureInfo.InvariantCulture).Length > MaxVolumeLength)
					{
						unit = "MC";
						value = Core.Constants.Volume.Convert(volume.Value, Core.Constants.Volume.Litre, Core.Constants.Volume.CubicMetres);
					}

					return new Volume
					{
						Unit = unit,
						Value = value
					};

				case Core.Constants.Volume.CubicMetres:
					return new Volume
					{
						Unit = "MC",
						Value = volume.Value
					};

				case Core.Constants.Volume.MegaLitre:
					return new Volume
					{
						Unit = "MC",
						Value = Core.Constants.Volume.Convert(volume.Value, Core.Constants.Volume.MegaLitre, Core.Constants.Volume.CubicMetres)
					};

				case Core.Constants.Volume.CubicInches:
					return new Volume
					{
						Unit = "CI",
						Value = volume.Value
					};

				case Core.Constants.Volume.CubicFeet:
					return new Volume
					{
						Unit = "CF",
						Value = volume.Value
					};

				case Core.Constants.Volume.CubicYards:
					return new Volume
					{
						Unit = "CF",
						Value = Core.Constants.Volume.Convert(volume.Value, volume.Unit, Core.Constants.Volume.CubicFeet)
					};

				case Core.Constants.Volume.TeaChest:
					return new Volume
					{
						Unit = "CF",
						Value = Core.Constants.Volume.Convert(volume.Value, volume.Unit, Core.Constants.Volume.CubicFeet)
					};

				default:
					return new Volume
					{
						Unit = "MC",
						Value = Core.Constants.Volume.Convert(volume.Value, volume.Unit, Core.Constants.Volume.CubicMetres)
					};
			}
		}

		#endregion

		#region Dimensions

		struct Dimensions
		{
			public ZInt Length { get; set; }
			public ZInt Width { get; set; }
			public ZInt Height { get; set; }
			public ZString Unit { get; set; }
		}

		Dimensions ConvertDimensionsToIATA(Dimensions dimensions)
		{
			switch (dimensions.Unit)
			{
				case Core.Constants.Length.Millimetres:
					return new Dimensions
					{
						Unit = "MMT",
						Length = dimensions.Length,
						Width = dimensions.Width,
						Height = dimensions.Height,
					};

				case Core.Constants.Length.Centimetres:
					return new Dimensions
					{
						Unit = "CMT",
						Length = dimensions.Length,
						Width = dimensions.Width,
						Height = dimensions.Height
					};

				case Core.Constants.Length.Metres:
					return new Dimensions
					{
						Unit = "MTR",
						Length = dimensions.Length,
						Width = dimensions.Width,
						Height = dimensions.Height
					};

				case Core.Constants.Length.Kilometres:
					return new Dimensions
					{
						Unit = "MTR",
						Length = (ZInt)Core.Constants.Length.Convert(dimensions.Length, dimensions.Unit, Core.Constants.Length.Metres),
						Width = (ZInt)Core.Constants.Length.Convert(dimensions.Width, dimensions.Unit, Core.Constants.Length.Metres),
						Height = (ZInt)Core.Constants.Length.Convert(dimensions.Height, dimensions.Unit, Core.Constants.Length.Metres)
					};

				case Core.Constants.Length.Inches:
					return new Dimensions
					{
						Unit = "INH",
						Length = dimensions.Length,
						Width = dimensions.Width,
						Height = dimensions.Height
					};

				case Core.Constants.Length.Feet:
					return new Dimensions
					{
						Unit = "FOT",
						Length = dimensions.Length,
						Width = dimensions.Width,
						Height = dimensions.Height
					};

				case Core.Constants.Length.Yards:
					return new Dimensions
					{
						Unit = "YRD",
						Length = dimensions.Length,
						Width = dimensions.Width,
						Height = dimensions.Height
					};

				case Core.Constants.Length.Miles:
					return new Dimensions
					{
						Unit = "YRD",
						Length = (ZInt)Core.Constants.Length.Convert(dimensions.Length, dimensions.Unit, Core.Constants.Length.Yards),
						Width = (ZInt)Core.Constants.Length.Convert(dimensions.Width, dimensions.Unit, Core.Constants.Length.Yards),
						Height = (ZInt)Core.Constants.Length.Convert(dimensions.Height, dimensions.Unit, Core.Constants.Length.Yards)
					};

				default:
					return new Dimensions
					{
						Unit = "MTR",
						Length = (ZInt)Core.Constants.Length.Convert(dimensions.Length, dimensions.Unit, Core.Constants.Length.Metres),
						Width = (ZInt)Core.Constants.Length.Convert(dimensions.Width, dimensions.Unit, Core.Constants.Length.Metres),
						Height = (ZInt)Core.Constants.Length.Convert(dimensions.Height, dimensions.Unit, Core.Constants.Length.Metres)
					};
			}
		}

		#endregion

		#endregion
	}
}
