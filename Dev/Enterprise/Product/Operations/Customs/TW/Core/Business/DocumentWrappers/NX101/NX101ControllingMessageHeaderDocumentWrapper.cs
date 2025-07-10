using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using static Enterprise.Customs.TW.Business.MessageConstants;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	public class NX101ControllingMessageHeaderDocumentWrapper : DocumentEngineCore.DocWrappers.DocumentWrapper
	{
		public NX101ControllingMessageHeaderDocumentWrapper(CusTWControllingMessageHeader header)
			: base(header, header.Factory)
		{
			this.header = Argument.NotNull(header, nameof(header));
			certificateType = header.TW1_CertificateType;
			isCertificateType07 = certificateType == CertificateTypeList.Codes.Code7;
			var manufacturerPrintingCode = Application.ManufacturerPrintingCode;
			var isCertificateType11_PrintingCode1Or2 = certificateType == CertificateTypeList.Codes.Code11 && (manufacturerPrintingCode == CPT_123_ManufacturerPrintingCodeList.Codes._1 || manufacturerPrintingCode == CPT_123_ManufacturerPrintingCodeList.Codes._2);
			manufacturerTelFaxOutputEmpty = isCertificateType07 || isCertificateType11_PrintingCode1Or2;
			manufacturerEmailIDOutputEmpty = isCertificateType11_PrintingCode1Or2;
			SetMarksNumbers();
		}

		readonly CusTWControllingMessageHeader header;
		readonly ZString certificateType;
		readonly bool isCertificateType07;
		readonly bool manufacturerTelFaxOutputEmpty;
		readonly bool manufacturerEmailIDOutputEmpty;

		ZString GetPropertyValueFromParty(IPartyDetails party, Func<IPartyDetails, ZString> getter)
		{
			return party != null ? getter.Invoke(party) : ZString.Empty;
		}

		NX101MessageSendingObject MessageSendingObject => messageSendingObject ??= new NX101MessageSendingObject(header);
		NX101MessageSendingObject messageSendingObject;

		IGoodsShipment GoodsShipment => goodsShipment ??= MessageSendingObject.GoodsShipment;
		IGoodsShipment goodsShipment;

		public ZString CertificateType => certificateType;

		public ZString ExporterInfos
		{
			get
			{
				var exporterInfos = new ZStringBuilder();
				switch (certificateType)
				{
					case CertificateTypeList.Codes.Code9:
					case CertificateTypeList.Codes.Code11:
					case CertificateTypeList.Codes.Code15:
					case CertificateTypeList.Codes.Code18:
					case CertificateTypeList.Codes.Code19:
						exporterInfos.AppendIfNotEmpty(ExporterName);
						exporterInfos.AppendIfNotEmpty(ExporterChineseName);
						exporterInfos.AppendIfNotEmpty(ExporterAddressLine);
						exporterInfos.AppendIfNotEmpty(ExporterAddressChineseLineLine);
						break;
					default:
						exporterInfos.AppendIfNotEmpty(ExporterID);
						exporterInfos.AppendIfNotEmpty(ExporterName);
						exporterInfos.AppendIfNotEmpty(ExporterChineseName);
						exporterInfos.AppendIfNotEmpty(ExporterAddressLine);
						exporterInfos.AppendIfNotEmpty(ExporterAddressChineseLineLine);
						if (!ExporterTEL.IsEmpty)
						{
							exporterInfos.Append($"{Constants.DocumentWrapper.Tel}{ExporterTEL}");
						}
						if (!ExporterFAX.IsEmpty)
						{
							exporterInfos.Append($"{Constants.DocumentWrapper.Fax}{ExporterFAX}");
						}
						if (!ExporterEMAIL.IsEmpty)
						{
							exporterInfos.Append($"{Constants.DocumentWrapper.Email}{ExporterEMAIL}");
						}
						break;
				}
				return exporterInfos.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString ExporterNameAndAddress
		{
			get
			{
				var exporterNameAndAddress = new ZStringBuilder();
				exporterNameAndAddress.AppendIfNotEmpty(ExporterName);
				exporterNameAndAddress.AppendIfNotEmpty(ExporterAddressLine);
				exporterNameAndAddress.AppendIfNotEmpty(ExporterChineseName);
				exporterNameAndAddress.AppendIfNotEmpty(ExporterAddressChineseLineLine);
				return exporterNameAndAddress.ToStringWithNewLineBetweenAppends();
			}
		}

		IPartyDetails GoodsShipmentExporter => goodsShipmentExporter ??= GoodsShipment?.Exporter;
		IPartyDetails goodsShipmentExporter;

		public ZString ExporterTypeCode => GetPropertyValueFromParty(GoodsShipmentExporter, x => x.TypeCode);

		public ZString ExporterID => GetPropertyValueFromParty(GoodsShipmentExporter, x => x.ID);

		public ZString ExporterName => GetPropertyValueFromParty(GoodsShipmentExporter, x => x.Name);

		public ZString ExporterChineseName => GetPropertyValueFromParty(GoodsShipmentExporter, x => x.ChineseName);

		Messaging.IAddress GoodsShipmentExporterAddress => goodsShipmentExporterAddress ??= GoodsShipmentExporter?.Address;
		Messaging.IAddress goodsShipmentExporterAddress;

		public ZString ExporterAddressLine => GoodsShipmentExporterAddress?.Line ?? ZString.Empty;

		public ZString ExporterAddressChineseLineLine => GoodsShipmentExporterAddress?.ChineseLine ?? ZString.Empty;

		IEnumerable<ICommunication> GoodsShipmentExporterCommunications => goodsShipmentExporterCommunications ??= GoodsShipmentExporter?.Communications;
		IEnumerable<ICommunication> goodsShipmentExporterCommunications;

		ZString GetCommunicationTypeValue(IEnumerable<ICommunication> communications, string type)
		{
			return communications?.FirstOrDefault(c => c.TypeID == type)?.ID ?? ZString.Empty;
		}

		public ZString ExporterTEL => GetCommunicationTypeValue(GoodsShipmentExporterCommunications, CommunicationTypeIDs.TE);

		public ZString ExporterFAX => GetCommunicationTypeValue(GoodsShipmentExporterCommunications, CommunicationTypeIDs.FX);

		public ZString ExporterEMAIL => GetCommunicationTypeValue(GoodsShipmentExporterCommunications, CommunicationTypeIDs.MA);

		IPartyDetails COImporter => cOImporter ??= MessageSendingObject.COImporter;
		IPartyDetails cOImporter;

		public ZString ImporterInfos
		{
			get
			{
				var importerInfos = new ZStringBuilder();
				switch (certificateType)
				{
					case CertificateTypeList.Codes.Code9:
						importerInfos.AppendIfNotEmpty(ImporterName);
						importerInfos.AppendIfNotEmpty(ImporterAddressLine);
						break;
					case CertificateTypeList.Codes.Code11:
						importerInfos.AppendIfNotEmpty(ImporterName);
						importerInfos.AppendIfNotEmpty(ImporterAddressLine);
						break;
					case CertificateTypeList.Codes.Code15:
					case CertificateTypeList.Codes.Code18:
					case CertificateTypeList.Codes.Code19:
						importerInfos.AppendIfNotEmpty(ImporterName);
						importerInfos.AppendIfNotEmpty(ImporterChineseName);
						importerInfos.AppendIfNotEmpty(ImporterAddressLine);
						importerInfos.AppendIfNotEmpty(ImporterAddressChineseLineLine);
						break;
					default:
						importerInfos.AppendIfNotEmpty(ImporterAddressChineseLineLine);
						importerInfos.AppendIfNotEmpty(ImporterChineseName);
						importerInfos.AppendIfNotEmpty(ImporterName);
						importerInfos.AppendIfNotEmpty(ImporterAddressLine);
						if (!ImporterTEL.IsEmpty)
						{
							importerInfos.Append($"{Constants.DocumentWrapper.Tel}{ImporterTEL}");
						}
						if (!ImporterFAX.IsEmpty)
						{
							importerInfos.Append($"{Constants.DocumentWrapper.Fax}{ImporterFAX}");
						}
						if (!ImporterEMAIL.IsEmpty)
						{
							importerInfos.Append($"{Constants.DocumentWrapper.Email}{ImporterEMAIL}");
						}
						break;
				}
				return importerInfos.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString ImporterTypeCode => GetPropertyValueFromParty(COImporter, x => x.TypeCode);

		public ZString ImporterID => GetPropertyValueFromParty(COImporter, x => x.ID);

		public ZString ImporterName => GetPropertyValueFromParty(COImporter, x => x.Name);

		public ZString ImporterChineseName => GetPropertyValueFromParty(COImporter, x => x.ChineseName);

		Messaging.IAddress COImporterAddress => cOImporterAddress ??= COImporter?.Address;
		Messaging.IAddress cOImporterAddress;

		public ZString ImporterAddressLine => COImporterAddress?.Line ?? ZString.Empty;

		public ZString ImporterAddressChineseLineLine => COImporterAddress?.ChineseLine ?? ZString.Empty;

		IEnumerable<ICommunication> COImporterCommunications => cOImporterCommunications ??= COImporter?.Communications;
		IEnumerable<ICommunication> cOImporterCommunications;

		public ZString ImporterTEL => GetCommunicationTypeValue(COImporterCommunications, CommunicationTypeIDs.TE);

		public ZString ImporterFAX => GetCommunicationTypeValue(COImporterCommunications, CommunicationTypeIDs.FX);

		public ZString ImporterEMAIL => GetCommunicationTypeValue(COImporterCommunications, CommunicationTypeIDs.MA);

		INX101Consignment Consignment => consignment ??= MessageSendingObject.Consignment;
		INX101Consignment consignment;

		INX101GovernmentAgencyGoodsItem ConsignmentGovernmentAgencyGoodsItem => Consignment?.GovernmentAgencyGoodsItem;

		IPartyDetails MainManufacturer => mainManufacturer ??= ConsignmentGovernmentAgencyGoodsItem?.Manufacturers?.FirstOrDefault(c => c.MainManufacturer == YesNoList.Codes.Yes);
		IPartyDetails mainManufacturer;

		public ZString ManufacturerTypeCode => GetPropertyValueFromParty(MainManufacturer, x => x.TypeCode);

		public ZString ManufacturerID => manufacturerEmailIDOutputEmpty ? ZString.Empty : GetPropertyValueFromParty(MainManufacturer, x => x.ID);

		ZString ManufacturerName => GetPropertyValueFromParty(MainManufacturer, x => x.Name);

		ZString ManufacturerChineseName => GetPropertyValueFromParty(MainManufacturer, x => x.ChineseName);

		Messaging.IAddress MainManufacturerAddress => mainManufacturerAddress ??= MainManufacturer?.Address;
		Messaging.IAddress mainManufacturerAddress;

		ZString ManufacturerAddressLine => MainManufacturerAddress?.Line ?? ZString.Empty;

		ZString ManufacturerAddressChineseLineLine => MainManufacturerAddress?.ChineseLine ?? ZString.Empty;

		public ZString ManufacturerInfos
		{
			get
			{
				var manufacturerInfos = new ZStringBuilder();
				switch (certificateType)
				{
					case CertificateTypeList.Codes.Code7:
						manufacturerInfos.Append((NoResString)"Available upon request of competent authority (主管機關要求時提供)");
						break;
					case CertificateTypeList.Codes.Code11:
						switch (Application.ManufacturerPrintingCode)
						{
							case CPT_123_ManufacturerPrintingCodeList.Codes._1:
								manufacturerInfos.Append((NoResString)"Available upon request of competent authority (主管機關要求時提供)");
								break;
							case CPT_123_ManufacturerPrintingCodeList.Codes._2:
								manufacturerInfos.Append((NoResString)"Same (相同)");
								break;
							default:
								manufacturerInfos.AppendIfNotEmpty(ManufacturerChineseName);
								manufacturerInfos.AppendIfNotEmpty(ManufacturerName);
								manufacturerInfos.AppendIfNotEmpty(ManufacturerAddressLine);
								manufacturerInfos.AppendIfNotEmpty(ManufacturerAddressChineseLineLine);
								break;
						}
						break;
					case CertificateTypeList.Codes.Code16:
						manufacturerInfos.AppendIfNotEmpty(ManufacturerChineseName);
						manufacturerInfos.AppendIfNotEmpty(ManufacturerAddressChineseLineLine);
						if (!ManufacturerTEL.IsEmpty)
						{
							manufacturerInfos.Append($"{Constants.DocumentWrapper.Tel}{ManufacturerTEL}");
						}
						if (!ManufacturerFAX.IsEmpty)
						{
							manufacturerInfos.Append($"{Constants.DocumentWrapper.Fax}{ManufacturerFAX}");
						}
						if (!ManufacturerEMAIL.IsEmpty)
						{
							manufacturerInfos.Append($"{Constants.DocumentWrapper.Email}{ManufacturerEMAIL}");
						}
						break;
					default:
						manufacturerInfos.AppendIfNotEmpty(ManufacturerName);
						manufacturerInfos.AppendIfNotEmpty(ManufacturerChineseName);
						manufacturerInfos.AppendIfNotEmpty(ManufacturerAddressLine);
						manufacturerInfos.AppendIfNotEmpty(ManufacturerAddressChineseLineLine);
						break;
				}
				return manufacturerInfos.ToStringWithNewLineBetweenAppends();
			}
		}

		IEnumerable<ICommunication> MainManufacturerCommunications => mainManufacturerCommunications ??= MainManufacturer?.Communications;
		IEnumerable<ICommunication> mainManufacturerCommunications;

		public ZString ManufacturerTEL => manufacturerTelFaxOutputEmpty ? ZString.Empty : GetCommunicationTypeValue(MainManufacturerCommunications, CommunicationTypeIDs.TE);

		public ZString ManufacturerFAX => manufacturerTelFaxOutputEmpty ? ZString.Empty : GetCommunicationTypeValue(MainManufacturerCommunications, CommunicationTypeIDs.FX);

		public ZString ManufacturerEMAIL => manufacturerEmailIDOutputEmpty ? ZString.Empty : GetCommunicationTypeValue(MainManufacturerCommunications, CommunicationTypeIDs.MA);

		IConsignment GoodsShipmentConsignment => goodsShipmentConsignment ??= GoodsShipment?.Consignment;
		IConsignment goodsShipmentConsignment;

		ILocation ConsignmentUnloadingLocation => consignmentUnloadingLocation ??= Consignment?.UnloadingLocation;
		ILocation consignmentUnloadingLocation;

		ILocation GoodsShipmentConsignmentLoadingLocation => goodsShipmentConsignmentLoadingLocation ??= GoodsShipmentConsignment?.LoadingLocation;
		ILocation goodsShipmentConsignmentLoadingLocation;

		public ZString PortofLoading => GoodsShipmentConsignmentLoadingLocation?.Name ?? ZString.Empty;

		public ZString DepartureId => departureId ??= GoodsShipmentConsignmentLoadingLocation?.ID ?? ZString.Empty;
		string departureId;

		public ZString PortofLoadingChineseDescription => DepartureId.IsEmpty ? ZString.Empty : TWRefCusCodeListLoader.LoadTaiwanRefCusCodeListDescription(Factory, DepartureId, Codes.ECFALoadingPort);

		public ZDateTime DepartureDate => GoodsShipmentConsignmentLoadingLocation?.LoadingDateTime ?? ZDateTime.Empty;

		public ZString EstimatedLoadingCode => GoodsShipmentConsignmentLoadingLocation?.EstimatedLoadingCode ?? ZString.Empty;

		public ZString PortofDischarge => ConsignmentUnloadingLocation?.Name ?? ZString.Empty;

		ZString PortofDischargeId => portofDischargeId ??= ConsignmentUnloadingLocation?.ID ?? ZString.Empty;
		string portofDischargeId;

		public ZString PortofDischargeChineseDescription => PortofDischargeId.IsEmpty ? ZString.Empty : TWRefCusCodeListLoader.LoadTaiwanRefCusCodeListDescription(Factory, PortofDischargeId, Codes.ECFAUnloadingPort);

		public ZString CountryofDestination => header.Declaration?.FinalDestinationCountry?.RN_Desc ?? ZString.Empty;

		public ZString Observations => Application?.Observations ?? ZString.Empty;

		public ZString MarksNumbers => MessageSendingObject.Packaging?.MarksNumbers ?? ZString.Empty;

		public NX101ControllingMessageHeaderSectionBodyDocumentWrapperCollection InvoiceLines
		{
			get
			{
				if (invoiceLines == null)
				{
					invoiceLines = new NX101ControllingMessageHeaderSectionBodyDocumentWrapperCollection(this, header.TW1_CertificateType, Factory);
					invoiceLines.AddLinesFrom(GoodsShipment.GovernmentAgencyGoodsItems.OfType<NX101GoodsShipmentGovernmentAgencyGoodsItem>());
				}
				return invoiceLines;
			}
		}
		NX101ControllingMessageHeaderSectionBodyDocumentWrapperCollection invoiceLines;

		string invoicePriceDecimalPlaces;

		internal string InvoicePriceDecimalPlaces
		{
			get
			{
				if (invoicePriceDecimalPlaces == null)
				{
					var decimalPlaces = InvoiceLines.Cast<NX101ControllingMessageHeaderSectionBodyDocumentWrapper>().Select(x => x.InvoicePriceInternal.DecimalPlaces).Max();
					invoicePriceDecimalPlaces = $"F{decimalPlaces}";
				}
				return invoicePriceDecimalPlaces;
			}
		}

		public ZString QuantityAndUnitSummary
		{
			get
			{
				if (quantityAndUnitSummary == null)
				{
					var quantityAndUnits = InvoiceLines.Cast<NX101ControllingMessageHeaderSectionBodyDocumentWrapper>().
						GroupBy(c => c.Unit).
						Select(g => new { Quantity = g.Sum(a => a.Quantity), Code = g.Key }).ToList();
					var mutileQuantityAndUnits = string.Empty;
					if (quantityAndUnits.Count > 0)
					{
						var summaryQuantityFormat = $"N{quantityAndUnits.Select(x => ((ZDecimal)x.Quantity).DecimalPlaces).Max()}";
						mutileQuantityAndUnits = string.Join("\r\n", quantityAndUnits.Select(c => $"{c.Quantity.ToString(summaryQuantityFormat, CultureInfo.InvariantCulture.NumberFormat)} {c.Code}"));
					}
					quantityAndUnitSummary = $"____________\r\n{mutileQuantityAndUnits}\r\nVVVVVVVVVV";
				}
				return quantityAndUnitSummary;
			}
		}

		string quantityAndUnitSummary;

		public ZString TradersRemarks => tradersRemarks ??= string.Join("\r\n", MessageSendingObject.GovernmentProcedure?.Select(c => c.Description) ?? Enumerable.Empty<ZString>());
		string tradersRemarks;

		public ZString GoodsInformation => $"{TradersRemarks}.\r\n☆☆☆";

		public ZString CountrysofOrigin => string.Join("/", ConsignmentGovernmentAgencyGoodsItem?.Origins?.Select(c => GetCountryDescription(c.CountryCode)) ?? Enumerable.Empty<ZString>());

		ZString GetCountryDescription(ZString countryCode) => (ZString?)Factory.LoadFromNaturalKey<MasterFiles.Business.RefCountry>(RefCountrySchema.RN_Code, countryCode)?.RN_DescMultilingual?.GetLocalizedValue(Core.SharedConstants.Languages.English) ?? ZString.Empty;

		IEnumerable<ZString> AdditionalDocument => additionalDocument ??= Consignment?.AdditionalDocument?.Select(c => c.ID) ?? Enumerable.Empty<ZString>();
		IEnumerable<ZString> additionalDocument;

		IEnumerable<ZString> PreviousDocuments => previousDocuments ??= ConsignmentGovernmentAgencyGoodsItem?.PreviousDocuments?.Select(c => c.ID) ?? Enumerable.Empty<ZString>();
		IEnumerable<ZString> previousDocuments;

		public ZString AdditionalDocumentIDs => additionalDocumentIDs ??= string.Join("/", AdditionalDocument);
		string additionalDocumentIDs;

		public ZString PreviousDocumentIDs => previousDocumentIDs ??= string.Join("/", PreviousDocuments);
		string previousDocumentIDs;

		public ZString DocumentIDs => documentIDs ??= string.Join("/", AdditionalDocument.Concat(PreviousDocuments));
		string documentIDs;

		INX101Application Application => application ??= MessageSendingObject.Application;
		INX101Application application;

		public ZString ManufacturerPrintingCode => Application.ManufacturerPrintingCode;

		public ZString GoodsReleaseReason
		{
			get
			{
				var result = ZString.Empty;
				var goodsReleaseCode = Application?.GoodsReleaseCode ?? ZString.Empty;
				if (goodsReleaseCode == YesNoList.Codes.No)
				{
					result = "N/A";
				}
				else if (goodsReleaseCode == YesNoList.Codes.Yes)
				{
					var code = Application.GoodsReleaseReasonCode;
					result = $"{code}{new CPT_127_GoodsReleaseReasonCodeList().GetDescriptionFromCode(code)}";
				}
				return result;
			}
		}

		public ZString TriangularTrade => Application?.TriangularTradeCode ?? ZString.Empty;

		public ZString ContainerNumber => string.Join("/", GoodsShipmentConsignment?.TransportEquipments?.Select(c => c.ID) ?? Enumerable.Empty<ZString>());

		public ZString Notes => string.Join("\r\n", GoodsShipmentConsignment?.AdditionalInformations?.Select(c => c.StatementDescription) ?? Enumerable.Empty<ZString>());

		ZString ContactOffice => Application?.ContactOffice ?? ZString.Empty;

		public ZString ContactOfficeDescription => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, ContactOffice, Core.Constants.CountryCodes.Taiwan, Codes.TaiwanCertificateOfOriginIssuingUnit, ZDateTime.Today)?.Languages?.Cast<ZZRefCusCodeListLanguageCombined>()?.FirstOrDefault(c => c.ZXA_ZX6_NKLanguage == Core.SharedConstants.Languages.English)?.ZXA_Description ?? ZString.Empty;

		public ZString ContactOfficeChineseDescription => TWRefCusCodeListLoader.LoadTaiwanRefCusCodeListDescription(Factory, ContactOffice, Codes.TaiwanCertificateOfOriginIssuingUnit);

		ZString VesselName => GoodsShipmentConsignment?.DepartureTransportMeans?.Name ?? ZString.Empty;

		ZString VoyageFlightNo => GoodsShipmentConsignment?.BorderTransportMeans?.JourneyID ?? ZString.Empty;

		public ZString Vessel => $"{VesselName} {VoyageFlightNo}";

		public ZString ECFANote => Application.ECFAPrintingDescription;

		#region MarksNumbers

		public ZString AlignQuantityAndUnitSummaryMarksNumbers { get; private set; }

		public ZString AlignTradersRemarksMarksNumbers { get; private set; }

		public ZString LastMarksNumbers { get; private set; }

		void SetMarksNumbers()
		{
			IEnumerator<ZString> marksNumbersTotalRows = DocumentWrapperHelper.SplitTextByLineBreak(MarksNumbers, 24, true).GetEnumerator();
			var hasNext = marksNumbersTotalRows.MoveNext();
			if (hasNext)
			{
				SetInvoiceLinesMarksNumbers(marksNumbersTotalRows, ref hasNext);
			}
			if (hasNext)
			{
				AlignQuantityAndUnitSummaryMarksNumbers = marksNumbersTotalRows.GetMarksNumbersByAlignedFields(QuantityAndUnitSummary, ref hasNext);
			}
			if (hasNext)
			{
				AlignTradersRemarksMarksNumbers = marksNumbersTotalRows.GetMarksNumbersByAlignedFields(TradersRemarks, ref hasNext);
			}
			if (hasNext)
			{
				SetLastMarksNumbers(marksNumbersTotalRows);
			}
		}

		void SetInvoiceLinesMarksNumbers(IEnumerator<ZString> marksNumbersTotalRows, ref bool hasNext)
		{
			foreach (var invoiceLine in InvoiceLines.Cast<NX101ControllingMessageHeaderSectionBodyDocumentWrapper>())
			{
				if (hasNext)
				{
					invoiceLine.AssignMarksNumbers(marksNumbersTotalRows, ref hasNext);
				}
				else
				{
					hasNext = false;
					break;
				}
			}
		}

		void SetLastMarksNumbers(IEnumerator<ZString> marksNumbersTotalRows)
		{
			var marksNumbersInfo = new ZStringBuilder();
			do
			{
				marksNumbersInfo.Append(marksNumbersTotalRows.Current);
			}
			while (marksNumbersTotalRows.MoveNext());
			LastMarksNumbers = marksNumbersInfo.ToStringWithNewLineBetweenAppends();
		}

		#endregion
	}
}
