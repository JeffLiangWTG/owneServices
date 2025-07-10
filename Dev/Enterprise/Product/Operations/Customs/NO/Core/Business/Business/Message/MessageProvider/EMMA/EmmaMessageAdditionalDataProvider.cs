using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Customs.NO.MessageContracts.EMMA;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NO.Business;

sealed class EmmaMessageAdditionalDataProvider : IEmmaSystemsFortollingAdditionalDataProvider
{
	string IEmmaSystemsFortollingAdditionalDataProvider.GetContainerMode(CusEntryHeader entryHeader) =>
		entryHeader?.Declaration?.JE_ContainerMode is { } mode && transportInContainerModes.Contains(mode)
			? GoodsTransportedInContainerIndicator
			: GoodsTransportedNotInContainerIndicator;

	decimal? IEmmaSystemsFortollingAdditionalDataProvider.GetTotalInvoiceOrLinesAmount(CusEntryHeader entryHeader)
		=> GetEntryHeaderDataProvider(entryHeader).GetTotalInvoiceAmount();

	IReadOnlyCollection<EmmaSystemsFortollingFortollingVedleggDataProviderAbstractClass> IEmmaSystemsFortollingAdditionalDataProvider.GetDocuments(CusEntryHeader businessObject)
		=> GetAttachedDocuments(businessObject).ToArray();

	string IEmmaSystemsFortollingAdditionalDataProvider.GetGNONumberFirstPart(CusEntryHeader entryHeader)
		=> entryHeader.Declaration?.JE_GoodsNumber;

	string IEmmaSystemsFortollingAdditionalDataProvider.GetGNOorGSPNumberSecondPart(CusEntryHeader entryHeader)
		=> GetEntryHeaderDataProvider(entryHeader).GetGoodsNumberPosition();

	string IEmmaSystemsFortollingAdditionalDataProvider.GetGoodsMarksAndNos(CusEntryLine entryLine)
	{
		var goodsMarks = entryLine?.RandomLine?.JI_GoodsMarks ?? ZString.Empty;
		return !goodsMarks.IsEmpty ? goodsMarks : GoodsMarksADR;
	}

	string IEmmaSystemsFortollingAdditionalDataProvider.GetGoodsDescription(CusEntryLine entryLine)
		=> string.Join("", GetEntryLineDataProvider(entryLine).GetGoodsDescriptions());

	string IEmmaSystemsFortollingAdditionalDataProvider.GetContainer1(CusEntryLine entryLine)
		=> GetContainerNameForEntryLine(entryLine, 0);

	string IEmmaSystemsFortollingAdditionalDataProvider.GetContainer2(CusEntryLine entryLine)
		=> GetContainerNameForEntryLine(entryLine, 1);

	string IEmmaSystemsFortollingAdditionalDataProvider.GetContainer3(CusEntryLine entryLine)
		=> GetContainerNameForEntryLine(entryLine, 2);

	string IEmmaSystemsFortollingAdditionalDataProvider.GetContainer4(CusEntryLine entryLine)
		=> GetContainerNameForEntryLine(entryLine, 3);

	string IEmmaSystemsFortollingAdditionalDataProvider.GetContainer5(CusEntryLine entryLine)
		=> GetContainerNameForEntryLine(entryLine, 4);

	string IEmmaSystemsFortollingAdditionalDataProvider.GetContainer6(CusEntryLine entryLine)
		=> GetContainerNameForEntryLine(entryLine, 5);

	string IEmmaSystemsFortollingAdditionalDataProvider.GetContainer7(CusEntryLine entryLine)
		=> GetContainerNameForEntryLine(entryLine, 6);

	string IEmmaSystemsFortollingAdditionalDataProvider.GetContainer8(CusEntryLine entryLine)
		=> GetContainerNameForEntryLine(entryLine, 7);

	string IEmmaSystemsFortollingAdditionalDataProvider.GetContainer9(CusEntryLine entryLine)
		=> GetContainerNameForEntryLine(entryLine, 8);

	decimal IEmmaSystemsFortollingAdditionalDataProvider.GetGrossWeight(CusEntryLine entryLine)
		=> GetEntryLineDataProvider(entryLine).GetTotalGrossWeight().InKilogramsSafe;

	string IEmmaSystemsFortollingAdditionalDataProvider.GetProcedureCode(CusEntryLine entryLine)
		=> GetEntryLineDataProvider(entryLine).GetProcedureCode();

	decimal? IEmmaSystemsFortollingAdditionalDataProvider.GetNetWeight(CusEntryLine entryLine)
		=> GetEntryLineDataProvider(entryLine).GetTotalNetWeight().InKilogramsSafe;

