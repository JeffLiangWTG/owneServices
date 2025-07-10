using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public static class TWJobDocAddressHelper
	{
		public static bool IsSupplierDocumentaryAddress(this TWJobDocAddress address) => address.DocAddressType == DocAddressType.SupplierDocumentaryAddress;

		public static bool IsSupplierDocumentaryOrPickupDeliveryAddress(this TWJobDocAddress address) => IsSupplierDocumentaryAddress(address) || address.DocAddressType == DocAddressType.SupplierPickupDeliveryAddress;

		public static bool IsImporterDocumentaryAddress(this TWJobDocAddress address) => address.DocAddressType == DocAddressType.ImporterDocumentaryAddress;

		public static bool IsImporterDocumentaryOrImporterPickupDeliveryAddress(this TWJobDocAddress address) => IsImporterDocumentaryAddress(address) || address.DocAddressType == DocAddressType.ImporterPickupDeliveryAddress;

		public static bool IsSupplierOrImporterDocumentaryAddress(this TWJobDocAddress address) => IsSupplierDocumentaryAddress(address) || IsImporterDocumentaryAddress(address);

		public static bool IsDocumentaryOrPickupDeliveryAddress(this TWJobDocAddress address) => IsSupplierDocumentaryOrPickupDeliveryAddress(address) || IsImporterDocumentaryOrImporterPickupDeliveryAddress(address);

		public static bool IsLocalProcessorAddress(this TWJobDocAddress address) => address.DocAddressType == DocAddressType.LocalProcessorAddress;

		public static bool IsApplicantAddress(this TWJobDocAddress address) => address.DocAddressType == DocAddressType.Applicant;

		public static bool IsApplicantTranslatedDocumentaryAddress(this TWJobDocAddress address) => address.DocAddressType == DocAddressType.ApplicantTranslatedDocumentaryAddress;

		public static bool IsManufacturerAddress(this TWJobDocAddress address) => address.DocAddressType == DocAddressType.Manufacturer;

		public static bool IsControllingMessageDocAddress(this TWJobDocAddress address) => address.Parent is CusTWControllingMessageHeader;

		public static bool IsLocalAddress(this TWJobDocAddress address) =>
			address.DocAddressType == DocAddressType.SupplierTranslatedDocumentaryAddress ||
			address.DocAddressType == DocAddressType.ImporterTranslatedDocumentaryAddress ||
			address.DocAddressType == DocAddressType.BuyerTranslatedDocumentaryAddress ||
			address.DocAddressType == DocAddressType.LocalProcessorTranslatedDocAddress ||
			address.DocAddressType == DocAddressType.ManufacturerTranslatedDocumentaryAddress;

		public static ZBool ShouldDefaultWareHouseFromSupplierAddress(ZString declarationType) =>
			declarationType == Constants.DeclarationTypes.Import.D2 ||
			declarationType == Constants.DeclarationTypes.Export.D5 ||
			declarationType == Constants.DeclarationTypes.Import.D7;

		public static ZBool ShouldDefaultWareHouse2FromImporterAddress(ZString declarationType) =>
			declarationType == Constants.DeclarationTypes.Export.B2 ||
			declarationType == Constants.DeclarationTypes.Export.D1 ||
			declarationType == Constants.DeclarationTypes.Import.D7 ||
			declarationType == Constants.DeclarationTypes.Import.D8;
	}
}
