using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Interfaces
{
	public interface ICusUnderbondUnionCollectionParent : IBusiness
	{
		CusUnderbondUnionCollection AllUnderbonds { get; }
		ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProviders();
		bool IsForAirCargo { get; }
	}

	public interface ICusUnderbondNilUnderbondPerformer : ICusUnderbondUnionCollectionParent
	{
		ZString PerformNilUnderbond(CusUnderbond underbond);
	}
}
