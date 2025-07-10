using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.DocumentWrappers;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class UTBEntryLineDetailWrapper : NonPersistentBusinessObject, IEntryLineDetails
{
	readonly Declaration.CusEntryLine cusEntryLine;
	readonly ZGuid linePK;

	public UTBEntryLineDetailWrapper(CusEntryLine line, ZGuid linePK) : base((line as BusinessObject)?.Factory ?? new BusinessObjectFactory())
	{
		cusEntryLine = (Declaration.CusEntryLine)line;
		this.linePK = linePK;
	}

	public ZInt ArticleSequenceNumber => cusEntryLine.CL_LineNumber;
	public ZDecimal TotalAmountDuties => cusEntryLine.DutyAmount;

	public ZDecimal TotalAmountTax => cusEntryLine.VATDetails;

	public ZDecimal TotalAmountDutiesAndTax => cusEntryLine.DutyDetailsForVAT;

	public BusinessObjectCollectionWrapper<UTBEntryLineFeeWrapper> FeeSummary => CachedValueHelper.GetValue(ref feeDetailsCached, () =>
	{
		var cusEntryLineFeesList = cusEntryLine.Fees.Cast<CusEntryLineFee>().Select(x => new UTBEntryLineFeeWrapper(x, true));
		var lineFeeDetails = new BusinessObjectCollectionWrapper<UTBEntryLineFeeWrapper>(cusEntryLineFeesList);
		return lineFeeDetails;
	});
	CachedValue<BusinessObjectCollectionWrapper<UTBEntryLineFeeWrapper>> feeDetailsCached;

	public BusinessObjectCollectionWrapper<UTBEntryLineFeeWrapper> FeeSummaryForPrint => IsLastFeeOfEntryLine ? FeeSummary : new BusinessObjectCollectionWrapper<UTBEntryLineFeeWrapper>();

	IEnumerable<IEntryLineFee> IEntryLineDetails.LineFeesSummary => FeeSummary.Select(x => x as IEntryLineFee);

	public ZString MovementReferenceNumber => cusEntryLine.Header.EntryNumber;

	public ZGuid LastFeePK => FeeSummary.Cast<UTBEntryLineFeeWrapper>().Last().LineFee.PK;

	public ZBool IsLastFeeOfEntryLine => LastFeePK == linePK;
}
