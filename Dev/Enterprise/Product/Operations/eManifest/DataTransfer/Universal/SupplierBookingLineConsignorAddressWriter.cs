using Enterprise.eManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eManifest.DataTransfer.Universal
{
	public class SupplierBookingLineConsignorAddressWriter : DataObjectWriter<SupplierBookingLine, OrganizationAddress>
	{
		public SupplierBookingLineConsignorAddressWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override OrganizationAddress PopulateDataObject(SupplierBookingLine supplierBookingLine)
		{
			if (supplierBookingLine == null || IsConsignorAddressEmpty(supplierBookingLine))
			{
				return null;
			}

			return new OrganizationAddress(writeManager.WriterStrategy)
			{
				AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress),
				CompanyName = supplierBookingLine.DL_ConsignorName,
				Address1 = supplierBookingLine.DL_ConsignorAddress1,
				Address2 = supplierBookingLine.DL_ConsignorAddress2,
				City = supplierBookingLine.DL_ConsignorCity,
				State = supplierBookingLine.DL_ConsignorState,
				Postcode = supplierBookingLine.DL_ConsignorPostCode,
				Country = Country.New(supplierBookingLine.ConsignorCountryCode),
				Contact = supplierBookingLine.DL_ConsignorContact,
				Email = supplierBookingLine.DL_ConsignorEmail,
				Phone = supplierBookingLine.DL_ConsignorPhone,
				Mobile = supplierBookingLine.DL_ConsignorMobile,
				Fax = supplierBookingLine.DL_ConsignorFax,
				ScreeningStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(ScreeningStatusesList.Codes.Unknown, new ScreeningStatusesList()),
				GovRegNum = string.Empty,
				GovRegNumType = new RegistrationNumberType { Code = JobDocAddressLookups.GovRegNumTypeDefaultValue, Description = Res.GetString("59f89810-5bda-48dd-abec-7b3cba4bd566", "Default") },
				AddressOverride = true
			};
		}

		bool IsConsignorAddressEmpty(SupplierBookingLine supplierBookingLine)
		{
			return supplierBookingLine.DL_ConsignorName.IsEmpty
				   && supplierBookingLine.DL_ConsignorAddress1.IsEmpty
				   && supplierBookingLine.DL_ConsignorAddress2.IsEmpty
				   && supplierBookingLine.DL_ConsignorCity.IsEmpty
				   && supplierBookingLine.DL_ConsignorPostCode.IsEmpty
				   && supplierBookingLine.DL_ConsignorState.IsEmpty
				   && supplierBookingLine.DL_RN_NKConsignorCountryCode.IsEmpty
				   && supplierBookingLine.DL_ConsignorContact.IsEmpty
				   && supplierBookingLine.DL_ConsignorPhone.IsEmpty
				   && supplierBookingLine.DL_ConsignorMobile.IsEmpty
				   && supplierBookingLine.DL_ConsignorEmail.IsEmpty
				   && supplierBookingLine.DL_ConsignorFax.IsEmpty;
		}
	}
}
