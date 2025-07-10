using Enterprise.eManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eManifest.DataTransfer.Universal
{
	public class SupplierBookingLineConsigneeAddressWriter : DataObjectWriter<SupplierBookingLine, OrganizationAddress>
	{
		public SupplierBookingLineConsigneeAddressWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override OrganizationAddress PopulateDataObject(SupplierBookingLine supplierBookingLine)
		{
			if (supplierBookingLine == null || IsConsigneeAddressEmpty(supplierBookingLine))
			{
				return null;
			}

			return new OrganizationAddress(writeManager.WriterStrategy)
			{
				AddressType = nameof(DocAddressType.ConsigneeAddress),
				CompanyName = supplierBookingLine.DL_ConsigneeName,
				Address1 = supplierBookingLine.DL_ConsigneeAddress1,
				Address2 = supplierBookingLine.DL_ConsigneeAddress2,
				City = supplierBookingLine.DL_ConsigneeCity,
				State = supplierBookingLine.DL_ConsigneeState,
				Postcode = supplierBookingLine.DL_ConsigneePostCode,
				Country = Country.New(supplierBookingLine.ConsigneeCountryCode),
				Contact = supplierBookingLine.DL_ConsigneeContact,
				Email = supplierBookingLine.DL_ConsigneeEmail,
				Phone = supplierBookingLine.DL_ConsigneePhone,
				Mobile = supplierBookingLine.DL_ConsigneeMobile,
				Fax = supplierBookingLine.DL_ConsigneeFax,
				ScreeningStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(ScreeningStatusesList.Codes.Unknown, new ScreeningStatusesList()),
				GovRegNum = string.Empty,
				GovRegNumType = new RegistrationNumberType { Code = JobDocAddressLookups.GovRegNumTypeDefaultValue, Description = Res.GetString("75352a6c-c6bf-4fbd-86ba-cce3ca73dc7f", "Default") },
				AddressOverride = true
			};
		}

		bool IsConsigneeAddressEmpty(SupplierBookingLine supplierBookingLine)
		{
			return supplierBookingLine.DL_ConsigneeName.IsEmpty
				   && supplierBookingLine.DL_ConsigneeAddress1.IsEmpty
				   && supplierBookingLine.DL_ConsigneeAddress2.IsEmpty
				   && supplierBookingLine.DL_ConsigneeCity.IsEmpty
				   && supplierBookingLine.DL_ConsigneePostCode.IsEmpty
				   && supplierBookingLine.DL_ConsigneeState.IsEmpty
				   && supplierBookingLine.DL_RN_NKConsigneeCountryCode.IsEmpty
				   && supplierBookingLine.DL_ConsigneeContact.IsEmpty
				   && supplierBookingLine.DL_ConsigneePhone.IsEmpty
				   && supplierBookingLine.DL_ConsigneeMobile.IsEmpty
				   && supplierBookingLine.DL_ConsigneeEmail.IsEmpty
				   && supplierBookingLine.DL_ConsigneeFax.IsEmpty;
		}
	}
}
