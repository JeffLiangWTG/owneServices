using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

[DependentBusinessObject(typeof(JobDeclaration), nameof(JobDeclaration.CustomsEntryHeaders))]
public class CusEntryHeader : EU.Business.Declaration.CusEntryHeader,
	Integration.Customs.PL.ICusEntryHeader,
	IEDIMessageCollectionProvider
{
	public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages => Messages;

	[ChildEditable(true)]
	public new EDIMessageCollection Messages => (EDIMessageCollection)base.Messages;

	protected override Messaging.Business.EDIMessageCollection GetNewMessageCollection() => new EDIMessageCollection(this);

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	public new CusEntryHeaderLookups Lookups => (CusEntryHeaderLookups)base.Lookups;

	protected override Customs.Business.CusEntryHeaderLookups GetNewLookups() => new CusEntryHeaderLookups(this);

	public override ZBool ShouldSetUCRinBGMReferenceNumber => false;

	public new CusEntryHeaderValidation Validation => (CusEntryHeaderValidation)base.Validation;

	protected override Customs.Business.CusEntryHeaderValidation GetNewValidation() => Declaration?.GetCusEntryHeaderValidation(this) ?? new CusEntryHeaderValidation(this);

	protected override void OnFactorySaving()
	{
		base.OnFactorySaving();
		PopulateCH_BGMReferenceIfNeeded();
	}

	public void PopulateCH_BGMReferenceIfNeeded()
	{
		var branchCode = Declaration?.Branch?.GB_Code ?? GlbBranch.CurrentBranch.GB_Code;
		var yearString = ZDateTime.Now.ToString("yy", CultureInfo.InvariantCulture);

		PopulateNumberPropertyIfRequired(CH_BGMReferenceInfo, GetNewReferenceNumber
			, ignoreInDatabaseCheck: true
			, forceRegenerate: !IsCurrentLRNMatchingData() && !HasBeenLodgedAtCustoms && !IsWaitingForResponse
		);

		ZString GetNewReferenceNumber(BusinessObjectFactory factory)
		{
			return Env.NumberFountains.PLBGMReferenceNumber(branchCode, yearString).GetNextFormatted(Factory);
		}

		ZBool IsCurrentLRNMatchingData()
		{
			return CH_BGMReference.StartsWith(System.FormattableString.Invariant($"{yearString}{branchCode}"));
		}
	}

	public new JobComInvoiceHeader RandomHeader => base.RandomHeader as JobComInvoiceHeader;

	public new IAllCusEntryLineCollection<CusEntryLine> AllEntryLines => (IAllCusEntryLineCollection<CusEntryLine>)base.AllEntryLines;

	protected override IAllCusEntryLineCollection<Customs.Business.CusEntryLine> GetAllEntryLinesCollection() => new AllCusEntryLineCollection<CusEntryLine>(this);

	public new ICusEntryLineCollection<CusEntryLine> MergedLines => (ICusEntryLineCollection<CusEntryLine>)base.MergedLines;

	public new IEnumerable<JobComInvoiceLine> InvoiceLines => base.InvoiceLines.Cast<JobComInvoiceLine>();

	protected override ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection() => new CusEntryLineCollection<CusEntryLine>(this);

	public new IEnumerable<SupportingDocument> SupportingDocuments => base.SupportingDocuments.Cast<SupportingDocument>();

	public new IEnumerable<PreviousDocument> PreviousDocuments => base.PreviousDocuments.Cast<PreviousDocument>();

	public new IEnumerable<AdditionalInfo> AdditionalInfos => base.AdditionalInfos.Cast<AdditionalInfo>();

	public bool HasAdditionalInfoCode(ZString additionalInfoCode) => Declaration.HasAdditionalInfoCode(additionalInfoCode)
																	|| InvoiceHeaders.Cast<JobComInvoiceHeader>().Any(x => x.HasAdditionalInfoCode(additionalInfoCode))
																	|| AllEntryLines.Cast<CusEntryLine>().Any(x => x.RandomLine.HasAdditionalInfoCode(additionalInfoCode));

	public bool HasPreviousDocumentCode(ZString previousDocumentCode) => Declaration.HasPreviousDocumentCode(previousDocumentCode)
																		|| InvoiceHeaders.Cast<JobComInvoiceHeader>().Any(x => x.HasPreviousDocumentCode(previousDocumentCode))
																		|| AllEntryLines.Cast<CusEntryLine>().Any(x => x.RandomLine.HasPreviousDocumentCode(previousDocumentCode));

	public bool HasSupportingDocumentCode(ZString supportingDocumentCode) => Declaration.HasSupportingDocumentCode(supportingDocumentCode)
																			|| InvoiceHeaders.Cast<JobComInvoiceHeader>().Any(x => x.HasSupportingDocumentCode(supportingDocumentCode))
																			|| AllEntryLines.Cast<CusEntryLine>().Any(x => x.RandomLine.HasSupportingDocumentCode(supportingDocumentCode));

	public bool HasConcessionCode(ZString concessionCode) => AllEntryLines.Cast<CusEntryLine>()
		.Any(x => x.RandomLine.ConcessionCodes
			.Any(concession => concession == concessionCode));
}
