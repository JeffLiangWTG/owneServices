using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class AdditionalReferenceProvider : IAdditionalReference
{
	public AdditionalReferenceProvider(int sequenceNumber, AdditionalInfo additionalReference, bool isInPhase5TransitionPeriod)
	{
		this.additionalReference = Argument.NotNull(additionalReference, nameof(additionalReference));
		SequenceNumber = sequenceNumber.ToString();
		this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
	}
	readonly AdditionalInfo additionalReference;
	readonly bool isInPhase5TransitionPeriod;

	public string SequenceNumber { get; }

	public string ReferenceType => additionalReference.CSI_Code;
	public string ReferenceNumber => additionalReference.Validation is IRuleG0321Checker validationChecker && validationChecker.CheckRuleG0321() ? "0" : additionalReference.CSI_ReferenceNumber.ToString();

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
