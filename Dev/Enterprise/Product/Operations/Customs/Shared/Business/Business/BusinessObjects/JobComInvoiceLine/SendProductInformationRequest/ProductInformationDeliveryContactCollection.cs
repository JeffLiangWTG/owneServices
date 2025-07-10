using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class ProductInformationDeliveryContactCollection : NonPersistentBusinessObjectCollection<ProductInformationDeliveryContact>
	{
		public ProductInformationDeliveryContactCollection(BusinessObjectFactory factory, CodeDescriptionPairList supplierList) : base(factory)
		{
			Argument.NotNull(supplierList, nameof(supplierList));
			SupplierList = supplierList;
		}

		CodeDescriptionPairList SupplierList { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ProductInformationDeliveryContact(Factory, SupplierList);
		}
	}
}
