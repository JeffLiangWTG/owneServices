using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using OwnerReferenceTypeCodeDescriptionList = Enterprise.Customs.NO.Business.OwnerReferenceTypeList;

namespace Enterprise.Customs.NO.Business;

public class CusTempStorageRegLineLookups(CusTempStorageRegLine parent) : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineLookups(parent)
{
	public override CodeDescriptionPairList OwnerReferenceTypeList => Factory.GetCachedValue("NO|CusTempStorageRegLineLookups|OwnerReferenceTypeList", () => new CodeDescriptionPairList
	{
		new CodeDescriptionPair(OwnerReferenceTypeCodeDescriptionList.Codes._AWB, OwnerReferenceTypeCodeDescriptionList.Descriptions._AWB),
		new CodeDescriptionPair(OwnerReferenceTypeCodeDescriptionList.Codes._ULD, OwnerReferenceTypeCodeDescriptionList.Descriptions._ULD),
		new CodeDescriptionPair(OwnerReferenceTypeCodeDescriptionList.Codes._ZZZ, OwnerReferenceTypeCodeDescriptionList.Descriptions._ZZZ)
	});

	public OrganisationsFindBoxCollection OrganizationsFindBoxList => new(Factory);

	public override CodeDescriptionPairList PackageTypeList =>
		Universal.RefCusCodeListTypes.GetCachedList(Factory,
			Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			ZDateTime.Today);

	public new CodeDescriptionPairList CustomsStatusList
	{
		get
		{
			return Factory.GetCachedValue($"NO.CusTempStorageRegHeaderLookups.CustomsStatusList", () =>
			{
				var list = new TemporaryStorageStatusCodeList();
				list.DefaultCode = Business.TemporaryStorageStatusCodeList.Codes.TST;
				return list;
			});
		}
	}
}
