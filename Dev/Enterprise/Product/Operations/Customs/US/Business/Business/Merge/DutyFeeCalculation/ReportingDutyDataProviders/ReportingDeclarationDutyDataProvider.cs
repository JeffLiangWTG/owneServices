using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	class ReportingDeclarationDutyDataProvider : IDeclarationDutyDataProvider
	{
		public ReportingDeclarationDutyDataProvider(JobDeclaration declaration, bool useSPI)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
			this.useSPI = useSPI;
			dutyData = declaration;
			invoiceLinesData = new Dictionary<ZGuid, ReportingInvoiceLineDutyDataProvider>();
			entryLinesData = new Dictionary<ZGuid, ReportingEntryLineDutyDataProvider>();
			entriesData = new Dictionary<ZGuid, ReportingEntryHeaderDutyDataProvider>();
		}

		internal void StoreResult()
		{
			var dutyColumn = useSPI ? JobComInvoiceLine.Schema.US_FTADuty : JobComInvoiceLine.Schema.US_NonFTADuty;
			var payableMPFColumn = useSPI ? JobComInvoiceLine.Schema.US_FTAPayableMPF : JobComInvoiceLine.Schema.US_NonFTAPayableMPF;
			var spiColumn = useSPI ? JobComInvoiceLine.Schema.US_FTASPI : null;
			foreach (var invoiceLine in invoiceLinesData.Values)
			{
				invoiceLine.StoreResult(dutyColumn, payableMPFColumn, spiColumn);
			}
		}

		readonly public bool useSPI;
		readonly JobDeclaration declaration;
		readonly IDutyDataLineHeaderProvider dutyData;
		readonly Dictionary<ZGuid, ReportingInvoiceLineDutyDataProvider> invoiceLinesData;
		readonly Dictionary<ZGuid, ReportingEntryLineDutyDataProvider> entryLinesData;
		readonly Dictionary<ZGuid, ReportingEntryHeaderDutyDataProvider> entriesData;

		ZBool IDeclarationDutyDataProvider.IsFormalImport => declaration.IsFormalImport;

		ZBool IDeclarationDutyDataProvider.IsFTZAdmission => declaration.IsFTZAdmission;

		bool IDeclarationDutyDataProvider.IsFixedTransportInstallations => declaration.IsFixedTransportInstallations;

		ZBool IDeclarationDutyDataProvider.US_MonthlyFiling => declaration.US_MonthlyFiling;

		IEntryHeaderDutyDataProvider IDeclarationDutyDataProvider.EntrySummaryEntry
		{
			get
			{
				var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				return entry == null ? null : GetOrCreate(entry);
			}
		}

		IEntryHeaderDutyDataProvider IDeclarationDutyDataProvider.FTZEntry
		{
			get
			{
				var entry = declaration.ActiveEntryHeaders.FTZEntry;
				return entry == null ? null : GetOrCreate(entry);
			}
		}

		IEnumerable<IInvoiceLineDutyDataProvider> IDeclarationDutyDataProvider.InvoiceLines => declaration.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => GetOrCreate(x));

		IEnumerable<IDutyDataLineHeader> IDutyDataLineHeaderProvider.EntriesToCalculateDutyFeeTax => entriesToCalculateDutyFeeTax ?? (entriesToCalculateDutyFeeTax = dutyData.EntriesToCalculateDutyFeeTax.Cast<CusEntryHeader>().Select(x => GetOrCreate(x)).ToArray());
		ReportingEntryHeaderDutyDataProvider[] entriesToCalculateDutyFeeTax;

		BusinessObjectFactory IDutyDataLineHeaderProvider.Factory => declaration.Factory;

		ZDecimal? IDutyDataLineHeaderProvider.OverridenTotalMPFPayable => dutyData.OverridenTotalMPFPayable;

		bool IDutyDataLineHeaderProvider.IsCustomsChargeRelevantForDecType(string chargeCode) => dutyData.IsCustomsChargeRelevantForDecType(chargeCode);

		internal ReportingInvoiceLineDutyDataProvider GetOrCreate(JobComInvoiceLine invoiceLine)
		{
			var key = invoiceLine.PK;
			if (!invoiceLinesData.TryGetValue(key, out var data))
			{
				data = new ReportingInvoiceLineDutyDataProvider(invoiceLine, this);
				invoiceLinesData.Add(key, data);
			}
			return data;
		}

		internal ReportingEntryLineDutyDataProvider GetOrCreate(CusEntryLine entryLine)
		{
			var key = entryLine.PK;
			if (!entryLinesData.TryGetValue(key, out var data))
			{
				data = new ReportingEntryLineDutyDataProvider(entryLine, this);
				entryLinesData.Add(key, data);
			}
			return data;
		}

		internal ReportingEntryHeaderDutyDataProvider GetOrCreate(CusEntryHeader entry)
		{
			var key = entry.PK;
			if (!entriesData.TryGetValue(key, out var data))
			{
				data = new ReportingEntryHeaderDutyDataProvider(entry, this);
				entriesData.Add(key, data);
			}
			return data;
		}

		internal ZString CalculateNewSPI(ZString countryOfOrigin, CodeDescriptionPairList spiList)
		{
			return useSPI ? GetDefaultSPI(countryOfOrigin, spiList) : ZString.Empty;
		}

		ZString GetDefaultSPI(ZString countryOfOrigin, CodeDescriptionPairList spiList)
		{
			var result = GetEffectiveCountry(countryOfOrigin);
			if (!spiList.ContainsCode(result))
			{
				result = spiList.OfType<ICodeDescription>().Select(x => x.Code).FirstOrDefault(x => x != SPICompleteList.MoreCodes.NotApplicable);
			}
			return result;
		}

		ZString GetEffectiveCountry(ZString countryOfOrigin)
		{
			var result = countryOfOrigin;
			if (CanadaProvinceTerritoryCodes.IsCanadianProvince(countryOfOrigin) ||
					CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(countryOfOrigin))
			{
				result = Core.Constants.CountryCodes.Canada;
			}
			return result;
		}
	}
}
