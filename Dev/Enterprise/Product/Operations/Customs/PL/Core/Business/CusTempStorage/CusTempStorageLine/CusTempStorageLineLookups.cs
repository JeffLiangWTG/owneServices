using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.CusTempStorage;

public class CusTempStorageLineLookups : EU.Business.CusTempStorage.CusTempStorageLineLookups
{
	public CusTempStorageLineLookups(CusTempStorageLine parent) : base(parent)
	{
	}

	protected new CusTempStorageLine Parent => base.Parent as CusTempStorageLine;

	public CodeDescriptionPairList EmptyList => new CodeDescriptionPairList();

	public CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);
}
