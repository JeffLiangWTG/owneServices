using Enterprise.Messaging.Business;

namespace Enterprise.Customs.PL.Business;

public static class EDIInterchangeExtensions
{
	internal static InterchangeReadContext GetReadContext(this EDIInterchange interchange) => new(interchange);
}
