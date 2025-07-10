using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class ProductInformationDeliveryContact : DocDeliveryContact
	{
		public ProductInformationDeliveryContact(BusinessObjectFactory factory, CodeDescriptionPairList supplierList) : base(factory)
		{
			Argument.NotNull(supplierList, nameof(supplierList));
			SupplierList = supplierList;
		}

		protected override CodeDescriptionPairList GetNotifyModes()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Core.Constants.ContactNotifyModes.Email, ResString.GetMultilingualString("Enterprise.Customs.GUI.DeliveryContact|EMail", "E-Mail"));
			return result;
		}

		#region SupplierInfo

		ZString supplier;

		[List(nameof(SupplierList))]
		[ResourceStringData("Enterprise.Customs.GUI.ProductInformationDeliveryContact|Supplier", Caption = "Supplier")]
		public ZString Supplier
		{
			get => supplier;
			set
			{
				SetNonPersistentPropertyValue(SupplierInfo, ref supplier, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSupplier();
				}
			}
		}

		public ZPropertyInfo SupplierInfo => GetZPropertyInfo(nameof(Supplier));

		public CodeDescriptionPairList SupplierList { get; }

		#endregion

		#region Validation

		protected override DocDeliveryContactValidation GetNewValidation() => new ProductInformationDeliveryContactValidation(this);

		public new ProductInformationDeliveryContactValidation Validation => (ProductInformationDeliveryContactValidation)GetNewValidation();

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
		}

		#endregion
	}
}