	decimal? IEmmaSystemsFortollingAdditionalDataProvider.GetCustomsSecondQuantity(CusEntryLine entryLine)
	{
		if (entryLine is not { RandomLine.UniversalTariff.UnitsOfMeasure: { } unitOfMeasures, InvoiceLines: { } invoiceLines })
		{
			return null;
		}

		var customsSecondUnit = unitOfMeasures
			.FirstOrDefault(a => a.ZZ8_Type == UOMTypeList.Codes.CU2)
			?.ZZ8_UOM ?? ZString.Empty;

		return customsSecondUnit.IsEmpty
			? null
			: invoiceLines.Where(l => l.JI_CustomsSecondUnitQty == customsSecondUnit).Sum(l => l.JI_CustomsSecondQuantity);
	}

	string IEmmaSystemsFortollingAdditionalDataProvider.GetValuationCodeOrMethod(CusEntryLine entryLine)
		=> GetEntryLineDataProvider(entryLine).GetValuationCodeOrMethod();

	string IEmmaSystemsFortollingAdditionalDataProvider.GetVatCode(CusEntryLine entryLine)
	{
		if (entryLine is not { Header: { } entryHeader })
		{
			return null;
		}

		return entryHeader.MergedLines.All(i => i.VatCode == UniversalReferenceConstants.RefCusTaxOrFee.MV2)
			? MVAExemptionCode
			: null;
	}

	decimal? IEmmaSystemsFortollingAdditionalDataProvider.GetCustomsValueWithoutLinesPrice(CusEntryLine entryLine)
	{
		if (entryLine == null)
		{
			return default;
		}

		return entryLine.CL_CustomsValue - entryLine.InvoiceLines.Sum(x => x.JI_LinePrice);
	}

	string GetContainerNameForEntryLine(CusEntryLine entryLine, int indexNumber)
	{
		if (!entryLineContainerNames.TryGetValue(entryLine.PK, out var containerNames))
		{
			containerNames = GetContainerNamesForEntryLine(entryLine);
			entryLineContainerNames.Add(entryLine.PK, containerNames);
		}

		return containerNames is not null && containerNames.Length >= indexNumber + 1
			? containerNames[indexNumber]
			: null;
	}

	static ZString[] GetContainerNamesForEntryLine(CusEntryLine entryLine)
		=> entryLine.InvoiceLines
			.SelectMany(line => line.ContainersForInvoiceLinesForBindingOnly)
			.OfType<NonPersistentCusContainer>()
			.Select(container => container.ContainerNumber)
			.Distinct()
			.ToArray();

	static IEnumerable<EmmaSystemsFortollingFortollingVedleggDataProviderClass> GetAttachedDocuments(CusEntryHeader entryHeader)
	{
		var documentsProvider = new EmmaDocumentProvider(entryHeader);
		return documentsProvider.GetDocuments()
			.Select(d => new EmmaSystemsFortollingFortollingVedleggDataProviderClass(d.FileName, d.Description, d.DateAdded))
			.ToArray();
	}

	CusEntryHeaderMessageDataProvider GetEntryHeaderDataProvider(CusEntryHeader entryHeader)
		=> GetEntityMessageDataProvider(entryHeader, entryHeaderMessageDataProviders, e => new(e));

	CusEntryLineMessageDataProvider GetEntryLineDataProvider(CusEntryLine entryLine)
		=> GetEntityMessageDataProvider(entryLine, entryLineMessageDataProviders, e => new(e));

	static TDataProvider GetEntityMessageDataProvider<TEntity, TDataProvider>(TEntity entity, Dictionary<TEntity, TDataProvider> dataProviderDict, Func<TEntity, TDataProvider> factory)
		where TDataProvider : class
	{
		if (!dataProviderDict.TryGetValue(entity, out var provider))
		{
			provider = factory(entity);
			dataProviderDict[entity] = provider;
		}
		return provider;
	}

	static readonly ImmutableHashSet<string> transportInContainerModes = new HashSet<string>()
	{
		Core.Constants.ContainerModes.FCL,
		Core.Constants.ContainerModes.LCL,
		Core.Constants.ContainerModes.ULD,
		Core.Constants.ContainerModes.Containerised
	}.ToImmutableHashSet();

	const string GoodsTransportedInContainerIndicator = "1";
	const string GoodsTransportedNotInContainerIndicator = "0";
	const string GoodsMarksADR = "ADR";
	const string MVAExemptionCode = "M2";

	readonly Dictionary<ZGuid, ZString[]> entryLineContainerNames = new();
	readonly Dictionary<CusEntryHeader, CusEntryHeaderMessageDataProvider> entryHeaderMessageDataProviders = new();
	readonly Dictionary<CusEntryLine, CusEntryLineMessageDataProvider> entryLineMessageDataProviders = new();
}
