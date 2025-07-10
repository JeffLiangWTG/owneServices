using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public sealed class CusTempStorageRegHeaderLookups(CusTempStorageRegHeader header) : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegHeaderLookups(header)
{
	public new CodeDescriptionPairList PreviousReferenceTypeList
	{
		get
		{
			return Factory.GetCachedValue("NO.CusTempStorageRegHeaderLookups.PreviousReferenceTypeList", () =>
			{
				var list = new PreviousReferenceTypes();
				list.DefaultCode = PreviousReferenceTypes.Codes.Manifest;
				return list;
			});
		}
	}

	public new CodeDescriptionPairList StatusList
	{
		get
		{
			return Factory.GetCachedValue($"NO.CusTempStorageRegHeaderLookups.StatusList", () =>
			{
				var list = new TemporaryStorageStatusCodeList();
				list.DefaultCode = Business.TemporaryStorageStatusCodeList.Codes.TST;
				return list;
			});
		}
	}
}
