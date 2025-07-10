using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;
using CusEntryHeader = Enterprise.Customs.PL.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.PL.Business;

public class AESConsignmentProvider : ConsignmentProvider, IAESConsignment
{
	public AESConsignmentProvider(CusEntryHeader entryHeader)
		: base(entryHeader)
	{
	}

	public string ModeOfTransportAtTheBorder => CachedValueHelper.GetValue(ref modeOfTransportAtTheBorder,
		() => CheckC0374() ? null : ModeOfTransportAtTheBorderValue);
	CachedValue<string> modeOfTransportAtTheBorder;

	public decimal? GrossMass => CachedValueHelper.GetValue(ref grossMass, () => GrossMassValue);
	CachedValue<decimal?> grossMass;

	public string ReferenceNumberUCR => declaration.JE_UCR;

	public string CarrierIdentificationNumber => CachedValueHelper.GetValue(ref carrierIdentificationNumber, GetCarrierIdentificationNumber);
	protected virtual string GetCarrierIdentificationNumber() => CheckG0048() ? null : CarrierIdentificationNumberValue;
	CachedValue<string> carrierIdentificationNumber;

	public IConsigneeConsignor Consignor => CachedValueHelper.GetValue(ref consignor, GetConsignor);
	protected virtual IConsigneeConsignor GetConsignor() => CheckC0846() ? null : ConsignorValue;
	CachedValue<IConsigneeConsignor> consignor;

	public IConsigneeConsignor Consignee => CachedValueHelper.GetValue(ref consignee, () => CheckC0283() ? null : GetConsigneeCore());
	protected virtual IConsigneeConsignor GetConsigneeCore() => AESConsignmentConsigneeConsignorProvider.NewOrNull(declaration.ImporterDocumentaryAddress);
	CachedValue<IConsigneeConsignor> consignee;

	public IReadOnlyCollection<ICountryOfRoutingOfConsignment> CountryOfRoutingOfConsignments => countryOfRoutingOfConsignments ??= GetCountryOfRoutingOfConsignments();
	protected virtual IReadOnlyCollection<ICountryOfRoutingOfConsignment> GetCountryOfRoutingOfConsignments() => declaration.ItineraryCountries
		.Cast<ItineraryCountry>()
		.Select((x, i) => new AESCountryOfRoutingOfConsignmentProvider(x, i + 1))
		.ToArray();
	IReadOnlyCollection<ICountryOfRoutingOfConsignment> countryOfRoutingOfConsignments;

	public IActiveBorderTransportMeans ActiveBorderTransportMeans => CachedValueHelper.GetValue(ref activeBorderTransportMeans,
		() => CheckC0890() ? GetActiveBorderTransportMeansValue : null);
	CachedValue<IActiveBorderTransportMeans> activeBorderTransportMeans;

	public IReadOnlyCollection<IDocument> TransportDocument => transportDocuments ??= GetTransportDocuments();
	protected virtual IReadOnlyCollection<IDocument> GetTransportDocuments() => IsAESTransitionPeriod
		? Array.Empty<IDocument>()
		: entryInstruction.AdditionalInfos.Cast<AdditionalInfo>()
			.Concat(entryHeader.AdditionalInfos.Cast<AdditionalInfo>())
			.Concat(GetAdditionalInfosFromInvoicesAndInvoiceLines())
			.Where(x => x.CSI_SubType == AdditionalInfoKindList.Codes.TRA)
			.GroupBy(x => new { x.CSI_Code, x.CSI_ReferenceNumber })
			.Select((x, i) => new AESDocumentProvider(x.First(), true))
			.ToArray();
	IReadOnlyCollection<IDocument> transportDocuments;

	public string TransportChargesMethodOfPayment => CachedValueHelper.GetValue(ref transportChargesMethodOfPayment, GetTransportChargesMethodOfPayment);
	protected virtual string GetTransportChargesMethodOfPayment() => CheckC0375() || CheckR0013G() ? null : entryHeader.RandomHeader.ZG_TransportChargesMethodOfPayment;
	CachedValue<string> transportChargesMethodOfPayment;

	string ModeOfTransportAtTheBorderValue => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode);

	decimal? GrossMassValue => entryHeader.TotalGrossWeightInKG.Round(3);

	string CarrierIdentificationNumberValue => EuEoriResolver.GetRegNoWithCountryCode(declaration.ShippingLine);

	string DeclarantIdentificationNumberValue => EuEoriResolver.GetRegNoWithCountryCode(declaration.DeclarantAddress);

	AESConsignmentConsigneeConsignorProvider ConsignorValue => AESConsignmentConsigneeConsignorProvider.NewOrNull(declaration.SupplierDocumentaryAddress);

	bool IsAESTransitionPeriod => entryInstruction.IsAESTransitionPeriod();

	AESActiveBorderTransportMeansProvider GetActiveBorderTransportMeansValue => new AESActiveBorderTransportMeansProvider(declaration);

	bool CheckC0374() => IsSubStyleBOrCOrEOrF();

	bool CheckG0048() => CarrierIdentificationNumberValue == DeclarantIdentificationNumberValue;

	bool CheckC0846() => declaration.InvoiceLines.Any(invoiceLine => !((JobComInvoiceLine)invoiceLine).JI_OA_ExporterAddress.IsEmpty);

	bool CheckC0283() => IsAESTransitionPeriod
		&& entryHeader.MergedLines
			.Any(entryLine => entryLine.AdditionalInfos
				.Any(addInfo => addInfo.CSI_SubType == AdditionalInfoKindList.Codes.INF && addInfo.CSI_Code == AdditionalInfoCodes._30600));

	bool CheckC0890() => !GetTransportId().IsEmpty && ExportJobDeclarationValidationHelper.IsC0890TransportIdAllowed(declaration);

	ZString GetTransportId() => declaration.IsAir
		? declaration.JE_VoyageFlightNo
		: declaration.IsSea && declaration.ZG_BorderTransportMeans == ExportBorderTransportMeansList.Codes._10
			? declaration.JE_LloydsIMO
			: declaration.JE_VesselName;

	bool CheckC0375() => IsSubStyleBOrCOrEOrF();

	bool CheckR0013G()
	{
		var transportChargesMethodOfPayment = entryHeader.TransportChargesMoP;

		return transportChargesMethodOfPayment.IsEmpty ||
			entryHeader.MergedLines.Any(entryLine => entryLine.TransportChargesMethodOfPayment != transportChargesMethodOfPayment);
	}

	IEnumerable<CusSupportingInfo> GetAdditionalInfosFromInvoicesAndInvoiceLines()
	{
		foreach (var invoiceLine in entryHeader.InvoiceLines)
		{
			foreach (var lineAdditionalInfo in invoiceLine.AdditionalInfos)
			{
				yield return lineAdditionalInfo;
			}
			foreach (var invoiceAdditionalInfo in invoiceLine.InvoiceHeader.AdditionalInfos)
			{
				yield return invoiceAdditionalInfo;
			}
		}
	}
}
