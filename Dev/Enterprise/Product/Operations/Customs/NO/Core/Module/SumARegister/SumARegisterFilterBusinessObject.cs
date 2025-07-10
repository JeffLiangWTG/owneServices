using SumAModule = Enterprise.Customs.EFTA.TemporaryStorageRegister.Module;

namespace Enterprise.Customs.NO.Module;

sealed class SumARegisterFilterBusinessObject : SumAModule.SumARegisterFilterBusinessObject
{
	protected override SumAModule.SumARegisterFilterBusinessObjectLookups GetNewSumARegisterBusinessObjectLookups()
		=> new SumARegisterFilterBusinessObjectLookups(this);
}
