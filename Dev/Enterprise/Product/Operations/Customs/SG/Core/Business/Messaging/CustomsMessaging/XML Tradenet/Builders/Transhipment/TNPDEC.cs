using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class TNPDEC : BaseTradeNetMessage<TranshipmentMovement>
	{
		public TNPDEC(ITNPDEC cusDec)
			: base(cusDec)
		{
			CusDec = cusDec;
		}

		public new ITNPDEC CusDec { get; }

		protected override void BuildTradenetDeclaration(TradenetDeclaration messageParent)
		{
			var inboundMessage = messageParent.InboundMessage = new TradenetDeclarationInboundMessage();
			inboundMessage.TranshipmentMovement = BuildDeclaration();
		}

		protected override Cargo BuildCargoCore()
		{
			var cargo = base.BuildCargoCore();
			BuildRemovalStartDate(cargo);

			if (CusDec.Is2bStoredBWCY || CusDec.IsStorageInFTZ)
			{
				BuildStorageLocation(cargo);
			}

			return cargo;
		}

		protected override Header BuildHeaderCore()
		{
			var header = base.BuildHeaderCore();
			return header;
		}

		protected override InwardTransportTransportMeans BuildInwardTransportTransportMeansCore()
		{
			InwardTransportTransportMeans transportMeans = null;
			var modeCode = CusDec.InwardTransportCode;
			if (modeCode > 0)
			{
				transportMeans = new InwardTransportTransportMeans();
				var mode = transportMeans.TransportMode = new InwardTransportTransportMeansTransportMode();
				mode.ModeCode = modeCode;
				mode.ModeCodeSpecified = true;
				SetValueIfNotEmpty(CusDec.InwardJourneyIdentifier, (s) => mode.ConveyanceReferenceNumber = s);
				SetValueIfNotEmpty(CusDec.InwardTransportIdentifier, (s) => mode.TransportIdentifier = s);
				if (!(CusDec.DeclarationType == DeclarationTypeCodeList.Codes.TTI || CusDec.DeclarationType == DeclarationTypeCodeList.Codes.TTF))
				{
					if (CusDec.HasInwardTransport)
					{
						SetValueIfNotEmpty(CusDec.InwardMasterBill, (s) => transportMeans.MAWBOUCROBLNumber = s);
					}
				}
			}

			return transportMeans;
		}

		protected override OutwardTransport BuildOutwardTransportCore()
		{
			var outwardTransport = new OutwardTransport();
			var departureDate = CusDec.DepartureDate;

			BuildOutwardTransportTransportMeans(outwardTransport);
			BuildOutwardTransportAdditionalVesselInformation(outwardTransport);
			if (!departureDate.IsEmpty)
			{
				outwardTransport.DepartureDate = departureDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
			}

			if (!CusDec.IsSeaStoreDeclaration)
			{
				outwardTransport.DischargePort = CusDec.PortOfDischarge;
				if (CusDec.HasOutwardTransport)
				{
					SetValueIfNotEmpty(CusDec.CountryOfFinalDestination, (c) => outwardTransport.FinalDestinationCountry = c);
				}
			}

			return outwardTransport;
		}

		protected override OutwardTransportTransportMeans BuildOutwardTransportTransportMeansCore()
		{
			OutwardTransportTransportMeans transportMeans = null;
			var modeCode = CusDec.OutwardTransportCode;
			if (modeCode > 0)
			{
				transportMeans = new OutwardTransportTransportMeans();
				var mode = transportMeans.TransportMode = new OutwardTransportTransportMeansTransportMode();
				mode.ModeCode = modeCode;
				mode.ModeCodeSpecified = true;
				SetValueIfNotEmpty(CusDec.OutwardJourneyIdentifier, (s) => mode.ConveyanceReferenceNumber = s);
				SetValueIfNotEmpty(CusDec.OutwardTransportIdentifier, (s) => mode.TransportIdentifier = s);
				if (!(CusDec.DeclarationType == DeclarationTypeCodeList.Codes.TTI || CusDec.DeclarationType == DeclarationTypeCodeList.Codes.TTF))
				{
					if (CusDec.HasOutwardTransport)
					{
						SetValueIfNotEmpty(CusDec.OutwardMasterBill, (s) => transportMeans.MAWBOUCROBLNumber = s);
					}
				}
			}

			return transportMeans;
		}

		protected override OutwardTransportAdditionalVesselInformation BuildOutwardTransportAdditionalVesselInformationCore()
		{
			var additionalVesselInformation = base.BuildOutwardTransportAdditionalVesselInformationCore();
			var hasData = additionalVesselInformation != null;
			if (CusDec.HasOutwardTransport && CusDec.IsSeaStoreDeclaration)
			{
				additionalVesselInformation = additionalVesselInformation ?? new OutwardTransportAdditionalVesselInformation();
				hasData |= BuildLoadingNextPortCore(additionalVesselInformation);
				if (CusDec.HasLiquorOrTobacco)
				{
					hasData |= BuildLoadingFinalPortCore(additionalVesselInformation);
				}
			}

			return hasData ? additionalVesselInformation : null;
		}

		public override TransportEquipmentTransportEquipmentSeal BuildTransportEquipmentSeal(ICusContainer container)
		{
			var equipmentSeal = new TransportEquipmentTransportEquipmentSeal();
			equipmentSeal.SealID = container.SealNumber.IsEmpty ? (ZString)EmptyValue : container.SealNumber;

			return equipmentSeal;
		}

		protected void BuildRemovalStartDate(Cargo cargo)
		{
			if (CusDec.DeclarationType == DeclarationTypeCodeList.Codes.REM || CusDec.DeclarationType == DeclarationTypeCodeList.Codes.BRE)
			{
				if (!CusDec.StartDateOfCargoRemoval.IsEmpty)
				{
					cargo.RemovalStartDate = CusDec.StartDateOfCargoRemoval.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
				}
			}
		}

		protected override ExporterParty BuildExporterPartyCore(IOrganisation exporter)
		{
			return null;
		}

		protected override bool SupportsEndUserParty => true;

		protected override bool SupportsHandlingAgentParty => true;

		public override void BuildItem(ITradeNetInSection message)
		{
			var items = new List<Item>();
			var index = 0;
			var cusItems = GetLineItemCollection();
			foreach (var cusItem in cusItems)
			{
				index++;
				var item = BuildItemCore(index, cusItem);
				BuildItemQuantity(item, cusItem);
				BuildTransactionValue(item, cusItem);
				BuildCASCProduct(item, cusItem);
				BuildPackingDescription(item, cusItem);
				BuildShippingMarksInformation(item, cusItem);
				BuildLotIdentification(item, cusItem);
				if (CusDec.InwardTransportCode == SGConstants.TransportCodes.Sea || CusDec.InwardTransportCode == SGConstants.TransportCodes.Air)
				{
					if (CusDec.DeclarationType == DeclarationTypeCodeList.Codes.TTI || CusDec.DeclarationType == DeclarationTypeCodeList.Codes.TTF)
					{
						SetValueIfNotEmpty(cusItem.InwardMAWB, (s) => item.InMAWBOUCROBLNumber = s);
					}

					SetValueIfNotEmpty(cusItem.InwardHAWB, (s) => item.InHAWBHUCRHBLNumber = s);
				}

				if (CusDec.OutwardTransportCode == SGConstants.TransportCodes.Sea || CusDec.OutwardTransportCode == SGConstants.TransportCodes.Air)
				{
					if (CusDec.DeclarationType == DeclarationTypeCodeList.Codes.TTI || CusDec.DeclarationType == DeclarationTypeCodeList.Codes.TTF)
					{
						SetValueIfNotEmpty(cusItem.OutwardMAWB, (s) => item.OutMAWBOUCROBLNumber = s);
					}

					SetValueIfNotEmpty(cusItem.OutwardHAWB, (s) => item.OutHAWBHUCRHBLNumber = s);
				}

				if (cusItem.IsMotorVehicle)
				{
					BuildMotorVehicleCore(item, cusItem);
				}

				items.Add(item);
			}

			message.Item = items.ToArray();
		}

		protected override void BuildTransactionValue(Item item, ICusItem cusItem)
		{
			if (CusDec.DeclarationType == DeclarationTypeCodeList.Codes.IGM ||
				CusDec.DeclarationType == DeclarationTypeCodeList.Codes.REM ||
				CusDec.DeclarationType == DeclarationTypeCodeList.Codes.BRE)
			{
				item.TransactionValue = BuildTransactionValueCore(cusItem);
			}
		}

		protected override TransactionValue BuildTransactionValueCore(ICusItem cusItem)
		{
			var hasData = false;
			var transactionValue = new TransactionValue();

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(cusItem.CustomsValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var customsValue) && customsValue > 0)
			{
				transactionValue.ItemCIFFOBValue = customsValue;
				transactionValue.ItemCIFFOBValueSpecified = true;

				hasData = true;
			}

			return hasData ? transactionValue : null;
		}

		protected override CASCProduct BuildStrategicGoodsData(ICusItem cusItem)
		{
			var strategicGoodsProduct = new CASCProduct();
			strategicGoodsProduct.CASCProductCode = cusItem.CategoryCode;

			var additionalCASCIdentifications = new List<CASCProductAdditionalCASCIdentification>();
			var strategicGoodsPermitDetails = new CASCProductAdditionalCASCIdentification();
			strategicGoodsPermitDetails.CASCCodeOne = cusItem.EndUseCode1;
			strategicGoodsPermitDetails.CASCCodeTwo = cusItem.EndUseCode2;
			strategicGoodsPermitDetails.CASCCodeThree = cusItem.EndUseCode3;
			additionalCASCIdentifications.Add(strategicGoodsPermitDetails);
			strategicGoodsProduct.AdditionalCASCIdentification = additionalCASCIdentifications.ToArray();

			var endUse = new CASCProductEndUseDescription();
			endUse.EndUseLine = cusItem.EndUseDescription.ToUpperInvariant();
			strategicGoodsProduct.EndUseDescription = endUse;

			return strategicGoodsProduct;
		}

		protected override Summary BuildSummaryCore()
		{
			var summary = new Summary
			{
				NumberOfItems = CusDec.Items.Count(),
				NumberOfItemsSpecified = true
			};

			if (CusDec.DeclarationType == DeclarationTypeCodeList.Codes.REM || CusDec.DeclarationType == DeclarationTypeCodeList.Codes.BRE || CusDec.DeclarationType == DeclarationTypeCodeList.Codes.IGM)
			{
				if (ZDecimal.TryParse(Utilities.FormatNumberNational(CusDec.TotalCustomsValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var totalCustomsValue) && totalCustomsValue > 0)
				{
					summary.TotalCIFFOBValue = totalCustomsValue;
					summary.TotalCIFFOBValueSpecified = true;
				}
			}

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(CusDec.TotalOuterPack, SGConstants.NumericFormatting.DecimalPlacesNone), out var totalOuterPack))
			{
				summary.TotalOuterPack = new TotalOuterPack
				{
					Value = totalOuterPack,
					unitCode = CusDec.TotalOuterPackUnitOfQty
				};
			}

			return summary;
		}

		#region ICusMessage

		public override string MessageType => CommonAccessReferenceCodeList.Codes.TNPDEC;

		public override string MessageSubType => CUSDECEDIMessage.Declaration;

		#endregion
	}
}
