using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public class COODEC : BaseTradeNetMessage<CertificateOfOrigin>
	{
		public COODEC(ITCODEC cusDec)
			: base(cusDec)
		{
			CusDec = cusDec;
		}

		public new ITCODEC CusDec { get; }

		protected override void BuildTradenetDeclaration(TradenetDeclaration messageParent)
		{
			var inboundMessage = messageParent.InboundMessage = new TradenetDeclarationInboundMessage();
			inboundMessage.CertificateOfOrigin = BuildDeclaration();
		}

		protected override Header BuildHeaderCore()
		{
			var header = new Header();
			BuildCommonHeaderSection(header);
			header.ApplicationType = ApplicationTypeCodeList.Codes.COO;
			SetValueIfNotEmpty(CusDec.PreviousPermitNumber, (s) => header.PreviousPermitNumber = s);
			BuildCertificateAdditionalInfo(header);

			return header;
		}

		#region CertificateAdditionalInfo

		protected void BuildCertificateAdditionalInfo(Header header)
		{
			var list = BuildCertificateAdditionalInfoCore(CusDec.AdditionalInformation);
			header.CertificateAdditionalInformation = list.Any() ? list.ToArray() : null;
		}

		protected virtual List<CertificateAdditionalInformation> BuildCertificateAdditionalInfoCore(ZString certificateAdditionalInformation)
		{
			var certificateAdditionalInfo = new List<CertificateAdditionalInformation>();
			var infos = certificateAdditionalInformation.SplitIntoArray(35, 5);
			if (infos.Any())
			{
				certificateAdditionalInfo.Add(new CertificateAdditionalInformation() { Line = infos });
			}

			return certificateAdditionalInfo;
		}

		#endregion

		protected override InwardTransport BuildInwardTransportCore()
		{
			return null;
		}

		protected override OutwardTransport BuildOutwardTransportCore()
		{
			var outwardTransport = new OutwardTransport();
			var departureDate = CusDec.DepartureDate;
			if (!departureDate.IsEmpty)
			{
				outwardTransport.DepartureDate = departureDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
			}

			outwardTransport.DischargePort = CusDec.PortOfDischarge;
			BuildOutwardTransportTransportMeans(outwardTransport);
			outwardTransport.FinalDestinationCountry = CusDec.CountryOfFinalDestination;
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
			}

			return transportMeans;
		}

		protected override InwardCarrierAgentParty BuildInwardCarrierAgentPartyCore(IOrganisation inwardCarrierAgent)
		{
			return null;
		}

		protected override ExporterParty BuildExporterPartyCore(IOrganisation exporter)
		{
			var exporterParty = base.BuildExporterPartyCore(exporter);
			var addressLine = exporter.Address.FullAddress;
			if (addressLine.Length > 105)
			{
				addressLine = exporter.Address.FullAddressWithoutAdditionalInfo;
			}

			var exporterAddress = addressLine.SplitIntoArray(35, 3);
			exporterParty.AddressLine = exporterAddress.Any() ? exporterAddress.ToArray() : null;
			return exporterParty;
		}

		protected override ConsigneeParty BuildConsigneePartyCore(IOrganisation consignee)
		{
			var consigneeParty = base.BuildConsigneePartyCore(consignee);
			var addressLine = consignee.Address.FullAddress;
			if (addressLine.Length > 105)
			{
				addressLine = consignee.Address.FullAddressWithoutAdditionalInfo;
			}

			var consigneeAddress = addressLine.SplitIntoArray(35, 3);
			consigneeParty.AddressLine = consigneeAddress.Any() ? consigneeAddress.ToArray() : null;
			return consigneeParty;
		}

		protected override ConsigneePartyAddress BuildConsigneePartyAddressCore(IAddress address)
		{
			return null;
		}

		protected override ClaimantParty BuildClaimantPartyCore(IOrganisation claimant)
		{
			return null;
		}

		protected override Certificate BuildCertificateCore()
		{
			var certificate = new Certificate();
			certificate.ApplicationProductType = CusDec.ApplicationProductType;
			BuildEntryYearElement(certificate);
			BuildDonorCountryElement(certificate);
			certificate.CertificateDetail = BuildCertificateDetailCore();

			var percCommContent1 = CusDec.PercCommContent1;
			if (percCommContent1 != 0)
			{
				certificate.PreferenceContentPercent = percCommContent1;
				certificate.PreferenceContentPercentSpecified = true;
			}

			if (!CusDec.CurrencyCode.IsEmpty)
			{
				certificate.CurrencyCode = CusDec.CurrencyCode;
			}

			if (!CusDec.AdditionalDetails1.IsEmpty)
			{
				certificate.AdditionalCertificateDetails = CusDec.AdditionalDetails1.SplitIntoArray(35, 5);
			}

			if (!CusDec.TransportDetails1.IsEmpty)
			{
				certificate.TransportDetails = CusDec.TransportDetails1.SplitIntoArray(35, 5);
			}

			return certificate;
		}

		void BuildEntryYearElement(Certificate certificate)
		{
			if (!CusDec.YearOfEntry.IsEmpty)
			{
				certificate.EntryYear = CusDec.YearOfEntry;
				certificate.EntryYearSpecified = true;
			}
		}

		void BuildDonorCountryElement(Certificate certificate)
		{
			if (!CusDec.DonorCountryCode.IsEmpty)
			{
				certificate.GSPDonorCountry = CusDec.DonorCountryCode;
			}
		}

		protected override CertificateDetail[] BuildCertificateDetailCore()
		{
			var certificateDetails = new List<CertificateDetail>();
			var certificate1 = new CertificateDetail();
			certificate1.SequenceNumeric = "1";
			certificate1.CertificateType = CusDec.CertificateType1;
			if (CusDec.NumberOfCopies1 > 0)
			{
				certificate1.CopiesNumeric = CusDec.NumberOfCopies1;
				certificate1.CopiesNumericSpecified = true;
			}

			certificateDetails.Add(certificate1);

			if (!CusDec.CertificateType2.IsEmpty)
			{
				var certificate2 = new CertificateDetail();
				certificate2.SequenceNumeric = "2";
				certificate2.CertificateType = CusDec.CertificateType2;
				if (CusDec.NumberOfCopies2 > 0)
				{
					certificate2.CopiesNumeric = CusDec.NumberOfCopies2;
					certificate2.CopiesNumericSpecified = true;
				}

				certificateDetails.Add(certificate2);
			}

			return certificateDetails.ToArray();
		}

		public override void BuildItem(ITradeNetInSection message)
		{
			var items = new List<Item>();
			var index = 0;
			foreach (var cusItem in CusItems)
			{
				index++;
				var item = BuildItemCore(index, cusItem);
				BuildShippingMarksInformation(item, cusItem);
				BuildItemCertificate(item, cusItem);
				items.Add(item);
			}

			message.Item = items.ToArray();
		}

		protected override Item BuildItemCore(int index, ICusItem cusItem)
		{
			var item = new Item();
			item.ItemSequenceNumeric = index;
			item.ItemSequenceNumericSpecified = true;
			item.ItemHarmonizedSystemCode = cusItem.HSCode;
			item.HarmonizedSystemQuantity = new HarmonizedSystemQuantity();
			var hsQuantity = ZDecimal.Parse(Utilities.FormatNumberNational(cusItem.HSQuantity, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues));
			item.HarmonizedSystemQuantity.Value = hsQuantity;
			item.HarmonizedSystemQuantity.unitCode = cusItem.HSQuantityUnitType;
			item.OriginCountry = cusItem.CountryOfOriginCode;
			if (ZDecimal.TryParse(Utilities.FormatNumberNational(cusItem.CustomsValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var customsValue) && customsValue > 0)
			{
				item.ItemCIFFOBValue = customsValue;
				item.ItemCIFFOBValueSpecified = true;
			}

			return item;
		}

		protected override ShippingMarksInformation[] BuildShippingMarksInformationCore(ICusItem cusItem)
		{
			var shippingMarksLines = new List<ShippingMarksInformation>();
			var marks = cusItem.MarksAndNumbers;
			if (!marks.IsEmpty)
			{
				var marksAndNumbers = marks.SplitIntoArray(17, 10);
				if (marksAndNumbers.Any())
				{
					shippingMarksLines.Add(new ShippingMarksInformation { ShippingMarks = marksAndNumbers });
				}
			}

			return shippingMarksLines.ToArray();
		}

		protected override ItemCertificate BuildItemCertificateCore(ICusItem cusItem)
		{
			var certItem = (ICusCertItem)cusItem;
			var itemCertificate = new ItemCertificate();
			if (certItem.ItemQuantity > 0)
			{
				var quantity = ZDecimal.Parse(Utilities.FormatNumberNational(certItem.ItemQuantity, SGConstants.NumericFormatting.DecimalPlacesForMeasurementValues));
				itemCertificate.ItemCertificateQuantity = new ItemCertificateQuantity
				{
					Value = quantity,
					unitCode = certItem.ItemQuantityUnitType
				};
			}

			if (ZDecimal.TryParse(Utilities.FormatNumberNational(certItem.ItemValue, SGConstants.NumericFormatting.DecimalPlacesForAmountValues), out var itemValue) && itemValue > 0)
			{
				itemCertificate.ItemValue = itemValue;
				itemCertificate.ItemValueSpecified = true;
			}

			if (!certItem.ItemDescription.IsEmpty)
			{
				BuildCertificateItemDescription(itemCertificate, certItem);
			}

			if (!certItem.DateOfManufacturingCost.IsEmpty)
			{
				itemCertificate.ManufacturingCostDate = certItem.DateOfManufacturingCost.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
			}

			if (!certItem.TextileCategoryCode.IsEmpty)
			{
				itemCertificate.TextileCategoryCode = certItem.TextileCategoryCode;
				itemCertificate.TextileQuotaQuantity = new TextileQuotaQuantity();
				itemCertificate.TextileQuotaQuantity.Value = certItem.TextileQuotaQty;
				itemCertificate.TextileQuotaQuantity.unitCode = certItem.TextileQuotaUnitCode;
			}

			if (CusDec.SendInvoiceDetails)
			{
				if (!certItem.InvoiceNumber.IsEmpty)
				{
					itemCertificate.ItemInvoiceNumber = certItem.InvoiceNumber;
				}

				if (!certItem.InvoiceDate.IsEmpty)
				{
					itemCertificate.ItemInvoiceDate = certItem.InvoiceDate.ToString(DateTimeFormat, CultureInfo.InvariantCulture);
				}
			}

			BuildOriginCriterion(itemCertificate, certItem, CusDec.CertificateType1);
			SetValueIfNotEmpty(certItem.CertHSCode, (s) => itemCertificate.HarmonizedSystemCode = s);

			if (certItem.PercentageContent > 0)
			{
				itemCertificate.ContentPercent = certItem.PercentageContent;
				itemCertificate.ContentPercentSpecified = true;
			}

			return itemCertificate;
		}

		public override void BuildSummary(ITradeNetInSection message)
		{
			message.Summary = new Summary
			{
				NumberOfItems = CusDec.CertItems.Count(),
				NumberOfItemsSpecified = true
			};
		}

		#region ICusMessage

		public override string MessageType => CommonAccessReferenceCodeList.Codes.COODEC;

		public override string MessageSubType => CUSDECEDIMessage.Declaration;

		#endregion
	}
}
