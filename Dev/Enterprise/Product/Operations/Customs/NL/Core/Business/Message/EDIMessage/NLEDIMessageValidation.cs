using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class NLEDIMessageValidation : EDIMessageValidation
{
	public NLEDIMessageValidation(NLEDIMessage parent) : base(parent)
	{
	}

	protected new NLEDIMessage Parent => (NLEDIMessage)base.Parent;

	protected override void CheckEM_HeldUntilDateIsValidZDateTimeRange()
	{
	}
}
