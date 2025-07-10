using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.WarehouseExtensions
{
	public interface IChangeOfOwnershipLineDetails
	{
		OrgSupplierPart OwnerPart { get; }
		ZPropertyInfo NewOwnerProductCodeInfo { get; }
		ZPropertyInfo NewOwnerPartAttribute1Info { get; }
		ZPropertyInfo NewOwnerPartAttribute2Info { get; }
		ZPropertyInfo NewOwnerPartAttribute3Info { get; }
		ZPropertyInfo NewOwnerSerialNumberInfo { get; }
	}
}
