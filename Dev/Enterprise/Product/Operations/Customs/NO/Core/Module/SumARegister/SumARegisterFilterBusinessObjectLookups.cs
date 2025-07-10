using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Module;

sealed class SumARegisterFilterBusinessObjectLookups(EFTA.TemporaryStorageRegister.Module.SumARegisterFilterBusinessObject filterBizObj)
	: EFTA.TemporaryStorageRegister.Module.SumARegisterFilterBusinessObjectLookups(filterBizObj)
{
	public override CodeDescriptionPairList OwnerReferenceTypeList => Factory.GetCachedValue("42BBA2A3-2842-491B-B7E8-96A07206530F", () =>
		new CodeDescriptionPairList
		{
			new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes._AWB, Business.OwnerReferenceTypeList.Descriptions._AWB),
			new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes._ULD, Business.OwnerReferenceTypeList.Descriptions._ULD),
			new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes._ZZZ, Business.OwnerReferenceTypeList.Descriptions._ZZZ)
		});
}
