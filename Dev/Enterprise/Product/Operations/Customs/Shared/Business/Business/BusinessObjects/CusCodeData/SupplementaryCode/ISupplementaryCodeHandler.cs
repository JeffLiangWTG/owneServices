using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ISupplementaryCodeHandler<out TSupplementaryCode> where TSupplementaryCode : BaseSupplementaryCode
	{
		TSupplementaryCode LoadWithOrder<TParent>(TParent parent, ZShort order)
			where TParent : BusinessObject, ISupplementaryCodeSupporter;

		TSupplementaryCode LoadOrCreate<TParent>(ZString value, TParent parent, ZShort order, ZPropertyInfo targetPropertyInfo)
			where TParent : BusinessObject, ISupplementaryCodeSupporter;
	}
}
