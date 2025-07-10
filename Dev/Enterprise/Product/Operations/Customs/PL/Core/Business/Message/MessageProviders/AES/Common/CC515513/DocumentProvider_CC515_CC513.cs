using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public sealed class DocumentProvider_CC515_CC513 : AESDocumentProvider
{
	readonly bool isAesTransitionPeriod;

	public DocumentProvider_CC515_CC513(CusSupportingInfo cusSupportingInfo, bool referenceNumberAsDescription, bool isAesTransitionPeriod)
		: base(cusSupportingInfo, referenceNumberAsDescription)
	{
		this.isAesTransitionPeriod = isAesTransitionPeriod;
	}

	protected override string GetReferenceNumberCore() => AesRuleHelper.ApplyE1104Rule(base.GetReferenceNumberCore(), isAesTransitionPeriod);
}
