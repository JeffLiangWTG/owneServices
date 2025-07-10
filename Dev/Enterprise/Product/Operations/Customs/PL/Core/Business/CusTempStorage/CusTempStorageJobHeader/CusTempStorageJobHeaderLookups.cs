using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.PL.Business.CusTempStorage;

public class CusTempStorageJobHeaderLookups : EU.Business.CusTempStorage.CusTempStorageJobHeaderLookups
{
	public CusTempStorageJobHeaderLookups(AutoCusTempStorageJobHeader parent) : base(parent)
	{
	}
	protected new CusTempStorageJobHeader Parent => base.Parent as CusTempStorageJobHeader;

	public IBusinessObjectCollection GuaranteeList => Factory.GetCachedValue("PLCusTempStorageJobHeaderLookups|GuaranteeList",
		() => new CusGuaranteeHeaderCollection(Factory));
}
