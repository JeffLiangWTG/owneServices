using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Integration
{
	// If you add members to these types, create a new file for them.

	public interface IWhsBondedWarehouseLink { }
	public interface IWhsAreaCollection : IBusinessObjectCollection { }
	public interface IWhsDynamicAreaCollection : IWhsAreaCollection { }
	public interface IWhsLocationCollection : IBusinessObjectCollection { }
	public interface IWhsRowCollection : IBusinessObjectCollection { }
	public interface ILocationStatus { }
	public interface IAreaTypes { }
	public interface IWhsPickFace { }
	public interface IWhsOrderCollection : IBusinessObjectCollection { }
	public interface IWhsOrgSupplierPartCollection { }

	// for DocumentEngine whswarehousejobtypeandsubtype
	public interface IWhsWarehouseJobTypeWithSubTypeCodeDescriptionPairProvider { }

	// for DocumentEngine adjustmentreason
	public interface IWhsAdjustmentReasonCodeDescriptionPairProvider { }
	public interface IWarehouseCustomsAttributeAddInfo : Enterprise.Integration.Customs.ICusAddInfo { }
}
