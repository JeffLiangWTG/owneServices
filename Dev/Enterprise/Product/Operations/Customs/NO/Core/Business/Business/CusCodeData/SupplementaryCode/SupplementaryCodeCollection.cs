using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

public sealed class SupplementaryCodeCollection : BaseSupplementaryCodeCollection<SupplementaryCode>
{
	public SupplementaryCodeCollection(ZPropertyInfo info, BaseSupplementaryCodeProvider provider)
		: base(Argument.NotNull(info, nameof(info)), Argument.NotNull(provider, nameof(provider)).NumberOfCodes, provider.CodesStartingOrder)
	{
	}
}
