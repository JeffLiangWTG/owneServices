using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class SupportingDocumentProvider : ISupportingDocument
{
	public SupportingDocumentProvider(int sequenceNumber, NctsSupportingDocument nctsSupportingDocument, bool isInPhase5TransitionPeriod)
	{
		SequenceNumber = sequenceNumber.ToString();
		this.nctsSupportingDocument = Argument.NotNull(nctsSupportingDocument, nameof(nctsSupportingDocument));
		this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
	}
	readonly NctsSupportingDocument nctsSupportingDocument;
	readonly bool isInPhase5TransitionPeriod;

	public string DocumentLineItemNumber => CachedValueHelper.GetValue(ref documentLineItemNumber, () => nctsSupportingDocument.CSI_ItemNumber.IsEmpty ? null : nctsSupportingDocument.CSI_ItemNumber.ToString());
	CachedValue<string> documentLineItemNumber;

	public string ComplementOfInformation => nctsSupportingDocument.CSI_ReferenceNumber2;

	public int ComplementOfInformationMaxLength => CachedValueHelper.GetValue(ref complementOfInformationMaxLength, () =>
	{
		var maxLengthInTransitionPeriod = 26;
		var maxLengthOutsideTransitionPeriod = 35;
		return isInPhase5TransitionPeriod
			? maxLengthInTransitionPeriod
			: maxLengthOutsideTransitionPeriod;
	});
	CachedValue<int> complementOfInformationMaxLength;

	public string SequenceNumber { get; }

	public string DocumentType => nctsSupportingDocument.CSI_Code;

	public string ReferenceNumber => nctsSupportingDocument.Validation is IRuleG0321Checker validationChecker && validationChecker.CheckRuleG0321() ? "0" : nctsSupportingDocument.CSI_ReferenceNumber.ToString();

	public int ReferenceNumberMaxLength => CachedValueHelper.GetValue(ref referenceNumberMaxLength, () =>
	{
		var maxLengthInTransitionPeriod = 35;
		var maxLengthOutsideTransitionPeriod = 70;
		return isInPhase5TransitionPeriod
			? maxLengthInTransitionPeriod
			: maxLengthOutsideTransitionPeriod;
	});
	CachedValue<int> referenceNumberMaxLength;
}
